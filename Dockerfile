# image: IIS + ASP.NET 4.8
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2022

WORKDIR /inetpub/wwwroot
COPY ./publish/ ./

# Default connection string (overridden in ECS via env var)
ENV ConnectionStrings__MovieReview="Data Source=(LocalDb)\\v11.0;Initial Catalog=MovieReview;Integrated Security=True;AttachDBFilename=|DataDirectory|\\MovieReview.mdf"
