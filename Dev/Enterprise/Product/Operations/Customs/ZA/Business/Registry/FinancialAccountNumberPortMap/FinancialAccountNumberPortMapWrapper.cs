using CargoWise.Types;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class FinancialAccountNumberPortMapWrapper
	{
		public FinancialAccountNumberPortMapWrapper(FinancialAccountNumberPortMap financialAccountNumberPortMap)
		{
			FinancialAccountNumberPortMap = financialAccountNumberPortMap;
		}

		public readonly FinancialAccountNumberPortMap FinancialAccountNumberPortMap;
		public ZDateTime DueDate;
		public ZDateTime StartDate;

		public ZDecimal VatDefermentAmount => FinancialAccountNumberPortMap.VatDefermentAmount;
		public ZDecimal DutyDefermentAmount => FinancialAccountNumberPortMap.DutyDefermentAmount;
	}
}
