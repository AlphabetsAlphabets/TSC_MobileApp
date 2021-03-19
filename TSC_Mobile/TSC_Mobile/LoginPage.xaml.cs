using System;

using Plugin.Toast;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TSC_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        public Page mainPage = new MainPage();

        public static string uri = MainPage.uri;

        private async void ValidateCredentials(object sender, EventArgs e)
        {
            var password = PasswordField.Text;
            var username = UsernameField.Text;
            var resource = $"{username}/{password}";

            var credentials = await Request.Login(uri, resource);
            if (credentials.Message != null) {
                alert.Text = "Incorrect credentials.";
                return;
            }

            CrossToastPopUp.Current.ShowToastMessage("Logged in successfully.");
            await Navigation.PushModalAsync(mainPage);
        }
    }
}
