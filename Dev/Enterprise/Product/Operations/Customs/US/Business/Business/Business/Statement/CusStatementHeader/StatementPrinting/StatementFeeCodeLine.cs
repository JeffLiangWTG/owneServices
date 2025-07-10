using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class StatementFeeCodeLine : NonPersistentBusinessObject
	{
		public StatementFeeCodeLine(CusStatementHeader statement, ZString code, ZString description)
		{
			header = statement;
			FeeCode = code;
			FeeDescription = description.ToUpper();
		}

		readonly CusStatementHeader header;

		public ZString FeeCode
		{
			get;
			private set;
		}

		public ZString FeeDescription
		{
			get;
			private set;
		}

		public ZDecimal TotalFee
		{
			get => header.GetTotalPayableAmountForAllLines(FeeCode);
		}

		public ZDecimal FinalTotalFee
		{
			get => header.GetTotalPayableAmountForActiveLines(FeeCode);
		}

		public ZDecimal Difference
		{
			get => FinalTotalFee - TotalFee;
		}
	}
}
