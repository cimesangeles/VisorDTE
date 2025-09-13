// /Services/StoreLicensingService.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Store;
using WinRT.Interop;

namespace VisorDTE.Services;

public class StoreLicensingService
{
    private StoreContext _storeContext;

    public StoreLicensingService()
    {
        _storeContext = StoreContext.GetDefault();
    }

    public async Task<HashSet<string>> GetPurchasedAddonIdsAsync()
    {
        // --- MODIFICACIÓN ---
        // Se ha eliminado el bloque #if DEBUG.
        // Este código ahora se ejecutará tanto en modo Debug como en Release.

        var log = new StringBuilder();
        var purchasedAddons = new HashSet<string>();
        try
        {
            log.AppendLine("--- Registro de Verificación de Licencia ---");
            log.AppendLine($"Fecha/Hora: {DateTime.Now}");

            _storeContext = StoreContext.GetDefault();
            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(_storeContext, hwnd);
            log.AppendLine("Paso 1: StoreContext inicializado con la ventana principal.");

            string[] productKinds = { "Durable" };
            var filterList = new List<string>(productKinds);

            log.AppendLine("Paso 2: Realizando llamada a GetAssociatedStoreProductsAsync...");
            StoreProductQueryResult queryResult = await _storeContext.GetAssociatedStoreProductsAsync(filterList);

            if (queryResult.ExtendedError != null)
            {
                throw queryResult.ExtendedError;
            }
            log.AppendLine($"Paso 3: Llamada exitosa. Se encontraron {queryResult.Products.Count} productos en total.");

            int foundCount = 0;
            foreach (var product in queryResult.Products.Values)
            {
                if (product.IsInUserCollection)
                {
                    purchasedAddons.Add(product.StoreId);
                    log.AppendLine($"- ADDON COMPRADO DETECTADO: {product.Title} ({product.StoreId})");
                    foundCount++;
                }
            }
            log.AppendLine($"Paso 4: Verificación completa. Total de addons comprados encontrados: {foundCount}.");

            // Si después de todo, no se encontraron addons, muestra el registro.
            if (purchasedAddons.Count == 0 && queryResult.Products.Count > 0)
            {
                log.AppendLine("\nADVERTENCIA: Se encontraron productos en la tienda, pero ninguno figura como comprado en la colección del usuario.");
                await ShowLicenseErrorDialog("Registro de Verificación", log.ToString());
            }
        }
        catch (Exception ex)
        {
            string errorCode = $"0x{ex.HResult:X}";
            log.AppendLine($"\n[EXCEPCIÓN] Ocurrió un error durante el proceso.");
            log.AppendLine($"   Código: {errorCode}");
            log.AppendLine($"   Mensaje: {ex.Message}");
            await ShowLicenseErrorDialog("Error de Licencia", log.ToString());
        }

        return purchasedAddons;
    }

    private async Task ShowLicenseErrorDialog(string title, string content)
    {
        if (App.MainWindow?.Content?.XamlRoot != null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = new ScrollViewer { Content = new TextBlock { Text = content, TextWrapping = TextWrapping.Wrap } },
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}