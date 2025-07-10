using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class MonthlyDeletedStatementLinesCollection : BusinessObjectCollection<CusStatementLine>
	{
		public MonthlyDeletedStatementLinesCollection(CusStatementHeader monthlyStatementHeader)
			: base(monthlyStatementHeader.Factory)
		{
			if (!monthlyStatementHeader.IsMonthlyStatement)
			{
				ErrorReporter.ReportOnce("Not a monthly statement", "You should not create this collection for the statement header as it is not monthly");
			}
			this.monthlyStatementHeader = monthlyStatementHeader;
		}

		readonly CusStatementHeader monthlyStatementHeader;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();

			result.AddToFilter(CusStatementLineSchema.B3_Status, StatementLineStatusList.Codes.Deleted);

			ZQuery b3_B2Filter = new ZQuery();
			b3_B2Filter.AddToFilter(JoinCondition.Or, CusStatementLineSchema.B3_B2, monthlyStatementHeader.DailyStatements.GetPKs());
			result.AddToFilter(b3_B2Filter);

			return result;
		}
	}
}
