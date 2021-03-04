using System;
using System.Diagnostics;

using Xamarin.Forms;
using Xamarin.Essentials;

using ZXing.Net.Mobile.Forms;

using Plugin.Toast;
using Geolocation;
using System.Threading;

namespace App1
{
    public partial class MainPage : ContentPage
    {
        // Fields
        private ZXingScannerPage Scanner_Page { get; set; } = new ZXingScannerPage();
        public string DbPath { get; } = @"/storage/emulated/0/Android/Data/PictureApp.jiahong/files/employee.db";

        // Latitude = Y, Longitude = X.
        public double lat_one = 4.5559201; public double lon_one = 101.1155948; // Desk
        public double lat_two = 4.5558876; public double lon_two = 101.1155414; // wall to the left
        public MainPage()
        {
            InitializeComponent();
        }

        // Features

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
                CrossToastPopUp.Current.ShowToastMessage(ex.Message);
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
            status += $"\nDistance from centre: {distance_from_midpoint}, radius: {radius}\n\n NOTE: All values concerning distances are in meters";
            if (distance_from_midpoint <= radius)
            {
                await DisplayAlert("Within the compound", status, "Ok");
            }
            else
            {
                await DisplayAlert("Not within the compound", status, "Ok");
            }

            status = "";

        }

        private async void Start_Scanner(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(Scanner_Page);
            Scanner_Page.OnScanResult += (result) =>
            {
                Scanner_Page.IsScanning = false;
                Scanner_Page.HeightRequest = 250;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopModalAsync();
                    await DisplayAlert("Scanned barcode", result.Text, "OK");
                });
            };
        }

        private async void Establish_Connection(object sender, EventArgs e)
        {
            var connection = await Database.Connect(DbPath);
            if (connection == null)
            {
                Debug.WriteLine("Connection not made.");
                return;
            }

            connection.Close();
            Debug.WriteLine("Returned!");
        }

        private async void Select_Picture(object sender, EventArgs e)
        {
            var iPv4 = "192.168.1.143:5000";
            var uri = $"http://{iPv4}/";
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
