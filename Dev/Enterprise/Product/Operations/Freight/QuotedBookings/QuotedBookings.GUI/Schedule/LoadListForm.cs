using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.QuotedBookings.GUI
{
	/// <summary>
	/// Summary description for LoadListForm.
	/// </summary>
	public partial class LoadListForm : ZForm
	{
		public LoadListForm(JobSailing sailing) : base(sailing)
		{
			InitializeComponent();
			fSailing = sailing;
			ZFormPostingButtonsStrategy.SetupPosting(this, LoadListPostingButtonsUserControl);
			SetUpContextMenus();
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("4E19A7F5-494A-40AE-BABC-C41199713A90", "Combine Shipment"), this.CombineShipmentMenuItem_Click);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected JobSailing fSailing;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			string journeySummary = Res.GetString("LoadListForm|SailingSummaryGroupBox|JourneySummary", "Journey Summary");

			if (fSailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Air)
			{
				JX_JV_NKVesselBoundTextBox.Visible = false;
				VesselNameLabel.Visible = false;
				VoyageLabel.Text = Res.GetString("LoadListForm|VoyageLabel|FlightNo", "Flight No.:");
				SailingSummaryGroupBox.Text = Res.GetString("LoadListForm|SailingSummaryGroupBox|FlightSummary", "Flight Summary");
				JX_CTOCutOffDateEdit.Visible = false;
			}
			else if (fSailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Road)
			{
				JX_JV_NKVesselBoundTextBox.Visible = false;
				VesselNameLabel.Visible = false;
				VoyageLabel.Text = Res.GetString("LoadListForm|VoyageLabel|TruckRef", "Truck Ref.:");
				SailingSummaryGroupBox.Text = journeySummary;
			}
			else if (fSailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Rail)
			{
				VesselNameLabel.Text = Res.GetString("LoadListForm|VesselNameLabel|Journey", "Journey:");
				VoyageLabel.Text = Res.GetString("LoadListForm|VoyageLabel|JourneyNo", "Journey No.:");
				SailingSummaryGroupBox.Text = journeySummary;
			}
			else
			{
				JX_JV_NKVesselBoundTextBox.Visible = true;
				VesselNameLabel.Visible = true;
				VoyageLabel.Text = Res.GetString("LoadListForm|VoyageLabel|VoyageNo", "Voyage No.:");
				SailingSummaryGroupBox.Text = Res.GetString("LoadListForm|SailingSummaryGroupBox|SailingSummary", "Sailing Summary");
				JX_CTOCutOffDateEdit.Visible = true;
			}
			DisableNewAction();
		}

		protected void SetUpContextMenus()
		{
			ContextMenu loadListMenu = new ContextMenu();

			MenuItem combine = new ZMenuItem(ResString.GetMultilingualString("QuotedBooking.Context.CombineBooking", "Combine Booking"));
			combine.Click += new EventHandler(CombineShipmentMenuItem_Click);
			ShipmentsGrid.ContextMenu.MenuItems.Add(0, combine);

			MenuItem divider = new ZMenuItem("-");
			ShipmentsGrid.ContextMenu.MenuItems.Add(1, divider);
		}

		private CombineShipmentsHelper Helper;

		private void CombineShipmentMenuItem_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedBookings = ShipmentsGrid.SelectedElements;
			int maximumAllowedCombines = 5;

			if (!fSailing.HasChanges && !Array.Exists(selectedBookings, x => x.HasChanges))
			{
				if (selectedBookings.Length == 0)
				{
					Globals.Message.Show(Res.GetString("7735c034-4d86-4f86-a5ef-3eb8d98bb177", "Please select bookings to combine."),
						Res.GetString("3e7d83d5-a0cc-4c4a-84c4-84f9761083d3", "Select Booking"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else if (selectedBookings.Length > maximumAllowedCombines)
				{
					Globals.Message.Show(Res.GetString("b91fd057-e69c-4339-a547-89f38d72f219", "Please select no more than {0} bookings to combine. Combining more at the same time can put unnecessary strain on the system.", maximumAllowedCombines),
						Res.GetString("3e7d83d5-a0cc-4c4a-84c4-84f9761083d3", "Select Booking"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					StringBuilder warningMessage = new StringBuilder();
					List<String> noMatchingBookings = new List<String>();
					List<String> cannotCombineBookings = new List<String>();

					foreach (CommonShipment booking in selectedBookings)
					{
						Helper = new CombineShipmentsHelper(booking, booking.Sailing);
						if (Helper.ShipmentCanBeCombined())
						{
							if (Helper.ShipmentHasSiblings)
							{
								CombineShipmentsForm combineForm = new CombineShipmentsForm(Helper);
								ZFormModaliser.Show(combineForm, this);
							}
							else
							{
								noMatchingBookings.Add(booking.HumanReadableName);
							}
						}
						else
						{
							cannotCombineBookings.Add(booking.HumanReadableName);
						}
					}

					if (noMatchingBookings.Count > 0)
					{
						string noMatchingBookingsString = "- " + String.Join(System.Environment.NewLine + "- ", noMatchingBookings.ToArray());
						warningMessage.Append(System.Environment.NewLine);
						warningMessage.AppendLine(Res.GetString("cbb1b53d-44ce-4841-9d91-f55b90d9e71b", "These booking do not have any matching bookings to combine with.\r\n{0}\r\nEither there are no bookings with the same Consignee/Consignor, or bookings with the same Consignee/Consignor are split, part of a Buyer's Consol or Co-load, or have already been invoiced.", noMatchingBookings));
					}

					if (cannotCombineBookings.Count > 0)
					{
						string cannotCombineBookingsString = "- " + String.Join(System.Environment.NewLine + "- ", cannotCombineBookings.ToArray());
						warningMessage.Append(System.Environment.NewLine);
						warningMessage.AppendLine(Res.GetString("a37c59ce-4adf-4d42-a5e0-e704039d0086", "These booking cannot be combined with other bookings.\r\n{0}\r\nThey may be split, part of a Buyer's Consol or Co-load, may have already been invoiced, or may not contain enough details (Consignee/Consignor).", cannotCombineBookings));
					}

					if (warningMessage.Length > 0)
					{
						warningMessage.Insert(0, Res.GetString("a559d71d-a9e7-45e2-a61b-311ca763b24c", "Can not combine all selected shipments:") + System.Environment.NewLine);
						Globals.Message.Show(warningMessage.ToString().Trim(),
							Res.GetString("e5444748-5f32-4d0d-812b-57243f217959", "Combine Booking"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("8306e9e2-2e9a-4109-af5f-c7386a5e71db", "Please save before combining bookings."),
					Res.GetString("08ece40e-9d43-4834-a66f-d9f3e3ed3f95", "Combine Booking"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
	}
}
