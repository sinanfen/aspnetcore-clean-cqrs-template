# =================================================================
# Makefile for ASP.NET Core Clean Architecture CQRS Template
# Geliştirici dostu komutlar
# =================================================================

.PHONY: help build run test clean docker-build docker-run docker-dev docker-down migration seed

# Default target
help: ## Bu yardım mesajını gösterir
	@echo "🚀 ASP.NET Core Clean Architecture CQRS Template"
	@echo "================================================="
	@echo "Kullanılabilir komutlar:"
	@echo ""
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@echo ""

# Development Commands
restore: ## NuGet paketlerini geri yükler
	@echo "📦 NuGet paketleri geri yükleniyor..."
	dotnet restore aspnetcore-clean-cqrs-template.sln

build: restore ## Projeyi derler
	@echo "🔨 Proje derleniyor..."
	dotnet build aspnetcore-clean-cqrs-template.sln --configuration Release --no-restore

clean: ## Build artifactlarını temizler
	@echo "🧹 Build artifactları temizleniyor..."
	dotnet clean aspnetcore-clean-cqrs-template.sln
	find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
	find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

run: ## API'yi Development modunda çalıştırır
	@echo "🚀 API Development modunda başlatılıyor..."
	cd src/Template.API && dotnet run --environment Development

watch: ## API'yi hot-reload ile çalıştırır
	@echo "👀 API hot-reload ile başlatılıyor..."
	cd src/Template.API && dotnet watch run --environment Development

test: ## Tüm testleri çalıştırır
	@echo "🧪 Testler çalıştırılıyor..."
	dotnet test aspnetcore-clean-cqrs-template.sln --configuration Release --logger trx --results-directory TestResults

test-coverage: ## Test coverage raporu oluşturur
	@echo "📊 Test coverage raporu oluşturuluyor..."
	dotnet test aspnetcore-clean-cqrs-template.sln --collect:"XPlat Code Coverage" --results-directory TestResults

# Database Commands
migration-add: ## Yeni migration ekler (name=MigrationName)
	@echo "📊 Yeni migration ekleniyor: $(name)"
	cd src/Template.Persistence && dotnet ef migrations add $(name) --startup-project ../Template.API

migration-remove: ## Son migration'ı kaldırır
	@echo "❌ Son migration kaldırılıyor..."
	cd src/Template.Persistence && dotnet ef migrations remove --startup-project ../Template.API

migration-update: ## Database'i son migration'a günceller
	@echo "🔄 Database migration uygulanıyor..."
	cd src/Template.Persistence && dotnet ef database update --startup-project ../Template.API

migration-script: ## SQL migration script'i oluşturur
	@echo "📝 Migration script oluşturuluyor..."
	cd src/Template.Persistence && dotnet ef migrations script --startup-project ../Template.API --output migration-script.sql

database-drop: ## Database'i siler
	@echo "💥 Database siliniyor..."
	cd src/Template.Persistence && dotnet ef database drop --startup-project ../Template.API --force

# Docker Commands - Production
docker-build: ## Production Docker image'ını build eder
	@echo "🐳 Production Docker image build ediliyor..."
	docker build -t template-api:latest .

docker-run: docker-build ## Production container'ını çalıştırır
	@echo "🚀 Production container başlatılıyor..."
	docker-compose up -d

docker-logs: ## Production container loglarını gösterir
	@echo "📋 Container logları:"
	docker-compose logs -f api

docker-down: ## Production container'ları durdurur
	@echo "🛑 Production container'lar durduruluyor..."
	docker-compose down

docker-clean: ## Tüm Docker resource'larını temizler
	@echo "🧹 Docker resource'ları temizleniyor..."
	docker-compose down -v --remove-orphans
	docker system prune -f

# Docker Commands - Development
docker-dev: ## Development environment'ı başlatır
	@echo "🛠️ Development environment başlatılıyor..."
	docker-compose -f docker-compose.dev.yml up -d

docker-dev-build: ## Development image'ını rebuild eder
	@echo "🔨 Development image rebuild ediliyor..."
	docker-compose -f docker-compose.dev.yml up -d --build

docker-dev-logs: ## Development container loglarını gösterir
	@echo "📋 Development container logları:"
	docker-compose -f docker-compose.dev.yml logs -f api-dev

docker-dev-down: ## Development container'ları durdurur
	@echo "🛑 Development container'lar durduruluyor..."
	docker-compose -f docker-compose.dev.yml down

docker-dev-clean: ## Development Docker resource'larını temizler
	@echo "🧹 Development Docker resource'ları temizleniyor..."
	docker-compose -f docker-compose.dev.yml down -v --remove-orphans

# Database Tools
pgadmin: ## pgAdmin'i başlatır (Development)
	@echo "🔧 pgAdmin başlatılıyor..."
	docker-compose -f docker-compose.dev.yml up -d pgadmin-dev
	@echo "🌐 pgAdmin: http://localhost:5050 (admin@template.com / admin)"

redis: ## Redis cache'i başlatır (Development)
	@echo "💾 Redis başlatılıyor..."
	docker-compose -f docker-compose.dev.yml --profile cache up -d redis-dev

# Project Setup
setup: ## Projeyi ilk kez kurar
	@echo "🚀 Proje ilk kez kuruluyor..."
	$(MAKE) restore
	$(MAKE) build
	@echo "✅ Proje kurulumu tamamlandı!"
	@echo ""
	@echo "🎯 Sıradaki adımlar:"
	@echo "1. make docker-dev     # Development environment'ı başlat"
	@echo "2. make run            # API'yi local olarak çalıştır"
	@echo "3. http://localhost:8080/swagger  # Swagger UI'ı aç"

# Health Checks
health: ## Servislerin sağlık durumunu kontrol eder
	@echo "🏥 Servis sağlık kontrolleri..."
	@curl -f http://localhost:8080/health || echo "❌ API erişilebilir değil"
	@docker ps --filter "name=template" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" || echo "❌ Docker container'lar çalışmıyor"

# Development Utilities
format: ## Kodu formatlar
	@echo "✨ Kod formatlanıyor..."
	dotnet format aspnetcore-clean-cqrs-template.sln

lint: ## Code analysis yapar
	@echo "🔍 Code analysis yapılıyor..."
	dotnet build aspnetcore-clean-cqrs-template.sln --verbosity normal

security-scan: ## Güvenlik taraması yapar
	@echo "🔒 Güvenlik taraması yapılıyor..."
	dotnet list package --vulnerable --include-transitive

update-packages: ## NuGet paketlerini günceller
	@echo "📦 NuGet paketleri güncelleniyor..."
	dotnet outdated -u

# Project Information
info: ## Proje bilgilerini gösterir
	@echo "ℹ️ Proje Bilgileri:"
	@echo "==================="
	@echo "🏗️ .NET Version: $(shell dotnet --version)"
	@echo "📁 Solution: aspnetcore-clean-cqrs-template.sln"
	@echo "🐳 Docker: $(shell docker --version 2>/dev/null || echo 'Docker kurulu değil')"
	@echo "🗄️ PostgreSQL Port: 5432"
	@echo "📊 pgAdmin Port: 5050"
	@echo "🌐 API Port: 8080"
	@echo "📖 Swagger: http://localhost:8080/swagger"
	@echo "🏥 Health Check: http://localhost:8080/health" 