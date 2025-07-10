using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataLookups : AutoOrgCountryDataLookups
	{
		public OrgCountryDataLookups(AutoOrgCountryData parent)
			: base(parent)
		{
		}

		new OrgCountryData Parent
		{
			get { return (OrgCountryData)base.Parent; }
		}

		#region AviationSecuritySchemeMembershipList

		public virtual CodeDescriptionPairList AviationSecuritySchemeMembershipList
		{
			get { return Parent.SupplyChainSecurityConfiguration.ApprovalCodesList; }
		}

		#endregion

		public BondedWarehouseCollection BondedWarehouseList
		{
			get { return new BondedWarehouseCollection(Factory); }
		}
	}
}
