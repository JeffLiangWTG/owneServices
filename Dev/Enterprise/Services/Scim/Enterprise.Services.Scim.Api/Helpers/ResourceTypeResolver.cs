using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Services.Scim.Api.Helpers
{
	public class ResourceTypeResolver : IResourceTypeResolver
	{
		readonly List<ResourceTypeResolutionResult> _resolutionResults;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Resolver strings")]
		public ResourceTypeResolver()
		{
			_resolutionResults = new List<ResourceTypeResolutionResult>
			{
				new ResourceTypeResolutionResult { ControllerName = "Users", ResourceType = "User" },
				new ResourceTypeResolutionResult { ControllerName = "Groups", ResourceType = "Group" }
			};
		}

		public ResourceTypeResolutionResult ResolveByResourceType(string resourceType)
		{
			return _resolutionResults.FirstOrDefault(r => r.ResourceType == resourceType);
		}
	}
}
