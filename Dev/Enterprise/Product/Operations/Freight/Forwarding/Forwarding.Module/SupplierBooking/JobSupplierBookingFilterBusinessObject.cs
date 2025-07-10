using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobSupplierBookingFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Booking #", JobSupplierBookingSchema.JSB_BookingId).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|Booking", "Booking #");
				filter.Visibility = FilterVisibility.AlwaysVisible;
				filter.MaxLength = 20;
			});

			filters.AddTextFilter("Booking Status", JobSupplierBookingSchema.JSB_Status, StatusList).With(filter =>
			{
				filter.Category = FilterCategories.StatusAndFlags;
				filter.Visibility = FilterVisibility.AlwaysVisible;
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|Status", "Booking Status");
			});

			filters.AddNumberFilter("Part No", GetPartNoFilter).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|PartNo", "Part No");
				filter.Visibility = FilterVisibility.AlwaysVisible;
			});

			filters.AddTextFilter("Load Mode", JobSupplierBookingSchema.JSB_LoadMode, LoadModeList).With(filter =>
			{
				filter.Category = FilterCategories.StatusAndFlags;
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|LoadMode", "Load Mode");
			});

			filters.AddTextFilter("Transport Mode", JobSupplierBookingSchema.JSB_TransportMode, TransportMode_List).With(filter =>
			{
				filter.Category = FilterCategories.ModesAndTypes;
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|TransportMode", "Transport Mode");
			});

			filters.AddLocationFilter("Load / Discharge", GetLoadDischargePortsQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List).With(filter =>
			{
				filter.SetItemDescriptions(Res.GetData("Forwarding|JobSupplierBookingFilter|Load", "Load"), Res.GetData("Forwarding|JobSupplierBookingFilter|Discharge", "Discharge"));
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|LoadDischarge", "Load / Discharge");
			});

			filters.AddDateFilter("Booking Date", JobSupplierBookingSchema.JSB_BookedOnDate).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|BookedOnDate", "Booking Date");
			});

			filters.AddTextFilter("Controlling Customer Company Name", (comparisonOperator, name) => GetDocAddressNameFilter(comparisonOperator, name, DocAddressType.ControllingCustomer)).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|ControllingCustomerCompanyName", "Controlling Customer Company Name");
			});

			filters.AddGuidFilter("Controlling Customer", ModuleIDs.Organisation, pk => GetDocAddressFilter(pk, DocAddressType.ControllingCustomer), BindingLists.OrgHeader_List).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|ControllingCustomer", "Controlling Customer");
				filter.MaxLength = 200;
			});

			filters.AddGuidFilter("Booking Party", ModuleIDs.Organisation, JobSupplierBookingSchema.JSB_OH_BookingParty, BindingLists.Organisations).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|BookingParty", "Booking Party");
			});

			filters.AddTextFilter("Supplier Company Name", (comparisonOperator, name) => GetDocAddressNameFilter(comparisonOperator, name, DocAddressType.SupplierDocumentaryAddress)).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|SupplierCompanyName", "Supplier Company Name");
				filter.MaxLength = 200;
			});

			filters.AddGuidFilter("Supplier", ModuleIDs.Organisation, pk => GetDocAddressFilter(pk, DocAddressType.SupplierDocumentaryAddress), BindingLists.OrgHeader_List).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|Supplier", "Supplier");
			});

			filters.AddDateFilter("Cargo Available Date", JobSupplierBookingSchema.JSB_CargoAvailableDate).With(filter =>
			{
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|CargoAvailableDate", "Cargo Available Date");
			});

			filters.AddTextFilter("INCO Term", JobSupplierBookingSchema.JSB_IncoTerm, IncoTermList).With(filter =>
			{
				filter.Category = FilterCategories.StatusAndFlags;
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|Incoterms", "INCO Term");
			});

			filters.AddNumberFilter("Order #", GetOrderNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(JobSupplierBookingSchema.JSB_OH_BookingParty).With(filter =>
			{
				filter.IsCommon = true;
				filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobSupplierBookingFilter|Order", "Order #");
				filter.MaxLength = 35;
			});

			return filters;
		}

		ZQuery GetLoadDischargePortsQuery(ZString origin, ZString destination)
		{
			ZQuery result = new ZQuery();

			if (!origin.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, origin, JobSupplierBookingSchema.JSB_RL_NKLoadPort, typeof(JobSupplierBooking)));
			}
			if (!destination.IsEmpty)
			{
				result.AddToFilter(LocationHelper.GetLocationFilter(Factory, destination, JobSupplierBookingSchema.JSB_RL_NKDischargePort, typeof(JobSupplierBooking)));
			}

			return result;
		}

		CodeDescriptionPairList TransportMode_List
		{
			get { return FreightCodePairLists.LinkableTransportModeList(); }
		}

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		CodeDescriptionPairList IncoTermList
		{
			get { return incoTermList ?? (incoTermList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)); }
		}
		CodeDescriptionPairList incoTermList;

		CodeDescriptionPairList StatusList
		{
			get { return statusList ?? (statusList = new SupplierBookingStatusList()); }
		}
		CodeDescriptionPairList statusList;

		CodeDescriptionPairList LoadModeList
		{
			get { return loadModeList ?? (loadModeList = new SupplierBookingLoadModeList()); }
		}
		CodeDescriptionPairList loadModeList;

		ZQuery GetOrderNumberQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			var orderLineLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
			orderLineLineSubQuery.AddSubQuery(JobOrderLineSchema.JO_JD, GetOrderNumberSubQuery(comparisonOperator, orderNo), JoinCondition.And);

			var supplierBookingLineSubQuery = new ZDBOnlySubQuery(typeof(AutoJobSupplierBookingLine), JobSupplierBookingLineSchema.PK);
			supplierBookingLineSubQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineLineSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(JobSupplierBooking));
			result.AddSubQuery(JobSupplierBookingSchema.PK, JobSupplierBookingLineSchema.JSL_JSB_Booking, supplierBookingLineSubQuery, JoinCondition.And);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZDBOnlySubQuery GetOrderNumberSubQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			var result = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);

			if (orderNo.Contains('-') || orderNo.Contains(RawDataRegistry.Instance.MultiSearchSeparator.Value))
			{
				var multiOrderNo = new ZString[] { orderNo };
				if (orderNo.Contains(RawDataRegistry.Instance.MultiSearchSeparator.Value))
				{
					multiOrderNo = orderNo.Split(RawDataRegistry.Instance.MultiSearchSeparator.Value);
				}

				foreach (var thisOrderNo in multiOrderNo)
				{
					var subResult = new ZDBOnlyQuery(typeof(Order));
					if (thisOrderNo.Contains('-'))
					{
						var splitOrderNo = thisOrderNo.Split('-');
						var orderNumberSplitName = "Cast(" + JobOrderHeaderSchema.JD_OrderNumberSplit.Name + " As Varchar(10))";
						var orderNumberSplitValue = splitOrderNo[1].Replace("'", "''");

						if (comparisonOperator == SQLComparisonOperator.Equal)
						{
							subResult.AddToFilter_PossiblyCommaSeparated(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, splitOrderNo[0]);
							subResult.AddFilterAndZSQLParameterCollection(orderNumberSplitName + " = '" + orderNumberSplitValue + "'", null);
						}
						else
						{
							subResult.AddToFilter_PossiblyCommaSeparated(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.EndsWith, splitOrderNo[0]);
							subResult.AddFilterAndZSQLParameterCollection(orderNumberSplitName + " like '" + orderNumberSplitValue + "%'", null);
						}
					}
					if (!string.IsNullOrEmpty(thisOrderNo))
					{
						subResult.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, thisOrderNo);
					}

					result.AddToFilter(subResult, comparisonOperator.IsNegativeSQLOperator() ? JoinCondition.And : JoinCondition.Or);
				}
			}
			else
			{
				result.AddToFilter_PossiblyCommaSeparated(JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
			}
			return result;
		}

		ZQuery GetDocAddressNameFilter(SQLComparisonOperator comparisonOperator, ZString name, DocAddressType docAddressType)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, name);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			var docAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, docAddressType));
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, false);

			var addressOverrideQuery = new ZQuery();
			addressOverrideQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, comparisonOperator, name);
			addressOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
			addressOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, docAddressType));
			docAddressQuery.AddToFilter(addressOverrideQuery, JoinCondition.Or);

			return GetDocAddressQueryToJobSupplierBooking(docAddressQuery, docAddressType);
		}

		ZQuery GetPartNoFilter(SQLComparisonOperator comparisonOperator, ZString partNo)
		{
			var orderLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
			orderLineSubQuery.AddToFilter(JobOrderLineSchema.JO_Partno, comparisonOperator, partNo);

			var supplierBookingLineSubQuery = new ZDBOnlySubQuery(typeof(AutoJobSupplierBookingLine), JobSupplierBookingLineSchema.PK);
			supplierBookingLineSubQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(JobSupplierBooking));
			result.AddSubQuery(JobSupplierBookingSchema.PK, JobSupplierBookingLineSchema.JSL_JSB_Booking, supplierBookingLineSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetDocAddressFilter(ZGuid pk, DocAddressType docAddressType)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, pk);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			var docAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, docAddressType));

			return GetDocAddressQueryToJobSupplierBooking(docAddressQuery, docAddressType);
		}

		protected virtual ZQuery GetDocAddressQueryToJobSupplierBooking(ZDBOnlySubQuery docAddressQuery, DocAddressType docAddressType)
		{
			var supplierBookingQuery = new ZDBOnlyQuery(typeof(JobSupplierBooking));
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobSupplierBookingSchema.Constants.Prefix);
			supplierBookingQuery.AddSubQuery(docAddressQuery, JoinCondition.And);
			return supplierBookingQuery;
		}
	}
}
