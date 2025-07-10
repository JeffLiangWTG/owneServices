using Enterprise.Services.Scim.Api.Helpers;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Helpers;
using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Api.Controllers
{
	public class UsersController : ScimController<ScimUser>
	{
		public UsersController(IPersistanceRepository<ScimUser> userRepository,
						IScimRepresentationConverter<ScimUser> scimRepresentationConverter,
						IScimToScimRepresentation scimToScimRepresentation,
						ISCIMSchemaQueryRepository scimSchemaQueryRepository,
						IResourceTypeResolver resourceTypeResolver,
						Helpers.IUriProvider uriProvider,
						IAttributeReferenceEnricher attributeReferenceEnricher,
						SCIMHostOptions options,
						ISCIMRepresentationHelper scimRepresentationHelper) : base(userRepository, scimRepresentationConverter, scimToScimRepresentation, scimSchemaQueryRepository, resourceTypeResolver, uriProvider, attributeReferenceEnricher, options, scimRepresentationHelper)
		{
		}
	}
}
