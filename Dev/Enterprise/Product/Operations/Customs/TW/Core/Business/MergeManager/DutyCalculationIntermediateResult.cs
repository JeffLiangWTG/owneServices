using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class DutyCalculationIntermediateResult
	{
		public ZString RateCode { get; set; }
		public ZString PaymentMethod { get; set; }
		public ZDecimal Amount { get; set; }
		public ZString UnitOfCalculation { get; set; }
		public ZDecimal Rate { get; set; }
		public ZDecimal BaseValue { get; set; }
		public ZString TypeCode { get; set; }
	}
}
