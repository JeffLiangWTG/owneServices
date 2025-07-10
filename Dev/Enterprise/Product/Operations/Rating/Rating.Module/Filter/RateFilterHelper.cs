using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Module
{
	/// <summary>
	/// Provides static filters / queries used in filters on Quotes / Rates / Costs / Global Tariffs.
	/// </summary>
	public class RateFilterHelper : RateQueryHelper
	{
		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class Constants
		{
			public const string QuoteNumber = "Quote Number";

			public const string Client = "Client";
			public const string ServiceProvider = "Service Provider";
			public const string OrganizationName = "Organization Name";

			public const string QuoteDate = "Quote Date";
			public const string ExpiryDate = "Expiry Date";
			public const string LastUpdated = "Last Updated";
			public const string AcceptedDate = "Accepted Date";
			public const string ClientAcceptedDate = "Client Accepted Date";
			public const string Signatory = "Signatory";
			public const string FollowUpDate = "Follow Up Date";

			public const string StaffFilterSalesRep = "Sales Rep";
			public const string StaffFilterCreatingUser = "Creating User";
			public const string GlobalRateFilter = "Global Rate";
		}

		#endregion

		public RateFilterHelper(BusinessObjectFactory factory)
			: base(factory) { }

		#region Date

		public ZQuery GetStartDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntrySubQuery(query, RateEntrySchema.TI_RateStartDate, comparisonOperator, value1, value2);
			return query;
		}

		public ZQuery GetEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntrySubQuery(query, RateEntrySchema.TI_RateEndDate, comparisonOperator, value1, value2);
			return query;
		}

		#endregion

		#region Organisation

		public ZQuery GetConsigneeQuery(ZGuid value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntrySubQuery(query, SQLComparisonOperator.Equal, value, RateEntrySchema.TI_OH_Consignee, System.Array.Empty<string>());
			return query;
		}

		public ZQuery GetConsignorQuery(ZGuid value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntrySubQuery(query, SQLComparisonOperator.Equal, value, RateEntrySchema.TI_OH_Consignor, System.Array.Empty<string>());
			return query;
		}

		public ZQuery GetTransportProviderQuery(ZGuid value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntrySubQuery(query, SQLComparisonOperator.Equal, value, RateEntrySchema.TI_OH_TransportProvider, System.Array.Empty<string>());
			return query;
		}

		#endregion

		#region Locations

		public ZQuery GetOriginDestinationQuery(ZString value1, ZString value2)
		{
			string[] rateTabs =
			{
				RatingConstants.RateCategory.AIR,
				RatingConstants.RateCategory.LCL,
				RatingConstants.RateCategory.FCL,
				RatingConstants.RateCategory.ORG,
				RatingConstants.RateCategory.DST,
				RatingConstants.RateCategory.SOR,
				RatingConstants.RateCategory.SDE,
				RatingConstants.RateCategory.SCO,
				RatingConstants.RateCategory.SNC,
				RatingConstants.RateCategory.SED,
				RatingConstants.RateCategory.SID
			};
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntryLocationSubQuery(query, value1, RateEntrySchema.TI_OriginLRC, rateTabs);
			AddRateEntryLocationSubQuery(query, value2, RateEntrySchema.TI_DestinationLRC, rateTabs);
			return query;
		}

		public ZQuery GetTranshipmentQuery(ZString value)
		{
			string[] rateTabs = { RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.LCL, RatingConstants.RateCategory.FCL };
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntrySubQuery(query, SQLComparisonOperator.Equal, value, RateEntrySchema.TI_ViaLRC, rateTabs);
			return query;
		}

		#endregion

		#region Other

		public ZQuery GetContractNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntrySubQuery(query, comparisonOperator, value, RateEntrySchema.TI_ContractNumber, System.Array.Empty<string>());
			return query;
		}

		public ZQuery GetLocationQuery(ZString location, SchemaColumn locationColumn)
		{
			var query = new ZDBOnlyQuery(typeof(RatingHeader));
			AddRateEntrySingleLocationSubQuery(query, location, locationColumn, System.Array.Empty<string>());
			return query;
		}

		public ZQuery GetSalesRepQuery(ZQuery filter)
		{
			var query = new ZDBOnlyQuery(typeof(RatingHeader));
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), RatingHeaderSchema.TH_OH);
			orgHeaderSubQuery.AddSubQuery(GetStaffAssignmentSalesRepQuery(filter), JoinCondition.And);
			query.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

			return query;
		}

		public ZQuery GetSalesRepQueryForQuotation(ZQuery filter)
		{
			var query = new ZDBOnlyQuery(typeof(RatingHeader));

			// DocAddress
			var addressCode = DocAddressTypes.GetCode(factory, DocAddressType.QuotationClientAddress);

			var docAddressesFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressesFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressesFilter.AddSubQuery(OrgAddressSchema.OA_OH, GetStaffAssignmentSalesRepQuery(filter), JoinCondition.And);
			docAddressesFilter.AddSubQuery(orgAddressesFilter, JoinCondition.And);
			docAddressesFilter.AddToFilter(JobDocAddress.GetFilter(addressCode, RatingHeaderSchema.Constants.Prefix, 0), JoinCondition.And);

			//Query
			query.AddSubQuery(RatingHeaderSchema.PK, docAddressesFilter, JoinCondition.And);

			return query;
		}

		ZDBOnlySubQuery GetStaffAssignmentSalesRepQuery(ZQuery filter)
		{
			ZDBOnlySubQuery staffAssignmentsSubQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			staffAssignmentsSubQuery.AddToFilter(filter);
			staffAssignmentsSubQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, StaffAssignmentRoles.Codes.SalesRep);
			staffAssignmentsSubQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Department, "ALL");

			ZQuery companyQuery = new ZQuery();
			companyQuery.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			companyQuery.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_GC, null);
			staffAssignmentsSubQuery.AddToFilter(companyQuery);

			return staffAssignmentsSubQuery;
		}

		#endregion

		#region Rate Entry Processor

		public ModuleFilterSubGroup RateEntryFilterProcessor
		{
			get { return rateEntryFilterProcessor ?? (rateEntryFilterProcessor = new RateEntryProcessor()); }
		}
		ModuleFilterSubGroup rateEntryFilterProcessor;

		class RateEntryProcessor : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var rateEntry = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_TH);
				rateEntry.AddToFilter(filter);

				var ratingHeader = new ZDBOnlyQuery(typeof(RatingHeader));
				ratingHeader.AddSubQuery(rateEntry, JoinCondition.And);

				return ratingHeader;
			}
		}

		#endregion

		#region Implementation

		void AddRateEntrySubQuery(ZQuery query, SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(RatingHeader));
			ZDBOnlySubQuery rateEntryQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_TH);

			if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				rateEntryQuery.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			}
			else if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				rateEntryQuery.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.Equal, ZDateTime.Empty);
			}
			else
			{
				if (fromDate.IsValid)
				{
					rateEntryQuery.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, fromDate);
				}

				if (toDate.IsValid)
				{
					rateEntryQuery.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, toDate);
				}
			}

			dbOnlyResult.AddSubQuery(rateEntryQuery, JoinCondition.And);

			query.AddToFilter(dbOnlyResult, JoinCondition.And);
		}

		void AddRateEntrySubQuery(ZQuery query, SQLComparisonOperator @operator, object value, SchemaColumn column, string[] rateTabs)
		{
			if (!((IZType)value).IsEmpty)
			{
				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(RatingHeader));
				ZDBOnlySubQuery rateEntryQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_TH);
				rateEntryQuery.AddToFilter(JoinCondition.And, column, @operator, value);

				if (rateTabs.Length > 0)
				{
					ZQuery typeFilter = new ZQuery(RateEntrySchema.TI_RateCategory, rateTabs);
					rateEntryQuery.AddToFilter(typeFilter, JoinCondition.And);
				}

				dbOnlyResult.AddSubQuery(rateEntryQuery, JoinCondition.And);

				query.AddToFilter(dbOnlyResult, JoinCondition.And);
			}
		}

		void AddRateEntrySingleLocationSubQuery(ZQuery query, ZString locationCode, SchemaColumn locationColumn, string[] rateTabs)
		{
			if (!locationCode.IsEmpty)
			{
				var dbOnlyResult = new ZDBOnlyQuery(typeof(RatingHeader));
				var rateEntryQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_TH);
				rateEntryQuery.AddToFilter(ModuleLocationFilter.GetLocationFilter(
					locationCode,
					locationColumn,
					isEmptyComparisonOperation: false,
					allowInternationalZones: true));

				if (rateTabs.Length > 0)
				{
					var typeFilter = new ZQuery(RateEntrySchema.TI_RateCategory, rateTabs);
					rateEntryQuery.AddToFilter(typeFilter, JoinCondition.And);
				}

				dbOnlyResult.AddSubQuery(rateEntryQuery, JoinCondition.And);

				query.AddToFilter(dbOnlyResult, JoinCondition.And);
			}
		}

		void AddRateEntryLocationSubQuery(ZQuery query, object value, SchemaColumn column, string[] rateTabs)
		{
			if (!((IZType)value).IsEmpty)
			{
				ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(RatingHeader));
				ZDBOnlySubQuery rateEntryQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_TH);
				rateEntryQuery.AddToFilter(LocationHelper.GetLocationFilter(factory, (ZString)value, column, typeof(RatingHeader)));

				if (rateTabs.Length > 0)
				{
					ZQuery typeFilter = new ZQuery(RateEntrySchema.TI_RateCategory, rateTabs);
					rateEntryQuery.AddToFilter(typeFilter, JoinCondition.And);
				}

				dbOnlyResult.AddSubQuery(rateEntryQuery, JoinCondition.And);

				query.AddToFilter(dbOnlyResult, JoinCondition.And);
			}
		}

		#endregion
	}
}
