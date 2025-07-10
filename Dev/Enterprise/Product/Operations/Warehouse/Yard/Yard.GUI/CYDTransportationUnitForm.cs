
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.GUI
{
	public partial class CYDTransportationUnitForm : ZTemplateForm
	{
		public CYDTransportationUnitForm(CYDTransportationUnit unit) : base(unit)
		{
			InitializeComponent();

			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			zWorkflowTabPage.Initialize(TransportationUnit);
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)TransportationUnit).InvoicingSupporter);
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
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/CYDTransportationUnit", TransportationUnit.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", TransportationUnit.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		#endregion

		#region Transportation Unit

		CYDTransportationUnit TransportationUnit
		{
			get { return (CYDTransportationUnit)DataSource; }
		}

		#endregion
	}
}
