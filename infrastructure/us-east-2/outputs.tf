output "alb_dns_name" {
  value = aws_lb.moviereview.dns_name
}

output "db_endpoint" {
  value = aws_db_instance.moviereview.address
}
