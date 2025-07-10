using System;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class ShipmentPickupConfirmControl : ZUserControl
	{
		public ShipmentPickupConfirmControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				new CustomFieldColumnCreator().Set(PackLinesGrid, new PackLineCustomFieldsDescriptor(), "PackLine");
			}

			FreightCalculateDistanceMenuHelper.SetupCalculateDistanceContextMenu(LoosePickupConfirmationGrid);

			ConfirmDetailsControl.PickupDateEdit.CaptionResourceString = Res.GetData("ShipmentPickupConfirmControl|c8d615eb-884a-44af-92ac-01930e5b6b25", "Picked Up At", "Pickup Time");
			ContainersOriginConfirmControl.PickupDateEdit.CaptionResourceString = Res.GetData("ShipmentPickupConfirmControl|c8d615eb-884a-44af-92ac-01930e5b6b25", "Picked Up At", "Pickup Time");

			LoosePickupConfirmationGrid.AllowOverlap(PickupConfirmPanel);
			PickupConfirmPanel.AllowOutsideOfParent();
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
			PickupConfirmsSplitContainer.Panel1Collapsed = shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup);
			PickupConfirmsSplitContainer.Panel2Collapsed = !PickupConfirmsSplitContainer.Panel1Collapsed;
		}

		#endregion
	}
}
