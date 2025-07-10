namespace Enterprise.Services.Scim.Api.Helpers
{
	public interface IResourceTypeResolver
	{
		ResourceTypeResolutionResult ResolveByResourceType(string resourceType);
	}
}
