using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationCollection))]
	sealed class CusGoodsLocationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CusGoodsLocationCollection>
	{
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override CusGoodsLocationCollection GetCollectionToTest() => new CusGoodsLocationCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new CusGoodsLocation();
	}
}
