using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace VisorDTE.Services
{
    public class CatalogService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _catalogs = new();
        private bool _isInitialized = false;

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var catalogPath = Path.Combine(dir, "Catalogs");

            if (!Directory.Exists(catalogPath)) return;

            foreach (var filePath in Directory.GetFiles(catalogPath, "*.ini"))
            {
                var catalogName = Path.GetFileNameWithoutExtension(filePath);
                var catalogData = new Dictionary<string, string>();
                var lines = await File.ReadAllLinesAsync(filePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("[")) continue;
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        catalogData[parts[0].Trim()] = parts[1].Trim();
                    }
                }
                _catalogs[catalogName] = catalogData;
            }
            _isInitialized = true;
        }

        public string GetDescription(string catalogName, string code)
        {
            if (string.IsNullOrEmpty(code)) return "N/A";

            if (_catalogs.TryGetValue(catalogName, out var catalog) && catalog.TryGetValue(code, out var description))
            {
                return $"{description}";
            }

            return code;
        }
    }
}