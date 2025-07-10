using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.GUI
{
	public class NCTSPhase5CredentialsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public NCTSPhase5CredentialsRegistryItemEditor(Integration.IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new NCTSPhase5CredentialsRegistryItemUserControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
