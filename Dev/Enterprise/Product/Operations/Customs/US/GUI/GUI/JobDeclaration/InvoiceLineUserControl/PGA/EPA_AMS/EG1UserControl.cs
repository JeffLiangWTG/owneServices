using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class EG1UserControl : ZUserControl, IAMSControlIdentity
	{
		public EG1UserControl()
		{
			InitializeComponent();
		}

		string IAMSControlIdentity.IdentityCode => AMSProgramList.Codes.EG1;
	}
}
