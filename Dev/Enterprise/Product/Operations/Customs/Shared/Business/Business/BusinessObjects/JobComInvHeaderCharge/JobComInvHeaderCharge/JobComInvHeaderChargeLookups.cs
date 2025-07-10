//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvHeaderChargeLookups
//
//    This class should be used for overriding collections in AutoJobComInvHeaderChargeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class JobComInvHeaderChargeLookups : Common.JobComInvHeaderChargeLookups
	{
		public JobComInvHeaderChargeLookups(BaseJobComInvHeaderCharge parent)
			: base(parent)
		{
		}

		new BaseJobComInvHeaderCharge Parent
		{
			get { return (BaseJobComInvHeaderCharge)base.Parent; }
		}

		public CodeDescriptionPairList GroupIsIncludedInLineOptionList
		{
			get
			{
				return Parent.Factory.GetCachedValue("GroupIsIncludedInLinesOptionList", delegate
				{
					GroupIsIncludedInLinesOptionList result = new GroupIsIncludedInLinesOptionList();

					result.RemoveCode(GroupIsIncludedInLinesOptionList.Codes.NotApplicable);
					return result;
				});
			}
		}

		public virtual CodeDescriptionPairList ExchangeRateTypeList
		{
			get { return Factory.GetCachedValue<ChargeExchangeRateTypeList>(); }
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get { return Parent.Parent != null ? Parent.Parent.ChargeTypeList : base.ChargeTypeList; }
		}

		public CodeDescriptionPairList AllChargeTypeList => Parent.Parent?.AllChargeTypeList ?? new CodeDescriptionPairList();
	}
}
