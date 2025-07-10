using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgLandedCostingPrefChargesCollection : DependentBusinessObjectCollection<OrgLandedCostingPrefCharges, OrgLandedCostingPrefs>
	{
		public OrgLandedCostingPrefChargesCollection(OrgLandedCostingPrefs landedCostingPrefs) : base(landedCostingPrefs)
		{
		}
	}
}
