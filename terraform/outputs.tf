output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "aks_cluster_name" {
  value = module.aks.cluster_name
}

output "sql_server_fqdn" {
  value = module.data.sql_server_fqdn
}

output "acr_login_server" {
  value = module.data.acr_login_server
}

output "acr_admin_username" {
  value = module.data.acr_admin_username
}

output "acr_admin_password" {
  value = module.data.acr_admin_password
  sensitive = true
}
