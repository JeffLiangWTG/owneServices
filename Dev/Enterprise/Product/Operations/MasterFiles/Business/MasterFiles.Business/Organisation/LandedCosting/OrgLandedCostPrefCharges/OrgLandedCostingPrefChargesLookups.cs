namespace Enterprise.MasterFiles.Business
{
	public class OrgLandedCostingPrefChargesLookups : AutoOrgLandedCostingPrefChargesLookups, Integration.IOrgLandedCostingPrefChargesLookups
	{
		public OrgLandedCostingPrefChargesLookups(AutoOrgLandedCostingPrefCharges parent) : base(parent)
		{
		}

		#region Charge Groups

		public IncoTermChargeCodeGroupList IncoTermChargeGroups
		{
			get
			{
				if (fIncoTermChargeGroups == null)
				{
					fIncoTermChargeGroups = new IncoTermChargeCodeGroupList();
					fIncoTermChargeGroups.AddPair(ChargeCodeGroupList.Codes.BrokerageOnly, ChargeCodeGroupList.Descriptions.BrokerageOnly.GetUnresolvedString());
					fIncoTermChargeGroups.SortByDescription();
				}

				return fIncoTermChargeGroups;
			}
		}

		IncoTermChargeCodeGroupList fIncoTermChargeGroups;

		#endregion
	}
}
