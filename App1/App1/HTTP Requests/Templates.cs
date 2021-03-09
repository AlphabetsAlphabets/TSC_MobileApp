using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using System;

/*
Main purpose is to be able to parse json, and csv files. And host custom exception messages.
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
    class Locale
    {
        [JsonProperty("location_information")]
        public string Location_Information { get; set; }
    }

    class HttpErrorException : Exception
    {
        public HttpErrorException(int errorCode, string message)
        {
            String.Format($"Error code: {errorCode}, message: {message}");
        }
    }
}
