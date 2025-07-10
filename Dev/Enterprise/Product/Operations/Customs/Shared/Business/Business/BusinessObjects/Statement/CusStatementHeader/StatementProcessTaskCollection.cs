using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class StatementProcessTaskCollection : ProcessTaskCollection
	{
		public StatementProcessTaskCollection(BaseCusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		public new StatementProcessTask this[int index] => (StatementProcessTask)Elements[index];

		public new StatementProcessTask AddNew()
		{
			return (StatementProcessTask)base.AddNew();
		}
	}
}
