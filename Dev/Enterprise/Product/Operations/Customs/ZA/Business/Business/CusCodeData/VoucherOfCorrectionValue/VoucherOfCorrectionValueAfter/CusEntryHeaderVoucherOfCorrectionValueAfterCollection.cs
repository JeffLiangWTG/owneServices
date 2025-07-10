using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryHeaderVoucherOfCorrectionValueAfterCollection : VoucherOfCorrectionValueAfterCollection<CusEntryHeader>
	{
		public CusEntryHeaderVoucherOfCorrectionValueAfterCollection(CusEntryHeader master) : base(master)
		{
		}

		protected override bool ShouldDeleteCodeOnSet(ZString code, ZDecimal value)
		{
			var result = value.IsEmpty;
			switch (code)
			{
				case VOCValueTypeList.Codes.ProvisionalPayment:
				case VOCValueTypeList.Codes.Penalty:
					result = false;
					break;
			}
			return result;
		}
	}
}
