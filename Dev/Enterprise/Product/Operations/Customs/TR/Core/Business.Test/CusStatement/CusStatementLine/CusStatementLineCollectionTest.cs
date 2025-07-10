using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(CusStatementLineCollection))]
	public class CusStatementLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			return new CusStatementLineCollection(statementHeader);
		}
	}
}
