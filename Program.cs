using MySqlConnector;
using System.Text.Json;
using System.Text.Json.Serialization;
using Services;

var builder = WebApplication.CreateBuilder(args);

// CORS ayarları
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JSON serialization ayarları (camelCase)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Servisleri kaydet
builder.Services.AddControllers();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<RakipService>();
builder.Services.AddScoped<KategoriService>();
builder.Services.AddScoped<SatisService>();
builder.Services.AddScoped<StokService>();
builder.Services.AddScoped<TedarikciService>();
builder.Services.AddScoped<IadeService>();
builder.Services.AddScoped<MusteriService>();

var app = builder.Build();

// Middleware sırası önemli
app.UseCors();

// Önce API route'larını map et
app.MapControllers();

// Sonra static dosyaları serve et
app.UseStaticFiles();

// En son fallback - index.html'i serve et
app.MapFallbackToFile("index.html");

app.Run();

