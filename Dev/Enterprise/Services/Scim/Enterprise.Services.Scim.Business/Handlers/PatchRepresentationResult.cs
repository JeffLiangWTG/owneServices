using SimpleIdServer.Scim.Domains;

namespace Enterprise.Services.Scim.Business.Handlers
{
	public class PatchRepresentationResult
	{
		public bool IsPatched { get; private set; }
		public SCIMRepresentation SCIMRepresentation { get; private set; }

		public static PatchRepresentationResult NoPatch()
		{
			return new PatchRepresentationResult { IsPatched = false };
		}

		public static PatchRepresentationResult Ok(SCIMRepresentation scimRepresentation)
		{
			return new PatchRepresentationResult { IsPatched = true, SCIMRepresentation = scimRepresentation };
		}
	}
}
