using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementMessageLineCollection))]
	sealed class StatementMessageLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<StatementMessageLineCollection>
	{
		protected override StatementMessageLineCollection GetCollectionToTest()
		{
			return new StatementMessageLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var blockQ1 = new PMSQ1();
			return new MonthlyStatementMessageLine(blockQ1);
		}
	}
}
