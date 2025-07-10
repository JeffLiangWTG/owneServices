using System;
using CargoWise.NetworkVisualisation.GUI.Services.Events;

namespace CargoWise.NetworkVisualisation.GUI.Services.Connection;

public interface IConnectionService : IDisposable
{
	event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

	ConnectionData GetConnectionModelData();

	Guid GetSourceConnectorPK();
	Guid GetTargetConnectorPK();
}
