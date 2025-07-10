using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class ComplianceReportsSetupCategoryRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ComplianceReportsSetupCategoryRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{ }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ComplianceReportsSetupCategoryControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
