using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App1
{
    class Store {
        private async void Select_Picture(object sender, EventArgs e)
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Please pick a photo"
            });

            var stream = await result.OpenReadAsync();
            //resultImage.Source = ImageSource.FromStream(() => stream);
            var image = ImageSource.FromStream(() => stream);
            Console.WriteLine(image);
        }
    }
}
