using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSATemplate.Data;
using VSATemplate.Features.Products;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=app.db";
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register handlers for Dependency Injection
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<GetProductHandler>();
builder.Services.AddScoped<GetAllProductsHandler>();
builder.Services.AddScoped<UpdateProductHandler>();
builder.Services.AddScoped<DeleteProductHandler>();

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map all product endpoints (Vertical Slices)
app.MapCreateProduct();
app.MapGetProduct();
app.MapGetAllProducts();
app.MapUpdateProduct();
app.MapDeleteProduct();

app.Run();
