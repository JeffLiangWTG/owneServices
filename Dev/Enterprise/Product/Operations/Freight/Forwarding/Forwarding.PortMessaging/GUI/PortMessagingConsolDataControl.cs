using System;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	public partial class PortMessagingConsolDataControl : ZUserControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String format")]
		public PortMessagingConsolDataControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				ShipperEORITextBox.CaptionResourceString = new ResourceStringData("22D7754B-CFD4-48BC-821E-5B6BF1EA31C2", string.Format("{0} EORI", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value));
			}
		}

		PortMessagingData PortMessagingData => CurrentDataItem as PortMessagingData;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetEORIControlsVisibility();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (PortMessagingData?.Consol != null)
			{
				PortMessagingData.Consol.JK_AgentTypeInfo.ValueChanged += JK_AgentTypeInfo_ValueChanged;
				ChangeLabelAndCaptionResourceString();
			}
		}

		void JK_AgentTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeLabelAndCaptionResourceString();
		}

		void ChangeLabelAndCaptionResourceString()
		{
			var consol = PortMessagingData?.Consol;
			if (consol != null)
			{
				var isCoLoad = consol.IsCoLoad;
				var shippingLineTextBoxCaption = isCoLoad
					? Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("cf1313cc-3ce8-4e8d-a791-7827fd57d85a", "Co-Load With")
					: Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("0ba90fc7-2a8e-4644-9143-58e7a3396e4d", "Shipping Line");

				ShippingLineTextBox.GetExtension<ILabelCaptionRenderer>().Caption = shippingLineTextBoxCaption.Caption;
				ShippingLineTextBox.CaptionResourceString = shippingLineTextBoxCaption;

				var carrierBookingRefTextBoxCaption = isCoLoad
									? Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("9a6d4367-8d7c-4342-a4a0-8279d4239ae6", "Co-Load Bkg. Ref.")
									: Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("7284604b-d56f-44b2-b513-a93af9e463a1", "Booking Ref.");
				CarrierBookingRefTextBox.GetExtension<ILabelCaptionRenderer>().Caption = carrierBookingRefTextBoxCaption.Caption;
				CarrierBookingRefTextBox.CaptionResourceString = carrierBookingRefTextBoxCaption;

				var billNoTextBoxCaption = isCoLoad
									? Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("9ce7a5a9-714d-4641-b9de-3273e769c68f", "Co-Load MBL")
									: Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("83b1e99f-5942-47a3-b8df-3658513b77d1", "Bill No");
				BillNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption = billNoTextBoxCaption.Caption;
				BillNoTextBox.CaptionResourceString = billNoTextBoxCaption;
			}
		}

		void SetEORIControlsVisibility()
		{
			ShipperEORITextBox.Visible = PortMessagingHelper.IsEORIAndLRNEffectiveDate();
			AgentEORITextBox.Visible = PortMessagingHelper.IsEORIAndLRNEffectiveDate();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (PortMessagingData?.Consol != null)
				{
					PortMessagingData.Consol.JK_AgentTypeInfo.ValueChanged -= JK_AgentTypeInfo_ValueChanged;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
