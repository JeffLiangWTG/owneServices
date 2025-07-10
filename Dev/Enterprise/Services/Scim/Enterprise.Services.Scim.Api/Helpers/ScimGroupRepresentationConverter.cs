using Enterprise.Services.Scim.Business;
using Enterprise.Services.Scim.Helpers;
using Enterprise.Services.Scim.Models;
using Newtonsoft.Json;
using SimpleIdServer.Scim.Helpers;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Api.Helpers
{
	#region SuppressResourceStringsCheckRegion
	public class ScimGroupRepresentationConverter : IScimRepresentationConverter<ScimGroup>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Auto generated baseline suppressions - WI00637629")]
		readonly ISCIMRepresentationHelper scimRepresentationHelper;

		public ScimGroupRepresentationConverter(ISCIMSchemaQueryRepository sCIMSchemaQueryRepository, ISCIMRepresentationHelper scimRepresentationHelper)
		{
			QueryRepository = sCIMSchemaQueryRepository;
			this.scimRepresentationHelper = scimRepresentationHelper;
		}

		public ISCIMSchemaQueryRepository QueryRepository { get; }

		public ScimGroup JsonToScim(string json)
		{
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			return JsonConvert.DeserializeObject<ScimGroup>(json, new ScimGroupJsonConverter());
		}
	}

	#endregion
}
