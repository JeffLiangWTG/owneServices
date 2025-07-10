using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ExportProductAMSEPAUserControl : ZUserControl
	{
		public ExportProductAMSEPAUserControl()
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
