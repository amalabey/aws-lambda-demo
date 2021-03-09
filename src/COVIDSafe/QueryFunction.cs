using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using HandlebarsDotNet;
using System;
using Amazon.DynamoDBv2.Model;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;

namespace COVIDSafe
{
    public class QueryFunction
    {
        private const string REGISTRATIONS_TABLE_NAME = "aa-covidsafe-registrations";
        private readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();


        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            try 
            {
                var locationId = apigProxyEvent.QueryStringParameters.ContainsKey("locationId") ? apigProxyEvent.QueryStringParameters["locationId"] : null;
                var dateText = apigProxyEvent.QueryStringParameters.ContainsKey("date") ? apigProxyEvent.QueryStringParameters["date"] : null;
                LambdaLogger.Log($"locationId: {locationId}, dateText: {dateText}");

                var visits = await QueryVisits(locationId, dateText);
                var visitsJson = JsonConvert.SerializeObject(visits);
                LambdaLogger.Log($"json: {visitsJson}");
                
                return new APIGatewayProxyResponse
                {
                    Body = visitsJson,
                    StatusCode = 200,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }catch(Exception ex)
            {
                LambdaLogger.Log($"exception message: {ex.Message}");
                LambdaLogger.Log($"stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<IEnumerable<Visit>> QueryVisits(string locationId, string dateText)
        {
            var visits = new List<Visit>();
            var queryRequest = BuildQuery(locationId, dateText);

            var response = await _dynamoDbClient.QueryAsync(queryRequest);
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                visits.Add(new Visit{
                    FullName = item.ContainsKey("fullName") ? item["fullName"].S : null,
                    Phone = item.ContainsKey("phone") ? item["phone"].S : null,
                    Timestamp = item.ContainsKey("timestamp") ? DateTime.ParseExact(item["timestamp"].N, "yyyyMMddHHmmssffff", new CultureInfo("en-AU")) : new DateTime(1900, 1, 1),
                    Location = item.ContainsKey("locationName") ? item["locationName"].S : null
                });
            }

            return visits;
        }

        private QueryRequest BuildQuery(string locationId, string dateText)
        {
            var request = new QueryRequest
            {
                TableName = REGISTRATIONS_TABLE_NAME,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            };
            var criteriaBuilder = new StringBuilder();

            if(!string.IsNullOrEmpty(locationId))
            {
                criteriaBuilder.Append("locationId = :v_locationId");
                request.ExpressionAttributeValues.Add(":v_locationId", new AttributeValue { S = locationId });
            }

            if(!string.IsNullOrEmpty(dateText))
            {
                var date = DateTime.Parse(dateText);
                var timestampRangeStart = date.ToString("yyyyMMdd0000000000");
                var timestampRangeEnd = date.AddDays(1).ToString("yyyyMMdd0000000000");

                if(criteriaBuilder.Length > 0) criteriaBuilder.Append(" and ");
                criteriaBuilder.Append("timestamp >= :v_timestampstart and timestamp < :v_timestampend");
                request.ExpressionAttributeValues.Add(":v_timestampstart", new AttributeValue { N = timestampRangeStart });
                request.ExpressionAttributeValues.Add(":v_timestampend", new AttributeValue { N = timestampRangeEnd });
            }

            request.KeyConditionExpression = criteriaBuilder.ToString();

            return request;
        }
    }

    class Visit
    {
        public string FullName{get; set;}
        public string Phone{get; set;}
        public DateTime Timestamp {get; set;}
        public string Location {get; set;}
    }
}
