using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.Registry.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.DataRegistry.GUI.Testing
{
	[TestedType(typeof(CycleNoUserControl))]
	sealed class CycleNoUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new CycleNoCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((CycleNoUserControl)control).CycleNoGrid.ReadOnly;
	}
}
