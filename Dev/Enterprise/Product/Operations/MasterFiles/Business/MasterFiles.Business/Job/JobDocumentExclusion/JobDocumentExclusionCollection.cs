using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentExclusionCollection : ActiveBusinessObjectCollection<JobDocumentExclusion>
	{
		public JobDocumentExclusionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JobDocumentExclusionCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public JobDocumentExclusionCollection(BusinessObjectFactory factory, BusinessObject parentBusinessObject)
			: base(factory, parentBusinessObject, null, JobDocumentExclusionSchema.JDE_ParentID)
		{
		}
	}
}
