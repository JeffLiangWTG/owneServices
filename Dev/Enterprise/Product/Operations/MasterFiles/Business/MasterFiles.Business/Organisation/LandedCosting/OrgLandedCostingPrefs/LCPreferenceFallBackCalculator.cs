using System.Collections;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class LCPreferenceFallBackCalculator
	{
		public LCPreferenceFallBackCalculator(OrgHeader consignee)
		{
			this.Consignee = consignee;
		}

		public ILandedCostPreference GetPreferenceFromAccChargeCode(AccChargeCode chargeCode)
		{
			ILandedCostPreference result;
			if (Consignee != null && Consignee.LandedCostingPreferences.Count > 0)
			{
				result = Consignee.LandedCostingPreferences.GetLandedCostingGroupFromChargeCode(chargeCode);
			}
			else
			{
				result = RegistryLCGroups.GetLandedCostingGroupFromChargeCode(chargeCode, null);
			}
			return result;
		}

		public string GetLandedCostingGroupNameFromID(ZByte groupID)
		{
			string result;
			if (Consignee != null && Consignee.LandedCostingPreferences.Count > 0)
			{
				result = Consignee.LandedCostingPreferences.GetLandedCostingGroupNameFromID(groupID);
			}
			else
			{
				result = RegistryLCGroups.GetLandedCostingGroupNameFromID(groupID);
			}
			return result;
		}

		public string GetLandedCostingGroupDistributionCodeFromID(ZByte groupID)
		{
			string result;

			if (Consignee != null && Consignee.LandedCostingPreferences.Count > 0)
			{
				result = Consignee.LandedCostingPreferences.GetLandedCostingGroupCostDistributeByFromID(groupID);
			}
			else
			{
				result = RegistryLCGroups.GetLandedCostingGroupDistributionCodeFromID(groupID);
			}
			return result;
		}

		public ILandedCostPreference[] GetPreferences()
		{
			ArrayList result = new ArrayList();
			if (Consignee != null && Consignee.LandedCostingPreferences.Count > 0)
			{
				result.AddRange(Consignee.LandedCostingPreferences);
			}
			else
			{
				result.AddRange(RegistryLCGroups);
			}
			return (ILandedCostPreference[])result.ToArray(typeof(ILandedCostPreference));
		}

		#region Implementation

		readonly OrgHeader Consignee;

		LandedCostingGroupCollection RegistryLCGroups
		{
			get { return FreightDataRegistry.Instance.LandedCostingPreferences.Value; }
		}

		#endregion
	}
}
