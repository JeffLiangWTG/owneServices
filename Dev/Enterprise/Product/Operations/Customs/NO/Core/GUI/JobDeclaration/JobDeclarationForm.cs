using System;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.GUI
{
	public partial class JobDeclarationForm : BaseJobDeclarationForm
	{
		[Obsolete("Do not call. Only for designer use.")]
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

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();
	}
}
