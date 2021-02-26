using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Essentials;

using System.Collections;

using Plugin.Media.Abstractions;
using Plugin.Media;


using System.Net.Http;
using System.Threading.Tasks;

using Plugin.Toast;

namespace App1
{
    public partial class MainPage : ContentPage
    {
        // Fields
        static string iPv4 = "192.168.1.143:5000/";
        static string uri = $"http://{iPv4}";

        public MainPage()
        {
            InitializeComponent();
        }

        private async void Select_Picture(object sender, EventArgs e)
        {
            var images = await FilePicker.PickMultipleAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Pick image(s)"
            });

            if (images == null) { Debug.WriteLine("User cancelled operation"); return; }

            string imagePath = "";
            string imageName = "";
            int count = 0;

            CrossToastPopUp.Current.ShowToastMessage("Uploading image(s)");
            foreach (var image in images)
            {
                try
                {
                    Debug.WriteLine($"Image name: {image.FileName}");
                    imageName = image.FileName;
                    imagePath = image.FullPath;

                    Debug.WriteLine($"Image path: {imagePath}");

                    Console.WriteLine($"URI: {uri}");
                    var result = await Request.Upload(uri, imagePath, image.FileName);

                    if (result.Code > 299) throw new HttpErrorException(result.Code, result.Message);
                    else if (result.Code == 200)
                    {
                        CrossToastPopUp.Current.ShowCustomToast($"Uploaded {imageName}", "grey", "green");
                        Debug.WriteLine($"Request successful, image has been uploaded to: {result.Path}");
                    }

                } catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    count = 1;
                    break;
                }
            }
            if (count == 1) {
                CrossToastPopUp.Current.ShowCustomToast($"Image {imageName} not uploaded.", "grey", "red");
                CrossToastPopUp.Current.ShowCustomToast($"URI: {uri}", "grey", "red");
            }
        }
    }
}
