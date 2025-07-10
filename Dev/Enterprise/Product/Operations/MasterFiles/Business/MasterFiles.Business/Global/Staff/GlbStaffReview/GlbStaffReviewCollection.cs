using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffReviewCollection : ActiveBusinessObjectCollection<GlbStaffReview>
	{
		public GlbStaffReviewCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public GlbStaffReviewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbStaffReviewCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
