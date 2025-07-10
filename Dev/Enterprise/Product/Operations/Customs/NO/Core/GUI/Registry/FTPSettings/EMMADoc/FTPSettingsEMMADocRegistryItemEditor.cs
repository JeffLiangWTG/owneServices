using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry.GUI;

public sealed class FTPSettingsEMMADocRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	: NonPersistentBusinessObjectBindingRegistryItemEditor(dataType, fallbackLevel, factory)
{
	protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		=> new FTPSettingsEMMADocRegistryItemControl();

	protected override EditorPaneAnchor Anchor => EditorPaneAnchor.TopLeftRight;

	protected override void EnableEditorPaneCore(Control editorPane, bool enabled)
	{
		if (editorPane is not FTPSettingsEMMADocRegistryItemControl itemControl)
		{
			return;
		}

		itemControl.UserNameTextBox.ReadOnly = !enabled;
		itemControl.PasswordTextBox.ReadOnly = !enabled;
		itemControl.ViewButton.ReadOnly = !enabled;
		itemControl.UrlAddressTextBox.ReadOnly = !enabled;
		itemControl.PortTextBox.ReadOnly = !enabled;
	}
}
