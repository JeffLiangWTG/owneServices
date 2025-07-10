using System.Windows.Forms;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.GUI
{
	public partial class CYDYardUnitStateForm : ZTemplateForm
	{
		public CYDYardUnitStateForm(CYDYardUnitState unit) : base(unit)
		{
			InitializeComponent();
			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			zWorkflowTabPage.Initialize(YardUnitState);
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region GLOW link open in browser

		void GlowLinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/CYDYardUnitState", YardUnitState.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", YardUnitState.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		#endregion

		#region Yard Unit State

		CYDYardUnitState YardUnitState
		{
			get { return (CYDYardUnitState)DataSource; }
		}

		#endregion
	}
}
