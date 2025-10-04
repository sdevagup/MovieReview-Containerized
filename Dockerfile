FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

WORKDIR /inetpub/wwwroot
COPY ./publish/ ./

ARG ConnectionStrings__MovieReview
ENV ConnectionStrings__MovieReview=${ConnectionStrings__MovieReview}

RUN powershell -NoProfile -Command "Remove-WebBinding -Name 'Default Web Site' -Port 80 -Protocol http -ErrorAction SilentlyContinue; New-WebBinding -Name 'Default Web Site' -Protocol http -Port 80 -IPAddress '*'"

EXPOSE 80
