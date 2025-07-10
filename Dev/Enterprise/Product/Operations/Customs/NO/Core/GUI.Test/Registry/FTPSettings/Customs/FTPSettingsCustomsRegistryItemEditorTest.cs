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

[TestedType(typeof(FTPSettingsCustomsRegistryItemEditor))]
sealed class FTPSettingsCustomsRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		=> new FTPSettingsCustomsRegistryItem(name: "",
			caption: null,
			category: null,
			hint: null,
			storage: RegistryStorageFlags.System,
			options: RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
			defaultValue: new());

	protected override RegistryItemEditor GetEditor()
		=> new FTPSettingsCustomsRegistryItemEditor(dataType: new FTPSettingsCustomsRegistryDataType(),
			fallbackLevel: new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty),
			factory: new BusinessObjectFactory());

	protected override Type GetExpectedEditorPaneType() => typeof(FTPSettingsCustomsRegistryItemControl);

	protected override object[] GetValidRegistryValues() =>
		[
			new FTPSettingsCustomsRegistry
			{
				Url = "ftp.ec.evry.com/",
				Port = "21",
				SendToCustomFolder = "in/",
				ReceiveFromCustomFolder = "out/",
				Username = "admin",
				Password = "password",
			}
		];

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		if (editorPane is FTPSettingsCustomsRegistryItemControl itemControl)
		{
			return !(itemControl.UserNameTextBox.ReadOnly
				&& itemControl.PasswordTextBox.ReadOnly
				&& itemControl.ViewButton.ReadOnly
				&& itemControl.UrlAddressTextBox.ReadOnly
				&& itemControl.PortTextBox.ReadOnly
				&& itemControl.SendToCustomFolderTextBox.ReadOnly
				&& itemControl.ReceiveFromCustomFolderTextBox.ReadOnly);
		}

		return false;
	}

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
}
