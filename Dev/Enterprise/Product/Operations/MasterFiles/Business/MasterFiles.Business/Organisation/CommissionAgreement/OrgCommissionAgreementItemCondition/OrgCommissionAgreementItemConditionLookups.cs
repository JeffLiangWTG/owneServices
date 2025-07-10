//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionAgreementItemConditionLookups
//
//    This class should be used for overriding collections in AutoOrgCommissionAgreementItemConditionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItemConditionLookups : AutoOrgCommissionAgreementItemConditionLookups
	{
		public OrgCommissionAgreementItemConditionLookups(AutoOrgCommissionAgreementItemCondition parent) : base(parent)
		{
		}

		public new OrgCommissionAgreementItemCondition Parent
		{
			get
			{
				return (OrgCommissionAgreementItemCondition)base.Parent;
			}
		}

		public ReadOnlyCodeDescriptionPairList Modes
		{
			get
			{
				var code = Parent?.Parent?.CAI_Code ?? string.Empty;
				return CommissionLookups.New(Factory).GetModes(code);
			}
		}

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}
	}
}
