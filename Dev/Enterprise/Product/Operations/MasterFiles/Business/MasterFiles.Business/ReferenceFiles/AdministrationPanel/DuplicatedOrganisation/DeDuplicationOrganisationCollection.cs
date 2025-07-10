using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class DeduplicationOrganisationCollection : BusinessObjectCollection<DeduplicationOrganisation>
	{
		public DeduplicationOrganisationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DeduplicationOrganisationCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
