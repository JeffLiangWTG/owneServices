using System.Collections.Generic;
using System.Linq;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Business
{
	public static class QueryRepositoryHelper
	{
		public static ICollection<SCIMSchema> GetExtensions(this ISCIMSchemaQueryRepository repository, SCIMSchema mainSchema)
		{
			return repository.FindSCIMSchemaByIdentifiers(mainSchema.SchemaExtensions.Select(e => e.Schema)).Result.ToList();
		}
	}
}
