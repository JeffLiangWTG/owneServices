using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal
{
	public class QuotedBookingMatcher : BookingMatcher, IMatchingBusinessEntityFinder<QuotedBooking>
	{
		public QuotedBookingMatcher(BusinessObjectFactory factory, ShipmentReferences references, IXmlImportLogger logger, IUniversalFreightHelper helper, IXmlEventValueObject xmlEvent = null)
			: base(factory, references, logger, helper)
		{
			this.xmlEvent = xmlEvent;
		}

		protected override void AddJobShipmentTypeFilter(ZQuery query)
		{
			query.AddToFilter(JobShipmentSchema.JS_IsBooking, ZBool.True);
		}

		protected override ZString DefaultReason => Res.GetString("cc0a257e-68b8-4c09-aa82-6fff21f696e0", "No matching quoted booking.");

		public new QuotedBooking GetBestMatch()
		{
			return GetBestMatchWithReason().quotedBooking;
		}

		public new (QuotedBooking quotedBooking, ZString reason) GetBestMatchWithReason()
		{
			var (shipmentBO, reason) = base.GetBestMatchWithReason();
			if (shipmentBO != null)
			{
				var query = new ZQuery(ViewQuotedBookingSchema.VB_JS, shipmentBO.PK);
				if (xmlEvent != null)
				{
					query.AddToFilter(new ZQuery(ViewQuotedBookingSchema.VB_IsConsolidated, false));
				}
				var bookingBOs = factory.Load<ViewQuotedBooking>(query)
					.Select(view => view.QuotedBooking)
					.ToArray();
				if (bookingBOs != null && bookingBOs.Length == 1)
				{
					return (bookingBOs[0], ZString.Empty);
				}
				reason = (xmlEvent != null) ? Res.GetString("c23e2aea-400b-49a6-94d3-b5e340d0bed2", $"Quoted booking ({shipmentBO.PK}) is consolidated or does not exist.") :
					Res.GetString("cfbf4fdf-30a5-4bf0-86ae-3fb39888c17a", $"Quoted booking ({shipmentBO.PK}) does not exist.");
			}
			else if (reason.IsEmpty)
			{
				reason = Res.GetString("cc0a257e-68b8-4c09-aa82-6fff21f696e0", "No matching quoted booking.");
			}

			return (null, reason);
		}

		protected override void BuildNVOCCReferencesQueryAndMatchDelegate(ShipmentReferences referencesParent)
		{
			if (!referencesParent.CoLoadBookingConfirmationReference.IsEmpty ||
				!referencesParent.AgentsReference.IsEmpty ||
				!referencesParent.ShipmentID.IsEmpty ||
				!referencesParent.HBOLNumber.IsEmpty)
			{
				AddPossibleMatchForQuotedBooking(referencesParent);
			}
			else
			{
				reason = Res.GetString("6a1f305f-b3cc-484c-bb9f-df9cd9afe638", "Booking confirmation and agents reference are empty.");
			}
		}

		#region Implementation

		void AddPossibleMatchForQuotedBooking(ShipmentReferences referencesParent)
		{
			var quotedBookingQuery = NVOCCBookingQueryHelper.GetBookingQuery(ZString.Empty,
				referencesParent.ShipmentID,
				referencesParent.HBOLNumber,
				referencesParent.CoLoadBookingConfirmationReference,
				JobShipmentSchema.JS_BookingReference,
				referencesParent.AgentsReference,
				referencesParent.BookingPartyPK,
				referencesParent.BookingPartyName);

			AddPossibleMatch(quotedBookingQuery, CreateMatchDelegate(referencesParent));
		}

		MatchDelegate CreateMatchDelegate(ShipmentReferences referencesParent)
		{
			return booking =>
			{
				var matchCount = 0;

				if (!referencesParent.HBOLNumber.IsEmpty && booking.JS_HouseBill == referencesParent.HBOLNumber)
				{
					matchCount += hasMasterBillNumberSubscription ? 16 : 8;
				}

				if (!referencesParent.ShipmentID.IsEmpty && booking.JS_UniqueConsignRef == referencesParent.ShipmentID)
				{
					matchCount += hasMasterBillNumberSubscription ? 8 : 16;
				}

				if (!referencesParent.CoLoadBookingConfirmationReference.IsEmpty && booking.JS_UniqueConsignRef == referencesParent.CoLoadBookingConfirmationReference)
				{
					matchCount += 4;
				}

				if (!referencesParent.AgentsReference.IsEmpty && booking.JS_BookingReference == referencesParent.AgentsReference)
				{
					matchCount += 2;

					if (!referencesParent.BookingPartyPK.IsEmpty && booking.BookingParty != null
						&& booking.BookingParty.PK == referencesParent.BookingPartyPK)
					{
						matchCount += 1;
					}
					else if (!referencesParent.BookingPartyName.IsEmpty && booking.BookingPartyDocumentaryAddress.E2_AddressOverride
						&& booking.BookingPartyDocumentaryAddress.E2_CompanyName == referencesParent.BookingPartyName)
					{
						matchCount += 1;
					}
				}

				return matchCount;
			};
		}

		readonly IXmlEventValueObject xmlEvent;

		bool hasMasterBillNumberSubscription => xmlEvent != null && xmlEvent.Context.SubscriptionType == "MasterBillNumber";

		#endregion
	}
}
