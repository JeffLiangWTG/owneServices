using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class StmFeatureTestForm : ZForm
	{
		public StmFeatureTestForm()
		{
			InitializeComponent();
			InitializeCustomBehaviour();
		}

		public StmFeatureTestForm(StmFeatureTest stmFeatureTest) : base(stmFeatureTest)
		{
			InitializeComponent();
			InitializeCustomBehaviour();
		}

		public void InitializeCustomBehaviour()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtons);
		}
	}
}
