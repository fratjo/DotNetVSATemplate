# DotNetVSATemplate
Vertical Slice Architecture ASP.NET Template

## 🏗️ Architecture Overview

This template demonstrates a **Vertical Slice Architecture** implementation in ASP.NET, providing an alternative to traditional layered architectures. Each feature is self-contained in a single file with all its concerns.

## ✨ Key Features

- **CQRS without MediatR** - Commands and Queries separated using custom interfaces
- **Fluent Validation** - Request validation integrated at the handler level
- **EF Core** - For write operations (Commands)
- **Dapper** - For read operations (Queries) - optimized performance
- **Result Pattern** - Explicit success/failure handling without exceptions
- **Maybe Monad** - Safe null handling with functional patterns
- **One Class Per Feature** - Each feature contains:
  - Endpoint definition
  - Handler (business logic)
  - Request/Response models
  - Validation rules

## 📁 Project Structure

```
VSATemplate/
├── Common/
│   ├── Result.cs          # Result pattern implementation
│   ├── Maybe.cs           # Maybe monad implementation
│   └── Interfaces.cs      # CQRS interfaces (ICommand, IQuery, handlers)
├── Data/
│   ├── AppDbContext.cs    # EF Core database context
│   └── Product.cs         # Domain entity
├── Features/
│   └── Products/
│       ├── CreateProduct.cs    # Command: Create product
│       ├── GetProduct.cs       # Query: Get single product (Dapper)
│       ├── GetAllProducts.cs   # Query: Get all products (Dapper)
│       ├── UpdateProduct.cs    # Command: Update product
│       └── DeleteProduct.cs    # Command: Delete product (Maybe monad demo)
└── Program.cs             # Application startup
```

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later

### Running the Application

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project src/VSATemplate
```

The API will be available at `http://localhost:5206`

### API Endpoints

#### Create Product
```http
POST /api/products
Content-Type: application/json

{
  "name": "Product Name",
  "description": "Product Description",
  "price": 29.99,
  "stockQuantity": 100
}
```

#### Get All Products
```http
GET /api/products
```

#### Get Product by ID
```http
GET /api/products/{id}
```

#### Update Product
```http
PUT /api/products/{id}
Content-Type: application/json

{
  "name": "Updated Name",
  "description": "Updated Description",
  "price": 39.99,
  "stockQuantity": 150
}
```

#### Delete Product
```http
DELETE /api/products/{id}
```

## 🎯 Vertical Slice Architecture

### What is Vertical Slice Architecture?

Instead of organizing code by technical layers (Controllers, Services, Repositories), code is organized by features or use cases. Each feature is a "vertical slice" through all layers of the application.

### Benefits

- **High Cohesion** - Related code stays together
- **Low Coupling** - Features are independent
- **Easy to Navigate** - Everything for a feature is in one place
- **Simple to Test** - Each feature can be tested in isolation
- **Minimal Abstraction** - No unnecessary interfaces or layers
- **Easy to Modify** - Changes to one feature don't affect others

### Example: CreateProduct Feature

```csharp
// Single file contains everything for creating a product:
// - Request model (CreateProductCommand)
// - Response model (CreateProductResponse)
// - Validator (CreateProductValidator)
// - Handler (CreateProductHandler) with business logic
// - Endpoint definition (CreateProductEndpoint)
```

## 🔧 Key Patterns Explained

### CQRS Without MediatR

Commands and queries are separated using simple interfaces:

```csharp
// Commands (write operations)
public interface ICommand { }
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

// Queries (read operations)
public interface IQuery<TResponse> { }
public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
```

No external dependencies like MediatR required. Handlers are registered directly in DI container.

### Result Pattern

Explicit error handling without exceptions:

```csharp
var result = await handler.HandleAsync(command);
if (result.IsSuccess)
{
    // Handle success
}
else
{
    // Handle failure with result.Error
}
```

### Maybe Monad

Safe null handling with functional patterns:

```csharp
var maybeProduct = Maybe<Product>.From(await _context.Products.FindAsync(id));

return maybeProduct.Match(
    some: product => Result.Success(),
    none: () => Result.Failure("Product not found")
);
```

### EF Core + Dapper

- **EF Core** for write operations (Commands) - leverages change tracking and migrations
- **Dapper** for read operations (Queries) - optimized performance for reads

## 📝 Adding a New Feature

1. Create a new file in `Features/<FeatureName>/`
2. Define your Command/Query
3. Define Response model
4. Create Validator using FluentValidation
5. Implement Handler
6. Define Endpoint
7. Register handler in `Program.cs`
8. Map endpoint in `Program.cs`

Example:

```csharp
// 1. Command
public record MyCommand(string Data) : ICommand;

// 2. Response (if needed)
public record MyResponse(int Id, string Data);

// 3. Validator
public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.Data).NotEmpty();
    }
}

// 4. Handler
public class MyCommandHandler : ICommandHandler<MyCommand>
{
    public async Task<Result> HandleAsync(MyCommand command, CancellationToken ct)
    {
        // Business logic here
        return Result.Success();
    }
}

// 5. Endpoint
public static class MyEndpoint
{
    public static void MapMyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/my-endpoint", async (MyCommand command, MyCommandHandler handler) =>
        {
            var result = await handler.HandleAsync(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}
```

## 🧪 Testing

The architecture makes testing straightforward:

- Test handlers in isolation
- Mock only what you need (DbContext for commands, connection string for queries)
- No need to mock multiple layers

## 📚 Further Reading

- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Result Pattern](https://enterprisecraftsmanship.com/posts/error-handling-exception-or-result/)
- [Maybe Monad](https://www.pluralsight.com/resources/blog/guides/maybe-monad-through-csharp)

## 📄 License

This template is provided as-is for educational and commercial use.

