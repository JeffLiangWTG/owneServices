using System.Windows.Forms;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.GUI
{
	public partial class CYDPickupHeaderForm : ZTemplateForm
	{
		public CYDPickupHeaderForm(CYDPickupHeader pickupHeader) : base(pickupHeader)
		{
			InitializeComponent();
			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			zWorkflowTabPage.Initialize(pickupHeader);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
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
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/CYDPickupHeader", pickupHeader.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", pickupHeader.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		#endregion

		#region CYDPickupHeader

		CYDPickupHeader pickupHeader
		{
			get { return (CYDPickupHeader)DataSource; }
		}

		#endregion
	}
}
