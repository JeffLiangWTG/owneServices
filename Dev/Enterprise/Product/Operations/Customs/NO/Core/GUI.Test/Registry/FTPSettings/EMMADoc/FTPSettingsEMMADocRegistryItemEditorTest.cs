using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.GUI.Testing;

[TestedType(typeof(FTPSettingsEMMADocRegistryItemEditor))]
sealed class FTPSettingsEMMADocRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor()
		=> new FTPSettingsEMMADocRegistryItemEditor(dataType: new FTPSettingsEMMADocRegistryDataType(),
			fallbackLevel: new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty),
			factory: new BusinessObjectFactory());

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		if (editorPane is FTPSettingsEMMADocRegistryItemControl itemControl)
		{
			return !(itemControl.UserNameTextBox.ReadOnly
				&& itemControl.PasswordTextBox.ReadOnly
				&& itemControl.ViewButton.ReadOnly
				&& itemControl.UrlAddressTextBox.ReadOnly
				&& itemControl.PortTextBox.ReadOnly);
		}

		return false;
	}

	protected override Type GetExpectedEditorPaneType() => typeof(FTPSettingsEMMADocRegistryItemControl);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		=> new FTPSettingsEMMADocRegistryItem(name: "",
			caption: null,
			category: null,
			hint: null,
			storage: RegistryStorageFlags.System,
			options: RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
			defaultValue: new());

	protected override object[] GetValidRegistryValues() =>
		[
			new FTPSettingsRegistry
			{
				Url = "ftpedoc.emma.no",
				Port = "21",
				Username = "admin",
				Password = "password",
			}
		];

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
}
