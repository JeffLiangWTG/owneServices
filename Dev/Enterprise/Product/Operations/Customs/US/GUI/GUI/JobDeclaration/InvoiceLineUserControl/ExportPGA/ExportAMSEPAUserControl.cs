using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportAMSEPAUserControl : ZUserControl
	{
		public ExportAMSEPAUserControl()
		{
			InitializeComponent();
		}

		internal void SetGroupBoxesVisibility(ZBool isAMSDeclared, ZBool isEPADeclared)
		{
			AMSGroupBox.Visible = isAMSDeclared;
			EPAGroupBox.Visible = isEPADeclared;
		}
	}
}
