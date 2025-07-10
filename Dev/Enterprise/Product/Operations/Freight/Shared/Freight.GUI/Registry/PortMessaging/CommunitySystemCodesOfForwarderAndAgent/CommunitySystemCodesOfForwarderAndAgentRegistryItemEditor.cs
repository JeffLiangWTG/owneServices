using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.GUI
{
	class CommunitySystemCodesOfForwarderAndAgentRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CommunitySystemCodesOfForwarderAndAgentRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new CommunitySystemCodesOfForwarderAndAgentControl();

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
