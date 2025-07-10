using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CusStatementLineAccountingReconCollection : ActiveBusinessObjectCollection<CusStatementLine>
	{
		public CusStatementLineAccountingReconCollection(CusStatementHeader statement)
			: base(statement)
		{
			this.statement = statement;
		}

		readonly CusStatementHeader statement;

		protected override object[] GetCollectionState()
		{
			return new object[] { statement.FilterStatementLinesBy };
		}

		protected override bool MatchesFilterCore(CusStatementLine element, bool fetchOnlyFromLocalCache)
		{
			bool result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && !element.IsStatusDeleted;

			if (result && !statement.FilterStatementLinesBy.IsEmpty)
			{
				switch (statement.FilterStatementLinesBy)
				{
					case StatementLineFilterByOptionList.Codes.LinesWithAPDiscrepany:
						result = element.DifferenceBetweenAPInvoiceAndCustomsAmount != 0m;
						break;

					case StatementLineFilterByOptionList.Codes.LinesWithARDiscrepany:
						result = element.DifferenceBetweenARInvoiceAndCustomsAmount != 0m;
						break;

					case StatementLineFilterByOptionList.Codes.LinesWithDiscrepany:
						result = element.HasDiscrepancyBetweenInvoicesAndCustomsAmount;
						break;

					case StatementLineFilterByOptionList.Codes.All:
						result = true;
						break;

					default:
						result = false;
						break;
				}
			}

			return result;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
