terraform {
  backend "azurerm" {}
}

provider "azurerm" {
  features {}
}

data "azurerm_resource_group" "main" {
  name = "rg-hcl-booking"
}

module "network" {
  source              = "./modules/network"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
}

module "aks" {
  source              = "./modules/aks"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  vnet_subnet_id      = module.network.aks_subnet_id
}

module "data" {
  source              = "./modules/data"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  subnet_id           = module.network.privatelink_subnet_id
  sql_password        = var.sql_password
  admin_object_id     = var.admin_object_id
}
