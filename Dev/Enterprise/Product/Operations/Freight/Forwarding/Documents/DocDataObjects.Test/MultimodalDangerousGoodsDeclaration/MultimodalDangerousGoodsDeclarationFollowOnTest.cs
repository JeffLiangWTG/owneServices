using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(MultimodalDangerousGoodsDeclarationFollowOn))]
	sealed class MultimodalDangerousGoodsDeclarationFollowOnTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new MultimodalDangerousGoodsDeclarationFollowOn();
	}
}
