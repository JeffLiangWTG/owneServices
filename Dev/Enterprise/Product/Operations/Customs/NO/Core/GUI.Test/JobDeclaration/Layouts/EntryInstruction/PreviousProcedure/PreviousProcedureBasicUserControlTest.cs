using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(PreviousProcedureBasicUserControl))]
sealed class PreviousProcedureBasicUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new PreviousProcedureBasicUserControl();
		_ = control.AssertContainsControl<ZDropEdit>("PreviousProcedureDropEdit", x => x
			.WithBindTo("CSI_Procedure")
			.WithCaption("Previous Procedure"));

		_ = control.AssertContainsControl<ZButton>("ImportFromTemporaryStorageRegisterButton", x => x
			.WithCaption("Import from TS Register")
			.WithFullDescription("Import from Temporary Storage Register")
			.WithValue(btn => btn.ToolTipCaption.ToString(), "Import from Temporary Storage Register"));
	});
}
