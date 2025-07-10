using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDeliveryCollection : ActiveBusinessObjectCollection<JobDocumentDelivery>
	{
		public JobDocumentDeliveryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JobDocumentDeliveryCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public JobDocumentDeliveryCollection(BusinessObjectFactory factory, BusinessObject parentBusinessObject)
			: base(factory, parentBusinessObject, null, JobDocumentDeliverySchema.JDC_ParentID)
		{
		}
	}
}
