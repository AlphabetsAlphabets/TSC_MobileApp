﻿# Table of contents
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
2. List and explain the parameters (if there are any).
3. specify the return type (if there is one).
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

All functions, classes, exceptions, etc. Are **all** written in PascalCase
```csharp
class ThisIsAnExceptionClass: Exception {
	public ThisIsAnExceptionClass() {
		String.Format("Exception occured.");
	}
}
```
while variables are written in camelCase.
```cs
string firstName = "John";
string lastName = "Doe";

public void WhatIsYourName(firstName: String, lastName: String) {
	fullName = $"{firstName} {lastName}";
	Console.WriteLine(fullName);
}
```

And be sure to write code that is clear. And leave nothing ambiguous, comments help. This example is a snippet 
from [Locate.cs](https://github.com/YJH16120/TSC_MobileApp/blob/master/MobileApp/Locate/Locate.cs).
```csharp
// Two points in the client's shop.
var origin = new Location(lat_one[i], lon_one[i]);
var end = new Location(lat_two[i], lon_two[i]);

// Gets the diameter, then the radius from it.
double diameter = Math.Abs(Location.CalculateDistance(origin, end, DistanceUnits.Kilometers));
double radius = diameter / 2;

// The midpoint is the circle's centre
double mp_lat = (lat_one[i] + lat_two[i]) / 2;
double mp_lon = (lon_one[i] + lon_two[i]) / 2;

var midpoint_of_shop = new Location(mp_lat, mp_lon);
var user_relative_distance_from_circle_centre = Math.Abs(Location.CalculateDistance(user_coordinate, midpoint_of_shop, DistanceUnits.Kilometers));
```
You can understand this when you read it for the first time, assuming you're familiar with C#. The `origin` is an instance of `Location`, and it's constructor is a value from the lists `lat_one`, and `lat_two`.
This applies to `end` as well. You can tell that `diameter` is obtained from the `CalculateDistance` method.

And look at the comment, it tells you that I'm going get the midpoint. And the midpoint coordinate would be the value of `mp_lat`, and `mp_lon`. 

Pay attention to how variables are named. It's very obvious that `user_relative_distance_from_circle_centre` is obviously
the user's relative distance from the circle's centre. 

There is absolutely no ambiguity about what each function is. Even if a variable isn't too clear, comments help clear things out.

For more information please read from the offical microsoft [docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)

