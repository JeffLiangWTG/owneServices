using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingShipmentMatcher : ShipmentMatcher<ForwardingShipment>
	{
		public ForwardingShipmentMatcher(BusinessObjectFactory factory, ShipmentReferences references, IXmlImportLogger logger, IUniversalFreightHelper helper, IXmlEventValueObject xmlEvent = null)
			: base(factory, references, logger, helper)
		{
			IsNVOCC = references.IsNVOCC;
			this.xmlEvent = xmlEvent;
		}

		protected override void AddJobShipmentTypeFilter(ZQuery query)
		{
			if (IsNVOCC)
			{
				var subQuery = new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, true);
				subQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_IsBooking, true);

				query.AddToFilter(subQuery);
				query.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);
			}
			else
			{
				base.AddJobShipmentTypeFilter(query);
			}
		}

		protected override void BuildNVOCCReferencesQueryAndMatchDelegate(ShipmentReferences referencesParent)
		{
			if (!referencesParent.GetHIREntryNumber().IsEmpty ||
				!referencesParent.CoLoadBookingConfirmationReference.IsEmpty ||
				!referencesParent.ShipmentID.IsEmpty ||
				!referencesParent.HBOLNumber.IsEmpty)
			{
				AddPossibleMatchForShipment(referencesParent);
			}
			else
			{
				reason = Res.GetString("34650348-07e6-42f5-b067-8e0059855c3f", "HIR entry number and booking confirmation reference are empty.");
			}
		}

		#region NVOCC

		ZBool IsNVOCC { get; }
		IXmlEventValueObject xmlEvent { get; }

		bool hasMasterBillNumberSubscription => xmlEvent != null && xmlEvent.Context.SubscriptionType == "MasterBillNumber";

		void AddPossibleMatchForShipment(ShipmentReferences referencesParent)
		{
			var shipmentQuery = NVOCCBookingQueryHelper.GetBookingQuery(referencesParent.GetHIREntryNumber(),
				referencesParent.ShipmentID,
				referencesParent.HBOLNumber,
				referencesParent.CoLoadBookingConfirmationReference,
				JobShipmentSchema.JS_HouseBill,
				referencesParent.CoLoadMasterBillNumber,
				referencesParent.BookingPartyPK,
				referencesParent.BookingPartyName);

			AddPossibleMatch(shipmentQuery, CreateMatchDelegate(referencesParent));
		}

		MatchDelegate CreateMatchDelegate(ShipmentReferences referencesParent)
		{
			return shipment =>
			{
				var matchCount = 0;

				if (!referencesParent.HBOLNumber.IsEmpty && shipment.JS_HouseBill == referencesParent.HBOLNumber)
				{
					matchCount += hasMasterBillNumberSubscription ? 64 : 32;
				}

				if (!referencesParent.ShipmentID.IsEmpty && shipment.JS_UniqueConsignRef == referencesParent.ShipmentID)
				{
					matchCount += hasMasterBillNumberSubscription ? 32 : 64;
				}

				var hir = referencesParent.GetHIREntryNumber();
				if (!hir.IsEmpty && hir == (shipment.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.eHubInterchangeReference.HIR)?.CE_EntryNum ?? ZString.Empty))
				{
					matchCount += 16;
				}

				if (!referencesParent.CoLoadBookingConfirmationReference.IsEmpty && shipment.JS_UniqueConsignRef == referencesParent.CoLoadBookingConfirmationReference)
				{
					matchCount += 8;
				}

				if (!referencesParent.CoLoadMasterBillNumber.IsEmpty && shipment.JS_HouseBill == referencesParent.CoLoadMasterBillNumber)
				{
					matchCount += shipment.JS_IsForwardRegistered ? 4 : 2;

					if (!referencesParent.BookingPartyPK.IsEmpty && shipment.BookingParty != null
						&& shipment.BookingParty.PK == referencesParent.BookingPartyPK)
					{
						matchCount += 1;
					}
					else if (!referencesParent.BookingPartyName.IsEmpty && shipment.BookingPartyDocumentaryAddress.E2_AddressOverride
						&& shipment.BookingPartyDocumentaryAddress.E2_CompanyName == referencesParent.BookingPartyName)
					{
						matchCount += 1;
					}
				}

				return matchCount;
			};
		}

		#endregion
	}
}
