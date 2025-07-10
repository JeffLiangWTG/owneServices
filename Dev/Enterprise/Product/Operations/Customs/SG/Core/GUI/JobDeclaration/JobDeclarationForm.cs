using Enterprise.Customs.GUI;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class JobDeclarationForm : BaseJobDeclarationForm
	{
		public JobDeclarationForm()
			: base()
		{
		}

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		public override int RoutingTabIndex => 2;

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

		public override string FormCaption
		{
			get
			{
				string result = base.FormCaption;
				if (DisplayMode != ODisplayMode.New && Declaration != null && !Declaration.JE_MessageType.IsEmpty)
				{
					result += " - " + Declaration.JE_MessageType + " - " + Declaration.JE_MessageSubType;
				}

				return result;
			}
		}

		public override bool IsResizableByTabPageAllowed => true;
	}
}
