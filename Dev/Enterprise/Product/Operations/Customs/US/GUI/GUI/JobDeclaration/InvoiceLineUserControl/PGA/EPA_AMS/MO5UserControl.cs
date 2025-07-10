using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class MO5UserControl : ZUserControl, IAMSControlIdentity
	{
		public MO5UserControl()
		{
			InitializeComponent();
		}

		string IAMSControlIdentity.IdentityCode => AMSProgramList.Codes.MO5;
	}
}
