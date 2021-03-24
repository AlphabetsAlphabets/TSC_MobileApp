using System;
using Newtonsoft.Json;
using System.Collections.Generic;

/*
Main purpose is to be able to parse json. And host custom exception messages.
*/

namespace MobileApp
{
    /// <summary>
    /// This is used to upload images. This class is used by the function <see cref="MainPage.Upload_Image(object, EventArgs)"/>
    /// </summary>
    [Serializable] // Upload endpoint
    public class UploadRequest
    {
        [JsonProperty("success")]
        public string Success { get; }

        [JsonProperty("message")]
        public string Message { get; }
    }

    [Serializable]
    public class Credentials // login endpoint, currently not in use.
    {
        [JsonProperty("fid")]
        public string Fid { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = null;
    }

    /// <summary>
    /// This class is used for the <see cref="MainPage.In_Area(object, EventArgs)"/> function. A HTTP GET request is made to an api which will return with two sets
    /// of latitude and longitude of each client's shop.
    /// </summary>
    [Serializable]
    public class Locale // location endpoint
    {
        [JsonProperty("name")]
        public List<string> Name { get; set; }

        [JsonProperty("lat_one")]
        public List<double> Lat_One { get; set; }

        [JsonProperty("lon_one")]
        public List<double> Lon_One { get; set; }

        [JsonProperty("lat_two")]
        public List<double> Lat_Two { get; set; }

        [JsonProperty("lon_two")]
        public List<double> Lon_Two { get; set; }
    }

    /// <summary>
    /// This exception will throw when a request is not made successfully.
    /// See <see cref="MainPage.Upload_Image(object, EventArgs)"/>
    /// </summary>
    class HttpErrorException : Exception
    {
        public HttpErrorException(int errorCode, string message)
        {
            String.Format($"Error code: {errorCode}, message: {message}");
        }
    }
}
