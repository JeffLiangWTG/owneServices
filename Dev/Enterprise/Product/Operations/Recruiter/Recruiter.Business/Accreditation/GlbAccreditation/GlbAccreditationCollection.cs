using CargoWise.EntityFramework;
using Enterprise.Integration.Recruiter;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationCollection : ActiveBusinessObjectCollection<GlbAccreditation>, IGlbAccreditationCollection
	{
		public GlbAccreditationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbAccreditationCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public GlbAccreditationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
