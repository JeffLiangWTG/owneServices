using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffWorkingBasisCollection : ActiveBusinessObjectCollection<GlbStaffWorkingBasis>
	{
		public GlbStaffWorkingBasisCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffWorkingBasisCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffWorkingBasisCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
