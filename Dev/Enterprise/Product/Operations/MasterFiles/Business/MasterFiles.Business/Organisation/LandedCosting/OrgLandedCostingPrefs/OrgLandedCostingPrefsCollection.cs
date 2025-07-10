using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgLandedCostingPrefsCollection : DependentBusinessObjectCollection<OrgLandedCostingPrefs, OrgHeader>
	{
		internal OrgLandedCostingPrefsCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgLandedCostingPrefsCollection(OrgHeader organisation) : base(organisation)
		{
		}

		#region Landed Cost Group Retrieval

		public OrgLandedCostingPrefs GetLandedCostingGroupFromID(ZByte groupID)
		{
			foreach (OrgLandedCostingPrefs preference in this)
			{
				if (preference.O9_LandedCostGroup == groupID)
				{
					return preference;
				}
			}
			return null;
		}

		public ZString GetLandedCostingGroupNameFromID(ZByte groupID)
		{
			OrgLandedCostingPrefs preference = GetLandedCostingGroupFromID(groupID);
			return preference == null ? ZString.Empty : preference.O9_LandedCostGroupName;
		}

		public ZString GetLandedCostingGroupCostDistributeByFromID(ZByte groupID)
		{
			OrgLandedCostingPrefs preference = GetLandedCostingGroupFromID(groupID);
			return preference == null ? ZString.Empty : preference.O9_DistributeCostBy;
		}

		public OrgLandedCostingPrefs GetLandedCostingGroupFromChargeCode(AccChargeCode chargeCode)
		{
			return GetLandedCostingGroupFromChargeCode(chargeCode, null);
		}

		internal OrgLandedCostingPrefs GetLandedCostingGroupFromChargeCode(AccChargeCode chargeCode, OrgLandedCostingPrefs excludePrefs)
		{
			OrgLandedCostingPrefs result = null;

			if (chargeCode != null)
			{
				foreach (OrgLandedCostingPrefs pref in this)
				{
					if (excludePrefs == null || (excludePrefs != null && pref != excludePrefs))
					{
						foreach (OrgLandedCostingPrefCharges charge in pref.Charges)
						{
							if (charge.O0_Excluded && charge.O0_AC_ChargeCode == chargeCode.PK)
							{
								result = null;
								break;
							}

							bool chargeGroupMatchAndNoExclusion = charge.O0_ChargeGroup == chargeCode.AC_ChargeGroup &&
								(!charge.O0_Excluded || (charge.O0_Excluded && charge.O0_AC_ChargeCode != chargeCode.PK));

							bool chargeCodeInclusionMatch = charge.O0_ChargeGroup != chargeCode.AC_ChargeGroup &&
								!charge.O0_Excluded && charge.O0_AC_ChargeCode == chargeCode.PK;

							if (chargeGroupMatchAndNoExclusion || chargeCodeInclusionMatch)
							{
								result = pref;
							}
						}
					}

					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		public OrgLandedCostingPrefs GetLandedCostingGroupFromChargeGroup(ZString chargeCodeGroup)
		{
			return GetLandedCostingGroupFromChargeGroup(chargeCodeGroup, null);
		}

		internal OrgLandedCostingPrefs GetLandedCostingGroupFromChargeGroup(ZString chargeCodeGroup, OrgLandedCostingPrefs excludePrefs)
		{
			OrgLandedCostingPrefs result = null;

			foreach (OrgLandedCostingPrefs pref in this)
			{
				if (excludePrefs == null || (excludePrefs != null && pref != excludePrefs))
				{
					foreach (OrgLandedCostingPrefCharges charge in pref.Charges)
					{
						if (charge.O0_ChargeGroup == chargeCodeGroup)
						{
							result = pref;
							break;
						}
					}
				}

				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		#endregion
	}
}
