using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGuaranteeLineTransactionCollection))]
	sealed class CusGuaranteeLineTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusGuaranteeLineTransactionCollection>
	{
		protected override CusGuaranteeLineTransactionCollection GetCollectionToTest()
		{
			return new CusGuaranteeLineTransactionCollection(Factory.New<BaseCusGuaranteeHeader>());
		}
	}
}
