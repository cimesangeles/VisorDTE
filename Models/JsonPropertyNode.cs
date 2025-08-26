// /Models/JsonPropertyNode.cs
using CommunityToolkit.Mvvm.Input; // <--- Añade este using
using System.Collections.ObjectModel;

namespace VisorDTE.Models
{
    public class JsonPropertyNode
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
        public ObservableCollection<JsonPropertyNode> Children { get; set; } = new ObservableCollection<JsonPropertyNode>();

        // Añade esta propiedad para sostener el comando
        public IRelayCommand CopyCommand { get; set; }
    }
}