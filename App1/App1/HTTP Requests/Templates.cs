using Newtonsoft.Json;
using System;

/*
Main purpose is to be able to parse json
*/

namespace App1
{
    [Serializable]
    public class Sync
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
    }

    [Serializable]
    public class Mock // mock request endpoint
    {
        [JsonProperty("first")]
        public string First { get; }
        [JsonProperty("second")]
        public string Second { get; }
        [JsonProperty("third")]
        public string Thrid { get; }
    }

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
    class HttpErrorException : Exception
    {
        public HttpErrorException(int errorCode, string message)
        {
            String.Format($"Error code: {errorCode}, message: {message}");
        }
    }
}
