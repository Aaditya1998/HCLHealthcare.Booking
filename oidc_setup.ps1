# OIDC Setup Script
$GITHUB_REPO = "Aaditya1998/HCLHealthcare.Booking"
$APP_NAME = "hcl-booking-oidc-app"
$RG_NAME = "rg-hcl-booking"

Write-Host "Creating Azure AD Application..."
$appId = az ad app create --display-name $APP_NAME --query appId -o tsv

Write-Host "Creating Service Principal..."
$spId = az ad sp create --id $appId --query id -o tsv

Write-Host "Creating Federated Credentials..."
# Trust main branch
az ad app federated-credential create --id $appId --parameters "{\"name\":\"github-actions-main\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:$GITHUB_REPO:ref:refs/heads/main\",\"description\":\"GitHub Actions for main branch\",\"audiences\":[\"api://AzureADTokenExchange\"]}"

# Trust production environment (optional, but good for environment protection)
az ad app federated-credential create --id $appId --parameters "{\"name\":\"github-actions-prod\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:$GITHUB_REPO:environment:production\",\"description\":\"GitHub Actions for production environment\",\"audiences\":[\"api://AzureADTokenExchange\"]}"

Write-Host "Assigning Contributor Role..."
$subId = az account show --query id -o tsv
az role assignment create --assignee $appId --role Contributor --scope /subscriptions/$subId

Write-Host "Retrieving IDs..."
$tenantId = az account show --query tenantId -o tsv

Write-Host "--------------------------------------------------"
Write-Host "OIDC SETUP COMPLETE"
Write-Host "--------------------------------------------------"
Write-Host "Setting GitHub Secrets..."

gh secret set AZURE_CLIENT_ID --body $appId
gh secret set AZURE_TENANT_ID --body $tenantId
gh secret set AZURE_SUBSCRIPTION_ID --body $subId

Write-Host "Secrets Configured!"
