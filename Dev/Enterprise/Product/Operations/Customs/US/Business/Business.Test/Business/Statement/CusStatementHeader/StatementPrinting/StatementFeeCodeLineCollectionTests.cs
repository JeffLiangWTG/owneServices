using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementFeeCodeLineCollection))]
	sealed class StatementFeeCodeLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<StatementFeeCodeLineCollection>
	{
		protected override StatementFeeCodeLineCollection GetCollectionToTest()
		{
			return new StatementFeeCodeLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<CusStatementHeader>();
			return new StatementFeeCodeLine(header, "124", "Pecan Fee");
		}
	}
}
