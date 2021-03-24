// std
using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

// Xamarin
using Xamarin.Forms;
using Xamarin.Essentials;

// NuGet packages
using Plugin.Media;
using Plugin.Toast;
using ZXing.Net.Mobile.Forms;

namespace MobileApp
{
    // IOS, Android, cross platform.
    // Put debtor code into QR
    // Log current location when you take an image
    // Increase the radius by 10 - 20% from phone's point

    public partial class MainPage : ContentPage
    {
        // Fields
        private ZXingScannerPage Scanner_Page { get; set; } = new ZXingScannerPage()
        {
            MinimumHeightRequest = 200,
            HeightRequest = 200
        }; // Needed for the QR Code scanner

        // If you want to save any files make sure you do it within the root of the mobile app
        public static string root = @"/storage/emulated/0/Android/Data/com.MobileApp/files/";

        public string DbPath = @"/storage/emulated/0/Android/Data/com.MobileApp/files/employee.db"; // The user's information

        // Temporary fields, will be removed once a static IP is set.
        public static string iPv4 = "192.168.1.137"; // Dynamic IP
        public static string uri = $"http://{iPv4}:5000/"; // Fully constructed IP

        public MainPage()
        {
            InitializeComponent();
        }


        private async void In_Area(object sender, EventArgs e) // Working w/Error handling & comments
        {
            /*
            For this to work you need to turn on the api. The endpoint is location.py in Web API/env-api/endpoints/location.py 
            Make sure that the table tlocations exists in the schema tsc_office. The information contained within the table looks
            like this (https://imgur.com/a/QF9HMVt) The location's name, two sets of latitudes (lat_one, lat_two) and two sets of longitudes (lon_one, lon_two)
             */

            Location user_location = null;
            List<String> names = null;
            List<double> lat_one = null;
            List<double> lon_one = null;
            List<double> lat_two = null;
            List<double> lon_two = null;

            try
            {
                // Attemps to get the user's current location, if it fails it will throw an exception then cancel the geolocation request.
                GeolocationRequest geolocationRequest = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var cts = new CancellationTokenSource();
                user_location = await Geolocation.GetLocationAsync(geolocationRequest, cts.Token);

                // Attemps to make a HTTP GET request. Will throw an exception if it fails.
                var apiResponse = await Request.Get_Location(uri);
                names = apiResponse.Name;
                lat_one = apiResponse.Lat_One; lon_one = apiResponse.Lon_One;
                lat_two = apiResponse.Lat_Two; lon_two = apiResponse.Lon_Two;
            }
            // Attempts to handle all the errors. 
            catch (TimeoutException)
            {
                Debug.WriteLine("Unable to make request to api.");
                CrossToastPopUp.Current.ShowToastError("Unable to make request to api.");
                return;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                Debug.WriteLine("Location services not enabled.");
                CrossToastPopUp.Current.ShowToastError($"Location services is turned off.\nDetail: {fneEx.Message}");
                return;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Debug.WriteLine("Feature not supported.");
                CrossToastPopUp.Current.ShowToastError($"Feature is not supported on this device.\nDetailed: {fnsEx.Message}");
                return;
            }
            catch (PermissionException pEx)
            {
                string error_message = "You have not given this app permission to access this device's location.";
                CrossToastPopUp.Current.ShowToastError(error_message + $"\nDetailed: {pEx.Message}");
                return;

            }
            catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError(ex.Message);
                return;
            }

            var user_coordinate = new Location(user_location.Latitude, user_location.Longitude); // User's current location

