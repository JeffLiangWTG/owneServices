using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.ZQueryHelpers
{
	public static class JobDocumentDataZQueryFilters
	{
		public static ZQuery GetIndexedQuery(ZGuid pk, string tablePrefix)
		{
			var query = new ZQuery(JobDocumentDataSchema.JDD_ParentID, pk);
			query.AddToFilter(JobDocumentDataSchema.JDD_ParentTableCode, tablePrefix);
			return query;
		}

		public static ZQuery GetIndexedQuery(ZGuid pk, string tablePrefix, string name)
		{
			var query = GetIndexedQuery(pk, tablePrefix);
			query.AddToFilter(JobDocumentDataSchema.JDD_Name, name);
			return query;
		}
	}
}
