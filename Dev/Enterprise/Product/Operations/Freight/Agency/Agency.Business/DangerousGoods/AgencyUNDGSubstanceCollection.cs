using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyUNDGSubstanceCollection : UNDGSubstanceCollection
	{
		public AgencyUNDGSubstanceCollection(BusinessObjectFactory factory) : base(factory)
		{
			CfrShouldBeDefaulted = false;
		}

		public AgencyUNDGSubstanceCollection(BusinessObjectFactory factory, bool cfrShouldBeDefaulted) : base(factory)
		{
			this.CfrShouldBeDefaulted = cfrShouldBeDefaulted;
		}

		public AgencyUNDGSubstanceCollection(BusinessObjectFactory factory, ZQuery filter, bool cfrShouldBeDefaulted) : base(factory, filter)
		{
			this.CfrShouldBeDefaulted = cfrShouldBeDefaulted;
		}

		public readonly bool CfrShouldBeDefaulted;

		protected override IFindBoxListProvider FindBoxListProvider => new AgencyUNDGSubstanceCollectionFindBoxListProvider(this);
	}
}
