//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgRateFeeChargeLevelLookups
//
//    This class should be used for overriding collections in AutoOrgRateFeeChargeLevelLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateFeeChargeLevelLookups : AutoOrgRateFeeChargeLevelLookups
	{
		public OrgRateFeeChargeLevelLookups(AutoOrgRateFeeChargeLevel parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList Amount1TypeList
		{
			get
			{
				if (amount1TypeList == null)
				{
					amount1TypeList = new CodeDescriptionPairList(OLookUpEditType.ServiceLevelAmount1Type);
				}

				return amount1TypeList;
			}
		}
		CodeDescriptionPairList amount1TypeList;

		public CodeDescriptionPairList Amount2TypeList
		{
			get
			{
				if (amount2TypeList == null)
				{
					amount2TypeList = new CodeDescriptionPairList(OLookUpEditType.ServiceLevelAmount2Type);
				}

				return amount2TypeList;
			}
		}
		CodeDescriptionPairList amount2TypeList;

		#region OrgRateFeeChargeType_List

		public CodeDescriptionPairList ServiceTypeList
		{
			get
			{
				if (serviceTypeList == null)
				{
					serviceTypeList = new CodeDescriptionPairList();

					foreach (FeeChargeType type in OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value.FeeChargeTypes)
					{
						serviceTypeList.AddPair(type.Code, (ZString)type.Description);
					}
				}
				return serviceTypeList;
			}
		}
		CodeDescriptionPairList serviceTypeList;

		#endregion

		#region Service Level List

		public CodeDescriptionPairList ServiceLevelList
		{
			get
			{
				serviceLevelList = new CodeDescriptionPairList();
				var chargeType = ((OrgRateFeeChargeLevel)Parent).ChargeType;

				if (chargeType != null)
				{
					foreach (FeeChargeLevel level in chargeType.FeeChargeLevels)
					{
						serviceLevelList.AddPair(level.Code, level.Description);
					}
				}

				return serviceLevelList;
			}
		}
		CodeDescriptionPairList serviceLevelList;

		#endregion
	}
}
