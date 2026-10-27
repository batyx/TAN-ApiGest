using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace TanApiGest.Views.Components;

/// <summary>Composant réutilisable : une icône Material Design suivie d'un libellé.</summary>
public partial class IconAndTextMenuItem : UserControl
{
    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(nameof(IconKind), typeof(PackIconKind), typeof(IconAndTextMenuItem), new PropertyMetadata(default(PackIconKind)));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(IconAndTextMenuItem), new PropertyMetadata(default(string)));

    public PackIconKind IconKind
    {
        get => (PackIconKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public IconAndTextMenuItem()
    {
        InitializeComponent();
    }
}
