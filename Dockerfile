# syntax=docker/dockerfile:1

# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, using only the project files, so this layer is cached
# until a dependency changes.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/TravelTogether.Domain/TravelTogether.Domain.csproj src/TravelTogether.Domain/
COPY src/TravelTogether.Application/TravelTogether.Application.csproj src/TravelTogether.Application/
COPY src/TravelTogether.Infrastructure/TravelTogether.Infrastructure.csproj src/TravelTogether.Infrastructure/
COPY src/TravelTogether.WebApi/TravelTogether.WebApi.csproj src/TravelTogether.WebApi/
RUN dotnet restore src/TravelTogether.WebApi/TravelTogether.WebApi.csproj

COPY src/ src/
RUN dotnet publish src/TravelTogether.WebApi/TravelTogether.WebApi.csproj \
    --configuration Release --no-restore --output /app/publish

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The official image ships a non-root user; run as it.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TravelTogether.WebApi.dll"]
