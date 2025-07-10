using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class MDMSupportCertificateRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		readonly MDMSupportCertificateRegistryDataType dataType;

		public MDMSupportCertificateRegistryEditor(MDMSupportCertificateRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
			this.dataType = dataType;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new MDMSupportCertificateUserControl(dataType);

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
