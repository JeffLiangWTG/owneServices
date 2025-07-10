using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BorderCargoPortCollection))]
	sealed class BorderCargoPortCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BorderCargoPortCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BorderCargoPortCollection GetCollectionToTest() => new BorderCargoPortCollection(new FallbackLevel(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new BorderCargoPort();
	}
}
