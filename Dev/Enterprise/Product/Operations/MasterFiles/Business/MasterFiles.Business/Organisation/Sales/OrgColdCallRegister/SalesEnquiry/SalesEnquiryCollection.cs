using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.SalesEnquiry)]
	public class SalesEnquiryCollection : ActiveBusinessObjectCollection<SalesEnquiry>
	{
		public SalesEnquiryCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public SalesEnquiryCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public SalesEnquiryCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
