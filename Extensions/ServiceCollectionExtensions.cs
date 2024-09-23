using ApiTester.Services;
using ApiTester.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTester.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection collection)
        {
            collection.AddSingleton<IPersistenceService, PersistenceService>();
            collection.AddTransient<MainWindowViewModel>();
        }
    }
}