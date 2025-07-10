using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class ShipmentUserControl : ZUserControl
	{
		public ShipmentUserControl()
		{
			InitializeComponent();
		}

		#region OnCurrentDataItemChanged

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var currentShipment = CurrentDataItem as Shipment;
			if (shipment != currentShipment)
			{
				if (shipment != null)
				{
					shipment.B0_ShipmentTypeInfo.ValueChanged -= OnShipmentTypeValueChanged;
					shipment.Factory.Saving -= Factory_Saving;
				}
				shipment = currentShipment;
				if (shipment != null)
				{
					shipment.B0_ShipmentTypeInfo.ValueChanged += OnShipmentTypeValueChanged;
					shipment.Factory.Saving += Factory_Saving;
				}
				DropInBondUserControlIfRequired();
				ChangeInBondTabPageVisibility();
			}
		}

		void OnShipmentTypeValueChanged(object sender, EventArgs e)
		{
			ChangeInBondTabPageVisibility();
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if ((shipment == null || shipment.B0_ShipmentType != ShipmentTypes.Codes.Inbond))
			{
				DropInBondUserControlIfRequired();
			}
		}

		#endregion

		#region InBondTabPage Visibility

		void ChangeInBondTabPageVisibility()
		{
			var newInBondTabVisible = shipment != null && shipment.B0_ShipmentType == ShipmentTypes.Codes.Inbond;
			if (newInBondTabVisible && InBondUserControl == null)
			{
				InitializeInBondTabPage();
			}
			InBondTabPage.TabVisible = newInBondTabVisible;
		}

		void InitializeInBondTabPage()
		{
			InBondUserControl = new InBondUserControl();
			ShipmentTabControl.SuspendLayout();
			InBondTabPage.SuspendLayout();
			InBondUserControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			InBondUserControl.SetDataBinding(shipment.InBond, "");
			InBondUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3);
			InBondUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 2000);
			InBondUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 0);
			InBondUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 317);
			InBondUserControl.TabIndex = 0;
			InBondTabPage.Controls.Add(InBondUserControl);
			ShipmentTabControl.ResumeLayout(false);
			InBondTabPage.ResumeLayout(false);
		}

		void DropInBondUserControlIfRequired()
		{
			if (InBondUserControl != null)
			{
				ShipmentTabControl.SuspendLayout();
				InBondUserControl.Dispose();
				InBondTabPage.Controls.Remove(InBondUserControl);
				InBondUserControl = null;
				ShipmentTabControl.ResumeLayout(false);
			}
		}

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && shipment != null)
			{
				shipment.B0_ShipmentTypeInfo.ValueChanged -= OnShipmentTypeValueChanged;
				shipment.Factory.Saving -= Factory_Saving;
			}
			base.Dispose(disposing);
		}

		Shipment shipment;

		#endregion
	}
}
