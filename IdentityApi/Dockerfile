#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

VOLUME /app/var/data/
COPY ["IdentityApi/var/data/medlemmer.json", "/app/var/data/"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IdentityApi/IdentityApi.csproj", "IdentityApi/"]
RUN dotnet restore "IdentityApi/IdentityApi.csproj"
COPY . .
WORKDIR "/src/IdentityApi"
COPY ["IdentityApi/identityapi.pfx", "/etc/ssl/certs/"]
RUN dotnet build "IdentityApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IdentityApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityApi.dll"]