using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ParlourPro.Services;
using ParlourPro.ViewModels;
using ParlourPro.Views;

namespace ParlourPro
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // Ye line add karein
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Service
            builder.Services.AddSingleton<IParlourService, ParlourService>();

            // Register ViewModel & Page
            builder.Services.AddTransient<ServiceBillingViewModel>();
            builder.Services.AddTransient<ServiceBillingPage>();
            builder.Services.AddSingleton<FirebaseService>();
            builder.Services.AddTransient<ServiceMasterViewModel>();
            builder.Services.AddTransient<ServiceMasterPage>();

            builder.Services.AddSingleton<DashboardPage>();
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddSingleton<DatabaseService>();

            builder.Services.AddTransient<BillDetailsPage>();
            builder.Services.AddTransient<BillDetailsViewModel>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<HistoryViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
