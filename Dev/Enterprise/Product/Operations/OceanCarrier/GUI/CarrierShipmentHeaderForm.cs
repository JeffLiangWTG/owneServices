using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.OceanCarrier.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.OceanCarrier.GUI
{
	public sealed partial class CarrierShipmentHeaderForm : ZTemplateForm
	{
		public CarrierShipmentHeaderForm(CarrierShipmentHeader carrierShipmentHeader)
			: base(carrierShipmentHeader)
		{
			InitializeComponent();

			if (!this.IsDesignMode())
			{
				PlugIns.AddJobInvoicing(carrierShipmentHeader.InvoicingPlugIn.InvoicingSupporter);
				WorkflowTabPage.Initialize(carrierShipmentHeader);
			}
		}

		CarrierShipmentHeader CarrierShipmentHeader => carrierShipmentHeader ?? (carrierShipmentHeader = (CarrierShipmentHeader)DataSource);
		CarrierShipmentHeader carrierShipmentHeader;

		#region Overrides

		public override string FormCaption => BusinessEntity?.HumanReadableName ?? base.FormCaption;
		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region Open in Browser

		void OnGlowLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => OceanCarrierGlowHelper.OpenInBrowser(CarrierShipmentHeader);

		#endregion
	}
}
