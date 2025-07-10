using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.GUI
{
	public partial class ReceiveTransportationUnitForm : ZTemplateForm
	{
		public ReceiveTransportationUnitForm(WhsItemReceiveTransportationUnit rtu) : base(rtu)
		{
			InitializeComponent();

			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)RTU).InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.ApportionmentForTransitTransportationUnit, 2);
			zWorkflowTabPage.Initialize(RTU);
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region Open in Browser

		void GlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			TransitWarehouseGUIHelper.OpenInBrowser("Goto/ReceiveTransportationUnit", RTU.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", RTU.PK.ToString()) });
		}

		#endregion

		#region FormCaption

		public override string FormCaption => BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption;

		#endregion

		#region ReceiveTransportationUnit

		WhsItemReceiveTransportationUnit RTU => (WhsItemReceiveTransportationUnit)DataSource;

		#endregion
	}
}
