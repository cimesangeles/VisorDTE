// /Views/PurchaseAddonsView.xaml.cs
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using VisorDTE.Models;

namespace VisorDTE.Views
{
    public sealed partial class PurchaseAddonsView : UserControl
    {
        // Esta propiedad se llenará desde el MainViewModel
        public List<AddonViewModel> AvailableAddons { get; set; }

        public PurchaseAddonsView()
        {
            this.InitializeComponent();
        }
    }
}