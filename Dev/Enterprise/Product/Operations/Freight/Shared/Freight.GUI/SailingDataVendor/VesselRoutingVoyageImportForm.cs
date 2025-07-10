using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.SailingDataVendor.GUI
{
	public partial class VesselRoutingVoyageImportForm : ZChildForm, INotifications, INotificationSubscriberQueryUser
	{
		public VesselRoutingVoyageImportForm()
		{
			ProgressTextBox.Font = new Font(FontFamily.GenericMonospace, 10);
			KeepTickedPortPairsCheckBox.Checked = false;
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public bool KeepTickedPortPairs
		{
			get { return KeepTickedPortPairsCheckBox.Checked; }
			set { KeepTickedPortPairsCheckBox.Checked = value; }
		}

		public void NotifyImportCompleted()
		{
			ProgressBar.Style = ProgressBarStyle.Blocks;
			CloseButton.Enabled = true;

			if (Notifications.HasErrors)
			{
				KeepTickedPortPairsCheckBox.Checked = true;
				ProgressTextBox.ForeColor = Color.Red;
				ImportStatusLabel.ForeColor = Color.Red;
				ImportStatusLabel.Text = Res.GetString("Freight|VesselRoutingVoyageImportForm|ImportCompletedWithErrors", "Import completed with errors");
			}
			else
			{
				KeepTickedPortPairsCheckBox.Enabled = true;
				ImportStatusLabel.ForeColor = Color.DarkGreen;
				ImportStatusLabel.Text = Res.GetString("Freight|VesselRoutingVoyageImportForm|ImportCompletedSuccessfully", "Import completed successfully");
			}
		}

		#region Event Handlers

		protected override void OnLoad(EventArgs e)
		{
			ProgressBar.Enabled = true;
			CloseButton.Enabled = false;
			KeepTickedPortPairsCheckBox.Enabled = false;

			ProgressTextBox.Text = "\r\n";
			ImportStatusLabel.Text = Res.GetString("Freight|VesselRoutingVoyageImportForm|ImportingSailingSchedules", "Importing Sailing Schedules...");
			base.OnLoad(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			if (!CloseButton.Enabled)
			{
				e.Cancel = true;
			}
		}

		void OnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region INotifications Members

		void INotifications.Add(INotification @event)
		{
			Notify(@event);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Notify(INotification @event)
		{
			Notifications.Notify(@event);
			ProgressTextBox.AppendText(@event.Message + System.Environment.NewLine);
			ProgressTextBox.ScrollToCaret();
			Application.DoEvents(); // update the progress bar
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			new VesselRoutingVoyageImportGuiHelper().QueryUser(e);
		}

		readonly NotificationBuffer Notifications = new NotificationBuffer();

		#endregion
	}
}
