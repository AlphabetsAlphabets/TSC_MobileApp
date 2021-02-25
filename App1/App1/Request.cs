using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using RestSharp;
using Plugin.Toast;


namespace App1
{
    public static class Request
    {
        private static RestClient client = new RestClient();
        public static async Task<UploadRequest> Upload(string uri, string path, string name)
        {
            try
            {

                client.BaseUrl = new Uri(uri);
                Debug.WriteLine("Making a PUT request");
                var request = new RestRequest($"upload/{name}", DataFormat.Json);

                Debug.WriteLine($"Uri: {client.BaseUrl}{request.Resource}");

                var completeRequest = request.AddFile("image", path);
                var content = await client.PutAsync<UploadRequest>(completeRequest);
                CrossToastPopUp.Current.ShowCustomToast("Request sent", "white", "green");

                Debug.WriteLine("Request made, returning result");

                return content;
            } catch (Exception ex)
            {
                CrossToastPopUp.Current.ShowCustomToast("Unable to send request.", "white", "red");
                Debug.WriteLine($"Error message:\n{ex.Message}");
                UploadRequest upload = new UploadRequest();
                upload.Code = 400;
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
    }
}
