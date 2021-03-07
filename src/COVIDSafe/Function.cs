using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace COVIDSafe
{

    public class FormRenderFunction
    {

        private static readonly HttpClient client = new HttpClient();

        private static async Task<string> GetCallingIP()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext:false);

            return msg.Replace("\n","");
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {

            var location = await GetCallingIP();
            // var body = new Dictionary<string, string>
            // {
            //     { "message", "hello world" },
            //     { "location", location }
            // };
            var body = "<html><head>Test Form</head><body><h3>this is a test html form. v1.0</body></html>";

            return new APIGatewayProxyResponse
            {
                Body = body,// JsonConvert.SerializeObject(body),
                StatusCode = 200,
                // Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } }
            };
        }
    }
}
