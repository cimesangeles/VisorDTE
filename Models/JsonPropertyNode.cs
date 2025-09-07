using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace VisorDTE.Models;

public partial class JsonPropertyNode : ObservableObject
{
    public string PropertyName { get; set; }
    public string Value { get; set; }
    public ObservableCollection<JsonPropertyNode> Children { get; } = new();

    // --- PROPIEDAD FALTANTE AÑADIDA AQUÍ ---
    // Esta propiedad controla si el nodo del árbol está expandido o colapsado.
    [ObservableProperty]
    private bool _isExpanded = true;
}