using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	/// <summary>
	/// Tested in DtbBooking module. New classes added here that are not used by/tested in the module need testing here.
	/// It is better to use ModuleFilterSubGroups where possible, this would be a later task (to prevent scope creep for Geodis).
	/// 
	/// Feel free to do this :).
	/// </summary>
	public class TransportBookingsQueryHelper
	{
		public ZQuery ConsolidationTransportCompanyQuery(ZGuid transportCoPk)
		{
			return TransportOrgAddressFieldForConsolQuery(SQLComparisonOperator.Equal, OrgAddressSchema.OA_OH, transportCoPk);
		}

		ZQuery TransportOrgAddressFieldForConsolQuery(SQLComparisonOperator comparisonOperator, SchemaColumn schemaColumn, IZType value)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBookingConsolidation));
			var addressSubQuery = AddressFieldSubQuery(comparisonOperator, schemaColumn, value, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
			result.AddSubQuery(addressSubQuery, JoinCondition.And);

			return result;
		}

		public ZQuery BookingCarrierAccountQuery(ZString carrierAccCode)
		{
			var carrierAccountSubQuery = new ZDBOnlySubQuery(typeof(OrgCarrierAccount), DtbBookingSchema.KM_OAN_CarrierAccount);
			carrierAccountSubQuery.AddToFilter(OrgCarrierAccountSchema.OAN_AccountNumber, carrierAccCode);

			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddSubQuery(carrierAccountSubQuery, JoinCondition.And);

			return result;
		}

		public ZQuery BookingTransportCompanyQuery(ZGuid transportCoPk)
		{
			return BookingTransportCoAddressFieldQuery(SQLComparisonOperator.Equal, OrgAddressSchema.OA_OH, transportCoPk);
		}

		ZQuery BookingTransportCoAddressFieldQuery(SQLComparisonOperator comparisonOperator, SchemaColumn schemaColumn, IZType value)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			var addressSubQuery = AddressFieldSubQuery(comparisonOperator, schemaColumn, value, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
			result.AddSubQuery(addressSubQuery, JoinCondition.And);

			return result;
		}

		public static ZQuery OrgAddressMatchQueryForNotEmpty(string addressTypeCode, ZGuid orgAddressToCompare)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			var matchAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);

			matchAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, addressTypeCode);
			matchAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.Equal, orgAddressToCompare);
			result.AddSubQuery(matchAddressSubQuery, JoinCondition.And);

			return result;
		}

		public static ZQuery OrgAddressMatchQueryForEmpty(string addressTypeCode)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			var blankAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, true);

			blankAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, addressTypeCode);
			blankAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.IsNotBlank, null);
			result.AddSubQuery(blankAddressSubQuery, JoinCondition.And);

			return result;
		}

		public static ZQuery BookingTemplateMatchesMasterBookingTemplate(string masterBookingTemplate)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddToFilter(DtbBookingSchema.KM_KT_NKBookingTemplate, SQLComparisonOperator.Equal, masterBookingTemplate);

			return result;
		}

		public static ZQuery BookingDirectionMatchesMasterBookingDirection(string masterBookingDirection)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddToFilter(DtbBookingSchema.KM_Direction, SQLComparisonOperator.Equal, masterBookingDirection);
			
			return result;
		}

		public static ZQuery BookingTransportModeMatchesMasterBookingTransportMode(string masterBookingTransportMode)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddToFilter(DtbBookingSchema.KM_TransportMode, SQLComparisonOperator.Equal, masterBookingTransportMode);

			return result;
		}

		public static ZQuery BookingConsolidationJobDirectionMatchesMasterBookingConsolidationJobDirection(string masterBookingConsolidationJobDirection)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			var consolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingConsolidationSchema.PK);
			consolidationSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobDirection, SQLComparisonOperator.Equal, masterBookingConsolidationJobDirection);

			result.AddSubQuery(DtbBookingSchema.KM_KB_Booking, consolidationSubQuery, JoinCondition.And);

			return result;
		}

		public ZQuery GetJobDocAddressQueryWithOperator(BusinessObjectFactory factory, object pK, DocAddressType addressType, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			var docAddressSubQuery = JobDocAddressSubQueryWithOperator(factory, pK, addressType, comparisonOperator);
			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);
			return result;
		}

		static ZDBOnlySubQuery JobDocAddressSubQueryWithOperator(BusinessObjectFactory factory, object pK, DocAddressType addressType, SQLComparisonOperator comparisonOperator)
		{
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var result = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			result.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(factory, addressType));

			if (comparisonOperator != SQLComparisonOperator.IsBlank && comparisonOperator != SQLComparisonOperator.IsNotBlank)
			{
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
			}

			result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return result;
		}

		public ZQuery BookingStringAttributeQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(DtbBooking));
			result.AddToFilter(schemaColumn, comparisonOperator, value);

			return result;
		}

		static ZDBOnlySubQuery AddressFieldSubQuery(SQLComparisonOperator comparisonOperator, SchemaColumn schemaColumn, IZType value, string addressType)
		{
			var result = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			result.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);

			var orgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddress.AddToFilter(schemaColumn, comparisonOperator, value);

			result.AddSubQuery(orgAddress, JoinCondition.And);

			return result;
		}
	}
}
