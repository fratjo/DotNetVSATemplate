using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSATemplate.Common;
using VSATemplate.Data;

namespace VSATemplate.Features.Products;

// Command (Request)
public record DeleteProductCommand(int Id) : ICommand;

// Validator
public class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");
    }
}

// Handler demonstrating Maybe Monad
public class DeleteProductHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly AppDbContext _context;
    private readonly IValidator<DeleteProductCommand> _validator;

    public DeleteProductHandler(AppDbContext context, IValidator<DeleteProductCommand> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result> HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        // Using Maybe Monad to handle potentially null product
        var maybeProduct = Maybe<Product>.From(
            await _context.Products.FindAsync(new object[] { command.Id }, cancellationToken)
        );

        // Pattern matching with Maybe
        return maybeProduct.Match(
            some: product =>
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                return Result.Success();
            },
            none: () => Result.Failure($"Product with ID {command.Id} not found")
        );
    }
}

// Endpoint
public static class DeleteProductEndpoint
{
    public static void MapDeleteProduct(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id}", async (
            int id,
            DeleteProductHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteProductCommand(id);
            var result = await handler.HandleAsync(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { message = "Product deleted successfully" })
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("DeleteProduct")
        .WithTags("Products");
    }
}
