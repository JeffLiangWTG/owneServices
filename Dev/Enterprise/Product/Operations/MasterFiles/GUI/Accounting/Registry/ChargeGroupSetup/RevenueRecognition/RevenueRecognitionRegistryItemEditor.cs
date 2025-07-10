using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class RevenueRecognitionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public RevenueRecognitionRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new RevenueRecognitionControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
