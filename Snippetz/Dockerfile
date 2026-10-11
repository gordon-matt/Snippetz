# Snippetz web container.

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app
EXPOSE 8080
RUN mkdir -p /app/data \
    && apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Snippetz/Snippetz.csproj", "Snippetz/"]
COPY ["Snippetz.Core/Snippetz.Core.csproj", "Snippetz.Core/"]
COPY ["Snippetz.Data/Snippetz.Data.csproj", "Snippetz.Data/"]
COPY ["Snippetz.Data.MySql/Snippetz.Data.MySql.csproj", "Snippetz.Data.MySql/"]
COPY ["Snippetz.Data.Npgsql/Snippetz.Data.Npgsql.csproj", "Snippetz.Data.Npgsql/"]
COPY ["Snippetz.Data.Sql/Snippetz.Data.Sql.csproj", "Snippetz.Data.Sql/"]
COPY ["Snippetz.Data.Sqlite/Snippetz.Data.Sqlite.csproj", "Snippetz.Data.Sqlite/"]
COPY ["Snippetz.Models/Snippetz.Models.csproj", "Snippetz.Models/"]
COPY ["Snippetz.Services/Snippetz.Services.csproj", "Snippetz.Services/"]
RUN dotnet restore "./Snippetz/Snippetz.csproj"
COPY . .
WORKDIR "/src/Snippetz"
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli --version 2.1.113 && \
    export PATH="${PATH}:/root/.dotnet/tools" && \
    libman restore --verbose
RUN dotnet build "./Snippetz.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Snippetz.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN cp -r /src/Snippetz/wwwroot/lib /app/publish/wwwroot/ || true

FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Snippetz.dll"]
