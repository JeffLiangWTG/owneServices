using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class MO1UserControl : ZUserControl, IAMSControlIdentity
	{
		public MO1UserControl()
		{
			InitializeComponent();
		}

		string IAMSControlIdentity.IdentityCode => AMSProgramList.Codes.MO1;
	}
}
