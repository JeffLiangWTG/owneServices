using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Models;

namespace NetworkVisualisation.GUI.Winzor.Models.Factories;

/// <summary>
/// Interface defining methods for creating NetworkNodeModel instances.
/// </summary>
public interface INodeModelFactory
{
	// <summary>
	/// Creates a NetworkNodeModel instance from a NodeViewModel.
	/// </summary>
	/// <param name="node">The NodeViewModel to convert.</param>
	/// <returns>A NetworkNodeModel instance representing the provided NodeViewModel.</returns>
	NetworkNodeModel GetNodeModel(NodeViewModel node);

	/// <summary>
	/// Asynchronously creates a NetworkNodeModel instance from a NodeViewModel.
	/// </summary>
	/// <param name="node">The NodeViewModel to convert.</param>
	/// <returns>A Task that resolves to a NetworkNodeModel instance representing the provided NodeViewModel.</returns>
	Task<NetworkNodeModel> GetNodeModelAsync(NodeViewModel node);
}
