using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class CertificateTypeRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CertificateTypeRegistryItemEditor(CertificateTypeRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new CertificateTypeRegistryItemControl();
		}

		public override void SetEditorPaneLayout(Control editorPane, int width, int height)
		{
			editorPane.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			ControlDpiScalingHelper.SetHeight(ref editorPane, height, false);
			ControlDpiScalingHelper.SetWidth(ref editorPane, width, false);
		}
	}
}
