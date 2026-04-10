FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
# Cổng mặc định của Render là 10000 hoặc dùng biến môi trường PORT
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "ConnectDB.dll"]