using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

using RestSharp;

<<<<<<< HEAD:TSC_Mobile/TSC_Mobile/HTTP Requests/Request.cs
namespace TSC_Mobile
=======
namespace MobileApp
>>>>>>> time:MobileApp/HTTP Requests/Request.cs
{
    /// <summary>
    /// The request class hosts the functions associated HTTP requests to a web API
    /// </summary>
    public static class Request
    {
        private static readonly RestClient client = new RestClient();

        /// <summary>
        /// Connects to an endpoint and uploads the image to the server.
        /// </summary>
        /// <param name="uri">uri of the request</param>
        /// <param name="path">path of the image</param>
        /// <param name="name">name of the image</param>
        /// <returns>UploadRequest</returns>
        public static async Task<UploadRequest> Upload(string uri, string path, string name)
        {
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

        /// <summary>
        /// Connects to an endpoint and gets the credentials of the user. Provided the authentication information
        /// is correct.
        /// </summary>
        /// <param name="uri">uri of the request</param>
        /// <param name="resource">resource of the request</param>
        /// <returns>Credentials</returns>
        public static async Task<Credentials> Login(string uri, string resource)
        {
            Debug.WriteLine("Making a GET request, to get crendetials");
            client.BaseUrl = new Uri(uri);
            var request = new RestRequest(resource, DataFormat.Json);
            var content = await client.GetAsync<Credentials>(request);

            Debug.WriteLine("Requested made, returning result");
            return content;
        }

        /// <summary>
        /// Gets location information of various clients of TSC.
        /// </summary>
        /// <param name="uri">uri of request</param>
        /// <returns>Locale</returns>
        public static async Task<Locale> Get_Location(string uri)
        {
            client.BaseUrl = new Uri(uri);
            var request = new RestRequest("locations", DataFormat.Json);
            var result = await client.GetAsync<Locale>(request);

            return result;
        }
    }
}
