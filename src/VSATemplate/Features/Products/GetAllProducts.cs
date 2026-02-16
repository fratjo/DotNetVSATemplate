using Dapper;
using Microsoft.Data.Sqlite;
using VSATemplate.Common;

namespace VSATemplate.Features.Products;

// Query (Request)
public record GetAllProductsQuery : IQuery<List<ProductListItem>>;

// Response
public class ProductListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

// Handler using Dapper for read operations
public class GetAllProductsHandler : IQueryHandler<GetAllProductsQuery, List<ProductListItem>>
{
    private readonly string _connectionString;

    public GetAllProductsHandler(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<Result<List<ProductListItem>>> HandleAsync(GetAllProductsQuery query, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        var sql = @"
            SELECT Id, Name, Price, StockQuantity
            FROM Products
            ORDER BY CreatedAt DESC";

        var products = await connection.QueryAsync<ProductListItem>(sql);

        return Result.Success(products.ToList());
    }
}

// Endpoint
public static class GetAllProductsEndpoint
{
    public static void MapGetAllProducts(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async (
            GetAllProductsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllProductsQuery();
            var result = await handler.HandleAsync(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAllProducts")
        .WithTags("Products");
    }
}
