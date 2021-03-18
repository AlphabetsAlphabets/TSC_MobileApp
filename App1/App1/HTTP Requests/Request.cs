using System;
using System.Diagnostics;
using System.Collections.Generic;
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

                Debug.WriteLine("Request made, returning result");

                return content;
            } catch (Exception ex)
            {
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

        public static async Task<Locale> Get_Location(string uri)
        {
            client.BaseUrl = new Uri(uri);
            var request = new RestRequest("locations", DataFormat.Json);
            var result = await client.GetAsync<Locale>(request);

            return result;
        }
    }
}
