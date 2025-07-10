using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefDocOrgCusCodeForm : ZForm
	{
		public RefDocOrgCusCodeForm(RefDocOrgCusCode refDocOrgCusCode)
			: base(refDocOrgCusCode)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}
	}
}
