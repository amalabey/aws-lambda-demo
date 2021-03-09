using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using HandlebarsDotNet;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace COVIDSafe
{
    public class FormRenderFunction
    {
        private const string LOCATIONS_TABLE_NAME = "aa-covidsafe-locations";
        private readonly AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient();

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            try 
            {
                var locationId = apigProxyEvent.QueryStringParameters["id"];
                var locationItem = await GetLocationNameAsync(locationId);
                var locationName = locationItem["name"];
                var submitUrl = $"https://{apigProxyEvent.RequestContext.DomainName}/Prod/register";
                LambdaLogger.Log($"locationName: {locationName}, submitUrl: {submitUrl}");

                var html = RenderRegistrationForm(locationId, locationName, submitUrl);
                LambdaLogger.Log($"rendered html: {html}");

                return new APIGatewayProxyResponse
                {
                    Body = html,
                    StatusCode = 200,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/html" } }
                };
            }catch(Exception ex)
            {
                LambdaLogger.Log($"exception message: {ex.Message}");
                LambdaLogger.Log($"stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<Document> GetLocationNameAsync(string locationId)
        {
            var locationsTable = Table.LoadTable(_dynamoDbClient, LOCATIONS_TABLE_NAME);
            var hash = new Primitive(locationId, false);
            var locationItem = await locationsTable.GetItemAsync(hash);
            return locationItem;
        }

        private string RenderRegistrationForm(string locationId, string locationName, string submitUrl)
        {
            string formTemplate = @"<html>
            <head>
                <meta content='text/html;charset=utf-8' http-equiv='Content-Type'>
                <meta content='utf-8' http-equiv='encoding'>
                <title>Register</title>
                <script src='https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js'></script>
                <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.0.0-beta2/dist/css/bootstrap.min.css' rel='stylesheet' integrity='sha384-BmbxuPwQa2lc/FVzBcNJ7UAyJxM6wuqIj61tLrc4wSX0szH/Ev+nYRRuWlolflfl' crossorigin='anonymous'>
            </head>
            <body>
                <form>
                    <h3> You are visiting {{locationName}} </h3>
                    <p> Please register using below form. </p>
                    <input type='hidden' id='locationId' value='{{locationId}}' /><br/>
                    <div class='form-group'>
                        <label for='fullName' >Name:</label>
                        <input type='text' id='fullName' placeholder='Enter full name' class='form-control' style='width: 300px;'><br/>
                    </div>
                    <div class='form-group'>
                        <label for='phone'>Phone:</label>
                        <input type='text' id='phone' placeholder='Phone number' class='form-control' style='width: 200px;'><br/>
                    </div>
                    <button id='submit' type='submit' class='btn btn-primary'>Submit</button>
                </form>
                <script>
                    const submit = document.querySelector('#submit')
                    const locationId = document.querySelector('#locationId')
                    const fullName = document.querySelector('#fullName')
                    const phone = document.querySelector('#phone')

                    submit.onclick = (e) => {
                        e.preventDefault();
                        axios.post('{{submitUrl}}', {
                            locationId: locationId.value,
                            fullName: fullName.value,
                            phone: phone.value
                        },
                        {
                            headers: {
                                ContentType: 'application/json'
                            }
                        })
                        .then(res => {
                        })
                        .catch(err => {
                        })
                    }
                </script>
            </body>
            </html>";

            var template = Handlebars.Compile(formTemplate);
            var data = new {
                locationId = locationId,
                locationName = locationName,
                submitUrl = submitUrl
            };
            var result = template(data);
            return result;
        }

    }
}
