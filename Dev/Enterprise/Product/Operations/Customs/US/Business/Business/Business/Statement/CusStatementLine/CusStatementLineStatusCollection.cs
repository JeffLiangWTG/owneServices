using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CusStatementLineStatusCollection : SubsetBusinessObjectCollection<CusStatementLine>
	{
		public CusStatementLineStatusCollection(CusStatementHeader statement, ZString statusToMatch)
			: base(statement.StatementLines)
		{
			this.statusToMatch = statusToMatch;
			Rebuild();
			this.Sort(CusStatementLineSchema.Constants.B3_EntryProcessPort, System.ComponentModel.ListSortDirection.Ascending);
		}

		readonly ZString statusToMatch;

		protected override void RebuildOnConstruction()
		{
			//statusToMatch has not been set yet
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return ((CusStatementLine)element).B3_Status == statusToMatch;
		}
	}
}
