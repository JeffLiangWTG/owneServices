using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FTZDutyFeeCalculator : IDutyFeeCalculator
	{
		public FTZDutyFeeCalculator(IDeclarationDutyDataProvider declaration)
		{
			this.declaration = declaration;
		}

		readonly IDeclarationDutyDataProvider declaration;

		public void Calculate()
		{
			IDutyDataLineHeader entry = declaration.FTZEntry;

			if (entry != null)
			{
				var feeCalculator = new FeeCalculator(declaration.Factory, entry.IsHMFApplicable, SetFeeResult);
				var lineCalculator = declaration.GetLineCalculator();

				foreach (IEntryLineOrInvoiceLineDutyData line in entry.DutyDataLines)
				{
					lineCalculator.Calculate(line, feeCalculator);

					line.RollUpFees(entry);
				}
			}
		}

		void SetFeeResult(FeeResult feeResult, string feeCode, IFeeCalculationDataProvider invoiceLine)
		{
			ZDecimal amount = feeResult.IsRequired ? feeResult.Amount : ZDecimal.Zero;
			invoiceLine.SetFeeResult(feeCode, amount.Round(2), new FeeCalculationInternalData());
		}
	}
}
