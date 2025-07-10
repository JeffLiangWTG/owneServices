using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class NVOCCBookingQueryHelper
	{
		#region Booking Query

		static public ZQuery GetBookingQuery(ZString hir, ZString coLoadBookingConfirmationReference, SchemaColumn bookingReferenceSchemaColumn, ZString bookingReference, ZGuid bookingPartyPK, ZString bookingPartyName)
		{
			var bookingQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));

			if (!hir.IsEmpty)
			{
				var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, ForwardingShipment.Schema.TableName);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CustomsReferenceNumberType.eHubInterchangeReference.HIR);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, hir);
				bookingQuery.AddSubQuery(entryNumberQuery, JoinCondition.Or);
			}

			if (!coLoadBookingConfirmationReference.IsEmpty)
			{
				bookingQuery.AddToFilter(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, coLoadBookingConfirmationReference), JoinCondition.Or);
			}

			if (!bookingReference.IsEmpty)
			{
				var bookingRefQuery = new ZQuery(bookingReferenceSchemaColumn, bookingReference);

				var bookingPartyQuery = new ZQuery();
				if (!bookingPartyPK.IsEmpty)
				{
					var bookingPartyAddressQuery = GetBookingPartyAddressQuery(bookingPartyPK);
					bookingPartyQuery.AddToFilter(bookingPartyAddressQuery);
				}

				if (!bookingPartyName.IsEmpty)
				{
					var bookingPartyNameQuery = GetBookingPartyNameQuery(bookingPartyName);
					bookingPartyQuery.AddToFilter(bookingPartyNameQuery, JoinCondition.Or);
				}

				bookingRefQuery.AddToFilter(bookingPartyQuery, JoinCondition.And);
				bookingQuery.AddToFilter(bookingRefQuery, JoinCondition.Or);
			}

			return bookingQuery;
		}

		static public ZQuery GetBookingQuery(ZString hir, ZString shipmentID, ZString hbolNumber, ZString coLoadBookingConfirmationReference, SchemaColumn bookingReferenceSchemaColumn, ZString bookingReference, ZGuid bookingPartyPK, ZString bookingPartyName)
		{
			var bookingQuery = GetBookingQuery(hir, coLoadBookingConfirmationReference, bookingReferenceSchemaColumn, bookingReference, bookingPartyPK, bookingPartyName);

			if (!shipmentID.IsEmpty)
			{
				bookingQuery.AddToFilter(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentID), JoinCondition.Or);
			}

			if (!hbolNumber.IsEmpty)
			{
				bookingQuery.AddToFilter(new ZQuery(JobShipmentSchema.JS_HouseBill, hbolNumber), JoinCondition.Or);
			}

			return bookingQuery;
		}

		static ZDBOnlyQuery GetBookingPartyAddressQuery(ZGuid bookingPartyPK)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, bookingPartyPK);

			jobDocAddressQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
			result.AddSubQuery(jobDocAddressQuery, JoinCondition.And);

			return result;
		}

		static ZDBOnlyQuery GetBookingPartyNameQuery(ZString bookingPartyName)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);

			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, bookingPartyName);
			result.AddSubQuery(jobDocAddressQuery, JoinCondition.And);

			return result;
		}

		#endregion

		public static ZGuid GetBookingParty(this UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var bookingPartyDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			if (bookingPartyDataObject != null)
			{
				var bookingPartyAddress = new OrganisationDataObjectReader(bookingPartyDataObject, logger, factory).GetMatched();
				if (bookingPartyAddress != null)
				{
					return bookingPartyAddress.OA_OH;
				}
			}

			return ZGuid.Empty;
		}

		public static ZString GetBookingPartyName(this UniversalShipment dataObject)
		{
			var bookingPartyDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			return bookingPartyDataObject?.CompanyName.GetValueOrDefault().ToUpper() ?? ZString.Empty;
		}

		public static bool IsBookingPartyMatched(ForwardingShipment booking, ZGuid bookingPartyPK, ZString bookingPartyName)
		{
			if (booking != null)
			{
				if (!bookingPartyPK.IsEmpty && booking.BookingParty != null && bookingPartyPK == booking.BookingParty.PK)
				{
					return true;
				}

				if (!bookingPartyName.IsEmpty && booking.BookingPartyDocumentaryAddress.E2_AddressOverride
					&& booking.BookingPartyDocumentaryAddress.E2_CompanyName == bookingPartyName)
				{
					return true;
				}
			}

			return false;
		}
	}
}
