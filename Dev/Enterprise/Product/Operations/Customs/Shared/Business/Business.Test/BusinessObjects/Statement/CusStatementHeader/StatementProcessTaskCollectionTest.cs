using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(StatementProcessTaskCollection))]
	sealed class StatementProcessTaskCollectionTest : ProcessTaskCollectionTest<StatementProcessTaskCollection>
	{
		protected override StatementProcessTaskCollection GetCollectionToTestCore()
		{
			return new StatementProcessTaskCollection(Factory.New<BaseCusStatementHeader>());
		}
	}
}
