using System;
using CargoWise.NetworkVisualisation.GUI.Services.Events;

namespace CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;

public interface IProgressBarModelService : IDisposable
{
	event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

	ProgressBarModelData GetProgressBarModelData();
}
