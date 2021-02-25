using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace App1
{
    class bunk
    {
        public static readonly HttpClient client = new HttpClient();
        async public static Task<Mock> MockRequest(string uri, string first, string second)
        {
            string completeUri = uri + "/hello_there";

            StringContent firstCondition = new StringContent(first);
            
            var response = await client.PutAsync(completeUri, firstCondition);
            var result = await response.Content.ReadAsStringAsync();
            var deserializedResult = JsonConvert.DeserializeObject<Mock>(result);

            return deserializedResult;
        }
        async public static Task Main(string[] args)
        {
            string uri = "http://192.168.1.143:5000/mock/new_Image_2";
            var result = await MockRequest(uri, "hello", "there");
            Console.WriteLine("Results!");
            Console.WriteLine($"First: {result.First}, Second: {result.Second}, Third: {result.Thrid}");
        }

    }
}
