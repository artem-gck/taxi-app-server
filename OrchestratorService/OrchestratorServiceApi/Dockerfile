#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
#WORKDIR /app
#EXPOSE 80
#
#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#WORKDIR /src
#COPY ["OrchestratorServiceApi/OrchestratorServiceApi.csproj", "OrchestratorServiceApi/"]
#COPY ["OrchestratorService.Saga/OrchestratorService.Saga.csproj", "OrchestratorService.Saga/"]
#RUN dotnet restore "OrchestratorServiceApi/OrchestratorServiceApi.csproj"
#COPY . .
#WORKDIR "/src/OrchestratorServiceApi"
#RUN dotnet build "OrchestratorServiceApi.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "OrchestratorServiceApi.csproj" -c Release -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "OrchestratorServiceApi.dll"]

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source
COPY ["Contracts.Shared", "Contracts.Shared"]
COPY ["OrchestratorService", "OrchestratorService"]

RUN dotnet restore "OrchestratorService/OrchestratorServiceApi/OrchestratorServiceApi.csproj"
RUN dotnet publish "OrchestratorService/OrchestratorServiceApi/OrchestratorServiceApi.csproj" -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
COPY --from=build /app ./

EXPOSE 80

ENTRYPOINT ["dotnet", "OrchestratorServiceApi.dll"]