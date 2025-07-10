using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruitment.Registry
{
	public class WorkItemTemplatePropertiesRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public WorkItemTemplatePropertiesRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{ }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new WorkItemTemplatePropertiesControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
