using System;
using Newtonsoft.Json;
using System.Collections.Generic;

/*
Main purpose is to be able to parse json. And host custom exception messages.
*/

namespace App1
{
    [Serializable] // Upload endpoint
    public class UploadRequest
    {
        [JsonProperty("success")]
        public string Success { get; }

        [JsonProperty("message")]
        public string Message { get; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    [Serializable]
    public class Credentials // login endpoint
    {
        [JsonProperty("fid")]
        public string Fid { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
    }

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

    class HttpErrorException : Exception
    {
        public HttpErrorException(int errorCode, string message)
        {
            String.Format($"Error code: {errorCode}, message: {message}");
        }
    }
}
