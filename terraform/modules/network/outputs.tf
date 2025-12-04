output "aks_subnet_id" {
  value = azurerm_subnet.aks.id
}

output "privatelink_subnet_id" {
  value = azurerm_subnet.privatelink.id
}
