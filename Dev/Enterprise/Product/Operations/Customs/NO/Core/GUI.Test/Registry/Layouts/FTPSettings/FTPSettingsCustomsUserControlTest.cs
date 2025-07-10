using CargoWise.EntityFramework;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(FTPSettingsCustomsUserControl))]
sealed class FTPSettingsCustomsUserControlTest : RegistryZUserControlTestCase
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var userControl = new FTPSettingsCustomsUserControl();
		_ = userControl.AssertThisControl(x => x
			.WithCaptionRenderingEnabled());

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.SendToCustomFolderTextBox), x => x
			.WithBindTo(nameof(FTPSettingsCustomsRegistry.SendToCustomFolder))
			.WithCaption("Folder for send to Customs"));

		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.ReceiveFromCustomFolderTextBox), x => x
			.WithBindTo(nameof(FTPSettingsCustomsRegistry.ReceiveFromCustomFolder))
			.WithCaption("Folder for receive from Customs"));
	});

	protected override IBusiness GetNewBusinessEntity() => new FTPSettingsCustomsRegistry();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		=> control.ReadOnly || businessEntity.IsReadOnly;
}
