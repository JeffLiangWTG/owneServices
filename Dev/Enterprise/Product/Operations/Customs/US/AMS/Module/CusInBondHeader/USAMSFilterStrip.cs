using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
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
	public class USAMSFilterStrip : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string TransportMode = "Transport Mode";
			public const string JobReference = "Job Reference";
			public const string CarrierSCAC = "Carrier SCAC";
			public const string ArrivalPortSchD = "Arrival Port (Sch.D)";
			public const string OriginalEsitmatedTime = "Estimated Time";
			public const string ImportingConveyanceName = "Conveyance Name";
			public const string VoyageNumber = "Voyage Number";
			public const string LloydsNumber = "Lloyds Number";
			public const string ConveyanceMessageStatus = "Conveyance Message Status";
			public const string ActualArrivalDate = "Actual Arrival Date";
			public const string LoadPortUNLOCO = "Load Port (UNLOCO)";
			public const string EstimatedDateofDeparture = "Estimated Date of Departure";
			public const string LatestAMSDisposition = "Latest AMS Disposition";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddTextFilter(FilterConstants.TransportMode, CusInBondHeaderSchema.BH_ImportTransportMode, TransportTypeList.GetConveyanceTransportTypeList(Factory)).Category = FilterCategories.ModesAndTypes;
			result.AddTextFilter(FilterConstants.CarrierSCAC, CusInBondHeaderSchema.BH_CarrierSCAC).Category = FilterCategories.NumbersAndReferences;
			result.AddTextFilter(FilterConstants.JobReference, CusInBondHeaderSchema.BH_JobReference).Category = FilterCategories.NumbersAndReferences;
			result.AddTextFilter(FilterConstants.ArrivalPortSchD, CusInBondHeaderSchema.BH_PortUnladingDCode).Category = FilterCategories.Locations;
			result.AddDateFilter(FilterConstants.OriginalEsitmatedTime, CusInBondHeaderSchema.BH_ETA).Category = FilterCategories.Dates;
			result.AddDateFilter(FilterConstants.EstimatedDateofDeparture, CusInBondHeaderSchema.BH_FirstExportDate).Category = FilterCategories.Dates;
			var messageStatus = result.AddTextFilter(FilterConstants.ConveyanceMessageStatus, new GetTextQueryWithOperator((comparisonOperator, value) => GetMoveHeaderQuery(CusInBondMoveHeaderSchema.BM_CustomsStatus, comparisonOperator, value, SubApplicationCodeList.Codes.AMS)), VesselMessageStatusList);
			messageStatus.Category = FilterCategories.NumbersAndReferences;
			messageStatus.MaxLength = CusInBondMoveHeaderSchema.BM_CustomsStatus.MaxLength;

			// TODO: remove comparison option
			result.AddTextFilter(FilterConstants.ImportingConveyanceName, CusInBondHeaderSchema.BH_ImportConveyanceName).Category = FilterCategories.NumbersAndReferences;
			result.AddTextFilter(FilterConstants.VoyageNumber, CusInBondHeaderSchema.BH_VoyageNumber).Category = FilterCategories.NumbersAndReferences;
			result.AddTextFilter(FilterConstants.LloydsNumber, CusInBondHeaderSchema.BH_LloydsNumber).Category = FilterCategories.NumbersAndReferences;
			result.AddDateFilter(FilterConstants.ActualArrivalDate, GetActualArrivalDateQuery).Category = FilterCategories.Dates;
			result.AddTextFilter(FilterConstants.LoadPortUNLOCO, CusInBondHeaderSchema.BH_RL_NKImportLoadPort).Category = FilterCategories.Locations;

			AddLatestAMSDispositionFilter(result);

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowHelper = new WorkflowFilterStripsHelperUSAMS(typeof(CusInBondHeader), WorkflowDescriptors.USAMSWorkflowDescriptorCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(workflowHelper);
			return helpers;
		}

		ZQuery GetMoveHeaderQuery(SchemaStringColumn column, SQLComparisonOperator comparisonOperator, ZString value, ZString subApplicationCode)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCode);
			moveHeaderQuery.AddToFilter(column, comparisonOperator, value);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetActualArrivalDateQuery(DateComparisonOperator sqlOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var billSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);

			if (!dateFrom.IsEmpty || !dateTo.IsEmpty)
			{
				AddDateRange(billSubQuery, sqlOperator, JoinCondition.And, CusInBondBillSchema.B0_A_ARV, dateFrom.Date, dateTo.Date);
			}
			result.AddSubQuery(billSubQuery, JoinCondition.And);

			return result;
		}

		void AddLatestAMSDispositionFilter(ModuleFilterCollection result)
		{
			var dispositionFilter = result.AddNkFilter(FilterConstants.LatestAMSDisposition, new GetNkQueryWithOperator((comparisonOperator, value) => GetDispositionQuery(comparisonOperator, value)), ModuleIDs.Customs.Universal.ZZRefCusCodeList, AMSDispositionCollection);
			dispositionFilter.Category = FilterCategories.StatusAndFlags;
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleNkFilter.ComparisonConstants.IsBlank);
			dispositionFilter.ComparisonOperator_List.RemoveCode(ModuleNkFilter.ComparisonConstants.IsNotBlank);
		}

		ZQuery GetDispositionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var billSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH, comparisonOperator == SQLComparisonOperator.NotEqual);
			billSubQuery.AddToFilter(CusInBondBillSchema.B0_ShipmentType, SQLComparisonOperator.NotEqual, CusInBondBill.OceanBillType);
			billSubQuery.AddFilterAndZSQLParameterCollection(
$@"B0_PK IN 
(
	SELECT B0_PK FROM CusInBondBill
	CROSS APPLY csfn_GetLatestDispositionCodeInLine(B0_PK, '') AS LatestDisposition
	WHERE LatestDisposition.DispositionCode = '{value}'
)"
, null);
			result.AddSubQuery(billSubQuery, JoinCondition.And);
			return result;
		}

		CodeDescriptionPairList VesselMessageStatusList
		{
			get { return AMSBillMessageStatusList.GetVesselMessageStatusList(Factory); }
		}

		IBusinessObjectCollection AMSDispositionCollection
		{
			get { return DispositionCodeListLoader.GetAMSDispositionCollection(Factory); }
		}
	}
}
