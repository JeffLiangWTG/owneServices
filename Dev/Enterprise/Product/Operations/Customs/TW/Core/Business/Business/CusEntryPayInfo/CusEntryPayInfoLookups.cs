using CargoWise.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryPayInfoLookups : Customs.Business.CusEntryPayInfoLookups
	{
		public CusEntryPayInfoLookups(CusEntryPayInfo parent)
			: base(parent)
		{
		}

		public override ICodeDescriptionPairList TransactionTypeList => Factory.GetCachedValue<EntryChargeTypeList>();

		public ICodeDescriptionPairList ReasonOfPaymentList => Factory.GetCachedValue<ReasonOfPaymentList>();
	}
}
