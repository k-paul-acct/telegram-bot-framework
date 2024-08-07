---
name: Bug report
about: Create a report about something that isn't working
labels: bug
---

## File a bug

Remember:

* Please check that the documentation does not explain the behavior you are seeing.
* Please search in both open and closed issues to check that your bug has not already been filed.

### Include your code

To fix any bug we must first reproduce it. To make this possible, please attach a small, runnable project
or post a small, runnable code listing that reproduces what you are seeing.

It is often impossible for us to reproduce a bug when working with only code snippets since we have to
guess at the missing code. 

Use triple-tick code fences for any posted code. For example:

```C#
Console.WriteLine("Hello, World!");
```

### Include stack traces

Include the full exception message and stack trace for any exception you encounter.

Use triple-tick fences for stack traces. For example:

```
Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
   at SixFour.Sub() in C:\Stuff\AllTogetherNow\SixFour\SixFour.cs:line 49
   at SixFour.Main() in C:\Stuff\AllTogetherNow\SixFour\SixFour.cs:line 54
```

### Include version information

Minimal Telegram Bot Framework version:

Target framework: (e.g. .NET 6.0)

Operating system:

IDE: (e.g. Visual Studio 2022 17.4)
