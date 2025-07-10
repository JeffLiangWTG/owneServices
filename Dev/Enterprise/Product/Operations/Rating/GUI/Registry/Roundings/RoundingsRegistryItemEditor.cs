using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI
{
	public class RoundingsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public RoundingsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new RoundingsControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}

