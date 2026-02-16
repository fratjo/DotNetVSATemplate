using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSATemplate.Common;
using VSATemplate.Data;

namespace VSATemplate.Features.Products;

// Command (Request)
public record UpdateProductCommand(int Id, string Name, string Description, decimal Price, int StockQuantity) : ICommand;

// Validator
public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");

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
public class UpdateProductHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly AppDbContext _context;
    private readonly IValidator<UpdateProductCommand> _validator;

    public UpdateProductHandler(AppDbContext context, IValidator<UpdateProductCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        var product = await _context.Products.FindAsync(new object[] { command.Id }, cancellationToken);

        if (product == null)
        {
            return Result.Failure($"Product with ID {command.Id} not found");
        }

        product.Name = command.Name;
        product.Description = command.Description;
        product.Price = command.Price;
        product.StockQuantity = command.StockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

// Endpoint
public static class UpdateProductEndpoint
{
    public static void MapUpdateProduct(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products/{id}", async (
            int id,
            UpdateProductCommand command,
            UpdateProductHandler handler,
            CancellationToken cancellationToken) =>
        {
            // Ensure the ID from the route matches the command
            var updatedCommand = command with { Id = id };
            var result = await handler.HandleAsync(updatedCommand, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { message = "Product updated successfully" })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpdateProduct")
        .WithTags("Products");
    }
}
