using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class TagableExtensionMethods
	{
		public static ZQuery CreateTagLinksQuery(this ITagable tagable)
		{
			var query = new ZQuery(TagLinkSchema.TGL_ParentId, tagable.PK);
			query.FetchOnlyFromLocalCache = !((BusinessObject)tagable).IsInDatabase;

			return query;
		}
	}
}
