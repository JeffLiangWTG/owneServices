using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGCommonDataForm : ZForm
	{
		public UNDGCommonDataForm(UNDGCommonData commonData)
			: base(commonData)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}
	}
}
