resource "aws_lb" "moviereview" {
  name               = "moviereview-alb-${var.env}"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.ecs.id]
  subnets            = module.vpc.public_subnets
}


resource "aws_lb_target_group" "moviereview" {
  name     = "moviereview-tg-${var.env}"
  port     = 80
  protocol = "HTTP"
  vpc_id   = module.vpc.vpc_id

  target_type = "ip" 

  health_check {
    path                = "/"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 2
    matcher             = "200-399"
  }
}

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.moviereview.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.moviereview.arn
  }
}

resource "aws_security_group" "ecs" {
  name        = "moviereview-ecs-sg-${var.env}"
  vpc_id      = module.vpc.vpc_id
  description = "Allow ECS traffic"

  ingress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}


resource "aws_ecs_cluster" "moviereview" {
  name = "moviereview-cluster-${var.env}"
}

data "aws_ecr_repository" "moviereview" {
  name = "moviereview"
}

resource "aws_iam_role" "ecs_task_execution" {
  name               = "ecsTaskExecutionRole-${var.env}"
  assume_role_policy = data.aws_iam_policy_document.ecs_task_assume.json
}

data "aws_iam_policy_document" "ecs_task_assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "Service"
      identifiers = ["ecs-tasks.amazonaws.com"]
    }
  }
}

resource "aws_iam_role_policy_attachment" "ecs_task_exec_attach" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role_policy_attachment" "ecs_logs" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/CloudWatchLogsFullAccess"
}

resource "aws_iam_role_policy_attachment" "ecs_secrets" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/SecretsManagerReadWrite"
}


resource "aws_ecs_task_definition" "moviereview_task" {
  family                   = "moviereview-task-${var.env}"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "1024"
  memory                   = "2048"
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn

  runtime_platform {
    operating_system_family = "WINDOWS_SERVER_2022_CORE"
    cpu_architecture        = "X86_64"
  }

  container_definitions = jsonencode([
    {
      name      = "moviereview-windows"
      image     = "${data.aws_ecr_repository.moviereview.repository_url}:latest"
      essential = true

      portMappings = [
        {
          containerPort = 80
          protocol      = "tcp"
        }
      ]

      secrets = [
        {
          name      = "ConnectionStrings__MovieReview"
          valueFrom = "arn:aws:secretsmanager:us-east-2:188244335075:secret:moviereview-db-credentials-2hUqS9:connection_string::"
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = "/ecs/moviereview-${var.env}"
          awslogs-region        = "us-east-2"
          awslogs-stream-prefix = "ecs"
        }
      }
    }
  ])
}



resource "aws_cloudwatch_log_group" "ecs" {
  name              = "/ecs/moviereview-${var.env}"
  retention_in_days = 7
}



resource "aws_ecs_service" "moviereview_service" {
  name             = "moviereview-service-${var.env}"
  cluster          = aws_ecs_cluster.moviereview.id
  task_definition  = aws_ecs_task_definition.moviereview_task.arn
  desired_count    = 1
  launch_type      = "FARGATE"
  platform_version = "1.0.0"

  network_configuration {
    subnets          = module.vpc.public_subnets
    security_groups  = [aws_security_group.ecs.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.moviereview.arn
    container_name   = "moviereview-windows"
    container_port   = 80
  }

  depends_on = [aws_lb_listener.http]
}

