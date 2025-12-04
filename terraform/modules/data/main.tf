resource "azurerm_mssql_server" "main" {
  name                         = "sql-hcl-booking-${random_id.server.hex}"
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = "sqladmin"
  administrator_login_password = var.sql_password
  minimum_tls_version          = "1.2"

  azuread_administrator {
    login_username = "AzureAD Admin"
    object_id      = var.admin_object_id
  }
}

resource "azurerm_mssql_database" "main" {
  name           = "HCLHealthcareBooking"
  server_id      = azurerm_mssql_server.main.id
  sku_name       = "HS_Gen5_2" # Hyperscale
  zone_redundant = true
}

resource "azurerm_private_endpoint" "sql" {
  name                = "pe-sql"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.subnet_id

  private_service_connection {
    name                           = "sql-privatelink"
    private_connection_resource_id = azurerm_mssql_server.main.id
    is_manual_connection           = false
    subresource_names              = ["sqlServer"]
  }
}

resource "random_id" "server" {
  byte_length = 4
}
