variable "aws_region" {
  default = "us-east-2"
}

variable "env" {
  default = "dev"
}

variable "image_tag" {
  default = "latest"
}

variable "db_instance_class" {
  default = "db.t3.micro"
}

variable "db_allocated_storage" {
  default = 20
}
