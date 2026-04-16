# =============================================================================
# Dockerfile for ProductCatalog.Api
# Multi-stage build: restore/build/publish in SDK image, run in runtime image.
# Compatible with Azure Container Apps, AWS ECS, and local Docker.
# =============================================================================

# ── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (layer-cached unless dependencies change)
COPY ["ProductCatalog.sln", "./"]
COPY ["src/ProductCatalog.Api/ProductCatalog.Api.csproj",             "src/ProductCatalog.Api/"]
COPY ["src/ProductCatalog.Application/ProductCatalog.Application.csproj", "src/ProductCatalog.Application/"]
COPY ["src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj", "src/ProductCatalog.Infrastructure/"]
COPY ["src/ProductCatalog.Domain/ProductCatalog.Domain.csproj",       "src/ProductCatalog.Domain/"]

RUN dotnet restore "src/ProductCatalog.Api/ProductCatalog.Api.csproj"

# Copy the rest of the source
COPY . .

# Publish release build
RUN dotnet publish "src/ProductCatalog.Api/ProductCatalog.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

# Copy published output from build stage
COPY --from=build /app/publish .

# Container Apps / ECS expect port 8080 by default
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProductCatalog.Api.dll"]
