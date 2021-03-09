using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2.Model;
using System;
using System.Threading;
using Newtonsoft.Json;

namespace COVIDSafe
{
    public class RegisterFunction
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();
        private const string REGISTRATIONS_TABLE_NAME = "aa-covidsafe-registrations";

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            LambdaLogger.Log($"visit data: {apigProxyEvent.Body}");
            var visitData = JsonConvert.DeserializeObject<Dictionary<string, string>>(apigProxyEvent.Body);
            
            await RegisterVisit(visitData["locationId"], visitData["fullName"], visitData["phone"]);
            return new APIGatewayProxyResponse
            {
                Body = "{status:'Success'}",
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        private async Task RegisterVisit(string locationId, string fullName, string phone)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");

            var request = new PutItemRequest
            {
                TableName = REGISTRATIONS_TABLE_NAME,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "locationId", new AttributeValue { S = locationId }},
                    { "visitId", new AttributeValue { S = $"{phone}-{timestamp}" }},
                    { "timestamp", new AttributeValue { S = timestamp }},
                    { "fullName", new AttributeValue { S = fullName }},
                    { "phone", new AttributeValue { S = "phone" }}
                }
            };
            await _dynamoDbClient.PutItemAsync(request);
        }
    }
}
