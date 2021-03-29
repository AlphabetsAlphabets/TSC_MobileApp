# Table of contents
Topic | description |
--- | --- |
|[To-do list](https://github.com/YJH16120/TSC_MobileApp#To-do-list) | List of things that need to be implemented. |
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
1. Write a quick summary.
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
This section details the language conventions of C#  

All functions, classes, exceptions, etc. Are **all** written in PascalCase
```csharp
class ThisIsAnExceptionClass: Exception {
	public ThisIsAnExceptionClass() {
		String.Format("Exception occured.");
	}
}
```
while variables are written in either camelCase, or snake_case. 
```cs
string firstName = "John"; // camelCase
string last_name = "Doe"; // snake_case

public void WhatIsYourName(firstName: String, lastName: String) {
	fullName = $"{firstName} {lastName}";
	Console.WriteLine(fullName);
}
```
You can mix both camelCase and snake_case named variables together, but it's not advised, unless it makes the code more readable.

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

Pay attention to how variables are named. It's very obvious that `user_relative_distance_from_circle_centre` is obviously the user's relative distance from the circle's centre. 

There is absolutely no ambiguity about what each function is. Even if a variable isn't too clear, comments help clear things out.

For more information please read from the offical microsoft [docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)

---
# Creating multiple source files.
At some point while developing a feature you may find yourself making either a very long [function](https://paste.rs/aRc.cs) (the code is from a very early `InArea` function). Or, you may find yourself creating multiple functions to split the tasks to make to make it more readable. You can see that it is very long. Furthermore, this function was going to be implemented with the `ScanQRCode` method, so the function body of `InArea` would be **added** to the body of the `ScanQRCode` method.

Which is why I've split up the `InArea` function to [three]https://github.com/YJH16120/TSC_MobileApp/blob/master/MobileApp/Locate/Locate.cs) different functions: Obtaining the user's current location, making a HTTP request to the API to get our clients' location information. And finally the one where I use the information from the API response, and the user's location to calculate whether or not a user is in one of our client's shop.

And those are the conditions to create a new source file. To recap:
1. When a function's body gets too long, 
2. or when you find yourself creating multiple 'sub-functions'.

---
# To-do list
These can all be done out of order. [Formatting guide](https://github.com/caiyongji/emoji-list#symbols).

- :white_check_mark: Completed/Done
- :x: Not completed/not done.

Print operations | status |
---------------- | ------ |
 Merge the print images, and print text. Into one function to be able to print a receipt. | :white_check_mark: |
 Find a way to get users to enter information for the invoice. | :white_check_mark: |
 Print images and text in the same function. | :white_check_mark: |
 Get the outlet's name through the location api, via the functions for geolocation. | :white_check_mark: |

Configure first time startups | status |
----------------------------- | ------ |
Automaitcally download TSC logo with the use of the api. | :x:
Create an offline database. | :x:
Create a dummy file for printing if the file doesn't already exist. | :white_check_mark:
