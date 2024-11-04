FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update -y && apt-get install -y iputils-ping
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Manager_App.csproj", "."]
RUN dotnet restore "./Manager_App.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Manager_App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Manager_App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Manager_App.dll"]