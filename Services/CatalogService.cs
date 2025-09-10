// /Services/CatalogService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
// --- INICIO DE LA MODIFICACIÓN 1: Añadir este using ---
using Windows.ApplicationModel;

namespace VisorDTE.Services
{
    public class CatalogService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _catalogs = new();
        private bool _isInitialized = false;

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            // --- INICIO DE LA MODIFICACIÓN 2: Usar la ruta de instalación del paquete ---
            var installDir = Package.Current.InstalledLocation.Path;
            var catalogPath = Path.Combine(installDir, "Catalogs");
            // --- FIN DE LA MODIFICACIÓN 2 ---

            if (!Directory.Exists(catalogPath))
            {
                // Agregamos un log para saber si no encuentra la carpeta
                System.Diagnostics.Debug.WriteLine($"Error: La carpeta de catálogos no fue encontrada en '{catalogPath}'");
                return;
            }

            foreach (var filePath in Directory.GetFiles(catalogPath, "*.ini"))
            {
                try
                {
                    var catalogName = Path.GetFileNameWithoutExtension(filePath);
                    var catalogData = new Dictionary<string, string>();
                    // Usamos UTF-8 explícitamente para asegurar la correcta lectura de tildes
                    var lines = await File.ReadAllLinesAsync(filePath, System.Text.Encoding.UTF8);

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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al cargar el catálogo {filePath}: {ex.Message}");
                }
            }
            _isInitialized = true;
        }

        public string GetDescription(string catalogName, string code)
        {
            if (string.IsNullOrEmpty(code)) return "N/A";

            if (_catalogs.TryGetValue(catalogName, out var catalog) && catalog.TryGetValue(code, out var description))
            {
                return description;
            }

            return code;
        }
    }
}