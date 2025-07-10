using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	[SuppressRebindBasherTest]
	public partial class PeriodicStatementForm : ZTemplateForm, IPostingButtonsProvider
	{
		public PeriodicStatementForm()
		{
		}

		public PeriodicStatementForm(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
			this.statementHeader = statementHeader;
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			WorkflowTabPage.Initialize(statementHeader);
		}

		readonly CusStatementHeader statementHeader;

		public override string FormCaption
		{
			get { return "Periodic Monthly Statement - " + statementHeader.B2_StatementNumber; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			statementHeaderDetailsUserControl1.ChangeCheckNoControlVisibility(false);
		}

		internal ZTabControl MainTabControlForTesting => MainTabControl;

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}
	}
}
