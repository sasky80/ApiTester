using ApiTester.Models;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ApiTester.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public async Task SaveAsync(ConfigurationData dataToSave)
        {
            var storageProvider = new Window().StorageProvider;
            var saveFilePickerOptions = new FilePickerSaveOptions
            {
                DefaultExtension = "json",
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } }
                }
            };

            var file = await storageProvider.SaveFilePickerAsync(saveFilePickerOptions);

            if (file is not null)
            {
                string json = JsonConvert.SerializeObject(dataToSave, Formatting.Indented);

                await using var stream = await file.OpenWriteAsync();
                using var streamWriter = new StreamWriter(stream);

                await streamWriter.WriteAsync(json);
            }
        }

        public async Task<ConfigurationData?> LoadAsync()
        {
            var storageProvider = new Window().StorageProvider;
            var openFilePickerOptions = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } }
                }
            };

            var result = await storageProvider.OpenFilePickerAsync(openFilePickerOptions);

            if (result != null && result.Count > 0)
            {
                var file = result[0];
                var stream = await file.OpenReadAsync();
                using var streamReader = new StreamReader(stream);
                string json = await streamReader.ReadToEndAsync();

                return JsonConvert.DeserializeObject<ConfigurationData>(json);
            }

            return null;
        }
    }
}