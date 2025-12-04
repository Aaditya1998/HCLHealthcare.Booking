# Deployment Setup Script
# Run this script after installing Azure CLI and logging in with `az login`

$RESOURCE_GROUP = "rg-hcl-booking"
$LOCATION = "eastus"
$STORAGE_ACCOUNT_NAME = "sttfstate" + (Get-Random -Minimum 1000 -Maximum 9999)
$CONTAINER_NAME = "tfstate"
$KV_NAME = "kv-hcl-" + (Get-Random -Minimum 1000 -Maximum 9999)

Write-Host "Creating Resource Group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

Write-Host "Creating Storage Account for Terraform State..."
az storage account create --resource-group $RESOURCE_GROUP --name $STORAGE_ACCOUNT_NAME --sku Standard_LRS --encryption-services blob
$ACCOUNT_KEY = az storage account keys list --resource-group $RESOURCE_GROUP --account-name $STORAGE_ACCOUNT_NAME --query "[0].value" -o tsv

Write-Host "Creating Blob Container..."
az storage container create --name $CONTAINER_NAME --account-name $STORAGE_ACCOUNT_NAME --account-key $ACCOUNT_KEY

Write-Host "Creating Key Vault..."
az keyvault create --name $KV_NAME --resource-group $RESOURCE_GROUP --location $LOCATION

Write-Host "--------------------------------------------------"
Write-Host "SETUP COMPLETE"
Write-Host "--------------------------------------------------"
Write-Host "Please configure the following Secrets in your GitHub Repository:"
Write-Host "AZURE_RESOURCE_GROUP: $RESOURCE_GROUP"
Write-Host "AZURE_STORAGE_ACCOUNT: $STORAGE_ACCOUNT_NAME"
Write-Host "AZURE_STORAGE_CONTAINER: $CONTAINER_NAME"
Write-Host "AZURE_STORAGE_KEY: $ACCOUNT_KEY"
Write-Host "--------------------------------------------------"
Write-Host "Next Steps:"
Write-Host "1. Create a Service Principal or User Assigned Identity for OIDC."
Write-Host "2. Add the Federated Credential to trust your GitHub Repo."
Write-Host "3. Add AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID to GitHub Secrets."
