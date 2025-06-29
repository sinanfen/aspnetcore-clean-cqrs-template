# 🚀 ASP.NET Core 9 Clean Architecture CQRS Template

A **production-ready**, **enterprise-grade** ASP.NET Core 9 template implementing **Clean Architecture** with **CQRS pattern**. Create professional applications in **30 seconds** with complete authentication, 2FA, and modern development practices.

## ⚡ Quick Start (30 seconds to production-ready app!)

```bash
# 1. Install template (one-time setup)
dotnet new install Sinan.AspNetCore.CleanArchitecture.CQRS.Template

# 2. Create your project (automatic renaming!)
dotnet new clean-cqrs-template -n MyAwesomeProject
cd MyAwesomeProject

# 3. Run immediately!
dotnet run --project src/MyAwesomeProject.API
```

**That's it!** You now have:
- ✅ `MyAwesomeProject.API` with full auth system
- ✅ `MyAwesomeProject.Application` with CQRS commands/queries  
- ✅ JWT authentication + 2FA ready
- ✅ Email confirmation system
- ✅ PostgreSQL database with seeded data
- ✅ Docker support
- ✅ Clean Architecture + CQRS pattern

## 🏗️ What You Get Instantly

### **Complete Project Structure** (auto-generated)
```
MyAwesomeProject/
├── src/
│   ├── MyAwesomeProject.API/          # 🌐 Web API with auth endpoints
│   ├── MyAwesomeProject.Application/   # 🎯 CQRS commands & queries
│   ├── MyAwesomeProject.Domain/       # 🏛️ Domain entities
│   ├── MyAwesomeProject.Infrastructure/ # 🔧 External services (email, etc.)
│   └── MyAwesomeProject.Persistence/  # 💾 Database context & migrations
└── tests/
    ├── MyAwesomeProject.Application.Tests/
    └── MyAwesomeProject.Infrastructure.Tests/
```

### **Ready-to-Use Authentication Endpoints**
```bash
POST /api/Auth/register          # ✅ User registration with email confirmation
POST /api/Auth/login            # ✅ Login with JWT tokens
POST /api/Auth/enable-2fa       # ✅ Google Authenticator 2FA
POST /api/Auth/complete-2fa-login # ✅ Complete 2FA login
POST /api/Auth/refresh-token    # ✅ JWT refresh tokens
POST /api/Auth/forgot-password  # ✅ Password reset via email
POST /api/Auth/confirm-email    # ✅ Email confirmation
```

### **Production Features Built-In**
- 🔐 **JWT Authentication** with refresh token rotation
- 🛡️ **Two-Factor Authentication** (Google Authenticator)
- 📧 **Real SMTP Email Service** (MailKit)
- 🏛️ **Clean Architecture** (proper dependency flow)
- 🎯 **CQRS Pattern** (MediatR implementation)
- 💾 **PostgreSQL** with Entity Framework Core
- 🐳 **Docker** ready with multi-stage builds
- 📊 **Serilog** structured logging
- ✅ **FluentValidation** for all requests
- 📚 **Swagger/OpenAPI** documentation
- 🧪 **Unit tests** structure ready

## 🎯 Installation Methods

### Method 1: NuGet Package (Coming Soon)
```bash
dotnet new install Sinan.AspNetCore.CleanArchitecture.CQRS.Template
dotnet new clean-cqrs-template -n YourProjectName
```

### Method 2: From Source (Current)
```bash
# Clone and install locally
git clone https://github.com/sinanfen/aspnetcore-clean-cqrs-template.git
cd aspnetcore-clean-cqrs-template
dotnet new install ./

# Create your project
dotnet new clean-cqrs-template -n YourProjectName
```

## 🔧 Configuration (2 minutes setup)

### 1. Database Connection
Update `src/YourProject.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=YourProjectDB;Username=postgres;Password=yourpassword"
  }
}
```

### 2. Email Configuration
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "%YOUR_EMAIL_PASSWORD%"
  }
}
```

### 3. Environment Variables
```bash
# Set your email app password
export YOUR_EMAIL_PASSWORD="your-gmail-app-password"
# Windows: $env:YOUR_EMAIL_PASSWORD = "your-gmail-app-password"
```

### 4. Initialize Database
```bash
# Install EF tools (if needed)
dotnet tool install --global dotnet-ef

# Apply migrations and seed data
dotnet ef database update --project src/YourProject.Persistence --startup-project src/YourProject.API
```

## 🚀 Run Your Application

```bash
# Development
dotnet run --project src/YourProject.API

# With Docker
docker-compose up -d

# Access your API
# Swagger: https://localhost:7176/swagger
# Health Check: https://localhost:7176/health
```

## 📖 Example: Complete User Registration Flow

```bash
# 1. Register a new user
curl -X POST "https://localhost:7176/api/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePass123!",
    "firstName": "John",
    "lastName": "Doe"
  }'

# 2. Check email for confirmation link and confirm

# 3. Login
curl -X POST "https://localhost:7176/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePass123!"
  }'

