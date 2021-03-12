[CmdletBinding()]
param (
    [Parameter(Mandatory=$True)]
    [string]$StackName,
    [Parameter()]
    [string]$Region = "us-east-1",
    [Parameter()]
    [switch]$Build,
    [Parameter()]
    [switch]$SeedTestData
)

if ($Build -eq $True) {
    Write-Host "Building API"
    Remove-Item -LiteralPath ".aws-sam" -Force -Recurse
    sam build
}

Write-Host "Deploying SAM template"
sam deploy --stack-name $StackName `
    --resolve-s3 `
    --no-confirm-changeset `
    --capabilities CAPABILITY_NAMED_IAM

if($SeedTestData -eq $True) {
    Write-Host "Adding test data"
    aws dynamodb put-item `
    --table-name aa-covidsafe-locations `
    --item file://test/test-data/conf-room1-location.json `
    --return-consumed-capacity TOTAL `
    --return-item-collection-metrics SIZE

    aws dynamodb put-item `
    --table-name aa-covidsafe-locations `
    --item file://test/test-data/kitchen-location.json `
    --return-consumed-capacity TOTAL `
    --return-item-collection-metrics SIZE
}