            for (int i = 0; i < names.Count; i++)
            {
                var origin = new Location(lat_one[i], lon_one[i]);
                var end = new Location(lat_two[i], lon_two[i]);

                // Gets the diameter, then the radius from it.
                double diameter = Math.Abs(Location.CalculateDistance(origin, end, DistanceUnits.Kilometers));
                double radius = diameter / 2;

                // The midpoint is the circle's centre
                double mp_lat = (lat_one[i] + lat_two[i]) / 2;
                double mp_lon = (lon_one[i] + lon_two[i]) / 2;

                var midpoint_of_shop = new Location(mp_lat, mp_lon);
                var relative_distance_from_centre = Location.CalculateDistance(user_coordinate, midpoint_of_shop, DistanceUnits.Kilometers);
                if (relative_distance_from_centre <= radius)
                {
                    await DisplayAlert("Within compound", $"You are {relative_distance_from_centre * 1000} meters away from {names[i]}\nTolerance: {radius * 1000} meters", "OK");
                    return;
                }
            }
            await DisplayAlert("Not within compound.", "You are not in the area of any shops/dealers", "OK");
        }
        private async void Scan_QR_Code(object sender, EventArgs e) // Working w/Error handling & comments
        {
            // Uses the scanner_page field defined at the start of the file.
            try
            {
                await Navigation.PushModalAsync(Scanner_Page);
                Scanner_Page.OnScanResult += (result) =>
                {
                    Scanner_Page.IsScanning = false;

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopModalAsync();
                        await DisplayAlert("Scanned barcode", result.Text, "OK");
                    });
                };
            }
            catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError($"This app cannot use the camera.\nDetailed: {ex.Message}\nError: {ex.ToString()}");
            }
        }
        private async void Establish_Connection(object sender, EventArgs e) // Working w/Error handling & comments
        {
            // Connects to the local sqlite database
            try
            {
                var connection = await Database.Connect(DbPath);
                if (connection == null)
                {
                    Debug.WriteLine("Connection not made.");
                    CrossToastPopUp.Current.ShowToastMessage("Connection not made.");
                    connection.Close();
                    return;
                }
                CrossToastPopUp.Current.ShowToastMessage("Connection made.");

                connection.Close();
                Debug.WriteLine("Returned!");
            }
            catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError(ex.Message + $"Error: {ex}");
            }
        }
        private async void Upload_Image(object sender, EventArgs e) // Working w/error handling & comments
        {
            // This requires the use of the api as well. Endpoint upload.
            Debug.WriteLine($"URI: {uri}");

            try
            {
                var images = await FilePicker.PickMultipleAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Pick image(s)"
                });

                if (images == null) { Debug.WriteLine("User cancelled operation"); return; }

                string imageName = "";

                foreach (var image in images)
                {
                    Debug.WriteLine($"Image name: {image.FileName}");
                    imageName = image.FileName;
                    string imagePath = image.FullPath;

                    Debug.WriteLine($"Image path: {imagePath}");

                    Console.WriteLine($"URI: {uri}");
                    var result = await Request.Upload(uri, imagePath, imageName);
                    if (result == null) throw new HttpErrorException(200, "unable to make request");

                }
                CrossToastPopUp.Current.ShowToastMessage("Image(s) uploaded successfully.");
            }
            catch (HttpErrorException heEx)
            {
                CrossToastPopUp.Current.ShowToastError($"Unable to make request\nDetailed: {heEx.Message}");
            }
            catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError($"{ex.Message}\nError: {ex}");
            }
        }
        private async void Take_Picture(object sender, EventArgs e)
        {
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                SaveToAlbum = true
            });
        }
        /*
        TODO:
            1. Let user choose to print images or text
         */
        private async void Print_Text(object sender, EventArgs e)
        {
            var socket = await Printing.ConnectToPrinterAsync();
            if (socket == null) return;

            var text_files = await Printing.SelectTextFilesAsync();
            if (text_files == null) return;

            await Printing.PrintTextFilesAsync(socket, text_files);
        }

        private async void Print_Images(object sender, EventArgs e)
        {
            var socket = await Printing.ConnectToPrinterAsync();
            if (socket == null) return;

            var image_files = await Printing.SelectImageFilesAsync();
            if (image_files == null) return;

            await Printing.PrintImageFilesAsync(socket, image_files);
        }
        private async void Test(object sender, EventArgs e)
        {
            var socket = await Printing.ConnectToPrinterAsync();
            if (socket == null) return;

            var image_files = await Printing.SelectImageFilesAsync();
            if (image_files == null) return;

            foreach (var image in image_files)
            {
                var path = image.FullPath;
                await Printing.PrintImageFilesAsync(path, socket);
            }

            var status = "Print job completed.";
            Debug.WriteLine(status);
            CrossToastPopUp.Current.ShowToastMessage(status);


        }
    }
}
