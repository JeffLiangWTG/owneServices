using CargoWise.EntityFramework;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostingCustomsFee : AutoLandedCostingCustomsFee
	{
		public LandedCostingCustomsFee(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public LandedCostingCustomsFeeLookups Lookups
		{
			get { return lookups ?? (lookups = new LandedCostingCustomsFeeLookups(this)); }
		}
		LandedCostingCustomsFeeLookups lookups;
	}
}
