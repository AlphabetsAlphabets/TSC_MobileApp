# Table of contents
Topic | description |
--- | --- |
|[How to write documentation](https://github.com/YJH16120/TSC_MobileApp#Writing-documentation) | Explains in detail how to write documentation |
|[Writing conventions](https://github.com/YJH16120/TSC_MobileApp#Writing-conventions) | Writing code that complies to the C# language standard |

---
### Writing documentation
Assume that you had a function that divides two numbers, where you divide x by y
```csharp
/// <summary>Divides two numbers<summary>
/// <param name="x">The numerator</param>
/// <param name="y">The denominator</param>
/// <return>double</return>
/// <exception cref="System.DivideByZeroException">Occurs when y is 0</exception>
public void double divide(x: int, y: int) {
	if (y == 0) throw System.DivideByZeroException;
	return x / y;
}
```
When writing documentation for a function you must do the following (in order):
1. Wrting a quick summary.
2. List and explain the parameters.
3. specify the return type.
4. If there are exceptions, include them in exception tags.

Writing documentation for a class is as such, the example will be a snippet from [Printing.cs](https://github.com/YJH16120/TSC_MobileApp/blob/master/MobileApp/Printing/Printing.cs)
```csharp
/// <summary>
/// This class is host to functions that are related to print job operations.
/// </summary>
public static class Printing
{
	...
}
```

### Writing conventions
This section details the language conventions of C# in terms of how to write C# code.  

All functions, classes, exceptions, etc. Are **all** written in PascalCasing
```csharp
class ThisIsAnExceptionClass: Exception {
	public ThisIsAnExceptionClass() {
		String.Format("Exception occured.");
	}
}
```
while variables are written in camelCasing.
```cs
string firstName = "John";
string lastName = "Doe";

public void WhatIsYourName(firstName: String, lastName: String) {
	fullName = $"{firstName} {lastName}";
	Console.WriteLine(fullName);
}
```

For more information please read from the offical microsoft [docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)

