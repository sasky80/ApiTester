using System.Threading.Tasks;
using ApiTester.Models;

namespace ApiTester.Services
{
    public interface IConfigurationService
    {
        Task SaveAsync(ConfigurationData dataToSave);
        Task<ConfigurationData> LoadAsync();
    }
}