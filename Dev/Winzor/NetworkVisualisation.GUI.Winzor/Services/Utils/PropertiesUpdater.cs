using System;
using System.Collections.Generic;
using CargoWise.NetworkVisualisation.GUI.Services.Events;

namespace CargoWise.NetworkVisualisation.GUI.Services.Utils;

public class PropertiesUpdater
{
	readonly Dictionary<string, Func<object, bool>> propertiesMap = new();

	internal PropertiesUpdater Add(string propertyName, Func<object, bool> tryUpdateFunc)
	{
		propertiesMap.Add(propertyName, tryUpdateFunc);
		return this;
	}

	internal bool TryUpdate(PropertyValueChangedEventArgs e) => TryUpdate(e.PropertyName, e.Value);
	internal bool TryUpdate(string propertyName, object value)
	{
		if (propertiesMap.TryGetValue(propertyName, out var func))
		{
			return func(value);
		}
		return false;
	}
}
