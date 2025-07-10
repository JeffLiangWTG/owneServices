using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(BranchDistrictPortUserControl))]
	sealed class BranchDistrictPortUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new BranchDistrictPortCollection(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((BranchDistrictPortUserControl)control).MainGrid.ReadOnly;
	}
}
