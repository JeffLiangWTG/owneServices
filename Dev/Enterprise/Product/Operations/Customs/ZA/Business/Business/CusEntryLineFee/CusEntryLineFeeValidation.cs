using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryLineFeeValidation : Customs.Business.CusEntryLineFeeValidation
	{
		public CusEntryLineFeeValidation(CusEntryLineFee parent) : base(parent)
		{
		}

		protected override void CheckCF_ChargeType()
		{
			var fee = (Parent as CusEntryLineFee);
			if (fee != null)
			{
				if (fee.IsRebate && fee.CF_ChargeAmount <= ZDecimal.Zero)
				{
					fee.AddRowWarning(RebateValueIsNotZeroError);
					fee.EntryLine.AddRowWarning(RebateValueIsNotZeroError);
				}
			}
		}

		public static string RebateValueIsNotZeroError => ResString.GetMultilingualString("2F6C3D6A-2018-431C-8A3F-3E04E42FC96F", "Rebate Value is Zero, please recheck the CPC and other values on the Invoice Line");
	}
}
