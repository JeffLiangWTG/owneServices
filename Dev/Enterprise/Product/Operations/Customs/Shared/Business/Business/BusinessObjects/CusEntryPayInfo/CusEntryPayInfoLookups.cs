//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryPayInfoLookups
//
//    This class should be used for overriding collections in AutoCusEntryPayInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusEntryPayInfoLookups : AutoCusEntryPayInfoLookups
	{
		public CusEntryPayInfoLookups(AutoCusEntryPayInfo parent) : base(parent)
		{
		}

		public virtual ICodeDescriptionPairList TransactionTypeList => Factory.GetCachedValue<CodeDescriptionPairList>();

		public ICodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<PaidByCodeList>();

		public virtual ICodeDescriptionPairList PaymentStatusList => Factory.GetCachedValue<CusEntryPayInfoStatusList>();
	}
}
