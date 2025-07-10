using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(CusStatementLineChargeCollection))]
	class CusStatementLineChargeCollectionTest : ActiveBusinessObjectCollectionTestCase<CusStatementLineChargeCollection>
	{
		protected override CusStatementLineChargeCollection GetCollectionToTest()
		{
			return new CusStatementLineChargeCollection(Factory.New<CusStatementHeader>().StatementLines.AddNew());
		}
	}
}
