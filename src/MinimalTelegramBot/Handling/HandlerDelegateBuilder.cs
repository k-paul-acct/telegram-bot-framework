using System.Linq.Expressions;
using System.Reflection;
using MinimalTelegramBot.Localization.Abstractions;
using MinimalTelegramBot.Results;
using MinimalTelegramBot.StateMachine.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using IResult = MinimalTelegramBot.Results.IResult;

namespace MinimalTelegramBot.Handling;

internal static class HandlerDelegateBuilder
{
    public static Func<BotRequestContext, Task<IResult>> Build(Delegate d)
    {
        var returnType = d.Method.ReturnType;
        var parameters = d.Method.GetParameters();
        var delegateConstant = Expression.Constant(d);
        var contextParameter = Expression.Parameter(typeof(BotRequestContext), "context");
        var argumentsExp = parameters.Select<ParameterInfo, Expression>(x =>
        {
            if (x.ParameterType == typeof(BotRequestContext))
            {
                return contextParameter;
            }

            if (x.ParameterType == typeof(string) && x.Name is "message" or "text" or "messageText" or "textMessage")
            {
                return Expression.Property(contextParameter, nameof(BotRequestContext.MessageText));
            }

            if (x.ParameterType == typeof(string) && x.Name is "callback" or "callBack" or "callbackData" or "callBackData")
            {
                return Expression.Property(contextParameter, nameof(BotRequestContext.CallbackData));
            }

            var genericCallbackModel = typeof(ICallbackDataParser<>).MakeGenericType(x.ParameterType);

            if (x.ParameterType.IsAssignableTo(genericCallbackModel))
            {
                var callbackExp = Expression.Property(contextParameter, nameof(BotRequestContext.CallbackData));
                return Expression.Call(null, x.ParameterType.GetMethod(nameof(ICallbackDataParser<object>.Parse))!, callbackExp);
            }

            var genericCommandModel = typeof(ICommandParser<>).MakeGenericType(x.ParameterType);

            if (x.ParameterType.IsAssignableTo(genericCommandModel))
            {
                var useFormatProvider = x.GetCustomAttributes().Any(y => y is UseFormatProviderAttribute);
                var commandExp = Expression.Property(contextParameter, nameof(BotRequestContext.MessageText));
                var nullFormatProvider = Expression.Constant(null, typeof(IFormatProvider));
                Expression finalFormatProvider;

                if (useFormatProvider)
                {
                    var userLocale = Expression.Property(contextParameter, nameof(BotRequestContext.UserLocale));
                    var cultureInfo = Expression.Property(userLocale, nameof(BotRequestContext.UserLocale.CultureInfo));
                    var cast = Expression.Convert(cultureInfo, typeof(IFormatProvider));
                    var nullLocale = Expression.Constant(null, typeof(Locale));
                    finalFormatProvider = Expression.Condition(Expression.Equal(userLocale, nullLocale), nullFormatProvider, cast);
                }
                else
                {
                    finalFormatProvider = nullFormatProvider;
                }

                return Expression.Call(null, x.ParameterType.GetMethod(nameof(ICommandParser<object>.Parse))!, commandExp, finalFormatProvider);
            }

            if (typeof(ITelegramBotClient).IsAssignableFrom(x.ParameterType))
            {
                return Expression.Property(contextParameter, nameof(BotRequestContext.Client));
            }

            if (x.ParameterType == typeof(Update))
            {
                return Expression.Property(contextParameter, nameof(BotRequestContext.Update));
            }

            if (x.ParameterType == typeof(long) && x.Name is "chatId" or "chatID")
            {
                return Expression.Property(contextParameter, nameof(BotRequestContext.ChatId));
            }

            if (typeof(State).IsAssignableFrom(x.ParameterType))
            {
                return Expression.Property(contextParameter, nameof(BotRequestContext.UserState));
            }

            var servicesProperty = Expression.Property(contextParameter, nameof(BotRequestContext.Services));

            if (x.ParameterType == typeof(IKeyedServiceProvider))
            {
                return Expression.Convert(servicesProperty, typeof(IKeyedServiceProvider));
            }

            var getServiceMethod = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService))!;
            var getKeyedServiceMethod = typeof(IKeyedServiceProvider).GetMethod(nameof(IKeyedServiceProvider.GetKeyedService), [typeof(Type), typeof(object),])!;
            var keyedAttribute = x.GetCustomAttributes().FirstOrDefault(y => y is FromKeyedServicesAttribute);
            var key = ((FromKeyedServicesAttribute?)keyedAttribute)?.Key;

            if (key is null)
            {
                return Expression.Convert(Expression.Call(servicesProperty, getServiceMethod, Expression.Constant(x.ParameterType)), x.ParameterType);
            }

            var keyExpression = Expression.Constant(key, typeof(object));
            var castServices = Expression.Convert(servicesProperty, typeof(IKeyedServiceProvider));

            return Expression.Convert(Expression.Call(castServices, getKeyedServiceMethod, Expression.Constant(x.ParameterType), keyExpression), x.ParameterType);
        });

        var delegateInvokeExp = Expression.Invoke(delegateConstant, argumentsExp);
        var delegateLambdaExp = Expression.Lambda(delegateInvokeExp, contextParameter);

        Func<BotRequestContext, Task<IResult>> compiled;

        if (returnType == typeof(void))
        {
            var wrapper = typeof(RequestDelegateHelper).GetMethod(nameof(RequestDelegateHelper.VoidDelegateWrapper))!;
            var wrappedCallExp = Expression.Call(wrapper, delegateLambdaExp);
            var wrappedInvokeExp = Expression.Invoke(wrappedCallExp, contextParameter);
            var finalLambdaExp = Expression.Lambda<Func<BotRequestContext, Task<IResult>>>(wrappedInvokeExp, contextParameter);
            var final = finalLambdaExp.Compile();
            compiled = final;
        }
        else if (typeof(IResult).IsAssignableFrom(returnType))
        {
            var wrapper = typeof(RequestDelegateHelper).GetMethod(nameof(RequestDelegateHelper.ResultDelegateWrapper))!;
            var wrappedCallExp = Expression.Call(wrapper, delegateLambdaExp);
            var wrappedInvokeExp = Expression.Invoke(wrappedCallExp, contextParameter);
            var finalLambdaExp = Expression.Lambda<Func<BotRequestContext, Task<IResult>>>(wrappedInvokeExp, contextParameter);
            var final = finalLambdaExp.Compile();
            compiled = final;
        }
        else if (AwaitableInfo.IsTypeAwaitable(returnType, out var taskType, out var genericType))
        {
            if (taskType == typeof(Task))
            {
                if (genericType is null)
                {
                    var wrapper = typeof(RequestDelegateHelper).GetMethod(nameof(RequestDelegateHelper.TaskDelegateWrapper))!;
                    var wrappedCallExp = Expression.Call(wrapper, delegateLambdaExp);
                    var wrappedInvokeExp = Expression.Invoke(wrappedCallExp, contextParameter);
                    var finalLambdaExp = Expression.Lambda<Func<BotRequestContext, Task<IResult>>>(wrappedInvokeExp, contextParameter);
                    var final = finalLambdaExp.Compile();
                    compiled = final;
                }
                else if (typeof(IResult).IsAssignableFrom(genericType))
                {
                    var final = delegateLambdaExp.Compile();
                    compiled = (Func<BotRequestContext, Task<IResult>>)final;
                }
                else
                {
                    var wrapper = typeof(RequestDelegateHelper).GetMethod(nameof(RequestDelegateHelper.GenericTaskDelegateWrapper))!.MakeGenericMethod(genericType);
                    var resultHandlerMethod = typeof(ResultHelper).GetMethod(nameof(ResultHelper.FromType))!.MakeGenericMethod(genericType);
                    var resultHandler = resultHandlerMethod.Invoke(null, null);
                    var resultHandlerExp = Expression.Constant(resultHandler);
                    var wrappedCallExp = Expression.Call(wrapper, delegateLambdaExp, resultHandlerExp);
                    var wrappedInvokeExp = Expression.Invoke(wrappedCallExp, contextParameter);
                    var finalLambdaExp = Expression.Lambda<Func<BotRequestContext, Task<IResult>>>(wrappedInvokeExp, contextParameter);
                    var final = finalLambdaExp.Compile();
                    compiled = final;
                }
            }
            else
            {
                if (genericType is null)
                {
                    var wrapper = typeof(RequestDelegateHelper).GetMethod(nameof(RequestDelegateHelper.ValueTaskDelegateWrapper))!;
                    var wrappedCallExp = Expression.Call(wrapper, delegateLambdaExp);
                    var wrappedInvokeExp = Expression.Invoke(wrappedCallExp, contextParameter);
                    var finalLambdaExp = Expression.Lambda<Func<BotRequestContext, Task<IResult>>>(wrappedInvokeExp, contextParameter);
                    var final = finalLambdaExp.Compile();
                    compiled = final;
                }
                else if (typeof(IResult).IsAssignableFrom(genericType))
                {
                    var wrapper = typeof(RequestDelegateHelper).GetMethod(nameof(RequestDelegateHelper.ResultValueTaskDelegateWrapper))!;
                    var wrappedCallExp = Expression.Call(wrapper, delegateLambdaExp);
                    var wrappedInvokeExp = Expression.Invoke(wrappedCallExp, contextParameter);
                    var finalLambdaExp = Expression.Lambda<Func<BotRequestContext, Task<IResult>>>(wrappedInvokeExp, contextParameter);
                    var final = finalLambdaExp.Compile();
                    compiled = final;
                }
                else
                {
                    var wrapper = typeof(RequestDelegateHelper).GetMethod(nameof(RequestDelegateHelper.GenericValueTaskDelegateWrapper))!.MakeGenericMethod(genericType);
                    var resultHandlerMethod = typeof(ResultHelper).GetMethod(nameof(ResultHelper.FromType))!.MakeGenericMethod(genericType);
                    var resultHandler = resultHandlerMethod.Invoke(null, null);
                    var resultHandlerExp = Expression.Constant(resultHandler);
                    var wrappedCallExp = Expression.Call(wrapper, delegateLambdaExp, resultHandlerExp);
                    var wrappedInvokeExp = Expression.Invoke(wrappedCallExp, contextParameter);
                    var finalLambdaExp = Expression.Lambda<Func<BotRequestContext, Task<IResult>>>(wrappedInvokeExp, contextParameter);
                    var final = finalLambdaExp.Compile();
                    compiled = final;
                }
            }
        }
        else
        {
            var wrapper = typeof(RequestDelegateHelper).GetMethod(nameof(RequestDelegateHelper.GenericDelegateWrapper))!.MakeGenericMethod(returnType);
            var resultHandlerMethod = typeof(ResultHelper).GetMethod(nameof(ResultHelper.FromType))!.MakeGenericMethod(returnType);
            var resultHandler = resultHandlerMethod.Invoke(null, null);
            var resultHandlerExp = Expression.Constant(resultHandler);
            var wrappedCallExp = Expression.Call(wrapper, delegateLambdaExp, resultHandlerExp);
            var wrappedInvokeExp = Expression.Invoke(wrappedCallExp, contextParameter);
            var finalLambdaExp = Expression.Lambda<Func<BotRequestContext, Task<IResult>>>(wrappedInvokeExp, contextParameter);
            var final = finalLambdaExp.Compile();
            compiled = final;
        }

        return compiled;
    }
}