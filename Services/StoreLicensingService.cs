// /Services/StoreLicensingService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Services.Store;

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
        var purchasedAddons = new HashSet<string>();

        try
        {
            StoreAppLicense appLicense = await _storeContext.GetAppLicenseAsync();

            foreach (var addon in appLicense.AddOnLicenses)
            {
                StoreLicense license = addon.Value;
                if (license.IsActive)
                {
                    purchasedAddons.Add(license.SkuStoreId);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al consultar la tienda: {ex.Message}");
        }

        // --- INICIO DE LA MODIFICACIÓN ---
        // Para pruebas en modo DEBUG, simulamos que los Add-ons han sido comprados.
#if DEBUG
        purchasedAddons.Add("9N123ABCDEF1"); // Add-on para Crédito Fiscal
        purchasedAddons.Add("9N123ABCDEF2"); // Add-on para Nota de Crédito
        purchasedAddons.Add("9N123ABCDEF3"); // Add-on para Factura de Exportación
        System.Diagnostics.Debug.WriteLine("MODO DEBUG: Añadidos Add-ons de prueba para CCF, NC y FEX.");
#endif
        // --- FIN DE LA MODIFICACIÓN ---

        return purchasedAddons;
    }
}