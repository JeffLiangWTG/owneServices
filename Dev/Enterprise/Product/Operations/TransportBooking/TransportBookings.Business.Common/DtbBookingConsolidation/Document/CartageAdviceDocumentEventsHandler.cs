using System.Collections;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Shared
{
	public abstract class CartageAdviceDocumentEventsHandler : IDocumentEventsHandler
	{
		public DocumentSupporter DocumentSupporter { get; set; }

		public abstract bool CanHandleMenuItem(IStmMenuItem menuItem);

		protected bool ContainsCartageAdvice(ICollection documents)
		{
			ZString[] cartageDocTypes = { RefDocTypes.CartageAdvice, RefDocTypes.CartageAdviceWithReceipt };

			return documents.OfType<IStmMenuTemplatePivot>().Any(doc =>
				doc.DocType != null && cartageDocTypes.Contains(doc.DocType.RT_DocType)
			);
		}

		public void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
		{
		}

		public void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (WasPrinted(e.DeliveryInstructionDestinationType))
			{
				if (Bookings.Length > 0)
				{
					foreach (IDtbBooking booking in Bookings)
					{
						if (booking.KM_BookingOfTransportRequestedDate.IsEmpty)
						{
							booking.KM_BookingOfTransportRequestedDate = ZDateTime.Now;
						}
					}

					try
					{
						if (Globals.IsUserInteractive)
						{
							((BusinessObject)Bookings.First()).Factory.Save();
						}
					}
					catch (ZSaveException ex) when (!ex.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
					catch (ZCannotSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		public void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
		}

		public void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
		}

		protected abstract IDocumentSupportable[] Bookings { get; }

		protected bool WasPrinted(DeliveryInstructionDestination destinationType)
		{
			return destinationType == DeliveryInstructionDestination.Print
				|| destinationType == DeliveryInstructionDestination.TakenFromContact
				|| destinationType == DeliveryInstructionDestination.Auto;
		}
	}
}
