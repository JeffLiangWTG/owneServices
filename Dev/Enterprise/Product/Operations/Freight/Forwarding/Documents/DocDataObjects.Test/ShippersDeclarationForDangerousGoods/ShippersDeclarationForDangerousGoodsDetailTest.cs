using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.ShippersDeclarationForDangerousGoods
{
	[TestedType(typeof(ShippersDeclarationForDangerousGoodsDetail))]
	class ShippersDeclarationForDangerousGoodsDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ShippersDeclarationForDangerousGoodsDetail(
			"zzz",
			new ShippersDeclarationForDangerousGoodsHeader(),
			System.Array.Empty<NatureAndQuantityOfDangerousGoodsLine>());
	}
}
