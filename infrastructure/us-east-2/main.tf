provider "aws" {
  region = var.aws_region
}

terraform {
  backend "s3" {
    bucket = "moviereview-terraform-state-bucket"
    key    = "moviereview/terraform.tfstate"
    region = "us-east-2"
  }
}
