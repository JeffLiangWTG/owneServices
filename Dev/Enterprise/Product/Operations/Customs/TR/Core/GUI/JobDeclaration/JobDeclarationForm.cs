using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.GUI
{
	public partial class JobDeclarationForm : EU.GUI.JobDeclarationForm
	{
		public JobDeclarationForm()
		{
		}

		public JobDeclarationForm(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();
	}
}
