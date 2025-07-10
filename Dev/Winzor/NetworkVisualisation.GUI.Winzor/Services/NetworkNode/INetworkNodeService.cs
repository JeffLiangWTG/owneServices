using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Services.Events;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;

public interface INetworkNodeService : IDisposable
{
	event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

	NetworkNodeData GetNetworkNodeData();

	string GetTooltipForEntityState(EntityState entityState);

	Task MoveAsync(double x, double y);
	Task ResizeAsync(double x, double y, double width, double height);
	Task ShowViewAsync();
	Task UpdateNotesAsync(string notes);
	Task UpdateIsSelectedAsync(bool selected);
	Task UpdateDiagramNameAsync(string diagramName);
	Task<IEnumerable<NetworkActionMenuItem>> GetContextMenuItemsAsync();
}
