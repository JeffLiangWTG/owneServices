using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusRulingForm : ZTemplateForm
	{
		public RefCusRulingForm(ZZRefCusRulingCombined dataSource)
			: base(dataSource)
		{
			this.SetDataBinding(dataSource, string.Empty);
			this.DataSourceType = dataSource.GetType();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		public override string FormCaption
		{
			get { return BusinessEntity?.HumanReadableName ?? Res.GetString("RefCusRulingForm|FormCaption", "Rulings"); }
		}

		protected new ZZRefCusRulingCombined BusinessEntity
		{
			get { return base.BusinessEntity as ZZRefCusRulingCombined; }
		}
	}
}
