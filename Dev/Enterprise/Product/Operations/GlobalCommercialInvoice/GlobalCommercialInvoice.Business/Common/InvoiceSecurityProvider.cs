using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Security;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	public static class InvoiceSecurityProvider
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject) =>
			bizObject switch
			{
				IForwardingShipment => Env.Security.ShipmentsCommercialInvoice,
				IQuotedBooking => Env.Security.BookingsCommercialInvoice,
				_ => Env.Security.None
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject) =>
			bizObject switch
			{
				IForwardingShipment => Env.Security.ShipmentsCommercialInvoice.IsAllowed
					? Env.Security.ShipmentsCommercialInvoiceEdit
					: Env.Security.ShipmentsCommercialInvoice,
				IQuotedBooking => Env.Security.BookingsCommercialInvoice.IsAllowed
					? Env.Security.BookingsCommercialInvoiceEdit
					: Env.Security.BookingsCommercialInvoice,
				_ => Env.Security.None
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsReadOnly(BusinessObjectFactory factory, ZGuid parentJobID)
		{
			var guid = parentJobID.IsEmpty ? Guid.Empty : parentJobID.ToGuid();
			var bizObject = factory.GetBizOsForPK(guid)
				.FirstOrDefault((bizO) => bizO is IForwardingShipment or IQuotedBooking);
			return !GetCheckPointForEdit(bizObject).IsAllowed;
		}
	}
}
