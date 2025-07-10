#nullable enable
using System.ComponentModel;

namespace CargoWise.NetworkVisualisation.GUI.Extensions;

/// <summary>
/// Extension class for handling property change event for <see cref="RibbonButton"/>
/// </summary>
public static class INotifyPropertyChangedExtensions
{
	public static void AddPropertyChangedHandler(this INotifyPropertyChanged item, PropertyChangedEventHandler handler)
	{
		item.PropertyChanged += handler;
	}

	public static void RemovePropertyChangedHandler(this INotifyPropertyChanged item, PropertyChangedEventHandler handler)
	{
		item.PropertyChanged -= handler;
	}
}
