using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class ConsolidatedTransportBookingForm : ZTemplateForm, INotifications
	{
		public ConsolidatedTransportBookingForm(CommonConsolidatedTransportBooking booking)
			: base(booking)
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		#region INotifications

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification.Message);
		}

		#endregion

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			ShowShipmentConfirmationsObsoleteWarning();
		}

		#region Warning Message

		void ShowShipmentConfirmationsObsoleteWarning()
		{
			if (!IsShipmentConfirmationsObsoleteWarningPrompted)
			{
				if (!Globals.IsTest)
				{
					var message = Res.GetString("45c8d1c4-958e-441a-9bdc-1c01daf9af07", @"This feature has been superseded by the Transport Bookings module and will become obsolete in the future system releases.
The information recorded under the Confirmations tab will become read only and retained for historical purposes.

For more information on Transport Bookings click on the link below or press OK to continue.");

					var messageLink = "https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#item=A338779E-F019-4C36-8A01-E9FAD8385062&video=11582944,5a806e6b150b0b065b32750bd332e6b4";

					var messageBox = new ZMessageBoxWithCheckbox(message, (NoResString)"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, messageLink, null);
					messageBox.SetDontAskMeAgainCheckBoxVisibility(false);
					ZFormModaliser.ShowDialogAndDispose(messageBox);
				}
				IsShipmentConfirmationsObsoleteWarningPrompted = true;
			}
		}

#if DEBUG
		internal
#endif
		bool IsShipmentConfirmationsObsoleteWarningPrompted
		{ set; get; }

		#endregion

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.ConsolidatedTransportBookingUserControl = new ConsolidatedTransportBookingControl();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.ConsolidatedTransportBookingUserControl);
			//
			// ConsolidatedTransportBookingUserControl
			//
			this.BindingSource.SetBindingMember(this.ConsolidatedTransportBookingUserControl, ".");
			this.ConsolidatedTransportBookingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolidatedTransportBookingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.ConsolidatedTransportBookingUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 571);
			this.ConsolidatedTransportBookingUserControl.Name = "ConsolidatedTransportBookingUserControl";
			this.ConsolidatedTransportBookingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(962, 601);
			this.ConsolidatedTransportBookingUserControl.TabIndex = 0;
			this.MainTabPage.ResumeLayout(true);
		}
	}
}
