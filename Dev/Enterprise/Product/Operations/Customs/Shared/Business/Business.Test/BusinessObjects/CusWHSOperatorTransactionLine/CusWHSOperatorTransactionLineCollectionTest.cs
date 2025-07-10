using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusWHSOperatorTransactionLineCollection))]
	public class CusWHSOperatorTransactionLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusWHSOperatorTransactionLineCollection(Factory, Factory.New<CusWHSOperatorTransaction>());
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusWHSOperatorTransactionLine>();
	}
}
