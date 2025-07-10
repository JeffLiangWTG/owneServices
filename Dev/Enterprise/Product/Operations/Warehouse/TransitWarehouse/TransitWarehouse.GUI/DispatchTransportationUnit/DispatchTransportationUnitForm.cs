using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.GUI
{
	public partial class DispatchTransportationUnitForm : ZTemplateForm
	{
		public DispatchTransportationUnitForm(WhsItemDispatchTransportationUnit dtu) : base(dtu)
		{
			InitializeComponent();

			AddPlugins();
		}

		#region Plugins

		void AddPlugins()
		{
			PlugIns.AddJobInvoicing(((IJobInvoicingPlugIn)DTU).InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.ApportionmentForTransitTransportationUnit, 2);
			zWorkflowTabPage.Initialize(DTU);
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region Open in Browser

		void GlowLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => TransitWarehouseGUIHelper.OpenInBrowser("Goto/DispatchTransportationUnit", DTU.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", DTU.PK.ToString()) });

		#endregion

		#region FormCaption

		public override string FormCaption => BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption;

		#endregion

		#region DispatchTransportationUnit

		WhsItemDispatchTransportationUnit DTU => (WhsItemDispatchTransportationUnit)DataSource;

		#endregion
	}
}
