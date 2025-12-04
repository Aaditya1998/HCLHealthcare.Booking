resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-hcl-booking"
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "hclbooking"

  default_node_pool {
    name       = "default"
    node_count = 3
    vm_size    = "Standard_D2s_v3"
    vnet_subnet_id = var.vnet_subnet_id
    enable_auto_scaling = true
    min_count = 3
    max_count = 10
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin = "azure"
    network_policy = "azure"
  }
}
