using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.GUI
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

		protected override IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
		{
			foreach (var strategy in base.GetPreSaveDialogStrategies())
			{
				yield return strategy;
			}

			yield return new CalculateDutyStrategy((JobDeclaration)Declaration);
		}

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

		public override int RoutingTabIndex => 2;
	}
}
