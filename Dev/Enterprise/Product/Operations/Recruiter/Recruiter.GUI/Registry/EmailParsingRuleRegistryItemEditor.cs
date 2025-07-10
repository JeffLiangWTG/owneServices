using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.GUI
{
	public class EmailParsingRuleRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public EmailParsingRuleRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new EmailParsingRuleControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
