using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

namespace COVIDSafe
{
    public class RegisterFunction
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            var body = $"<html><head>Test Form</head><body><h3>Register for Location <br/>body: {apigProxyEvent.Body}</body></html>";
            return new APIGatewayProxyResponse
            {
                Body = body,
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } }
            };
        }
    }
}
