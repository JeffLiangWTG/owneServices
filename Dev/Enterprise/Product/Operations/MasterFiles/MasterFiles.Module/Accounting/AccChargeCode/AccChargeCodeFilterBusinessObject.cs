using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccChargeCodeFilterBusinessObject : FilterStripBusinessObject
	{
		public AccChargeCodeFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddStatusFilters(filters);
			AddCustomFilters(filters);
			AddAdditionalFilters(filters);
			AddOrganisationFilters(filters);
			return filters;
		}

		#endregion

		#region Text

		protected void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", AccChargeCodeSchema.AC_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", AccChargeCodeSchema.AC_Desc, typeof(AccChargeCode), ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|Description", "Description"));

			var universalChargeCodeFilter = filters.AddTextFilter("Universal Charge Code", GetUniversalChargeCodeMappingQuery);
			universalChargeCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|UniversalChargeCode", "Universal Charge Code");
			universalChargeCodeFilter.MaxLength = AutoAccChargeCodeUniversalCodeMapping.Schema.AUP_CodeMaxLength;
		}

		#endregion

		#region Status

		protected void AddStatusFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter chargeTypeFilter = filters.AddTextFilter("Charge Type", AccChargeCodeSchema.AC_ChargeType, ChargeTypeList);
			chargeTypeFilter.Category = FilterCategories.StatusAndFlags;
			chargeTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|ChargeType", "Charge Type");

			ModuleTextFilter chargeGroupFilter = filters.AddTextFilter("Charge Group", ChargeGroupPanelFilter, ChargeGroupList);
			chargeGroupFilter.Category = FilterCategories.StatusAndFlags;
			chargeGroupFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|ChargeGroup", "Charge Group");

			ModuleTextFilter concolStatusFilter = filters.AddTextFilter("Consol Level Status", GetConsolLevelFilter, ConsolLevelStatusList);
			concolStatusFilter.Category = FilterCategories.StatusAndFlags;
			concolStatusFilter.DefaultProperty = ConsolLevelStatus.Code.All;
			concolStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|ConsolLevelStatus", "Consol Level Status");

			var iataCodeFilter = filters.AddTextFilter("IATA Code", IATACodeFilter, IATACodeList);
			iataCodeFilter.Category = FilterCategories.StatusAndFlags;
			iataCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|IATACode", "IATA Code");

			var airlineIataCodeFilter = filters.AddTextFilter("Airline IATA Code", AirlineIATACodeFilter, IATACodeList);
			airlineIataCodeFilter.Category = FilterCategories.StatusAndFlags;
			airlineIataCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|AirlineIATACode", "Airline IATA Code");
		}

		ZQuery GetConsolLevelFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == ConsolLevelStatus.Code.Consol)
			{
				query.AddToFilter(AccChargeCodeSchema.AC_IsGroupageCharge, ZBool.True);
			}
			else if (value == ConsolLevelStatus.Code.NonConsol)
			{
				query.AddToFilter(AccChargeCodeSchema.AC_IsGroupageCharge, ZBool.False);
			}

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery DepartmentListPanelFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			query.DefaultJoinCondition = JoinCondition.And;

			if (!value.IsEmpty)
			{
				query.AddToFilter(AccChargeCodeSchema.AC_DepartmentFilterList, SQLComparisonOperator.Contains, "ALL");
				ZString[] depts = value.Split(',', ' ');
				foreach (ZString dept in depts)
				{
					if (!dept.IsEmpty)
					{
						query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_DepartmentFilterList, SQLComparisonOperator.Contains, dept.SubstringSafe(0, AccChargeCodeSchema.AC_DepartmentFilterList.MaxLength));
					}
				}
			}
			return query;
		}

		ZQuery ChargeGroupPanelFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			switch (value)
			{
				case AccChargeCodeLookups.OriginAndLoadingGroupFilterCode:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.Origin);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.Loading);
					break;
				case AccChargeCodeLookups.DestinationAndUnloadingGroupFilterCode:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.Destination);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.Unloading);
					break;
				case AccChargeCodeLookups.CFSGroupFilterCode:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.CFSShipment);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.CFSLoadList);
					break;
				case AccChargeCodeLookups.WHSGroupFilterCode:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.WHSInwards);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.WHSOutwards);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.WHSStorage);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.WHSAdHocServiceJob);
					break;
				case AccChargeCodeLookups.TRWGroupFilterCode:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.TRWReceive);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.TRWDispatch);
					break;
				case AccChargeCodeLookups.TWUGroupFilterCode:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
					break;
				case AccChargeCodeLookups.CYDGroupFilterCode:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.YardGateIn);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.YardGateOut);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.YardStorage);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.MNRWorkOrderHeader);					
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.LabourHourRate);
					break;
				case AccChargeCodeLookups.CYUGroupFilterCode:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.YardTransportationUnitGateIn);
					query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, ChargeCodeGroupList.Codes.YardTransportationUnitGateOut);
					break;
				default:
					query.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, SQLComparisonOperator.Equal, value);
					break;
			}

			return query;
		}

		ZQuery IATACodeFilter(ZString value)
		{
			return new ZQuery(AccChargeCodeSchema.AC_IATA_ChargeCodeMap, SQLComparisonOperator.Equal, value);
		}

		ZQuery AirlineIATACodeFilter(ZString value)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(AccChargeCodeCarrierIataMapping), AccChargeCodeCarrierIataMappingSchema.ACI_AC_ChargeCode);
			subQuery.AddToFilter(AccChargeCodeCarrierIataMappingSchema.ACI_IATAChargeCodeMap, value);

			var query = new ZDBOnlyQuery(typeof(AccChargeCode));
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Custom

		protected void AddCustomFilters(ModuleFilterCollection filters)
		{
			AccChargeCodeContainsOnlyModuleFilter deptFilter = new AccChargeCodeContainsOnlyModuleFilter("Dept Filter", DepartmentListPanelFilter);
			deptFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|DeptFilter", "Dept Filter");
			filters.AddCustomFilter(deptFilter);
		}

		#endregion

		#region LocalChargeCodeOnlyFilters

		protected virtual void AddAdditionalFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("LinkedToGlobal", GetLinkedToGlobalQuery, LinkedToGlobalList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.DefaultProperty = LinkedToGlobalOption.Code.All;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|LinkedToGlobal", "Linked To Global Charge Code");

			if (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				filters.AddTextFilter("Government Charge Code", AccChargeCodeSchema.AC_GovtChargeCode).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|GovernmentChargeCode", "Government Charge Code");
			}
		}

		ZQuery GetLinkedToGlobalQuery(ZString code)
		{
			if (code == LinkedToGlobalOption.Code.LinkedToGlobal || code == LinkedToGlobalOption.Code.NotLinkedToGlobal)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.AC_Code, code == LinkedToGlobalOption.Code.NotLinkedToGlobal);
				subQuery.AddToFilter(AccChargeCodeSchema.AC_GC, null);
				var query = new ZDBOnlyQuery(typeof(AccChargeCode));
				query.AddSubQuery(
					AccChargeCodeSchema.AC_Code,
					AccChargeCodeSchema.AC_Code,
					subQuery,
					JoinCondition.And);
				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		ZQuery GetUniversalChargeCodeMappingQuery(SQLComparisonOperator comparisonOperator, ZString code)
		{
			var inOperations = new SQLComparisonOperator[] { SQLComparisonOperator.Contains, SQLComparisonOperator.StartsWith, SQLComparisonOperator.Equal, SQLComparisonOperator.IsNotBlank };
			var notInOperations = new SQLComparisonOperator[] { SQLComparisonOperator.NotContains, SQLComparisonOperator.DoesNotStartWith, SQLComparisonOperator.NotEqual, SQLComparisonOperator.IsBlank };

			var chargeCodeQuery = new ZDBOnlyQuery(typeof(AccChargeCode));
			if (inOperations.Contains(comparisonOperator) || notInOperations.Contains(comparisonOperator))
			{
				var notIn = notInOperations.Contains(comparisonOperator);
				var mappingSubQuery = new ZDBOnlySubQuery(typeof(AccChargeCodeUniversalCodeMapping), AccChargeCodeUniversalCodeMappingSchema.AUP_AC, notIn);

				if (comparisonOperator != SQLComparisonOperator.IsBlank && comparisonOperator != SQLComparisonOperator.IsNotBlank)
				{
					if (inOperations.Contains(comparisonOperator))
					{
						mappingSubQuery.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_Code, comparisonOperator, code);
					}
					else
					{
						mappingSubQuery.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_Code, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), code);
					}
				}

				chargeCodeQuery.AddSubQuery(AccChargeCodeSchema.PK, mappingSubQuery, JoinCondition.And);
			}
			return chargeCodeQuery;
		}

		#endregion

		#region OrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var airlineOrgFilter = filters.AddGuidFilter("IATA Code Airline",
				ModuleIDs.Organisation,
				new GetGuidQueryWithOperator(GetAirlineOrgQueryWithOperator),
				CarrierList);
			airlineOrgFilter.Category = FilterCategories.Organisations;
			airlineOrgFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccChargeCodeFilter|IATACodeAirline", "IATA Code Airline");
			airlineOrgFilter.SupportsBlankComparisonOperators = false;
		}

		ZQuery GetAirlineOrgQueryWithOperator(SQLComparisonOperator comparisonOperator, object carrierPK)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(AccChargeCodeCarrierIataMapping), AccChargeCodeCarrierIataMappingSchema.ACI_AC_ChargeCode);
			subQuery.AddToFilter(AccChargeCodeCarrierIataMappingSchema.ACI_OH_Carrier, comparisonOperator, carrierPK);

			var query = new ZDBOnlyQuery(typeof(AccChargeCode));
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		IBusinessObjectCollection CarrierList => carrierList ?? (carrierList = new OrganisationsFindBoxCollection(Factory, GetCarrierListQuery()));
		IBusinessObjectCollection carrierList;

		ZQuery GetCarrierListQuery()
		{
			var result = new ZDBOnlyQuery(typeof(OrgHeader));
			result.AddToFilter(OrgHeaderSchema.OH_IsAirLine, true);

			var subQuery = new ZDBOnlySubQuery(typeof(AccChargeCodeCarrierIataMapping), AccChargeCodeCarrierIataMappingSchema.ACI_OH_Carrier);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Lookups

		#region ChargeTypeList List

		CodeDescriptionPairList fChargeTypeList;
		public CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				if (fChargeTypeList == null)
				{
					fChargeTypeList = new CodeDescriptionPairList(OLookUpEditType.ChargeTypes);
				}
				return fChargeTypeList;
			}
		}

		#endregion

		#region ChargeGroupList List

		CodeDescriptionPairList fChargeGroupList;
		public CodeDescriptionPairList ChargeGroupList
		{
			get
			{
				if (fChargeGroupList == null)
				{
					fChargeGroupList = new ChargeCodeGroupList();
					fChargeGroupList.AddPair(AccChargeCodeLookups.OriginAndLoadingGroupFilterCode, Res.GetString("Accounting|AccChargeCodeFilter|OriginAndLoadingCharges", "Origin and Loading Charges"));
					fChargeGroupList.AddPair(AccChargeCodeLookups.DestinationAndUnloadingGroupFilterCode, Res.GetString("Accounting|AccChargeCodeFilter|DestinationAndUnloadingCharges", "Destination and Unloading Charges"));
					fChargeGroupList.AddPair(AccChargeCodeLookups.CFSGroupFilterCode, Res.GetString("Accounting|AccChargeCodeFilter|CFSLoadListAndShipmentCharges", "CFS Load List and Shipment Charges"));
					fChargeGroupList.AddPair(AccChargeCodeLookups.WHSGroupFilterCode, Res.GetString("Accounting|AccChargeCodeFilter|ProductWarehouseCharges", "Product Warehouse Charges"));
					fChargeGroupList.AddPair(AccChargeCodeLookups.TRWGroupFilterCode, Res.GetString("Accounting|AccChargeCodeFilter|TransitWarehouseCharges", "Transit Warehouse Charges"));
					fChargeGroupList.AddPair(AccChargeCodeLookups.TWUGroupFilterCode, Res.GetString("Accounting|AccChargeCodeFilter|TransitWarehouseTransportationUnitCharges", "Transit Warehouse Transportation Unit Charges"));
					fChargeGroupList.AddPair(AccChargeCodeLookups.CYDGroupFilterCode, Res.GetString("Accounting|AccChargeCodeFilter|ContainerYardCharges", "Container Yard Charges"));
					fChargeGroupList.AddPair(AccChargeCodeLookups.CYUGroupFilterCode, Res.GetString("Accounting|AccChargeCodeFilter|ContainerYardTransportationUnitCharges", "Container Yard Transportation Unit Charges"));
					fChargeGroupList.Sort();
				}
				return fChargeGroupList;
			}
		}

		#endregion

		#region ConsolLevelStatus List

		public static class ConsolLevelStatus
		{
			public static class Code
			{
				public const string Consol = "CON";
				public const string All = "ALL";
				public const string NonConsol = "NOT";
			}

			public static class Description
			{
				public static string Consol { get { return Res.GetString("Accounting|AccChargeCodeFilter|ConsolLevelOnly", "Consol Level Only"); } }
				public static string All { get { return Res.GetString("Accounting|AccChargeCodeFilter|AllConsolAndNonConsolLavels", "All Consol and Non Consol Levels"); } }
				public static string NonConsol { get { return Res.GetString("Accounting|AccChargeCodeFilter|NonConsolLevelOnly", "Non Consol Level Only"); } }
			}
		}

		public CodeDescriptionPairList ConsolLevelStatusList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(ConsolLevelStatus.Code.All, ConsolLevelStatus.Description.All);
				list.AddPair(ConsolLevelStatus.Code.Consol, ConsolLevelStatus.Description.Consol);
				list.AddPair(ConsolLevelStatus.Code.NonConsol, ConsolLevelStatus.Description.NonConsol);

				return list;
			}
		}

		public static class LinkedToGlobalOption
		{
			public static class Code
			{
				public const string LinkedToGlobal = "LGC";
				public const string All = "ALL";
				public const string NotLinkedToGlobal = "NGC";
			}

			public static class Description
			{
				public static string LinkedToGlobal { get { return Res.GetString("c9f9ae5e-0a61-458f-8b9c-456e8a531326", "Is Linked to Global Code"); } }
				public static string All { get { return Res.GetString("93f54b67-847a-4e4c-ab96-6b478c946eaa", "Both Linked And Not Linked To Global Code"); } }
				public static string NotLinkedToGlobal { get { return Res.GetString("ba1fbebc-9f2a-44a6-9f6f-07793d80a365", "Is Not Linked to Global Code"); } }
			}
		}

		public CodeDescriptionPairList LinkedToGlobalList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();

				list.AddPair(LinkedToGlobalOption.Code.All, LinkedToGlobalOption.Description.All);
				list.AddPair(LinkedToGlobalOption.Code.LinkedToGlobal, LinkedToGlobalOption.Description.LinkedToGlobal);
				list.AddPair(LinkedToGlobalOption.Code.NotLinkedToGlobal, LinkedToGlobalOption.Description.NotLinkedToGlobal);

				return list;
			}
		}

		#endregion

		#region IATACodeList List

		public CodeDescriptionPairList IATACodeList => new UntranslatableCodeDescriptionPairList((NoResString)"IATA AWB values cannot be translated", OLookUpEditType.AWBChargeCodes);

		#endregion

		#endregion
	}
}
