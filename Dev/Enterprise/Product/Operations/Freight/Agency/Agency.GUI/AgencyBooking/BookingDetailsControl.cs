using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BookingDetailsControl : ZUserControl
	{
		public BookingDetailsControl()
		{
			InitializeComponent();

			ConfirmButton.AllowOverlap(SailingControl);
		}

		public event EventHandler Confirm
		{
			add { ConfirmButton.Click += value; }
			remove { ConfirmButton.Click -= value; }
		}

		#region Implementation

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (Shipment != null)
			{
				UnHookShipment(Shipment);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (Shipment != null)
			{
				HookShipment(Shipment);

				if (Shipment.ReadOnly)
				{
					SetButtonsEnabled(false);
				}
			}
		}

		protected void SetButtonsEnabled(bool desiredEnabledness)
		{
			ConfirmButton.Enabled = desiredEnabledness;
			SailingControl.SetButtonsEnabled(desiredEnabledness);
		}

		void HookShipment(AgencyShipment shipment)
		{
			shipment.JS_PackingModeInfo.ValueChanged += new EventHandler(JS_PackingModeInfo_ValueChanged);
			shipment.JS_RL_NKOriginInfo.ValueChanged += JS_RL_NKOriginInfo_ValueChanged;
			shipment.JS_RL_NKDestinationInfo.ValueChanged += JS_RL_NKDestinationInfo_ValueChanged;

			JS_PackingModeInfo_ValueChanged(shipment, EventArgs.Empty);
		}

		void JS_RL_NKDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(Shipment.JS_RL_NKDestinationInfo);
		}

		void JS_RL_NKOriginInfo_ValueChanged(object sender, EventArgs e)
		{
			ConfirmReversal(Shipment.JS_RL_NKOriginInfo);
		}

		protected virtual void ConfirmReversal(ZPropertyInfo info)
		{
			CommissionReversalConfirmationHelper.ConfirmReversal(Shipment, info, new[]
							{
								Shipment.JS_RL_NKOriginInfo,
								Shipment.JS_RL_NKDestinationInfo,
								Shipment.JS_PackingModeInfo,
							});
		}

		void UnHookShipment(AgencyShipment shipment)
		{
			shipment.JS_PackingModeInfo.ValueChanged -= new EventHandler(JS_PackingModeInfo_ValueChanged);
			shipment.JS_RL_NKOriginInfo.ValueChanged -= JS_RL_NKOriginInfo_ValueChanged;
			shipment.JS_RL_NKDestinationInfo.ValueChanged -= JS_RL_NKDestinationInfo_ValueChanged;
		}

		void SetWeightAndVolumeAndPacksVisible(bool value)
		{
			JS_ActualVolumeCalcDropEdit.Visible = value;
			JS_ActualWeightCalcDropEdit.Visible = value;
			PacksCountCalcDropEdit.Visible = value;
		}

		void SetControlsBehaviorForPackingMode(string packingMode)
		{
			ShipmentContentTabControl.UpdateVisibleTabsForPackingMode(packingMode);
			SetWeightAndVolumeAndPacksVisible(packingMode != Constants.ContainerModes.FCL);

			if (packingMode != Constants.ContainerModes.RollOnRollOff)
			{
				PacksCountCalcDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("45641605-8708-4d1e-bfbc-d169ab5cbbc7", "Packs");
			}
			else
			{
				PacksCountCalcDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("c692a75e-7966-435e-87c9-c7944147a52b", "Vehicles");
			}
		}

		AgencyShipment Shipment
		{
			get { return (AgencyShipment)CurrentDataItem; }
		}

		#endregion

		#region Events

		void OrderReferencesButton_Click(object sender, EventArgs e)
		{
			if (Shipment != null)
			{
				OrderItemCollectionForm.ShowDialog(Shipment.DocsAndCartage.OrderItems);
			}
		}

		void JS_PackingModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetControlsBehaviorForPackingMode(Shipment.JS_PackingMode);
			ConfirmReversal(Shipment.JS_PackingModeInfo);
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Agency.GUI
{
	partial class BookingDetailsControl
	{
		public void ConfirmPerformClick()
		{
			ConfirmButton.PerformClick();
		}

		public List<Control> ButtonsExposedForTesting
		{
			get
			{
				var buttons = new List<Control>(this.SailingControl.AllButtonsExposedForTesting);
				buttons.Add(this.ConfirmButton);

				return buttons;
			}
		}
	}
}

#endregion



#endif
#endregion
