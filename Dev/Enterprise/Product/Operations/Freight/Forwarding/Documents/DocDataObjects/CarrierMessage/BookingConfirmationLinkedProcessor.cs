using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	public sealed class BookingConfirmationLinkedProcessor : LinkedDocumentMessageProcessor, IBookingConfirmationLinkedProcessor
	{
		protected override bool ShouldProcess(IEDIMessage message) => message
			?.EM_MessageText
			.Contains((NoResString)"<DocumentName>Booking Confirmation</DocumentName>") // programmatic constant
			?? false;

		protected override ZGuid MenuItemPK => new ZGuid("913BDCEC-A6F5-4A30-84E9-E5941484BDEA"); // Booking Confirmation system defined menu item

		protected override ZInt MenuItemDocumentCount => 1;

		#region IBookingConfirmationLinkedProcessor members

		void IBookingConfirmationLinkedProcessor.Process(IBusiness bizObj, IStmALog log) => Process(bizObj, log);

		#endregion
	}
}
