FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

WORKDIR /inetpub/wwwroot
COPY ./publish/ ./

# Default connection string (overridden in ECS via env vars)
ENV ConnectionStrings__MovieReview="Data Source=(LocalDb)\\v11.0;Initial Catalog=MovieReview;Integrated Security=True;AttachDBFilename=|DataDirectory|\\MovieReview.mdf"

RUN powershell -NoProfile -Command "Remove-WebBinding -Name 'Default Web Site' -Port 80 -Protocol http -ErrorAction SilentlyContinue; New-WebBinding -Name 'Default Web Site' -Protocol http -Port 80 -IPAddress '*'"

EXPOSE 80
