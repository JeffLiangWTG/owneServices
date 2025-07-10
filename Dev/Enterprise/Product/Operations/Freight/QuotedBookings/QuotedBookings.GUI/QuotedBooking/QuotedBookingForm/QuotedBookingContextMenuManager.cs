using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	/// <summary>
	/// There should exist one of these classes per QuotedBooking form.
	/// It is created by the ObjectFactory when the JobChargeUser control
	/// is bound.
	/// </summary>
	public class QuotedBookingContextMenuManager : IQuotedBookingContextMenuManager
	{
		/// <summary>
		/// This method should be called only once per instance
		/// </summary>
		void IQuotedBookingContextMenuManager.AddMenuToCharges(object jobChargesGrid, IQuotedBooking quotedBooking)
		{
			this.quotedBooking = (QuotedBooking)quotedBooking;
			this.jobChargesGrid = (ZGrid)jobChargesGrid;

			AddForChargesSelectingCreditor();
		}

		void ContextMenuShown(object sender, EventArgs e)
		{
			if (selectingCreditorMenuItem is not null)
			{
				selectingCreditorMenuItem.Visible =
					jobChargesGrid.IsMouseOnAValidRow &&
					ShouldShowSelectingCreditorsForChargesMenuItem;
			}
		}

		#region For Job Charges - Creditor selection

		internal bool ShouldShowSelectingCreditorsForChargesMenuItem =>
			quotedBooking?.Carrier is null &&
			(quotedBooking?.Creditor ?? ZGuid.Empty) == ZGuid.Empty &&
			(quotedBooking?.Quote?.CurrentOneOffQuote?.PossibleCarriers?.Count ?? 0) > 0;

		void AddForChargesSelectingCreditor()
		{
			selectingCreditorMenuItem =
				new ZMenuItem(
					ResString.GetMultilingualString("e43abcb6-5ec1-4986-a8ef-fb3cdfa4ecfc", "Select Creditor from Potential Carriers"),
					SelectingCreditorMenuItemClicked);
			jobChargesGrid.ContextMenu.MenuItems.Add(selectingCreditorMenuItem);
			jobChargesGrid.ContextMenu.Popup += ContextMenuShown;
			selectingCreditorMenuItem.Visible = ShouldShowSelectingCreditorsForChargesMenuItem;
		}

		void SelectingCreditorMenuItemClicked(object sender, EventArgs e)
		{
			var chargeToUpdate = CurrentJobCharge;
			if (chargeToUpdate != null)
			{
				var selectCreditor = new JobChargePossibleCarrierSelection(chargeToUpdate, quotedBooking.Quote?.CurrentOneOffQuote?.PossibleCarriers);
				ZFormModaliser.Show(new JobChargePossibleCarrierSelectionForm(selectCreditor), jobChargesGrid.FindForm());
			}
			else
			{
				Globals.Message.Show(Res.GetString("97014835-7bc5-4a8d-8fcf-f11a2f026adf", "Please select a row to make a selection."));
			}
		}

		MenuItem selectingCreditorMenuItem;

		#endregion

		QuotedBooking quotedBooking;
		ZGrid jobChargesGrid;

		internal ICharge CurrentJobCharge
		{
			get
			{
				return currentJobChargeOverride ?? (ICharge)jobChargesGrid.ListManager?.GetCurrent();
			}
#if DEBUG
			set
			{
				currentJobChargeOverride = value;
			}
#endif
		}
		ICharge currentJobChargeOverride;
	}
}
