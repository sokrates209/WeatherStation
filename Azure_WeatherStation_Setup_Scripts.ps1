# C:\Users\Tomek\Desktop\Azure_ctx.json - context file to use with azure powershell session

#login
$account = Import-AzureRmContext -Path C:\Users\Tomek\Desktop\Azure_ctx.json
$account

#get subscription
$subscription = Get-AzureRmSubscription -SubscriptionName EcoVadis-ITD-DEV
$subscription

#try get resource group for IOT if not existing create
$resourceGroup = Get-AzureRmResourceGroup -Location "West US" -Name IOT
if(-not $resourceGroup){
    $resourceGroup = New-AzureRmResourceGroup -Location "West US" -Name IOT
}
$resourceGroup

#try get IOT Hub if not existing create
$iotHub = Get-AzureRmIotHub -Name IOT -ResourceGroupName IOT
if(-not $iotHub){
    $iotHub =  New-AzureRmIotHub -Location "West US" -Name EcoVadisWeatherStation -ResourceGroupName IOT -SkuName F1 -Units 1   
}
$iotHub

#Get-AzureSubscription

#Remove-AzureAccount 