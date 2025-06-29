#!/bin/bash
# =================================================================
# Docker Entrypoint Script for ASP.NET Core Clean Architecture Template
# Handles PostgreSQL readiness and application startup
# =================================================================

set -e

echo "🚀 ASP.NET Core Clean Architecture CQRS Template başlatılıyor..."
echo "📅 $(date)"
echo "🌍 Ortam: $ASPNETCORE_ENVIRONMENT"
echo "🔧 .NET Version: $(dotnet --version)"

# Wait for PostgreSQL to be ready
echo "🔄 Ana veritabanı bekleniyor..."
./scripts/wait-for-postgres.sh "postgres" "5432"

echo "🔄 Log veritabanı bekleniyor..."
./scripts/wait-for-postgres.sh "postgres-logs" "5432"

echo "✅ Tüm veritabanları hazır!"

# Run any additional initialization if needed
if [ "$ASPNETCORE_ENVIRONMENT" = "Development" ]; then
    echo "🛠️ Development ortamı: Ek geliştirme ayarları uygulanıyor..."
    # Development-specific commands could go here
fi

# Start the application
echo "🚀 Template.API başlatılıyor..."
echo "🌐 URL: $ASPNETCORE_URLS"
echo "📊 Health Check: http://localhost:8080/health"
echo "📖 Swagger UI: http://localhost:8080/swagger"
echo "============================================"

# Execute the .NET application
exec dotnet Template.API.dll 