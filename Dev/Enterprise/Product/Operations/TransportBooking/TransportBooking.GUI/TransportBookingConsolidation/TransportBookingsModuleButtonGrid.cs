using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.GUI;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	class TransportBookingsModuleButtonGrid : TransportModuleButtonGrid
	{
		public TransportBookingsModuleButtonGrid() : base()
		{
			Detached += TransportBookingsModuleButtonGrid_Detached;
			OnAttach += TransportBookingsModuleButtonGrid_OnAttach;
			MessageBoxButtons = MessageBoxButtons.YesNo;
		}

		ZString[] BookingStatusesNotRequiringDeactivation => new ZString[] { TransportStatuses.Codes.Available, TransportStatuses.Codes.Held };

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			UpdateBookingsRemoveAction(dataSource as BusinessObject);
		}

		void UpdateBookingsRemoveAction(BusinessObject businessObject)
		{
			if (DataSource is DtbBooking masterBooking)
			{
				MasterBooking = masterBooking;
				GridMode = TransportBookingsModuleButtonGridMode.MasterBooking;
			}
			else
			{
				GridMode = TransportBookingsModuleButtonGridMode.ConsolidationMultiJob;
				MasterBooking = null;
			}

			if (GridMode == TransportBookingsModuleButtonGridMode.MasterBooking || (businessObject != null && ConsolidationViewModeService.GetViewMode(businessObject.Factory) == ConsolidationViewMode.MultiJob))
			{
				InnerGrid.RemoveAction = RemoveAction.Remove;
				InnerGrid.DeleteMenuItem.Text = Res.GetString("a3025fc6-f781-4b4c-b13a-b5a6c9c1da5f", "Detach Booking");
			}
			else
			{
				InnerGrid.RemoveAction = RemoveAction.RemoveAndDelete;
				InnerGrid.DeleteMenuItem.Text = Res.GetString("4e77c1f7-fdd8-47d3-afcb-ec914919a930", "Delete Booking");
			}

			if (MasterBooking != null && BookingStatusesNotRequiringDeactivation.Contains(MasterBooking.KM_Status))
			{
				DetachMessage = Res.GetData("9962d21e-c990-4f69-97fd-5300607c78d8", "All records selected will be detached from this Master Transport Booking. Please confirm this action.");
			}
			else if (MasterBooking != null)
			{
				DetachMessage = Res.GetData("61c0d496-3f9e-4bb5-bac9-e1437b87fcbc", "This Master TB has already commenced. Detaching records now will result in the deactivation of the selected records.\r\nAlternatively, remove Actual Pickup/Delivery dates to reset the status to Available and then detach without deactivating the selected records.");
			}
			else
			{
				DetachMessage = Res.GetData("6e5b759e-12a2-428e-987b-7e0e30867222", "Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled.", "Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled. ");
			}
		}

		public void PerformClickOnAttachButton()
		{
			// If the user changes the selected Org and before tabbing off, clicks "Attach", the TransportCo will not be committed
			// and will therefore not appear in the attach filter. This is because the .NET ToolStrip control does not accept focus.
			// Core have a WI to address this however it is low priority -- WI00029212.
			this.Focus();
			AttachButton_Click(this, EventArgs.Empty);
		}

		public void PerformClickOnDetachButton()
		{
			// see comment above for Focus().
			this.Focus();
			DetachButton_Click(this, EventArgs.Empty);
		}

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			SelectFirstRowIfOnlyRowInGrid();

			var cannotDetachMessage = GetCannotDetachMessage();
			if (!cannotDetachMessage.IsEmpty)
			{
				Globals.Message.Show(cannotDetachMessage);
			}
			else
			{
				base.DetachButton_Click(sender, e);
			}
		}

		ZString GetCannotDetachMessage()
		{
			var result = ZString.Empty;

			if (InnerGrid.SelectedElements != null && InnerGrid.SelectedElements.Length > 0)
			{
				var selectedBookings = InnerGrid.SelectedElements.Cast<DtbBooking>();
				switch (GridMode)
				{
					case TransportBookingsModuleButtonGridMode.ConsolidationMultiJob:
						result = ShowBookingMessageHelper.GetCannotDetachMessage(selectedBookings);
						break;

					case TransportBookingsModuleButtonGridMode.MasterBooking:
						result = ShowBookingMessageHelper.GetCannotDetachFromMasterBookingMessage(selectedBookings, MasterBooking);
						break;

					default:
						throw new InvalidOperationException("This should never occur as TransportBookingsModuleButtonGridMode does not have any other enum values");
				}
			}

			return result;
		}

		void TransportBookingsModuleButtonGrid_Detached(object sender, ModuleButtonGridOnDetachedEventArgs e)
		{
			if (GridMode == TransportBookingsModuleButtonGridMode.MasterBooking)
			{
				MasterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(e.DetachedBusinessObjects.Cast<DtbBooking>().ToList());
			}
			DeactivateSubBookingsIfNecessary(e);
		}

		void TransportBookingsModuleButtonGrid_OnAttach(object sender, ModuleButtonGridOnAttachEventArgs e)
		{
			if (GridMode == TransportBookingsModuleButtonGridMode.MasterBooking)
			{
				MasterBooking.DefaultPackagesIfMasterFromSubBookingsForAllInstructions(e.AttachedBusinessObjects.Cast<DtbBooking>().ToList());
			}
		}

		void DeactivateSubBookingsIfNecessary(ModuleButtonGridOnDetachedEventArgs e)
		{
			if (MasterBooking != null && !BookingStatusesNotRequiringDeactivation.Contains(MasterBooking.KM_Status))
			{
				foreach (var subBooking in e.DetachedBusinessObjects.Cast<DtbBooking>())
				{
					subBooking.Deactivate();
				}
			}
		}

#if DEBUG
		internal
#endif
		TransportBookingsModuleButtonGridMode GridMode { get; set; }
#if DEBUG
		internal
#endif
		DtbBooking MasterBooking { get; set; }
	}

#if DEBUG
	internal
#endif
	enum TransportBookingsModuleButtonGridMode
	{
		ConsolidationMultiJob,
		MasterBooking
	}
}
