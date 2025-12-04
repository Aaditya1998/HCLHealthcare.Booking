variable "resource_group_name" {
  default = "rg-hcl-booking"
}

variable "location" {
  default = "East US"
}

variable "sql_password" {
  sensitive = true
}

variable "admin_object_id" {
  description = "Object ID of the Azure AD Admin for SQL"
}
