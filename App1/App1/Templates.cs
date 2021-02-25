using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.IO;

namespace App1
{
[Serializable]
    public class Mock
    {
        [JsonProperty("first")]
        public string First { get; }
        [JsonProperty("second")]
        public string Second { get; }
        [JsonProperty("third")]
        public string Thrid { get; }
    }

    [Serializable]
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
    public class Credentials 
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
