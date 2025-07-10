using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentExclusion : AutoJobDocumentExclusion
	{
		public JobDocumentExclusion(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static ZQuery GetJobDocumentExclusionQuery(ZGuid orgDocPk, ZGuid parentId, string parentTableCode)
		{
			var query = new ZQuery(JobDocumentExclusionSchema.JDE_OD_Document, orgDocPk);
			query.AddToFilter(JobDocumentExclusionSchema.JDE_ParentID, parentId);
			query.AddToFilter(JobDocumentExclusionSchema.JDE_ParentTableCode, parentTableCode);
			return query;
		}
	}
}
