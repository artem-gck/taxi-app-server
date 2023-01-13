# #See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
# WORKDIR /app
# EXPOSE 80

# FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# WORKDIR /src
# COPY ["DriversServiceApi/DriversServiceApi.csproj", "DriversServiceApi/"]
# COPY ["DriversService.Adapters.Consumers/DriversService.Adapters.Consumers.csproj", "DriversService.Adapters.Consumers/"]
# COPY ["DriversService.Ports.DataBase/DriversService.Ports.DataBase.csproj", "DriversService.Ports.DataBase/"]
# COPY ["DriversService.Domain/DriversService.Domain.csproj", "DriversService.Domain/"]
# COPY ["DriversService.Adapters.DataBase/DriversService.Adapters.DataBase.csproj", "DriversService.Adapters.DataBase/"]
# RUN dotnet restore "DriversServiceApi/DriversServiceApi.csproj"
# COPY . .
# WORKDIR "/src/DriversServiceApi"
# RUN dotnet build "DriversServiceApi.csproj" -c Release -o /app/build

# FROM build AS publish
# RUN dotnet publish "DriversServiceApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "DriversServiceApi.dll"]

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source
COPY ["Contracts.Shared", "Contracts.Shared"]
COPY ["DriversService", "DriversService"]

RUN dotnet restore "DriversService/DriversServiceApi/DriversServiceApi.csproj"
RUN dotnet publish "DriversService/DriversServiceApi/DriversServiceApi.csproj" -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
COPY --from=build /app ./

EXPOSE 80

ENTRYPOINT ["dotnet", "DriversServiceApi.dll"]