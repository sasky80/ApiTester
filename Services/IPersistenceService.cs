using System.Threading.Tasks;
using ApiTester.Models;

namespace ApiTester.Services
{

    //Persistence Service
    public interface IPersistenceService
    {
        Task SaveAsync(HttpRequestPersistentDataModel dataToSave);
        Task<HttpRequestPersistentDataModel> LoadAsync();
    }
}