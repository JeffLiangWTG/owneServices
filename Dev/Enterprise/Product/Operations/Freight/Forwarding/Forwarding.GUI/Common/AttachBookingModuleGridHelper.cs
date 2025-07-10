using System;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class AttachBookingModuleGridHelper
	{
		public AttachBookingModuleGridHelper(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		public static EventHandler NewEventHandler(ForwardingConsol consol)
		{
			return delegate
			{
				AttachBookingModuleGridHelper helper = new AttachBookingModuleGridHelper(consol);
				helper.ShowBookingGrid();
			};
		}

		internal void ShowBookingGrid()
		{
			var attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(consol, null);

			if (!attachRequest.Errors.IsEmpty)
			{
				Globals.Message.ShowError(attachRequest.Errors);
			}
			else
			{
				var findBox = new VirtualFindBox();
				var decisionProvider = new AttachBookingsModuleButtonGridDecisionProvider(consol.Factory, findBox, consol);

				using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.QuotedBookings))
				{
					ShowEmbededModulePopupAndAttachEvent(module, findBox, decisionProvider);
				}
			}
		}

		void BookingsSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var shipmentPKsAndQuotedBookingPKs = e.SelectedBusinessObjects.Select(obj =>
			{
				var booking = (IViewQuotedBooking)obj;
				return Tuple.Create(booking.VB_JS, booking.PK);
			});
			var helper = new BuildConsolHelper();
			helper.ShipmentCannotBeAttachedToConsol += (sender, args) => Globals.Message.ShowError(args.Message);
			helper.AddBookingsToConsol(consol, shipmentPKsAndQuotedBookingPKs.ToArray());
		}

		protected virtual void ShowEmbededModulePopupAndAttachEvent(ZFilterModule module, VirtualFindBox findBox, AttachBookingsModuleButtonGridDecisionProvider decisionProvider)
		{
			module.OverrideModuleDecisionProvider(decisionProvider);
			using (var embeddedModulePopup = new EmbeddedModulePopup(module))
			{
				AddBookingsSelectedEvent(embeddedModulePopup);
				findBox.Initialize(decisionProvider.List, embeddedModulePopup);
				embeddedModulePopup.ShowDialog();
			}
		}

		protected virtual void AddBookingsSelectedEvent(EmbeddedModulePopup embeddedModulePopup)
		{
			embeddedModulePopup.Selected += BookingsSelected;
		}
	}
}
