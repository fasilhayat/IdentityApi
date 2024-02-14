#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app
EXPOSE 80
EXPOSE 443

VOLUME /app/var/data/
COPY ["IdentityApi/var/data/medlemmer.json", "/app/var/data/"]

COPY . ./
RUN dotnet publish "IdentityApi/IdentityApi.csproj" -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IdentityApi.dll"]