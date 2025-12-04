resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-hcl-booking"
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "hclbooking"

  default_node_pool {
    name       = "default"
    node_count = 1
    vm_size    = "Standard_B2s"
    vnet_subnet_id = var.vnet_subnet_id
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin = "azure"
  }
}
