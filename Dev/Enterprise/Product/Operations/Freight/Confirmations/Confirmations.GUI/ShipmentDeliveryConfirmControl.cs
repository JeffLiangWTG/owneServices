using System;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class ShipmentDeliveryConfirmControl : ZUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public ShipmentDeliveryConfirmControl()
		{
			InitializeComponent();

			LooseDeliveryConfirmationGrid.AllowOverlap(DeliveryConfirmControl);
			DeliveryConfirmControl.AllowOutsideOfParent();

			if (!DesignModeFinder.IsDesigning)
			{
				new CustomFieldColumnCreator().Set(PackLinesGrid, new PackLineCustomFieldsDescriptor(), "PackLine");
			}

			FreightCalculateDistanceMenuHelper.SetupCalculateDistanceContextMenu(LooseDeliveryConfirmationGrid);
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
			DeliveryConfirmsSplitContainer.Panel1Collapsed = shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery);
			DeliveryConfirmsSplitContainer.Panel2Collapsed = !DeliveryConfirmsSplitContainer.Panel1Collapsed;
		}

		#endregion

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return control == DeliveryConfirmControl && previousControl == LooseDeliveryConfirmationGrid;
		}
	}
}
