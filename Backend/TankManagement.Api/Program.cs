using Microsoft.EntityFrameworkCore;
using TankManagement.Api.Data;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Repositories;
using TankManagement.Api.Services;
using TankManagement.Api.Configuration;
using TankManagement.Api.Serial;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:3000");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No existe ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<TankManagementDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.Configure<ArduinoSerialOptions>(builder.Configuration.GetSection(ArduinoSerialOptions.SectionName));
builder.Services.AddSingleton<ArduinoSerialService>();
builder.Services.AddSingleton<IArduinoControlService>(sp => sp.GetRequiredService<ArduinoSerialService>());
builder.Services.AddHostedService<ArduinoSerialService>(sp => sp.GetRequiredService<ArduinoSerialService>());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<ITanqueRepository, TanqueRepository>();
builder.Services.AddScoped<IUsuarioTanqueRepository, UsuarioTanqueRepository>();
builder.Services.AddScoped<ISolicitudRepository, SolicitudRepository>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITanqueService, TanqueService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TankManagementDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.Run();
