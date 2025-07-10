using CargoWise.Integration;

namespace Enterprise.Customs.ZA.Business
{
	public class ProvisionalPaymentCusEntryPayInfoLookups : CusEntryPayInfoLookups
	{
		public ProvisionalPaymentCusEntryPayInfoLookups(ProvisionalPaymentCusEntryPayInfo parent) : base(parent) { }

		protected new ProvisionalPaymentCusEntryPayInfo Parent => (ProvisionalPaymentCusEntryPayInfo)base.Parent;

		public ICodeDescriptionPairList ProvisionalPaymentStatuses => ZARefCusCodeListTypes.GetCustomsStatusList(Factory);
	}
}
