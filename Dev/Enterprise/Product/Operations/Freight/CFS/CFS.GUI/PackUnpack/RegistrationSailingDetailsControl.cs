using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class RegistrationSailingDetailsControl : ZUserControl
	{
		public RegistrationSailingDetailsControl()
		{
			InitializeComponent();
			HookEvents();
		}

		#region Binding

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				UnhookEvents();
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				HookEvents();
				SetTransportModeControlState();
				SetSelectSailingButtonState();
			}
		}

		CFSContainer CFSContainer
		{
			get { return (CFSContainer)CurrentDataItem; }
		}

		#endregion

		#region Form State

		void SetSelectSailingButtonState()
		{
			SelectSailingButton.Enabled = CFSContainer.JC_JK.IsEmpty;
			SetSailingButtonText();
		}

		void HookEvents()
		{
			if (CFSContainer != null)
			{
				CFSContainer.JC_TransportModeInfo.ValueChanged += new EventHandler(JC_TransportModeInfo_ValueChanged);
				CFSContainer.JC_JKInfo.ValueChanged += new EventHandler(SetSelectSailingButtonState);
				CFSContainer.JC_JXInfo.ValueChanged += new EventHandler(SetSelectSailingButtonState);
			}
		}

		void UnhookEvents()
		{
			if (CFSContainer != null)
			{
				CFSContainer.JC_TransportModeInfo.ValueChanged -= new EventHandler(JC_TransportModeInfo_ValueChanged);
				CFSContainer.JC_JKInfo.ValueChanged -= new EventHandler(SetSelectSailingButtonState);
				CFSContainer.JC_JXInfo.ValueChanged -= new EventHandler(SetSelectSailingButtonState);
			}
		}

		void JC_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetTransportModeControlState();
		}

		void SetSelectSailingButtonState(object sender, EventArgs e)
		{
			SetSelectSailingButtonState();
		}

		void SelectSailingButton_Click(object sender, EventArgs e)
		{
			if (CFSContainer.JC_JX.IsEmpty)
			{
				if (ParentForm is ZForm)
				{
					SailingIFindBox helper = new SailingIFindBox(CFSContainer, (ZForm)ParentForm);
					helper.ShowModuleFromContainer(CFSContainer.JC_TransportMode);
				}
			}
			else
			{
				CFSContainer.JC_JX = ZGuid.Empty;
			}
		}

		void SetTransportModeControlState()
		{
			ZString journeyDetailsResString = Res.GetString("71A7CF26-6D71-4230-9B08-BD48624F1E2D", "Journey Details");

			if (CFSContainer.JC_TransportMode == Constants.TransportModes.Air)
			{
				JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("46F7EABF-0D45-4575-9104-F18B6D243864", "Flight");
				VesselCodeFindBox.Visible = false;
				SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("8D7EACD3-E617-405b-9069-F254AEF68E04", "Flight Details");
			}
			else if (CFSContainer.JC_TransportMode == Constants.TransportModes.Rail)
			{
				JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("DD64E5EC-6E38-4c8f-9CEF-4A82C5FFE405", "Jrny. No.");
				VesselCodeFindBox.Visible = true;
				VesselCodeFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("2DF9993B-F445-4993-B870-6241CF9F4CAB", "Journey");
				SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = journeyDetailsResString;
			}
			else if (CFSContainer.JC_TransportMode == Constants.TransportModes.Road)
			{
				JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("ABC6FAB1-752E-4086-8B60-AED9152160E2", "Truck");
				VesselCodeFindBox.Visible = false;
				SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = journeyDetailsResString;
			}
			else
			{
				JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("9CE5A69C-EFED-4bab-86C3-2C8D79767303", "Voyage");
				VesselCodeFindBox.Visible = true;
				VesselCodeFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("A77BCA20-0768-41a2-9636-70F791E3298E", "Vessel");
				SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("DF4FFFA7-3D17-4d5f-B6FF-F79BAAED362F", "Sailing Details");
			}
			SetSailingButtonText();
		}

		void SetSailingButtonText()
		{
			ZString selectJourneyScheduleResString = Res.GetString("2524BBD8-502F-4632-A769-74B377D144E3", "Select Journey Schedule");

			if (!CFSContainer.JC_JK.IsEmpty || !CFSContainer.JC_JX.IsEmpty)
			{
				SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("DCA391A2-7F14-49b8-9397-E01F4B7B8EFF", "Remove Schedule");
			}
			else if (CFSContainer.JC_TransportMode == Constants.TransportModes.Air)
			{
				SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("04739D35-B9A4-4e0e-B73B-3C4302CC55DF", "Select Flight Schedule");
			}
			else if (CFSContainer.JC_TransportMode == Constants.TransportModes.Rail)
			{
				SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption = selectJourneyScheduleResString;
			}
			else if (CFSContainer.JC_TransportMode == Constants.TransportModes.Road)
			{
				SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption = selectJourneyScheduleResString;
			}
			else
			{
				SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("7B0764ED-A996-46e9-9A45-DC0B464ACBAD", "Select Sailing Schedule");
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			UnhookEvents();
			base.Dispose(disposing);
		}

		#endregion
	}
}

