using System.Collections.Generic;
using System.Linq;

namespace CargoWise.NetworkVisualisation.GUI;

/// <summary>
/// Provides network ribbon resources.
/// </summary>
public class NetworkRibbonResourcesProvider
{
	readonly Dictionary<string, string> resources = new Dictionary<string, string>();

	/// <summary>
	/// Checks if there are any resources.
	/// </summary>
	/// <returns>True if there are any resources, false otherwise.</returns>
	public bool HasResources() => resources.Any();

	/// <summary>
	/// Gets the resource associated with the given resource name.
	/// </summary>
	/// <param name="resourceName">The name of the resource.</param>
	/// <returns>The resource if it exists, null otherwise.</returns>
	public string GetResource(string resourceName)
	{
		if (string.IsNullOrEmpty(resourceName) || !resources.ContainsKey(resourceName))
		{
			return null;
		}

		return resources[resourceName];
	}

	/// <summary>
	/// Adds the given resources to the existing resources.
	/// </summary>
	/// <param name="resources">The resources to be added.</param>
	public void AddRibbonResources(Dictionary<string, string> resources)
	{
		foreach (var item in resources)
		{
			this.resources.Add(item.Key, item.Value);
		}
	}
}
