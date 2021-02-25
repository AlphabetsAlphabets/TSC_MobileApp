# UploadSharp
A mobile app that runs on Xamarin.Android and C#! It can upload either an image or images. To your computer remotely, assuming you have an API that is already pre-made.

# How it works
When you click select image on the app and finish selecting the images, it will send a `PUT` request and send the images over to the api. To be
resized, with the added side effect of reducing the file size. The only endpoint available in the api is upload. The repository for web api can be found [here](https://github.com/YJH16120/Web-API)

### No it's not a new nuget package
