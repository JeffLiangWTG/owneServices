using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Services.Scim.Contracts;
using Enterprise.Services.Scim.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Scim.Domain;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Business
{
	public class ScimToScimRepresentation : IScimToScimRepresentation
	{
		readonly ISCIMRepresentationHelper scimRepresentationHelper;

		public ScimToScimRepresentation(ISCIMSchemaQueryRepository sCIMSchemaQueryRepository, ISCIMRepresentationHelper scimRepresentationHelper)
		{
			QueryRepository = sCIMSchemaQueryRepository;
			this.scimRepresentationHelper = scimRepresentationHelper;
		}

		public ISCIMSchemaQueryRepository QueryRepository { get; }

		public async Task<SCIMRepresentation> ScimToSCIMRepresentation(ScimBase scim, bool isGetOperation = false)
		{
			var mainSchema = await QueryRepository.FindRootSCIMSchemaByResourceType(GetResourceType(scim));
			if (isGetOperation)
			{
				mainSchema.Attributes.Where(a => a.FullPath.StartsWith(AttributeNames.Groups)).ForEach(att => att.Mutability = SCIMSchemaAttributeMutabilities.READWRITE);
			}

			var schemas = new List<SCIMSchema>
			{
				mainSchema
			};
			var result = new SCIMRepresentation
			{
				ExternalId = scim.ExternalId,
				Schemas = schemas,
				ResourceType = GetResourceType(scim)
			};

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			string json = scim.GetType() == typeof(ScimUser)
				? JsonConvert.SerializeObject(scim, Formatting.Indented, new ScimUserJsonConverter())
				: JsonConvert.SerializeObject(scim, Formatting.Indented, new ScimGroupJsonConverter());

			if (string.IsNullOrEmpty(json))
			{
				return null;
			}

			var res = scimRepresentationHelper.ExtractSCIMRepresentationFromJSON(JObject.Parse(json), scim.ExternalId, mainSchema, QueryRepository.GetExtensions(mainSchema));
			res.Id = scim.Id.ToString();
			res.ResourceType = GetResourceType(scim);
			return res;
		}

		string GetResourceType(ScimBase scim)
		{
			return scim.GetType() == typeof(ScimUser) ? SCIMResourceTypes.User : SCIMResourceTypes.Group;
		}

		public ScimBase RepresentationToScim(SCIMRepresentation representation)
		{
			if (representation == null)
			{
				return null;
			}

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			var json = representation.ToResponse("").ToString();

			return representation.ResourceType == SCIMResourceTypes.User
				? JsonConvert.DeserializeObject<ScimUser>(json, new ScimUserJsonConverter())
				: JsonConvert.DeserializeObject<ScimGroup>(json, new ScimGroupJsonConverter());
		}
	}
}
