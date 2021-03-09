using System;
using System.Diagnostics;
using System.Threading.Tasks;

using RestSharp;
using Plugin.Toast;


namespace App1
{
    public static class Request
    {
        // Summary:
        //   The request class hosts the functions associated with uploading, and getting the credentials of the user.
        //    Getting their respective api key, assuming that they have provided the correct username, and password.
        

        private static readonly RestClient client = new RestClient();
        public static async Task<UploadRequest> Upload(string uri, string path, string name)
        {
            /*
            Summary:
                Makes a request to the upload endpoint. And uploads the images. 
            */
            client.BaseUrl = new Uri(uri);
            Debug.WriteLine("Making a PUT request");
            var request = new RestRequest($"upload/{name}", DataFormat.Json);
            try
            {
                Debug.WriteLine($"Uri: {client.BaseUrl}{request.Resource}");

                var completeRequest = request.AddFile("image", path);
                var content = await client.PutAsync<UploadRequest>(completeRequest);
                CrossToastPopUp.Current.ShowCustomToast("Request sent", "white", "green");

                Debug.WriteLine("Request made, returning result");

                return content;
            } catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowCustomToast("Unable to send request.", "white", "red");
                CrossToastPopUp.Current.ShowCustomToast($"{client.BaseUrl}{request.Resource}", "white", "red");
                Debug.WriteLine($"Error message:\n{ex.Message}");
                UploadRequest upload = new UploadRequest()
                {
                    Code = 400
                };
                return upload;
            }
        }

        public static async Task<Credentials> Login(string uri, string resource)
        {
            Debug.WriteLine("Making a GET request, to get crendetials");
            client.BaseUrl = new Uri(uri);
            var request = new RestRequest(resource, DataFormat.Json);
            var content = await client.GetAsync<Credentials>(request);

            Debug.WriteLine("Requested made, returning result");
            return content;
        }

        public static async Task<string> Get_Location(string uri, string location_name)
        {
            client.BaseUrl = new Uri(uri);
            var request = new RestRequest($"location/{location_name}");
            var result = await client.GetAsync<Locale>(request);

            return null;
        }
    }
}
