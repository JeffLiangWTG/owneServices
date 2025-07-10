using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementFeeCodeLine))]
	sealed class StatementFeeCodeLineCollectionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusStatementHeader>();
			return new StatementFeeCodeLine(header, "124", "Pecan Fee");
		}
	}
}
