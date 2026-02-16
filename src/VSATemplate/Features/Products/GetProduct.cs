using Dapper;
using FluentValidation;
using Microsoft.Data.Sqlite;
using VSATemplate.Common;

namespace VSATemplate.Features.Products;

// Query (Request)
public record GetProductQuery(int Id) : IQuery<GetProductResponse>;

// Response
public class GetProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Validator
public class GetProductValidator : AbstractValidator<GetProductQuery>
{
    public GetProductValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");
    }
}

// Handler using Dapper for read operations
public class GetProductHandler : IQueryHandler<GetProductQuery, GetProductResponse>
{
    private readonly string _connectionString;
    private readonly IValidator<GetProductQuery> _validator;

    public GetProductHandler(IConfiguration configuration, IValidator<GetProductQuery> validator)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _validator = validator;
    }

    public async Task<Result<GetProductResponse>> HandleAsync(GetProductQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<GetProductResponse>(errors);
        }

        using var connection = new SqliteConnection(_connectionString);
        var sql = @"
            SELECT Id, Name, Description, Price, StockQuantity, CreatedAt
            FROM Products
            WHERE Id = @Id";

        var product = await connection.QuerySingleOrDefaultAsync<GetProductResponse>(sql, new { query.Id });

        if (product == null)
        {
            return Result.Failure<GetProductResponse>($"Product with ID {query.Id} not found");
        }

        return Result.Success(product);
    }
}

// Endpoint
public static class GetProductEndpoint
{
    public static void MapGetProduct(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id}", async (
            int id,
            GetProductHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductQuery(id);
            var result = await handler.HandleAsync(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetProduct")
        .WithTags("Products");
    }
}