# 4. Enable 2FA (returns QR code)
curl -X POST "https://localhost:7176/api/Auth/enable-2fa" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# 5. Complete 2FA login
curl -X POST "https://localhost:7176/api/Auth/complete-2fa-login" \
  -H "Content-Type: application/json" \
  -d '{"twoFactorCode": "123456"}'
```

## 🏗️ Architecture Overview

### Clean Architecture Layers
```
🌐 API Layer (Controllers, Middleware)
    ↓ depends on
🎯 Application Layer (CQRS, Business Logic)  
    ↓ depends on
🏛️ Domain Layer (Entities, Business Rules)
    ↑ depended by
🔧 Infrastructure Layer (Email, External APIs)
    ↑ depended by  
💾 Persistence Layer (Database, Repositories)
```

### CQRS Pattern Structure
```
Commands/
├── RegisterUser/
│   ├── RegisterUserCommand.cs
│   ├── RegisterUserCommandHandler.cs
│   └── RegisterUserCommandValidator.cs
└── Enable2FA/
    ├── Enable2FACommand.cs
    ├── Enable2FACommandHandler.cs
    └── Enable2FACommandValidator.cs

Queries/
└── LoginUser/
    ├── LoginUserQuery.cs
    ├── LoginUserQueryHandler.cs
    └── LoginUserQueryValidator.cs
```

## 🔥 Adding New Features (CQRS Style)

### 1. Add a New Entity
```csharp
// src/YourProject.Domain/Entities/Product.cs
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
```

### 2. Create CQRS Command
```csharp
// src/YourProject.Application/Features/Products/Commands/CreateProduct/
public record CreateProductCommand(string Name, decimal Price, string Description) 
    : IRequest<Result<Guid>>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
    }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

### 3. Add Controller
```csharp
// src/YourProject.API/Controllers/ProductController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
```

### 4. Update Database
```bash
dotnet ef migrations add AddProductEntity --project src/YourProject.Persistence
dotnet ef database update --project src/YourProject.Persistence
```

## 🐳 Docker Deployment

```bash
# Development with Docker Compose
docker-compose -f docker-compose.dev.yml up -d

# Production deployment
docker-compose up -d

# Build custom image
docker build -t yourproject-api .
docker run -p 8080:8080 yourproject-api
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific test project
dotnet test tests/YourProject.Application.Tests/
```

## 📦 Technologies & Dependencies

### Core Framework
- **ASP.NET Core 9.0** - Latest .NET web framework
- **Entity Framework Core 9.0** - Modern ORM
- **PostgreSQL** - Production database

### Architecture & Patterns  
- **MediatR** - CQRS implementation
- **FluentValidation** - Request validation
- **AutoMapper** - Object-to-object mapping

### Authentication & Security
- **ASP.NET Core Identity** - User management
- **JWT Bearer** - Token authentication  
- **Google Authenticator** - 2FA implementation

### Infrastructure
- **MailKit** - Professional email service
- **Serilog** - Structured logging
- **Swashbuckle** - OpenAPI/Swagger
- **Docker** - Containerization

## 🔒 Security Features

- ✅ **JWT Authentication** with RS256 signing
- ✅ **Refresh Token Rotation** for security
- ✅ **Two-Factor Authentication** (TOTP)
- ✅ **Email Confirmation** required
- ✅ **Password Reset** via secure tokens
- ✅ **Rate Limiting** protection
- ✅ **CORS** properly configured
- ✅ **Input Validation** on all endpoints
- ✅ **SQL Injection** protection
- ✅ **XSS Protection** headers

## 🌟 Why This Template?

### ❌ **Without This Template** (Traditional Approach)
- 📅 **2-3 weeks** to setup basic auth
- 🐛 Security vulnerabilities from scratch implementation
- 🔄 Reinventing the wheel for common patterns
- 📚 Learning curve for Clean Architecture + CQRS
- ⚙️ Complex configuration setup

### ✅ **With This Template** (Modern Approach)  
- ⚡ **30 seconds** to production-ready app
- 🔒 Enterprise-grade security built-in
- 🏗️ Proven architecture patterns
- 📖 Well-documented and tested
- 🚀 Focus on business logic, not infrastructure

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md).

```bash
# Fork the repo, then:
git clone https://github.com/yourusername/aspnetcore-clean-cqrs-template.git
cd aspnetcore-clean-cqrs-template

# Create feature branch
git checkout -b feature/amazing-feature

# Make changes and test
dotnet test
dotnet build

# Submit PR
git push origin feature/amazing-feature
```

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Robert C. Martin** - Clean Architecture principles
- **Greg Young** - CQRS pattern advocacy  
- **ASP.NET Core Team** - Amazing framework
- **Community Contributors** - Feedback and improvements

## ⭐ Support

If this template helps you build better applications, please:

1. ⭐ **Star this repository**
2. 🐦 **Share on Twitter** 
3. 💝 **Consider sponsoring** the project
4. 📝 **Write a blog post** about your experience

---

## 🚀 Ready to build your next enterprise application?

```bash
dotnet new install Sinan.AspNetCore.CleanArchitecture.CQRS.Template
dotnet new clean-cqrs-template -n MyNextBigProject
```

**Happy coding!** 🎉
