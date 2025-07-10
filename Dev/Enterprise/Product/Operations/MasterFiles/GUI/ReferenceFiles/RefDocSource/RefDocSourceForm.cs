using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefDocSourceForm : ZForm
	{
		public RefDocSourceForm(RefDocSource docSource)
			: base(docSource)
		{
			InitializeComponent();

			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		public new RefDocSource BusinessEntity
		{
			get { return (RefDocSource)base.BusinessEntity; }
		}
	}
}
