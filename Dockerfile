# =================================================================
# Multi-stage Dockerfile for ASP.NET Core Clean Architecture CQRS Template
# =================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (for better layer caching)
COPY ["src/Template.API/Template.API.csproj", "src/Template.API/"]
COPY ["src/Template.Application/Template.Application.csproj", "src/Template.Application/"]
COPY ["src/Template.Domain/Template.Domain.csproj", "src/Template.Domain/"]
COPY ["src/Template.Infrastructure/Template.Infrastructure.csproj", "src/Template.Infrastructure/"]
COPY ["src/Template.Persistence/Template.Persistence.csproj", "src/Template.Persistence/"]
COPY ["aspnetcore-clean-cqrs-template.sln", "./"]

# Restore NuGet packages
RUN dotnet restore "aspnetcore-clean-cqrs-template.sln"

# Copy source code
COPY . .

# Build and publish the application
WORKDIR "/src/src/Template.API"
RUN dotnet build "Template.API.csproj" -c Release -o /app/build --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish "Template.API.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Install necessary dependencies for health checks and PostgreSQL client
RUN apt-get update && apt-get install -y \
    curl \
    postgresql-client \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN addgroup --gid 1001 --system appgroup && \
    adduser --uid 1001 --system --gid 1001 appuser

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Copy wait scripts
COPY scripts/wait-for-postgres.sh ./scripts/
COPY scripts/docker-entrypoint.sh ./scripts/

# Make scripts executable
RUN chmod +x ./scripts/wait-for-postgres.sh
RUN chmod +x ./scripts/docker-entrypoint.sh

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Create logs directory
RUN mkdir -p /app/Logs

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Entry point with wait script
ENTRYPOINT ["./scripts/docker-entrypoint.sh"] 