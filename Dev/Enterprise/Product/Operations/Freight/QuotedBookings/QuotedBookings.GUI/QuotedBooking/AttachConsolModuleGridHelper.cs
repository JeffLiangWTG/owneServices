using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Freight.QuotedBookings.Business.QuotedBookingToShipmentConverter;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public class AttachConsolModuleGridHelper
	{
		public AttachConsolModuleGridHelper(QuotedBooking quotedBooking)
		{
			this.quotedBooking = quotedBooking;
		}

		readonly QuotedBooking quotedBooking;

		public void ShowConsolGrid()
		{
			ForwardingShipment shipment;

			if (quotedBooking.Booking == null)
			{
				shipment = GetBooking(new BusinessObjectFactory());
			}
			else
			{
				shipment = quotedBooking.Booking;
			}

			var attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachConsol(shipment, null);
			if (!attachRequest.Errors.IsEmpty)
			{
				Globals.Message.ShowError(attachRequest.Errors);
			}
			else
			{
				var findBox = new VirtualFindBox();
				var decisionProvider = new AttachConsolModuleButtonGridDecisionProvider(quotedBooking.Factory, findBox, shipment);

				using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.JobConsol))
				{
					module.OverrideModuleDecisionProvider(decisionProvider);
					using (var embeddedModulePopup = new EmbeddedModulePopup(module))
					{
#if DEBUG
						LastPopupForTesting_ForTesting = embeddedModulePopup;
#endif
						embeddedModulePopup.Selected += BusinessObjectSelected;
						findBox.Initialize(decisionProvider.List, embeddedModulePopup);
						ZFormModaliser.ShowDialogWithoutDispose(embeddedModulePopup);
					}
				}
			}
		}

		void BusinessObjectSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var consol = (ForwardingConsol)e.SelectedBusinessObjects[0];

			if (consol != null)
			{
				var consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);
#if DEBUG
				LastController_ForTesting = consolController;
#endif
				consolController.ShowEditForm(consol);

				var forwardingConsol = consolController.LastShownForm != null ?
					(ForwardingConsol)((ZForm)consolController.LastShownForm).BusinessEntity
					: null;

				if (forwardingConsol != null)
				{
					ZGuid bookingPK;
					if (quotedBooking.Booking == null)
					{
						var booking = GetBooking(forwardingConsol.Factory);
						bookingPK = booking.PK;
					}
					else
					{
						bookingPK = quotedBooking.Booking.PK;
					}

					var helper = new BuildConsolHelper();
					helper.ShipmentCannotBeAttachedToConsol += (sender, args) => Globals.Message.ShowError(args.Message);
					helper.AddBookingsToConsol(forwardingConsol, new[] { bookingPK }, quotedBookingPK: quotedBooking.PK);

					var quotedBookingFromFactory = forwardingConsol.Factory.Load<QuotedBooking>(bookingPK);
					quotedBookingFromFactory.Booking?.MarkAsNeedingValidation();

					var converter = new QuotedBookingToShipmentConverter(quotedBookingFromFactory, BookingToShipmentConversionSource.Form);
					converter.CreateDeclarationIfNecessary(quotedBookingFromFactory.Booking);

					// Accept the quote
					if (quotedBookingFromFactory.Quote != null)
					{
						quotedBookingFromFactory.Quote.TH_IsLocked = true;
						quotedBookingFromFactory.Quote.TH_Accepted = ZDateTime.Today;
						quotedBookingFromFactory.Quote.Logs.AddNew(AutoEvents.QuotationAccepted);
					}

					foreach (ProcessTask task in quotedBookingFromFactory.WorkflowItems.Exceptions)
					{
						task.IsExceptionActioned = true;
					}
				}
			}
		}

		ForwardingShipment GetBooking(BusinessObjectFactory factory)
		{
			var vQuotedBooking = factory.Load<ViewQuotedBooking>(quotedBooking.Quote.PK);
			vQuotedBooking.QuotedBooking.ConvertQuoteToQuotedBooking();
			return vQuotedBooking.QuotedBooking.Booking;
		}

		#region Test Only

#if DEBUG
		internal EmbeddedModulePopup LastPopupForTesting_ForTesting;
		internal ZController LastController_ForTesting;
#endif
		#endregion
	}
}
