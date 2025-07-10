using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

//using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.GUI
{
	public partial class MNRWorkOrderForm : ZTemplateForm
	{
		public MNRWorkOrderForm(MNRWorkOrderHeader workOrderHeader) : base(workOrderHeader)
		{
			InitializeComponent();
			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)workOrderHeader).InvoicingSupporter);
			zWorkflowTabPage.Initialize(workOrderHeader);
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
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/MNRWorkOrderHeader", workOrderHeader.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", workOrderHeader.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		#endregion

		#region MNRWorkOrderHeader

		MNRWorkOrderHeader workOrderHeader
		{
			get { return (MNRWorkOrderHeader)DataSource; }
		}

		#endregion
	}
}
