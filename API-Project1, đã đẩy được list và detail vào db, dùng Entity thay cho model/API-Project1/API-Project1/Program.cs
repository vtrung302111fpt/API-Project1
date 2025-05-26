using API_Project1;
using API_Project1.Interfaces;
using API_Project1.Services;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500") // hoặc "http://localhost:5500"
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<LoginConfig>(builder.Configuration.GetSection("Login"));
builder.Services.AddSingleton(resolver =>
{
    var config = resolver.GetRequiredService<IConfiguration>();
    var loginConfig = config.GetSection("Login").Get<LoginConfig>();
    return loginConfig;
});

//builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddHttpClient(); // Nếu chưa có
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserInfoService, UserInfoService>();
builder.Services.AddScoped<IInvoiceListService, InvoiceListService>();
builder.Services.AddScoped<IInvoiceDetailService, InvoiceDetailService>();
builder.Services.AddScoped<IGetDataService, GetDataService>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("BillStoreConnection")));
builder.Services.AddAutoMapper(typeof(MappingProfile));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
