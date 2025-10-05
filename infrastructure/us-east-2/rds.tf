resource "aws_db_subnet_group" "moviereview" {
  name       = "moviereview-db-subnet-${var.env}"
  subnet_ids = module.vpc.private_subnets
}

resource "aws_security_group" "rds" {
  name        = "moviereview-rds-sg-${var.env}"
  description = "Allow DB access from ECS"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port       = 1433
    to_port         = 1433
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_db_instance" "moviereview" {
  identifier             = "moviereview-db-${var.env}"
  engine                 = "sqlserver-ex"
  engine_version         = "15.00.4043.16.v1"
  instance_class         = var.db_instance_class
  allocated_storage      = var.db_allocated_storage
  username               = local.db_creds["username"]
  password               = local.db_creds["password"]

  db_subnet_group_name   = aws_db_subnet_group.moviereview.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  skip_final_snapshot    = true
}

