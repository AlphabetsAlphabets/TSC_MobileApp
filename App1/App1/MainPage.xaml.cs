using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

// Xamarin
using Xamarin.Forms;
using Xamarin.Essentials;

// NuGet packages
using CsvHelper;
using Geolocation;
using Plugin.Toast;
using Newtonsoft.Json;
using ZXing.Net.Mobile.Forms;
using CsvHelper.Configuration;

namespace App1
{
    // IOS, Android, cross platform.
    // Put debtor code into QR
    // Log current location when you take an image
    // Increase the radius by 10 - 20% from phone's point

    public partial class MainPage : ContentPage
    {
        // Fields
        private ZXingScannerPage Scanner_Page { get; set; } = new ZXingScannerPage();
        public string DbPath = @"/storage/emulated/0/Android/Data/PictureApp.jiahong/files/employee.db";
        public string Locations_Path = @"/storage/emulated/0/Android/Data/PictureApp.jiahong/files/locations.csv";
        public string Base_Path = @"/storage/emulated/0/Android/Data/PictureApp.jiahong/files/";
        public static string iPv4 = "192.168.1.147:5000";
        public static string uri = $"http://{iPv4}/";

        // Coord of compound
        public double lat_one = 4.556026; public double lon_one = 101.115623; // Bottom left
        public double lat_two = 4.555880; public double lon_two = 101.115608; // Top right
        public MainPage()
        {
            InitializeComponent();
        }

        // Features
        private async void Set_Bounds(object sender, EventArgs e)
        {
            // Parse JSON
            Debug.WriteLine("Parsing JSON.");
            var result = JsonConvert.DeserializeObject<Locale>("hi");
            /*
                TODO:
                Make a new class (or refactor it) to facilitate the change in format of the JSON response. 
            */ 
            Debug.WriteLine("JSON data has been parsed successfully.");
            //double lat_one = record.Lat_one; double lon_one = record.Lon_one;
            //double lat_two = record.Lat_two; double lon_two = record.Lon_two;

        }
        

        private async void In_Area(object sender, EventArgs e)
        {
            Location current_location = null;
            try
            {
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var cts = new CancellationTokenSource();
                current_location = await Xamarin.Essentials.Geolocation.GetLocationAsync(request, cts.Token);
            }catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError(ex.Message);
                return;
            }

            Coordinate location = new Coordinate(current_location.Latitude, current_location.Longitude); // User's current location

            string status = $"Current lat: {current_location.Latitude}, lon: {current_location.Longitude}";

            Coordinate origin = new Coordinate(lat_one, lon_one);
            Coordinate end = new Coordinate(lat_two, lon_two);

            double diameter = Math.Abs(GeoCalculator.GetDistance(origin, end, 4, DistanceUnit.Meters));
            double radius = diameter / 2;

            double mp_lat = (lat_one + lat_two) / 2;
            double mp_lon = (lon_one + lon_two) / 2;

            Coordinate midpoint = new Coordinate(mp_lat, mp_lon);

            double distance_from_midpoint = Math.Abs(GeoCalculator.GetDistance(location, midpoint, 4, DistanceUnit.Meters));
            status += $"\nDistance from centre: {distance_from_midpoint}, radius: {radius}\n\nNOTE: All values concerning distances are in meters";
            if (distance_from_midpoint <= radius) await DisplayAlert("Within the compound", status, "Ok");
            else await DisplayAlert("Not within the compound", status, "Ok");

            status = "";

        }

        private async void Start_Scanner(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(Scanner_Page);
            Scanner_Page.OnScanResult += (result) =>
            {
                Scanner_Page.IsScanning = false;
                Scanner_Page.HeightRequest = 200;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopModalAsync();
                    await DisplayAlert("Result", result.Text, "OK");
                });
            };
        }

        private async void Establish_Connection(object sender, EventArgs e)
        {
            var connection = await Database.Connect(DbPath);
            if (connection == null)
            {
                CrossToastPopUp.Current.ShowToastError("Connection not established.");
                return;
            }

            connection.Close();
            CrossToastPopUp.Current.ShowToastMessage("Connected.");
        }

        private async void Select_Picture(object sender, EventArgs e)
        {
            Debug.WriteLine($"URI: {uri}");

            var images = await FilePicker.PickMultipleAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Pick image(s)"
            });

            if (images == null) { Debug.WriteLine("User cancelled operation"); return; }

            string imageName = "";
            int count = 0;

            CrossToastPopUp.Current.ShowToastMessage("Uploading image(s)");
            foreach (var image in images)
            {
                try
                {
                    Debug.WriteLine($"Image name: {image.FileName}");
                    imageName = image.FileName;
                    string imagePath = image.FullPath;

                    Debug.WriteLine($"Image path: {imagePath}");

                    Console.WriteLine($"URI: {uri}");
                    var result = await Request.Upload(uri, imagePath, imageName);

                    if (result.Code > 299) throw new HttpErrorException(result.Code, result.Message);
                    else if (result.Code == 200)
                    {
                        CrossToastPopUp.Current.ShowCustomToast($"Uploaded {imageName}", "grey", "green");
                        Debug.WriteLine($"Request successful, image has been uploaded to: {result.Path}");
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    count = 1;
                    break;
                }
            }
            if (count == 1)
            {
                CrossToastPopUp.Current.ShowCustomToast($"Image {imageName} not uploaded.", "grey", "red");
                CrossToastPopUp.Current.ShowCustomToast($"URI: {uri}", "grey", "red");
            }
        }

        // Error related functions
    }
}
