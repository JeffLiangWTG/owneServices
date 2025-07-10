using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class QuickPODShipmentSelectionForm : ZChildForm
	{
		public QuickPODShipmentSelectionForm(QuickPODMultipleShipmentsEventArgs quickPodMultipleShipmentEventArgs)
			: base(quickPodMultipleShipmentEventArgs.Shipments)
		{
			InitializeComponent();

			msgLabel.AllowOutsideOfParent();
			QuickPodMultipleShipmentEventArgs = quickPodMultipleShipmentEventArgs;
			msgLabel.Text = Res.GetString("QuickPODShipmentSelectionForm|msgLabel", "There are multiple Shipments with house bill '{0}'. Please choose from the list below and select OK.", QuickPodMultipleShipmentEventArgs.HouseBill);
		}
		readonly QuickPODMultipleShipmentsEventArgs QuickPodMultipleShipmentEventArgs;

		public override string FormVerb
		{
			get
			{
				return ZString.Empty;
			}
		}

		void okButton_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedShipments = ShipmentsGrid.SelectedElements;

			if (selectedShipments.Length == 0)
			{
				Globals.Message.ShowWarning(Res.GetString("1e2013bc-59e0-44a0-adea-eb8138d87ca3", "Please select a Shipment."));
			}
			else if (selectedShipments.Length > 1)
			{
				Globals.Message.ShowWarning(Res.GetString("a49dbd26-f085-45e8-889f-55eb66129ee9", "Please select only one Shipment."));
			}
			else
			{
				QuickPodMultipleShipmentEventArgs.SelectedShipment = (CommonShipment)selectedShipments[0];
				Close();
			}
		}
	}
}
