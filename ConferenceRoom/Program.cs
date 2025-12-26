using ConferenceRoom.UI;
using ConferenceRoom.Application;
using ConferenceRoom.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var configuration = builder.Build();

var services = new ServiceCollection();

// === DB ===
services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// === SERVICES ===
services.AddScoped<IBookingService, BookingService>();
services.AddScoped<MainMenu>();
services.AddScoped<BookingCommands>();
services.AddScoped<InputReader>();
services.AddScoped<OutputWriter>();

// === BUILDER CONTAINER ===
var serviceProvider = services.BuildServiceProvider();

var menu = serviceProvider.GetRequiredService<MainMenu>();
await menu.RunAsync();