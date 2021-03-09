# AWS Lambda Demo
Demonstrates AWS Lambda with a DynamoDB backend developed using SAM cli/template.  

Example Scenario:  

*In a Covid-Safe office environment, we need to track the movements of staff around the office. Imagine QR codes are installed on every desk, in meeting rooms, and in lunch rooms. The QR Code has the form of a base URL, followed by a Guid that is a unique identifier for that location (e.g. https://yourlambda.amazonaws.com/Prod/LocationId= 69ec46cb-6e87-41af-a020-c693fa9553b9), meeting room or lunch room. Users are expected to use their phones to be redirected to a website that calls an API to register that they were there. The API records the users name and phone number and the time they registered. The API can then be queried by date, or by location guid in order to see where people had been.*


# How To Develop

Refer [here](HowToDev.md) for instructions on how to develop using AWS SAM.


# How to Run the Example

- Deploy the application by running below command. You need to have AWS CLI credentials configured before running the command.
```
.\Deploy.ps1 -Build -SeedTestData
```

- Visit below url to access the registration form:  
https://yourlambda-domain-name/Prod/location?id=f0605b10-9ff9-44fb-87e7-dd7fc52eb4b2

- Query for people who visited a given location using below:
https://yourlambda-domain-name/Prod/query?locationId=06e50221-6c06-48e0-867c-f65f3c516aea
