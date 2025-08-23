// /Views/DteView.xaml.cs
using Microsoft.UI.Xaml.Controls;

namespace VisorDTE.Views;

public sealed partial class DteView : UserControl
{
    // Ya no necesitamos la propiedad ViewModel ni el evento DataContextChanged
    public DteView()
    {
        this.InitializeComponent();
    }
}