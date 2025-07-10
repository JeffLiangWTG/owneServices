using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ContainerLoadListFilterBusinessObject : FilterStripBusinessObject
	{
		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(CYContainerLoadList), WorkflowDescriptors.ContainerLoadListWorkflowDescriptorCode, Factory);
			helpers.Add(helper);

			return helpers;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Load List #", ContainerLoadListHeaderSchema.CLH_LoadListId).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|LoadList", "Load List #");
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_LoadListId.MaxLength;
				});

			filters.AddTextFilter("Supplier Booking #", GetJobSupplierBookingTextFieldQuery(JobSupplierBookingSchema.JSB_BookingId)).With(
				filter =>
				{
					filter.IsCommon = true;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|SupplierBooking", "Supplier Booking #");
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.MaxLength = JobSupplierBookingSchema.JSB_BookingId.MaxLength;
				});

			filters.AddTextFilter("Transport Mode", GetJobSupplierBookingTextFieldQuery(JobSupplierBookingSchema.JSB_TransportMode), TransportMode_List).With(
				filter =>
				{
					filter.Category = FilterCategories.ModesAndTypes;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|TransportMode", "Transport Mode");
					filter.MaxLength = JobSupplierBookingSchema.JSB_TransportMode.MaxLength;
				});

			filters.AddTextFilter("INCO Term", GetJobSupplierBookingTextFieldQuery(JobSupplierBookingSchema.JSB_IncoTerm), IncoTermList).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|INCOTerm", "INCO Term");
					filter.MaxLength = JobSupplierBookingSchema.JSB_IncoTerm.MaxLength;
				});

			filters.AddTextFilter("Consol #", GetConsolTextFieldQuery(JobConsolSchema.JK_UniqueConsignRef)).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|JK_UniqueConsignRef", "Consol ID");
					filter.MaxLength = JobConsolSchema.JK_UniqueConsignRef.MaxLength;
				});

			filters.AddTextFilter("Booking Reference", GetConsolTextFieldQuery(JobConsolSchema.JK_BookingReference)).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|JK_BookingReference", "Booking Reference");
					filter.MaxLength = JobConsolSchema.JK_BookingReference.MaxLength;
				});

			filters.AddTextFilter("Master Bill", GetConsolTextFieldQuery(JobConsolSchema.JK_MasterBillNum)).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|JK_MasterBillNum", "Master Bill");
					filter.MaxLength = JobConsolSchema.JK_MasterBillNum.MaxLength;
				});

			filters.AddGuidFilter("Carrier", ModuleIDs.Organisation, GetConsolCarrierAddress(), BindingLists.ShippingProvider_FilterList).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|JK_OA_ShippingLineAddress", "Carrier");
					filter.SupportsBlankComparisonOperators = true;
				});

			filters.AddTextFilter("Container #", GetContainerTextFieldQuery(JobContainerSchema.JC_ContainerNum)).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|JC_ContainerNum", "Container Number");
					filter.MaxLength = JobContainerSchema.JC_ContainerNum.MaxLength;
				});

			filters.AddTextFilter("Container Type", GetContainerTypeQuery).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|RC_Code", "Container Type");
					filter.MaxLength = RefContainerSchema.RC_Code.MaxLength;
				});

			filters.AddGuidFilter("Load List Party", ModuleIDs.Organisation, ContainerLoadListHeaderSchema.CLH_OH_LoadListParty, BindingLists.Organisations).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|CLH_OH_LoadListParty", "Load List Party");
					filter.Visibility = FilterVisibility.AlwaysVisible;
				});

			filters.AddTextFilter("Load List Status", ContainerLoadListHeaderSchema.CLH_Status, StatusList).With(
				filter =>
				{
					filter.Category = FilterCategories.StatusAndFlags;
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|CLH_Status", "Load List Status");
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_Status.MaxLength;
				});

			filters.AddTextFilter("Controlling Customer Company Name", (comparisonOperator, name) => GetDocAddressNameFilter(comparisonOperator, name, DocAddressType.ControllingCustomer)).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|ControllingCustomerCompanyName", "Controlling Customer Company Name");
					filter.MaxLength = 200;
				});

			filters.AddGuidFilter("Controlling Customer", ModuleIDs.Organisation, pk => GetDocAddressFilter(pk, DocAddressType.ControllingCustomer), BindingLists.OrgHeader_List).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|ControllingCustomer", "Controlling Customer");
					filter.MaxLength = 200;
				});

			filters.AddTextFilter("Supplier Company Name", (comparisonOperator, name) => GetDocAddressNameFilter(comparisonOperator, name, DocAddressType.SupplierDocumentaryAddress)).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|SupplierCompanyName", "Supplier Company Name");
					filter.MaxLength = 200;
				});

			filters.AddGuidFilter("Supplier", ModuleIDs.Organisation, pk => GetDocAddressFilter(pk, DocAddressType.SupplierDocumentaryAddress), BindingLists.OrgHeader_List).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|Supplier", "Supplier");
					filter.MaxLength = 200;
				});

			filters.AddLocationFilter("Load / Discharge (Booking)", GetLoadDischargePortsForBookingQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List).With(
				filter =>
				{
					filter.SetItemDescriptions(Res.GetData("Forwarding|ContainerLoadListFilter|BookingLoad", "Load"), Res.GetData("Forwarding|ContainerLoadListFilter|BookingDischarge", "Discharge"));
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|BookingLoadDischarge", "Load / Discharge (Booking)");
				});

			filters.AddLocationFilter("Load / Discharge (Carrier)", GetLoadDischargePortsForCarrierQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List).With(
				filter =>
				{
					filter.SetItemDescriptions(Res.GetData("Forwarding|ContainerLoadListFilter|CarrierLoad", "Load"), Res.GetData("Forwarding|ContainerLoadListFilter|CarrierDischarge", "Discharge"));
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|CarrierLoadDischarge", "Load / Discharge (Carrier)");
				});

			filters.AddTextFilter("Order No", GetOrderNumberQuery).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|OrderNumber", "Order Number");
					filter.MaxLength = JobOrderHeaderSchema.JD_OrderNumber.MaxLength;
				});

			filters.AddTextFilter("Seal Number", GetContainerTextFieldQuery(JobContainerSchema.JC_SealNum)).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|SealNumber", "Seal Number");
					filter.MaxLength = JobContainerSchema.JC_SealNum.MaxLength;
				});

			filters.AddTextFilter("Part No", GetOrderLineTextFieldQuery(JobOrderLineSchema.JO_Partno)).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|PartNo", "Part No");
					filter.MaxLength = JobOrderLineSchema.JO_Partno.MaxLength;
				});

			return filters;
		}

		CodeDescriptionPairList StatusList => statusList ?? (statusList = new CommonContainerLoadListStatusList());

		CodeDescriptionPairList statusList;

		BindToLists BindingLists => BindToLists.GetCachedLists(Factory);

		ZQuery GetOrderNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
			orderSubQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, value);

			var orderLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
			orderLineSubQuery.AddSubQuery(JobOrderLineSchema.JO_JD, orderSubQuery, JoinCondition.And);

			var supplierBookingLineSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JSB_Booking);
			supplierBookingLineSubQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
			result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, supplierBookingLineSubQuery, JoinCondition.And);
			return result;
		}

		GetTextQueryWithOperator GetOrderLineTextFieldQuery(SchemaColumn filterColumn)
		{
			return (SQLComparisonOperator comparisonOperator, ZString value) =>
			{
				var orderLineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
				orderLineSubQuery.AddToFilter(filterColumn, comparisonOperator, value);

				var supplierBookingLineSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBookingLine), JobSupplierBookingLineSchema.JSL_JSB_Booking);
				supplierBookingLineSubQuery.AddSubQuery(JobSupplierBookingLineSchema.JSL_JO_OrderLine, orderLineSubQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
				result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, supplierBookingLineSubQuery, JoinCondition.And);
				return result;
			};
		}

		GetTextQueryWithOperator GetJobSupplierBookingTextFieldQuery(SchemaColumn filterColumn)
		{
			return (SQLComparisonOperator comparisonOperator, ZString value) =>
			{
				var supplierBookingSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBooking), JobSupplierBookingSchema.PK);
				supplierBookingSubQuery.AddToFilter(filterColumn, comparisonOperator, value);

				var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
				result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, supplierBookingSubQuery, JoinCondition.And);
				return result;
			};
		}

		ZQuery GetLoadDischargePortsForCarrierQuery(ZString origin, ZString destination)
		{
			var consolSubQuery = new ZDBOnlySubQuery(typeof(Business.ForwardingConsol), JobConsolSchema.PK);
			if (!origin.IsEmpty)
			{
				consolSubQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, origin, JobConsolSchema.JK_RL_NKLoadPort, typeof(Business.ForwardingConsol)));
			}
			if (!destination.IsEmpty)
			{
				consolSubQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, destination, JobConsolSchema.JK_RL_NKDischargePort, typeof(Business.ForwardingConsol)));
			}

			var containerSubQuery = new ZDBOnlySubQuery(typeof(Business.ForwardingContainer), JobContainerSchema.JC_JSB_SupplierBooking);
			containerSubQuery.AddSubQuery(JobContainerSchema.JC_JK, consolSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
			result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, containerSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetLoadDischargePortsForBookingQuery(ZString origin, ZString destination)
		{
			var supplierBookingSubQuery = new ZDBOnlySubQuery(typeof(JobSupplierBooking), JobSupplierBookingSchema.PK);

			if (!origin.IsEmpty)
			{
				supplierBookingSubQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, origin, JobSupplierBookingSchema.JSB_RL_NKLoadPort, typeof(JobSupplierBooking)));
			}
			if (!destination.IsEmpty)
			{
				supplierBookingSubQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, destination, JobSupplierBookingSchema.JSB_RL_NKDischargePort, typeof(JobSupplierBooking)));
			}

			var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
			result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, supplierBookingSubQuery, JoinCondition.And);
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

			var supplierBookingQuery = new ZDBOnlySubQuery(typeof(JobSupplierBooking), JobSupplierBookingSchema.PK);
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobSupplierBookingSchema.Constants.Prefix);
			supplierBookingQuery.AddSubQuery(docAddressQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
			result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, supplierBookingQuery, JoinCondition.And);
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

			var supplierBookingQuery = new ZDBOnlySubQuery(typeof(JobSupplierBooking), JobSupplierBookingSchema.PK);
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobSupplierBookingSchema.Constants.Prefix);
			supplierBookingQuery.AddSubQuery(docAddressQuery, JoinCondition.And);

			var containerLoadListQuery = new ZDBOnlyQuery(typeof(CYContainerLoadList));
			containerLoadListQuery.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, supplierBookingQuery, JoinCondition.And);
			return containerLoadListQuery;
		}

		static ZQuery GetContainerTypeQuery(SQLComparisonOperator comparisonOperator, ZString containerType)
		{
			var refContainerSubQuery = new ZDBOnlySubQuery(typeof(RefContainer), RefContainerSchema.PK);
			refContainerSubQuery.AddToFilter(RefContainerSchema.RC_Code, comparisonOperator, containerType);

			var containerSubQuery = new ZDBOnlySubQuery(typeof(Business.ForwardingContainer), JobContainerSchema.JC_JSB_SupplierBooking);
			containerSubQuery.AddSubQuery(JobContainerSchema.JC_RC, refContainerSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
			result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, containerSubQuery, JoinCondition.And);

			return result;
		}

		static GetTextQueryWithOperator GetContainerTextFieldQuery(SchemaColumn filterColumn)
		{
			return (SQLComparisonOperator comparisonOperator, ZString value) =>
			{
				var containerSubQuery = new ZDBOnlySubQuery(typeof(Business.ForwardingContainer), JobContainerSchema.JC_JSB_SupplierBooking);
				containerSubQuery.AddToFilter(filterColumn, comparisonOperator, value);

				var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
				result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, containerSubQuery, JoinCondition.And);

				return result;
			};
		}

		static GetTextQueryWithOperator GetConsolTextFieldQuery(SchemaColumn filterColumn)
		{
			return (SQLComparisonOperator comparisonOperator, ZString value) =>
			{
				var consolSubQuery = new ZDBOnlySubQuery(typeof(Business.ForwardingConsol), JobConsolSchema.PK);
				consolSubQuery.AddToFilter(filterColumn, comparisonOperator, value);

				var containerSubQuery = new ZDBOnlySubQuery(typeof(Business.ForwardingContainer), JobContainerSchema.JC_JSB_SupplierBooking);
				containerSubQuery.AddSubQuery(JobContainerSchema.JC_JK, consolSubQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
				result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, containerSubQuery, JoinCondition.And);

				return result;
			};
		}

		static GetGuidQueryWithOperator GetConsolCarrierAddress()
		{
			return (SQLComparisonOperator comparisonOperator, object pK) =>
			{
				var consolSubQuery = new ZDBOnlySubQuery(typeof(Business.ForwardingConsol), JobConsolSchema.PK);

				if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
				{
					consolSubQuery.AddToFilter(JobConsolSchema.JK_OA_ShippingLineAddress, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
				}
				else
				{
					var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ShippingLineAddress);
					orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
					consolSubQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
				}

				var containerSubQuery = new ZDBOnlySubQuery(typeof(Business.ForwardingContainer), JobContainerSchema.JC_JSB_SupplierBooking);
				containerSubQuery.AddSubQuery(JobContainerSchema.JC_JK, consolSubQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(CYContainerLoadList));
				result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_JSB_Booking, containerSubQuery, JoinCondition.And);

				return result;
			};
		}

		CodeDescriptionPairList IncoTermList
		{
			get { return incoTermList ?? (incoTermList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)); }
		}
		CodeDescriptionPairList incoTermList;

		CodeDescriptionPairList TransportMode_List
		{
			get { return FreightCodePairLists.LinkableTransportModeList(); }
		}
	}
}
