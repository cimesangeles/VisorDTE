// /Models/AddonViewModel.cs
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace VisorDTE.Models
{
    public class AddonViewModel
    {
        public string StoreId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public bool IsPurchased { get; set; }
        public IAsyncRelayCommand PurchaseCommand { get; set; }
    }
}