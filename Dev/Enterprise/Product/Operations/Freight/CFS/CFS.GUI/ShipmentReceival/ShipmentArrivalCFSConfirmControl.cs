using System;

using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class ShipmentArrivalCFSConfirmControl : ZUserControl
	{
		public ShipmentArrivalCFSConfirmControl()
		{
			InitializeComponent();
		}

		CommonShipment shipment;

		#region Overrides

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (shipment != null)
			{
				Unhook();
			}

			shipment = (CommonShipment)dataSource;

			if (shipment != null)
			{
				Hook();
				SetupLayout();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Hook/Unhook

		void Hook()
		{
			shipment.JS_PackingModeInfo.ValueChanged += new EventHandler(JS_PackingModeInfo_ValueChanged);
		}

		void Unhook()
		{
			shipment.JS_PackingModeInfo.ValueChanged -= new EventHandler(JS_PackingModeInfo_ValueChanged);
		}

		void JS_PackingModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupLayout();
		}

		void SetupLayout()
		{
			DeliveryCFSConfirmsSplitContainer.Panel1Collapsed = shipment.JS_PackingMode == Constants.ContainerModes.FCL;
			DeliveryCFSConfirmsSplitContainer.Panel2Collapsed = !DeliveryCFSConfirmsSplitContainer.Panel1Collapsed;
		}

		#endregion
	}
}
