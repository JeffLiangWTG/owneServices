using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class QuotedBookingFilterStripBusinessObject : BaseQuotedBookingFilterStripBusinessObject
	{
		public QuotedBookingFilterStripBusinessObject()
			: this(false)
		{
		}

		public QuotedBookingFilterStripBusinessObject(bool allowTemplateRecords)
		{
			AllowTemplateRecords = allowTemplateRecords;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery quoteCompanyQuery = new ZQuery();
				quoteCompanyQuery.AddToFilter(ViewQuotedBookingSchema.VB_GC, Env.CurrentCompany.PK);
				quoteCompanyQuery.AddToFilter(JoinCondition.Or, ViewQuotedBookingSchema.VB_TH, null);

				ZQuery query = new ZQuery();
				query.AddToFilter(base.Filter);
				query.AddToFilter(quoteCompanyQuery);
				query.AddToFilter(JoinCondition.And, ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, null);
				return query;
			}
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumbersAndReferencesFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddDatesFilters(filters);
			AddLocationsFilters(filters);
			AddOrganisationsStaffFilters(filters);
			AddModesAndTypesFilters(filters);
			AccountingFilterStrip.AddBillingFilters(filters);
			securityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			AddTemplateRecordFilter(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.QuickBookingJobInvoicing);
			AddHBLDeliveryModeFilter(filters);

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				AddCO2Filters(filters);
			}

			return filters;
		}

		#region Numbers And References

		protected override void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			base.AddNumbersAndReferencesFilters(filters);
			var referenceNumberFilter = new ReferenceNumberFilter(
				Descriptions.NumbersAndReferences.AdditionalReferenceNumber,
				new QuotedBookingReferenceNumberFilterHelper().GetReferenceNumberFilter,
				new RefCountryCollection(Factory)
			).WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|AdditionalRefNum", "Additional Reference #");

			filters.AddCustomFilter(referenceNumberFilter);

			var vessalVoyageModuleFilter = new VoyageVesselModuleFilter(
				Descriptions.NumbersAndReferences.FlightVoyageNumber,
				GetFlightVoyageNumberAndVesselQuery,
				BindingLists.RefVessel_List
			).WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			vessalVoyageModuleFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|FlightVoyageNumVessel", "Flight/Voyage # and Vessel");

			filters.AddCustomFilter(vessalVoyageModuleFilter);

			new FilterBuilder(filters, FilterCategories.NumbersAndReferences)
			//Quote
			.AddFountainFilter(Descriptions.NumbersAndReferences.QuoteNumber, GetQuoteNoQuery, "").WithMaxLengthOf(RatingHeaderSchema.TH_QuoteNumber).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|QuoteNum", "Quote #"));
			var houseBillNumberFilter = filters.AddNumberFilter(Descriptions.NumbersAndReferences.HouseBillNumber, GetHouseBillNoQuery);
			houseBillNumberFilter.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;
			houseBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|HouseBillNum", "House Bill #");
			houseBillNumberFilter.IsCommon = true;

			ModuleFountainFilter shipmentNoFilter = new ModuleFountainFilter(Descriptions.NumbersAndReferences.BookingNumber, GetBookingNoQuery, "S");
			shipmentNoFilter.IsCommon = true;
			shipmentNoFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|BookingNum", "Booking #");
			shipmentNoFilter.SupportsXQuery = true;
			filters.AddFilter(shipmentNoFilter);

			var cfsRefFilter = filters.AddTextFilter(Descriptions.NumbersAndReferences.CfsRefNumber, JobShipmentSchema.JS_CFSReference).WithMaxLengthOf<ModuleTextFilter>(JobShipmentSchema.JS_CFSReference);
			cfsRefFilter.SubGroup = new CFSRefFilterSubGroup();
			cfsRefFilter.Category = FilterCategories.NumbersAndReferences;
			cfsRefFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|CFSRefNum", "CFS Ref #");

			var containerFilter = filters.AddNumberFilter(Descriptions.NumbersAndReferences.ContainerNumber, JobContainerSchema.JC_ContainerNum).WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum);
			containerFilter.SubGroup = new ContainerFilterSubGroup();
			containerFilter.Category = FilterCategories.NumbersAndReferences;
			containerFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ContainerNum", "Container #");

			var directMAWBFilter = filters.AddNumberFilter(Descriptions.NumbersAndReferences.DirectMawbNumber, JobShipmentSchema.JS_HouseBill).WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_HouseBill);
			directMAWBFilter.SubGroup = new DirectMAWBFilterSubGroup();
			directMAWBFilter.Category = FilterCategories.NumbersAndReferences;
			directMAWBFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|DirectMAWBNum", "Direct MAWB #");

			var shippersRefFilter = filters.AddNumberFilter(Descriptions.NumbersAndReferences.ShippersRefNumber, JobShipmentSchema.JS_BookingReference).WithMaxLengthOf<ModuleNumberFilter>(JobShipmentSchema.JS_BookingReference);
			shippersRefFilter.SubGroup = new ShippersRefFilterSubGroup();
			shippersRefFilter.Category = FilterCategories.NumbersAndReferences;
			shippersRefFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ShippersRefNum", "Shippers Ref #");

			var productCodeFilter = filters.AddTextFilter("Product Code", GetPackProductProductCode);
			productCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|PackProductProductCode", "Product Code");
			productCodeFilter.Category = FilterCategories.NumbersAndReferences;

			if (FreightConfigurationRegistry.Instance.EnableClientContractNumber.Value)
			{
				var clientContractNoFilter = filters.AddNumberFilter(Descriptions.NumbersAndReferences.ClientContractNumber, GetClientContractNoQuery).WithMaxLengthOf<ModuleNumberFilter>(JobHeaderSchema.JH_ClientContractNumber);
				clientContractNoFilter.Category = FilterCategories.NumbersAndReferences;
				clientContractNoFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ClientContractNumber", "Client Contract #");
			}

			if (ObjectFactory.Get<IContractPermissions>().IsAllocationsVisible())
			{
				var contractNumberFilter = filters.AddTextFilter(Descriptions.NumbersAndReferences.CarrierContractNumber, GetCarrierContractNoQuery).WithMaxLengthOf<ModuleTextFilter>(JobShipmentSchema.JS_CarrierContractNumber);
				contractNumberFilter.Category = FilterCategories.NumbersAndReferences;
				contractNumberFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|CarrierContractNumber", "Carrier Contract #");

				var allocationIDFilter = filters.AddNumberFilter(Descriptions.NumbersAndReferences.AllocationID, GetAllocationQuery);
				allocationIDFilter.Category = FilterCategories.NumbersAndReferences;
				allocationIDFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|AllocationID", "Allocation ID");
			}
		}

		ZQuery GetCarrierContractNoQuery(SQLComparisonOperator comparisonOperator, ZString carrierContractNo)
		{
			var result = new ZQuery();

			var viewFilter = GetWithBookingQuery(JobShipmentSchema.JS_CarrierContractNumber, comparisonOperator, carrierContractNo);
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				var noBookingFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				noBookingFilter.AddToFilter(ViewQuotedBookingSchema.VB_JS, comparisonOperator, null);
				viewFilter.AddToFilter(noBookingFilter, JoinCondition.Or);
			}

			result.AddToFilter(viewFilter, JoinCondition.And);

			return result;
		}

		ZQuery GetAllocationQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				var blankResult = new ZQuery();
				var blankViewFilter = GetWithBookingQuery(JobShipmentSchema.JS_RCA_BookingAllocationLine, comparisonOperator, value);
				blankResult.AddToFilter(blankViewFilter, JoinCondition.And);

				var noBookingFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				noBookingFilter.AddToFilter(ViewQuotedBookingSchema.VB_JS, comparisonOperator, null);
				blankViewFilter.AddToFilter(noBookingFilter, JoinCondition.Or);

				return blankViewFilter;
			}

			var allocationSubQuery = new ZDBOnlySubQuery(typeof(IRatingContractAllocationLine), JobShipmentSchema.JS_RCA_BookingAllocationLine);
			allocationSubQuery.AddToFilter_PossiblyCommaSeparated(RatingContractAllocationLineSchema.RCA_AllocationLineID, comparisonOperator, value);

			var viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			viewFilter.IgnoreActiveFilter = true;

			var bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK, false);
			bookingSubQuery.AddSubQuery(allocationSubQuery, JoinCondition.And);
			viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			return viewFilter;
		}

		ZQuery GetPackProductProductCode(SQLComparisonOperator @operator, ZString productCode)
		{
			var shipmentQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));

			var packLineSubQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			var productSubQuery = new ZDBOnlySubQuery(typeof(PackProduct), JobPackProductSchema.D2_JL);

			productSubQuery.AddToFilter(JobPackProductSchema.D2_ProductCode, @operator, productCode);
			packLineSubQuery.AddSubQuery(productSubQuery, JoinCondition.And);
			shipmentQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, packLineSubQuery, JoinCondition.And);

			return shipmentQuery;
		}

		#region GetQuoteNoQuery

		protected override ZQuery GetQuoteNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = base.GetQuoteNoQuery(comparisonOperator, value);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(ViewQuotedBookingSchema.VB_TH, comparisonOperator, null);
			}
			return result;
		}

		#endregion

		#region GetBookingNoQuery

		ZQuery GetBookingNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery();

			if (!value.IsEmpty)
			{
				ZDBOnlyQuery viewFilter = GetWithBookingQuery(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, value);
				result.AddToFilter(viewFilter, JoinCondition.And);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(ViewQuotedBookingSchema.VB_JS, comparisonOperator, null);
			}

			return result;
		}

		#endregion

		#region GetHouseBillNoQuery

		ZQuery GetHouseBillNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery();

			ZDBOnlyQuery viewFilter = GetWithBookingQuery(JobShipmentSchema.JS_HouseBill, comparisonOperator, value);
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				ZDBOnlyQuery noBookingFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				noBookingFilter.AddToFilter(ViewQuotedBookingSchema.VB_JS, comparisonOperator, null);
				viewFilter.AddToFilter(noBookingFilter, JoinCondition.Or);
			}

			result.AddToFilter(viewFilter, JoinCondition.And);

			return result;
		}

		#endregion

		#region CFSRefFilterSubGroup

		class CFSRefFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery query)
			{
				return GetWithBookingQuery(query);
			}
		}

		#endregion

		#region DirectMAWBFilterSubGroup

		class DirectMAWBFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery query)
			{
				var result = new ZQuery();

				var viewFilter1 = GetWithBookingQuery(query);
				var viewFilter2 = GetWithBookingQuery(JobShipmentSchema.JS_IsDirectBooking, SQLComparisonOperator.Equal, true);
				result.AddToFilter(viewFilter1, JoinCondition.And);
				result.AddToFilter(viewFilter2, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region ShippersRefFilterSubGroup

		class ShippersRefFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery query)
			{
				var result = new ZQuery();

				var viewFilter = GetWithBookingQuery(query);
				result.AddToFilter(viewFilter, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region GetFlightVoyageNumberAndVesselQuery

		ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator comparisonOperator, ZString flightOrVoyageNo, ZString vesselNK, ZBool includeArchived)
		{
			ZQuery result = new ZQuery();

			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);

			builder.Vessel = vesselNK;
			builder.VoyageFlight = flightOrVoyageNo;
			builder.VoyageFlightComparisonOperator = comparisonOperator;
			builder.IncludeArchived = includeArchived;

			ZDBOnlyQuery viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			ZDBOnlySubQuery bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			bookingSubQuery.AddToFilter(builder.ToShipmentFilter(GetRelationshipFlags()));

			viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			result.AddToFilter(viewFilter, JoinCondition.And);

			return result;
		}

		static string[] ShipmentFilterConsolidatedStatuses => new[] { ConsolidatedStatus.Code.Cons, ConsolidatedStatus.Code.All };

		SailingFilterBuilder.RelationshipFlags GetRelationshipFlags()
		{
			var flags = SailingFilterBuilder.RelationshipFlags.Direct;
			if (consolidatedFilter == null)
			{
				return flags;
			}
			if (consolidatedFilter.IsActive && ShipmentFilterConsolidatedStatuses.Contains(consolidatedFilter.Property.ToString()))
			{
				flags |= SailingFilterBuilder.RelationshipFlags.ViaConsol | SailingFilterBuilder.RelationshipFlags.ViaDirectTransports;
			}
			return flags;
		}

		#endregion

		#region ContainerFilterSubGroup

		class ContainerFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery query)
			{
				ZQuery result = new ZQuery();

				ZDBOnlyQuery viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				ZDBOnlySubQuery bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);

				ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(ForwardingContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				containerSubQuery.AddToFilter(query);

				bookingSubQuery.AddSubQuery(containerSubQuery, JoinCondition.And);
				viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);
				result.AddToFilter(viewFilter, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#region GetClientCarrierNoQuery

		ZQuery GetClientContractNoQuery(SQLComparisonOperator comparisonOperator, ZString clientContractNum)
		{
			var result = new ZQuery();

			var isBlankOperator = comparisonOperator == SpecialComparisonOperator.IsBlank
				|| comparisonOperator == SQLComparisonOperator.DoesNotStartWith
				|| comparisonOperator == SQLComparisonOperator.NotContains
				|| comparisonOperator == SQLComparisonOperator.NotEqual;

			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_ClientContractNumber, comparisonOperator, clientContractNum);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			var bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			bookingSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_TH, null);
			bookingSubQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);

			var bookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			bookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			var quoteSubQuery = new ZDBOnlySubQuery(typeof(ViewQuotedBooking), ViewQuotedBookingSchema.VB_TH);
			quoteSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, null);
			quoteSubQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, jobHeaderSubQuery, JoinCondition.And);

			var quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.Or);

			result.AddToFilter(bookingQuery, JoinCondition.Or);
			result.AddToFilter(quoteQuery, JoinCondition.Or);

			if (isBlankOperator)
			{
				var blankOperatorJobQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);

				var blankOperatorBookingSubQuery = new ZDBOnlySubQuery(typeof(ViewQuotedBooking), ViewQuotedBookingSchema.VB_JS);
				blankOperatorBookingSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_TH, null);
				blankOperatorBookingSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, null);
				blankOperatorBookingSubQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, blankOperatorJobQuery, JoinCondition.And);

				var blankOperatorBookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				blankOperatorBookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, blankOperatorBookingSubQuery, JoinCondition.And);

				result.AddToFilter(blankOperatorBookingQuery, JoinCondition.Or);
			}

			return result;
		}

		#endregion

		#endregion

		#region Status And Flags

		ModuleTextFilter consolidatedFilter;

		protected override void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			base.AddStatusAndFlagsFilters(filters);

			consolidatedFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.ConsolidatedOrConverted, GetConsolidatedQuery, ConsolidatedStatusList);
			consolidatedFilter.Category = FilterCategories.StatusAndFlags;
			consolidatedFilter.DefaultProperty = ConsolidatedStatus.Code.Ncons;
			consolidatedFilter.Visibility = FilterVisibility.AlwaysApplied;
			consolidatedFilter.IsPublishedOnWeb = false;
			consolidatedFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Consolidated", "Consolidated/Converted");

			var bookingStatusFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.ShipmentStatus, GetShipmentStatusQuery, ShipmentStatusList);
			bookingStatusFilter.Category = FilterCategories.StatusAndFlags;
			bookingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentFilter|BookingStatus", "HBL Booking Status");

			filters.AddFlagsFilter(Descriptions.StatusAndFlags.IsHazardous,
				new string[] { Res.GetString("Forwarding|QuotedBookingFilterControl|IsHazardous", "Is Hazardous") },
				new GetFlagsQuery[] { GetIsHazardousQuery }).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|QuotedBookingFilterControl|IsHazardous", "Is Hazardous");

			AddQuoteAndOrBookingFilter(filters);

			var isTemperatureControlledFilter = filters.AddFlagsFilter(
				"Is Temperature Controlled",
				new string[] { Res.GetString("QuotedBookings|QuotedBookingFilterControl|IsTemperatureControlled", "Is Temperature Controlled") },
				new GetFlagsQuery[] { GetIsTemperatureControlledQuery }
			);
			isTemperatureControlledFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|IsTemperatureControlled", "Is Temperature Controlled");
		}

		ModuleTextFilter AddQuoteAndOrBookingFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter quoteAndOrBookingFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.QuoteAndOrBooking, GetQuoteAndOrBookingQuery, QuoteAndOrBookingList);
			quoteAndOrBookingFilter.Category = FilterCategories.StatusAndFlags;
			quoteAndOrBookingFilter.IsPublishedOnWeb = false;
			quoteAndOrBookingFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|QuoteAndOrBooking", "Quote And/Or Booking");
			return quoteAndOrBookingFilter;
		}

		#region GetConsolidatedQuery

		ZQuery GetConsolidatedQuery(ZString value)
		{
			ZQuery result = new ZQuery();

			if (!value.IsEmpty && value != ConsolidatedStatus.Code.All)
			{
				result.AddToFilter(ViewQuotedBookingSchema.VB_IsConsolidated, SQLComparisonOperator.Equal, value == ConsolidatedStatus.Code.Cons);
			}

			return result;
		}

		#endregion

		#region GetQuoteAndOrBookingQuery

		ZQuery GetQuoteAndOrBookingQuery(ZString value)
		{
			ZQuery result = new ZQuery();

			if (!value.IsEmpty)
			{
				ZQuery viewFilter = new ZQuery();

				switch (value)
				{
					case "BOO":
						viewFilter.AddToFilter(JoinCondition.And, ViewQuotedBookingSchema.VB_TH, SQLComparisonOperator.Equal, null);
						break;
					case "BWQ":
						viewFilter.AddToFilter(JoinCondition.And, ViewQuotedBookingSchema.VB_TH, SQLComparisonOperator.NotEqual, null);
						break;
					case "BOQ":
						break;
				}
				result.AddToFilter(viewFilter, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region GetIsHazardous

		ZQuery GetIsHazardousQuery(ZBool isHazardous)
		{
			var isHazardousQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			isHazardousQuery.AddSubQuery(
				ViewQuotedBookingSchema.VB_JS,
				ForwardingShipmentQueryHelper.GetHazardousShipmentsAgainstQuery(JobShipmentSchema.PK, isHazardous),
				JoinCondition.And);

			return isHazardousQuery;
		}

		#endregion

		#region GetShipmentStatusQuery

		ZQuery GetShipmentStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery();

			var viewFilter = GetWithBookingQuery(JobShipmentSchema.JS_ShipmentStatus, comparisonOperator, value);
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				var noBookingFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				noBookingFilter.AddToFilter(ViewQuotedBookingSchema.VB_JS, comparisonOperator, null);
				viewFilter.AddToFilter(noBookingFilter, JoinCondition.Or);
			}

			result.AddToFilter(viewFilter, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetIsTemperatureControlledQuery

		ZQuery GetIsTemperatureControlledQuery(ZBool isTemperatureControlled)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			ZDBOnlySubQuery bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery packLineQuery = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_RequiresTemperatureControl, isTemperatureControlled);
			packLineQuery.AddToFilter(JobPackLinesSchema.JL_FreightMode, FreightConstants.OuterPackType);
			bookingSubQuery.AddSubQuery(packLineQuery, JoinCondition.And);
			result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#endregion

		#region Dates

		protected override void AddDatesFilters(ModuleFilterCollection filters)
		{
			new FilterBuilder(filters, FilterCategories.Dates)

			.AddDateFilter(Descriptions.Dates.AdditionalRefNumIssueDate, GetAdditionalReferenceNumberIssueDateQuery).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|AdditionalRefNumIssueDate", "Additional Ref No Issue Date"))
			.AddDateTimeFilter(Descriptions.Dates.BookingDate, JobShipmentSchema.JS_A_BKD, JobShipmentSubGroup).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|BookingDate", "Booking Date"))
			.AddDateTimeFilter(Descriptions.Dates.ClientReqETA, JobShipmentSchema.JS_ClientRequestedETA, JobShipmentSubGroup).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ClientReqETA", "Client Req. ETA"))
			.AddDateTimeFilter(Descriptions.Dates.EstimatedPickup, JobDocsAndCartageSchema.JP_EstimatedPickup, JobDocsAndCartageSubGroup).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|PEstimatedPickup", "Estimated Pickup"))
			.AddDateTimeFilter(Descriptions.Dates.PickupRequiredBy, JobDocsAndCartageSchema.JP_PickupRequiredBy, JobDocsAndCartageSubGroup).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|PickupRequiredBy", "Pickup Required By"))
			.AddDateTimeFilter(Descriptions.Dates.EstimatedDeliveryDate, JobDocsAndCartageSchema.JP_EstimatedDelivery, JobDocsAndCartageSubGroup).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|EstimatedDeliveryDate", "Estimated Delivery Date"))
			.AddDateTimeFilter(Descriptions.Dates.DeliveryRequiredBy, JobDocsAndCartageSchema.JP_DeliveryRequiredBy, JobDocsAndCartageSubGroup).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|DeliveryRequiredBy", "Delivery Required By"))
			.AddDateTimeFilter(Descriptions.Dates.ETD, JobShipmentSchema.JS_E_DEP, JobShipmentSubGroup).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ETD", "ETD"))
			.AddDateTimeFilter(Descriptions.Dates.ETA, JobShipmentSchema.JS_E_ARV, JobShipmentSubGroup).MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ETA", "ETA"));

			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				var deliveryDueDateFilter = filters.AddDateFilter(Descriptions.Dates.DeliveryDueDate, JobShipmentSchema.JS_DeliveryDueDate);
				deliveryDueDateFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|DeliveryDueDate", "Delivery Due Date");
				deliveryDueDateFilter.SubGroup = JobShipmentSubGroup;
			}
		}

		ZQuery GetAdditionalReferenceNumberIssueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime minValue, ZDateTime maxValue)
		{
			ZDBOnlySubQuery cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, ForwardingShipment.Schema.TableName);

			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasNoDateEntered:
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
					break;
				case DateComparisonOperator.HasDateEntered:
					cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
					break;
				case DateComparisonOperator.HasDateInRange:
					if (minValue.IsValid)
					{
						cusEntryNumQuery.AddToFilter(new ZQuery(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, minValue));
					}

					if (maxValue.IsValid)
					{
						cusEntryNumQuery.AddToFilter(new ZQuery(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, maxValue));
					}
					break;
			}

			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);

			ZDBOnlyQuery viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			viewFilter.IgnoreActiveFilter = true;

			ZDBOnlySubQuery bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			bookingSubQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

			viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			return viewFilter;
		}

		#endregion

		#region Locations

		protected override void AddLocationsFilters(ModuleFilterCollection filters)
		{
			AddLoadDischargeFilter(filters);
			base.AddLocationsFilters(filters);
		}

		#region GetOriginDestinationQuery

		protected override ZQuery GetOriginDestinationQuery(ZString originNK, ZString destinationNK)
		{
			ZQuery result = new ZQuery();

			if (!originNK.IsEmpty || !destinationNK.IsEmpty)
			{
				ZQuery bookingQuery = new ZQuery();

				if (!originNK.IsEmpty)
				{
					bookingQuery.AddToFilter(GetWithBookingQuery(LocationHelper.GetLocationFilter(Factory, originNK, JobShipmentSchema.JS_RL_NKOrigin, typeof(CommonShipment))));
				}

				if (!destinationNK.IsEmpty)
				{
					bookingQuery.AddToFilter(GetWithBookingQuery(LocationHelper.GetLocationFilter(Factory, destinationNK, JobShipmentSchema.JS_RL_NKDestination, typeof(CommonShipment))));
				}

				result.AddToFilter(bookingQuery);
			}

			return result;
		}

		#endregion

		#endregion

		#region Modes And Types

		protected override void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			base.AddModesAndTypesFilters(filters);

			var serviceTypeDateBookedFilter = new ServiceTypeDateFilter(this, typeof(JobDocsAndCartage), true);
			serviceTypeDateBookedFilter.SubGroup = JobDocsAndCartageSubGroup;
			filters.AddCustomFilter(serviceTypeDateBookedFilter);

			var serviceTypeDateCompletedFilter = new ServiceTypeDateFilter(this, typeof(JobDocsAndCartage), false);
			serviceTypeDateCompletedFilter.SubGroup = JobDocsAndCartageSubGroup;
			filters.AddCustomFilter(serviceTypeDateCompletedFilter);
		}

		#endregion

		#endregion

		#region Organisations Staff

		protected override void AddOrganisationsStaffFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationsStaffFilters(filters);

			var bookingPartyNameFilter = filters.AddTextFilter(Descriptions.OrganisationsStaff.BookingPartyName, GetBookingPartyNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			bookingPartyNameFilter.Category = FilterCategories.Organisations;
			bookingPartyNameFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|BookingPartyName|a98801d9-5d02-4f25-b091-51985fc8b85b", "Booking Party Name");

			var bookingPartyFilter = filters.AddGuidFilter(Descriptions.OrganisationsStaff.BookingParty, ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.BookingPartyDocumentaryAddress), BindingLists.OrgHeader_List);
			bookingPartyFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|BookingParty|6d5088d5-5ae5-43ef-ab74-9a1ba27e169f", "Booking Party");
			bookingPartyFilter.MultiValueQueryDelegate = GetDocAddressMultiValueQueryDelegate(DocAddressType.BookingPartyDocumentaryAddress);

			var deliveryAgentFilter = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.DeliveryAgent, ModuleIDs.Organisation, GetShipmentOrgHeaderColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OH_DeliveryAgent), BindingLists.OrgForwarder_FilterList);
			deliveryAgentFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.DeliveryAgent, OrgHeaderSchema.OH_Code.MaxLength);
			deliveryAgentFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|DeliveryAgent", "Delivery Agent");
			deliveryAgentFilter.SupportsBlankComparisonOperators = true;
			filters.AddFilter(deliveryAgentFilter);

			var pickupAgentFilter = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.PickupAgent, ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.PickupAgent), BindingLists.OrgForwarder_FilterList);
			pickupAgentFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.PickupAgent, OrgHeaderSchema.OH_Code.MaxLength);
			pickupAgentFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|PickupAgent", "Pickup Agent");
			pickupAgentFilter.MultiValueQueryDelegate = GetDocAddressMultiValueQueryDelegate(DocAddressType.PickupAgent);
			filters.AddFilter(pickupAgentFilter);

			var controllingCustomerFilter = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.ControllingCustomer, ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.ControllingCustomer), BindingLists.OrgControllingCustomerFilterList);
			controllingCustomerFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ControllingCustomer, OrgHeaderSchema.OH_Code.MaxLength);
			controllingCustomerFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ControllingCustomer", "Controlling Customer");
			controllingCustomerFilter.MultiValueQueryDelegate = GetDocAddressMultiValueQueryDelegate(DocAddressType.ControllingCustomer);
			filters.AddFilter(controllingCustomerFilter);

			var controllingAgentFilter = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.ControllingAgent, ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.ControllingAgent), BindingLists.OrgControllingAgentFilterList);
			controllingAgentFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ControllingAgent, OrgHeaderSchema.OH_Code.MaxLength);
			controllingAgentFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ControllingAgent", "Controlling Agent");
			controllingAgentFilter.MultiValueQueryDelegate = GetDocAddressMultiValueQueryDelegate(DocAddressType.ControllingAgent);
			filters.AddFilter(controllingAgentFilter);

			var importBrokerFilter = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.ImportBroker, ModuleIDs.Organisation, GetShipmentOrgHeaderColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OH_ImportBroker), BindingLists.OrgMiscServBroker_FilterList);
			importBrokerFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ImportBroker, OrgHeaderSchema.OH_Code.MaxLength);
			importBrokerFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ImportBroker", "Import Broker");
			filters.AddFilter(importBrokerFilter);

			var exportBrokerFilter = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.ExportBroker, ModuleIDs.Organisation, GetShipmentOrgHeaderColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OH_ExportBroker), BindingLists.OrgMiscServBroker_FilterList);
			exportBrokerFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ExportBroker, OrgHeaderSchema.OH_Code.MaxLength);
			exportBrokerFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ExportBroker", "Export Broker");
			filters.AddFilter(exportBrokerFilter);

			var cfs = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.CFS, ModuleIDs.Organisation, GetCFSWithOperatorQuery, BindingLists.PackDepot_FilterList);
			cfs.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|CFS", "CFS");
			filters.AddFilter(cfs);

			var deliveryPostCode = filters.AddTextFilter("Delivery Address Post Code", GetDeliveryPostCodeQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_PostCode);
			deliveryPostCode.Category = FilterCategories.Locations;
			deliveryPostCode.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|DeliveryPostCode", "Delivery Address Post Code");

			var pickupPostCode = filters.AddTextFilter("Pickup Address Post Code", GetPickupPostCodeQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_PostCode);
			pickupPostCode.Category = FilterCategories.Locations;
			pickupPostCode.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|PickupPostCode", "Pickup Address Post Code");

			var consignorTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value.IsEmpty ? (ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue : ((ZString)FreightDataRegistry.Instance.ConsignorShipperTerminology.Value).SubstringSafe(0, 15);

			var consignorFilter = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.Consignor, ModuleIDs.Organisation, GetConsignorQuery, BindingLists.OrgConsignor_FilterList);
			consignorFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ConsignorDocumentaryAddress, OrgHeaderSchema.OH_Code.MaxLength);
			consignorFilter.MultilingualDescription = ResString.GetMultilingualString("eac01866-105a-cfa6-48c1-02cb5d5fde93", "{0}", consignorTerminology);
			consignorFilter.SupportsBlankComparisonOperators = true;
			consignorFilter.MultiValueQueryDelegate = GetDocAddressMultiValueQueryDelegate(DocAddressType.ConsignorDocumentaryAddress);
			filters.AddFilter(consignorFilter);

			var consigneeFilter = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.Consignee, ModuleIDs.Organisation, GetConsigneeQuery, BindingLists.OrgConsignee_FilterList);
			consigneeFilter.XQueryInfo = new XQueryFilterInfo(ShipmentXQueryPaths.ConsigneeDocumentaryAddress, OrgHeaderSchema.OH_Code.MaxLength);
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Consignee", "Consignee");
			consigneeFilter.SupportsBlankComparisonOperators = true;
			consigneeFilter.MultiValueQueryDelegate = GetDocAddressMultiValueQueryDelegate(DocAddressType.ConsigneeDocumentaryAddress);
			filters.AddFilter(consigneeFilter);

			filters.AddGuidFilter(Descriptions.OrganisationsStaff.Carrier, ModuleIDs.Organisation, GetCarrierQuery, BindingLists.OrgHeader_List).MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Carrier", "Carrier");
			filters.AddGuidFilter(Descriptions.OrganisationsStaff.Client, ModuleIDs.Organisation, GetClientQuery, BindingLists.OrgHeader_List).MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Client", "Client");
			filters.AddGuidFilter(Descriptions.OrganisationsStaff.Creditor, ModuleIDs.Organisation, GetShipmentOrgHeaderColumnQueryWithOperatorDelegate(JobShipmentSchema.JS_OH_Creditor), BindingLists.OrgHeader_List).MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Creditor", "Creditor");

			var pickupTransportCompany = new ModuleGuidFilterForOrg(Descriptions.OrganisationsStaff.PickupTransport, ModuleIDs.Organisation, GetCartageCompanyQueryComparisonDelegate(JobDocsAndCartageSchema.JP_OA_PickupCartageCoAddr), BindingLists.ShippingProvider_FilterList);
			pickupTransportCompany.MultilingualDescription = ResString.GetMultilingualString("Forwarding|QuotedBookingFilterControl|PickupTransport", "Pickup Transport");
			pickupTransportCompany.SupportsBlankComparisonOperators = true;
			filters.AddFilter(pickupTransportCompany);
		}

		#region GetSalesRepFilter

		protected override ModuleFilter GetSalesRepFilter()
		{
			return new ModuleNkFilter(Descriptions.OrganisationsStaff.SalesRepresentative, GetSalesRepQuery, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
		}

		ZQuery GetSalesRepQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString salesRepNK)
		{
			ZDBOnlyQuery salesRepQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			ZDBOnlySubQuery quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			if (filtersMatchQuery == null)
			{
				jobSubQuery.AddToFilter(JobHeaderSchema.JH_GS_NKRepSales, comparisonOperator, salesRepNK);
			}
			else
			{
				jobSubQuery.AddSubQuery(JobHeaderSchema.JH_GS_NKRepSales, GlbStaffSchema.GS_Code, filtersMatchQuery, JoinCondition.And);
			}
			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			quoteSubQuery.AddSubQuery(jobSubQuery, JoinCondition.And);

			salesRepQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			salesRepQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, jobSubQuery, JoinCondition.Or);

			return salesRepQuery;
		}

		#endregion

		#region GetCarrierQuery

		protected ZQuery GetCarrierQuery(SQLComparisonOperator comparisonOperator, object pk)
		{
			var viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			viewFilter.IgnoreActiveFilter = true;

			var bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobShipmentSchema.JS_OA_BookedShippingLineAddress, notIn);

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				bookingSubQuery.AddToFilter(JobShipmentSchema.JS_OA_BookedShippingLineAddress, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pk);
				bookingSubQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
			}

			viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			return viewFilter;
		}

		#endregion

		#region GetClientQuery

		protected override ZQuery GetClientNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var result = new ZQuery();

			var isBlankOrNot = comparisonOperator == SpecialComparisonOperator.IsBlank
				|| comparisonOperator == SQLComparisonOperator.DoesNotStartWith
				|| comparisonOperator == SQLComparisonOperator.NotContains
				|| comparisonOperator == SQLComparisonOperator.NotEqual;

			var condtion = isBlankOrNot ? JoinCondition.Or : JoinCondition.And;
			var operatorForJob = isBlankOrNot ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;

			// JobHeader

			var orgHeaderBookingSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaderBookingSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, companyName);

			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			addressQuery.AddSubQuery(orgHeaderBookingSubQuery, JoinCondition.And);

			var jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobSubQuery.AddSubQuery(addressQuery, JoinCondition.And);
			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobSubQuery.AddToFilter(condtion, JobHeaderSchema.JH_OA_LocalChargesAddr, operatorForJob, null);

			var noneJobQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);

			//Booking

			var bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			bookingSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_TH, null);
			bookingSubQuery.AddSubQuery(jobSubQuery, JoinCondition.And);

			var bookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			bookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			var noneJobBookingSubQuery = new ZDBOnlySubQuery(typeof(ViewQuotedBooking), ViewQuotedBookingSchema.VB_JS);
			noneJobBookingSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_TH, null);
			noneJobBookingSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, null);
			noneJobBookingSubQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, noneJobQuery, JoinCondition.And);

			var noneJobBookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			noneJobBookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, noneJobBookingSubQuery, JoinCondition.And);

			//Quote

			var quoteJobSubQuery = new ZDBOnlySubQuery(typeof(ViewQuotedBooking), ViewQuotedBookingSchema.VB_TH);
			quoteJobSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, null);
			quoteJobSubQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, jobSubQuery, JoinCondition.And);

			var quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteJobSubQuery, JoinCondition.Or);

			//Join

			result.AddToFilter(bookingQuery, JoinCondition.Or);
			result.AddToFilter(quoteQuery, JoinCondition.Or);

			if (isBlankOrNot)
			{
				result.AddToFilter(noneJobBookingQuery, JoinCondition.Or);
			}

			return result;
		}

		protected ZQuery GetClientQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
				var joinCondition = comparisonOperator == SQLComparisonOperator.IsBlank ? JoinCondition.And : JoinCondition.Or;
				return GetClientQuery(SQLComparisonOperator.NotEqual, ZGuid.Empty, notIn, joinCondition);
			}

			return GetClientQuery(comparisonOperator, (ZGuid)value, false, JoinCondition.Or);
		}

		ZQuery GetClientQuery(SQLComparisonOperator valueComparisonOperator, ZGuid value, bool notIn, JoinCondition joinCondition)
		{
			ZQuery result = new ZQuery();
			ZString addressCode = DocAddressTypes.GetCode(Factory, DocAddressType.QuotationClientAddress);

			// DocAddress

			ZDBOnlySubQuery docAddresses = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			ZDBOnlySubQuery orgAddresses = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddresses.AddToFilter(OrgAddressSchema.OA_OH, valueComparisonOperator, value);
			docAddresses.AddSubQuery(orgAddresses, JoinCondition.And);
			docAddresses.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, addressCode);

			//Booking

			ZDBOnlyQuery bookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			ZDBOnlySubQuery bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK, notIn);

			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, valueComparisonOperator, value);
			jobSubQuery.AddSubQuery(addressQuery, JoinCondition.And);

			bookingSubQuery.AddSubQuery(jobSubQuery, JoinCondition.And);

			bookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			//Quote

			ZDBOnlyQuery quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			ZDBOnlySubQuery quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK, notIn);

			quoteSubQuery.AddSubQuery(RatingHeaderSchema.PK, docAddresses, JoinCondition.And);
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.Or);

			//Join
			result.AddToFilter(bookingQuery, joinCondition);
			result.AddToFilter(quoteQuery, joinCondition);
			return result;
		}

		#endregion

		#region GetCartageCompanyQueryComparisonDelegate
		GetGuidQueryWithOperator GetCartageCompanyQueryComparisonDelegate(SchemaColumn addressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) =>
			{
				// note: directly copied from shipment
				var notIn = comparisonOperator == SpecialComparisonOperator.IsBlank;
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingShipment));
				ZDBOnlySubQuery jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID, notIn);
				ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), addressColumn);
				if (comparisonOperator != SpecialComparisonOperator.IsBlank && comparisonOperator != SpecialComparisonOperator.IsNotBlank)
				{
					addressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
				}
				jobDocsAndCartageQuery.AddSubQuery(addressQuery, JoinCondition.And);
				result.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);
				return GetWithBookingQuery(result);
			};
		}

		#endregion

		#region GetBookingPartyQuery

		ZQuery GetBookingPartyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			ZQuery result = new ZQuery();

			if (!companyName.IsEmpty)
			{
				ZDBOnlyQuery bookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				ZDBOnlySubQuery bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);

				bookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

				ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);

				orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, companyName);
				orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.BookingPartyDocumentaryAddress));
				docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				bookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, docAddressSubQuery, JoinCondition.And);

				result.AddToFilter(bookingQuery);
			}

			return result;
		}

		GetGuidQueryWithOperator GetDocAddressQueryWithOperatorDelegate(DocAddressType addressType)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetDocAddressQueryWithOperator(pK, addressType, comparisonOperator);
		}

		#endregion

		#region GetConsignorConsigneeQuery

		ZQuery GetConsignorQuery(SQLComparisonOperator comparisonOperator, object pk)
		{
			return GetDocAddressQueryWithOperator(pk, DocAddressType.ConsignorDocumentaryAddress, comparisonOperator);
		}

		ZQuery GetConsigneeQuery(SQLComparisonOperator comparisonOperator, object pk)
		{
			return GetDocAddressQueryWithOperator(pk, DocAddressType.ConsigneeDocumentaryAddress, comparisonOperator);
		}

		#endregion

		#region GetRelatedPartiesQuery

		protected override ZQuery GetClientRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery jobHeader = OrgRelatedPartiesFilterHelper.GetJobHeaderFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			ZQuery bookingQuery = GetBookingFromJobHeaderFilter(jobHeader);

			ZQuery docAddress = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddress.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.QuotationClientAddress);
			ZQuery quoteQuery = GetQuoteFromDocAddressFilter(docAddress);

			ZQuery result = new ZQuery();
			result.AddToFilter(bookingQuery, JoinCondition.Or);
			result.AddToFilter(quoteQuery, JoinCondition.Or);

			return result;
		}

		protected override ZQuery GetConsignorRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			var docAddress = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddress.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ConsignorDocumentaryAddress);

			return GetBookingFromDocAddressFilter(docAddress);
		}

		protected override ZQuery GetConsigneeRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			var docAddress = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddress.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ConsigneeDocumentaryAddress);

			return GetBookingFromDocAddressFilter(docAddress);
		}

		#endregion

		#region GetShipmentOrgHeaderColumnQueryWithOperatorDelegate

		GetGuidQueryWithOperator GetShipmentOrgHeaderColumnQueryWithOperatorDelegate(SchemaColumn shipmentColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) =>
			{
				if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
				{
					var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
					return GetWithBookingQuery(new ZQuery(shipmentColumn, SQLComparisonOperator.NotEqual, null), notIn);
				}
				return GetWithBookingQuery(new ZQuery(shipmentColumn, comparisonOperator, pK));
			};
		}

		#endregion

		#endregion

		#region PostCode queries

		ZQuery GetPickupPostCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPostCodeQuery(AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress, comparisonOperator, value);
		}

		ZQuery GetDeliveryPostCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetPostCodeQuery(AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress, comparisonOperator, value);
		}

		ZQuery GetPostCodeQuery(string addressType, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ForwardingShipment));

			ZDBOnlySubQuery jobDocAddressFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, "N");
			jobDocAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			jobDocAddressFilter.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobShipmentSchema.Constants.Prefix);

			ZDBOnlySubQuery orgAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressFilter.AddToFilter(OrgAddressSchema.OA_PostCode, comparisonOperator, value);
			jobDocAddressFilter.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressFilter, JoinCondition.And);

			query.AddSubQuery(jobDocAddressFilter, JoinCondition.And);

			ZDBOnlySubQuery jobDocAddressOverrideFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, "Y");
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			jobDocAddressOverrideFilter.AddToFilter(JobDocAddressSchema.E2_Postcode, comparisonOperator, value);

			query.AddSubQuery(jobDocAddressOverrideFilter, JoinCondition.Or);

			return GetWithBookingQuery(query);
		}

		#endregion

		#region CFS query
		ZQuery GetCFSWithOperatorQuery(SQLComparisonOperator comparisonOperator, object pk)
		{
			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
				return GetWithBookingQuery(GetCFSQueryShipment(new ZQuery(OrgAddressSchema.OA_OH, SQLComparisonOperator.NotEqual, ZGuid.Empty)), notIn);
			}
			return GetWithBookingQuery(GetCFSQueryShipment(new ZQuery(OrgAddressSchema.OA_OH, comparisonOperator, pk)));
		}

		ZDBOnlyQuery GetCFSQueryShipment(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(ForwardingShipment));

			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobShipmentSchema.JS_OA_ExportReceivingDepot);
			addressQuery.AddToFilter(filter);
			result.AddSubQuery(addressQuery, JoinCondition.And);

			return result;
		}

		#endregion

		protected override ZQuery GetCompanyTariffLevelOverride(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				ZByte.TryParse(value, out var companyTariffLevelOverride);
				result.AddToFilter(GetWithBookingQuery(JobShipmentSchema.JS_CompanyTariffLevelOverride, SQLComparisonOperator.Equal, companyTariffLevelOverride));
			}
			return result;
		}

		protected override void AddFMCTariffIDFilter(ModuleFilterCollection filters)
		{
			var fmcTariffIDFilter = filters.AddTextFilter(Descriptions.NumbersAndReferences.FMCTariffID, GetFMCTariffIDQuery);
			fmcTariffIDFilter.MaxLength = JobShipmentSchema.JS_FMCTariffID.MaxLength;
			fmcTariffIDFilter.Category = FilterCategories.NumbersAndReferences;
			fmcTariffIDFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|FMCTariffID", "FMC Tariff ID");
		}

		ZQuery GetFMCTariffIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery();
			result.AddToFilter(GetWithBookingQuery(JobShipmentSchema.JS_FMCTariffID, comparisonOperator, value));
			return result;
		}

		protected override void AddCommodityCodeFilter(ModuleFilterCollection filters)
		{
			var rateCommodityFilter = filters.AddNkFilter("Commodity Code", GetCommodityCodeQuery, ModuleIDs.RefCommodityCode, BindingLists.RefCommodityCode_List);
			rateCommodityFilter.Category = FilterCategories.NumbersAndReferences;
			rateCommodityFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|CommodityCode", "Commodity Code");
		}

		ZQuery GetCommodityCodeQuery(ZString value)
		{
			var result = new ZQuery();
			result.AddToFilter(GetWithBookingQuery(JobShipmentSchema.JS_RH_NKRateCommodity, SQLComparisonOperator.Equal, value));
			return result;
		}

		protected void AddHBLDeliveryModeFilter(ModuleFilterCollection filters)
		{
			var hBLDeliveryModeList = RateEntryLookups.GetHBLDeliveryModeList(string.Empty, string.Empty);
			var hBLDeliveryModeFilter = filters.AddTextFilter(Descriptions.ModesAndTypes.HBLDeliveryMode, GetHBLDeliveryModeQuery, hBLDeliveryModeList);
			hBLDeliveryModeFilter.Category = FilterCategories.ModesAndTypes;
			hBLDeliveryModeFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|HBLDeliveryMode", "HBL Delivery Mode");
		}

		ZQuery GetHBLDeliveryModeQuery(ZString value)
		{
			var result = new ZQuery();
			result.AddToFilter(GetWithBookingQuery(JobShipmentSchema.JS_HBLContainerPackModeOverride, SQLComparisonOperator.Equal, value));
			return result;
		}

		#region Modes And Types

		#region GetModeQuery

		protected override ZQuery GetModeQuery(ZString value)
		{
			if (!value.IsEmpty)
			{
				var isMainTransportMode = FreightCodePairLists.JS_PackingModeList(value).Count > 0;

				var bookingQuery = new ZQuery();
				bookingQuery.AddToFilter(GetWithBookingQuery(JobShipmentSchema.JS_TransportMode, SQLComparisonOperator.Equal, RatingConstants.GetTransportModeFromMode(value)), JoinCondition.And);
				if (!isMainTransportMode)
				{
					bookingQuery.AddToFilter(GetWithBookingQuery(JobShipmentSchema.JS_PackingMode, SQLComparisonOperator.Equal, RatingConstants.GetContainerModeFromMode(value)), JoinCondition.And);
				}

				return bookingQuery;
			}

			return new ZQuery();
		}

		protected override ZQuery GetModeXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleTextFilter;

			if (!filter.Property.IsEmpty)
			{
				var value = filter.Property;

				var isMainTransportMode = FreightCodePairLists.JS_PackingModeList(value).Count > 0;

				var bookingQuery = new ZQuery();

				bookingQuery.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.Equal, RatingConstants.GetTransportModeFromMode(value)), new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, JobShipmentSchema.JS_TransportMode.MaxLength)));
				if (!isMainTransportMode)
				{
					bookingQuery.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.Equal, RatingConstants.GetContainerModeFromMode(value)), new XQueryFilterInfo(ShipmentXQueryPaths.ContainerMode, JobShipmentSchema.JS_PackingMode.MaxLength)));
				}

				return bookingQuery;
			}

			return new ZQuery();
		}

		protected override ZQuery GetTransportModeQuery(ZString value)
		{
			var bookingQuery = new ZQuery();
			if (!value.IsEmpty)
			{
				bookingQuery.AddToFilter(GetWithBookingQuery(JobShipmentSchema.JS_TransportMode, SQLComparisonOperator.Equal, value), JoinCondition.And);
			}

			return bookingQuery;
		}

		protected override ZQuery GetTransportModeXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleTextFilter;
			var bookingQuery = new ZQuery();

			if (!filter.Property.IsEmpty)
			{
				bookingQuery.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.Equal, filter.Property), new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, JobShipmentSchema.JS_TransportMode.MaxLength)));
			}

			return bookingQuery;
		}

		protected override ZQuery GetContainerModeQuery(ZString value)
		{
			var bookingQuery = new ZQuery();

			if (!value.IsEmpty)
			{
				bookingQuery.AddToFilter(GetWithBookingQuery(JobShipmentSchema.JS_PackingMode, SQLComparisonOperator.Equal, value), JoinCondition.And);
			}

			return bookingQuery;
		}

		protected override ZQuery GetContainerModeXQuery(ModuleFilter moduleFilter)
		{
			var filter = moduleFilter as ModuleTextFilter;

			var bookingQuery = new ZQuery();

			if (!filter.Property.IsEmpty)
			{
				bookingQuery.AddToFilter(XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SQLComparisonOperator.Equal, filter.Property), new XQueryFilterInfo(ShipmentXQueryPaths.ContainerMode, JobShipmentSchema.JS_PackingMode.MaxLength)));
			}

			return bookingQuery;
		}

		#endregion

		#region GetServiceLevelQuery

		protected override ZQuery GetServiceLevelQuery(ZString value)
		{
			return value.IsEmpty ? new ZQuery() : GetWithBookingQuery(JobShipmentSchema.JS_RS_NKServiceLevel, SQLComparisonOperator.Equal, value);
		}

		#endregion

		#endregion

		#region Implementation

		ZQuery GetBookingFromJobHeaderFilter(ZQuery jobHeaderFilter)
		{
			ZDBOnlySubQuery jobHeader = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobHeader.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobHeader.AddToFilter(jobHeaderFilter);

			ZDBOnlySubQuery bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			bookingSubQuery.AddSubQuery(jobHeader, JoinCondition.And);

			ZDBOnlyQuery bookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			bookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

			return bookingQuery;
		}

		#endregion

		#region CRM Security

		readonly QuotedBookingCRMSecurityProvider securityProvider = new QuotedBookingCRMSecurityProvider();

		#endregion

		#region GetDocAddressQueryWithOperator

		protected ZQuery GetDocAddressQueryWithOperator(object pK, DocAddressType addressType, SQLComparisonOperator comparisonOperator)
		{
			ZQuery result = new ZQuery();
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			ZDBOnlyQuery bookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			bookingQuery.IgnoreActiveFilter = true;

			ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			}
			else
			{
				ZGuid orgPK = (ZGuid)pK;
				if (!orgPK.IsValid)
				{
					return new ZQuery();
				}
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, orgPK);
			}
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			bookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_JS, docAddressSubQuery, JoinCondition.And);

			result.AddToFilter(bookingQuery);

			return result;
		}

		Func<object, SQLComparisonOperator, ZQuery> GetDocAddressMultiValueQueryDelegate(DocAddressType addressType)
		{
			return (object value, SQLComparisonOperator filterOperator) => DocAddressMultiValueQuery(value, filterOperator, addressType);
		}

		ZQuery DocAddressMultiValueQuery(object value, SQLComparisonOperator filterOperator, DocAddressType docAddressType)
		{
			var result = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			result.IgnoreActiveFilter = true;

			if (value is List<ZGuid> orgList && orgList.Count > 0)
			{
				var docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);

				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddressSubQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_OH, filterOperator, orgList);

				docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, docAddressType));
				docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

				result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, docAddressSubQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region Sub Groups

		protected ModuleFilterSubGroup JobShipmentSubGroup
		{
			get { return jobShipmentSubGroup ?? (jobShipmentSubGroup = new JobShipmentFilterSubGroup()); }
		}
		JobShipmentFilterSubGroup jobShipmentSubGroup;

		ModuleFilterSubGroup JobDocsAndCartageSubGroup
		{
			get { return jobDocsAndCartageSubGroup ?? (jobDocsAndCartageSubGroup = new JobDocsAndCartageFilterSubGroup()); }
		}
		JobDocsAndCartageFilterSubGroup jobDocsAndCartageSubGroup;

		class JobShipmentFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ViewQuotedBooking)) { IgnoreActiveFilter = true };

				var bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
				bookingSubQuery.AddToFilter(filter);

				result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);
				return result;
			}
		}

		class JobDocsAndCartageFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				var bookingSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);

				var jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				jobDocsAndCartageQuery.AddToFilter(filter);

				bookingSubQuery.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);
				result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, bookingSubQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion
	}
}
