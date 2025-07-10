using CargoWise.Types;

namespace Enterprise.Customs.NO.Business.Testing
{
	static class CusEntryLineFeeCollectionTestExtension
	{
		public static CusEntryLineFee AddNew(this CusEntryLineFeeCollection self, ZString chargeType, ZDecimal chargeAmount)
		{
			var fee = self.AddNew();
			fee.CF_ChargeType = chargeType;
			fee.CF_ChargeAmount = chargeAmount;
			return fee;
		}
	}
}
