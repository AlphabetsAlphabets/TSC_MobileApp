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
using Android.Bluetooth;
using Microsoft.Data.Sqlite;
using Plugin.Media.Abstractions;

namespace MobileApp
{
    // IOS, Android, cross platform.
    // Put debtor code into QR
    // Log current location when you take an image
    // Increase the radius by 10 - 20% from phone's point

    /// <summary>
    /// This file hosts the call backs for MainPage.xaml
    /// </summary>
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

        public static BluetoothSocket socket = null;

        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This function will determine whether or not a user is in a client's shop. 
        /// The way this is calculated is in the <see cref="Locate"/> class.
        /// </summary>
        /// <param name="sender">The button itself</param>
        /// <param name="e">What happens when you click a button</param>
        private async void InArea(object sender, EventArgs e) // Working w/Error handling & comments
        {
            string status = await Locate.IsUserNearClientAsync(uri);
            await DisplayAlert("Status", status, "OK");
        }
        /// <summary>
        /// Scans the QR code, and checks the user's current location to see if the user is in a client's shop.
        /// </summary>
        /// <param name="sender">The button itself</param>
        /// <param name="e">What happens when you click a button</param>
        private async void ScanQRCode(object sender, EventArgs e) // Working w/Error handling & comments
        {
            // Uses the scanner_page field defined at the start of the file.
            string qrValue = "";
            try
            {
                await Navigation.PushModalAsync(Scanner_Page);
                Scanner_Page.OnScanResult += (result) =>
                {
                    Scanner_Page.IsScanning = false;

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopModalAsync();
                        qrValue = result.Text;

                    // Get user's location 
                    string status = await Locate.IsUserNearClientAsync(uri);
                    await DisplayAlert("Status", status, "OK");
                    });
                };
                // Check the user's current location
            } // Attempts to handle all the errors. 
            catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowToastError(ex.Message);
            }
        }
        /// <summary>
        /// Connects to the SQLITE3 database
        /// </summary>
        /// <param name="sender">The button itself</param>
        /// <param name="e">What happens when you click a button</param>
        private async void EstablishConnection(object sender, EventArgs e) // Working w/Error handling & comments
        {
            // Connects to the local sqlite database
            try
            {
                SqliteConnection connection = await Database.Connect(DbPath);
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
        /// <summary>
        /// Uploads an image to the server through the API.
        /// </summary>
        /// <param name="sender">The button itself</param>
        /// <param name="e">What happens when you click a button</param>
        private async void UploadImage(object sender, EventArgs e) // Working w/error handling & comments
        {
            // This requires the use of the api as well. Endpoint upload.
            Debug.WriteLine($"URI: {uri}");

            try
            {
                IEnumerable<FileResult> images = await FilePicker.PickMultipleAsync(new PickOptions
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
        /// <summary>
        /// Takes an image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TakePicture(object sender, EventArgs e)
        {
            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                SaveToAlbum = true
            });
        }
        /*
        TODO:
            1. Let user choose to print images or text
         */
        /// <summary>
        /// Prints text from a text file
        /// </summary>
        /// <param name="sender">The button itself</param>
        /// <param name="e">What happens when you click a button</param>
        private async void PrintText(object sender, EventArgs e)
        {
            BluetoothSocket socket = await Printing.ConnectToPrinterAsync();
            if (socket == null) return;

            IEnumerable<FileResult> text_files = await Printing.SelectTextFilesAsync();
            if (text_files == null) return;

            await Printing.PrintTextFilesAsync(socket, text_files);
            socket.Close();
            socket.Dispose();
        }

        private async void PrintString(object sender, EventArgs e)
        {
            var socket = await Printing.ConnectToPrinterAsync();
            if (socket == null) return;
            string textToPrint = "Hello.";
            await Printing.PrintStringAsync(socket, textToPrint);
        }

        /// <summary>
        /// Prints an image. (Doesn't work)
        /// </summary>
        /// <param name="sender">The button itself</param>
        /// <param name="e">What happens when you click a button</param>
        private async void PrintImages(object sender, EventArgs e)
        {
            BluetoothSocket socket = await Printing.ConnectToPrinterAsync();
            if (socket == null) return;

            IEnumerable<FileResult> image_files = await Printing.SelectImageFilesAsync();
            if (image_files == null) return;

            await Printing.PrintImageFilesAsync(socket, image_files);
            
            socket.Close();
            socket.Dispose();
        }
        /// <summary>
        /// Testing purposes only. For when you want to workshop something, and to not modify existing code.
        /// </summary>
        /// <param name="sender">The button itself</param>
        /// <param name="e">What happens when you click a button</param>
        private async void Test(object sender, EventArgs e)
        {
            BluetoothSocket socket = await Printing.ConnectToPrinterAsync();
            if (socket == null) return;

            IEnumerable<FileResult> image_files = await Printing.SelectImageFilesAsync();
            if (image_files == null) return;

            foreach (var image in image_files)
            {
                var path = image.FullPath;
                await Printing.PrintImageFilesAsync(path, socket);
            }

            var status = "Print job completed.";
            Debug.WriteLine(status);
            CrossToastPopUp.Current.ShowToastMessage(status);

            socket.Close();
            socket.Dispose();

        }
    }
}
