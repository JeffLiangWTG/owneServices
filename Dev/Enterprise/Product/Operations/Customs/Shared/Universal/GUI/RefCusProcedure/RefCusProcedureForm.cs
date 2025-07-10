using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusProcedureForm : ZForm
	{
		public RefCusProcedureForm(RefCusProcedure dataSource) : base(dataSource)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl2);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return BusinessEntity?.HumanReadableName ?? Res.GetString("RefCusProcedureForm|FormCaption", "Global CPCs"); }
		}

		protected override void SaveToRecentItems()
		{
		}

		protected new RefCusProcedure BusinessEntity
		{
			get { return base.BusinessEntity as RefCusProcedure; }
		}
	}
}
