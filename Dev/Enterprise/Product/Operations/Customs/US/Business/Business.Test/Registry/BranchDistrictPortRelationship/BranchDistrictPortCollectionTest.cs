using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BranchDistrictPortCollection))]
	sealed class BranchDistrictPortCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BranchDistrictPortCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BranchDistrictPortCollection GetCollectionToTest() => BranchPorts;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new BranchDistrictPort(FallbackLevel, Factory, BranchPorts);

		BranchDistrictPortCollection branchPorts;
		BranchDistrictPortCollection BranchPorts => branchPorts ?? (branchPorts = new BranchDistrictPortCollection(FallbackLevel, Factory));

		FallbackLevel fallbackLevel;
		FallbackLevel FallbackLevel => fallbackLevel ?? (fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
	}
}
