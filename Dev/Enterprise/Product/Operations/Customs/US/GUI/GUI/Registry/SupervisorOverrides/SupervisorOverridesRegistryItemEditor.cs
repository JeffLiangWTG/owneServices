using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.GUI
{
	public class SupervisorOverridesRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public SupervisorOverridesRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected sealed override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new SupervisorOverridesControl();
		}

		protected sealed override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
