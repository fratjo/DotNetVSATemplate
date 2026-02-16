using FluentValidation;
using VSATemplate.Common;
using VSATemplate.Data;

namespace VSATemplate.Features.Products;

// Command (Request)
public record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity) : ICommand;

// Response
public record CreateProductResponse(int Id, string Name, decimal Price);

// Validator
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be greater than or equal to 0");
    }
}

// Handler
public class CreateProductHandler : ICommandHandler<CreateProductCommand>
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductHandler(AppDbContext context, IValidator<CreateProductCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        var product = new Product
        {
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            StockQuantity = command.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

// Endpoint
public static class CreateProductEndpoint
{
    public static void MapCreateProduct(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", async (
            CreateProductCommand command,
            CreateProductHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/products", new { message = "Product created successfully" })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateProduct")
        .WithTags("Products");
    }
}
