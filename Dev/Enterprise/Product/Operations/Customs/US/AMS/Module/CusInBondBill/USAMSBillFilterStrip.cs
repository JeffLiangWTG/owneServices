using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Module
{
	public class USAMSBillFilterStrip : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string ApplicationCode = "Application Code";
			public const string JobReference = "Job Reference";
			public const string CarrierSCAC = "Carrier SCAC";
			public const string TransportMode = "Transport Mode";
			public const string OriginalEsitmatedTime = "Estimated Time of Arrival";
			public const string ImportingConveyanceName = "Conveyance Name";
			public const string VoyageNumber = "Voyage Number";
			public const string PortOfUnladingSchD = "Port Of Unlading (Sch. D)";
			public const string PortOfUnladingUNLOCO = "Port Of Unlading (UNLOCO)";
			public const string PortOfLadingSchK = "Port Of Lading (Sch. K)";
			public const string PortOfLadingUNLOCO = "Port Of Lading (UNLOCO)";
			public const string IssuerCode = "Issuer Code";
			public const string BillStatus = "Bill Type";
			public const string FilingStatus = "Filing Status";
			public const string ManifestMessageStatus = "Manifest Message Status";
			public const string ArrivalStatus = "Arrival Status";
			public const string DepartureStatus = "Departure Status";
			public const string LatestISFDisposition = "Latest ISF Disposition";
			public const string LatestPTTDisposition = "Latest PTT Disposition";
			public const string LatestAMSDisposition = "Latest AMS Disposition";
			public const string InBondStatus = "In-Bond Status";
			public const string InBondMessageStatus = "In-Bond Message Status";
			public const string JobCreatedBy = "Job Created By";
			public const string JobCreatedTime = "Job Created Time";
			public const string ActualArrivalDate = "Actual Arrival Date";
			public const string LoadPortUNLOCO = "Load Port (UNLOCO)";
			public const string EstimatedDateofDeparture = "Estimated Date of Departure";
			public const string MasterBillNumber = "Master Bill Number";
			public const string HouseBillNumber = "House Bill Number";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddHeaderFilters(result);
			var scac = result.AddTextFilter(FilterConstants.CarrierSCAC, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(CusInBondHeaderSchema.BH_CarrierSCAC, comparisonOperator, value)));
			scac.Category = FilterCategories.NumbersAndReferences;
			scac.MaxLength = CusInBondHeaderSchema.BH_CarrierSCAC.MaxLength;
			result.AddTextFilter(FilterConstants.TransportMode, new GetTextQuery((value) => GetHeaderQuery(CusInBondHeaderSchema.BH_ImportTransportMode, SQLComparisonOperator.Equal, value)), TransportTypeList.GetConveyanceTransportTypeList(Factory)).Category = FilterCategories.ModesAndTypes;
			result.AddDateFilter(FilterConstants.OriginalEsitmatedTime, new GetDateQuery((comparisonOperator, value1, value2) => GetHeaderQuery(CusInBondHeaderSchema.BH_ETA, comparisonOperator, value1, value2))).Category = FilterCategories.Dates;
			result.AddDateFilter(FilterConstants.EstimatedDateofDeparture, new GetDateQuery((comparisonOperator, value1, value2) => GetHeaderQuery(CusInBondHeaderSchema.BH_FirstExportDate, comparisonOperator, value1, value2))).Category = FilterCategories.Dates;
			var conveyanceName = result.AddTextFilter(FilterConstants.ImportingConveyanceName, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(CusInBondHeaderSchema.BH_ImportConveyanceName, comparisonOperator, value)));
			conveyanceName.Category = FilterCategories.NumbersAndReferences;
			conveyanceName.MaxLength = CusInBondHeaderSchema.BH_ImportConveyanceName.MaxLength;
			var voyageNumber = result.AddTextFilter(FilterConstants.VoyageNumber, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(CusInBondHeaderSchema.BH_VoyageNumber, comparisonOperator, value)));
			voyageNumber.Category = FilterCategories.NumbersAndReferences;
			voyageNumber.MaxLength = CusInBondHeaderSchema.BH_VoyageNumber.MaxLength;
			var schd = result.AddTextFilter(FilterConstants.PortOfUnladingSchD, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(CusInBondHeaderSchema.BH_PortUnladingDCode, comparisonOperator, value)));
			schd.Category = FilterCategories.Locations;
			schd.MaxLength = CusInBondHeaderSchema.BH_PortUnladingDCode.MaxLength;
			var unloco = result.AddTextFilter(FilterConstants.PortOfUnladingUNLOCO, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(CusInBondHeaderSchema.BH_RL_NKPortUnlading, comparisonOperator, value)));
			unloco.Category = FilterCategories.Locations;
			unloco.MaxLength = CusInBondHeaderSchema.BH_RL_NKPortUnlading.MaxLength;
			var loadPort = result.AddTextFilter(FilterConstants.LoadPortUNLOCO, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(CusInBondHeaderSchema.BH_RL_NKImportLoadPort, comparisonOperator, value)));
			loadPort.Category = FilterCategories.Locations;
			loadPort.MaxLength = CusInBondHeaderSchema.BH_RL_NKImportLoadPort.MaxLength;
			result.AddTextFilter(FilterConstants.PortOfLadingSchK, CusInBondBillSchema.B0_PortOfLadingKCode).Category = FilterCategories.Locations;
			result.AddTextFilter(FilterConstants.PortOfLadingUNLOCO, CusInBondBillSchema.B0_RL_NKPortOfLading).Category = FilterCategories.Locations;
			result.AddTextFilter(FilterConstants.IssuerCode, CusInBondBillSchema.B0_IssuerCode).Category = FilterCategories.NumbersAndReferences;
			result.AddDateFilter(FilterConstants.ActualArrivalDate, CusInBondBillSchema.B0_A_ARV).Category = FilterCategories.Dates;
			var masterBillNumber = result.AddTextFilter(FilterConstants.MasterBillNumber, new GetTextQueryWithOperator((comparisonOperator, value) => GetMasterBillNumberQuery(comparisonOperator, value)));
			masterBillNumber.Category = FilterCategories.NumbersAndReferences;
			masterBillNumber.MaxLength = CusInBondBillSchema.B0_MasterBillNumber.MaxLength;
			var house = result.AddTextFilter(FilterConstants.HouseBillNumber, new GetTextQueryWithOperator((comparisonOperator, value) => GetHouseBillNumberQuery(comparisonOperator, value)));
			house.Category = FilterCategories.NumbersAndReferences;
			house.MaxLength = CusInBondBillSchema.B0_MasterBillNumber.MaxLength;
			var billStatusFilter = result.AddTextFilter(FilterConstants.BillStatus, CusInBondBillSchema.B0_BillStatus, AMSBillStatusList);
			billStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			billStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			billStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			billStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			billStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			billStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			billStatusFilter.Category = FilterCategories.StatusAndFlags;
			result.AddTextFilter(FilterConstants.FilingStatus, new GetTextQuery((value) =>
				{
					var notIn = value == AMSBillCustomsStatusList.Codes.NotOnFile;
					return GetMoveDetailQuery(CusInBondMoveDetailSchema.B9_CustomsStatus, SQLComparisonOperator.Equal, notIn ? AMSBillCustomsStatusList.Codes.OnFile : value.ToString(), notIn, SubApplicationCodeList.Codes.AMS);
				}), Factory.GetCachedValue<AMSBillCustomsStatusList>()).Category = FilterCategories.StatusAndFlags;
			result.AddTextFilter(FilterConstants.ManifestMessageStatus, new GetTextQuery((value) => GetMoveDetailQuery(CusInBondMoveDetailSchema.B9_MessageStatus, SQLComparisonOperator.Equal, value, false, SubApplicationCodeList.Codes.AMS)), Factory.GetCachedValue<AMSBillMessageStatusList>()).Category = FilterCategories.StatusAndFlags;
			AddDispositionFilter(result, FilterConstants.LatestISFDisposition, ISFDispositionList);
			AddDispositionFilter(result, FilterConstants.LatestPTTDisposition, PTTDispositionList);
			AddLatestAMSDispositionFilter(result);
			AddInBondStatusFilter(result);
			return result;
		}

		static FilterCategory JobCategory
		{
			get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("USAMSBillFilterStrip.HeaderCategory", "Job")); }
		}

		void AddHeaderFilters(ModuleFilterCollection result)
		{
			var jobReference = result.AddTextFilter(FilterConstants.JobReference, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(CusInBondHeaderSchema.BH_JobReference, comparisonOperator, value)));
			jobReference.Category = JobCategory;
			jobReference.MaxLength = CusInBondHeaderSchema.BH_JobReference.MaxLength;

			var jobCreatedBy = result.AddNkFilter(FilterConstants.JobCreatedBy, new GetNkQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(CusInBondHeaderSchema.BH_SystemCreateUser, comparisonOperator, value)), ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			jobCreatedBy.Category = JobCategory;
			jobCreatedBy.MaxLength = CusInBondHeaderSchema.BH_SystemCreateUser.MaxLength;

			result.AddDateFilter(FilterConstants.JobCreatedTime, new GetDateQuery((comparisonOperator, value1, value2) => GetHeaderQuery(CusInBondHeaderSchema.BH_SystemCreateTimeUtc, comparisonOperator, value1, value2))).Category = JobCategory;
		}

		void AddDispositionFilter(ModuleFilterCollection result, ZString description, ICodeDescriptionPairList list)
		{
			var dispositionFilter = result.AddTextFilter(description, new GetTextQueryWithOperator((comparisonOperator, value) => GetDispositionQuery(comparisonOperator, value, list)), list);
			dispositionFilter.Category = FilterCategories.StatusAndFlags;
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
		}

		void AddLatestAMSDispositionFilter(ModuleFilterCollection result)
		{
			var dispositionFilter = result.AddNkFilter(FilterConstants.LatestAMSDisposition, new GetNkQueryWithOperator((comparisonOperator, value) => GetDispositionQuery(comparisonOperator, value, new CodeDescriptionPairList())), ModuleIDs.Customs.Universal.ZZRefCusCodeList, AMSDispositionCollection);
			dispositionFilter.Category = FilterCategories.StatusAndFlags;
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleNkFilter.ComparisonConstants.IsBlank);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleNkFilter.ComparisonConstants.IsNotBlank);
		}

		void AddInBondStatusFilter(ModuleFilterCollection result)
		{
			var inBondStatusFilter = result.AddTextFilter(FilterConstants.InBondStatus, new GetTextQueryWithOperator((comparisonOperator, value) => GetMoveDetailQuery(CusInBondMoveDetailSchema.B9_CustomsStatus, comparisonOperator, value, false, SubApplicationCodeList.Codes.MasterInBond, SubApplicationCodeList.Codes.SubsequentInBond)), InBondStatusList);
			inBondStatusFilter.Category = FilterCategories.StatusAndFlags;
			inBondStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			inBondStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			inBondStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			inBondStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			inBondStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			inBondStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			inBondStatusFilter.MaxLength = CusInBondMoveDetailSchema.B9_CustomsStatus.MaxLength;

			var inBondMessageStatusFilter = result.AddTextFilter(FilterConstants.InBondMessageStatus, new GetTextQueryWithOperator((comparisonOperator, value) => GetMoveDetailQuery(CusInBondMoveDetailSchema.B9_MessageStatus, comparisonOperator, value, false, SubApplicationCodeList.Codes.MasterInBond, SubApplicationCodeList.Codes.SubsequentInBond)), InBondMessageStatusList);
			inBondMessageStatusFilter.Category = FilterCategories.StatusAndFlags;
			inBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			inBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			inBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			inBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			inBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			inBondMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			inBondMessageStatusFilter.MaxLength = CusInBondMoveDetailSchema.B9_MessageStatus.MaxLength;
		}

		ZDBOnlyQuery GetAMSBillDBOnlyQuery()
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondBill));
			var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_TransitDirection, DirectionTypeList.Codes.NVOCC);
			result.AddSubQuery(CusInBondBillSchema.B0_BH, CusInBondHeaderSchema.PK, headerQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetDispositionQuery(SQLComparisonOperator comparisonOperator, ZString value, ICodeDescriptionPairList list)
		{
			var result = GetAMSBillDBOnlyQuery();
			var dataBuilder = new ZStringBuilder();
			for (var i = 0; i < list.Count; i++)
			{
				dataBuilder.Append(((ICodeDescription)list[i]).Code);
			}

			var codesNeedToBeFilter = dataBuilder.ToStringWithDelimiterBetweenAppends(",");
			var notOrBlank = comparisonOperator == SQLComparisonOperator.NotEqual ? "NOT " : "";
			result.AddFilterAndZSQLParameterCollection(
$@"B0_PK {notOrBlank}IN 
(
	SELECT B0_PK FROM CusInBondBill
	CROSS APPLY csfn_GetLatestDispositionCodeInLine(B0_PK, '{codesNeedToBeFilter}') AS LatestDisposition
	WHERE LatestDisposition.DispositionCode = '{value}'
)"
, null);
			return result;
		}

		ZQuery GetMoveDetailQuery(SchemaStringColumn column, SQLComparisonOperator comparisonOperator, ZString value, bool notIn, params ZString[] subApplicationCodes)
		{
			var result = GetAMSBillDBOnlyQuery();
			var moveDetaiQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveDetail), CusInBondMoveDetailSchema.B9_B0, notIn);
			moveDetaiQuery.AddToFilter(column, comparisonOperator, value);
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.PK);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCodes);
			moveDetaiQuery.AddSubQuery(CusInBondMoveDetailSchema.B9_BM, moveHeaderQuery, JoinCondition.And);
			result.AddSubQuery(CusInBondBillSchema.PK, CusInBondMoveDetailSchema.B9_B0, moveDetaiQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetHeaderQuery(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondBill));
			var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_TransitDirection, DirectionTypeList.Codes.NVOCC);
			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasDateEntered:
					headerQuery.AddToFilter(column, value1);
					break;
				case DateComparisonOperator.HasNoDateEntered:
					headerQuery.AddToFilter(column, ZDateTime.Empty);
					break;
				default:
					AddDateTimeRange(headerQuery, comparisonOperator, JoinCondition.And, column, value1, value2);
					break;
			}
			result.AddSubQuery(CusInBondBillSchema.B0_BH, CusInBondHeaderSchema.PK, headerQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetHeaderQuery(SchemaStringColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondBill));
			var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_TransitDirection, DirectionTypeList.Codes.NVOCC);
			headerQuery.AddToFilter(column, comparisonOperator, value);
			result.AddSubQuery(CusInBondBillSchema.B0_BH, CusInBondHeaderSchema.PK, headerQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetMasterBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondBill));
			result.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, comparisonOperator, value);
			result.AddToFilter(CusInBondBillSchema.B0_ShipmentType, CusInBondBill.OceanBillType);

			var dbQuery = new ZDBOnlyQuery(typeof(CusInBondBill));
			dbQuery.AddToFilter(CusInBondBillSchema.B0_ShipmentType, ZString.Empty);
			var subQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
			subQuery.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, comparisonOperator, value);
			subQuery.AddToFilter(CusInBondBillSchema.B0_ShipmentType, CusInBondBill.OceanBillType);
			dbQuery.AddSubQuery(CusInBondBillSchema.B0_BH, subQuery, JoinCondition.And);
			result.AddToFilter(dbQuery, JoinCondition.Or);

			return result;
		}

		ZQuery GetHouseBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondBill));
			result.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, comparisonOperator, value);
			result.AddToFilter(CusInBondBillSchema.B0_ShipmentType, ZString.Empty);
			return result;
		}

		CodeDescriptionPairList AMSBillStatusList
		{
			get { return BillOfLadingStatusIndicatorList.GetCachedValue(Factory, false, false, US.Business.ZZCustomsFunctionality.IsAMSHBREffective); }
		}

		CodeDescriptionPairList InBondStatusList
		{
			get { return AMSBillMessageStatusList.GetInBondStatusList(Factory); }
		}

		CodeDescriptionPairList InBondMessageStatusList
		{
			get { return AMSBillMessageStatusList.GetInBondMessageStatusList(Factory); }
		}

		ICodeDescriptionPairList ISFDispositionList
		{
			get { return DispositionCodeListLoader.GetCachedISFCodesForAMS(Factory); }
		}

		ICodeDescriptionPairList PTTDispositionList
		{
			get { return DispositionCodeListLoader.GetCachedPTTCodesForAMS(Factory); }
		}

		IBusinessObjectCollection AMSDispositionCollection
		{
			get { return DispositionCodeListLoader.GetAMSDispositionCollection(Factory); }
		}
	}
}
