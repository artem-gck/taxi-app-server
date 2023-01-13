FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source
COPY ["Contracts.Shared", "Contracts.Shared"]
COPY ["UsersService", "UsersService"]

RUN dotnet restore "UsersService/UsersServiceApi/UsersServiceApi.csproj"
RUN dotnet publish "UsersService/UsersServiceApi/UsersServiceApi.csproj" -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
COPY --from=build /app ./

EXPOSE 80

ENTRYPOINT ["dotnet", "UsersServiceApi.dll"]