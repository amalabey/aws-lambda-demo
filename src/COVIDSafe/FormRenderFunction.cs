using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace COVIDSafe
{
    public class FormRenderFunction
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var locationId = apigProxyEvent.QueryStringParameters["id"];
            var locationItem = await GetLocationNameAsync(locationId);
            var locationName = locationItem["name"];

            var body = $"<html><head>Test Form</head><body><h3>Register for Location {locationName} <br/>querystring: {string.Join(",", apigProxyEvent.QueryStringParameters.Keys)}  <br/>body: {apigProxyEvent.Body}</body></html>";
            return new APIGatewayProxyResponse
            {
                Body = body,
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } }
            };
        }

        private async Task<Document> GetLocationNameAsync(string locationId)
        {
            var locationsTable = Table.LoadTable(_dynamoDbClient, "COVIDSafeLocationsTest");

            var hash = new Primitive(locationId, false);
            var locationItem = await locationsTable.GetItemAsync(hash);

            return locationItem;
        }
    }
}
