using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(MultimodalDangerousGoodsDeclaration))]
	sealed class MultimodalDangerousGoodsDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MultimodalDangerousGoodsDeclaration("zzz")
			{
				FollowOnPages = Array.Empty<MultimodalDangerousGoodsDeclarationFollowOn>()
			};
		}
	}
}
