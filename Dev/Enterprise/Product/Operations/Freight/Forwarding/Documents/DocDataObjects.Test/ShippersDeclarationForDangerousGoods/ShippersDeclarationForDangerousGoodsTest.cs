using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(ShippersDeclarationForDangerousGoods))]
	sealed class ShippersDeclarationForDangerousGoodsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ShippersDeclarationForDangerousGoods();
	}
}
