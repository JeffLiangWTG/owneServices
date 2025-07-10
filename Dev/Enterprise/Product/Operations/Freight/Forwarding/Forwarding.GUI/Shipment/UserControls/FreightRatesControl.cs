using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class FreightRatesControl : ZUserControl
	{
		public FreightRatesControl()
		{
			InitializeComponent();
		}

		#region Movement

		void MoveUpButton_Click(object sender, System.EventArgs e)
		{
			var error = GetErrorForMovingGateway();
			if (!error.IsEmpty)
			{
				Globals.Message.Show(error);
			}
			else
			{
				if (this.GatewaysGrid.SelectedElements.Length > 0)
				{
					var gateway = this.GatewaysGrid.SelectedElements[0] as ShipmentGateway;
					if (gateway.Shipment.Gateways.SwapGateways(gateway.JSG_Sequence, (byte)(gateway.JSG_Sequence - 1)))
					{
						this.GatewaysGrid.UnSelectAll();
						this.GatewaysGrid.Select(gateway.JSG_Sequence - 2);
					}
				}
			}
		}

		void MoveDownButton_Click(object sender, System.EventArgs e)
		{
			var error = GetErrorForMovingGateway();
			if (!error.IsEmpty)
			{
				Globals.Message.Show(error);
			}
			else
			{
				if (this.GatewaysGrid.SelectedElements.Length > 0)
				{
					var gateway = this.GatewaysGrid.SelectedElements[0] as ShipmentGateway;
					if (gateway.Shipment.Gateways.SwapGateways(gateway.JSG_Sequence, (byte)(gateway.JSG_Sequence + 1)))
					{
						this.GatewaysGrid.UnSelectAll();
						this.GatewaysGrid.Select(gateway.JSG_Sequence);
					}
				}
			}
		}

		ZString GetErrorForMovingGateway()
		{
			if (this.GatewaysGrid.SelectedRowCount == 0)
			{
				return Res.GetString("1e4e44cb-1296-478f-84a2-08adcfd2e749", "Please select a Gateway to move from the grid.");
			}
			else if (this.GatewaysGrid.SelectedRowCount > 1)
			{
				return Res.GetString("aea4cf0e-2331-4b31-a867-964f84492dcb", "Please select only one Gateway to move in the grid.");
			}

			return ZString.Empty;
		}

		#endregion

		[FormBasherTestPopupExclude]
		class MoveItemButton : ZButton
		{
		}
	}
}
