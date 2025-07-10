using Enterprise.Customs.GUI;
#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Customs.TR.GUI
{
	public partial class SupportingDocumentsFieldsControl : BaseCustomsEntryUserControl
	{
		public SupportingDocumentsFieldsControl()
		{
			InitializeComponent();
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(this.CSI_StatusDropEdit);
#endif
		}
	}
}
