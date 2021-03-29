using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

using RestSharp;

namespace MobileApp
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
            var partiallyFormedRequest = new RestRequest($"upload/{name}", DataFormat.Json);
            try
            {
                Debug.WriteLine($"Uri: {client.BaseUrl}{partiallyFormedRequest.Resource}");

                var completeRequest = partiallyFormedRequest.AddFile("image", path);
                var apiResponse = await client.PutAsync<UploadRequest>(completeRequest);

                Debug.WriteLine("Request made, returning result");

                return apiResponse;
            } catch (Exception ex)
            {
                Debug.WriteLine($"Error message:\n{ex.Message}");
                return null;
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
            var apiResponse = await client.GetAsync<Credentials>(request);

            Debug.WriteLine("Requested made, returning result");
            return apiResponse; 
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
            var apiResponse = await client.GetAsync<Locale>(request);

            return apiResponse;
        }

        public static FirstTimeSetup FTS(string uri)
        {
            client.BaseUrl = new Uri(uri);
            var request = new RestRequest("fts", DataFormat.Json);
            var apiResponse = client.Get<FirstTimeSetup>(request).Data;

            return apiResponse;
        }
    }
}
