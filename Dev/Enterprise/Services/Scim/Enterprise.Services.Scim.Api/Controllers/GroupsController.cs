using Enterprise.Services.Scim.Api.Helpers;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Helpers;
using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Api.Controllers
{
	public class GroupsController : ScimController<ScimGroup>
	{
		public GroupsController(IPersistanceRepository<ScimGroup> groupRepository,
						IScimRepresentationConverter<ScimGroup> scimRepresentationConverter,
						IScimToScimRepresentation scimToScimRepresentation,
						ISCIMSchemaQueryRepository scimSchemaQueryRepository,
						IResourceTypeResolver resourceTypeResolver,
						Helpers.IUriProvider uriProvider,
						IAttributeReferenceEnricher attributeReferenceEnricher,
						SCIMHostOptions options,
						ISCIMRepresentationHelper scimRepresentationHelper) : base(groupRepository, scimRepresentationConverter, scimToScimRepresentation, scimSchemaQueryRepository, resourceTypeResolver, uriProvider, attributeReferenceEnricher, options, scimRepresentationHelper)
		{
		}
	}
}
