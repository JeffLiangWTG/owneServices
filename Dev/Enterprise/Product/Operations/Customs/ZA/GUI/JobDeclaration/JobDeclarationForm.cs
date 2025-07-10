using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.GUI
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

		public new CustomsBrokerageUserControl CustomsBrokerageUserControl => (CustomsBrokerageUserControl)base.CustomsBrokerageUserControl;

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		public override int RoutingTabIndex => 2;

		protected override IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
		{
			foreach (var strategy in base.GetPreSaveDialogStrategies())
			{
				yield return strategy;
			}
			yield return new DA63PreSaveDialogStrategy(Declaration);
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		protected override void AddPlugins()
		{
			base.AddPlugins();
			PlugIns.Add(ControllerIDs.DocumentVisualizer);
		}

		new JobDeclaration Declaration => base.Declaration as JobDeclaration;
	}
}
