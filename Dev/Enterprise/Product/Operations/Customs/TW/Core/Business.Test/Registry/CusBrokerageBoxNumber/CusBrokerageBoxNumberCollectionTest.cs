using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusBrokerageBoxNumberCollection))]
	sealed class CusBrokerageBoxNumberCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CusBrokerageBoxNumberCollection>
	{
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override CusBrokerageBoxNumberCollection GetCollectionToTest() => new CusBrokerageBoxNumberCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new CusBrokerageBoxNumber();
	}
}
