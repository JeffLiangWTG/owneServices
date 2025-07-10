using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.GUI
{
	public class UCMEDIMessageTestTypesRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public UCMEDIMessageTestTypesRegistryItemEditor(UCMEDIMessageTestTypeRegistryItemDataType dataType, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
			: base(dataType, currentFallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new UCMEDIMessageTestTypeUserControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
