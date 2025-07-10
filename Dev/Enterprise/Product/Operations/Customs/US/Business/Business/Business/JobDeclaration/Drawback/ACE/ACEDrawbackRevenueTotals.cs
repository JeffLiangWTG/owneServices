using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackRevenueTotals : IACEDrawbackRevenueTotals
	{
		public ACEDrawbackRevenueTotals(ZString accountingClassCode, ZDecimal totalAmount)
		{
			this.accountingClassCode = accountingClassCode;
			this.totalAmount = totalAmount;
		}
		readonly ZString accountingClassCode;
		readonly ZDecimal totalAmount;

		#region IACEDrawbackRevenueTotals Members

		ZString IACEDrawbackRevenueTotals.AccountingClassCode
		{
			get { return accountingClassCode; }
		}

		ZDecimal IACEDrawbackRevenueTotals.TotalAmount
		{
			get { return totalAmount; }
		}

		#endregion
	}
}
