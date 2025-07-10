using System;
using System.ComponentModel;
using System.Linq;

namespace CargoWise.NetworkVisualisation.GUI;

/// <summary>
/// Abstract base class implementing the <see cref="INotifyPropertyChanged"/> interface 
/// to support property change notifications.
/// </summary>
public abstract class ViewModelWithNotification : INotifyPropertyChanged
{
	#region Notification

	event PropertyChangedEventHandler propertyChanged;
	public event PropertyChangedEventHandler PropertyChanged
	{
		add
		{
			if (value != null)
			{
				if (propertyChanged == null || !propertyChanged.GetInvocationList().Select(i => i.Method).Contains(value.Method))
				{
					propertyChanged += value;
				}
			}
		}
		remove
		{
			propertyChanged -= value;
		}
	}

	/// <summary>
	/// Raises the <see cref="PropertyChanged"/> event for the specified property name.
	/// </summary>
	/// <param name="name">The name of the property that changed.</param>
	/// <exception cref="ArgumentException">Thrown when the property name is null or empty.</exception>
	protected void OnPropertyChanged(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException(name, nameof(name));
		}

		propertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	#endregion
}
