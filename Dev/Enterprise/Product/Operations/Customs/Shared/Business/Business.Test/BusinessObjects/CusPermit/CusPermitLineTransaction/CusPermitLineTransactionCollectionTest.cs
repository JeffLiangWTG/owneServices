using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPermitLineTransactionCollection))]
	sealed class CusPermitLineTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusPermitLineTransactionCollection>
	{
		protected override CusPermitLineTransactionCollection GetCollectionToTest()
		{
			return new CusPermitLineTransactionCollection(Factory.New<BaseCusPermitHeader>());
		}
	}
}
