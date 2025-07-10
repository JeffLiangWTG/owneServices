using System.ComponentModel;

namespace CargoWise.NetworkVisualisation.GUI.Services.Events;

public class PropertyValueChangedEventArgs : PropertyChangedEventArgs
{
	public PropertyValueChangedEventArgs(string propertyName, object value) : base(propertyName)
	{
		Value = value;
	}

	public object Value { get; }
}
