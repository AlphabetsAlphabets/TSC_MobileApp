using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Essentials;

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
        static string iPv4 = "192.168.1.143:5000";
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

            string imagePath = "";
            int count = 0;

            CrossToastPopUp.Current.ShowToastMessage("Uploading image(s)");
            foreach (var image in images)
            {
                try
                {
                    Debug.WriteLine($"Image name: {image.FileName}");
                    imagePath = image.FullPath;

                    Debug.WriteLine($"Image path: {imagePath}");

                    Console.WriteLine($"URI: {uri}");
                    var result = await Request.Upload(uri, imagePath, image.FileName);

                    if (result.Code > 299) throw new HttpErrorException(result.Code, result.Message);
                    else if (result.Code == 200) Debug.WriteLine($"Request successful, image has been uploaded to: {result.Path}");

                } catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    count = 1;
                    break;
                }
            }
            if (count == 0) { statusBar.Text = "Image(s) successfully uploaded!"; CrossToastPopUp.Current.ShowCustomToast("Uploaded.", "grey", "green"); }
            else if (count == 1) { statusBar.Text = "Image(s) not uploaded."; CrossToastPopUp.Current.ShowCustomToast("Not uploaded.", "grey", "red"); }
        }
    }
}
