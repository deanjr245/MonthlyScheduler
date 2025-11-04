using Microsoft.Extensions.DependencyInjection;
using MonthlyScheduler.Data;

namespace MonthlyScheduler;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        // Configure dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        
        // Run the application with DI
        var mainForm = ServiceProvider.GetRequiredService<Form1>();
        Application.Run(mainForm);
        
        // Clean up
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        // Register DbContext as singleton
        services.AddSingleton<SchedulerDbContext>();
        
        // Register forms
        services.AddTransient<Form1>();
    }
}