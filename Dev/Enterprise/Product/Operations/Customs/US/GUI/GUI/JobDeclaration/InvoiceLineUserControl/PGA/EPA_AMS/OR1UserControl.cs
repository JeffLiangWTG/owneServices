using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class OR1UserControl : ZUserControl, IAMSControlIdentity
	{
		public OR1UserControl()
		{
			InitializeComponent();
		}

		string IAMSControlIdentity.IdentityCode => AMSProgramList.Codes.OR1;
	}
}
