using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class CustomsFee : ICustomsFee
	{
		public CustomsFee(string feeType, decimal amount)
		{
			this.feeType = feeType;
			this.amount = amount;
		}

		readonly decimal amount;
		readonly string feeType;

		public ZDecimal AmountInLocalCurrency => amount;

		public ZString FeeCode => feeType;
	}
}
