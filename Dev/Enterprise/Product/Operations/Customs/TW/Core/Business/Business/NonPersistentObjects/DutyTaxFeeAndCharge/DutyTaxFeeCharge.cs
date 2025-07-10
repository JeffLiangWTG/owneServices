using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class DutyTaxFeeCharge : AutoDutyTaxFeeCharge
	{
		public DutyTaxFeeCharge(CusEntryHeader supporter) : base(supporter.Factory)
		{
		}
		public override ZString ChargeTypeDesc { get => ChargeTypeList.GetDescriptionFromCode(ChargeType); }

		public override ZString MethodOfPaymentDesc { get => PaymentMethodsList.GetDescriptionFromCode(MethodOfPayment); }

		ICodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<DutyTaxFeeCodeList>();

		ICodeDescriptionPairList PaymentMethodsList => Factory.GetCachedValue<EntryChargePaymentMethod>();
	}
}
