using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(SenderInfoUserControl))]
class SenderInfoUserControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity()
	{
		return new SenderInfoCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
	}

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
	{
		var mainGrid = GUITestHelper.FindControl<ZGrid>(control.Controls, "MainGrid");
		return mainGrid.ReadOnly;
	}
}
