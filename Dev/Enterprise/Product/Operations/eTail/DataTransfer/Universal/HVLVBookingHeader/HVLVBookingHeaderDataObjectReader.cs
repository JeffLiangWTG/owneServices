using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVBookingHeaderDataObjectReader : ShipmentDataObjectReader<HVLVBookingHeader>
	{
		public HVLVBookingHeaderDataObjectReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentDataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.HVLVBookingHeader;

		#region Matching

		protected override IMatchingBusinessEntityFinder<HVLVBookingHeader> GetCombinedReferenceMatcher() => null;

		protected override HVLVBookingHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		#endregion

		#region Populate

		protected override void PopulateBusinessObject(HVLVBookingHeader bookingHeaderBO)
		{
			SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_UseShipperDeliveryAccount, dataObject.IsLastMileDeliverySelfBooked);
			SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_RS_NKBookingServiceLevel, dataObject.ServiceLevel);

			if (dataObject.BookingConfirmationReference.HasValue)
			{
				var isConfirmed = dataObject.BookingConfirmationReference.Value == Freight.Integration.ShipmentStatusList.Codes.Confirmed;
				SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_IsBookingConfirmed, isConfirmed);
			}

			PopulateOrganizations(bookingHeaderBO);
			PopulateConsignments(bookingHeaderBO);

			if (dataObject.LocalProcessing != null)
			{
				new LocalProcessingDataObjectReader(dataObject, logger, factory).PopulateBusinessObject(bookingHeaderBO.DocsAndCartage);
			}
		}

		void PopulateConsignments(HVLVBookingHeader bookingHeaderBO)
		{
			if (dataObject.SubShipmentCollection != null)
			{
				var collectionReader = new HVLVConsignmentCollectionDataObjectReader(bookingHeaderBO, logger, factory, dataObject.SubShipmentCollection.ToArray(), null);
				collectionReader.ReadIntoCollectionRetainingUnmatchedElements();
				HVLVConsignmentTransactionParticipant.UnRegister(bookingHeaderBO.Consignments);
			}
		}

		void PopulateOrganizations(HVLVBookingHeader bookingHeaderBO)
		{
			var billToParty = GetMatchingOrgAddress(AddressTypes.SendersLocalClient);

			if (billToParty != null)
			{
				SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_OA_BillToParty, billToParty.PK);

				var billToPartyContact = GetMatchingContact(AddressTypes.SendersLocalClient);

				if (billToPartyContact != null)
				{
					SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_OC_BillToPartyContact, billToPartyContact.PK);
				}
			}
			else if (!bookingHeaderBO.IsInDatabase)
			{
				var errorMessage = Res.GetString("f1e2d6d1-3827-4f65-9389-1008aafb126d", "Cannot find matching Local Client Organization, which is required for creating a new HVLV Booking Header.");
				throw new DataObjectReadFailureException(errorMessage);
			}

			var dispatchAddress = GetMatchingOrgAddress(nameof(DocAddressType.ConsignorPickupDeliveryAddress));

			if (dispatchAddress != null)
			{
				SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_OA_DispatchAddress, dispatchAddress.PK);
			}

			var freightAgent = GetMatchingOrgAddress(nameof(DocAddressType.ExportBroker));

			if (freightAgent != null && !freightAgent.OA_OH.IsEmpty)
			{
				SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_OH_FreightAgent, freightAgent.OA_OH);
			}

			var originDepot = GetMatchingOrgAddress(nameof(DocAddressType.DepartureCFSAddress));

			if (originDepot != null)
			{
				SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_OA_OriginDepot, originDepot.PK);
			}

			var bookedByContact = GetMatchingContact(nameof(DocAddressType.BookingPartyDocumentaryAddress));

			if (bookedByContact != null)
			{
				SetValue(bookingHeaderBO, HVLVBookingHeaderSchema.HVH_OC_BookedBy, bookedByContact.PK);
			}
		}

		OrgAddress GetMatchingOrgAddress(string addressType)
		{
			var addressDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(addressType);
			return addressDataObject != null ? new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched() : null;
		}

		OrgContact GetMatchingContact(string addressType)
		{
			var addressDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(addressType);
			var orgAddress = addressDataObject != null ? new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched() : null;
			var contactName = addressDataObject?.Contact.GetValueOrDefault() ?? ZString.Empty;

			if (orgAddress != null && !contactName.IsEmpty)
			{
				var query = new ZQuery(OrgContactSchema.OC_OH, orgAddress.OA_OH);
				query.AddToFilter(OrgContactSchema.OC_ContactName, contactName);

				return factory.LoadTop1<OrgContact>(query);
			}

			return null;
		}

		#endregion
	}
}
