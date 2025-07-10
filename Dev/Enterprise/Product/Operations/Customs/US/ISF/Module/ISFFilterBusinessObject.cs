using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Module
{
	public class ISFFilterBusinessObject : FilterStripBusinessObject
	{
		#region Constants

		public static class Constants
		{
			internal const string JobNumber = "Job Number";
			internal const string OwnerReference = "Owner Reference";
			internal const string CustomAttribute1 = "Custom Attribute 1";
			internal const string CustomAttribute2 = "Custom Attribute 2";
			internal const string CustomsReference = "Customs Reference";
			internal const string DiscardedCustomsReference = "Discarded Customs Reference";
			internal const string Status = "Status";
			internal const string EntryType = "Entry Type";
			internal const string ShipmentType = "Shipment Type";
			internal const string ShipmentNumber = "Shipment Number";
			internal const string SCAC = "SCAC";
			internal const string ImporterCodeType = "Importer Code Type";
			internal const string ImporterCode = "Importer Code";
			internal const string ConsigneeCodeType = "Consignee Code Type";
			internal const string ConsigneeCode = "Consignee Code";
			internal const string ImporterDOB = "Importer DOB";
			internal const string UnloadPort = "Unload Port";
			internal const string DeliveryPort = "Delivery Port";
			internal const string CountryOfIssue = "Country of Issue";
			internal const string BondHolder = "Bond Holder";
			internal const string BondSuretyCode = "Bond Surety Code";
			internal const string ISFBondNumber = "ISF Bond Number (Before CSMS #09-000148 changes)";
			internal const string MasterBill = "Master Bill";
			internal const string HouseBill = "House Bill";
			internal const string OceanBill = "Ocean Bill";
			internal const string BillStatus = "Bill Status";
			internal const string CBPEntryNumber = "CBP Entry Number";
			internal const string NoOfHTSDigits = "No. Of HTS Digits";
			internal const string SendEquipment = "Send Equipment";
			internal const string MergeStyle = "Merge Style";
			internal const string Importer = "Importer";
			internal const string ManufacturerShipToParty = "Manufacturer / Ship To Party";
			internal const string SellingBuyingParty = "Selling Party / Buying Party";
			internal const string ConsolidatorStuffingLocation = "Consolidator / Stuffing Location";
			internal const string BookingParty = "Booking Party";
			internal const string SendingAgent = "Sending Agent";
			internal const string ActionReasonCode = "Action Reason Code";
			internal const string Branch = "Branch";
			internal const string Vessel = "Vessel";
			internal const string VoyageFlight = "Voyage/Flight";
			internal const string LoadDischarge = "Load / Discharge";
			internal const string ETD = "ETD";
			internal const string ETA = "ETA";
			internal const string ATD = "ATD";
			internal const string ATA = "ATA";
			internal const string FirstAccepted = "First Accepted";
			internal const string LastAccepted = "Last Accepted";
			internal const string MatchedDate = "Matched Date";
			internal const string FirstMatchedDate = "First Matched Date";
			internal const string BondReference = "Bond Reference";
			internal const string CarnetReference = "Carnet Reference";
			internal const string ContainerNumber = "Container Number";
			internal const string DISStatus = "DIS Status";
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddFountainFilter(Constants.JobNumber, CusISFHeaderSchema.BF_JobReference, "ISF", 7).Category = FilterCategories.NumbersAndReferences;
			filters.AddTextFilter(Constants.OwnerReference, CusISFHeaderSchema.BF_OwnerReference).Category = FilterCategories.NumbersAndReferences;
			var att1 = filters.AddNumberFilter(Constants.CustomAttribute1, GetCustomAttribute1);
			att1.MaxLength = GenCustomAddOnValueSchema.XV_Data.MaxLength;
			att1.UseMultiSearch = false;
			var att2 = filters.AddNumberFilter(Constants.CustomAttribute2, GetCustomAttribute2);
			att2.MaxLength = GenCustomAddOnValueSchema.XV_Data.MaxLength;
			att2.UseMultiSearch = false;
			filters.AddTextFilter(Constants.CustomsReference, CusISFHeaderSchema.BF_CustomsReference).Category = FilterCategories.NumbersAndReferences;
			var discardedCustomsReferencefilter = filters.AddTextFilter(Constants.DiscardedCustomsReference, GetDiscardedCustomsReferenceQuery);
			discardedCustomsReferencefilter.Category = FilterCategories.NumbersAndReferences;
			discardedCustomsReferencefilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			discardedCustomsReferencefilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			discardedCustomsReferencefilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			discardedCustomsReferencefilter.MaxLength = EDIMessageSchema.EM_ApplicationReference.MaxLength;
			filters.AddTextFilter(Constants.Status, CusISFHeaderSchema.BF_CustomsStatus, Lookups.MessageStatusList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(Constants.EntryType, CusISFHeaderSchema.BF_EntryType, Lookups.EntryTypes).Category = FilterCategories.ModesAndTypes;
			filters.AddTextFilter(Constants.ShipmentType, CusISFHeaderSchema.BF_ShipmentType, Lookups.ShipmentTypes).Category = FilterCategories.ModesAndTypes;
			filters.AddTextFilter(Constants.SCAC, CusISFHeaderSchema.BF_SCAC).Category = FilterCategories.NumbersAndReferences;
			filters.AddTextFilter(Constants.ImporterCodeType, CusISFHeaderSchema.BF_ImporterCodeType, Lookups.ImporterCodeTypes).Category = FilterCategories.ModesAndTypes;
			filters.AddTextFilter(Constants.ImporterCode, CusISFHeaderSchema.BF_ImporterCode).Category = FilterCategories.Organisations;
			filters.AddTextFilter(Constants.ConsigneeCodeType, CusISFHeaderSchema.BF_ConsigneeCodeType, Lookups.ConsigneeCodeTypes).Category = FilterCategories.ModesAndTypes;
			filters.AddTextFilter(Constants.ConsigneeCode, CusISFHeaderSchema.BF_ConsigneeCode).Category = FilterCategories.Organisations;

			filters.AddDateFilter(Constants.ImporterDOB, CusISFHeaderSchema.BF_DateOfBirth).Category = FilterCategories.Dates;

			filters.AddNkFilter(Constants.UnloadPort, CusISFHeaderSchema.BF_RL_NKPortOfUnload, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory)).Category = FilterCategories.Locations;
			filters.AddNkFilter(Constants.DeliveryPort, CusISFHeaderSchema.BF_RL_NKPlaceOfDelivery, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory)).Category = FilterCategories.Locations;
			filters.AddNkFilter(Constants.CountryOfIssue, CusISFHeaderSchema.BF_CountryOfIssue, ModuleIDs.RefCountry, new RefCountryCollection(Factory)).Category = FilterCategories.Locations;

			filters.AddTextFilter(Constants.BondHolder, CusISFHeaderSchema.BF_BondNumberOrHolder).Category = FilterCategories.NumbersAndReferences;
			var bondSuretyCode = filters.AddTextFilter(Constants.BondSuretyCode, GetBondSuretyCodeQuery);
			bondSuretyCode.Category = FilterCategories.NumbersAndReferences;
			bondSuretyCode.MaxLength = CusISFBillSchema.BB_BillNum.MaxLength;
			var bondNumber = filters.AddTextFilter(Constants.ISFBondNumber, GetISFBondNumberQuery);
			bondNumber.Category = FilterCategories.NumbersAndReferences;
			bondNumber.MaxLength = CusISFBillSchema.BB_BillNum.MaxLength;
			var masterBill = filters.AddTextFilter(Constants.MasterBill, GetMasterBillQuery);
			masterBill.Category = FilterCategories.NumbersAndReferences;
			masterBill.MaxLength = CusISFBillSchema.BB_BillNum.MaxLength;
			var houseBill = filters.AddTextFilter(Constants.HouseBill, GetHouseBillQuery);
			houseBill.Category = FilterCategories.NumbersAndReferences;
			houseBill.MaxLength = CusISFBillSchema.BB_BillNum.MaxLength;
			var oceanBill = filters.AddTextFilter(Constants.OceanBill, GetOceanBillQuery);
			oceanBill.Category = FilterCategories.NumbersAndReferences;
			oceanBill.MaxLength = CusISFBillSchema.BB_BillNum.MaxLength;

			var isfBillStatusFilter = filters.AddTextFilter(Constants.BillStatus, GetBillStatusQuery, DispositionCodeList);
			isfBillStatusFilter.Category = FilterCategories.StatusAndFlags;
			isfBillStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			isfBillStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var disStatusFilter = filters.AddTextFilter(Constants.DISStatus, GetDISStatusQuery, DISStatusList);
			disStatusFilter.Category = FilterCategories.StatusAndFlags;
			disStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			disStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			disStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			disStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			disStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			disStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			disStatusFilter.MaxLength = JobRequiredDocumentAddInfoSchema.EX_Status.MaxLength;

			var cbp = filters.AddTextFilter(Constants.CBPEntryNumber, GetCBPEntryNumberQuery);
			cbp.Category = FilterCategories.NumbersAndReferences;
			cbp.MaxLength = CusISFBillSchema.BB_BillNum.MaxLength;
			filters.AddTextFilter(Constants.NoOfHTSDigits, CusISFHeaderSchema.BF_NumOfHarmChars, Lookups.NumberOfHarmonizedDigitsToReportList).Category = FilterCategories.ModesAndTypes;
			filters.AddTextFilter(Constants.SendEquipment, CusISFHeaderSchema.BF_SendEquipment, Lookups.YesNoDefaultList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(Constants.MergeStyle, CusISFHeaderSchema.BF_LineMergeStyle, Lookups.MergeStyleList).Category = FilterCategories.ModesAndTypes;

			filters.AddGuidFilter(Constants.Importer, ModuleIDs.Organisation, CusISFHeaderSchema.BF_OH_Importer, Lookups.Importers).Category = FilterCategories.Organisations;
			filters.AddGuidFilter(Constants.ManufacturerShipToParty, ModuleIDs.Organisation, GetManufacturerShipToPartyQuery, new OrgHeaderCollection(Factory), new ConsigneeCollection(Factory)).Category = FilterCategories.Organisations;
			filters.AddGuidFilter(Constants.SellingBuyingParty, ModuleIDs.Organisation, GetSellingPartyBuyingPartyQuery, new ConsignorCollection(Factory), new ConsigneeCollection(Factory)).Category = FilterCategories.Organisations;
			filters.AddGuidFilter(Constants.ConsolidatorStuffingLocation, ModuleIDs.Organisation, GetConsolidatorStuffingLocationQuery, new OrgHeaderCollection(Factory), new OrgHeaderCollection(Factory)).Category = FilterCategories.Organisations;
			filters.AddGuidFilter(Constants.BookingParty, ModuleIDs.Organisation, GetBookingPartyQuery, new OrgHeaderCollection(Factory)).Category = FilterCategories.Organisations;
			filters.AddGuidFilter(Constants.SendingAgent, ModuleIDs.Organisation, GetSendingAgentQuery, new OrgHeaderCollection(Factory)).Category = FilterCategories.Organisations;
			filters.AddTextFilter(Constants.ActionReasonCode, CusISFHeaderSchema.BF_ActionReasonCode, Lookups.ActionReasonCodeList).Category = FilterCategories.NumbersAndReferences;

			ModuleFilter branchFilter = filters.AddGuidFilter(Constants.Branch, ModuleIDs.GlbBranch, CusISFHeaderSchema.BF_GB, Lookups.Branches);
			branchFilter.IsPublishedOnWeb = false;

			var shipmentNumber = filters.AddTextFilter(Constants.ShipmentNumber, GetShipmentNumber);
			shipmentNumber.Category = FilterCategories.NumbersAndReferences;
			shipmentNumber.MaxLength = JobShipmentSchema.JS_UniqueConsignRef.MaxLength;

			var vessel = filters.AddTextFilter(Constants.Vessel, GetTransportVessel);
			vessel.Category = FilterCategories.NumbersAndReferences;
			vessel.MaxLength = JobConsolTransportSchema.JW_Vessel.MaxLength;

			var voyageFlight = filters.AddTextFilter(Constants.VoyageFlight, GetTransportVoyageFlight);
			voyageFlight.Category = FilterCategories.NumbersAndReferences;
			voyageFlight.MaxLength = JobConsolTransportSchema.JW_VoyageFlight.MaxLength;
			ModuleLocationFilter locationFilter = filters.AddLocationFilter(Constants.LoadDischarge, GetTransportLoadDiscPort, Location_List, Location_List);
			locationFilter.SetItemDescriptions(Res.GetData("ISFFilterBusinessObject|Load", "Load"), Res.GetData("ISFFilterBusinessObject", "Discharge"));
			locationFilter.Category = FilterCategories.Locations;
			filters.AddDateFilter(Constants.ETD, GetTransportETD).Category = FilterCategories.Dates;
			filters.AddDateFilter(Constants.ETA, GetTransportETA).Category = FilterCategories.Dates;
			filters.AddDateFilter(Constants.ATD, GetTransportATD).Category = FilterCategories.Dates;
			filters.AddDateFilter(Constants.ATA, GetTransportATA).Category = FilterCategories.Dates;

			filters.AddDateFilter(Constants.FirstAccepted, CusISFHeaderSchema.BF_FirstAcceptedDate).Category = FilterCategories.Dates;
			filters.AddDateFilter(Constants.LastAccepted, CusISFHeaderSchema.BF_LastAcceptedDate).Category = FilterCategories.Dates;
			filters.AddDateFilter(Constants.MatchedDate, GetBillMatchedDate).Category = FilterCategories.Dates;
			filters.AddDateFilter(Constants.FirstMatchedDate, GetFirstBillMatchedDate).Category = FilterCategories.Dates;

			var bondRef = filters.AddTextFilter(Constants.BondReference, GetBondReferenceQuery);
			bondRef.Category = FilterCategories.NumbersAndReferences;
			bondRef.MaxLength = CusISFBillSchema.BB_BillNum.MaxLength;
			var carnetRef = filters.AddTextFilter(Constants.CarnetReference, GetCarnetReferenceQuery);
			carnetRef.Category = FilterCategories.NumbersAndReferences;
			carnetRef.MaxLength = CusISFBillSchema.BB_BillNum.MaxLength;

			var container = filters.AddTextFilter(Constants.ContainerNumber, GetContainerNumberQuery);
			container.Category = FilterCategories.NumbersAndReferences;
			container.MaxLength = CusISFEquipSchema.BE_ContainerNum.MaxLength;

			return filters;
		}

		#region workflow Custom Fields filter

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowHelper = new MasterFiles.Module.WorkflowFilterStripsHelper(typeof(CusISFHeader), WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(workflowHelper);
			return helpers;
		}

		#endregion

		#region Implementation

		ZQuery GetDiscardedCustomsReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID);
			subQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Discarded);
			subQuery.AddToFilter(EDIMessageSchema.EM_ApplicationReference, comparisonOperator, value);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetContainerNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusISFEquip), CusISFEquipSchema.BE_BF);
			subQuery.AddToFilter(CusISFEquipSchema.BE_ContainerNum, comparisonOperator, value);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetBondReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceDataQuery(comparisonOperator, BillTypeList.Codes.BondReferenceNumber, value);
		}

		ZQuery GetCarnetReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceDataQuery(comparisonOperator, BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber, value);
		}

		ZQuery GetCBPEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceDataQuery(comparisonOperator, BillTypeList.Codes.USCBPEntryNumber, value);
		}

		#region Bill Status

		//SELECT BB_BF
		//FROM (
		//    SELECT CASE WHEN COUNT(BB_BF) > 1 THEN 'Multiple' ELSE MAX (BB_CustomsStatus) END AS BB_CustomsStatus, BB_BF
		//    FROM (
		//        SELECT DISTINCT BB_BF, BB_CustomsStatus
		//        FROM dbo.CusISFBill
		//       WHERE BB_BillType IN ('OB', 'BM')
		//    ) AS ISFBillStatus
		//    GROUP BY BB_BF
		//) AS ISFBillStatus
		//WHERE BB_CustomsStatus = 'Multiple'

		//SELECT BB_BF
		//FROM dbo.CusISFBill
		//WHERE BB_BillType IN ('OB', 'BM') AND BB_CustomsStatus = ''

		//SELECT BB_BF
		//FROM dbo.CusISFBill
		//WHERE BB_BillType IN ('OB', 'BM') AND BB_CustomsStatus <> ''

		ZQuery GetBillStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			ZQuery result = null;
			var isMultiple = value == Common.US.ISF.ISFStatusHelper.Multiple;
			var isBlackTypeFilter = filterOperator == SpecialComparisonOperator.IsNotBlank || filterOperator == SpecialComparisonOperator.IsBlank;
			var isNotMatching = filterOperator == SpecialComparisonOperator.IsBlank || filterOperator == SQLComparisonOperator.NotEqual;

			if (isMultiple && !isBlackTypeFilter)
			{
				//BF_PK NOT IN (
				//    SELECT BB_BF
				//    FROM (
				//        SELECT CASE WHEN COUNT(BB_BF) > 1 THEN 'Multiple' ELSE MAX (BB_CustomsStatus) END AS BB_CustomsStatus, BB_BF
				//        FROM (
				//            SELECT DISTINCT BB_BF, BB_CustomsStatus
				//            FROM dbo.CusISFBill 
				//           WHERE BB_BillType IN ('OB', 'BM') AND BB_CustomsStatus <> ''
				//        ) AS ISFBillStatus
				//        GROUP BY BB_BF
				//    ) AS ISFBillStatus
				//   WHERE BB_CustomsStatus = 'Multiple'
				//)

				var sqlFilter = string.Format(@"
					{0} {1} IN (
						SELECT {2}
						FROM (
							SELECT CASE WHEN COUNT({2}) > 1 THEN @MultipleValue ELSE MAX ({3}) END AS {3}, {2}
							FROM (
								SELECT DISTINCT {2}, {3}
								FROM {4} 
								WHERE {5} IN (@OceanBillType, @HouseBillType) AND {3} <> ''
							) AS ISFBillStatus
							GROUP BY {2}
						) AS ISFBillStatus
						WHERE {3} = @MultipleValue
					)"
					, CusISFHeaderSchema.Constants.PK // {0}
					, isNotMatching ? "NOT" : "" // {1}
					, CusISFBillSchema.Constants.BB_BF // {2}
					, CusISFBillSchema.Constants.BB_CustomsStatus // {3}
					, CusISFBillSchema.Constants.TableName // {4}
					, CusISFBillSchema.Constants.BB_BillType // {5}
				);

				var sqlFilterParameters = new ZSqlParameterCollection(
					ZSqlParameter.New("@MultipleValue", ISFStatusHelper.Multiple, CusISFBillSchema.BB_CustomsStatus),
					ZSqlParameter.New("@OceanBillType", BillTypeList.Codes.OceanBillOfLading, CusISFBillSchema.BB_BillType),
					ZSqlParameter.New("@HouseBillType", BillTypeList.Codes.HouseBillOfLading, CusISFBillSchema.BB_BillType)
				);

				result = new ZDBOnlyQuery(typeof(CusISFHeader));
				result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			}
			else
			{
				//BF_PK NOT IN (
				//    SELECT BB_BF
				//    FROM dbo.CusISFBill 
				//   WHERE BB_BillType IN ('OB', 'BM') AND BB_CustomsStatus = ''
				//)

				var sqlFilter = string.Format(@"
					{0} {1} IN (
						SELECT {2}
						FROM {3} 
						WHERE {4} IN (@OceanBillType, @HouseBillType) AND {5} {6} @CustomsStatus
					)"
					, CusISFHeaderSchema.Constants.PK // {0}
					, isNotMatching ? "NOT" : "" // {1}
					, CusISFBillSchema.Constants.BB_BF // {2}
					, CusISFBillSchema.Constants.TableName // {3}
					, CusISFBillSchema.Constants.BB_BillType // {4}
					, CusISFBillSchema.Constants.BB_CustomsStatus // {5}
					, isBlackTypeFilter ? "<>" : "=" // {6}
				);

				var sqlFilterParameters = new ZSqlParameterCollection(
					ZSqlParameter.New("@OceanBillType", BillTypeList.Codes.OceanBillOfLading, CusISFBillSchema.BB_BillType),
					ZSqlParameter.New("@HouseBillType", BillTypeList.Codes.HouseBillOfLading, CusISFBillSchema.BB_BillType),
					ZSqlParameter.New("@CustomsStatus", isBlackTypeFilter ? ZString.Empty : value, CusISFBillSchema.BB_CustomsStatus)
				);

				result = new ZDBOnlyQuery(typeof(CusISFHeader));
				result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			}
			return result;
		}

		#endregion

		ZQuery GetDISStatusQuery(ZString value)
		{
			var query = new ZQuery();

			if (!value.IsEmpty)
			{
				var result = new ZDBOnlyQuery(typeof(CusISFHeader));

				var rquiredDocumentAddInfoQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocumentAddInfo), JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument);
				rquiredDocumentAddInfoQuery.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_ApplicationCode, Core.Constants.Customs.DocumentImageSystemIDs.US_DIS);
				rquiredDocumentAddInfoQuery.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_Status, value);

				var rquiredDocumentQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentSchema.EQ_ParentID);
				rquiredDocumentQuery.AddSubQuery(rquiredDocumentAddInfoQuery, JoinCondition.And);

				result.AddSubQuery(rquiredDocumentQuery, JoinCondition.And);

				query = result;
			}

			return query;
		}

		ZQuery GetOceanBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceDataQuery(comparisonOperator, BillTypeList.Codes.OceanBillOfLading, value);
		}

		ZQuery GetHouseBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceDataQuery(comparisonOperator, BillTypeList.Codes.HouseBillOfLading, value);
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceDataQuery(comparisonOperator, BillTypeList.Codes.MasterBillOfLading, value);
		}

		ZQuery GetISFBondNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceDataQuery(comparisonOperator, BillTypeList.Codes.ISFBondNumber, value);
		}

		ZQuery GetBondSuretyCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetReferenceDataQuery(comparisonOperator, BillTypeList.Codes.SuretyCode, value);
		}

		ZQuery GetReferenceDataQuery(SQLComparisonOperator comparisonOperator, ZString billType, ZString billNumber)
		{
			var result = new ZDBOnlyQuery(typeof(CusISFHeader));

			var isBlankOperator = comparisonOperator == SpecialComparisonOperator.IsBlank;
			var referenceDataSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF, isBlankOperator);
			referenceDataSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, billType);

			if (!isBlankOperator)
			{
				referenceDataSubQuery.AddToFilter(CusISFBillSchema.BB_BillNum, comparisonOperator, billNumber);
			}
			result.AddSubQuery(referenceDataSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetBillMatchedDate(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));
			ZDBOnlySubQuery referenceDataSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF);
			referenceDataSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, new ZString[] { BillTypeList.Codes.HouseBillOfLading, BillTypeList.Codes.OceanBillOfLading });
			AddDateTimeRange(referenceDataSubQuery, comparisonOperator, JoinCondition.And, CusISFBillSchema.BB_MatchDate, dateFrom, dateTo);
			result.AddSubQuery(referenceDataSubQuery, JoinCondition.And);

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				ZDBOnlySubQuery noreferenceDataSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF, true);
				noreferenceDataSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, new ZString[] { BillTypeList.Codes.HouseBillOfLading, BillTypeList.Codes.OceanBillOfLading });
				result.AddSubQuery(noreferenceDataSubQuery, JoinCondition.Or);
			}

			return result;
		}

		ZQuery GetFirstBillMatchedDate(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));
			ZDBOnlySubQuery referenceDataSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF);
			referenceDataSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, new ZString[] { BillTypeList.Codes.HouseBillOfLading, BillTypeList.Codes.OceanBillOfLading });
			AddDateTimeRange(referenceDataSubQuery, comparisonOperator, JoinCondition.And, CusISFBillSchema.BB_FirstMatchedDate, dateFrom, dateTo);
			result.AddSubQuery(referenceDataSubQuery, JoinCondition.And);

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				ZDBOnlySubQuery noreferenceDataSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF, true);
				noreferenceDataSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, new ZString[] { BillTypeList.Codes.HouseBillOfLading, BillTypeList.Codes.OceanBillOfLading });
				result.AddSubQuery(noreferenceDataSubQuery, JoinCondition.Or);
			}

			return result;
		}

		ZQuery GetBookingPartyQuery(ZGuid value)
		{
			return GetOrganisationQuery(DocAddressType.BookingPartyDocumentaryAddress, value);
		}

		ZQuery GetConsolidatorStuffingLocationQuery(ZGuid organisation1PK, ZGuid organisation2PK)
		{
			return GetOrganisationsQuery(organisation1PK, DocAddressType.Consolidator, organisation2PK, DocAddressType.ScheduledContainerStuffingLocation);
		}

		ZQuery GetSellingPartyBuyingPartyQuery(ZGuid organisation1PK, ZGuid organisation2PK)
		{
			return GetOrganisationsQuery(organisation1PK, DocAddressType.SellingParty, organisation2PK, DocAddressType.BuyingParty);
		}

		ZQuery GetManufacturerShipToPartyQuery(ZGuid organisation1PK, ZGuid organisation2PK)
		{
			return GetOrganisationsQuery(organisation1PK, DocAddressType.Manufacturer, organisation2PK, DocAddressType.ShipToParty);
		}

		ZQuery GetOrganisationsQuery(ZGuid organisation1PK, DocAddressType organisation1AddressType, ZGuid organisation2PK, DocAddressType organisation2AddressType)
		{
			ZQuery result = new ZQuery();

			if (organisation1PK.IsValid)
			{
				result.AddToFilter(GetOrganisationQuery(organisation1AddressType, organisation1PK));
			}
			if (organisation2PK.IsValid)
			{
				result.AddToFilter(GetOrganisationQuery(organisation2AddressType, organisation2PK));
			}

			return result;
		}

		#region SendingAgentQuery

		public ZDBOnlyQuery GetSendingAgentQuery(ZGuid orgPK)
		{
			ZDBOnlySubQuery oSBLTM = new ZDBOnlySubQuery(typeof(OrgSupBuyLinkTrnMode), OrgSupBuyLinkTrnModeSchema.PF_OL);
			oSBLTM.AddToFilter(OrgSupBuyLinkTrnModeSchema.PF_OH_SendingAgent, orgPK);

			ZDBOnlySubQuery buyOSBL = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), OrgSupplierBuyerLinkSchema.OL_OH_Buyer);
			buyOSBL.AddSubQuery(oSBLTM, JoinCondition.And);
			ZDBOnlySubQuery supOSBL = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), OrgSupplierBuyerLinkSchema.OL_OH_Supplier);
			supOSBL.AddSubQuery(oSBLTM, JoinCondition.And);

			ZDBOnlySubQuery buyOA = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			buyOA.AddSubQuery(OrgAddressSchema.OA_OH, buyOSBL, JoinCondition.And);
			ZDBOnlySubQuery supOA = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			supOA.AddSubQuery(OrgAddressSchema.OA_OH, supOSBL, JoinCondition.And);

			ZDBOnlySubQuery buyJDA = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			buyJDA.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
			buyJDA.AddSubQuery(JobDocAddressSchema.E2_OA_Address, buyOA, JoinCondition.And);
			AddBuyers(buyJDA);
			ZDBOnlySubQuery supJDA = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			supJDA.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
			supJDA.AddSubQuery(JobDocAddressSchema.E2_OA_Address, supOA, JoinCondition.And);
			AddSuppliers(supJDA);

			ZDBOnlySubQuery allBuyers = new ZDBOnlySubQuery(typeof(CusISFHeader), CusISFHeaderSchema.PK);
			allBuyers.AddSubQuery(buyJDA, JoinCondition.Or);
			allBuyers.AddSubQuery(CusISFHeaderSchema.BF_OH_Importer, buyOSBL, JoinCondition.Or);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));
			result.AddSubQuery(allBuyers, JoinCondition.And);
			result.AddSubQuery(supJDA, JoinCondition.And);

			return result;
		}

		public void AddBuyers(ZDBOnlySubQuery jobDocAddressSubQuery)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.Or, JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.BuyingParty));
			filter.AddToFilter(JoinCondition.Or, JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.ShipToParty));
			jobDocAddressSubQuery.AddToFilter(filter);
		}

		public void AddSuppliers(ZDBOnlySubQuery jobDocAddressSubQuery)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.Or, JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.SellingParty));
			filter.AddToFilter(JoinCondition.Or, JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.Manufacturer));
			jobDocAddressSubQuery.AddToFilter(filter);
		}

		#endregion

		public ZDBOnlyQuery GetOrganisationQuery(DocAddressType addressType, ZGuid orgPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));
			ZDBOnlySubQuery docAddressSubQuery = GetDocAddressSubQuery(addressType, orgPK);
			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);
			return result;
		}

		ZDBOnlySubQuery GetDocAddressSubQuery(DocAddressType addressType, ZGuid orgPK)
		{
			ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);

			orgHeaderSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return docAddressSubQuery;
		}

		ZQuery GetTransportETD(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetTransport(JobConsolTransportSchema.JW_ETD, JobVoyOriginSchema.JA_E_DEP, comparisonOperator, dateFrom, dateTo, true);
		}

		ZQuery GetTransportETA(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetTransport(JobConsolTransportSchema.JW_ETA, JobVoyDestinationSchema.JB_E_ARV, comparisonOperator, dateFrom, dateTo, false);
		}

		ZQuery GetTransportATD(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetTransport(JobConsolTransportSchema.JW_ATD, JobVoyOriginSchema.JA_A_DEP, comparisonOperator, dateFrom, dateTo, true);
		}

		ZQuery GetTransportATA(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return GetTransport(JobConsolTransportSchema.JW_ATA, JobVoyDestinationSchema.JB_A_ARV, comparisonOperator, dateFrom, dateTo, false);
		}

		ZQuery GetTransportLoadDiscPort(ZString loadPort, ZString discPort)
		{
			ZQuery query = new ZQuery();

			if (!loadPort.IsEmpty || !discPort.IsEmpty)
			{
				ZDBOnlyQuery headerQuery = new ZDBOnlyQuery(typeof(CusISFHeader));
				ZDBOnlySubQuery transportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transportSubQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.ImporterSecurityFiling);

				if (!loadPort.IsEmpty)
				{
					bool isCountryCode = loadPort.Length == 2;
					SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

					transportSubQuery.AddToFilter(JoinCondition.And, JobConsolTransportSchema.JW_RL_NKLoadPort, comparisonOperator, loadPort);
				}

				if (!discPort.IsEmpty)
				{
					bool isCountryCode = discPort.Length == 2;
					SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

					transportSubQuery.AddToFilter(JoinCondition.And, JobConsolTransportSchema.JW_RL_NKDiscPort, comparisonOperator, discPort);
				}

				headerQuery.AddSubQuery(transportSubQuery, JoinCondition.And);

				query.AddToFilter(headerQuery);
			}

			return query;
		}

		ZQuery GetShipmentNumber(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, value);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));
			result.AddSubQuery(CusISFHeaderSchema.BF_JS_Shipment, shipmentSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetTransportVoyageFlight(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetTransport(JobConsolTransportSchema.JW_VoyageFlight, comparisonOperator, value);
		}

		ZQuery GetTransportVessel(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetTransport(JobConsolTransportSchema.JW_Vessel, comparisonOperator, value);
		}

		ZDBOnlyQuery GetTransport(SchemaDateTimeColumn column, SchemaDateTimeColumn voyageColumn, DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo, ZBool isOrigin)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));
			ZDBOnlySubQuery transportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
			ZDBOnlySubQuery voyageSubQuery = new ZDBOnlySubQuery(isOrigin ? typeof(VoyageOrigin) : typeof(VoyageDestination), isOrigin ? JobSailingSchema.JX_JA : JobSailingSchema.JX_JB);
			AddDateTimeRange(voyageSubQuery, comparisonOperator, JoinCondition.And, voyageColumn, dateFrom, dateTo);

			sailingSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);
			transportSubQuery.AddSubQuery(sailingSubQuery, JoinCondition.And);
			transportSubQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.ImporterSecurityFiling);
			transportSubQuery.AddToFilter(JobConsolTransportSchema.JW_IsLinked, true);

			ZDBOnlySubQuery dateSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			AddDateTimeRange(dateSubQuery, comparisonOperator, JoinCondition.And, column, dateFrom, dateTo);
			transportSubQuery.AddAsUnionQuery(dateSubQuery);

			result.AddSubQuery(transportSubQuery, JoinCondition.And);

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				ZDBOnlySubQuery noTransportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID, true);
				noTransportSubQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.ImporterSecurityFiling);
				result.AddSubQuery(noTransportSubQuery, JoinCondition.Or);
			}

			return result;
		}

		ZDBOnlyQuery GetTransport(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusISFHeader));
			ZDBOnlySubQuery transportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportSubQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.ImporterSecurityFiling);
			transportSubQuery.AddToFilter(column, comparisonOperator, value);
			result.AddSubQuery(transportSubQuery, JoinCondition.And);

			return result;
		}

		CusISFHeaderLookups Lookups
		{
			get { return fLookups ?? (fLookups = Factory.GetNull<CusISFHeader>().Lookups); }
		}
		CusISFHeaderLookups fLookups;

		public LocationCollection Location_List
		{
			get { return fLocation_List ?? (fLocation_List = new LocationCollection(Factory)); }
		}
		LocationCollection fLocation_List;

		public CodeDescriptionPairList DispositionCodeList
		{
			get
			{
				return Factory.GetCachedValue("USISF.DispositionCodeListWithMultiple", delegate
				{
					var result = new DispositionCodeList();
					result.AddPair(ISFStatusHelper.Multiple, CusISFHeader.MultipleBillsWithDifferentStatuses);
					return result;
				});
			}
		}

		public CodeDescriptionPairList DISStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("DISStatusList", delegate
				{
					var result = new Common.US.DIS.StatusList();
					result.RemoveCode(Enterprise.Customs.Common.US.DIS.StatusList.Codes.MUL);
					return result;
				});
			}
		}

		#region Customs Attributes
		ZQuery GetCustomAttribute1(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetGenCustomAddOnValueQuery(comparisonOperator, CusISFHeader.Schema.CustomAttribute1, value);
		}

		ZQuery GetCustomAttribute2(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetGenCustomAddOnValueQuery(comparisonOperator, CusISFHeader.Schema.CustomAttribute2, value);
		}

		ZQuery GetGenCustomAddOnValueQuery(SQLComparisonOperator comparisonOperator, ZString propertyName, ZString value)
		{
			var operatorToSwitch = comparisonOperator.Equals(SQLComparisonOperator.NotContains) ||
				comparisonOperator.Equals(SpecialComparisonOperator.IsBlank) ||
				comparisonOperator.Equals(SQLComparisonOperator.NotEqual) ||
				comparisonOperator.Equals(SQLComparisonOperator.DoesNotStartWith);
			return GetGenCustomAddOnValueQuery(comparisonOperator, propertyName, value, operatorToSwitch);
		}

		ZDBOnlyQuery GetGenCustomAddOnValueQuery(SQLComparisonOperator comparisonOperator, ZString propertyName, ZString value, bool notIn)
		{
			var query = new ZDBOnlyQuery(typeof(CusISFHeader));
			var referenceDataSubQuery = new ZDBOnlySubQuery(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, notIn);
			referenceDataSubQuery.AddToFilter(GenCustomAddOnValueSchema.XV_Name, propertyName);
			referenceDataSubQuery.AddToFilter(GenCustomAddOnValueSchema.XV_Data, ChangeComparisonOperator(comparisonOperator), value);
			query.AddSubQuery(referenceDataSubQuery, JoinCondition.And);

			return query;
		}

		SQLComparisonOperator ChangeComparisonOperator(SQLComparisonOperator comparisonOperator)
		{
			if (comparisonOperator.Equals(SQLComparisonOperator.NotContains))
			{
				comparisonOperator = SQLComparisonOperator.Contains;
			}
			else if (comparisonOperator.Equals(SpecialComparisonOperator.IsBlank))
			{
				comparisonOperator = SQLComparisonOperator.NotEqual;
			}
			else if (comparisonOperator.Equals(SQLComparisonOperator.NotEqual))
			{
				comparisonOperator = SQLComparisonOperator.Equal;
			}
			else if (comparisonOperator.Equals(SQLComparisonOperator.DoesNotStartWith))
			{
				comparisonOperator = SQLComparisonOperator.StartsWith;
			}
			return comparisonOperator;
		}
		#endregion

		#endregion
	}
}
