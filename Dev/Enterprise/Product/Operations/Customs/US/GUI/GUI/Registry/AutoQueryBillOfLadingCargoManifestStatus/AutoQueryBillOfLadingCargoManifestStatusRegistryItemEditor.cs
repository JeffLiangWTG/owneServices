using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public class AutoQueryBillOfLadingCargoManifestStatusRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AutoQueryBillOfLadingCargoManifestStatusRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new AutoQueryBillOfLadingCargoManifestStatusUserControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeft; }
		}
	}
}
