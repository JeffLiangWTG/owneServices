using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class MO3UserControl : ZUserControl, IAMSControlIdentity
	{
		public MO3UserControl()
		{
			InitializeComponent();
		}

		string IAMSControlIdentity.IdentityCode => AMSProgramList.Codes.MO3;
	}
}
