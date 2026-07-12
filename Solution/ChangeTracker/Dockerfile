FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Solution/ChangeTracker/ChangeTracker.csproj Solution/ChangeTracker/
COPY Solution/Shared/Shared.csproj Solution/Shared/

RUN dotnet restore Solution/ChangeTracker/ChangeTracker.csproj

COPY . .

RUN dotnet publish Solution/ChangeTracker/ChangeTracker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ChangeTracker.dll"]