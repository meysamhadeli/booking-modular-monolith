FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

# Copy solution-level files
COPY .editorconfig .
COPY global.json .
COPY Directory.Build.props .

# Copy project files first (better Docker layer caching)
COPY src/BuildingBlocks/BuildingBlocks.csproj src/BuildingBlocks/
COPY src/Modules/Booking/src/Booking.csproj src/Modules/Booking/src/
COPY src/Modules/Flight/src/Flight.csproj src/Modules/Flight/src/
COPY src/Modules/Identity/src/Identity.csproj src/Modules/Identity/src/
COPY src/Modules/Passenger/src/Passenger.csproj src/Modules/Passenger/src/
COPY src/Api/src/Api.csproj src/Api/src/
COPY src/Aspire/src/ServiceDefaults/ServiceDefaults.csproj src/Aspire/src/ServiceDefaults/

# Restore packages
RUN dotnet restore src/Api/src/Api.csproj

# Copy everything else
COPY src ./src

# Build
RUN dotnet build src/Api/src/Api.csproj -c Release --no-restore

# Publish
RUN dotnet publish src/Api/src/Api.csproj -c Release --no-build -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=builder /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=docker

EXPOSE 80

ENTRYPOINT ["dotnet", "Api.dll"]