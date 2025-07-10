using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class BookingInvoicingSupporter : ForwardingShipment.BaseForwardingShipmentInvoicingSupporter
	{
		public BookingInvoicingSupporter(QuotedBooking parent)
			: base(parent.Booking)
		{
			QuotedBooking = parent;
		}

		readonly QuotedBooking QuotedBooking;

		public override JobInvoicingConsumerType ConsumerType
		{
			get
			{
				return QuotedBooking.IsConsolidated ? JobInvoicingConsumerTypes.Shipment : JobInvoicingConsumerTypes.QuotedBooking;
			}
		}

		protected override JobSailing Sailing
		{
			get { return QuotedBooking.ScheduleChooser.Sailing; }
		}

		protected override IJobHeaderParent GetJobHeaderParentCore()
		{
			return QuotedBooking.JobHeaderParent;
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			var currentBooking = QuotedBooking.Booking;
			var carrier = QuotedBooking.Carrier;
			var creditor = currentBooking.Creditor;

			// If Carrier matches RateProviderOrgPK, return Creditor or Carrier
			if (carrier != null
				&& carrier.PK == defaultCreditorSetting.RateProviderOrgPK)
			{
				return creditor ?? carrier;
			}

			if (creditor != null
				&& creditor.PK == defaultCreditorSetting.RateProviderOrgPK)
			{
				return creditor;
			}

			if (!defaultCreditorSetting.RateProviderOrgPK.IsEmpty)
			{
				return MatchWithOperators(defaultCreditorSetting.RateProviderOrgPK);
			}

			return base.GetDefaultCreditor(defaultCreditorSetting);
		}

		OrgHeader MatchWithOperators(ZGuid rateProviderOrgPK)
		{
			var orgPKs = new List<ZGuid>
				{
					QuotedBooking.ExportReceivingDepot_ZAddress.OrgPK,
					QuotedBooking.ImportReleaseDepot_ZAddress.OrgPK,
					QuotedBooking.Booking.DocsAndCartage.JP_OA_PickupCartageCoAddr_ZAddress.OrgPK,
					QuotedBooking.Booking.PickupAgentDocumentaryAddress.OrganisationPK,
					QuotedBooking.Booking.JS_OH_DeliveryAgent,
					QuotedBooking.Booking.JS_OH_ExportBroker,
					QuotedBooking.Booking.JS_OH_ImportBroker,
					QuotedBooking.Booking.ControllingCustomerAddress.OrganisationPK,
					QuotedBooking.Booking.ControllingAgentDocumentaryAddress.OrganisationPK
				};

			if (orgPKs.Contains(rateProviderOrgPK))
			{
				return QuotedBooking.Factory.Load<OrgHeader>(rateProviderOrgPK);
			}

			return null;
		}

		public override bool IsQuote
		{
			get { return QuotedBooking.ObjectState == QuotedBookingState.AcceptedBookingWithQuote || QuotedBooking.ObjectState == QuotedBookingState.UnacceptedBookingWithQuote; }
		}

		public override bool IsBookingWithQuote
		{
			get
			{
				return QuotedBooking.Quote != null;
			}
		}

		public override bool ProviderPreferred => false;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.QuickBookingJobInvoicing;
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.QuickBooking;
		}

		public override ZString CustomsEntryNumberType => QuotedBooking.Booking?.CustomsEntryNumberType ?? base.CustomsEntryNumberType;

		public override ZString CommunityTransitStatus => QuotedBooking.Booking?.JS_CommunityTransitStatus ?? base.CommunityTransitStatus;
	}
}
