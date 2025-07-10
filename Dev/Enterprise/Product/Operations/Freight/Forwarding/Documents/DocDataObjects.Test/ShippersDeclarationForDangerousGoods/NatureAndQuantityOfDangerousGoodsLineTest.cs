using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(NatureAndQuantityOfDangerousGoodsLine))]
	sealed class NatureAndQuantityOfDangerousGoodsLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAuthorizationReadonly()
		{
			var nqg = new NatureAndQuantityOfDangerousGoodsLine(0, NatureAndQuantityOfDangerousGoodsLineType.Detail);
			AssertEquals("Authorization is not readonly for Detail line", false, nqg.Authorization_ReadOnly);

			nqg = new NatureAndQuantityOfDangerousGoodsLine(0, NatureAndQuantityOfDangerousGoodsLineType.Dummy);
			AssertEquals("Authorization is readonly for Dummy line", true, nqg.Authorization_ReadOnly);

			nqg = new NatureAndQuantityOfDangerousGoodsLine(0, NatureAndQuantityOfDangerousGoodsLineType.Summary);
			AssertEquals("Authorization is readonly for Summary line", true, nqg.Authorization_ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => new NatureAndQuantityOfDangerousGoodsLine(0, NatureAndQuantityOfDangerousGoodsLineType.Detail);
	}
}
