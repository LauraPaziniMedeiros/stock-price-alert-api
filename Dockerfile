# ==============================================================================
# Stage 1: Build & Test (SDK Image)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copy solution and project files first to leverage Docker layer caching for dependencies
COPY StockPriceAlert.slnx ./
COPY StockPriceAlert/StockPriceAlert.csproj StockPriceAlert/
COPY StockPriceAlert.Tests/StockPriceAlert.Tests.csproj StockPriceAlert.Tests/

RUN dotnet restore StockPriceAlert.slnx

# 2. Copy the remaining source files
COPY StockPriceAlert/ StockPriceAlert/
COPY StockPriceAlert.Tests/ StockPriceAlert.Tests/

# 3. Execute unit tests inside the build container (image build fails if any test fails)
RUN dotnet test StockPriceAlert.Tests/StockPriceAlert.Tests.csproj -c Release --no-restore

# 4. Publish optimized Release binaries
RUN dotnet publish StockPriceAlert/StockPriceAlert.csproj -c Release -o /app/publish --no-restore

# ==============================================================================
# Stage 2: Minimal Runtime Environment
# ==============================================================================
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

# Run as non-root user for security best practices
USER $APP_UID

# Copy compiled binaries from the build stage
COPY --from=build /app/publish .

# Configures the application to accept CLI arguments passed to 'docker run'
ENTRYPOINT ["dotnet", "StockPriceAlert.dll"]
