# ==============================================================================
# Dockerfile: SACD Core Engine & Verification Worker
# Base: Multi-stage build for .NET 8.0 Linux Container (Debian Chiseled/Alpine)
# ==============================================================================

# STAGE 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy Solution & Projects for efficient caching
COPY SACD.sln ./
COPY src/SACD.Domain/SACD.Domain.csproj src/SACD.Domain/
COPY src/SACD.Application/SACD.Application.csproj src/SACD.Application/
COPY src/SACD.Infrastructure/SACD.Infrastructure.csproj src/SACD.Infrastructure/
COPY src/SACD.Cli/SACD.Cli.csproj src/SACD.Cli/

# Restore dependencies
RUN dotnet restore src/SACD.Cli/SACD.Cli.csproj

# Copy source code and build
COPY src/SACD.Domain/ src/SACD.Domain/
COPY src/SACD.Application/ src/SACD.Application/
COPY src/SACD.Infrastructure/ src/SACD.Infrastructure/
COPY src/SACD.Cli/ src/SACD.Cli/

WORKDIR /app/src/SACD.Cli
RUN dotnet publish -c Release -o /out

# STAGE 2: Runtime Container
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# Setup local storage volume for air-gap data
ENV SACD_WORKSPACE=/data/sacd_workspace
ENV DOTNET_EnableDiagnostics=0

VOLUME ["/data"]

COPY --from=build /out ./

# Healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
  CMD dotnet /app/SACD.Cli.dll /data/sacd_workspace --diag || exit 1

ENTRYPOINT ["dotnet", "SACD.Cli.dll"]
CMD ["/data/sacd_workspace"]
