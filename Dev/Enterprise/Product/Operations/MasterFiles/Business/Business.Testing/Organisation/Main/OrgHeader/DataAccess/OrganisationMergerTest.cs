using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using static CargoWise.Database.TestFramework.ObjectModel.AssertionChainExtensions;
using static CargoWise.Database.TestFramework.ObjectModel.IWhsWarehouseSQLExtensions;
using static Enterprise.MasterFiles.Business.OrgPartRelation;
using BarcodeRule = CargoWise.Database.TestFramework.ObjectModel.BarcodeRule;
using BarcodeRuleSet = CargoWise.Database.TestFramework.ObjectModel.BarcodeRuleSet;
using BarcodeValidationRule = CargoWise.Database.TestFramework.ObjectModel.BarcodeValidationRule;
using JobStorage = CargoWise.Database.TestFramework.ObjectModel.JobStorage;
using OrgAddressDO = CargoWise.Database.TestFramework.ObjectModel.OrgAddress;
using OrgHeaderDO = CargoWise.Database.TestFramework.ObjectModel.OrgHeader;
using OrgSupplierPartDO = CargoWise.Database.TestFramework.ObjectModel.OrgSupplierPart;
using PkgPackage = CargoWise.Database.TestFramework.ObjectModel.PkgPackage;
using PkgPackageJob = CargoWise.Database.TestFramework.ObjectModel.PkgPackageJob;
using ProductionRule = CargoWise.Database.TestFramework.ObjectModel.ProductionRule;
using ProductionRuleSet = CargoWise.Database.TestFramework.ObjectModel.ProductionRuleSet;
using StmALogDO = CargoWise.Database.TestFramework.ObjectModel.StmALog;
using WhsAdHocServiceJob = CargoWise.Database.TestFramework.ObjectModel.WhsAdHocServiceJob;
using WhsArea = CargoWise.Database.TestFramework.ObjectModel.WhsArea;
using WhsClientParameterByWarehouse = CargoWise.Database.TestFramework.ObjectModel.WhsClientParameterByWarehouse;
using WhsClientPickPackParamsByWhs = CargoWise.Database.TestFramework.ObjectModel.WhsClientPickPackParamsByWhs;
using WhsDocket = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;
using WhsDocketLine = CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine;
using WhsPick = CargoWise.Database.TestFramework.ObjectModel.WhsPick;
using WhsPickFace = CargoWise.Database.TestFramework.ObjectModel.WhsPickFace;
using WhsProductParamsByWhsAndClient = CargoWise.Database.TestFramework.ObjectModel.WhsProductParamsByWhsAndClient;
using WhsProductStyle = CargoWise.Database.TestFramework.ObjectModel.WhsProductStyle;
using WhsSalesChannel = CargoWise.Database.TestFramework.ObjectModel.WhsSalesChannel;
using WhsSerialNumber = CargoWise.Database.TestFramework.ObjectModel.WhsSerialNumber;
using WhsSerialNumberPivot = CargoWise.Database.TestFramework.ObjectModel.WhsSerialNumberPivot;
using WhsVASOrder = CargoWise.Database.TestFramework.ObjectModel.WhsVASOrder;
using WhsWarehouse = CargoWise.Database.TestFramework.ObjectModel.WhsWarehouse;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrganisationMergerTest : TransactionedTestCase
	{
		public void TestRemoveConcurrencyExceptionBeforeMerge()
		{
			var factory = new BusinessObjectFactory();
			var oldOrg = factory.New<OrgHeader>();
			var mergeHeader = new MergeOrgHeader(factory, oldOrg);
			mergeHeader.ConcurrencyException = new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Ooops!"), ((INeedRow)oldOrg).Row, Db.Connection), factory);
			var merger = new OrganisationMergerForTest(mergeHeader);

			AssertNotNull(mergeHeader.ConcurrencyException);
			merger.ActionOnSave = OrganisationMergerActionOnSave.DeleteOnly | OrganisationMergerActionOnSave.MergeOnly;
			merger.Save();
			AssertNull(mergeHeader.ConcurrencyException);
		}

		public void TestDuplicateStaffAssignments()
		{
			var factory = new BusinessObjectFactory();

			var newOrg = factory.NewWithValidTestData<OrgHeader>();
			var oldOrg = factory.NewWithValidTestData<OrgHeader>();

			var company = factory.NewWithValidTestData<GlbCompany>();

			var ass1 = newOrg.StaffAssignments.AddNew();
			var ass2 = oldOrg.StaffAssignments.AddNew();

			var staff1 = factory.NewWithValidTestData<GlbStaff>();

			ass1.O8_Role = ass2.O8_Role = "ACT";
			ass1.O8_GS_NKPersonResponsible = ass2.O8_GS_NKPersonResponsible = staff1.GS_Code;
			ass1.O8_Department = ass2.O8_Department = "FRT";
			ass1.O8_GC = ass2.O8_GC = company.PK;

			factory.Save();

			var testMerger = new OrganisationMergerForTest(newOrg.PK, oldOrg.PK, new MergeOrgAddressCollection(factory, newOrg, oldOrg), new MergeOrgContactCollection(factory, newOrg, oldOrg));
			testMerger.DeleteOldOrg = false;

			AssertNoExceptionThrown("We should not have any issues merging identical staff assignments", () => testMerger.Save());

			var reloadedOrgStaffAssignments = (new BusinessObjectFactory()).Load<OrgHeader>(oldOrg.PK).StaffAssignments;
			reloadedOrgStaffAssignments.CompanySpecific = false;
			AssertEquals("Since post merge the assignments were identical, there should have been no attempt to merge", 1, reloadedOrgStaffAssignments.Count);
		}

		public void TestUniqueIndexes()
		{
			StringCollectionX handled = new StringCollectionX();
			handled.Add(JobVoyAccountSchema.Constants.Indexes.NR_UX__NA_JV_NA_OH_NA_GC); // handled in OrgMerger validation
			handled.Add(AccTransactionHeaderSchema.Constants.Indexes.NR_UX__AH_GC_AH_Ledger_AH_OH_AH_TransactionType_AH_TransactionNum_AH_TransactionCount);
			handled.Add(JobOrderHeaderSchema.Constants.Indexes.NR_UX__JD_OrderNumber_JD_OrderNumberSplit_JD_OA_BuyerAddress);
			handled.Add(OrgAddressCapabilitySchema.Constants.Indexes.FK_UC__PZ_OA_PZ_AddressType);
			handled.Add(OrgCompanyDataSchema.Constants.Indexes.FK_UC__OB_GC_OB_OH);
			handled.Add(OrgContactSchema.Constants.Indexes.NR_UX__OC_ContactName_OC_OH);
			handled.Add(OrgContactSchema.Constants.Indexes.NR_UX__OC_OH_OC_Email);
			handled.Add(OrgPartRelationSchema.Constants.Indexes.FK_UC__OU_OP_OU_Relationship_OU_OH);
			handled.Add(OrgRelatedPartySchema.Constants.Indexes.NR_UX__PR_OH_Parent_PR_GC_PR_PartyType_PR_FreightTransportMode_PR_FreightContainerMode_PR_FreightDirection_PR_Service_PR_Loc);
			handled.Add(RatingHeaderSchema.Constants.Indexes.FK_UX__TH_OH_TH_GC_TH_RateType_TH_QuoteNumber_TH_GlobalRateLevel);
			handled.Add(RefOrgConsortiumPivotSchema.Constants.Indexes.NR_UC__RO_RG_RO_OH);
			handled.Add(WhsClientParameterByWarehouseSchema.Constants.Indexes.NR_UX__WY_WW_Whs_WY_OH_Client_WY_ReceiveCategory);
			handled.Add(WhsProductParamsByWhsAndClientSchema.Constants.Indexes.FK_UX__W3_OH_W3_WW_W3_OP);
			handled.Add(WhsClientPickPackParamsByWhsSchema.Constants.Indexes.FK_UC__WPP_OH_Client_WPP_WW_Warehouse_WPP_WSH_SalesChannel);
			handled.Add(WhsDocketSchema.Constants.Indexes.NR_UX__WD_OH_Client_WD_ExternalReference_WD_DocketType_WD_ExternalReferenceSplit);
			handled.Add(WhsDocketSchema.Constants.Indexes.NR_UX__WD_WP_ParentPickForReceive_WD_OH_Client);
			handled.Add(WhsPickFaceSchema.Constants.Indexes.FK_UX__WF_OH_Client_WF_OP_WF_WL);
			handled.Add(WhsVASOrderSchema.Constants.Indexes.NR_UX__WVO_CustomerReferenceNo_WVO_OH_Client);
			handled.Add(CusSeaManSlotOrgSchema.Constants.Indexes.FK_UX__BS_OH_SlotCharterer_BS_BT);
			handled.Add(JobTradeLaneVoyageSchema.Constants.Indexes.FK_UX__NB_OH_NB_JV);
			handled.Add("FK_UX__LC_OH");//special case
			handled.Add(RefPacksSchema.Constants.Indexes.NR_UX__RP_Type_RP_CommercialPack_RP_CustomsPack_RP_CustomsCountry_RP_OH_Supplier);
			handled.Add(OrgProductTypeSchema.Constants.Indexes.FK_UC__OPT_OH_Owner_OPT_Code);
			handled.Add(StmMenuDocumentConfigSchema.Constants.Indexes.NR_UX__S3_SI_S3_GC_S3_OH_S3_IsSystem);
			handled.Add(OrgAddressSchema.Constants.Indexes.FK_UX__OA_OH_OA_Code);
			handled.Add(OrgServiceLevelSchema.Constants.Indexes.FK_UX__PM_RS_PM_RS_NKSrvLvl_PM_OH);
			handled.Add(OrgRateFeeChargeLevelSchema.Constants.Indexes.FK_UC__ORF_OH_ORF_ServiceType);
			handled.Add(OrgContactItemSchema.Constants.Indexes.FK_UC__OI_OC_OI_ContactItemType_OI_Description_OI_Address);
			handled.Add(AccGlobalChargeCodeMapPivotSchema.Constants.Indexes.FK_UX__YP_YG_YP_AC_YP_TYPE_YP_OH_LocalClientOverride);
			handled.Add(BarcodeRuleSetSchema.Constants.Indexes.NR_UX__BRS_Module_BRS_OH_Buyer_BRS_OH_Supplier_BRS_RelatedEntityId_BRS_IsSystem);
			handled.Add(BPMConfigurationTmplSchema.Constants.Indexes.NR_UX__VCT_Hostname_VCT_ParentTableCode_VCT_ParentID_VCT_GE_Department_VCT_GB_Branch_VCT_GC_Company_VCT_OH_Client);
			handled.Add(BPMConfigurationTmplSchema.Constants.Indexes.NR_UX__VCT_ParentID_VCT_GE_Department_VCT_GB_Branch_VCT_GC_Company_VCT_OH_Client);
			handled.Add(OrgRateCommodityDefaultingRuleSchema.Constants.Indexes.NR_UX__ORC_OH_ORC_Commodity_ORC_Origin_ORC_Destination_ORC_TransportMode_ORC_ContainerMode_ORC_Direction_ORC_ServiceLevel);
			handled.Add(GlbCompanyCampaignSubscriptionSchema.Constants.Indexes.NR_UX__GCS_MediaCategory_GCS_MediaType_GCS_OH_GCS_Email);
			handled.Add(OrgCarrierAccountSchema.Constants.Indexes.FK_UC__OAN_OH_Carrier_OAN_AccountNumber);
			handled.Add(OrgCarrierAccountMetaDataSchema.Constants.Indexes.NR_UX__OAM_OAN_CarrierAccount_OAM_Name);
			handled.Add(OrgCarrierNamedAccountSchema.Constants.Indexes.FK_UX__ONA_OH_Carrier_ONA_ForeignName);
			handled.Add(CusPermitHeaderSchema.Constants.Indexes.FK_UX__CPH_OH_PermitHolder_CPH_RN_NKCountryCode_CPH_Number_CPH_StartDate_CPH_Type_CPH_SubType);
			handled.Add(StmNumberRangeMatchingDetailSchema.Constants.Indexes.NR_UX__NRM_OwnerId_NRM_RangeType_NRM_OH_Client_NRM_WW_Whs);
			handled.Add(JobExRateSchema.Constants.Indexes.FK_UX__JF_OH_Org_JF_JH_JF_OrgType_JF_RX_NKRateCurrency_JF_InvoiceCurrencyType);
			handled.Add(JobShipmentGatewaySchema.Constants.Indexes.FK_UX__JSG_OA_ForwarderAddress_JSG_Sequence_JSG_JS_Shipment);
			handled.Add("FK_UX__WPC_WL_Location_WPC_LocationCacheType_WPC_PartialPalletID_WPC_OP_Product_WPC_OH_Client");
			handled.Add(StmUniversalJobLinkSchema.Constants.Indexes.FK_UX__UCL_OH_Owner_UCL_ParentID_UCL_SourceType);
			handled.Add(OrgCusAccountSchema.Constants.Indexes.FK_UC__CZ_OH_CZ_RN_NKCountryCode_CZ_Code_CZ_Issuer_CZ_Account);
			handled.Add(WhsAdHocServiceJobSchema.Constants.Indexes.FK_UX__WSJ_OH_Client_WSJ_CustomerReference);
			handled.Add(OrgAirlineMAWBStockManagementSchema.Constants.Indexes.FK_UX__OHM_OH_Carrier_OHM_GC_Company_OHM_GB_Branch);
			handled.Add(AccTaxReturnLineSchema.Constants.Indexes.FK_UC__ARL_ATR_AccTaxReturn_ARL_OH_Organisation_ARL_OrgMergeCounter);
			handled.Add(OrgAirlineBranchAccountSchema.Constants.Indexes.FK_UC__OAA_OH_Carrier_OAA_GB_Branch);
			handled.Add(AccOrgTaxConfigurationTemplateSchema.Constants.Indexes.FK_UC__OCT_GC_Company_OCT_Code); //it has not relation to this and included here only because it contains OC in name as per this test SQL.
			handled.Add(AccOrgTaxConfigurationTemplateSchema.Constants.Indexes.NR_UX__OCT_GC_Company_OCT_Description); //it has not relation to this and included here only because it contains OC in name as per this test SQL.
			handled.Add(AccOrgTaxConfigurationSchema.Constants.Indexes.FK_UX__OTC_OCT_OTC_ETC); //it has not relation to this and included here only because it contains OC in name as per this test SQL.
			handled.Add(OrgTranslatedAddressSchema.Constants.Indexes.FK_UC__OTA_OA_OTA_Language);
			handled.Add(RatingContractSchema.Constants.Indexes.FK_UX__RCT_OH_RCT_ContractNumber);
			handled.Add(OrgAddressAdditionalInfoSchema.Constants.Indexes.NR_UX__OAI_OA_Address);
			handled.Add(OrgTranslatedAddressAdditionalInfoSchema.Constants.Indexes.FK_UC__OTI_OAI_OTI_Language);
			handled.Add(RatingContractNamedAccountPivotSchema.Constants.Indexes.NR_UC__RNP_ParentTableCode_RNP_ParentID_RNP_OH_NamedAccount);
			handled.Add(AllocationRouteAgentPivotSchema.Constants.Indexes.NR_UC__ARA_RCA_AllocationLine_ARA_OH_Agent);
			handled.Add(CusIntrastatGroupSchema.Constants.Indexes.NR_UX__CIG_GC_Company_CIG_OH_Reporter_CIG_Period_CIG_Flow);
			handled.Add(CusIntrastatHeaderSchema.Constants.Indexes.NR_UX__CIH_GC_Company_CIH_OH_Supplier_CIH_SupplierName_CIH_OH_Consignee_CIH_ConsigneeName_CIH_TradersReference);
			handled.Add(RateOneOffCarrierSchema.Constants.Indexes.FK_UC__TTC_TT_TTC_OH_Carrier);
			// Cannot Merge Duplicate Serial across two Clients, see TestMergeWhsSerialNumber_Duplicate
			handled.Add(WhsSerialNumberSchema.Constants.Indexes.NR_UX__WSN_SerialNumber_WSN_OH_Client_WSN_OP_Product);
			handled.Add(OrgCompetitorSchema.Constants.Indexes.NR_UX__OCP_OH_Parent_OCP_Type_OCP_OH_Competitor_OCP_GC_Company);
			handled.Add(RatingDocumentsChargeOrderSchema.Constants.Indexes.NR_UX__RCO_DocumentType_RCO_OH_Client_RCO_AC_ChargeCode);
			handled.Add(OrgRefFacilitySchema.Constants.Indexes.NR_UX__OFC_RFT_Facility_OFC_OH_Organization_OFC_OA_PremisesAddress);
			handled.Add(GlbGroupOrgContactLinkSchema.Constants.Indexes.NR_UX__GCK_GG_Group_GCK_OC_Contact);
			handled.Add(GlbGroupOrgLinkSchema.Constants.Indexes.NR_UX__GOK_GG_Group_GOK_OH_Org);
			handled.Add(CusCalculationRuleSchema.Constants.Indexes.NR_UX__CCR_GC_Company_CCR_OH_Importer_CCR_TransportMode_CCR_RuleType_CCR_StartDate);
			handled.Add(CusBRForeignOperatorSchema.Constants.Indexes.NR_UX__BFR_OH_Owner_BFR_AuthorityIdentifier);
			handled.Add(CusBRForeignOperatorSchema.Constants.Indexes.NR_UX__BFR_OH_Owner_BFR_OH_ForeignOperator);
			handled.Add("FK_UC__COC_COP_Opportunity_COC_OC_Contact"); //Unique index from CrmOpportunityContact table, which is not exposed in Dev.

			StringCollectionX handledOnOrgDelete = new StringCollectionX();
			handledOnOrgDelete.Add(OrgSupplierBuyerLinkSchema.Constants.Indexes.FK_UX__OL_OH_Supplier_OL_OH_Buyer_OL_RN_NKImporterCountry);
			handledOnOrgDelete.Add(OrgSecuritySchema.Constants.Indexes.NR_UX__OX_SecurityItemName_OX_OH_OX_SU);
			handledOnOrgDelete.Add(OrgSecurityContactsSchema.Constants.Indexes.FK_UC__OZ_OX_OZ_OC);
			handledOnOrgDelete.Add(OrgPatternMatchOverrideSchema.Constants.Indexes.FK_UC__OO_OH_OO_Relationship_OO_ForeignCode_OO_Context);
			handledOnOrgDelete.Add(OrgCusCodeSchema.Constants.Indexes.NR_UX__OK_CodeType_OK_OH_OK_RN_NKCodeCountry_OK_OA_PremisesAddress);
			handledOnOrgDelete.Add(OrgCusCodeSchema.Constants.Indexes.NR_UX__OK_OH_OK_RN_NKCodeCountry_OK_CodeType);
			handledOnOrgDelete.Add(OrgCusCodeValiditySchema.Constants.Indexes.FK_UC__OCV_OK_OrgCusCode);
			handledOnOrgDelete.Add(OrgMiscServSchema.Constants.Indexes.FK_UC__OM_OH);
			handledOnOrgDelete.Add(OrgCountryDataSchema.Constants.Indexes.FK_UC__OV_OH_OrgHeader_OV_RN_NKClientCountryRelation_OV_OA_ApprovedLocation);
			handledOnOrgDelete.Add(OrgAgentRelationshipSchema.Constants.Indexes.FK_UX__O3_OH_SendingAgent_O3_OH_ReceivingAgent_O3_OH_GroupNetworkOrFranchise);
			handledOnOrgDelete.Add("NR_UC__Y3_OH_Y3_GC_Y3_Ledger"); // this is special as should not be seen by business layer at all
			handledOnOrgDelete.Add(OrgStaffAssignmentsSchema.Constants.Indexes.NR_UX__O8_Role_O8_Department_O8_OH_O8_GC_O8_Product);
			handledOnOrgDelete.Add(OrgTradePeriodSchema.Constants.Indexes.FK_UX__PAS_PA_PAS_OH_Client_PAS_Period_PAS_IsJobValue_PAS_IsTraded);
			handledOnOrgDelete.Add(AccChargeCodeCarrierIataMappingSchema.Constants.Indexes.FK_UC__ACI_AC_ChargeCode_ACI_OH_Carrier);
			handledOnOrgDelete.Add(OrgAddressAdditionalInfoSchema.Constants.Indexes.NR_UX__OAI_OA_Address);
			handledOnOrgDelete.Add(OrgTranslatedAddressAdditionalInfoSchema.Constants.Indexes.FK_UC__OTI_OAI_OTI_Language);

			string sql = @"select tab.name, ind.name as indexname from sys.indexes ind
				inner join sys.objects tab on ind.object_id = tab.object_id
				where
					ind.is_unique = 1
					and ind.name not like 'PK_%'
					and tab.type = 'U'
					and (ind.name like '%[_]OH%' or ind.name like '%[_]OA%' or ind.name like '%[_]OC%')
					and tab.name <> 'OrgHeader'
				order by tab.name, ind.name";
			DataTable indexesTable = new DataTable("UniqueIndexes");
			Db.Connection.Command(sql).NewDataAdapter().Fill(indexesTable); // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			StringCollectionX col = new StringCollectionX();
			foreach (DataRow row in indexesTable.Rows)
			{
				string entry = string.Empty;
				if (!handled.Contains(row["indexname"].ToString()) && !handledOnOrgDelete.Contains(row["indexname"].ToString()))
				{
					foreach (DataColumn column in indexesTable.Columns)
					{
						entry += entry.Length > 0 ? "::          " + row[column].ToString() : row[column].ToString();
					}
					col.Add(entry);
				}
			}
			Assert("Following unique indexes were not handled properly and may cause problems during org merge:" +
				System.Environment.NewLine + System.Environment.NewLine + string.Join(System.Environment.NewLine +
				System.Environment.NewLine + System.Environment.NewLine + "This may happen if a new index was added. After you fix these, add appropriate index to this test to consider it handled"
				, col.ToArray()), col.Count == 0);
		}

		public void TestAllOrgTablesHandled()
		{
			List<string> alreadyHandled = new List<string>();
			alreadyHandled.Add("OrgAddress");
			alreadyHandled.Add("OrgAgentRelationship");
			alreadyHandled.Add("OrgAirlineMAWBStockManagement");
			alreadyHandled.Add("OrgAppointedAgentPorts");
			alreadyHandled.Add("OrgBrandOrRelatedName");
			alreadyHandled.Add("OrgCarrierNamedAccount");
			alreadyHandled.Add("OrgColdCallRegister");
			alreadyHandled.Add("OrgCommissionAgreement");
			alreadyHandled.Add("OrgCommissionAgreementRecipient");
			alreadyHandled.Add("OrgCompanyData");
			alreadyHandled.Add("OrgContact");
			alreadyHandled.Add("OrgContainerDetention");
			alreadyHandled.Add("OrgCountryData");
			alreadyHandled.Add("OrgCusAccount");
			alreadyHandled.Add("OrgCusCode");
			alreadyHandled.Add("OrgCustomerAddress");
			alreadyHandled.Add("OrgCustomLabels");
			alreadyHandled.Add("OrgDocument");
			alreadyHandled.Add("OrgLandedCostingPrefs");
			alreadyHandled.Add("OrgMatchApproval");
			alreadyHandled.Add("OrgMiscServ");
			alreadyHandled.Add("OrgOpportunity");
			alreadyHandled.Add("OrgPartRelation");
			alreadyHandled.Add("OrgPatternMatch");
			alreadyHandled.Add("OrgPatternMatchAddress");
			alreadyHandled.Add("OrgPatternMatchOverride");
			alreadyHandled.Add("OrgProfitShareDetails");
			alreadyHandled.Add("OrgRateCommodityDefaultingRule");
			alreadyHandled.Add("OrgRateTariffLevel");
			alreadyHandled.Add("OrgRelatedParty");
			alreadyHandled.Add("OrgSales");
			alreadyHandled.Add("OrgSalesCall");
			alreadyHandled.Add("OrgSecurity");
			alreadyHandled.Add("OrgServiceLevel");
			alreadyHandled.Add("OrgStaffAssignments");
			alreadyHandled.Add("OrgSupBuyLinkTrnMode");
			alreadyHandled.Add("OrgSupplierBuyerLink");
			alreadyHandled.Add("OrgTradeDetail");
			alreadyHandled.Add("OrgTradePeriod");
			alreadyHandled.Add("OrgTradeProspect");
			alreadyHandled.Add("OrgTradeValue");
			alreadyHandled.Add("OrgWebURL");
			alreadyHandled.Add("OrgBondSuretyDetail");
			alreadyHandled.Add("OrgProductType");
			alreadyHandled.Add("OrgRateFeeChargeLevel");
			alreadyHandled.Add("OrgCarrierAccount");
			alreadyHandled.Add("OrgWhsClientAccountAssociation");
			alreadyHandled.Add("OrgAirlineBranchAccount");
			alreadyHandled.Add("OrgCompetitor");
			alreadyHandled.Add("OrgRefFacility");

			List<string> addedTables = new List<string>();

			string sqlText = @"
				SELECT
					distinct col.table_name
				FROM
					information_schema.columns col
					INNER JOIN information_schema.tables tab ON tab.table_name = col.table_name
				WHERE
					tab.table_type = 'BASE TABLE'
					AND col.data_type = 'uniqueidentifier'
					AND tab.table_name like 'Org%'
					AND (
						col.column_name like '__[_]OH' OR col.column_name like '__[_]OH[_]%'
						OR col.column_name like '___[_]OH' OR col.column_name like '___[_]OH[_]%'
					)";

			using (var reader = GetCommandOnMainConnection(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					string tableName = reader.GetString(0);

					if (!alreadyHandled.Contains(tableName))
					{
						addedTables.Add(tableName);
					}
				}
			}

			Assert(@"There were some Org tables added. This may result in Organization merge problems.
To fix this:
1) If this table(s) has to be merged during org merge
	1.1) go to Dev\Enterprise\Product\Core\Database\Script\Public\MasterFiles\OrgTablesToMerge.sql and add its name there
	1.2) if the table has the FK to OrgHeader as a part of any unique index, you have to write a script here to delete all data that can't be merged due to index violation (see DeleteRemainingServiceLevels() for an example)
	1.3) fix this test by adding table name to the alreadyHandled list
2) if this table(s) does not have to be merged you can either
	2.1) go to OrgHeader.Delete() and delete collection or whatever represents this table in OrgHeader OR
	2.2) write a script here to delete all data from this table (see DeleteRemainingServiceLevels() for an example)
	2.3) fix this test by adding table name to the alreadyHandled list

Tables that require fixing: " + string.Join(", ", addedTables.ToArray()), addedTables.Count == 0);
		}

		public void TestMoveGlobalChargeCodePivot()
		{
			// Get Organisations to test (OLD and NEW)
			string sqlText = "SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'EDICUS'";
			Guid oldOrgPk;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldOrgPk = (Guid)cmd.ExecuteScalar();
			}
			sqlText = "SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'DEMORG'";
			Guid newOrgPk;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newOrgPk = (Guid)cmd.ExecuteScalar();
			}

			sqlText = string.Format("SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_CODE = 'FRT' AND AC_GC = '{0}'", GlbCompany.CurrentCompany.PK);
			Guid fRT;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				fRT = (Guid)cmd.ExecuteScalar();
			}
			sqlText = string.Format("SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_CODE = 'BAF' AND AC_GC = '{0}'", GlbCompany.CurrentCompany.PK);
			Guid bAF;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				bAF = (Guid)cmd.ExecuteScalar();
			}

			Guid globalChargeCodePK = Guid.NewGuid();
			Guid pivot1PK = Guid.NewGuid();
			Guid pivot2PK = Guid.NewGuid();
			Guid pivot3PK = Guid.NewGuid();

			// Insert test OrgCusCode records - 2 linked to OldOrg and 1 linked to NewOrg conflicting with one of OldOrg
			sqlText = string.Format(@"
				INSERT dbo.AccGlobalChargeCodeMap(YG_PK, YG_Code, YG_Desc, YG_OH, YG_IsActive, YG_SystemCreateTimeUtc, YG_SystemCreateUser, YG_SystemLastEditTimeUtc, YG_SystemLastEditUser) VALUES ('{0}', 'TESTCODE', 'Test Only', null, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.AccGlobalChargeCodeMapPivot (YP_PK, YP_YG, YP_AC, YP_TYPE, YP_OH_LocalClientOverride, YP_SystemCreateTimeUtc, YP_SystemCreateUser, YP_SystemLastEditTimeUtc, YP_SystemLastEditUser) VALUES ('{5}', '{0}', '{3}', 'AP', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.AccGlobalChargeCodeMapPivot (YP_PK, YP_YG, YP_AC, YP_TYPE, YP_OH_LocalClientOverride, YP_SystemCreateTimeUtc, YP_SystemCreateUser, YP_SystemLastEditTimeUtc, YP_SystemLastEditUser) VALUES ('{6}', '{0}', '{4}', 'AP', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.AccGlobalChargeCodeMapPivot (YP_PK, YP_YG, YP_AC, YP_TYPE, YP_OH_LocalClientOverride, YP_SystemCreateTimeUtc, YP_SystemCreateUser, YP_SystemLastEditTimeUtc, YP_SystemLastEditUser) VALUES ('{7}', '{0}', '{3}', 'AP', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				globalChargeCodePK, oldOrgPk, newOrgPk, fRT, bAF, pivot1PK, pivot2PK, pivot3PK);
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.ExecuteNonQuery();
			}

			sqlText = string.Format("SELECT YP_OH_LocalClientOverride FROM dbo.AccGlobalChargeCodeMapPivot WHERE YP_PK = '{0}'", pivot1PK);
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				AssertEquals("BEFORE - pivot1 should use oldOrgPK", oldOrgPk, (Guid)cmd.ExecuteScalar());
			}
			sqlText = string.Format("SELECT YP_OH_LocalClientOverride FROM dbo.AccGlobalChargeCodeMapPivot WHERE YP_PK = '{0}'", pivot2PK);
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				AssertEquals("BEFORE - pivot2 should use oldOrgPK", oldOrgPk, (Guid)cmd.ExecuteScalar());
			}
			sqlText = string.Format("SELECT YP_OH_LocalClientOverride FROM dbo.AccGlobalChargeCodeMapPivot WHERE YP_PK = '{0}'", pivot3PK);
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				AssertEquals("BEFORE - pivot3 should use newOrgPK", newOrgPk, (Guid)cmd.ExecuteScalar());
			}

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgPk, newOrgPk, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			sqlText = string.Format("SELECT YP_OH_LocalClientOverride FROM dbo.AccGlobalChargeCodeMapPivot WHERE YP_PK = '{0}'", pivot1PK);
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				AssertNull("After - pivot1 should be deleted as duplicate already exists", cmd.ExecuteScalar());
			}
			sqlText = string.Format("SELECT YP_OH_LocalClientOverride FROM dbo.AccGlobalChargeCodeMapPivot WHERE YP_PK = '{0}'", pivot2PK);
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				AssertEquals("After - pivot2 should be updated to use the newOrgPK", newOrgPk, (Guid)cmd.ExecuteScalar());
			}
			sqlText = string.Format("SELECT YP_OH_LocalClientOverride FROM dbo.AccGlobalChargeCodeMapPivot WHERE YP_PK = '{0}'", pivot3PK);
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				AssertEquals("After - pivot3 should stay untouched", newOrgPk, (Guid)cmd.ExecuteScalar());
			}
		}

		public void TestMoveGlobalChargeCodePivot_DoesNotThrowForLargeQuery()
		{
			// Get Organisations to test (OLD and NEW)
			string sqlText = "SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'EDICUS'";
			Guid oldOrgPk;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldOrgPk = (Guid)cmd.ExecuteScalar();
			}
			sqlText = "SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'DEMORG'";
			Guid newOrgPk;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newOrgPk = (Guid)cmd.ExecuteScalar();
			}

			sqlText = string.Format("SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_CODE = 'FRT' AND AC_GC = '{0}'", GlbCompany.CurrentCompany.PK);
			Guid fRT;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				fRT = (Guid)cmd.ExecuteScalar();
			}
			sqlText = string.Format("SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_CODE = 'BAF' AND AC_GC = '{0}'", GlbCompany.CurrentCompany.PK);
			Guid bAF;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				bAF = (Guid)cmd.ExecuteScalar();
			}

			var sqlBuilder = new StringBuilder();
			for (int i = 0; i <= 2100; i++)
			{
				Guid globalChargeCodePK = Guid.NewGuid();
				Guid pivot1PK = Guid.NewGuid();
				Guid pivot2PK = Guid.NewGuid();

				// Insert test OrgCusCode records - 2 linked to OldOrg and 1 linked to NewOrg conflicting with one of OldOrg
				sqlBuilder.AppendLine(string.Format(@"
				INSERT dbo.AccGlobalChargeCodeMap(YG_PK, YG_Code, YG_Desc, YG_OH, YG_IsActive, YG_SystemCreateTimeUtc, YG_SystemCreateUser, YG_SystemLastEditTimeUtc, YG_SystemLastEditUser) VALUES ('{0}', 'TESTCODE', 'Test Only', null, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.AccGlobalChargeCodeMapPivot (YP_PK, YP_YG, YP_AC, YP_TYPE, YP_OH_LocalClientOverride, YP_SystemCreateTimeUtc, YP_SystemCreateUser, YP_SystemLastEditTimeUtc, YP_SystemLastEditUser) VALUES ('{5}', '{0}', '{3}', 'AP', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.AccGlobalChargeCodeMapPivot (YP_PK, YP_YG, YP_AC, YP_TYPE, YP_OH_LocalClientOverride, YP_SystemCreateTimeUtc, YP_SystemCreateUser, YP_SystemLastEditTimeUtc, YP_SystemLastEditUser) VALUES ('{6}', '{0}', '{3}', 'AP', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
					globalChargeCodePK, oldOrgPk, newOrgPk, fRT, bAF, pivot1PK, pivot2PK));
			}

			using (var cmd = GetCommandOnMainConnection(sqlBuilder.ToString()))
			{
				cmd.ExecuteNonQuery();
			}

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgPk, newOrgPk, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			AssertNoExceptionThrown(() => testMerger.Save());
		}

		public void TestWhsPutawayLocationCache()
		{
			var connection = Db.Connection; // We're testing tables and views that don't have Schemas in Z
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var oldOrg = factory.NewWithValidTestData<OrgHeader>();
			var newOrg = factory.NewWithValidTestData<OrgHeader>();
			var warehouse = helper.CreateWarehouse("FTP", "A", 2, 2);
			factory.Save();

			var locationSql = "SELECT TOP(1) WL_PK, WL_WA_PutawayArea FROM dbo.WhsLocation;";
			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(locationSql);
			var location = new ZGuid(dynamicCollection[0]["WL_PK"]);
			var area = new ZGuid(dynamicCollection[0]["WL_WA_PutawayArea"]);

			var insertSql = $@"
INSERT INTO
dbo.WhsPutawayLocationCache
(
	WPC_PK,
	WPC_Column,
	WPC_Level,
	WPC_Tray,
	WPC_PutawaySequence,
	WPC_LocationCacheType,
	WPC_WW_Warehouse,
	WPC_WL_Location,
	WPC_WA_Area,
	WPC_OH_Client,
	WPC_SystemCreateUser,
	WPC_SystemLastEditUser,
	WPC_SystemCreateTimeUtc,
	WPC_SystemLastEditTimeUtc
)
VALUES
(
	@Pk,
	1,
	1,
	1,
	1,
	'FIX',
	@WarehousePk,
	@LocationPk,
	@AreaPk,
	@OrgHeaderPk,
	'~BP',
	'~BP',
	GETUTCDATE(),
	GETUTCDATE()
);";
			var oldRowPk = new ZGuid("00000000-0000-0000-0000-000000000001");
			var newRowPk = new ZGuid("00000000-0000-0000-0000-000000000002");
			using (var cmd = connection.Command(insertSql))
			{
				cmd.AddParameter("@Pk", SqlDbType.UniqueIdentifier, oldRowPk.ToGuid());
				cmd.AddParameter("@WarehousePk", SqlDbType.UniqueIdentifier, warehouse.PK.ToGuid());
				cmd.AddParameter("@LocationPk", SqlDbType.UniqueIdentifier, location.ToGuid());
				cmd.AddParameter("@AreaPk", SqlDbType.UniqueIdentifier, area.ToGuid());
				cmd.AddParameter("@OrgHeaderPk", SqlDbType.UniqueIdentifier, oldOrg.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			using (var cmd = connection.Command(insertSql))
			{
				cmd.AddParameter("@Pk", SqlDbType.UniqueIdentifier, newRowPk.ToGuid());
				cmd.AddParameter("@WarehousePk", SqlDbType.UniqueIdentifier, warehouse.PK.ToGuid());
				cmd.AddParameter("@LocationPk", SqlDbType.UniqueIdentifier, location.ToGuid());
				cmd.AddParameter("@AreaPK", SqlDbType.UniqueIdentifier, area.ToGuid());
				cmd.AddParameter("@OrgHeaderPk", SqlDbType.UniqueIdentifier, newOrg.PK.ToGuid());
				cmd.ExecuteNonQuery();
			}

			var mergeHeader = new MergeOrgHeader(factory, oldOrg, newOrg);
			var testMerger = new OrganisationMergerForTest(mergeHeader);
			testMerger.Save();

			var allCountSql = "SELECT COUNT(*) FROM dbo.WhsPutawayLocationCache;";
			AssertEquals("Location cache should only have one row.", 1, connection.ExecuteScalar(allCountSql));
			var countSql = "SELECT COUNT(*) FROM dbo.WhsPutawayLocationCache WHERE WPC_PK = @Pk";
			using (var cmd = connection.Command(countSql))
			{
				cmd.AddParameter("@Pk", SqlDbType.UniqueIdentifier, oldRowPk.ToGuid());
				AssertEquals("Location cache should no longer include row with old Pk.", 0, cmd.ExecuteScalar());
			}

			using (var cmd = connection.Command(countSql))
			{
				cmd.AddParameter("@Pk", SqlDbType.UniqueIdentifier, newRowPk.ToGuid());
				AssertEquals("Location cache should still contain row with new Pk.", 1, cmd.ExecuteScalar());
			}
		}

		public void TestOrgCountryDatas()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "~test~org~1";
			org1.MainAddress.OA_Address1 = "It's boooriiiiing";
			OrgAddress adr1 = org1.Addresses.AddNew();
			adr1.OA_Address1 = "~big~boobs~";
			OrgHeader org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "~test~org~2~abcd";
			org2.MainAddress.OA_Address1 = "55 min to go!";
			OrgAddress adr2 = org2.Addresses.AddNew();
			adr2.OA_Address1 = "~big~boobs~";
			OrgHeader org3 = factory.NewWithValidTestData<OrgHeader>(); // no country data on this guy
			org3.OH_FullName = "~test~org~3~abcd";
			org3.MainAddress.OA_Address1 = "woohoo!";

			OrgCountryData data1 = factory.New<OrgCountryData>();
			data1.OV_OH_OrgHeader = org1.PK;
			data1.OV_RN_NKClientCountryRelation = "AU";
			ZGuid data1PK = data1.PK;
			OrgCountryData data2 = factory.New<OrgCountryData>();
			data2.OV_OH_OrgHeader = org1.PK;
			data2.OV_RN_NKClientCountryRelation = "US";
			data2.OV_EXExportPermissionDetails = "big bro";
			data2.OV_OA_ApprovedLocation = org1.MainAddress.PK; // just copies
			ZGuid data2PK = data2.PK;
			OrgCountryData data3 = factory.New<OrgCountryData>();
			data3.OV_OH_OrgHeader = org1.PK;
			data3.OV_RN_NKClientCountryRelation = "US";
			data3.OV_EXExportPermissionDetails = "is on duty today";
			data3.OV_OA_ApprovedLocation = adr1.PK; // should be merged, i.e. ignored
			ZGuid data3PK = data3.PK;

			OrgCountryData data4 = factory.New<OrgCountryData>();
			data4.OV_OH_OrgHeader = org2.PK;
			data4.OV_RN_NKClientCountryRelation = "AU";
			ZGuid data4PK = data4.PK;
			OrgCountryData data5 = factory.New<OrgCountryData>();
			data5.OV_OH_OrgHeader = org2.PK;
			data5.OV_RN_NKClientCountryRelation = "US";
			data5.OV_EXExportPermissionDetails = "watches you";
			data5.OV_OA_ApprovedLocation = adr2.PK; // should prevent data3 to get through
			ZGuid data5PK = data5.PK;

			factory.Save();

			MergeOrgHeader merge = new MergeOrgHeader(factory, org1, org2);
			foreach (MergeOrgAddress adr in merge.OldOrgAddressesCollection)
			{
				if (adr.OldObject.PK == adr1.PK)
				{
					adr.Action = MergeOrgAddress.ActionMerge;
					adr.NewObjectPK = adr2.PK;
				}
				else
				{
					adr.Action = MergeOrgAddress.ActionAdd;
				}
			}
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(merge);
			testMerger.Save();

			factory = new BusinessObjectFactory();
			OrgCountryData assert1 = factory.Load<OrgCountryData>(data1PK);
			OrgCountryData assert2 = factory.Load<OrgCountryData>(data2PK);
			OrgCountryData assert3 = factory.Load<OrgCountryData>(data3PK);
			OrgCountryData assert4 = factory.Load<OrgCountryData>(data4PK);
			OrgCountryData assert5 = factory.Load<OrgCountryData>(data5PK);

			AssertNull(assert1);
			AssertNotNull(assert2);
			AssertNull(assert3);
			AssertNotNull(assert4);
			AssertNotNull(assert5);

			AssertEquals(org2.PK, assert2.OV_OH_OrgHeader);
			AssertEquals(org2.PK, assert4.OV_OH_OrgHeader);
			AssertEquals(org2.PK, assert5.OV_OH_OrgHeader);
		}

		public void TestRegenPatterns()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "~test~org~1";
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.MainAddress.OA_Address1 = "main address 1";
			org1.MainAddress.OA_City = "qqq";
			OrgAddress adr1 = org1.Addresses.AddNew();
			adr1.OA_Address1 = "123 qwerty";
			adr1.OA_CompanyNameOverride = "xxx";
			OrgBrandOrRelatedName brand1 = org1.BrandsOrRelatedNames.AddNew();
			brand1.P1_RelatedName = "aaa";

			OrgHeader org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "~test~org~1~abcd";
			org2.OH_RL_NKClosestPort = "AUSYD";
			org2.MainAddress.OA_Address1 = "main 2 uuuuu kiev";
			org2.MainAddress.OA_City = "qqq";
			OrgAddress adr2 = org2.Addresses.AddNew();
			adr2.OA_Address1 = "address abcd";
			adr2.OA_CompanyNameOverride = "yyy world";
			OrgBrandOrRelatedName brand2 = org2.BrandsOrRelatedNames.AddNew();
			brand2.P1_RelatedName = "bbb uuu";

			factory.Save();
			AssertPatternMatches(org1.PK, 5);
			AssertPatternMatches(org2.PK, 5);

			MergeOrgHeader merge = new MergeOrgHeader(factory, org2, org1);
			foreach (MergeOrgAddress adr in merge.OldOrgAddressesCollection)
			{
				adr.Action = MergeOrgAddress.ActionAdd;
			}
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(merge);
			testMerger.Save();

			AssertPatternMatches(org1.PK, 14);
		}

		void AssertPatternMatches(ZGuid orgPK, int number)
		{
			AssertEquals(number, new BusinessObjectFactory().Load<OrgPatternMatch>(new ZQuery(OrgPatternMatchSchema.OS_OH, orgPK)).Length);
		}

		public void TestOC_OA_OrgAddress()
		{
			var factory = new BusinessObjectFactory();
			OrgHeader org = factory.New<OrgHeader>();
			org.OH_Code = "cntotherX";
			var cnt = org.Contacts.AddNew();

			OrgHeader orgOld = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "cnttest1";
			orgNew.OH_Code = "cnttest2";
			orgOld.MainAddress.OA_Address1 = "~address to match~";
			orgNew.MainAddress.OA_Address1 = "~address to match~";
			orgOld.MainAddress.OA_Code = "zz1";
			orgNew.MainAddress.OA_Code = "zz2";

			cnt.OC_OA_OrgAddress = orgOld.MainAddress.PK;

			ZGuid oldPK = orgOld.PK;

			factory.Save();

			MergeOrgAddressCollection mergeAddressInfoList = new MergeOrgAddressCollection(factory, orgOld, orgNew);
			mergeAddressInfoList[0].Action = MergeOrgAddress.ActionMerge;
			mergeAddressInfoList[0].NewObjectPK = orgNew.MainAddress.PK;

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, mergeAddressInfoList, new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			AssertNull(factory.Load<OrgHeader>(oldPK));
			AssertEquals(orgNew.MainAddress.PK, factory.Load<OrgContact>(cnt.PK).OC_OA_OrgAddress);
		}

		public void Testvw_AccOrgBalanceTables()
		{
			var connection = Db.Connection; // We're testing tables and views that don't have Schemas in Z

			var factory = new BusinessObjectFactory();
			OrgHeader orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "test1";
			var oldPK = orgOld.PK;

			OrgHeader orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgNew.OH_Code = "test2";
			var newPK = orgNew.PK;

			CreateAccTransactionHeader(factory, oldPK, "1", 500m);
			CreateAccTransactionLine(factory, oldPK, -50m);
			CreateJobCharge(factory, oldPK, 5m);

			CreateAccTransactionHeader(factory, newPK, "2", 200m);
			CreateAccTransactionLine(factory, newPK, -20m);
			CreateJobCharge(factory, newPK, 2m);

			factory.Save();

			AssertEquals("AccTransactionHeader records for Old Org", 1, factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, oldPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("AccTransactionLines records for Old Org", 1, factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, oldPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("JobCharge records for Old Org", 1, factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, oldPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("vw_AccOrgBalance records for Old Org", 1, connection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = '{0}'", oldPK)));

			AssertEquals("AccTransactionHeader records for New Org", 1, factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, newPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("AccTransactionLines records for New Org", 1, factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, newPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("JobCharge records for New Org", 1, factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, newPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("vw_AccOrgBalance records for New Org", 1, connection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = '{0}'", newPK)));

			var mergeAddressInfoList = new MergeOrgAddressCollection(factory, orgOld, orgNew);
			mergeAddressInfoList[0].Action = MergeOrgAddress.ActionMerge;
			mergeAddressInfoList[0].NewObjectPK = orgNew.MainAddress.PK;

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, mergeAddressInfoList, new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNull("Old org should no longer exist", newFactory.Load<OrgHeader>(oldPK));
			AssertNotNull("New org should still exist", newFactory.Load<OrgHeader>(newPK));

			AssertEquals("AccTransactionHeader records for Old Org", 0, newFactory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, oldPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("AccTransactionLines records for Old Org", 0, newFactory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, oldPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("JobCharge records for Old Org", 0, newFactory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, oldPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("vw_AccOrgBalance records for Old Org", 1, connection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = '{0}'", oldPK)));

			AssertEquals("AccTransactionHeader records for New Org", 2, newFactory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, newPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("AccTransactionLines records for New Org", 2, newFactory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, newPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("JobCharge records for New Org", 2, newFactory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_OH, newPK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)).Length);
			AssertEquals("vw_AccOrgBalance records for New Org", 1, connection.ExecuteScalar(string.Format("SELECT COUNT(*) FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = '{0}'", newPK)));
			AssertEquals("vw_AccOrgBalance.Balance for New Org", 700m, connection.ExecuteScalar(string.Format("SELECT TOP 1 BalanceTotal FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = '{0}'", newPK)));
			AssertEquals("vw_AccOrgBalance.Recognized for New Org", 70m, connection.ExecuteScalar(string.Format("SELECT TOP 1 RecognizedTotal FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = '{0}'", newPK)));
			AssertEquals("vw_AccOrgBalance.Unrecognized for New Org", 7m, connection.ExecuteScalar(string.Format("SELECT TOP 1 UnrecognizedTotal FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = '{0}'", newPK)));
		}

		AccTransactionHeader CreateAccTransactionHeader(BusinessObjectFactory factory, ZGuid organisationPK, ZString transactionNum, ZDecimal amount)
		{
			var header = factory.New<AccTransactionHeader>();
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_GC = GlbCompany.CurrentCompany.PK;
			header.AH_GE = GlbDepartment.CurrentDepartment.PK;
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_TransactionNum = transactionNum;
			header.AH_InvoiceDate = ZDateTime.Now;
			header.AH_PostDate = ZDateTime.Now;
			header.AH_OH = organisationPK;
			header.AH_InvoiceAmount = amount;
			header.AH_OutstandingAmount = amount;
			return header;
		}

		AccTransactionLines CreateAccTransactionLine(BusinessObjectFactory factory, ZGuid organisationPK, ZDecimal amount)
		{
			var line = factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GC = GlbCompany.CurrentCompany.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_OH = organisationPK;
			line.AL_LineAmount = amount;
			line.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;
			return line;
		}

		JobCharge CreateJobCharge(BusinessObjectFactory factory, ZGuid organisationPK, ZDecimal amount)
		{
			var jobHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			ObjectFactory.Get<IAccounting>().Registry.CreateWIPOrAccrualWhenNoInvoicesPosted_ForTestOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var chargeCode = factory.NewWithValidTestData<AccChargeCode>();

			var jobCharge = factory.New<JobCharge>();
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_OH_SellAccount = organisationPK;
			jobCharge.JR_LocalSellAmt = amount;
			jobCharge.JR_OSSellAmt = amount;
			return jobCharge;
		}

		public void TestFixAccTransactionHeaders()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AssertAccTran(factory, "acctran1", "acctran2", "111", "222", "", 1, 1, 0);
			AssertAccTran(factory, "acctran3", "acctran4", "333", "333", "", 1, 1, 0);
			AssertAccTran(factory, "acctran5", "acctran6", "444G", "444G", "", 1, 3, 0);
			AssertAccTran(factory, "acctran7", "acctran8", "555", "555", "555Y", 1, 1, 2);
			AssertAccTran(factory, "acctran9", "acctranA", "666", "666", "666Z", 2, 1, 3);
			AssertAccTran(factory, "acctranZ", "acctranX", "'777", "'777", "", 1, 1, 0);
		}

		void AssertAccTran(BusinessObjectFactory factory, string org1, string org2, string num1, string num2, string num3, byte tranCount1, byte tranCount2, byte tranCount3)
		{
			OrgHeader orgOld = factory.NewWithValidTestData<OrgHeader>();
			ZGuid oldPK = orgOld.PK;
			OrgHeader orgNew = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgOther = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = org1;
			orgNew.OH_Code = org2;
			orgOther.OH_Code = "otherorg" + org1[org1.Length - 1];

			OrgHeader otherOrg1 = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader otherOrg2 = factory.NewWithValidTestData<OrgHeader>();
			GlbBranch gb = factory.NewWithValidTestData<GlbBranch>();

			AccTransactionHeader acc1 = CreateAccTran(factory, num1, orgOld.PK, gb.PK, tranCount1);
			AccTransactionHeader acc2 = CreateAccTran(factory, num2, orgNew.PK, gb.PK, tranCount2);
			AccTransactionHeader acc1other = CreateAccTran(factory, num1, otherOrg1.PK, gb.PK, tranCount1);
			AccTransactionHeader acc2other = CreateAccTran(factory, num2, otherOrg2.PK, gb.PK, tranCount2);
			if (!string.IsNullOrEmpty(num3))
			{
				AccTransactionHeader acc3 = CreateAccTran(factory, num3, orgNew.PK, gb.PK, tranCount3);
			}
			factory.Save();

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccTransactionHeader assertAcc1 = newFactory.Load<AccTransactionHeader>(acc1.PK);
			AccTransactionHeader assertAcc2 = newFactory.Load<AccTransactionHeader>(acc2.PK);
			acc1other = newFactory.Load<AccTransactionHeader>(acc1other.PK);
			acc2other = newFactory.Load<AccTransactionHeader>(acc2other.PK);
			AssertNotNull(assertAcc1);
			AssertNotNull(assertAcc2);
			AssertEquals(orgNew.PK, assertAcc1.AH_OH);
			AssertEquals(orgNew.PK, assertAcc2.AH_OH);
			AssertNull(newFactory.Load<OrgHeader>(oldPK));
			AssertEquals("Shouldn't be changed as this org is not merged", tranCount1, acc1other.AH_TransactionCount);
			AssertEquals("Shouldn't be changed as this org is not merged", tranCount2, acc2other.AH_TransactionCount);
		}

		AccTransactionHeader CreateAccTran(BusinessObjectFactory factory, string num, ZGuid fk, ZGuid gb, byte transactionCount)
		{
			AccTransactionHeader result = factory.NewWithValidTestData<AccTransactionHeader>();
			result.AH_Ledger = "AP";
			result.AH_OH = fk;
			result.AH_TransactionCount = transactionCount;
			result.AH_TransactionNum = num;
			result.AH_GB = gb;
			result.AH_TransactionType = "INV";
			return result;
		}

		public void TestFixAccTransactionHeaders_WhenOver255TransactionsWithSameNumber()
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "OLD_ORG";
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgNew.OH_Code = "NEW_ORG";
			var acc1 = CreateAccTran(factory, "APINVWithSameNumber", orgOld.PK, Env.CurrentBranchPK, 1);
			var acc2 = CreateAccTran(factory, "APINVWithSameNumber", orgNew.PK, Env.CurrentBranchPK, 1);
			factory.Save();

			var sqlForOrgHeaders = new StringBuilder("INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ");
			var sqlForTransactionHeaders = new StringBuilder(@"INSERT INTO dbo.AccTransactionHeader
(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_OH, AH_GB, AH_GC, AH_GE, AH_InvoiceAmount, AH_InvoiceDate) VALUES ");
			for (int i = 0; i < 512; i++)
			{
				var ohPk = Guid.NewGuid();
				sqlForOrgHeaders.AppendLine($"('{ohPk}', 'ORGNUM{i}'),");
				sqlForTransactionHeaders.AppendLine(
					 $"(NEWID(), 'AP', 'INV', 'APINVWithSameNumber', 1, '{ohPk}', '{Env.CurrentBranchPK}', '{Env.CurrentCompanyPK}', '{Env.CurrentDepartmentPK}', {i}, SYSDATETIME()),"
					);
			}
			sqlForOrgHeaders.Remove(sqlForOrgHeaders.Length - 3, 3);
			sqlForTransactionHeaders.Remove(sqlForTransactionHeaders.Length - 3, 3);

			Db.Connection.ExecuteNonQuery(sqlForOrgHeaders.ToString());
			Db.Connection.ExecuteNonQuery(sqlForTransactionHeaders.ToString());

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			acc1.Reload();
			acc2.Reload();
			AssertEquals("Precondtion: merge was successful for transaction 1", orgNew.PK, acc1.AH_OH);
			AssertEquals("Precondtion: merge was successful for transaction 2", orgNew.PK, acc2.AH_OH);
			var transactionCounts = new[] { acc1.AH_TransactionCount, acc2.AH_TransactionCount }.OrderBy(x => x);
			var expectedCounts = new ZByte[] { 1, 2 };
			AssertContainsExactElementsInExactOrder("Transaction count should be renumbered for just the transactions affects by the merge", expectedCounts, transactionCounts);
		}

		public void TestMergeSubAccountTypeOrgnisationInTransactionLine()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader orgOld = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "oldOrg";
			orgNew.OH_Code = "newOrg";
			AccTransactionLines line = factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;

			var glHederSubAccount = factory.New<AccGLHeaderSubAccount>();
			glHederSubAccount.ASA_SubClass = OrgHeaderSchema.Constants.Prefix;
			glHederSubAccount.ASA_AG = line.AL_AG;

			var lineSubAccount = factory.New<AccTransactionLineSubAccount>();
			lineSubAccount.AL1_AL = line.PK;
			lineSubAccount.AL1_SubClassParentId = orgOld.PK;
			lineSubAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			factory.Save();

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();

			var newLineSubAccount = factory.LoadTop1<AccTransactionLineSubAccount>(new ZQuery(AccTransactionLineSubAccountSchema.AL1_AL, line.PK));
			AssertEquals(orgNew.PK, newLineSubAccount.AL1_SubClassParentId);
		}

		public void TestMergeSubAccountTypeOrgnisationInTransactionHeader()
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "oldOrg";
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgNew.OH_Code = "newOrg";
			var header = factory.NewWithValidTestData<AccTransactionHeader>();

			var headerSubAccount = factory.New<AccTransactionHeaderSubAccount>();
			headerSubAccount.AHS_AH = header.PK;
			headerSubAccount.AHS_SubClassParentId = orgOld.PK;
			headerSubAccount.AHS_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			factory.Save();

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();

			var newHeaderSubAccount = factory.LoadTop1<AccTransactionHeaderSubAccount>(new ZQuery(AccTransactionHeaderSubAccountSchema.AHS_AH, header.PK));
			AssertEquals(orgNew.PK, newHeaderSubAccount.AHS_SubClassParentId);
		}

		public void TestFixAccTaxReturnLines()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			var org3 = factory.NewWithValidTestData<OrgHeader>();
			var org4 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "org1";
			org2.OH_Code = "org2";
			org3.OH_Code = "org3";
			org4.OH_Code = "org4";
			factory.Save();

			var complianceReportPk = Guid.NewGuid();
			var taxReturnPk = Guid.NewGuid();

			var sqlInsertRecords = @"
INSERT INTO dbo.AccComplianceReport (ACR_PK,ACR_ReportType,ACR_Periodicity,ACR_DateFrom,ACR_DateTo,ACR_GC_Company,ACR_Status,ACR_SystemCreateTimeUtc,ACR_SystemLastEditTimeUtc,ACR_SystemCreateUser,ACR_SystemLastEditUser)
	VALUES (@ReportPk,'TST','PER','2020-05-01','2020-05-31',@CompanyPK,'GEN','2020-06-01','2020-06-01','~BP','~BP')

INSERT INTO dbo.AccTaxReturn (ATR_PK,ATR_ACR_ComplianceReport,ATR_Status,ATR_SystemCreateTimeUtc,ATR_SystemCreateUser,ATR_SystemLastEditTimeUtc,ATR_SystemLastEditUser)
	VALUES (@ReturnPk,@ReportPK,'SAV','2020-06-01','E','2020-06-01','E')

INSERT INTO dbo.AccTaxReturnLine (ARL_PK,ARL_ATR_AccTaxReturn,ARL_OH_Organisation,ARL_OrgMergeCounter,ARL_SystemCreateTimeUtc,ARL_SystemCreateUser,ARL_SystemLastEditTimeUtc,ARL_SystemLastEditUser)
	VALUES (NEWID(),@ReturnPk,@Org1,0,'2020-06-01','E','2020-06-01','E')
INSERT INTO dbo.AccTaxReturnLine (ARL_PK,ARL_ATR_AccTaxReturn,ARL_OH_Organisation,ARL_OrgMergeCounter,ARL_SystemCreateTimeUtc,ARL_SystemCreateUser,ARL_SystemLastEditTimeUtc,ARL_SystemLastEditUser)
	VALUES (NEWID(),@ReturnPk,@Org2,0,'2020-06-01','E','2020-06-01','E')
INSERT INTO dbo.AccTaxReturnLine (ARL_PK,ARL_ATR_AccTaxReturn,ARL_OH_Organisation,ARL_OrgMergeCounter,ARL_SystemCreateTimeUtc,ARL_SystemCreateUser,ARL_SystemLastEditTimeUtc,ARL_SystemLastEditUser)
	VALUES (NEWID(),@ReturnPk,@Org3,0,'2020-06-01','E','2020-06-01','E')
INSERT INTO dbo.AccTaxReturnLine (ARL_PK,ARL_ATR_AccTaxReturn,ARL_OH_Organisation,ARL_OrgMergeCounter,ARL_SystemCreateTimeUtc,ARL_SystemCreateUser,ARL_SystemLastEditTimeUtc,ARL_SystemLastEditUser)
	VALUES (NEWID(),@ReturnPk,@Org4,0,'2020-06-01','E','2020-06-01','E')
";

			using (var cmd = GetCommandOnMainConnection(sqlInsertRecords))
			{
				cmd.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Environment.Env.CurrentCompany.PK);
				cmd.AddParameter("@ReportPk", SqlDbType.UniqueIdentifier, complianceReportPk);
				cmd.AddParameter("@ReturnPk", SqlDbType.UniqueIdentifier, taxReturnPk);
				cmd.AddParameter("@Org1", SqlDbType.UniqueIdentifier, org1.PK.ToGuid());
				cmd.AddParameter("@Org2", SqlDbType.UniqueIdentifier, org2.PK.ToGuid());
				cmd.AddParameter("@Org3", SqlDbType.UniqueIdentifier, org3.PK.ToGuid());
				cmd.AddParameter("@Org4", SqlDbType.UniqueIdentifier, org4.PK.ToGuid());

				cmd.ExecuteNonQuery();
			}

			var testMerger1 = new OrganisationMergerForTest(org1.PK, org2.PK, new MergeOrgAddressCollection(factory, org1, org2), new MergeOrgContactCollection(factory, org1, org2));
			AssertNoExceptionThrown("We should not have any issues fixing AccTaxReturnLine", () => testMerger1.Save());

			var testMerger2 = new OrganisationMergerForTest(org3.PK, org4.PK, new MergeOrgAddressCollection(factory, org3, org4), new MergeOrgContactCollection(factory, org3, org4));
			AssertNoExceptionThrown("We should not have any issues fixing AccTaxReturnLine", () => testMerger2.Save());

			var testMerger3 = new OrganisationMergerForTest(org2.PK, org4.PK, new MergeOrgAddressCollection(factory, org2, org4), new MergeOrgContactCollection(factory, org2, org4));
			AssertNoExceptionThrown("We should not have any issues fixing AccTaxReturnLine", () => testMerger3.Save());

			using (var cmd = GetCommandOnMainConnection("SELECT ARL_OH_Organisation FROM dbo.AccTaxReturnLine WHERE ARL_ATR_AccTaxReturn = @ReturnPk AND ARL_OH_Organisation = @NewOrg"))
			{
				cmd.AddParameter("@ReturnPk", SqlDbType.UniqueIdentifier, taxReturnPk);
				cmd.AddParameter("@NewOrg", SqlDbType.UniqueIdentifier, org4.PK.ToGuid());

				var lineTable = new DataTable(AccTaxReturnLineSchema.Constants.TableName);
				cmd.NewDataAdapter().Fill(lineTable);
				AssertEquals("Both AccTaxReturnLine records are linked to NewOrg", 4, lineTable.Rows.Count);
				lineTable.Rows.Cast<DataRow>().ForEach(x =>
				{
					AssertEquals(org4.PK, x["ARL_OH_Organisation"]);
				});
			}
		}

		#region TestMergeStmNumberMatchingDetails

		public void TestMergeStmNumberMatchingDetails_DifferentPrefix()
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "oldOrg";
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgNew.OH_Code = "newOrg";

			CreateStmMatchingDetails(factory, "AAA", orgOld.PK);
			CreateStmMatchingDetails(factory, "BBB", orgNew.PK);
			factory.Save();

			AssertStmNumsAndDetails(orgOld.PK);
			AssertStmNumsAndDetails(orgNew.PK);

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			AssertNull("Precondition - Old Organisation must be deleted.", new BusinessObjectFactory().Load<OrgHeader>(orgOld.PK));
			AssertStmNumsAndDetails(orgNew.PK);
		}

		public void TestMergeStmNumberMatchingDetails_SamePrefix()
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "oldOrg";
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgNew.OH_Code = "newOrg";

			CreateStmMatchingDetails(factory, "AAA", orgOld.PK);
			CreateStmMatchingDetails(factory, "AAA", orgNew.PK);
			factory.Save();

			AssertStmNumsAndDetails(orgOld.PK);
			AssertStmNumsAndDetails(orgNew.PK);

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			AssertNull("Precondition - Old Organisation must be deleted.", new BusinessObjectFactory().Load<OrgHeader>(orgOld.PK));
			AssertStmNumsAndDetails(orgNew.PK);
		}

		static void AssertStmNumsAndDetails(ZGuid orgPK)
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var org = factory.Load<OrgHeader>(orgPK);
			AssertEquals(1, org.OrgFountains.Count);
			AssertEquals(1, org.NumberRangeMatchingDetails.Count);
		}

		static void CreateStmMatchingDetails(BusinessObjectFactory factory, string prefix, ZGuid orgPK)
		{
			var stmNum = factory.NewWithValidTestData<OrganisationViewStmNums>();
			var type = OrgStmNumsTypeList.Codes.TransportReferenceNumbers;
			stmNum.SN_Type = type;
			stmNum.SN_MinimumValue = 100;
			stmNum.SN_MaximumValue = 200;
			stmNum.SN_Prefix = prefix;
			stmNum.SN_Count = 10;
			stmNum.SN_Owner = orgPK;
			factory.Save();
			var matchingDetails = factory.NewWithValidTestData<StmNumberRangeMatchingDetail>();
			// populate with valid data
			matchingDetails.NRM_RangeType = type;
			matchingDetails.NRM_Prefix = prefix;
			matchingDetails.NRM_OwnerTableCode = OrgHeaderSchema.Constants.Prefix;
			matchingDetails.NRM_OwnerId = orgPK;
		}

		#endregion

		public void TestMergeCusPermitHeader()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "oldOrg";
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "newOrg";
			factory.Save();

			ZGuid cusPermitHeader = ZGuid.NewZGuid();
			string sql = @"
			insert into dbo.CusPermitHeader (CPH_PK, CPH_OH_PermitHolder, CPH_StartDate, CPH_Number, CPH_QtyValIndicator, CPH_Type, CPH_RN_NKCountryCode, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser) values(NEWID(), '" + oldOrgHeader.PK + @"', CURRENT_TIMESTAMP, '5678', 'BTH', 'BBB', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.CusPermitHeader (CPH_PK, CPH_OH_PermitHolder, CPH_StartDate, CPH_Number, CPH_QtyValIndicator, CPH_Type, CPH_RN_NKCountryCode, CPH_SystemCreateTimeUtc, CPH_SystemCreateUser, CPH_SystemLastEditTimeUtc, CPH_SystemLastEditUser) values('" + cusPermitHeader + @"', '" + oldOrgHeader.PK + @"', CURRENT_TIMESTAMP, '1234', 'BTH', 'AAA', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.ExecuteNonQuery();
			}

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertDelete(mergeOrgHeader, oldOrgHeader.PK, ZGuid.NewZGuid(), newOrgHeader.PK, "CusPermitHeader", "CPH_OH_PermitHolder");
		}

		public void TestDeleteOldOrgRefOrgConsortiumPivots()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader orgOld = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "refcons1";
			orgNew.OH_Code = "refcons2";
			RefCarrierConsortium carrier1 = factory.NewWithValidTestData<RefCarrierConsortium>();
			RefCarrierConsortium carrier2 = factory.NewWithValidTestData<RefCarrierConsortium>();
			RefOrgConsortiumPivot pivot = factory.New<RefOrgConsortiumPivot>();
			pivot.RO_OH = orgOld.PK;
			pivot.RO_RG = carrier1.PK;
			factory.Save();

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			RefOrgConsortiumPivot piv1 = factory.LoadTop1<RefOrgConsortiumPivot>(new ZQuery(RefOrgConsortiumPivotSchema.RO_OH, orgOld.PK));
			RefOrgConsortiumPivot piv2 = factory.LoadTop1<RefOrgConsortiumPivot>(new ZQuery(RefOrgConsortiumPivotSchema.RO_OH, orgNew.PK));
			AssertNull("should be moved to new org", piv1);
			AssertNotNull("should be moved from old org", piv2);

			factory = new BusinessObjectFactory();
			orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "refcons3";
			orgNew.OH_Code = "refcons4";
			pivot = factory.New<RefOrgConsortiumPivot>();
			pivot.RO_OH = orgNew.PK;
			pivot.RO_RG = carrier2.PK;
			factory.Save();

			testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			piv1 = factory.LoadTop1<RefOrgConsortiumPivot>(new ZQuery(RefOrgConsortiumPivotSchema.RO_OH, orgOld.PK));
			piv2 = factory.LoadTop1<RefOrgConsortiumPivot>(new ZQuery(RefOrgConsortiumPivotSchema.RO_OH, orgNew.PK));
			AssertNull("should not appear", piv1);
			AssertNotNull("should stay", piv2);

			factory = new BusinessObjectFactory();
			orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "refcons5";
			orgNew.OH_Code = "refcons6";
			pivot = factory.New<RefOrgConsortiumPivot>();
			pivot.RO_OH = orgNew.PK;
			pivot.RO_RG = carrier2.PK;
			RefOrgConsortiumPivot pivot2 = factory.New<RefOrgConsortiumPivot>();
			pivot2.RO_OH = orgOld.PK;
			pivot2.RO_RG = carrier2.PK;
			factory.Save();

			testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			piv1 = factory.LoadTop1<RefOrgConsortiumPivot>(new ZQuery(RefOrgConsortiumPivotSchema.RO_OH, orgOld.PK));
			piv2 = factory.LoadTop1<RefOrgConsortiumPivot>(new ZQuery(RefOrgConsortiumPivotSchema.RO_OH, orgNew.PK));
			AssertNull("should be deleted as new org already has a pivot", piv1);
			AssertNotNull("should stay", piv2);
			AssertEquals("should stay", pivot.PK, piv2.PK);
		}

		public void TestDeleteOldPartRelations_DuplicateOwners_BothToBoth()
			=> AssertDeleteOldPartRelations(RelationshipTypes.Both, RelationshipTypes.Both, AssertNull);

		public void TestDeleteOldPartRelations_DuplicateOwners_OwnerToBoth()
			=> AssertDeleteOldPartRelations(RelationshipTypes.Owner, RelationshipTypes.Both, AssertNull);

		public void TestDeleteOldPartRelations_DuplicateOwners_BothToOwner()
			=> AssertDeleteOldPartRelations(RelationshipTypes.Both, RelationshipTypes.Owner, AssertNull);

		public void TestDeleteOldPartRelations_DuplicateOwners_OwnerToOwner()
			=> AssertDeleteOldPartRelations(RelationshipTypes.Owner, RelationshipTypes.Owner, AssertNull);

		public void TestDeleteOldPartRelations_DuplicateSuppliers_SupplierToBoth()
			=> AssertDeleteOldPartRelations(RelationshipTypes.Supplier, RelationshipTypes.Both, AssertNull);

		public void TestDeleteOldPartRelations_DuplicateSuppliers_BothToSupplier()
			=> AssertDeleteOldPartRelations(RelationshipTypes.Both, RelationshipTypes.Supplier, AssertNull);

		public void TestDeleteOldPartRelations_DuplicateSuppliers_SupplierToSupplier()
			=> AssertDeleteOldPartRelations(RelationshipTypes.Supplier, RelationshipTypes.Supplier, AssertNull);

		public void TestDeleteOldPartRelations_Different()
			=> AssertDeleteOldPartRelations(RelationshipTypes.Supplier, RelationshipTypes.Owner, AssertNotNull);

		public void TestDeleteOldPartRelations_Same()
			=> AssertDeleteOldPartRelations("ABC", "ABC", AssertNull);

		void AssertDeleteOldPartRelations(string oldRelationshipType, string newRelationshipType, Action<object> assertExpectation)
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "~PRL1~";
			orgNew.OH_Code = "~PRL2~";
			var supplierPart = factory.NewWithValidTestData<OrgSupplierPart>();
			factory.Load<OrgHeader>(supplierPart.OP_OH_FormLayoutController).OH_Code = "~PRL3~";
			factory.Save();

			var part1 = factory.New<OrgPartRelation>();
			var part2 = factory.New<OrgPartRelation>();
			part1.OU_OH = orgOld.PK;
			part1.OU_Relationship = oldRelationshipType;
			part1.OU_OP = supplierPart.PK;
			part2.OU_OH = orgNew.PK;
			part2.OU_Relationship = newRelationshipType;
			part2.OU_OP = supplierPart.PK;
			factory.Save();

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			var assertPart1 = factory.Load<OrgPartRelation>(part1.PK);
			var assertPart2 = factory.Load<OrgPartRelation>(part2.PK);
			assertExpectation(assertPart1);
			AssertNotNull("New relationship should not be deleted", assertPart2);
			if (assertPart1 != null)
			{
				AssertEquals("should be moved to new org", orgNew.PK, assertPart1.OU_OH);
			}
		}

		public void TestMergeOrgWebURL()
		{
			//no urls
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader orgOld = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgNew = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = "mergeurl1";
			orgNew.OH_Code = "mergeurl2";
			factory.Save();

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			OrgWebURL[] col1;
			OrgWebURL[] col2;
			factory = new BusinessObjectFactory();
			col1 = factory.Load<OrgWebURL>(new ZQuery(OrgWebURLSchema.PU_OH, orgOld.PK));
			col2 = factory.Load<OrgWebURL>(new ZQuery(OrgWebURLSchema.PU_OH, orgNew.PK));
			AssertEquals("No urls were added", 0, col1.Length);
			AssertEquals("No urls were added", 0, col2.Length);

			//urls in new org
			factory = new BusinessObjectFactory();
			orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgNew = factory.NewWithValidTestData<OrgHeader>();
			OrgWebURL url1 = orgNew.OrgWebURLs.AddNew();
			url1.PU_URL = "www.aaa";
			url1.PU_Type = "xxx";
			url1.PU_IsPrimary = ZBool.True;
			OrgWebURL url2 = orgNew.OrgWebURLs.AddNew();
			url2.PU_URL = "www.bbb";
			url2.PU_Type = "ttt";
			url2.PU_IsPrimary = ZBool.False;
			orgOld.OH_Code = "mergeurl3";
			orgNew.OH_Code = "mergeurl4";
			factory.Save();

			testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			col1 = factory.Load<OrgWebURL>(new ZQuery(OrgWebURLSchema.PU_OH, orgOld.PK));
			col2 = factory.Load<OrgWebURL>(new ZQuery(OrgWebURLSchema.PU_OH, orgNew.PK));

			AssertEquals("No urls were added", 0, col1.Length);
			AssertEquals("No new urls were added, 2 remain", 2, col2.Length);

			//urls in old org - moving to new
			factory = new BusinessObjectFactory();
			orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgNew = factory.NewWithValidTestData<OrgHeader>();
			url1 = orgOld.OrgWebURLs.AddNew();
			url1.PU_URL = "www.aaa";
			url1.PU_Type = "xxx";
			url1.PU_IsPrimary = ZBool.True;
			url2 = orgOld.OrgWebURLs.AddNew();
			url2.PU_URL = "www.bbb";
			url2.PU_Type = "ttt";
			url2.PU_IsPrimary = ZBool.False;
			orgOld.OH_Code = "mergeurl5";
			orgNew.OH_Code = "mergeurl6";
			factory.Save();

			testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			col1 = factory.Load<OrgWebURL>(new ZQuery(OrgWebURLSchema.PU_OH, orgOld.PK));
			col2 = factory.Load<OrgWebURL>(new ZQuery(OrgWebURLSchema.PU_OH, orgNew.PK));

			AssertEquals("No urls left", 0, col1.Length);
			AssertEquals("2 new urls were added", 2, col2.Length);

			//urls in both orgs
			factory = new BusinessObjectFactory();
			orgOld = factory.NewWithValidTestData<OrgHeader>();
			orgNew = factory.NewWithValidTestData<OrgHeader>();
			url1 = orgOld.OrgWebURLs.AddNew();
			url1.PU_URL = "www.aaa";
			url1.PU_Type = "xxx";
			url1.PU_IsPrimary = ZBool.True;
			url2 = orgOld.OrgWebURLs.AddNew();
			url2.PU_URL = "www.bbb";
			url2.PU_Type = "ttt";
			url2.PU_IsPrimary = ZBool.False;
			OrgWebURL url3 = orgOld.OrgWebURLs.AddNew();
			url3.PU_URL = "www.bbb.new";
			url3.PU_Type = "YYY";
			url3.PU_IsPrimary = ZBool.False;
			OrgWebURL url4 = orgOld.OrgWebURLs.AddNew();
			url4.PU_URL = "www.aaa.new";
			url4.PU_Type = "PPP";
			url4.PU_IsPrimary = ZBool.False;

			OrgWebURL url5 = orgNew.OrgWebURLs.AddNew();
			url5.PU_URL = "www.aaa.new";
			url5.PU_Type = "ZZZ";
			url5.PU_IsPrimary = ZBool.True;
			OrgWebURL url6 = orgNew.OrgWebURLs.AddNew();
			url6.PU_URL = "www.bbb.new";
			url6.PU_Type = "UUU";
			url6.PU_IsPrimary = ZBool.False;

			orgOld.OH_Code = "mergeurl7";
			orgNew.OH_Code = "mergeurl8";
			factory.Save();

			testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			col1 = factory.Load<OrgWebURL>(new ZQuery(OrgWebURLSchema.PU_OH, orgOld.PK));
			col2 = factory.Load<OrgWebURL>(new ZQuery(OrgWebURLSchema.PU_OH, orgNew.PK));

			AssertEquals("No urls left", 0, col1.Length);
			AssertEquals("2 new urls were added, 2 remain", 4, col2.Length);
			int count = 0;
			int count2 = 0;
			int count3 = 0;
			OrgWebURL assertUrl = null;
			OrgWebURL assertUrl2 = null;
			OrgWebURL assertUrl3 = null;
			foreach (OrgWebURL url in col2)
			{
				if (url.PU_IsPrimary)
				{
					assertUrl = url;
					count++;
				}
				if (url.PU_URL == "www.bbb.new")
				{
					assertUrl2 = url;
					count2++;
				}
				if (url.PU_URL == "www.aaa.new")
				{
					assertUrl3 = url;
					count3++;
				}
			}

			AssertEquals("There should be only one primary url", 1, count);
			AssertEquals("www.aaa.new", assertUrl.PU_URL);
			AssertEquals("ZZZ", assertUrl.PU_Type);

			AssertEquals("urls with similar text are not merged", 1, count2);
			AssertEquals("www.bbb.new", assertUrl2.PU_URL);
			AssertEquals("urls with similar text are not merged", "UUU", assertUrl2.PU_Type);

			AssertEquals("urls with similar text are not merged", 1, count3);
			AssertEquals("www.aaa.new", assertUrl3.PU_URL);
			AssertEquals("urls with similar text are not merged", "ZZZ", assertUrl3.PU_Type);
		}

		public void TestMergeARAP()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbCompany company1 = factory.NewWithValidTestData<GlbCompany>();
			OrgHeader org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~TESTOLDORG~";
			org1.MainAddress.OA_Code = "~zzz1~";
			org1.OH_IsDebtor = true;
			org1.OH_Code = "~test123~";
			OrgHeader org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "~TESTNEWORG~";
			org2.MainAddress.OA_Code = "~zzz2~";
			OrgCompanyData data1 = org1.CompanyDataCollection.AddNew();
			data1.OB_GC = company1.PK;
			data1.OB_IsDebtor = true;
			factory.Save();

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(org2.PK, org1.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			AssertEquals("MoveARAPRecords shouldn't be run as nothing should be moved for now", 0, testMerger.MoveARAPRecordsCounter);
			AssertEquals("MergeARAPRecords shouldn't be run as nothing should be merged for now", 0, testMerger.MergeARAPRecordsCounter);

			org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org1.OH_Code = "~test789~";
			org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsDebtor = true;
			org2.OH_Code = "~test987~";
			data1 = org1.CompanyDataCollection.AddNew();
			data1.OB_GC = company1.PK;
			data1.OB_IsDebtor = true;
			factory.Save();

			testMerger = new OrganisationMergerForTest(org2.PK, org1.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			AssertEquals("MoveARAPRecords shouldn't be run as nothing should be moved for now", 0, testMerger.MoveARAPRecordsCounter);
			AssertEquals("MergeARAPRecords shouldn't be run as nothing should be merged for now", 0, testMerger.MergeARAPRecordsCounter);

			org1 = factory.NewWithValidTestData<OrgHeader>();
			org2 = factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "~test654~";
			org2.OH_Code = "~test321~";

			List<ZGuid> guids = new List<ZGuid>();
			OrgCompanyData newdata = org1.CompanyDataCollection.AddNew();
			newdata.OB_GC = company1.PK;
			newdata.OB_IsDebtor = true;
			newdata.OB_ARCategory = "XXX";
			newdata.OB_ARCreditRating = "YYY";
			newdata.OB_ARQualityAssured = ZBool.True;
			newdata.OB_ARExternalDebtorCode = "112233";

			guids.Add(newdata.InvoiceTypes.AddNew().PK);
			guids.Add(newdata.InvoiceRollupOrGroups.AddNew().PK);
			OrgWhsChgAttribGrpBy att = newdata.Factory.NewWithValidTestData<OrgWhsChgAttribGrpBy>();
			att.PX_OB = newdata.PK;
			guids.Add(att.PK);

			guids.Add(newdata.AccountDetailsCollection.AddNew().PK);
			OrgCollectionNote note = newdata.Factory.NewWithValidTestData<OrgCollectionNote>();
			note.PN_OB = newdata.PK;
			guids.Add(note.PK);

			var aRTerm = newdata.ARTerms.AddNew();
			guids.Add(aRTerm.PK);

			var originalARTermsCycleCount = factory.Load<OrgARTermsCycle>(new ZQuery()).Length;
			guids.Add(aRTerm.ARTermsCycles.AddNew().PK);
			guids.Add(aRTerm.ARTermsCycles.AddNew().PK);

			var cfxConfig = newdata.AccCFXConfigurations.AddNew();
			cfxConfig.JCF_ServiceDirection = "IMP";
			cfxConfig.JCF_TransportMode = "SEA";
			guids.Add(cfxConfig.PK);

			var aROrgTaxConfig = newdata.Factory.NewWithValidTestData<AccOrgTaxConfiguration>();
			aROrgTaxConfig.OTC_OB = newdata.PK;
			guids.Add(aROrgTaxConfig.PK);

			factory.Save();

			testMerger = new OrganisationMergerForTest(org1.PK, org2.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.DeleteOldOrg = false;
			testMerger.Save();

			AssertNotEquals("MoveARAPRecords should be run", 0, testMerger.MoveARAPRecordsCounter);
			AssertEquals("MergeARAPRecords shouldn't be run as nothing should be merged for now", 0, testMerger.MergeARAPRecordsCounter);

			factory = new BusinessObjectFactory();
			OrgInvoiceType inv = factory.Load<OrgInvoiceType>(guids[0]);
			OrgInvoiceRollupOrGroup rollup = factory.Load<OrgInvoiceRollupOrGroup>(guids[1]);
			att = factory.Load<OrgWhsChgAttribGrpBy>(guids[2]);
			AccAPAccountDetails acc = factory.Load<AccAPAccountDetails>(guids[3]);
			note = factory.Load<OrgCollectionNote>(guids[4]);
			OrgARTerms oldARTerm = factory.Load<OrgARTerms>(guids[5]);
			OrgARTermsCycle oldARTermCycle1 = factory.Load<OrgARTermsCycle>(guids[6]);
			OrgARTermsCycle oldARTermCycle2 = factory.Load<OrgARTermsCycle>(guids[7]);
			var oldCFXConfig = factory.Load<AccCFXUpliftConfiguration>(guids[8]);
			var oldAROrgTaxConfig = factory.Load<AccOrgTaxConfiguration>(guids[9]);

			ZQuery query = new ZQuery(OrgCompanyDataSchema.OB_OH, org2.PK);
			query.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, company1.PK);
			OrgCompanyData[] datas = factory.Load<OrgCompanyData>(query);
			AssertNotEquals(0, datas.Length);
			OrgCompanyData data2 = datas[0];

			AssertNull(inv);
			AssertNull(rollup);
			AssertNull(att);
			AssertNotNull(acc);
			AssertNull(note);
			AssertNull(oldARTerm);
			AssertNull(oldARTermCycle1);
			AssertNull(oldARTermCycle2);
			AssertNull(oldCFXConfig);
			AssertNotNull(oldAROrgTaxConfig);
			AssertNotNull(data2);

			Assert(data2.OB_IsDebtor);
			AssertEquals("XXX", data2.OB_ARCategory);
			AssertEquals("YYY", data2.OB_ARCreditRating);
			AssertEquals("112233", data2.OB_ARExternalDebtorCode);
			Assert(data2.OB_ARQualityAssured);

			var orgInvoiceTypes = factory.Load<OrgInvoiceType>(new ZQuery(OrgInvoiceTypeSchema.PI_OB, data2.PK));
			AssertNotEquals(0, orgInvoiceTypes.Length);
			Assert(orgInvoiceTypes.All(o => o.PI_SystemCreateUser == "E" && o.PI_SystemLastEditUser == "E" && o.PI_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1) && o.PI_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1)));
			AssertNotEquals(0, factory.Load<OrgInvoiceRollupOrGroup>(new ZQuery(OrgInvoiceRollupOrGroupSchema.PG_OB, data2.PK)).Length);
			var orgWhsChgAttribs = factory.Load<OrgWhsChgAttribGrpBy>(new ZQuery(OrgWhsChgAttribGrpBySchema.PX_OB, data2.PK));
			AssertNotEquals(0, orgWhsChgAttribs.Length);
			Assert(orgWhsChgAttribs.All(o => o.PX_SystemCreateUser == "E" && o.PX_SystemLastEditUser == "E" && o.PX_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1) && o.PX_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1)));
			AssertEquals(0, factory.Load<AccAPAccountDetails>(new ZQuery(AccAPAccountDetailsSchema.A1_OB, data2.PK)).Length);
			var orgNotes = factory.Load<OrgCollectionNote>(new ZQuery(OrgCollectionNoteSchema.PN_OB, data2.PK));
			Assert(orgNotes.All(o => o.PN_SystemCreateUser == "E" && o.PN_SystemLastEditUser == "E" && o.PN_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1) && o.PN_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1)));
			AssertNotEquals(0, orgNotes.Length);
			var arTerms = factory.Load<OrgARTerms>(new ZQuery(OrgARTermsSchema.PY_OB, data2.PK));
			AssertEquals(2, arTerms.Length);
			Assert(arTerms.All(o => o.PY_SystemCreateUser == "E" && o.PY_SystemLastEditUser == "E" && o.PY_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1) && o.PY_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1)));
			var arTermsCycles = factory.Load<OrgARTermsCycle>(new ZQuery());
			AssertEquals(originalARTermsCycleCount + 2, arTermsCycles.Length);
			Assert(arTermsCycles.All(o => o.P5_SystemCreateUser == "E" && o.P5_SystemLastEditUser == "E" && o.P5_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1) && o.P5_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1)));
			AssertEquals(1, factory.Load<AccCFXUpliftConfiguration>(new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_ParentID, data2.OB_OH)).Length);
			AssertEquals(1, factory.Load<AccOrgTaxConfiguration>(new ZQuery(AccOrgTaxConfigurationSchema.OTC_OB, data2.PK)).Length);

			BusinessObjectFactory newfactory = new BusinessObjectFactory();

			OrgHeader oldOrg = newfactory.NewWithValidTestData<OrgHeader>();
			OrgHeader newOrg = newfactory.NewWithValidTestData<OrgHeader>();
			oldOrg.OH_Code = "~other123~";
			newOrg.OH_Code = "~other456~";
			oldOrg.CompanyData.OB_ARConsolidatedAccountingCategory = "QQQ";
			newOrg.CompanyData.OB_ARConsolidatedAccountingCategory = "ZZZ";
			oldOrg.OH_IsDebtor = true;

			oldOrg.CompanyData.OB_ARCategory = "XXX";
			oldOrg.CompanyData.OB_ARCreditRating = "YYY";
			oldOrg.CompanyData.OB_ARQualityAssured = ZBool.True;
			oldOrg.CompanyData.OB_ARTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;

			oldOrg.CompanyData.InvoiceTypes.AddNew();
			oldOrg.CompanyData.InvoiceRollupOrGroups.AddNew();
			att = oldOrg.CompanyData.Factory.New<OrgWhsChgAttribGrpBy>();
			att.PX_OB = oldOrg.CompanyData.PK;

			oldOrg.CompanyData.AccountDetailsCollection.AddNew();
			note = oldOrg.CompanyData.Factory.New<OrgCollectionNote>();
			note.PN_OB = oldOrg.CompanyData.PK;

			var aROrgTaxConfig2 = newfactory.Load<AccOrgTaxConfiguration>(new ZQuery(AccOrgTaxConfigurationSchema.OTC_OB, data2.PK)).First();
			aROrgTaxConfig2.OTC_OB = oldOrg.CompanyData.PK;

			newOrg.OH_IsDebtor = false;

			oldOrg.OH_Code = "blabla1";
			newOrg.OH_Code = "blabla2";
			newfactory.Save();

			testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.DeleteOldOrg = false;
			testMerger.Save();

			AssertEquals("MoveARAPRecords shouldn't be run as nothing should be moved for now", 0, testMerger.MoveARAPRecordsCounter);
			AssertNotEquals("MergeARAPRecords should be run", 0, testMerger.MergeARAPRecordsCounter);

			newfactory = new BusinessObjectFactory();

			newOrg = factory.Load<OrgHeader>(newOrg.PK);
			Assert(newOrg.CompanyData.OB_IsDebtor);
			AssertEquals("XXX", newOrg.CompanyData.OB_ARCategory);
			AssertEquals("YYY", newOrg.CompanyData.OB_ARCreditRating);
			Assert(newOrg.CompanyData.OB_ARQualityAssured);
			AssertEquals("ZZZ", newOrg.CompanyData.OB_ARConsolidatedAccountingCategory);
			AssertEquals("ALL", newOrg.CompanyData.OB_ARTransactionCreationRestriction);

			AssertNotEquals(0, newfactory.Load<OrgInvoiceType>(new ZQuery(OrgInvoiceTypeSchema.PI_OB, newOrg.CompanyData.PK)).Length);
			AssertNotEquals(1, newfactory.Load<OrgInvoiceRollupOrGroup>(new ZQuery(OrgInvoiceRollupOrGroupSchema.PG_OB, newOrg.CompanyData.PK)).Length);
			AssertNotEquals(0, newfactory.Load<OrgWhsChgAttribGrpBy>(new ZQuery(OrgWhsChgAttribGrpBySchema.PX_OB, newOrg.CompanyData.PK)).Length);
			AssertEquals(0, newfactory.Load<AccAPAccountDetails>(new ZQuery(AccAPAccountDetailsSchema.A1_OB, newOrg.CompanyData.PK)).Length);
			AssertNotEquals(0, newfactory.Load<OrgCollectionNote>(new ZQuery(OrgCollectionNoteSchema.PN_OB, newOrg.CompanyData.PK)).Length);
			AssertNotEquals(0, newfactory.Load<AccOrgTaxConfiguration>(new ZQuery(AccOrgTaxConfigurationSchema.OTC_OB, newOrg.CompanyData.PK)).Length);

			AssertNull(newfactory.Load<OrgInvoiceType>(oldOrg.CompanyData.InvoiceTypes[0].PK));
			AssertNull(newfactory.Load<OrgInvoiceRollupOrGroup>(oldOrg.CompanyData.InvoiceRollupOrGroups[0].PK));
			AssertNull(newfactory.Load<OrgWhsChgAttribGrpBy>(att.PK));
			AssertNotNull(newfactory.Load<AccAPAccountDetails>(oldOrg.CompanyData.AccountDetailsCollection[0].PK));
			AssertNull(newfactory.Load<OrgCollectionNote>(note.PK));
			AssertEquals(0, factory.Load<AccCFXUpliftConfiguration>(new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_ParentID, oldOrg.PK)).Length);
			AssertEquals(0, newfactory.Load<AccOrgTaxConfiguration>(new ZQuery(AccOrgTaxConfigurationSchema.OTC_OB, oldOrg.CompanyData.PK)).Length);

			newfactory = new BusinessObjectFactory();
			var aROrgTaxConfig3 = newfactory.Load<AccOrgTaxConfiguration>(new ZQuery(AccOrgTaxConfigurationSchema.OTC_OB, newOrg.CompanyData.PK)).First();

			oldOrg = newfactory.NewWithValidTestData<OrgHeader>();
			newOrg = newfactory.NewWithValidTestData<OrgHeader>();
			oldOrg.OH_Code = "~other789~";
			newOrg.OH_Code = "~other987~";
			oldOrg.OH_IsCreditor = true;

			oldOrg.CompanyData.OB_APCategory = "XXX";
			oldOrg.CompanyData.OB_APCreditLimit = 100;
			oldOrg.CompanyData.OB_APPaymentTerms = "ZZZ";
			oldOrg.CompanyData.OB_ARConsolidatedAccountingCategory = "QQQ";
			oldOrg.CompanyData.OB_APExternalCreditorCode = "445566";
			oldOrg.CompanyData.OB_APTransactionCreationRestriction = Enterprise.Core.Constants.TransactionCreationRestriction.All;

			oldOrg.CompanyData.InvoiceTypes.AddNew();
			oldOrg.CompanyData.InvoiceRollupOrGroups.AddNew();
			att = oldOrg.CompanyData.Factory.New<OrgWhsChgAttribGrpBy>();
			att.PX_OB = oldOrg.CompanyData.PK;

			oldOrg.CompanyData.AccountDetailsCollection.AddNew();
			note = oldOrg.CompanyData.Factory.New<OrgCollectionNote>();
			note.PN_OB = oldOrg.CompanyData.PK;

			aROrgTaxConfig3.OTC_OB = oldOrg.CompanyData.PK;

			newOrg.OH_IsCreditor = false;

			oldOrg.OH_Code = "blabla3";
			newOrg.OH_Code = "blabla4";
			newfactory.Save();

			testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			AssertEquals("MoveARAPRecords shouldn't be run as nothing should be moved for now", 0, testMerger.MoveARAPRecordsCounter);
			AssertNotEquals("MergeARAPRecords should be run", 0, testMerger.MergeARAPRecordsCounter);

			newfactory = new BusinessObjectFactory();

			newOrg = factory.Load<OrgHeader>(newOrg.PK);
			Assert(newOrg.CompanyData.OB_IsCreditor);
			AssertEquals("XXX", newOrg.CompanyData.OB_APCategory);
			AssertEquals((decimal)100, newOrg.CompanyData.OB_APCreditLimit);
			AssertEquals("ZZZ", newOrg.CompanyData.OB_APPaymentTerms);
			AssertEquals("QQQ", newOrg.CompanyData.OB_ARConsolidatedAccountingCategory);
			AssertEquals("445566", newOrg.CompanyData.OB_APExternalCreditorCode);
			AssertEquals("ALL", newOrg.CompanyData.OB_APTransactionCreationRestriction);
			Assert(newOrg.CompanyData.OB_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1));
			Assert(newOrg.CompanyData.OB_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1));
			AssertEquals("E", newOrg.CompanyData.OB_SystemCreateUser);
			AssertEquals("E", newOrg.CompanyData.OB_SystemLastEditUser);

			AssertEquals(0, newfactory.Load<OrgInvoiceType>(new ZQuery(OrgInvoiceTypeSchema.PI_OB, newOrg.CompanyData.PK)).Length);
			var orgInvoiceRollups = newfactory.Load<OrgInvoiceRollupOrGroup>(new ZQuery(OrgInvoiceRollupOrGroupSchema.PG_OB, newOrg.CompanyData.PK));
			AssertEquals(2, orgInvoiceRollups.Length);
			Assert(orgInvoiceRollups.All(o => o.PG_SystemCreateUser == "E" && o.PG_SystemLastEditUser == "E" && o.PG_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1) && o.PG_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1)));
			AssertEquals(0, newfactory.Load<OrgWhsChgAttribGrpBy>(new ZQuery(OrgWhsChgAttribGrpBySchema.PX_OB, newOrg.CompanyData.PK)).Length);
			var apAccountDetails = newfactory.Load<AccAPAccountDetails>(new ZQuery(AccAPAccountDetailsSchema.A1_OB, newOrg.CompanyData.PK));
			AssertNotEquals(0, apAccountDetails.Length);
			Assert(apAccountDetails.All(o => o.A1_SystemCreateUser == "E" && o.A1_SystemLastEditUser == "E" && o.A1_SystemCreateTimeUtc >= ZDateTime.UtcNow.AddDays(-1) && o.A1_SystemLastEditTimeUtc >= ZDateTime.UtcNow.AddDays(-1)));
			AssertNotEquals(0, newfactory.Load<OrgCollectionNote>(new ZQuery(OrgCollectionNoteSchema.PN_OB, newOrg.CompanyData.PK)).Length);
			AssertEquals(0, factory.Load<AccCFXUpliftConfiguration>(new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_ParentID, newOrg.PK)).Length);
			AssertEquals(0, newfactory.Load<AccOrgTaxConfiguration>(new ZQuery(AccOrgTaxConfigurationSchema.OTC_OB, newOrg.CompanyData.PK)).Length);

			newfactory = new BusinessObjectFactory();

			GlbCompany comp = newfactory.NewWithValidTestData<GlbCompany>();
			comp.GC_Code = "999";
			oldOrg = newfactory.NewWithValidTestData<OrgHeader>();
			newOrg = newfactory.NewWithValidTestData<OrgHeader>();
			newOrg.CompanyData.OB_GC = comp.PK;

			oldOrg.OH_Code = "blabla5";
			newOrg.OH_Code = "blabla6";
			newfactory.Save();

			testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.Save();

			Assert("No exception happened", true);
		}

		public void TestMergeARAPDoesNotUpdateAccJobConfigOfUnrelatedOrg()
		{
			var factory = new BusinessObjectFactory();
			var newCompany = factory.NewWithValidTestData<GlbCompany>();

			var oldOrg = factory.NewWithValidTestData<OrgHeader>();
			oldOrg.OH_IsDebtor = true;
			oldOrg.OH_Code = "~test123~";

			var oldCompanydata = oldOrg.CompanyDataCollection.AddNew();
			oldCompanydata.OB_GC = newCompany.PK;
			oldCompanydata.OB_IsDebtor = true;
			oldCompanydata.OB_ARCategory = "XXX";

			var oldCFXConfig = oldCompanydata.AccCFXConfigurations.AddNew();
			oldCFXConfig.JCF_ServiceDirection = "ALL";
			oldCFXConfig.JCF_TransportMode = "ALL";
			oldCFXConfig.JCF_JobType = "ALL";
			oldCFXConfig.JCF_RX_NKCurrency = "AUD";
			oldCFXConfig.JCF_CFXPercentage = 2M;

			var newOrg = factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_IsDebtor = true;
			newOrg.OH_Code = "~test456~";

			var newCompanydata = newOrg.CompanyDataCollection.AddNew();
			newCompanydata.OB_GC = newCompany.PK;
			newCompanydata.OB_IsDebtor = false;
			newCompanydata.OB_ARCategory = "XXX";

			var newCFXConfig = newCompanydata.AccCFXConfigurations.AddNew();
			newCFXConfig.JCF_ServiceDirection = "ALL";
			newCFXConfig.JCF_TransportMode = "ALL";
			newCFXConfig.JCF_JobType = "ALL";
			newCFXConfig.JCF_RX_NKCurrency = "AUD";
			newCFXConfig.JCF_CFXPercentage = 3M;

			var anotherOrg = factory.NewWithValidTestData<OrgHeader>();
			anotherOrg.OH_IsDebtor = true;
			anotherOrg.OH_Code = "~test789~";

			var anotherCompanydata = anotherOrg.CompanyDataCollection.AddNew();
			anotherCompanydata.OB_GC = newCompany.PK;
			anotherCompanydata.OB_IsDebtor = false;
			anotherCompanydata.OB_ARCategory = "XXX";

			var anotherCFXConfig = anotherCompanydata.AccCFXConfigurations.AddNew();
			anotherCFXConfig.JCF_ServiceDirection = "ALL";
			anotherCFXConfig.JCF_TransportMode = "ALL";
			anotherCFXConfig.JCF_JobType = "ALL";
			anotherCFXConfig.JCF_RX_NKCurrency = "AUD";
			anotherCFXConfig.JCF_CFXPercentage = 5M;

			factory.Save();

			var testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			AssertNoExceptionThrown(testMerger.Save);

			AssertEquals("MoveARAPRecords shouldn't be run as nothing should be moved for now", 0, testMerger.MoveARAPRecordsCounter);
			AssertNotEquals("MergeARAPRecords should be run", 0, testMerger.MergeARAPRecordsCounter);

			var newFactory = new BusinessObjectFactory();
			newCFXConfig = newFactory.Load<AccCFXUpliftConfiguration>(newCFXConfig.PK);
			AssertNotNull(newCFXConfig);
			AssertEquals("job config which is from new organisation should be merged", 2M, newCFXConfig.JCF_CFXPercentage);

			anotherCFXConfig = newFactory.Load<AccCFXUpliftConfiguration>(anotherCFXConfig.PK);
			AssertNotNull(anotherCFXConfig);
			AssertEquals("job config which is not from new organisation should not be merged", 5M, anotherCFXConfig.JCF_CFXPercentage);
		}

		public void TestMergeARAP_ARTaxTemplateIsSetOnNewOrgFromOldOrg()
		{
			var factory = new BusinessObjectFactory();
			var accountingTestObjectCreator = new AccountingTestObjectCreator(factory);
			var (oldOrg, newOrg) = GetOrgHeaders(accountingTestObjectCreator, "org1", "org2", false, true, false, false);
			factory.Save();
			var testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			testMerger.Save();
			newOrg = (new BusinessObjectFactory()).Load<OrgHeader>(newOrg.PK);
			AssertNull(newOrg.CompanyData.ARTaxTemplate);

			(oldOrg, newOrg) = GetOrgHeaders(accountingTestObjectCreator, "org3", "org4", false, true, false, false);
			var accOrgTaxConfigurationTemplateAR = accountingTestObjectCreator.CreateAccOrgTaxConfigurationTemplate("templateAR", true, null);
			oldOrg.CompanyData.OB_OCT_ARTaxTemplate = accOrgTaxConfigurationTemplateAR.PK;
			factory.Save();
			testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			testMerger.Save();
			newOrg = (new BusinessObjectFactory()).Load<OrgHeader>(newOrg.PK);
			AssertNotNull(newOrg.CompanyData.ARTaxTemplate);
			AssertEquals(accOrgTaxConfigurationTemplateAR.OCT_Code, newOrg.CompanyData.ARTaxTemplate.OCT_Code);
		}

		public void TestMergeARAP_APTaxTemplateIsSetOnNewOrgFromOldOrg()
		{
			var factory = new BusinessObjectFactory();
			var accountingTestObjectCreator = new AccountingTestObjectCreator(factory);
			var (oldOrg, newOrg) = GetOrgHeaders(accountingTestObjectCreator, "org1", "org2", true, false, false, false);
			factory.Save();
			var testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			testMerger.Save();
			newOrg = (new BusinessObjectFactory()).Load<OrgHeader>(newOrg.PK);
			AssertNull(newOrg.CompanyData.APTaxTemplate);

			(oldOrg, newOrg) = GetOrgHeaders(accountingTestObjectCreator, "org3", "org4", true, false, false, false);
			var accOrgTaxConfigurationTemplateAP = accountingTestObjectCreator.CreateAccOrgTaxConfigurationTemplate("templateAP", false, null);
			oldOrg.CompanyData.OB_OCT_APTaxTemplate = accOrgTaxConfigurationTemplateAP.PK;
			factory.Save();
			testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			testMerger.Save();
			newOrg = (new BusinessObjectFactory()).Load<OrgHeader>(newOrg.PK);
			AssertNotNull(newOrg.CompanyData.APTaxTemplate);
			AssertEquals(accOrgTaxConfigurationTemplateAP.OCT_Code, newOrg.CompanyData.APTaxTemplate.OCT_Code);
		}

		public void TestMergeARAP_ARTaxTemplateIsNotSetOnNewOrgFromOldOrg_WhenNewOrgAlreadyHasTemplate()
		{
			var factory = new BusinessObjectFactory();
			var accountingTestObjectCreator = new AccountingTestObjectCreator(factory);
			var accOrgTaxConfigurationTemplate = accountingTestObjectCreator.CreateAccOrgTaxConfigurationTemplate("ARTaxTemplate", true, null);
			var (oldOrg, newOrg) = GetOrgHeaders(accountingTestObjectCreator, "org1", "org2", false, true, false, false);
			oldOrg.CompanyData.OB_OCT_ARTaxTemplate = accOrgTaxConfigurationTemplate.PK;
			factory.Save();
			var testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			testMerger.Save();
			newOrg = (new BusinessObjectFactory()).Load<OrgHeader>(newOrg.PK);
			AssertNotNull(newOrg.CompanyData.ARTaxTemplate);
			AssertEquals("Template set from old org", "ARTaxTemplate", newOrg.CompanyData.ARTaxTemplate.OCT_Code);

			var accOrgTaxConfigurationTemplate2 = accountingTestObjectCreator.CreateAccOrgTaxConfigurationTemplate("ARTaxTemplate2", true, null);
			var (oldOrg2, newOrg2) = GetOrgHeaders(accountingTestObjectCreator, "org3", "org4", false, true, false, true);
			oldOrg2.CompanyData.OB_OCT_ARTaxTemplate = accOrgTaxConfigurationTemplate.PK;
			newOrg2.CompanyData.OB_OCT_ARTaxTemplate = accOrgTaxConfigurationTemplate2.PK;
			factory.Save();
			testMerger = new OrganisationMergerForTest(oldOrg2.PK, newOrg2.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			testMerger.Save();
			newOrg2 = (new BusinessObjectFactory()).Load<OrgHeader>(newOrg2.PK);
			AssertEquals("Template not set from old org", "ARTaxTemplate2", newOrg2.CompanyData.ARTaxTemplate.OCT_Code);
		}

		public void TestMergeARAP_APTaxTemplateIsNotSetOnNewOrgFromOldOrg_WhenNewOrgAlreadyHasTemplate()
		{
			var factory = new BusinessObjectFactory();
			var accountingTestObjectCreator = new AccountingTestObjectCreator(factory);
			var accOrgTaxConfigurationTemplate = accountingTestObjectCreator.CreateAccOrgTaxConfigurationTemplate("APTaxTemplate", false, null);
			var (oldOrg, newOrg) = GetOrgHeaders(accountingTestObjectCreator, "org1", "org2", true, false, false, false);
			oldOrg.CompanyData.OB_OCT_APTaxTemplate = accOrgTaxConfigurationTemplate.PK;
			factory.Save();
			var testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			testMerger.Save();
			newOrg = (new BusinessObjectFactory()).Load<OrgHeader>(newOrg.PK);
			AssertNotNull(newOrg.CompanyData.APTaxTemplate);
			AssertEquals("Template set from old org", "APTaxTemplate", newOrg.CompanyData.APTaxTemplate.OCT_Code);

			var accOrgTaxConfigurationTemplate2 = accountingTestObjectCreator.CreateAccOrgTaxConfigurationTemplate("APTaxTemplate2", false, null);
			var (oldOrg2, newOrg2) = GetOrgHeaders(accountingTestObjectCreator, "org3", "org4", true, false, true, false);
			oldOrg2.CompanyData.OB_OCT_APTaxTemplate = accOrgTaxConfigurationTemplate.PK;
			newOrg2.CompanyData.OB_OCT_APTaxTemplate = accOrgTaxConfigurationTemplate2.PK;
			factory.Save();
			testMerger = new OrganisationMergerForTest(oldOrg2.PK, newOrg2.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory));
			testMerger.Save();
			newOrg2 = (new BusinessObjectFactory()).Load<OrgHeader>(newOrg2.PK);
			AssertEquals("Template not set from old org", "APTaxTemplate2", newOrg2.CompanyData.APTaxTemplate.OCT_Code);
		}

		(OrgHeader, OrgHeader) GetOrgHeaders(AccountingTestObjectCreator accountingTestObjectCreator, string org1Code, string org2Code, bool org1IsCreditor, bool org1IsDebtor, bool org2IsCreditor, bool org2IsDebtor)
		{
			var oldOrg = accountingTestObjectCreator.CreateOrgHeader(org1Code, org1IsCreditor, org1IsDebtor);
			var newOrg = accountingTestObjectCreator.CreateOrgHeader(org2Code, org2IsCreditor, org2IsDebtor);

			return (oldOrg, newOrg);
		}

		public void TestSave()
		{
			// Get Organisations to test (OLD and NEW)
			string sqlText = "SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'EDICUS'";
			Guid oldOrgPk;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldOrgPk = (Guid)cmd.ExecuteScalar();
			}
			sqlText = "SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'DEMORG'";
			Guid newOrgPk;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newOrgPk = (Guid)cmd.ExecuteScalar();
			}

			// Update some contacts to point to the OLD Organisation
			sqlText = string.Format(
				"UPDATE dbo.OrgContact SET OC_OH = '{0}' WHERE OC_OH = '{1}'",
				oldOrgPk.ToString(), "C3F842EF-3BE5-448C-BED3-0017B232C624");
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.ExecuteNonQuery();
			}

			// Insert test OrgCusCode records - 2 linked to OldOrg and 1 linked to NewOrg conflicting with one of OldOrg
			sqlText = string.Format(@"
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode01o', '~01', '{2}', '{0}')
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode02o', '~02', '{2}', '{0}')
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode01n', '~01', '{2}', '{1}')",
				oldOrgPk.ToString(), newOrgPk.ToString(), GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.ExecuteNonQuery();
			}

			// Asserts OldOrg has ADDRESS and CONTACT records - BEFORE SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgAddress WHERE OA_OH = '{0}'", oldOrgPk.ToString());
			int oldAddressCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldAddressCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				Assert("[Pre-condition] OldOrg Address Count", oldAddressCountBefore > 0);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.OrgContact WHERE OC_OH = '{0}'", oldOrgPk.ToString());
			int oldContactCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldContactCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				Assert("[Pre-condition] OldOrg Contact Count", oldContactCountBefore > 0);
			}

			// Asserts there are Companies and Branches which proxy organisation is OldOrg - BEFORE SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbCompany WHERE GC_OH_OrgProxy = '{0}'", oldOrgPk.ToString());
			int oldCompanyCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldCompanyCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				Assert("[Pre-condition] Company Count", oldCompanyCountBefore > 0);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.GlbBranch WHERE GB_OH_OrgProxy = '{0}'", oldOrgPk.ToString());
			int oldBranchCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldBranchCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				Assert("[Pre-condition] Branch Count", oldBranchCountBefore > 0);
			}

			// Assert OldOrg has the newly inserted CusCodes - BEFORE SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgCusCode WHERE OK_OH = '{0}'", oldOrgPk.ToString());
			int oldCusCodeCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldCusCodeCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("[PRE-CONDITION] OldOrg CusCode Count", 2, oldCusCodeCountBefore);
			}

			// Asserts OldOrg is active - BEFORE SAVE
			sqlText = string.Format("SELECT OH_IsActive FROM dbo.OrgHeader WHERE OH_PK = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				var oldOrgIsActiveBefore = cmd.ExecuteScalar();
				AssertEquals("[Pre-condition] IsActive", true, oldOrgIsActiveBefore);
			}

			// Gets the number of Companies and Branches which proxy organisation is NewOrg - BEFORE SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbCompany WHERE GC_OH_OrgProxy = '{0}'", newOrgPk.ToString());
			int newCompanyCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newCompanyCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
			}
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbBranch WHERE GB_OH_OrgProxy = '{0}'", newOrgPk.ToString());
			int newBranchCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newBranchCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
			}

			// Assert NewOrg has the newly inserted CusCode - BEFORE SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgCusCode WHERE OK_OH = '{0}'", newOrgPk.ToString());
			int newCusCodeCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newCusCodeCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("[PRE-CONDITION] NewOrg CusCode Count", 1, newCusCodeCountBefore);
			}

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgPk, newOrgPk, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.DeleteOldOrg = false;
			testMerger.Save();

			// Asserts OldOrg still has the same number of ADDRESS and CONTACT records - AFTER SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgAddress WHERE OA_OH = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int oldAddressCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg Address Count - AFTER DELETING", oldAddressCountBefore, oldAddressCountAfter);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.OrgContact WHERE OC_OH = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int oldContactCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg Contact Count - AFTER DELETING", oldContactCountBefore, oldContactCountAfter);
			}

			// Asserts there are NO Companies or Branches which proxy organisation is OldOrg - AFTER SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbCompany WHERE GC_OH_OrgProxy = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int oldCompanyCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg Company Count - AFTER MOVING", 0, oldCompanyCountAfter);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.GlbBranch WHERE GB_OH_OrgProxy = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int oldBranchCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg Branch Count - AFTER MOVING", 0, oldBranchCountAfter);
			}

			// Assert OldOrg has only 1 CusCode. The one that conflicted with a NewOrg CusCode - AFTER SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgCusCode WHERE OK_OH = '{0}'", oldOrgPk.ToString());
			int oldCusCodeCountAfter;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldCusCodeCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg CusCode Count", 1, oldCusCodeCountAfter);
			}

			// Asserts number of Companies and Branches which proxy organisation is NewOrg - AFTER SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbCompany WHERE GC_OH_OrgProxy = '{0}'", newOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int newCompanyCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("NewOrg Company Count - AFTER MOVING", oldCompanyCountBefore + newCompanyCountBefore, newCompanyCountAfter);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.GlbBranch WHERE GB_OH_OrgProxy = '{0}'", newOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int newBranchCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("NewOrg Branch Count - AFTER MOVING", oldBranchCountBefore + newBranchCountBefore, newBranchCountAfter);
			}

			// Assert number of CusCodes which reference NewOrg - AFTER SAVE
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgCusCode WHERE OK_OH = '{0}'", newOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int newCusCodeCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg CusCode Count", (oldCusCodeCountBefore - oldCusCodeCountAfter) + newCusCodeCountBefore, newCusCodeCountAfter);
			}

			// Asserts OldOrg is NOT active - AFTER SAVE
			sqlText = string.Format("SELECT OH_IsActive FROM dbo.OrgHeader WHERE OH_PK = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				var oldOrgIsActiveAfter = cmd.ExecuteScalar();
				AssertEquals("IsActive - AFTER SETTING STATUS", false, oldOrgIsActiveAfter);
			}
		}

		public void TestSaveMergesOrgAddresses()
		{
			// -----------------
			// PREPARE TEST DATA
			// -----------------

			BusinessObjectFactory factory = new BusinessObjectFactory();

			// Create Old Organisation and Addresses
			OrgHeader oldOrgHeader = factory.New<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			OrgAddress oldAddress1 = oldOrgHeader.MainAddress;
			oldAddress1.OA_Address1 = "~TestOldAddr1~";
			oldAddress1.OA_Code = "ABC";
			OrgAddress oldAddress2 = oldOrgHeader.Addresses.AddNew();
			oldAddress2.OA_Address1 = "~TestOldAddr2~";
			oldAddress2.OA_Code = "EDF";

			// Create New Organisation and Address
			OrgHeader newOrgHeader = factory.New<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			OrgAddress newAddress1 = newOrgHeader.MainAddress;
			newAddress1.OA_Address1 = "~TestNewAddr1~";
			newAddress1.OA_Code = "ABC";

			// Create New Organisation and Address
			OrgHeader oldOrgHeader2 = factory.New<OrgHeader>();
			oldOrgHeader2.OH_Code = "~TESTOLDOR2~";
			OrgAddress oldAddress3 = oldOrgHeader2.MainAddress;
			oldAddress3.OA_Address1 = "~TestOldAddr3~";
			oldAddress3.OA_Code = "EDF";

			factory.Save();

			// Create Dependent Jobs for OlrOrg Address (to be added to new org)
			ZGuid oldAddressAddPk = oldAddress1.PK;
			JobDocAddress oldAddressAddJob = factory.NewWithValidTestData<JobDocAddress>();
			oldAddressAddJob.E2_OA_Address = oldAddressAddPk;
			oldAddressAddJob.E2_ParentID = ZGuid.NewZGuid();

			// Create Dependent Jobs for OlrOrg Address (to be merged with existing new org address)
			ZGuid oldAddressMergePk = oldAddress2.PK;
			JobDocAddress oldAddressMergeJob = factory.NewWithValidTestData<JobDocAddress>();
			oldAddressMergeJob.E2_OA_Address = oldAddressMergePk;
			oldAddressMergeJob.E2_ParentID = ZGuid.NewZGuid();

			factory.Save();

			// --------------
			// PRE-ASSERTIONS
			// --------------

			// Assert ADDED address was moved to NewOrg, and MERGED address remained on OldOrg (only references should have been moved)
			AssertEquals("BEFORE MERGE - OldOrg Address Count", 2, oldOrgHeader.Addresses.Count);
			AssertEquals("BEFORE MERGE - NewOrg Address Count", 1, newOrgHeader.Addresses.Count);
			AssertEquals("BEFORE MERGE - OldOrg has Address PK = " + oldAddressMergePk.ToString(), true, DoesOrgHeaderHaveAddress(oldOrgHeader, oldAddressMergePk));
			AssertEquals("BEFORE MERGE - OldOrg has Address PK = " + oldAddressAddPk.ToString(), true, DoesOrgHeaderHaveAddress(oldOrgHeader, oldAddressAddPk));
			AssertEquals("BEFORE MERGE - NewOrg Address", newAddress1.PK, newOrgHeader.Addresses[0].PK);

			// Assert address references were updated for MERGED address, and remained the same for ADDED address
			AssertEquals("BEFORE MERGE - Merged Address Job", oldAddressMergePk, oldAddressMergeJob.Address.PK);
			AssertEquals("BEFORE MERGE - Add Address Job", oldAddressAddPk, oldAddressAddJob.Address.PK);

			// Assert Address Capability
			AssertEquals("BEFORE MERGE - OldOrg Addr1 is Main Address", true, DoesAddressHaveMainCapability(oldOrgHeader.Addresses[0].PK));
			AssertEquals("BEFORE MERGE - OldOrg Addr2 is not Main Address", false, DoesAddressHaveMainCapability(oldOrgHeader.Addresses[1].PK));
			AssertEquals("BEFORE MERGE - NewOrg Addr1 is Main Address", true, DoesAddressHaveMainCapability(newOrgHeader.Addresses[0].PK));

			// -------------
			// PERFORM MERGE
			// -------------

			// Set up Merge Adress List
			MergeOrgAddressCollection mergeAddressInfoList = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			mergeAddressInfoList[0].Action = MergeOrgAddress.ActionAdd;
			mergeAddressInfoList[1].Action = MergeOrgAddress.ActionMerge;
			mergeAddressInfoList[1].NewObjectPK = newAddress1.PK;

			MergeOrgContactCollection mergeContactsInfoList = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeAddressInfoList, mergeContactsInfoList);
			testMerger.Save();

			// Instantiate a new factory to load data after merge
			BusinessObjectFactory assertFactory = new BusinessObjectFactory();
			OrgHeader assertOldOrg = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			OrgHeader assertNewOrg = assertFactory.Load<OrgHeader>(newOrgHeader.PK);

			// ----------
			// ASSERTIONS
			// ----------

			// Assert ADDED address was moved to NewOrg, and MERGED address remained on OldOrg (only references should have been moved)
			AssertNull("AFTER MERGE - OldOrg is null", assertOldOrg);
			AssertEquals("AFTER MERGE - NewOrg Address Count", 2, assertNewOrg.Addresses.Count);
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + newAddress1.PK.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, newAddress1.PK));
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + oldAddressAddPk.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, oldAddressAddPk));

			// Assert address references were updated for MERGED address, and remained the same for ADDED address
			JobDocAddress assertAddressMergeJob = assertFactory.Load<JobDocAddress>(oldAddressMergeJob.PK);
			JobDocAddress assertAddressAddJob = assertFactory.Load<JobDocAddress>(oldAddressAddJob.PK);
			AssertEquals("AFTER MERGE - Merged Address Job", newAddress1.PK, assertAddressMergeJob.Address.PK);
			AssertEquals("AFTER MERGE - Add Address Job", oldAddressAddPk, assertAddressAddJob.Address.PK);

			// Assert Address Capability - New address added from OldOrg should not have main address capability
			int index1 = 0;
			int index2 = 1;
			if (assertNewOrg.Addresses[1].OA_Address1 == "~TestNewAddr1~")
			{
				index1 = 1;
				index2 = 0;
			}
			AssertEquals("AFTER MERGE - NewOrg Addr1 is Main Address", true, DoesAddressHaveMainCapability(assertNewOrg.Addresses[index1].PK));
			AssertEquals("AFTER MERGE - NewOrg Addr2 is not Main Address", false, !DoesAddressHaveMainCapability(assertNewOrg.Addresses[index1].PK));
			AssertEquals("AFTER MERGE - NewOrg Addr1 has its code", "ABC", assertNewOrg.Addresses[index1].OA_Code);
			AssertNotEquals("AFTER MERGE - NewOrg Addr2 has other code", "ABC", assertNewOrg.Addresses[index2].OA_Code);

			mergeAddressInfoList = new MergeOrgAddressCollection(factory, oldOrgHeader2, newOrgHeader);
			mergeAddressInfoList[0].Action = MergeOrgAddress.ActionAdd;

			mergeContactsInfoList = new MergeOrgContactCollection(factory, oldOrgHeader2, newOrgHeader);

			testMerger = new OrganisationMergerForTest(oldOrgHeader2.PK, newOrgHeader.PK, mergeAddressInfoList, mergeContactsInfoList);
			testMerger.Save();

			// Instantiate a new factory to load data after merge
			assertFactory = new BusinessObjectFactory();
			OrgHeader assertOldOrg2 = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			assertNewOrg = assertFactory.Load<OrgHeader>(newOrgHeader.PK);
			AssertEquals("AFTER MERGE - NewOrg Address Count", 3, assertNewOrg.Addresses.Count);
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + oldAddress3.PK.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, oldAddress3.PK));
		}

		public void TestSaveMergesOrgAddressesTakingSortCodeInotAccount()
		{
			var factory = new BusinessObjectFactory();

			// Create Old Organisation and Addresses
			var oldOrgHeader = factory.New<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			var oldAddress1 = oldOrgHeader.MainAddress;
			oldAddress1.OA_Address1 = "~TestAddr1~";
			oldAddress1.OA_Address2 = "~TestAddr1~";
			oldAddress1.OA_Code = "ABC";
			var oldAddress2 = oldOrgHeader.Addresses.AddNew();
			oldAddress2.OA_Address1 = "~TestAddr2~";
			oldAddress2.OA_Address2 = "~TestAddr2~";
			oldAddress2.OA_Code = "EDF";

			// Create New Organisation and Address
			var newOrgHeader = factory.New<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			var newAddress1 = newOrgHeader.MainAddress;
			newAddress1.OA_Address1 = "~TestAddr1~";
			newAddress1.OA_Address2 = "~TestAddr1~";
			newAddress1.OA_Code = "ABC";
			var newAddress2 = newOrgHeader.Addresses.AddNew();
			newAddress2.OA_Address1 = "~TestAddr2~";
			newAddress2.OA_Address2 = "~TestAddr2~";
			newAddress2.OA_Code = "XYZ";

			factory.Save();

			AssertEquals("BEFORE MERGE - OldOrg Address Count", 2, oldOrgHeader.Addresses.Count);
			AssertEquals("BEFORE MERGE - NewOrg Address Count", 2, newOrgHeader.Addresses.Count);

			// Set up Merge Adress List
			var mergeAddressInfoList = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			mergeAddressInfoList[0].Action = MergeOrgAddress.ActionAdd;
			mergeAddressInfoList[1].Action = MergeOrgAddress.ActionAdd;

			mergeAddressInfoList[0].ValidateAction();
			mergeAddressInfoList[1].ValidateAction();
			Assert(mergeAddressInfoList[0].ActionInfo.HasWarning("This address will be merged because an identical address exists on the target organization"));
			Assert(!mergeAddressInfoList[1].ActionInfo.HasWarning("This address will be merged because an identical address exists on the target organization"));

			var mergeContactsInfoList = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeAddressInfoList, mergeContactsInfoList);
			testMerger.Save();

			// Instantiate a new factory to load data after merge
			var assertFactory = new BusinessObjectFactory();
			var assertOldOrg = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			var assertNewOrg = assertFactory.Load<OrgHeader>(newOrgHeader.PK);

			AssertNull("AFTER MERGE - OldOrg is null", assertOldOrg);
			AssertEquals("AFTER MERGE - NewOrg Address Count", 3, assertNewOrg.Addresses.Count);
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + newAddress1.PK.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, newAddress1.PK));
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + newAddress2.PK.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, newAddress2.PK));
			AssertEquals("AFTER MERGE - NewOrg doesn't have Address PK = " + oldAddress1.PK.ToString(), false, DoesOrgHeaderHaveAddress(assertNewOrg, oldAddress1.PK));
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + oldAddress2.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, oldAddress2.PK));
		}

		public void TestMergeAddressIgnoreTrimSpaces()
		{
			var factory = new BusinessObjectFactory();

			// Create Old Organisation and Addresses
			var oldOrgHeader = factory.New<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			var oldAddress1 = oldOrgHeader.MainAddress;
			oldAddress1.OA_Address1 = "~TestAddr1~";
			oldAddress1.OA_Address2 = "~TestAddr1~";
			oldAddress1.OA_Code = "ABC";
			oldAddress1.OA_City = "City";
			oldAddress1.OA_State = "State";
			oldAddress1.OA_Phone = "123123123";
			oldAddress1.OA_Fax = "123123123";
			oldAddress1.OA_Mobile = "123123123";
			oldAddress1.OA_Email = "123@123.com";
			oldAddress1.OA_Code = "Code";
			var oldAddress2 = oldOrgHeader.Addresses.AddNew();
			oldAddress2.OA_Address1 = "~TestAddr2~";
			oldAddress2.OA_Address2 = "~TestAddr2~";
			oldAddress2.OA_Code = "ABC2";
			oldAddress2.OA_City = "City2";
			oldAddress2.OA_State = "State2";
			oldAddress2.OA_Phone = "1231231232";
			oldAddress2.OA_Fax = "1231231232";
			oldAddress2.OA_Mobile = "1231231232";
			oldAddress2.OA_Email = "1232@123.com";
			oldAddress2.OA_Code = "Code2";

			// Create New Organisation and Address
			var newOrgHeader = factory.New<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			var newAddress1 = newOrgHeader.MainAddress;
			newAddress1.OA_Address1 = " ~TestAddr1~ ";
			newAddress1.OA_Address2 = " ~TestAddr1~ ";
			newAddress1.OA_Code = " ABC ";
			newAddress1.OA_City = " City ";
			newAddress1.OA_State = " State ";
			newAddress1.OA_Phone = " 123123123 ";
			newAddress1.OA_Fax = " 123123123 ";
			newAddress1.OA_Mobile = " 123123123 ";
			newAddress1.OA_Email = " 123@123.com ";
			newAddress1.OA_Code = " Code ";
			var newAddress2 = newOrgHeader.Addresses.AddNew();
			newAddress2.OA_Address1 = " ~Test Addr2~ ";
			newAddress2.OA_Address2 = " ~Test Addr2~ ";
			newAddress2.OA_Code = " ABC2 ";
			newAddress2.OA_City = " City2 ";
			newAddress2.OA_State = " State2 ";
			newAddress2.OA_Phone = " 1231231232 ";
			newAddress2.OA_Fax = " 1231231232 ";
			newAddress2.OA_Mobile = " 1231231232 ";
			newAddress2.OA_Email = " 1232@123.com ";
			newAddress2.OA_Code = " Code2 ";

			factory.Save();

			AssertEquals("BEFORE MERGE - OldOrg Address Count", 2, oldOrgHeader.Addresses.Count);
			AssertEquals("BEFORE MERGE - NewOrg Address Count", 2, newOrgHeader.Addresses.Count);

			// Set up Merge Adress List
			var mergeAddressInfoList = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			mergeAddressInfoList[0].Action = MergeOrgAddress.ActionAdd;
			mergeAddressInfoList[1].Action = MergeOrgAddress.ActionAdd;

			mergeAddressInfoList[0].ValidateAction();
			mergeAddressInfoList[1].ValidateAction();
			Assert(mergeAddressInfoList[0].ActionInfo.HasWarning("This address will be merged because an identical address exists on the target organization"));
			Assert(!mergeAddressInfoList[1].ActionInfo.HasWarning("This address will be merged because an identical address exists on the target organization"));

			var mergeContactsInfoList = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeAddressInfoList, mergeContactsInfoList);
			testMerger.Save();

			// Instantiate a new factory to load data after merge
			var assertFactory = new BusinessObjectFactory();
			var assertOldOrg = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			var assertNewOrg = assertFactory.Load<OrgHeader>(newOrgHeader.PK);

			AssertNull("AFTER MERGE - OldOrg is null", assertOldOrg);
			AssertEquals("AFTER MERGE - NewOrg Address Count", 3, assertNewOrg.Addresses.Count);
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + newAddress1.PK.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, newAddress1.PK));
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + newAddress2.PK.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, newAddress2.PK));
			AssertEquals("AFTER MERGE - NewOrg doesn't have Address PK = " + oldAddress1.PK.ToString(), false, DoesOrgHeaderHaveAddress(assertNewOrg, oldAddress1.PK));
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + oldAddress2.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, oldAddress2.PK));
		}

		public void TestSaveMergeWithInvalidAction()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var oldAddress1 = oldOrgHeader.MainAddress;
			oldAddress1.OA_Address1 = "TestAddress1";
			oldAddress1.OA_Code = "TA1";
			var oldAddress2 = oldOrgHeader.Addresses.AddNew();
			oldAddress2.OA_Address1 = "TestAddress2";
			oldAddress2.OA_Code = "TA2";
			var oldContact1 = oldOrgHeader.Contacts.AddNew();
			oldContact1.OC_ContactName = "A";
			var oldContact2 = oldOrgHeader.Contacts.AddNew();
			oldContact2.OC_ContactName = "B";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newAddress1 = newOrgHeader.MainAddress;
			newAddress1.OA_Address1 = "TestAddress2";
			newAddress1.OA_Code = "TA2";
			var newAddress2 = newOrgHeader.Addresses.AddNew();
			newAddress2.OA_Address1 = "TestAddress3";
			newAddress2.OA_Code = "TA3";
			var newContact1 = newOrgHeader.Contacts.AddNew();
			newContact1.OC_ContactName = "C";
			var newContact2 = newOrgHeader.Contacts.AddNew();
			newContact2.OC_ContactName = "D";

			factory.Save();

			AssertEquals("BEFORE MERGE - OldOrg Address Count", 2, oldOrgHeader.Addresses.Count);
			AssertEquals("BEFORE MERGE - NewOrg Address Count", 2, newOrgHeader.Addresses.Count);
			AssertEquals("BEFORE MERGE - OldOrg Contact Count", 2, oldOrgHeader.Contacts.Count);
			AssertEquals("BEFORE MERGE - NewOrg Contact Count", 2, newOrgHeader.Contacts.Count);

			AssertSaveMergeOrgAddressesWithInvalidAction(string.Empty, string.Empty, "Please enter a value.", "Please enter a value.");
			AssertSaveMergeOrgAddressesWithInvalidAction("AAA", "BBB", "Enter a valid selection.", "Enter a valid selection.");
			AssertSaveMergeOrgContactsWithInvalidAction(string.Empty, string.Empty, "Please enter a value.", "Please enter a value.");
			AssertSaveMergeOrgContactsWithInvalidAction("AAA", "BBB", "Enter a valid selection.", "Enter a valid selection.");

			void AssertSaveMergeOrgAddressesWithInvalidAction(string action1, string action2, string expectedError1, string expectedError2)
			{
				var mergeAddressInfoList = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
				mergeAddressInfoList[0].Action = MergeOrgAddress.ActionAdd;
				mergeAddressInfoList[1].Action = MergeOrgAddress.ActionMerge;

				AssertEquals(0, mergeAddressInfoList[0].ActionInfo.Notifications.Count());
				AssertEquals(0, mergeAddressInfoList[1].ActionInfo.Notifications.Count());

				mergeAddressInfoList[0].Action = action1;
				mergeAddressInfoList[1].Action = action2;

				Assert(mergeAddressInfoList[0].ActionInfo.HasNotification(expectedError1));
				Assert(mergeAddressInfoList[1].ActionInfo.HasNotification(expectedError2));

				var mergeContactsInfoList = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
				var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeAddressInfoList, mergeContactsInfoList);
				AssertExceptionThrown<InvalidOperationException>(() => testMerger.Save());
			}

			void AssertSaveMergeOrgContactsWithInvalidAction(string action1, string action2, string expectedError1, string expectedError2)
			{
				var mergeAddressInfoList = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
				var mergeContactsInfoList = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
				mergeContactsInfoList[0].Action = MergeOrgAddress.ActionAdd;
				mergeContactsInfoList[1].Action = MergeOrgAddress.ActionMerge;

				AssertEquals(0, mergeContactsInfoList[0].ActionInfo.Notifications.Count());
				AssertEquals(0, mergeContactsInfoList[1].ActionInfo.Notifications.Count());

				mergeContactsInfoList[0].Action = action1;
				mergeContactsInfoList[1].Action = action2;

				Assert(mergeContactsInfoList[0].ActionInfo.HasNotification(expectedError1));
				Assert(mergeContactsInfoList[1].ActionInfo.HasNotification(expectedError2));

				var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeAddressInfoList, mergeContactsInfoList);
				AssertExceptionThrown<InvalidOperationException>(() => testMerger.Save());
			}
		}

		public void TestSaveMergesOnlyUniqueAddresses()
		{
			// -----------------
			// PREPARE TEST DATA
			// -----------------

			var factory = new BusinessObjectFactory();

			// Create Old Organisation and Addresses
			var oldOrg = factory.New<OrgHeader>();
			oldOrg.OH_Code = "GONDOR";
			var oldAddress1 = oldOrg.MainAddress;
			SetAddressValues(oldAddress1, "MINAS TIRITH", "Pelennor Fields", "GONDOR", "Middle Earth", "123", "456", "789", "aragorn@gondor.com");
			var oldAddress2 = oldOrg.Addresses.AddNew();
			SetAddressValues(oldAddress2, "OSGILIATH", "Anduin River", "GONDOR", "Middle Earth", "987", "654", "321", "faramir@gondor.com");

			var jobDocAddress1 = factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress1.E2_OA_Address = oldAddress1.PK;
			jobDocAddress1.E2_AddressType = "OFC";
			var jobDocAddress2 = factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress2.E2_OA_Address = oldAddress2.PK;
			jobDocAddress2.E2_AddressType = "CNE";

			// Create New Organisation and Address
			var newOrg = factory.New<OrgHeader>();
			newOrg.OH_Code = "OSGILIATH";
			var newAddress1 = newOrg.MainAddress;
			SetAddressValues(newAddress1, "OSGILIATH", "Anduin River", "GONDOR", "Middle Earth", "987", "654", "321", "faramir@gondor.com");

			factory.Save();

			// --------------
			// PRE-ASSERTIONS
			// --------------

			// Assert ADDED address was moved to NewOrg, and MERGED address remained on OldOrg (only references should have been moved)
			AssertEquals("BEFORE MERGE - OldOrg Address Count", 2, oldOrg.Addresses.Count);
			AssertEquals("BEFORE MERGE - NewOrg Address Count", 1, newOrg.Addresses.Count);
			AssertEquals("BEFORE MERGE - NewOrg Address", newAddress1.PK, newOrg.Addresses[0].PK);

			// Assert Address Capability
			AssertEquals("BEFORE MERGE - OldOrg Addr1 is Main Address", true, DoesAddressHaveMainCapability(oldOrg.Addresses[0].PK));
			AssertEquals("BEFORE MERGE - OldOrg Addr2 is not Main Address", false, DoesAddressHaveMainCapability(oldOrg.Addresses[1].PK));
			AssertEquals("BEFORE MERGE - NewOrg Addr1 is Main Address", true, DoesAddressHaveMainCapability(newOrg.Addresses[0].PK));

			// -------------
			// PERFORM MERGE
			// -------------

			// Set up Merge Adress List
			var mergeAddressInfoList = new MergeOrgAddressCollection(factory, oldOrg, newOrg);
			mergeAddressInfoList[0].Action = MergeOrgAddress.ActionAdd;
			mergeAddressInfoList[1].Action = MergeOrgAddress.ActionAdd;

			mergeAddressInfoList[0].ValidateAction();
			mergeAddressInfoList[1].ValidateAction();
			Assert(!mergeAddressInfoList[0].ActionInfo.HasWarning("This address will be merged because an identical address exists on the target organization"));
			Assert(mergeAddressInfoList[1].ActionInfo.HasWarning("This address will be merged because an identical address exists on the target organization"));

			var mergeContactsInfoList = new MergeOrgContactCollection(factory, oldOrg, newOrg);

			var testMerger = new OrganisationMergerForTest(oldOrg.PK, newOrg.PK, mergeAddressInfoList, mergeContactsInfoList);
			testMerger.Save();

			// Instantiate a new factory to load data after merge
			var assertFactory = new BusinessObjectFactory();
			var assertOldOrg = assertFactory.Load<OrgHeader>(oldOrg.PK);
			var assertNewOrg = assertFactory.Load<OrgHeader>(newOrg.PK);
			var assertJobDocAddress1 = assertFactory.Load<JobDocAddress>(jobDocAddress1.PK);
			var assertJobDocAddress2 = assertFactory.Load<JobDocAddress>(jobDocAddress2.PK);

			// ----------
			// ASSERTIONS
			// ----------

			// Assert only unique addresses were copied
			var minasTirith = assertNewOrg.Addresses.Cast<OrgAddress>().Single(a => a.OA_Address1 == "MINAS TIRITH");
			var osgiliath = assertNewOrg.Addresses.Cast<OrgAddress>().Single(a => a.OA_Address1 == "OSGILIATH");

			AssertNull("AFTER MERGE - OldOrg is null", assertOldOrg);
			AssertEquals("AFTER MERGE - NewOrg Address Count", 2, assertNewOrg.Addresses.Count);
			AssertEquals("AFTER MERGE - NewOrg has Address PK = " + newAddress1.PK.ToString(), true, DoesOrgHeaderHaveAddress(assertNewOrg, newAddress1.PK));
			AssertNotEquals("AFTER MERGE - Addresses are different", minasTirith.OA_Address1, osgiliath.OA_Address1);
			AssertEquals("AFTER MERGE - Addresses match original org addresses", "MINAS TIRITH", minasTirith.OA_Address1);
			AssertEquals("AFTER MERGE - Addresses match original org addresses", "OSGILIATH", osgiliath.OA_Address1);

			// Assert JobDocAddress reference updated
			AssertEquals("AFTER MERGE - JobDocAddress should now reference neworg address", minasTirith.PK, assertJobDocAddress1.E2_OA_Address);
			AssertEquals("AFTER MERGE - JobDocAddress should now reference neworg address", osgiliath.PK, assertJobDocAddress2.E2_OA_Address);
		}

		void SetAddressValues(OrgAddress address, string address1, string address2, string city, string state, string phone, string fax, string mobile, string email)
		{
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_City = city;
			address.OA_State = state;
			address.OA_Phone = phone;
			address.OA_Fax = fax;
			address.OA_Mobile = mobile;
			address.OA_Email = email;
		}

		public void TestSaveMergesOrgContacts()
		{
			// -----------------
			// PREPARE TEST DATA
			// -----------------

			BusinessObjectFactory factory = new BusinessObjectFactory();

			// Create Old Organisation and Contacts
			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			OrgContact oldCnt1 = factory.NewWithValidTestData<OrgContact>();
			oldCnt1.OC_OH = oldOrgHeader.PK;
			OrgContact oldCnt2 = factory.NewWithValidTestData<OrgContact>();
			oldCnt2.OC_OH = oldOrgHeader.PK;
			OrgAddress oldAddress1 = oldOrgHeader.MainAddress;
			oldAddress1.OA_Address1 = "~TestOldAddr1~";
			oldCnt1.OC_OA_OrgAddress = oldAddress1.PK;

			// Create New Organisation and Contacts
			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			OrgContact newCnt1 = factory.NewWithValidTestData<OrgContact>();
			newCnt1.OC_OH = newOrgHeader.PK;
			newCnt1.OC_OH_AddressOverride = oldOrgHeader.PK;
			OrgAddress newAddress1 = newOrgHeader.MainAddress;
			newAddress1.OA_Address1 = "~TestNewAddr1~";

			OrgHeader otherOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			otherOrgHeader.OH_Code = "~TESTOTHER~";
			OrgContact otherCnt1 = factory.NewWithValidTestData<OrgContact>();
			otherCnt1.OC_OH = otherOrgHeader.PK;
			otherCnt1.OC_OH_AddressOverride = oldOrgHeader.PK;

			factory.Save();

			// Create Dependent Jobs for OlrOrg Contact (to be added to new org)
			ZGuid oldCntAddPk = oldCnt1.PK;
			JobHeader oldCntAddJob = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			oldCntAddJob.JH_OC_LocalBillingContact = oldCntAddPk;

			// Create Dependent Jobs for OlrOrg Contact (to be merged with existing new org Contact)
			ZGuid oldCntMergePk = oldCnt2.PK;
			JobHeader oldCntMergeJob = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			oldCntMergeJob.JH_OC_LocalBillingContact = oldCntMergePk;

			factory.Save();

			// --------------
			// PRE-ASSERTIONS
			// --------------

			// Assert ADDED contact was moved to NewOrg, and MERGED contact remained on OldOrg (only references should have been moved)
			AssertEquals("BEFORE MERGE - OldOrg Contact Count", 2, oldOrgHeader.Contacts.Count);
			AssertEquals("BEFORE MERGE - NewOrg Contact Count", 1, newOrgHeader.Contacts.Count);
			AssertEquals("BEFORE MERGE - OldOrg has Contact PK = " + oldCntMergePk.ToString(), true, DoesOrgHeaderHaveContact(oldOrgHeader, oldCntMergePk));
			AssertEquals("BEFORE MERGE - OldOrg has Contact PK = " + oldCntAddPk.ToString(), true, DoesOrgHeaderHaveContact(oldOrgHeader, oldCntAddPk));
			AssertEquals("BEFORE MERGE - NewOrg Contact", newCnt1.PK, newOrgHeader.Contacts[0].PK);

			// Assert contact references were updated for MERGED address, and remained the same for ADDED address
			AssertEquals("BEFORE MERGE - Merged Contact Job", oldCntMergePk, oldCntMergeJob.LocalBillingContact.PK);
			AssertEquals("BEFORE MERGE - Add Contact Job", oldCntAddPk, oldCntAddJob.LocalBillingContact.PK);

			// -------------
			// PERFORM MERGE
			// -------------

			// Set up Merge Adress List
			MergeOrgAddressCollection mergeAddressInfoList = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			mergeAddressInfoList[0].Action = MergeOrgAddress.ActionMerge;
			mergeAddressInfoList[0].NewObjectPK = newAddress1.PK;

			MergeOrgContactCollection mergeContactsInfoList = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			mergeContactsInfoList[0].Action = MergeOrgContact.ActionAdd;
			mergeContactsInfoList[1].Action = MergeOrgContact.ActionMerge;
			mergeContactsInfoList[1].NewObjectPK = newCnt1.PK;

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeAddressInfoList, mergeContactsInfoList);
			testMerger.DeleteOldOrg = false;
			testMerger.Save();

			// Instantiate a new factory to load data after merge
			BusinessObjectFactory assertFactory = new BusinessObjectFactory();
			OrgHeader assertOldOrg = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			OrgHeader assertNewOrg = assertFactory.Load<OrgHeader>(newOrgHeader.PK);
			OrgHeader assertOtherOrg = assertFactory.Load<OrgHeader>(otherOrgHeader.PK);

			// ----------
			// ASSERTIONS
			// ----------

			// Assert ADDED address was moved to NewOrg, and MERGED address remained on OldOrg (only references should have been moved)
			AssertEquals("AFTER MERGE - OldOrg Contact Count", 1, assertOldOrg.Contacts.Count);
			AssertEquals("AFTER MERGE - NewOrg Contact Count", 2, assertNewOrg.Contacts.Count);
			AssertEquals("AFTER MERGE - OldOrg Contact", oldCntMergePk, assertOldOrg.Contacts[0].PK);
			AssertEquals("AFTER MERGE - NewOrg has Contact PK = " + newCnt1.PK.ToString(), true, DoesOrgHeaderHaveContact(assertNewOrg, newCnt1.PK));
			AssertEquals("AFTER MERGE - NewOrg has Contact PK = " + oldCntAddPk.ToString(), true, DoesOrgHeaderHaveContact(assertNewOrg, oldCntAddPk));
			AssertNotEquals(oldAddress1.PK, assertNewOrg.Contacts[0].OC_OA_OrgAddress);
			AssertNotEquals(oldAddress1.PK, assertNewOrg.Contacts[1].OC_OA_OrgAddress);
			AssertNotEquals(assertOldOrg.PK, assertNewOrg.Contacts[0].OC_OH_AddressOverride);
			AssertNotEquals(assertOldOrg.PK, assertNewOrg.Contacts[1].OC_OH_AddressOverride);
			AssertNotEquals(assertOldOrg.PK, assertOtherOrg.Contacts[0].OC_OH_AddressOverride);

			// Assert address references were updated for MERGED address, and remained the same for ADDED address
			JobHeader assertCntMergeJob = assertFactory.Load<JobHeader>(oldCntMergeJob.PK);
			JobHeader assertCntAddJob = assertFactory.Load<JobHeader>(oldCntAddJob.PK);
			AssertEquals("AFTER MERGE - Merged Contact Job", newCnt1.PK, assertCntMergeJob.LocalBillingContact.PK);
			AssertEquals("AFTER MERGE - Add Contact Job", oldCntAddPk, assertCntAddJob.LocalBillingContact.PK);
		}

		public void TestOrgRelatedPartyReferences()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "~zzz1~";

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "2";

			OrgHeader otherOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			otherOrgHeader.OH_Code = "~TESTOTHER~";
			otherOrgHeader.MainAddress.OA_Code = "3";

			oldOrgHeader.AllRelatedParties.SetRelatedParty(oldOrgHeader, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			oldOrgHeader.AllRelatedParties.SetRelatedParty(newOrgHeader, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);
			oldOrgHeader.AllRelatedParties.SetRelatedParty(otherOrgHeader, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			oldOrgHeader.AllRelatedParties.SetRelatedParty(otherOrgHeader, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			oldOrgHeader.AllRelatedParties.SetRelatedParty(otherOrgHeader.MainAddress.PK, oldOrgHeader, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			oldOrgHeader.AllRelatedParties.SetRelatedParty(otherOrgHeader.MainAddress.PK, oldOrgHeader, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			newOrgHeader.AllRelatedParties.SetRelatedParty(oldOrgHeader, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			newOrgHeader.AllRelatedParties.SetRelatedParty(newOrgHeader, RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			newOrgHeader.AllRelatedParties.SetRelatedParty(newOrgHeader, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			newOrgHeader.AllRelatedParties.SetRelatedParty(otherOrgHeader.MainAddress.PK, newOrgHeader, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			otherOrgHeader.AllRelatedParties.SetRelatedParty(oldOrgHeader, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			otherOrgHeader.AllRelatedParties.SetRelatedParty(newOrgHeader, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty);
			otherOrgHeader.AllRelatedParties.SetRelatedParty(otherOrgHeader, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			factory.Save();

			AssertEquals("Precondition: ", oldOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty).PK, oldOrgHeader.PK);
			AssertEquals("Precondition: ", oldOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty).PK, newOrgHeader.PK);
			AssertEquals("Precondition: ", oldOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup).PK, otherOrgHeader.PK);
			AssertEquals("Precondition: ", oldOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery).PK, otherOrgHeader.PK);
			AssertEquals("Precondition: ", oldOrgHeader.GetRelatedParty(otherOrgHeader.MainAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty).PK, oldOrgHeader.PK);
			AssertEquals("Precondition: ", oldOrgHeader.GetRelatedParty(otherOrgHeader.MainAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK, oldOrgHeader.PK);

			AssertEquals("Precondition: ", newOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty).PK, oldOrgHeader.PK);
			AssertEquals("Precondition: ", newOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Pickup).PK, newOrgHeader.PK);
			AssertEquals("Precondition: ", newOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery).PK, newOrgHeader.PK);
			AssertEquals("Precondition: ", newOrgHeader.GetRelatedParty(otherOrgHeader.MainAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty).PK, newOrgHeader.PK);

			AssertEquals("Precondition: ", otherOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL).PK, oldOrgHeader.PK);
			AssertEquals("Precondition: ", otherOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder).PK, newOrgHeader.PK);
			AssertEquals("Precondition: ", otherOrgHeader.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK, otherOrgHeader.PK);

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader), new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader));
			testMerger.Save();

			BusinessObjectFactory assertFactory = new BusinessObjectFactory();
			OrgHeader assertOldOrg = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			OrgHeader assertNewOrg = assertFactory.Load<OrgHeader>(newOrgHeader.PK);
			OrgHeader assertOtherOrg = assertFactory.Load<OrgHeader>(otherOrgHeader.PK);

			assertNewOrg.AllRelatedParties.Load();

			AssertNull(assertOldOrg);

			AssertEquals("4 own and 4 from old org", 7, assertNewOrg.AllRelatedParties.Count);

			AssertEquals("After merge: changed", newOrgHeader.PK, assertNewOrg.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty).PK);
			AssertEquals("After merge: not changed", newOrgHeader.PK, assertNewOrg.GetRelatedParty(RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyDirectionList.Codes.Pickup).PK);
			AssertEquals("After merge: not changed", newOrgHeader.PK, assertNewOrg.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Delivery).PK);
			AssertEquals("After merge: not changed", newOrgHeader.PK, assertNewOrg.MiscServ.OM_OH);
			AssertEquals("After merge: not changed", newOrgHeader.PK, assertNewOrg.GetRelatedParty(otherOrgHeader.MainAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty).PK);
			AssertEquals("After merge: changed", newOrgHeader.PK, assertNewOrg.GetRelatedParty(otherOrgHeader.MainAddress.PK, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK);

			AssertEquals("After merge: changed", newOrgHeader.PK, assertOtherOrg.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL).PK);
			AssertEquals("After merge: not changed", newOrgHeader.PK, assertOtherOrg.GetRelatedParty(RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder).PK);
			AssertEquals("After merge: not changed", otherOrgHeader.PK, assertOtherOrg.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL).PK);
			AssertEquals("After merge: not changed", otherOrgHeader.PK, assertOtherOrg.MiscServ.OM_OH);
		}

		public void TestOrgMerge_ManagementRelatedParty()
		{
			var factory = new BusinessObjectFactory();
			var orgGreatGrandParent = factory.NewWithValidTestData<OrgHeader>();
			var orgGrandParent = factory.NewWithValidTestData<OrgHeader>();
			var orgParent = factory.NewWithValidTestData<OrgHeader>();
			var orgChild = factory.NewWithValidTestData<OrgHeader>();

			CreateOrgManagementRelatedParty(factory, orgGreatGrandParent.PK, orgGreatGrandParent.PK, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup);
			CreateOrgManagementRelatedParty(factory, orgGreatGrandParent.PK, orgGreatGrandParent.PK, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup);

			CreateOrgManagementRelatedParty(factory, orgGrandParent.PK, orgGrandParent.PK, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup);
			CreateOrgManagementRelatedParty(factory, orgGrandParent.PK, orgGrandParent.PK, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup);

			CreateOrgManagementRelatedParty(factory, orgParent.PK, orgParent.PK, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup);
			CreateOrgManagementRelatedParty(factory, orgParent.PK, orgParent.PK, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup);

			CreateOrgManagementRelatedParty(factory, orgChild.PK, orgChild.PK, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup);
			CreateOrgManagementRelatedParty(factory, orgChild.PK, orgChild.PK, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup);

			CreateOrgManagementRelatedParty(factory, orgGrandParent.PK, orgGreatGrandParent.PK, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder);
			CreateOrgManagementRelatedParty(factory, orgParent.PK, orgGrandParent.PK, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder);
			CreateOrgManagementRelatedParty(factory, orgChild.PK, orgParent.PK, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder);
			factory.Save();

			var relatedPartyListBeforeMerge = factory.Load<OrgManagementRelatedParty>(new ZQuery(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping));
			AssertEquals(3, relatedPartyListBeforeMerge.Length);

			var organisationMerger = new OrganisationMergerForTest(orgParent.PK, orgGreatGrandParent.PK, new MergeOrgAddressCollection(factory, orgParent, orgGreatGrandParent), new MergeOrgContactCollection(factory, orgParent, orgGreatGrandParent));
			organisationMerger.Save();

			var assertFactory = new BusinessObjectFactory();
			var newOrgParent = assertFactory.Load<OrgHeader>(orgGreatGrandParent.PK);
			var newOrgChild = assertFactory.Load<OrgHeader>(orgGrandParent.PK);
			var oldOrgChild = assertFactory.Load<OrgHeader>(orgChild.PK);

			var relatedPartyListAfterMerge = assertFactory.Load<OrgManagementRelatedParty>(new ZQuery(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping));
			AssertEquals(2, relatedPartyListAfterMerge.Length);

			var relatedPartyOldOrgChild = assertFactory.LoadTop1<OrgManagementRelatedParty>(new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, oldOrgChild.PK));
			AssertEquals(newOrgParent.PK, relatedPartyOldOrgChild.PR_OH_RelatedParty);

			var relatedPartyNewOrgChild = assertFactory.LoadTop1<OrgManagementRelatedParty>(new ZQuery(OrgRelatedPartySchema.PR_OH_Parent, newOrgChild.PK));
			AssertEquals(newOrgParent.PK, relatedPartyNewOrgChild.PR_OH_RelatedParty);
		}

		void CreateOrgManagementRelatedParty(BusinessObjectFactory factory, ZGuid orgParentPk, ZGuid orgRelatedPartyPk, ZString partyType, ZString freightDirection)
		{
			var orgManagementRelatedParty = factory.New<OrgManagementRelatedParty>();
			orgManagementRelatedParty.PR_OH_Parent = orgParentPk;
			orgManagementRelatedParty.PR_OH_RelatedParty = orgRelatedPartyPk;
			orgManagementRelatedParty.PR_PartyType = partyType;
			orgManagementRelatedParty.PR_FreightDirection = freightDirection;
		}

		public static void TestOrgMerge_AllowMultipleRelatedParties_ForPartyTypeSRV()
		{
			AssertAllowMultipleRelatedParties_Merged(RelatedPartyTypeList.Codes.ServiceProvider);
		}

		public void TestOrgMerge_AllowMultipleRelatedParties_ForPartyTypeCAV()
		{
			AssertAllowMultipleRelatedParties_Merged(RelatedPartyTypeList.Codes.CSAApprovedVendor);
		}

		public void TestOrgMerge_AllowMultipleRelatedParties_ForPartyTypeCAU()
		{
			AssertAllowMultipleRelatedParties_Merged(RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee);
		}

		public void TestOrgMerge_AllowMultipleRelatedParties_ForPartyTypeMAN()
		{
			AssertAllowMultipleRelatedParties_Merged(RelatedPartyTypeList.Codes.Manufacturer);
		}

		public static void AssertAllowMultipleRelatedParties_Merged(string partyType)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var otherOrgHeader1 = factory.NewWithValidTestData<OrgHeader>();
			var otherOrgHeader2 = factory.NewWithValidTestData<OrgHeader>();
			var otherOrgHeader3 = factory.NewWithValidTestData<OrgHeader>();

			oldOrgHeader.AddRelatedParty(otherOrgHeader1.PK, partyType, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			oldOrgHeader.AddRelatedParty(otherOrgHeader2.PK, partyType, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

			newOrgHeader.AddRelatedParty(otherOrgHeader2.PK, partyType, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);
			newOrgHeader.AddRelatedParty(otherOrgHeader3.PK, partyType, ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

			factory.Save();

			CombineAssertions($"PreCondition: Org with party type {partyType}", () =>
			{
				AssertEquals("OldOrgHeader has 2 related parties", 2, oldOrgHeader.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count());
				AssertEquals("NewOrgHeader has 2 related parties", 2, newOrgHeader.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count());
			});

			CombineAssertions($"PreCondition: Org with party type {partyType}", () =>
			{
				AssertEquals("OldOrgHeader has related party OtherHeader1", 1, oldOrgHeader.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader1.PK));
				AssertEquals("OldOrgHeader has related party OtherHeader2", 1, oldOrgHeader.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader2.PK));
				AssertEquals("OldOrgHeader has related party OtherHeader3", 0, oldOrgHeader.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader3.PK));

				AssertEquals("NewOrgHeader has related party OtherHeader1", 0, newOrgHeader.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader1.PK));
				AssertEquals("NewOrgHeader has related party OtherHeader2", 1, newOrgHeader.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader2.PK));
				AssertEquals("NewOrgHeader has related party OtherHeader3", 1, newOrgHeader.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader3.PK));
			});

			var organisationMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader), new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader));
			organisationMerger.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var oldOrgHeaderAfterMerge = newFactory.Load<OrgHeader>(oldOrgHeader.PK);
			var newOrgHeaderAfterMerge = newFactory.Load<OrgHeader>(newOrgHeader.PK);

			CombineAssertions($"After merge: Org with party type {partyType}", () =>
			{
				AssertNull("OldOrgHeader should not exist", oldOrgHeaderAfterMerge);
				AssertNotNull("NewOrgHeader should exist", newOrgHeaderAfterMerge);
			});

			AssertEquals($"After merge: NewOrgHeader with party type {partyType} has 3 related parties", 3, newOrgHeaderAfterMerge.AllRelatedPartiesView.Count);

			CombineAssertions($"After merge: Org with party type {partyType}", () =>
			{
				AssertEquals("NewOrgHeader has related party OtherHeader1", 1, newOrgHeaderAfterMerge.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader1.PK));
				AssertEquals("NewOrgHeader has related party OtherHeader2", 1, newOrgHeaderAfterMerge.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader2.PK));
				AssertEquals("NewOrgHeader has related party OtherHeader3", 1, newOrgHeaderAfterMerge.AllRelatedPartiesView.Cast<OrgRelatedParty>().Count(x => x.PR_OH_RelatedParty == otherOrgHeader3.PK));
			});
		}

		public void TestMoveOrgStaffAssignments()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "~zzz1~";
			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "2";

			OrgStaffAssignments ass1 = factory.NewWithValidTestData<OrgStaffAssignments>();
			OrgStaffAssignments ass2 = factory.NewWithValidTestData<OrgStaffAssignments>();
			OrgStaffAssignments ass3 = factory.NewWithValidTestData<OrgStaffAssignments>();

			GlbStaff staff1 = factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = factory.NewWithValidTestData<GlbStaff>();

			ass1.O8_OH = oldOrgHeader.PK;
			ass1.O8_Role = "ACT";
			ass1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			ass1.O8_Department = "FRT";
			ass1.O8_GC = GlbCompany.CurrentCompany.PK;

			ass2.O8_OH = oldOrgHeader.PK;
			ass2.O8_Role = "CUS";
			ass2.O8_GS_NKPersonResponsible = staff1.GS_Code;
			ass2.O8_Department = "ALL";
			ass2.O8_GC = GlbCompany.CurrentCompany.PK;

			ass3.O8_OH = newOrgHeader.PK;
			ass3.O8_Role = "ACT";
			ass3.O8_GS_NKPersonResponsible = staff2.GS_Code;
			ass3.O8_Department = "FRT";
			ass3.O8_GC = GlbCompany.CurrentCompany.PK;

			oldOrgHeader.StaffAssignments.Add(ass1);
			oldOrgHeader.StaffAssignments.Add(ass2);
			newOrgHeader.StaffAssignments.Add(ass3);

			factory.Save();

			AssertEquals("Precondition: ", 2, oldOrgHeader.StaffAssignments.Count);
			AssertEquals("Precondition: ", 1, newOrgHeader.StaffAssignments.Count);

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader), new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader));
			testMerger.DeleteOldOrg = false;
			testMerger.Save();

			BusinessObjectFactory assertFactory = new BusinessObjectFactory();
			OrgHeader assertOldOrg = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			OrgHeader assertNewOrg = assertFactory.Load<OrgHeader>(newOrgHeader.PK);

			AssertEquals("After merge: 1 moved, 1 stayed because of validation", 1, assertOldOrg.StaffAssignments.Count);
			AssertEquals("After merge: 1 added", 2, assertNewOrg.StaffAssignments.Count);
		}

		public void TestMoveStaffAssignmentsWithMixedCompanies_ShouldTransferCorrectly()
		{
			var factory = new BusinessObjectFactory();
			var staff1 = factory.NewWithValidTestData<GlbStaff>();
			var staff2 = factory.NewWithValidTestData<GlbStaff>();
			var nonCurrentCompany = factory.Load<GlbCompany>(new ZQuery()).FirstOrDefault(x => x.PK != GlbCompany.CurrentCompany.PK);

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newAssignments1 = factory.NewWithValidTestData<OrgStaffAssignments>();
			var newAssignments2 = factory.NewWithValidTestData<OrgStaffAssignments>();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var oldAssignments1 = factory.NewWithValidTestData<OrgStaffAssignments>();
			var oldAssignments2 = factory.NewWithValidTestData<OrgStaffAssignments>();
			var oldAssignments3 = factory.NewWithValidTestData<OrgStaffAssignments>();
			var oldAssignments4 = factory.NewWithValidTestData<OrgStaffAssignments>();

			newAssignments1.O8_OH = newOrgHeader.PK;
			newAssignments1.O8_Role = "ACT";
			newAssignments1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			newAssignments1.O8_Department = "ALL";
			newAssignments1.O8_GC = GlbCompany.CurrentCompany.PK;

			newAssignments2.O8_OH = newOrgHeader.PK;
			newAssignments2.O8_Role = "SAL";
			newAssignments2.O8_GS_NKPersonResponsible = staff1.GS_Code;
			newAssignments2.O8_Department = "ALL";
			newAssignments2.O8_GC = nonCurrentCompany.PK;

			newOrgHeader.StaffAssignments.Add(newAssignments1);
			newOrgHeader.StaffAssignments.Add(newAssignments2);

			oldAssignments1.O8_OH = oldOrgHeader.PK;
			oldAssignments1.O8_Role = "ACT";
			oldAssignments1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			oldAssignments1.O8_Department = "ALL";
			oldAssignments1.O8_GC = GlbCompany.CurrentCompany.PK;

			oldAssignments2.O8_OH = oldOrgHeader.PK;
			oldAssignments2.O8_Role = "CAR";
			oldAssignments2.O8_GS_NKPersonResponsible = staff1.GS_Code;
			oldAssignments2.O8_Department = "ALL";
			oldAssignments2.O8_GC = GlbCompany.CurrentCompany.PK;

			oldAssignments3.O8_OH = oldOrgHeader.PK;
			oldAssignments3.O8_Role = "SAL";
			oldAssignments3.O8_GS_NKPersonResponsible = staff2.GS_Code;
			oldAssignments3.O8_Department = "ALL";
			oldAssignments3.O8_GC = nonCurrentCompany.PK;

			oldAssignments4.O8_OH = oldOrgHeader.PK;
			oldAssignments4.O8_Role = "CUS";
			oldAssignments4.O8_GS_NKPersonResponsible = staff2.GS_Code;
			oldAssignments4.O8_Department = "ALL";
			oldAssignments4.O8_GC = nonCurrentCompany.PK;

			oldOrgHeader.StaffAssignments.Add(oldAssignments1);
			oldOrgHeader.StaffAssignments.Add(oldAssignments2);
			oldOrgHeader.StaffAssignments.Add(oldAssignments3);
			oldOrgHeader.StaffAssignments.Add(oldAssignments4);

			factory.Save();

			CombineAssertions("PreCondition", () =>
			{
				AssertEquals(2, newOrgHeader.StaffAssignments.Count);
				AssertEquals(4, oldOrgHeader.StaffAssignments.Count);
				AssertEquals(GlbCompany.CurrentCompany.PK, oldAssignments1.Company.PK);
				AssertEquals(nonCurrentCompany.PK, oldAssignments3.Company.PK);
				AssertEquals("OldAssignments1 will stay because it's exactly the same as newAssignments1", true, newAssignments1.O8_Role == oldAssignments1.O8_Role &&
																							newAssignments1.O8_GS_NKPersonResponsible == oldAssignments1.O8_GS_NKPersonResponsible &&
																							newAssignments1.O8_Department == oldAssignments1.O8_Department &&
																							newAssignments1.O8_GC == oldAssignments1.O8_GC);
				AssertEquals("OldAssignments3 will stay because it's exactly the same as newAssignments2 except staff info", true, newAssignments2.O8_Role == oldAssignments3.O8_Role &&
																							newAssignments2.O8_GS_NKPersonResponsible != oldAssignments3.O8_GS_NKPersonResponsible &&
																							newAssignments2.O8_Department == oldAssignments3.O8_Department &&
																							newAssignments2.O8_GC == oldAssignments3.O8_GC);
			});

			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader), new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader));
			testMerger.DeleteOldOrg = false;
			testMerger.Save();

			var assertFactory = new BusinessObjectFactory();
			var assertNewOrg = assertFactory.Load<OrgHeader>(newOrgHeader.PK);
			var newAssignments = assertNewOrg.StaffAssignments;
			newAssignments.CompanySpecific = false;
			var assertOldOrg = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			var oldAssignments = assertOldOrg.StaffAssignments;
			oldAssignments.CompanySpecific = false;

			CombineAssertions(() =>
			{
				AssertEquals("Two StaffAssignments transferred", 4, newAssignments.Count);
				AssertEquals("Two StaffAssignments stayed", 2, oldAssignments.Count);
				AssertEquals("OldAssignments1 stayed", assertOldOrg.PK, oldAssignments1.Header.PK);
				AssertEquals("OldAssignments3 stayed", assertOldOrg.PK, oldAssignments3.Header.PK);
			});
		}

		public void TestDeleteOverlappingRateEntriesByRatingHeader()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var oldRatingHeaderPK = Guid.NewGuid();
			var newRatingHeaderPK = Guid.NewGuid();

			InsertRatingHeader(oldRatingHeaderPK, oldOrgHeader.PK.ToGuid());
			InsertRatingHeader(newRatingHeaderPK, newOrgHeader.PK.ToGuid());

			var oldOrgRateEntryPK = InsertRateEntry(oldRatingHeaderPK, DateTime.Today.AddMonths(-5), DateTime.Today.AddMonths(-3));
			var oldOrgOverlappingRateEntryPK = InsertRateEntry(oldRatingHeaderPK, DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(-1));
			var newOrgOverlappingRateEntryPK = InsertRateEntry(newRatingHeaderPK, DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(-1));

			InsertRateLine(oldOrgRateEntryPK);
			InsertRateLine(oldOrgOverlappingRateEntryPK);
			InsertRateLine(oldOrgOverlappingRateEntryPK);
			InsertRateLine(newOrgOverlappingRateEntryPK);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var ratingHeaderDataTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			var rateEntryDataTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			var rateLineDataTable = LoadDataTable(RateLinesSchema.Constants.TableName);

			AssertEquals(1, ratingHeaderDataTable.Select($"{RatingHeaderSchema.Constants.PK} = '{newRatingHeaderPK}'").Length);
			AssertEquals(0, ratingHeaderDataTable.Select($"{RatingHeaderSchema.Constants.PK} = '{oldRatingHeaderPK}'").Length);

			AssertEquals(1, ratingHeaderDataTable.Select($"{RatingHeaderSchema.Constants.TH_OH} = '{newOrgHeader.PK}'").Length);
			AssertEquals(0, ratingHeaderDataTable.Select($"{RatingHeaderSchema.Constants.TH_OH} = '{oldOrgHeader.PK}'").Length);

			AssertEquals("Non-overlapping rate entry should just be moved across", 1, rateEntryDataTable.Select($"{RateEntrySchema.Constants.PK} = '{oldOrgRateEntryPK}'").Length);
			AssertEquals("Rate entry for new org should remain untouched", 0, rateEntryDataTable.Select($"{RateEntrySchema.Constants.PK} = '{oldOrgOverlappingRateEntryPK}'").Length);
			AssertEquals("Deleted overlapping rate entry should have no lines ", 1, rateEntryDataTable.Select($"{RateEntrySchema.Constants.PK} = '{newOrgOverlappingRateEntryPK}'").Length);

			AssertEquals(1, rateLineDataTable.Select($"{RateLinesSchema.Constants.TL_TI} = '{oldOrgRateEntryPK}'").Length);
			AssertEquals(0, rateLineDataTable.Select($"{RateLinesSchema.Constants.TL_TI} = '{oldOrgOverlappingRateEntryPK}'").Length);
			AssertEquals(1, rateLineDataTable.Select($"{RateLinesSchema.Constants.TL_TI} = '{newOrgOverlappingRateEntryPK}'").Length);

			var expectedLog = "Merging OLDORG with NEWORG has resulted in 1 Rate Entry(s) overlap. OLDORG rates deleted.";
			AssertContainsLog(expectedLog, 1, newRatingHeaderPK, factory);
		}

		public void TestDeleteOverlappingRateEntriesByOrgReferences()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var ratingHeaderPK = Guid.NewGuid();

			InsertRatingHeader(ratingHeaderPK, newOrgHeader.PK.ToGuid());

			InsertRateEntry(ratingHeaderPK, DateTime.Today.AddMonths(-5), DateTime.Today.AddMonths(-3), oldOrgHeader.PK);
			InsertRateEntry(ratingHeaderPK, DateTime.Today.AddMonths(-5), DateTime.Today.AddMonths(-3), newOrgHeader.PK);
			InsertRateEntry(ratingHeaderPK, DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(-1), oldOrgHeader.PK);
			InsertRateEntry(ratingHeaderPK, DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(-1), newOrgHeader.PK);

			// Insert test OrgCusCode records - 2 linked to OldOrg and 1 linked to NewOrg conflicting with one of OldOrg
			var sqlText = ZString.Format(@"
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode01o', '~01', '{2}', '{0}')
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode02o', '~02', '{2}', '{0}')
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode01n', '~01', '{2}', '{1}')",
				oldOrgHeader.PK, newOrgHeader.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.ExecuteNonQuery();
			}

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var rateEntryDataTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			var orgCusCodeDataTable = LoadDataTable(OrgCusCodeSchema.Constants.TableName);

			AssertEquals("Duplicate should have been deleted", 2, rateEntryDataTable.Select($"TI_TH = '{ratingHeaderPK}'").Length);
			AssertEquals("OrgCusCodes should have been updated", 2, orgCusCodeDataTable.Select($"OK_OH = '{newOrgHeader.PK}'").Length);

			var expectedLog = "Merging OLDORG with NEWORG has resulted in 2 Rate Entry(s) overlap. OLDORG rates deleted.";
			AssertContainsLog(expectedLog, 1, ratingHeaderPK, factory);
		}

		[TestDate(2020, 12, 11)]
		public void TestDeleteOverlappingRateEntriesByOrgReferenceInTemporaryTable()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var otherOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var ratingHeaderPK1 = Guid.NewGuid();
			InsertRatingHeader(ratingHeaderPK1, newOrgHeader.PK.ToGuid());

			var oldPK = oldOrgHeader.PK.ToGuid();
			var newPK = newOrgHeader.PK.ToGuid();
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK1, DateTime.Today, oldPK, oldPK, newPK);
			var bestMatchPK = InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK1, DateTime.Today, newPK, oldPK, newPK);

			var ratingHeaderPK2 = Guid.NewGuid();
			InsertRatingHeader(ratingHeaderPK2, otherOrgHeader.PK.ToGuid(), "COS");
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK2, DateTime.Today, oldPK, oldPK, oldPK);
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK2, DateTime.Today, newPK, oldPK, newPK);
			InsertRateEntry(ratingHeaderPK2, DateTime.Today, DateTime.Today.AddMonths(6), oldOrgHeader, oldOrgHeader, oldOrgHeader.MainAddress, oldOrgHeader.MainAddress);
			InsertRateEntry(ratingHeaderPK2, DateTime.Today, DateTime.Today.AddMonths(6), oldOrgHeader, newOrgHeader, oldOrgHeader.MainAddress, oldOrgHeader.MainAddress);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var rateEntryDataTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			AssertEquals("Duplicate should have been deleted", 1, rateEntryDataTable.Select($"TI_TH = '{ratingHeaderPK1}'").Length);
			AssertEquals("Best match not deleted", 1, rateEntryDataTable.Select($"TI_PK = '{bestMatchPK}'").Length);
			AssertEquals("Duplicate should have been deleted", 2, rateEntryDataTable.Select($"TI_TH = '{ratingHeaderPK2}'").Length);

			var expectedLog = "Merging OLDORG with NEWORG has resulted in 1 Rate Entry(s) overlap. OLDORG rates deleted.";
			AssertContainsLog(expectedLog, 1, ratingHeaderPK1, factory);
			expectedLog = "Merging OLDORG with NEWORG has resulted in 2 Rate Entry(s) overlap. OLDORG rates deleted.";
			AssertContainsLog(expectedLog, 1, ratingHeaderPK2, factory);
		}

		public void TestDeleteOverlappingRateEntriesByOrgAddress()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";

			var oldAddress1 = oldOrgHeader.MainAddress;
			oldAddress1.OA_Address1 = "Address1";
			oldAddress1.OA_Code = "AD1";

			var oldAddress2 = oldOrgHeader.Addresses.AddNew();
			oldAddress2.OA_Address1 = "Address2";
			oldAddress2.OA_Code = "AD2";

			var oldAddress3 = oldOrgHeader.Addresses.AddNew();
			oldAddress3.OA_Address1 = "Address3";
			oldAddress3.OA_Code = "AD3";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "NEWORG";

			var newAddress1 = newOrgHeader.MainAddress;
			newAddress1.OA_Address1 = "Address1";
			newAddress1.OA_Code = "AD1";

			var newAddress2 = newOrgHeader.Addresses.AddNew();
			newAddress2.OA_Address1 = "Address2";
			newAddress2.OA_Code = "AD2";

			factory.Save();

			var ratingHeaderPK = Guid.NewGuid();

			InsertRatingHeader(ratingHeaderPK, newOrgHeader.PK.ToGuid());

			var today = DateTime.Today;

			var oldPk1 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-5), today.AddMonths(-4), oldOrgHeader, oldOrgHeader, oldAddress1, oldAddress1);
			var newPk1 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-5), today.AddMonths(-4), newOrgHeader, newOrgHeader, newAddress1, newAddress1);

			var oldPk2 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-3), today.AddMonths(-2), oldOrgHeader, null, oldAddress1, null);
			var newPk2 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-3), today.AddMonths(-2), newOrgHeader, null, newAddress1, null);
			var oldPk3 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-3), today.AddMonths(-2), oldOrgHeader, null, oldAddress2, null);
			var newPk3 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-3), today.AddMonths(-2), newOrgHeader, null, newAddress2, null);

			var oldPk4 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-1), today.AddMonths(0), null, oldOrgHeader, null, oldAddress1);
			var newPk4 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-1), today.AddMonths(0), null, newOrgHeader, null, newAddress1);
			var oldPk5 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-1), today.AddMonths(0), null, oldOrgHeader, null, oldAddress2);
			var newPk5 = InsertRateEntry(ratingHeaderPK, today.AddMonths(-1), today.AddMonths(0), null, newOrgHeader, null, newAddress2);

			var oldPk6 = InsertRateEntry(ratingHeaderPK, today.AddMonths(1), today.AddMonths(2), oldOrgHeader, oldOrgHeader, oldAddress1, oldAddress2);
			var oldPk7 = InsertRateEntry(ratingHeaderPK, today.AddMonths(1), today.AddMonths(2), oldOrgHeader, oldOrgHeader, oldAddress2, oldAddress3);
			var oldPk8 = InsertRateEntry(ratingHeaderPK, today.AddMonths(1), today.AddMonths(2), oldOrgHeader, oldOrgHeader, oldAddress3, oldAddress1);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);

			// Merge old address 1 to new address 1 - creates 3 duplicates
			// Merge old address 2 to new address 2 - creates 2 duplicates
			// Add old address 3 - no change in rate entry address PK
			var mergeSequence = mergeOrgAddressCollection.Cast<MergeOrgAddress>();
			var merge1 = mergeSequence.Single(x => x.OldAddressPK == oldAddress1.PK);
			var merge2 = mergeSequence.Single(x => x.OldAddressPK == oldAddress2.PK);
			var merge3 = mergeSequence.Single(x => x.OldAddressPK == oldAddress3.PK);
			merge1.Action = MergeOrgAddress.ActionMerge;
			merge1.NewAddressPK = newAddress1.PK;
			merge2.Action = MergeOrgAddress.ActionMerge;
			merge2.NewAddressPK = newAddress2.PK;
			merge3.Action = MergeOrgAddress.ActionAdd;

			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var rateEntryDataTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			var rows = rateEntryDataTable.Select($"TI_TH = '{ratingHeaderPK}'");
			AssertEquals("Duplicate should have been deleted", 8, rows.Length);
			var pksRemaining = rows.Select(row => (Guid)row["TI_PK"]).ToList();
			AssertContainsExactElementsInAnyOrder(new[] { newPk1, newPk2, newPk3, newPk4, newPk5, oldPk6, oldPk7, oldPk8 }, pksRemaining);
			var rowOldPk6 = rows.Single(row => (Guid)row["TI_PK"] == oldPk6);
			var rowOldPk7 = rows.Single(row => (Guid)row["TI_PK"] == oldPk7);
			var rowOldPk8 = rows.Single(row => (Guid)row["TI_PK"] == oldPk8);
			AssertEquals("pickup addr 1 updated", newAddress1.PK.ToGuid(), (Guid)rowOldPk6["TI_OA_CartagePickupAddressOverride"]);
			AssertEquals("pickup addr 2 updated", newAddress2.PK.ToGuid(), (Guid)rowOldPk7["TI_OA_CartagePickupAddressOverride"]);
			AssertEquals("pickup addr 3 same", oldAddress3.PK.ToGuid(), (Guid)rowOldPk8["TI_OA_CartagePickupAddressOverride"]);
			AssertEquals("delivery addr 1 updated", newAddress1.PK.ToGuid(), (Guid)rowOldPk8["TI_OA_CartageDeliveryAddressOverride"]);
			AssertEquals("delivery addr 2 updated", newAddress2.PK.ToGuid(), (Guid)rowOldPk6["TI_OA_CartageDeliveryAddressOverride"]);
			AssertEquals("delivery addr 3 same", oldAddress3.PK.ToGuid(), (Guid)rowOldPk7["TI_OA_CartageDeliveryAddressOverride"]);

			var expectedLog = "Merging OLDORG with NEWORG has resulted in 5 Rate Entry(s) overlap. OLDORG rates deleted.";
			AssertContainsLog(expectedLog, 1, ratingHeaderPK, factory);
		}

		public void TestDeleteOverlappingRateEntriesByOrgReferencesAndRatingHeader()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var oldRatingHeaderPK = Guid.NewGuid();
			var newRatingHeaderPK = Guid.NewGuid();

			InsertRatingHeader(oldRatingHeaderPK, oldOrgHeader.PK.ToGuid());
			InsertRatingHeader(newRatingHeaderPK, newOrgHeader.PK.ToGuid());

			InsertRateEntry(oldRatingHeaderPK, DateTime.Today, DateTime.Today.AddMonths(3), oldOrgHeader.PK);
			InsertRateEntry(oldRatingHeaderPK, DateTime.Today, DateTime.Today.AddMonths(3), newOrgHeader.PK);
			InsertRateEntry(newRatingHeaderPK, DateTime.Today, DateTime.Today.AddMonths(3), newOrgHeader.PK);

			var ratingHeaderDataTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			var rateEntryDataTable = LoadDataTable(RateEntrySchema.Constants.TableName);

			AssertEquals("Pre-condition", 2, ratingHeaderDataTable.Select().Length);
			AssertEquals("Pre-condition", 3, rateEntryDataTable.Select().Length);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			ratingHeaderDataTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			rateEntryDataTable = LoadDataTable(RateEntrySchema.Constants.TableName);

			AssertEquals(0, ratingHeaderDataTable.Select($"{RatingHeaderSchema.Constants.PK} = '{oldRatingHeaderPK}'").Length);
			AssertEquals(1, ratingHeaderDataTable.Select($"{RatingHeaderSchema.Constants.PK} = '{newRatingHeaderPK}'").Length);
			AssertEquals(0, rateEntryDataTable.Select($"TI_TH = '{oldRatingHeaderPK}'").Length);
			AssertEquals(1, rateEntryDataTable.Select($"TI_TH = '{newRatingHeaderPK}'").Length);

			var expectedLog = "Merging OLDORG with NEWORG has resulted in 1 Rate Entry(s) overlap. OLDORG rates deleted.";
			AssertContainsLog(expectedLog, 2, newRatingHeaderPK, factory);
		}

		public void TestDeleteOverlappingRateEntriesByOrgReferencesWithMultipleColumnsMatching()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var ratingHeaderPK = Guid.NewGuid();

			InsertRatingHeader(ratingHeaderPK, newOrgHeader.PK.ToGuid());

			var date = DateTime.Today;
			var oldPK = oldOrgHeader.PK.ToGuid();
			var newPK = newOrgHeader.PK.ToGuid();
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK, date, oldPK, oldPK, oldPK);
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK, date, oldPK, oldPK, newPK);
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK, date, oldPK, newPK, oldPK);
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK, date, newPK, oldPK, oldPK);

			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK, date, oldPK, newPK, newPK);
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK, date, newPK, oldPK, newPK);
			InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK, date, newPK, newPK, oldPK);
			var expectedEntryPK = InsertRateEntryWithMultipleOrgReferenceColumns(ratingHeaderPK, date, newPK, newPK, newPK);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var ratingHeaderDataTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			var rateEntryDataTable = LoadDataTable(RateEntrySchema.Constants.TableName);

			AssertEquals("There can be only one", 1, rateEntryDataTable.Rows.Count);
			AssertEquals(1, rateEntryDataTable.Select($"TI_PK = '{expectedEntryPK}'").Length);

			var expectedLog = "Merging OLDORG with NEWORG has resulted in 7 Rate Entry(s) overlap. OLDORG rates deleted.";
			AssertContainsLog(expectedLog, 1, ratingHeaderPK, factory);
		}

		public void TestDeleteOverlappingRateEntriesWithPreExistingBadData_NonOverlappingDates()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var ratingHeaderPK = Guid.NewGuid();
			InsertRatingHeader(ratingHeaderPK, newOrgHeader.PK.ToGuid());

			var today = DateTime.Today.AddDays(30);
			var oldOrgPK = oldOrgHeader.PK.ToGuid();

			var disableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_CheckNoRateEntryOverlaps' AND Object_Name(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry DISABLE TRIGGER TG_CheckNoRateEntryOverlaps";

			Db.Connection.ExecuteNonQuery(disableTriggerSql);

			InsertRateEntry(ratingHeaderPK, today, today.AddDays(6), oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today.AddDays(7), today.AddDays(13), oldOrgPK);

			var enableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_CheckNoRateEntryOverlaps' AND Object_Name(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry ENABLE TRIGGER TG_CheckNoRateEntryOverlaps";
			Db.Connection.ExecuteNonQuery(enableTriggerSql);

			var rateEntryTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			AssertEquals("Pre-condition", 2, rateEntryTable.Rows.Count);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var ratingHeaderTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			rateEntryTable = LoadDataTable(RateEntrySchema.Constants.TableName);

			AssertEquals("Don't overlap, should not be purged", 2, rateEntryTable.Select($"TI_TH = '{ratingHeaderPK}'").Length);
		}

		public void TestDeleteOverlappingRateEntriesWithPreExistingBadData_NewOrgClientRate_TwoRatesOverlap()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var ratingHeaderPK = Guid.NewGuid();
			InsertRatingHeader(ratingHeaderPK, newOrgHeader.PK.ToGuid());

			var today = DateTime.Today.AddDays(30);
			var oldOrgPK = oldOrgHeader.PK.ToGuid();

			var disableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_CheckNoRateEntryOverlaps' AND Object_Name(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry DISABLE TRIGGER TG_CheckNoRateEntryOverlaps";

			Db.Connection.ExecuteNonQuery(disableTriggerSql);

			InsertRateEntry(ratingHeaderPK, today, today, oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today, today, oldOrgPK);

			var enableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_CheckNoRateEntryOverlaps' AND Object_Name(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry ENABLE TRIGGER TG_CheckNoRateEntryOverlaps";
			Db.Connection.ExecuteNonQuery(enableTriggerSql);

			var rateEntryTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			AssertEquals("Pre-condition: overlapping rates should exist before merging rates", 2, rateEntryTable.Rows.Count);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var ratingHeaderTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			rateEntryTable = LoadDataTable(RateEntrySchema.Constants.TableName);

			CombineAssertions(() =>
			{
				AssertEquals("One of the overlapping rates and the 2 non overlapping rates should have been moved", 1, rateEntryTable.Select($"TI_TH = '{ratingHeaderPK}'").Length);
				AssertEquals("No rates should remain on old rating header at the end of the merge", 0, rateEntryTable.Select($"TI_OH_Consignor = '{oldOrgPK}'").Length);

				var expectedLog = "Merging OLDORG with NEWORG has resulted in 1 Rate Entry(s) overlap. OLDORG rates deleted.";
				AssertContainsLog(expectedLog, 1, ratingHeaderPK, factory);
			});
		}

		//test again with the same rate key but doesn't overlap

		public void TestDeleteOverlappingRateEntriesWithPreExistingBadData_NewOrgClientRate_MoreThanTwoRatesOverlap()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var ratingHeaderPK = Guid.NewGuid();
			InsertRatingHeader(ratingHeaderPK, newOrgHeader.PK.ToGuid());

			var today = DateTime.Today.AddDays(30);
			var oldOrgPK = oldOrgHeader.PK.ToGuid();

			var disableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_CheckNoRateEntryOverlaps' AND Object_Name(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry DISABLE TRIGGER TG_CheckNoRateEntryOverlaps";

			Db.Connection.ExecuteNonQuery(disableTriggerSql);

			InsertRateEntry(ratingHeaderPK, today.AddDays(-30), today, oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today, today, oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today, today, oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today, today, oldOrgPK);

			var enableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_CheckNoRateEntryOverlaps' AND Object_Name(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry ENABLE TRIGGER TG_CheckNoRateEntryOverlaps";
			Db.Connection.ExecuteNonQuery(enableTriggerSql);

			var rateEntryTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			AssertEquals("Pre-condition: overlapping rates should exist before merging rates", 4, rateEntryTable.Rows.Count);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var ratingHeaderTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			rateEntryTable = LoadDataTable(RateEntrySchema.Constants.TableName);

			CombineAssertions(() =>
			{
				AssertEquals("One of the overlapping rates and the 2 non overlapping rates should have been moved", 1, rateEntryTable.Select($"TI_TH = '{ratingHeaderPK}'").Length);
				AssertEquals("No rates should remain on old rating header at the end of the merge", 0, rateEntryTable.Select($"TI_OH_Consignor = '{oldOrgPK}'").Length);

				var expectedLog = "Merging OLDORG with NEWORG has resulted in 3 Rate Entry(s) overlap. OLDORG rates deleted.";
				AssertContainsLog(expectedLog, 1, ratingHeaderPK, factory);
			});
		}

		public void TestDeleteOverlappingRateEntriesWithPreExistingBadData_NewOrgClientRate_MoreThanTwoRatesOverlap_PlusOneNonOverlappingRate()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var ratingHeaderPK = Guid.NewGuid();
			InsertRatingHeader(ratingHeaderPK, newOrgHeader.PK.ToGuid());

			var today = DateTime.Today.AddDays(30);
			var oldOrgPK = oldOrgHeader.PK.ToGuid();

			var disableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_CheckNoRateEntryOverlaps' AND Object_Name(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry DISABLE TRIGGER TG_CheckNoRateEntryOverlaps";

			Db.Connection.ExecuteNonQuery(disableTriggerSql);

			InsertRateEntry(ratingHeaderPK, today.AddDays(-30), today, oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today, today, oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today, today, oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today, today, oldOrgPK);
			InsertRateEntry(ratingHeaderPK, today.AddMonths(2), today.AddMonths(6), oldOrgPK);

			var enableTriggerSql = @"
IF EXISTS (SELECT null FROM sys.triggers WHERE Name = 'TG_CheckNoRateEntryOverlaps' AND Object_Name(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry ENABLE TRIGGER TG_CheckNoRateEntryOverlaps";
			Db.Connection.ExecuteNonQuery(enableTriggerSql);

			var rateEntryTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			AssertEquals("Pre-condition: overlapping rates should exist before merging rates", 5, rateEntryTable.Rows.Count);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			var ratingHeaderTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			rateEntryTable = LoadDataTable(RateEntrySchema.Constants.TableName);

			AssertEquals("One of the overlapping rates and the 2 non overlapping rates should have been moved", 2, rateEntryTable.Select($"TI_TH = '{ratingHeaderPK}'").Length);

			var expectedLog = "Merging OLDORG with NEWORG has resulted in 3 Rate Entry(s) overlap. OLDORG rates deleted.";
			AssertContainsLog(expectedLog, 1, ratingHeaderPK, factory);
		}

		public void TestDeleteOverlappingRateEntriesWithPreExistingBadData_OldOrgClientRate()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "OLDORG";
			newOrgHeader.OH_Code = "NEWORG";

			factory.Save();

			var disableTriggerSql = @"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE NAME = 'TG_CheckNoRateEntryOverlaps' AND OBJECT_NAME(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry DISABLE TRIGGER TG_CheckNoRateEntryOverlaps";

			Db.Connection.ExecuteNonQuery(disableTriggerSql);

			var oldRatingHeaderPK = Guid.NewGuid();
			var newRatingHeaderPK = Guid.NewGuid();
			InsertRatingHeader(oldRatingHeaderPK, oldOrgHeader.PK.ToGuid());
			InsertRatingHeader(newRatingHeaderPK, newOrgHeader.PK.ToGuid());

			var date = DateTime.Today.AddDays(30);
			var newPK = newOrgHeader.PK.ToGuid();
			InsertRateEntry(oldRatingHeaderPK, date.AddDays(-20), date);
			InsertRateEntry(oldRatingHeaderPK, date, date);
			InsertRateEntry(oldRatingHeaderPK, date, date);
			var rateEntryWithDifferentDateRange = InsertRateEntry(oldRatingHeaderPK, date.AddDays(50), date.AddDays(60));
			var rateEntryWithDifferentRateKey = InsertRateEntryWithMultipleOrgReferenceColumns(oldRatingHeaderPK, date, newPK, newPK, newPK);

			var enableTriggerSql = @"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE NAME = 'TG_CheckNoRateEntryOverlaps' AND OBJECT_NAME(parent_id) = 'RateEntry')
	ALTER TABLE dbo.RateEntry ENABLE TRIGGER TG_CheckNoRateEntryOverlaps";

			Db.Connection.ExecuteNonQuery(enableTriggerSql);

			var rateEntry = LoadDataTable(RateEntrySchema.Constants.TableName);
			AssertEquals("Pre-condition: rates should exist before merging rates", 5, rateEntry.Rows.Count);

			var mergeOrgAddressCollection = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			var mergeOrgContactCollection = new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, mergeOrgAddressCollection, mergeOrgContactCollection);
			testMerger.Save();

			CombineAssertions(() =>
			{
				var ratingHeader = LoadDataTable(RatingHeaderSchema.Constants.TableName);
				AssertEquals("Pre-condition: OldRatingHeader should be deleted", 0, ratingHeader.Select($"{RatingHeaderSchema.Constants.PK} = '{oldRatingHeaderPK}'").Length);

				rateEntry = LoadDataTable(RateEntrySchema.Constants.TableName);
				AssertEquals("No rates should remain on old rating header at the end of the merge", 0, rateEntry.Select($"TI_TH = '{oldRatingHeaderPK}'").Length);
				AssertEquals("One of the overlapping rates and the 2 non overlapping rates should have been moved", 3, rateEntry.Select($"TI_TH = '{newRatingHeaderPK}'").Length);
				AssertEquals("Has date range despite same rate key", 1, rateEntry.Select($"TI_PK = '{rateEntryWithDifferentDateRange}'").Length);
				AssertEquals("Has different rate key so should not be considered overlapping", 1, rateEntry.Select($"TI_PK = '{rateEntryWithDifferentRateKey}'").Length);

				var expectedLog = "Merging OLDORG with NEWORG has resulted in 2 Rate Entry(s) overlap. OLDORG rates deleted.";
				AssertContainsLog(expectedLog, 1, newRatingHeaderPK, factory);
			});
		}

		void AssertContainsLog(string expectedLog, int expectedCount, Guid newRatingHeaderPK, BusinessObjectFactory factory)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, newRatingHeaderPK);
			query.AddToFilter(StmALogSchema.SL_Table, RatingHeaderSchema.Constants.TableName);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.DeletedARecordInTheSystem.Code);

			var results = factory.Load<StmALog>(query);

			AssertEquals("Expected Log to have been created", expectedCount, results.Length);
			AssertEquals(expectedLog, results[0].SL_Reference);
			AssertNotEquals(ZDateTime.Empty, results[0].SL_EventTime);
			AssertNotEquals(ZDateTime.Empty, results[0].SL_PostedTimeUtc);
		}

		public void TestDeleteOverlappingRateEntriesByColumn_TI_OHColumns_AllAreAccountedFor()
		{
			var columnsToCheck = new List<string>();
			var columnsToCheckSql = @"SELECT column_name FROM information_schema.columns WHERE table_name = 'RateEntry' AND data_type = 'uniqueidentifier' AND column_name LIKE 'TI_OH%'";
			using (var cmd = GetCommandOnMainConnection(columnsToCheckSql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					columnsToCheck.Add(reader.GetString(0));
				}
			}

			var expectedColumnsToSkip = new[]
			{
				RateEntrySchema.Constants.TI_OH_AgentOverride,
			};

			var expectedColumnsToCheck = new[]
			{
				RateEntrySchema.Constants.TI_OH_TransportProvider,
				RateEntrySchema.Constants.TI_OH_Supplier,
				RateEntrySchema.Constants.TI_OH_Consignor,
				RateEntrySchema.Constants.TI_OH_Consignee,
				RateEntrySchema.Constants.TI_OH_ControllingCustomer,
			};

			var result = columnsToCheck.Any() && columnsToCheck.All(c => expectedColumnsToCheck.Contains(c) || expectedColumnsToSkip.Contains(c));
			Assert("If you've added a new org header foreign key column, please ensure it's handled by DeleteOverlappingRateEntriesByColumn", result);
		}

		public void TestDeleteOverlappingRateEntriesByColumn_TI_OAColumns_AllAreAccountedFor()
		{
			var columnsToCheck = new List<string>();
			var columnsToCheckSql = @"SELECT column_name FROM information_schema.columns WHERE table_name = 'RateEntry' AND data_type = 'uniqueidentifier' AND column_name LIKE 'TI_OA%'";
			using (var cmd = GetCommandOnMainConnection(columnsToCheckSql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					columnsToCheck.Add(reader.GetString(0));
				}
			}

			var expectedColumns = new[]
			{
				RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride,
				RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride,
			};

			var result = columnsToCheck.All(c => expectedColumns.Contains(c));
			Assert("If you've added a new org address foreign key column, please ensure it's handled by DeleteOverlappingRateEntriesByColumn", result);
		}

		public void TestMergeRatingHeader_UpdatesForeignKeys()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "~zzz1~";
			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "2";

			factory.Save();

			Guid th1 = Guid.NewGuid();
			Guid th2 = Guid.NewGuid();

			InsertRatingHeader(th1, oldOrgHeader.PK.ToGuid());
			InsertRatingHeader(th2, newOrgHeader.PK.ToGuid());

			InsertRateEntry(th1, DateTime.Today.AddMonths(-6), DateTime.Today.AddMonths(-3));
			InsertRateEntry(th2, DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(-1));

			InsertRateAttachment(th1);
			InsertRateAttachment(th2);

			InsertRateElement("RateOneOffShipment", "TT", th1);
			InsertRateElement("RateOneOffShipment", "TT", th2);

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader), new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader));
			testMerger.Save();

			DataTable ratingHeaderDataTable = LoadDataTable(RatingHeaderSchema.Constants.TableName);
			DataTable rateEntryDataTable = LoadDataTable(RateEntrySchema.Constants.TableName);
			DataTable rateAttachmentDataTable = LoadDataTable(RateAttachmentSchema.Constants.TableName);
			DataTable rateOneOffShipmentDataTable = LoadDataTable(RateOneOffShipmentSchema.Constants.TableName);

			AssertEquals(1, ratingHeaderDataTable.Select($"{RatingHeaderSchema.Constants.TH_OH} = '{newOrgHeader.PK}'").Length);
			AssertEquals(0, ratingHeaderDataTable.Select($"{RatingHeaderSchema.Constants.TH_OH} = '{oldOrgHeader.PK}'").Length);
			AssertEquals("2 RateEntries should have new org as FK", 2, rateEntryDataTable.Select("TI_TH = '" + th2.ToString() + "'").Length);
			AssertEquals("0 RateEntries should have old org as FK", 0, rateEntryDataTable.Select("TI_TH = '" + th1.ToString() + "'").Length);
			AssertEquals("2 RateAttachments should have new org as FK", 2, rateAttachmentDataTable.Select("TA_TH = '" + th2.ToString() + "'").Length);
			AssertEquals("0 RateAttachments should have old org as FK", 0, rateAttachmentDataTable.Select("TA_TH = '" + th1.ToString() + "'").Length);
			AssertEquals("2 RateOneOffShipments should have new org as FK", 2, rateOneOffShipmentDataTable.Select("TT_TH = '" + th2.ToString() + "'").Length);
			AssertEquals("0 RateOneOffShipments should have old org as FK", 0, rateOneOffShipmentDataTable.Select("TT_TH = '" + th1.ToString() + "'").Length);
		}

		void InsertRatingHeader(Guid pk, Guid fk, string rateType = "SAL")
		{
			string cmdText = @"INSERT INTO dbo.RatingHeader
								 (TH_PK, TH_GlobalRateLevel, TH_RateType, TH_GC, TH_OH, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
							   VALUES
								 (@TH_PK, @TH_GlobalRateLevel, @TH_RateType, @TH_GC, @TH_OH, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (DbCommand cmd = Db.Connection.Command(cmdText)) // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			{
				cmd.AddParameterBasedOnDbColumn("@TH_PK", pk, RatingHeaderSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@TH_GlobalRateLevel", 0, RatingHeaderSchema.TH_GlobalRateLevel);
				cmd.AddParameterBasedOnDbColumn("@TH_RateType", rateType, RatingHeaderSchema.TH_RateType);
				cmd.AddParameterBasedOnDbColumn("@TH_GC", GlbCompany.CurrentCompany.PK.ToGuid(),
					RatingHeaderSchema.TH_GC);
				cmd.AddParameterBasedOnDbColumn("@TH_OH", fk, RatingHeaderSchema.TH_OH);

				cmd.ExecuteNonQuery();
			}
		}

		void InsertRateElement(string table, string prefix, Guid fk, Guid? pk = null)
		{
			var newPk = pk.HasValue ? $"'{pk}'" : "newid()";
			string sql = $"insert into {table} ({prefix}_PK, {prefix}_TH, {prefix}_SystemLastEditTimeUtc, {prefix}_SystemLastEditUser, {prefix}_SystemCreateTimeUtc, {prefix}_SystemCreateUser) values ({newPk},'{fk}', GetUtcDate(), '{GlbStaff.CurrentUser.GS_Code}', GetUtcDate(), '{GlbStaff.CurrentUser.GS_Code}')";
			using (DbCommand cmd = Db.Connection.Command(sql)) // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			{
				cmd.ExecuteNonQuery();
			}
		}

		Guid InsertRateEntry(Guid headerPK, DateTime entryStartDate, DateTime entryEndDate)
		{
			var sql = @"INSERT INTO dbo.RateEntry
						  (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateEndDate, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
						VALUES
						  (@TI_PK, @TI_TH, @TI_GC_Publisher, @TI_RateStartDate, @TI_RateEndDate, @TI_RateCategory, @TI_Mode, @TI_OriginLRC, @TI_SystemLastEditTimeUtc, @TI_SystemLastEditUser, @TI_SystemCreateTimeUtc, @TI_SystemCreateUser)";
			using (var cmd = Db.Connection.Command(sql)) // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			{
				var rateEntryPK = Guid.NewGuid();
				cmd.AddParameterBasedOnDbColumn("@TI_PK", rateEntryPK, RateEntrySchema.PK);
				cmd.AddParameterBasedOnDbColumn("@TI_TH", headerPK, RateEntrySchema.TI_TH);
				cmd.AddParameterBasedOnDbColumn("@TI_GC_Publisher", GlbCompany.CurrentCompany.PK.ToGuid(),
					RateEntrySchema.TI_GC_Publisher);
				cmd.AddParameterBasedOnDbColumn("@TI_RateStartDate", entryStartDate, RateEntrySchema.TI_RateStartDate);
				cmd.AddParameterBasedOnDbColumn("@TI_RateEndDate", entryEndDate, RateEntrySchema.TI_RateEndDate);
				cmd.AddParameterBasedOnDbColumn("@TI_RateCategory", "AIR", RateEntrySchema.TI_RateCategory);
				cmd.AddParameterBasedOnDbColumn("@TI_Mode", "LSE", RateEntrySchema.TI_Mode);
				cmd.AddParameterBasedOnDbColumn("@TI_OriginLRC", "AU", RateEntrySchema.TI_OriginLRC);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemLastEditTimeUtc", DateTime.UtcNow, RateEntrySchema.TI_SystemLastEditTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateEntrySchema.TI_SystemLastEditUser);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemCreateTimeUtc", DateTime.UtcNow, RateEntrySchema.TI_SystemCreateTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemCreateUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateEntrySchema.TI_SystemCreateUser);

				cmd.ExecuteNonQuery();

				return rateEntryPK;
			}
		}

		void InsertRateEntry(Guid headerPK, DateTime entryStartDate, DateTime entryEndDate, ZGuid consignorPK)
		{
			var sql = @"INSERT INTO dbo.RateEntry
						  (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateEndDate, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_OH_Consignor, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
						VALUES
						  (@TI_PK, @TI_TH, @TI_GC_Publisher, @TI_RateStartDate, @TI_RateEndDate, @TI_RateCategory, @TI_Mode, @TI_OriginLRC, @TI_OH_Consignor, @TI_SystemLastEditTimeUtc, @TI_SystemLastEditUser, @TI_SystemCreateTimeUtc, @TI_SystemCreateUser)";

			using (var cmd = Db.Connection.Command(sql)) // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			{
				var rateEntryPK = Guid.NewGuid();
				cmd.AddParameterBasedOnDbColumn("@TI_PK", rateEntryPK, RateEntrySchema.PK);
				cmd.AddParameterBasedOnDbColumn("@TI_TH", headerPK, RateEntrySchema.TI_TH);
				cmd.AddParameterBasedOnDbColumn("@TI_GC_Publisher", GlbCompany.CurrentCompany.PK.ToGuid(), RateEntrySchema.TI_GC_Publisher);
				cmd.AddParameterBasedOnDbColumn("@TI_RateStartDate", entryStartDate, RateEntrySchema.TI_RateStartDate);
				cmd.AddParameterBasedOnDbColumn("@TI_RateEndDate", entryEndDate, RateEntrySchema.TI_RateEndDate);
				cmd.AddParameterBasedOnDbColumn("@TI_RateCategory", "AIR", RateEntrySchema.TI_RateCategory);
				cmd.AddParameterBasedOnDbColumn("@TI_Mode", "LSE", RateEntrySchema.TI_Mode);
				cmd.AddParameterBasedOnDbColumn("@TI_OriginLRC", "AU", RateEntrySchema.TI_OriginLRC);
				cmd.AddParameterBasedOnDbColumn("@TI_OH_Consignor", consignorPK.ToGuid(), RateEntrySchema.TI_OH_Consignor);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemLastEditTimeUtc", DateTime.UtcNow, RateEntrySchema.TI_SystemLastEditTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateEntrySchema.TI_SystemLastEditUser);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemCreateTimeUtc", DateTime.UtcNow, RateEntrySchema.TI_SystemCreateTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemCreateUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateEntrySchema.TI_SystemCreateUser);

				cmd.ExecuteNonQuery();
			}
		}

		Guid InsertRateEntry(Guid headerPK, DateTime? entryStartDate, DateTime? entryEndDate, OrgHeader consignor, OrgHeader consignee, OrgAddress pickupAddress, OrgAddress deliveryAddress)
		{
			var sql = @"INSERT INTO dbo.RateEntry
						  (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateEndDate, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_OH_Consignor, TI_OH_Consignee, TI_OA_CartagePickupAddressOverride, TI_OA_CartageDeliveryAddressOverride, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
						VALUES
						  (@TI_PK, @TI_TH, @TI_GC_Publisher, @TI_RateStartDate, @TI_RateEndDate, @TI_RateCategory, @TI_Mode, @TI_OriginLRC, @TI_OH_Consignor, @TI_OH_Consignee, @TI_OA_CartagePickupAddressOverride, @TI_OA_CartageDeliveryAddressOverride, @TI_SystemLastEditTimeUtc, @TI_SystemLastEditUser, @TI_SystemCreateTimeUtc, @TI_SystemCreateUser)";

			using (var cmd = Db.Connection.Command(sql)) // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			{
				var rateEntryPK = Guid.NewGuid();
				cmd.AddParameterBasedOnDbColumn("@TI_PK", rateEntryPK, RateEntrySchema.PK);
				cmd.AddParameterBasedOnDbColumn("@TI_TH", headerPK, RateEntrySchema.TI_TH);
				cmd.AddParameterBasedOnDbColumn("@TI_GC_Publisher", GlbCompany.CurrentCompany.PK.ToGuid(), RateEntrySchema.TI_GC_Publisher);
				cmd.AddParameterBasedOnDbColumn("@TI_RateStartDate", entryStartDate ?? (object)DBNull.Value, RateEntrySchema.TI_RateStartDate);
				cmd.AddParameterBasedOnDbColumn("@TI_RateEndDate", entryEndDate ?? (object)DBNull.Value, RateEntrySchema.TI_RateEndDate);
				cmd.AddParameterBasedOnDbColumn("@TI_RateCategory", "AIR", RateEntrySchema.TI_RateCategory);
				cmd.AddParameterBasedOnDbColumn("@TI_Mode", "LSE", RateEntrySchema.TI_Mode);
				cmd.AddParameterBasedOnDbColumn("@TI_OriginLRC", "AU", RateEntrySchema.TI_OriginLRC);
				cmd.AddParameterBasedOnDbColumn("@TI_OH_Consignor", consignor?.PK.ToGuid() ?? (object)DBNull.Value, RateEntrySchema.TI_OH_Consignor);
				cmd.AddParameterBasedOnDbColumn("@TI_OH_Consignee", consignee?.PK.ToGuid() ?? (object)DBNull.Value, RateEntrySchema.TI_OH_Consignee);
				cmd.AddParameterBasedOnDbColumn("@TI_OA_CartagePickupAddressOverride", pickupAddress?.PK.ToGuid() ?? (object)DBNull.Value, RateEntrySchema.TI_OA_CartagePickupAddressOverride);
				cmd.AddParameterBasedOnDbColumn("@TI_OA_CartageDeliveryAddressOverride", deliveryAddress?.PK.ToGuid() ?? (object)DBNull.Value, RateEntrySchema.TI_OA_CartageDeliveryAddressOverride);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemLastEditTimeUtc", DateTime.UtcNow, RateEntrySchema.TI_SystemLastEditTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateEntrySchema.TI_SystemLastEditUser);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemCreateTimeUtc", DateTime.UtcNow, RateEntrySchema.TI_SystemCreateTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemCreateUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateEntrySchema.TI_SystemCreateUser);

				cmd.ExecuteNonQuery();
				return rateEntryPK;
			}
		}

		Guid InsertRateEntryWithMultipleOrgReferenceColumns(Guid headerPK, DateTime entryStartDate, Guid? supplierPK, Guid? consignorPK, Guid? consigneePK)
		{
			var sql = @"INSERT INTO dbo.RateEntry
							(TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_OH_Supplier, TI_OH_Consignor, TI_OH_Consignee, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
						VALUES
							(@TI_PK, @TI_TH, @TI_GC_Publisher, @TI_RateStartDate, @TI_RateCategory, @TI_Mode, @TI_OriginLRC, @TI_OH_Supplier, @TI_OH_Consignor, @TI_OH_Consignee, @TI_SystemLastEditTimeUtc, @TI_SystemLastEditUser, @TI_SystemCreateTimeUtc, @TI_SystemCreateUser)";

			using (var cmd = Db.Connection.Command(sql)) // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			{
				var rateEntryPK = Guid.NewGuid();
				cmd.AddParameterBasedOnDbColumn("@TI_PK", rateEntryPK, RateEntrySchema.PK);
				cmd.AddParameterBasedOnDbColumn("@TI_TH", headerPK, RateEntrySchema.TI_TH);
				cmd.AddParameterBasedOnDbColumn("@TI_GC_Publisher", GlbCompany.CurrentCompany.PK.ToGuid(), RateEntrySchema.TI_GC_Publisher);
				cmd.AddParameterBasedOnDbColumn("@TI_RateStartDate", entryStartDate, RateEntrySchema.TI_RateStartDate);
				cmd.AddParameterBasedOnDbColumn("@TI_RateCategory", "AIR", RateEntrySchema.TI_RateCategory);
				cmd.AddParameterBasedOnDbColumn("@TI_Mode", "LSE", RateEntrySchema.TI_Mode);
				cmd.AddParameterBasedOnDbColumn("@TI_OriginLRC", "AU", RateEntrySchema.TI_OriginLRC);
				cmd.AddParameterBasedOnDbColumn("@TI_OH_Supplier", supplierPK ?? (object)DBNull.Value, RateEntrySchema.TI_OH_Supplier);
				cmd.AddParameterBasedOnDbColumn("@TI_OH_Consignor", consignorPK ?? (object)DBNull.Value, RateEntrySchema.TI_OH_Consignor);
				cmd.AddParameterBasedOnDbColumn("@TI_OH_Consignee", consigneePK ?? (object)DBNull.Value, RateEntrySchema.TI_OH_Consignee);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemLastEditTimeUtc", DateTime.UtcNow, RateEntrySchema.TI_SystemLastEditTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateEntrySchema.TI_SystemLastEditUser);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemCreateTimeUtc", DateTime.UtcNow, RateEntrySchema.TI_SystemCreateTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TI_SystemCreateUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateEntrySchema.TI_SystemCreateUser);

				cmd.ExecuteNonQuery();

				return rateEntryPK;
			}
		}

		void InsertRateLine(Guid entryPK)
		{
			var sqlText = ZString.Format("SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_CODE = 'FRT' AND AC_GC = '{0}'", GlbCompany.CurrentCompany.PK);
			var ac = (Guid)Db.Connection.ExecuteScalar(sqlText);

			var rateLinePK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.RateLines
							(TL_PK, TL_TI, TL_RateCalculator, TL_RX_NKCurrency, TL_AC, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser)
						VALUES
							(@TL_PK, @TL_TI, @TL_RateCalculator, @TL_RX_NKCurrency, @TL_AC, @TL_SystemLastEditTimeUtc, @TL_SystemLastEditUser, @TL_SystemCreateTimeUtc, @TL_SystemCreateUser)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@TL_PK", rateLinePK, RateLinesSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@TL_TI", entryPK, RateLinesSchema.TL_TI);
				cmd.AddParameterBasedOnDbColumn("@TL_AC", ac, RateLinesSchema.TL_AC);
				cmd.AddParameterBasedOnDbColumn("@TL_RateCalculator", "FLT", RateLinesSchema.TL_RateCalculator);
				cmd.AddParameterBasedOnDbColumn("@TL_RX_NKCurrency", "AUD", RateLinesSchema.TL_RX_NKCurrency);
				cmd.AddParameterBasedOnDbColumn("@TL_SystemLastEditTimeUtc", DateTime.UtcNow, RateLinesSchema.TL_SystemLastEditTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TL_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateLinesSchema.TL_SystemLastEditUser);
				cmd.AddParameterBasedOnDbColumn("@TL_SystemCreateTimeUtc", DateTime.UtcNow, RateLinesSchema.TL_SystemCreateTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@TL_SystemCreateUser", GlbStaff.CurrentUser.GS_Code.ToString(), RateLinesSchema.TL_SystemCreateUser);

				cmd.ExecuteNonQuery();
			}
		}

		void InsertRateAttachment(Guid fk)
		{
			Guid ts = Guid.NewGuid();
			Guid su;

			using (DbCommand cmd = Db.Connection.Command("select top 1 su_pk from dbo.stmmenuitem where su_businesscontext = 'quotation'")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				su = (Guid)cmd.ExecuteScalar();
			}

			string sql = @"
			insert into dbo.RateAttachmentSet (TS_PK, TS_SU, TS_SystemLastEditTimeUtc, TS_SystemLastEditUser, TS_SystemCreateTimeUtc, TS_SystemCreateUser) values(@TS, @SU, @SystemLastEditTimeUtc, @SystemLastEditUser, @SystemCreateTimeUtc, @SystemCreateUser)
			insert into dbo.RateAttachment (TA_PK, TA_TH, TA_TS, TA_SystemLastEditTimeUtc, TA_SystemLastEditUser, TA_SystemCreateTimeUtc, TA_SystemCreateUser) values (newid(),@FK, @TS, @SystemLastEditTimeUtc, @SystemLastEditUser, @SystemCreateTimeUtc, @SystemCreateUser)";

			using (DbCommand cmd = Db.Connection.Command(sql)) // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			{
				cmd.AddParameter("@TS", SqlDbType.UniqueIdentifier, ts);
				cmd.AddParameter("@SU", SqlDbType.UniqueIdentifier, su);
				cmd.AddParameter("@FK", SqlDbType.UniqueIdentifier, fk);
				cmd.AddParameter("@SystemLastEditTimeUtc", SqlDbType.SmallDateTime, DateTime.UtcNow);
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.AddParameter("@SystemCreateTimeUtc", SqlDbType.SmallDateTime, DateTime.UtcNow);
				cmd.AddParameter("@SystemCreateUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				cmd.ExecuteNonQuery();
			}
		}

		public void TestMergeSubscriptions()
		{
			var factory = new BusinessObjectFactory();

			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			var orgOther = factory.NewWithValidTestData<OrgHeader>();
			orgOld.OH_Code = nameof(orgOld);
			orgNew.OH_Code = nameof(orgNew);
			orgOther.OH_Code = nameof(orgOther);
			var subscriptionOld1 = orgOld.Subscriptions.AddNew();
			var subscriptionOld2 = orgOld.Subscriptions.AddNew();
			var subscriptionOldC = orgOld.Subscriptions.AddNew();
			var subscriptionNew1 = orgNew.Subscriptions.AddNew();
			var subscriptionNewC = orgNew.Subscriptions.AddNew();
			var subscriptionOther = orgOther.Subscriptions.AddNew();

			subscriptionOld1.GCS_MediaCategory = "CT1";
			subscriptionOld2.GCS_MediaCategory = "CT2";
			subscriptionOld2.GCS_MediaType = "MT2";
			subscriptionOldC.GCS_MediaCategory = "CTC";
			subscriptionNew1.GCS_MediaCategory = "CT3";
			subscriptionNewC.GCS_MediaCategory = subscriptionOldC.GCS_MediaCategory;
			subscriptionOther.GCS_MediaCategory = subscriptionOldC.GCS_MediaCategory;

			subscriptionOld1.GCS_IsSubscribed = true;
			subscriptionOld2.GCS_IsSubscribed = false;
			subscriptionOldC.GCS_IsSubscribed = true;
			subscriptionNew1.GCS_IsSubscribed = false;
			subscriptionNewC.GCS_IsSubscribed = false;
			subscriptionOther.GCS_IsSubscribed = true;

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, orgOld, orgNew);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();
			AssertEquals("", mergeOrgHeader.DeleteError);

			factory = new BusinessObjectFactory();

			var subscriptions = factory.Load<OrgHeader>(orgNew.PK).Subscriptions.OrderBy(subscription => subscription.GCS_MediaCategory).ToArray();
			AssertEquals("Should be 4 new subscriptions.", 4, subscriptions.Length);

			AssertSubscription(orgNew.PK, subscriptionOld1, subscriptions[0]);
			AssertSubscription(orgNew.PK, subscriptionOld2, subscriptions[1]);
			AssertSubscription(orgNew.PK, subscriptionNew1, subscriptions[2]);
			AssertSubscription(orgNew.PK, subscriptionNewC, subscriptions[3]);

			subscriptions = factory.Load<OrgHeader>(orgOther.PK).Subscriptions.OrderBy(subscription => subscription.GCS_MediaCategory).ToArray();
			AssertEquals("Should be 1 subscription.", 1, subscriptions.Length);
			AssertSubscription(orgOther.PK, subscriptionOther, subscriptions[0]);

			var query = $"SELECT COUNT([GCS_PK]) FROM [dbo].[GlbCompanyCampaignSubscription] WHERE [GCS_OH]='{orgOld.PK}' OR [GCS_PK]='{subscriptionOldC.PK}'";
			using (var cmd = GetCommandOnMainConnection(query))
			{
				Assert("Should be no old subscriptions.", (int)cmd.ExecuteScalar() == 0);
			}
		}

		static void AssertSubscription(ZGuid orgPk, IGlbCompanyCampaignSubscription expectedSubscription, IGlbCompanyCampaignSubscription actualSubscription)
		{
			AssertEquals(orgPk, actualSubscription.GCS_OH);
			AssertEquals(expectedSubscription.GCS_Email, actualSubscription.GCS_Email);
			AssertEquals(expectedSubscription.GCS_MediaCategory, actualSubscription.GCS_MediaCategory);
			AssertEquals(expectedSubscription.GCS_MediaType, actualSubscription.GCS_MediaType);
			AssertEquals(expectedSubscription.GCS_IsSubscribed, actualSubscription.GCS_IsSubscribed);
		}

		public void TestMoveOrgReferences()
		{
			// Get Organisations to test (OLD and NEW)
			string sqlText = "SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'EDICUS'";
			Guid oldOrgPk;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldOrgPk = (Guid)cmd.ExecuteScalar();
			}
			sqlText = "SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'DEMORG'";
			Guid newOrgPk;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newOrgPk = (Guid)cmd.ExecuteScalar();
			}

			// Insert test OrgCusCode records - 2 linked to OldOrg and 1 linked to NewOrg conflicting with one of OldOrg
			sqlText = string.Format(@"
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode01o', '~01', '{2}', '{0}')
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode02o', '~02', '{2}', '{0}')
				INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (NEWID(), '~OrgMergeTestCusCode01n', '~01', '{2}', '{1}')",
				oldOrgPk.ToString(), newOrgPk.ToString(), GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.ExecuteNonQuery();
			}

			sqlText = string.Format(@"INSERT INTO dbo.OrgColdCallRegister (O1_PK,O1_LeadUniqueReference,O1_CompanyName,O1_ContactName,O1_Address1,O1_Address2,O1_City,O1_State,O1_Phone,O1_Mobile,O1_PostCode
			,O1_Email,O1_Fax,O1_BusinessRegNo,O1_GS_NKRepAssigned,O1_LeadStatus,O1_LeadSource,O1_LeadSourcePerson,O1_OH_SourceOfLead,O1_PortOrCountry,O1_SystemCreateTimeUtc,O1_SystemCreateUser,O1_SystemLastEditTimeUtc,O1_SystemLastEditUser)
	 VALUES
		   (newid(),'I90001000','DEMORG','contact','adr1','adr2','city','state','','','12345','','','123','~BP','CLD','WOM','PostMaster','{0}','AU',GetUtcDate(),'~BP',GetUtcDate(),'~BP')", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.ExecuteNonQuery();
			}

			// Asserts there are Companies and Branches which proxy organisation is OldOrg - BEFORE MOVING
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbCompany WHERE GC_OH_OrgProxy = '{0}'", oldOrgPk.ToString());
			int oldCompanyCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldCompanyCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				Assert("[Pre-condition] Company Count", oldCompanyCountBefore > 0);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.GlbBranch WHERE GB_OH_OrgProxy = '{0}'", oldOrgPk.ToString());
			int oldBranchCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldBranchCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				Assert("[Pre-condition] Branch Count", oldBranchCountBefore > 0);
			}

			// Assert OldOrg has the newly inserted CusCodes - BEFORE MOVING
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgCusCode WHERE OK_OH = '{0}'", oldOrgPk.ToString());
			int oldCusCodeCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldCusCodeCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("[PRE-CONDITION] OldOrg CusCode Count", 2, oldCusCodeCountBefore);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.OrgColdCallRegister WHERE O1_OH_SourceOfLead = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int oldColdCallCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("[PRE-CONDITION] OldOrg ColdCall Count", 1, oldColdCallCountBefore);
			}

			// Gets the number of Companies and Branches which proxy organisation is NewOrg - BEFORE MOVING
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbCompany WHERE GC_OH_OrgProxy = '{0}'", newOrgPk.ToString());
			int newCompanyCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newCompanyCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
			}
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbBranch WHERE GB_OH_OrgProxy = '{0}'", newOrgPk.ToString());
			int newBranchCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newBranchCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
			}

			// Assert NewOrg has the newly inserted CusCode - BEFORE MOVING
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgCusCode WHERE OK_OH = '{0}'", newOrgPk.ToString());
			int newCusCodeCountBefore;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				newCusCodeCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("[PRE-CONDITION] NewOrg CusCode Count", 1, newCusCodeCountBefore);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.OrgColdCallRegister WHERE O1_OH_SourceOfLead = '{0}'", newOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int newColdCallCountBefore = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("[PRE-CONDITION] NewOrg ColdCall Count", 0, newColdCallCountBefore);
			}

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgPk, newOrgPk, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.MoveOrgReferences_Exposed();

			// Asserts there are NO Companies or Branches which proxy organisation is OldOrg - AFTER MOVING
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbCompany WHERE GC_OH_OrgProxy = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int oldCompanyCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg Company Count - AFTER MOVING", 0, oldCompanyCountAfter);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.GlbBranch WHERE GB_OH_OrgProxy = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int oldBranchCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg Branch Count - AFTER MOVING", 0, oldBranchCountAfter);
			}

			// Assert OldOrg has only 1 CusCode. The one that conflicted with a NewOrg CusCode - AFTER MOVING
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgCusCode WHERE OK_OH = '{0}'", oldOrgPk.ToString());
			int oldCusCodeCountAfter;
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				oldCusCodeCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg CusCode Count", 1, oldCusCodeCountAfter);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.OrgColdCallRegister WHERE O1_OH_SourceOfLead = '{0}'", oldOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int oldColdCallCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg ColdCall Count", 0, oldColdCallCountAfter);
			}

			// Asserts number of Companies and Branches which proxy organisation is NewOrg - AFTER MOVING
			sqlText = string.Format("SELECT count(*) FROM dbo.GlbCompany WHERE GC_OH_OrgProxy = '{0}'", newOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int newCompanyCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("NewOrg Company Count - AFTER MOVING", oldCompanyCountBefore + newCompanyCountBefore, newCompanyCountAfter);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.GlbBranch WHERE GB_OH_OrgProxy = '{0}'", newOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int newBranchCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("NewOrg Branch Count - AFTER MOVING", oldBranchCountBefore + newBranchCountBefore, newBranchCountAfter);
			}

			// Assert number of CusCodes which reference NewOrg - AFTER MOVING
			sqlText = string.Format("SELECT count(*) FROM dbo.OrgCusCode WHERE OK_OH = '{0}'", newOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int newCusCodeCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("OldOrg CusCode Count", (oldCusCodeCountBefore - oldCusCodeCountAfter) + newCusCodeCountBefore, newCusCodeCountAfter);
			}

			sqlText = string.Format("SELECT count(*) FROM dbo.OrgColdCallRegister WHERE O1_OH_SourceOfLead = '{0}'", newOrgPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int newColdCallCountAfter = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals("NewOrg ColdCall Count", 1, newColdCallCountAfter);
			}
		}

		public void TestSetOldOrganisationInactive()
		{
			// Create Organisations to test (OLD and NEW)
			Guid oldOrg = Guid.NewGuid();
			Guid newOrg = Guid.NewGuid();

			string sqlText = string.Format(@"
				INSERT dbo.OrgHeader (OH_PK, OH_FullName, OH_Code, OH_IsActive) VALUES ('{0}', '~TestFullName_Old', '~OLD', 1)
				INSERT dbo.OrgHeader (OH_PK, OH_FullName, OH_Code, OH_IsActive) VALUES ('{1}', '~TestFullName_New', '~NEW', 1)",
				oldOrg.ToString(), newOrg.ToString());

			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.ExecuteNonQuery();
			}

			// Asserts OldOrg is active - BEFORE SETTING STATUS
			sqlText = string.Format("SELECT OH_IsActive FROM dbo.OrgHeader WHERE OH_PK = '{0}'", oldOrg.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				var oldOrgIsActiveBefore = cmd.ExecuteScalar();
				AssertEquals("[PRE-CONDITION] IsActive", true, oldOrgIsActiveBefore);
			}

			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrg, newOrg, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			testMerger.SetOldOrganisationInactive_Exposed();

			// Asserts OldOrg is NOT active - AFTER SETTING STATUS
			sqlText = string.Format("SELECT OH_IsActive FROM dbo.OrgHeader WHERE OH_PK = '{0}'", oldOrg.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				var oldOrgIsActiveAfter = cmd.ExecuteScalar();
				AssertEquals("IsActive - AFTER SETTING STATUS", false, oldOrgIsActiveAfter);
			}
		}

		public void TestMergeOrgSalesCall()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "1";
			oldOrgHeader.OH_IsSalesLead = true;
			oldOrgHeader.SalesCalls.AddNew();
			oldOrgHeader.SalesCalls.AddNew();

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "2";
			newOrgHeader.OH_IsSalesLead = true;
			newOrgHeader.SalesCalls.AddNew();
			newOrgHeader.SalesCalls.AddNew();

			factory.Save();

			AssertEquals(2, oldOrgHeader.SalesCalls.Count);
			AssertEquals(2, newOrgHeader.SalesCalls.Count);

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			BusinessObjectFactory assertFactory = new BusinessObjectFactory();
			newOrgHeader = assertFactory.Load<OrgHeader>(newOrgHeader.PK);
			AssertEquals(4, newOrgHeader.SalesCalls.Count);
		}

		public void TestMergeOrgSales()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "1";
			oldOrgHeader.OH_IsSalesLead = true;

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "2";
			newOrgHeader.OH_IsSalesLead = true;

			var sales1 = factory.New<OrgSales>();
			sales1.OW_IsTraded = false;
			sales1.OW_OH_Primary = oldOrgHeader.PK;
			sales1.OW_OH_Buyer = oldOrgHeader.PK;
			sales1.OW_OH_Supplier = oldOrgHeader.PK;

			var detail1 = sales1.TradeDetails.AddNew();

			var period1 = detail1.CurrentProspectPeriod;
			period1.PAS_OH_Client = oldOrgHeader.PK;

			var prospect1 = detail1.ProspectDetail;
			prospect1.PAP_OH_Competitor = oldOrgHeader.PK;
			prospect1.PAP_OH_ControllingAgent = oldOrgHeader.PK;
			prospect1.PAP_OH_ServiceProvider = oldOrgHeader.PK;

			var sales2 = factory.New<OrgSales>();
			sales2.OW_IsTraded = true;
			sales2.OW_OH_Buyer = oldOrgHeader.PK;
			sales2.OW_OH_Supplier = newOrgHeader.PK;

			var detail2 = sales2.TradeDetails.AddNew();

			var period2a = detail2.TradedPeriods.AddNew();
			period2a.PAS_Period = new ZDate(2017, 12, 1);
			period2a.PAS_OH_Client = oldOrgHeader.PK;
			period2a.PAS_IsJobValue = false;

			var tradeValue2a = period2a.TradeValues.AddNew();
			tradeValue2a.PAV_GC = GlbCompany.CurrentCompany.PK;

			var period2b = detail2.TradedPeriods.AddNew();
			period2b.PAS_Period = new ZDate(2017, 12, 1);
			period2b.PAS_OH_Client = newOrgHeader.PK;
			period2b.PAS_IsJobValue = false;

			var tradeValue2b = period2b.TradeValues.AddNew();
			tradeValue2b.PAV_GC = GlbCompany.CurrentCompany.PK;

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			var assertFactory = new BusinessObjectFactory();

			var loadedSales1 = assertFactory.Load<OrgSales>(sales1.PK);
			AssertEquals(newOrgHeader.PK, loadedSales1.OW_OH_Primary);
			AssertEquals(newOrgHeader.PK, loadedSales1.OW_OH_Supplier);
			AssertEquals(newOrgHeader.PK, loadedSales1.OW_OH_Buyer);

			var loadedPeriod1 = assertFactory.Load<OrgTradePeriod>(period1.PK);
			AssertEquals(newOrgHeader.PK, loadedPeriod1.PAS_OH_Client);

			var loadedProspect1 = assertFactory.Load<OrgTradeProspect>(prospect1.PK);
			AssertEquals(newOrgHeader.PK, loadedProspect1.PAP_OH_Competitor);
			AssertEquals(newOrgHeader.PK, loadedProspect1.PAP_OH_ControllingAgent);
			AssertEquals(newOrgHeader.PK, loadedProspect1.PAP_OH_ServiceProvider);

			var loadedPeriod2a = assertFactory.Load<OrgTradePeriod>(period2a.PK);
			AssertNull(loadedPeriod2a);

			var loadedTradeValue2a = assertFactory.Load<OrgTradeValue>(tradeValue2a.PK);
			AssertNull(loadedTradeValue2a);

			var loadedPeriod2b = assertFactory.Load<OrgTradePeriod>(period2b.PK);
			AssertEquals(newOrgHeader.PK, loadedPeriod2b.PAS_OH_Client);

			var loadedTradeValue2b = assertFactory.Load<OrgTradeValue>(tradeValue2b.PK);
			AssertNotNull(loadedTradeValue2b);
		}

		public void TestMergeOrders()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ZGuid pk1 = ZGuid.NewZGuid();
			ZGuid pk2 = ZGuid.NewZGuid();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "1";

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "2";

			factory.Save();

			string sqlText = string.Format(@"
				INSERT dbo.joborderheader (JD_PK, JD_OrderNumber, JD_OA_BuyerAddress, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser) VALUES ('" + pk1.ToString() + @"', '123', '{0}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.joborderheader (JD_PK, JD_OrderNumber, JD_OA_BuyerAddress, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser) VALUES ('" + pk2.ToString() + @"', '456', '{0}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.joborderheader (JD_PK, JD_OrderNumber, JD_OA_BuyerAddress, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser) VALUES (newid(), '666', '{0}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.joborderheader (JD_PK, JD_OrderNumber, JD_OA_BuyerAddress, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser) VALUES (newid(), '123', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.joborderheader (JD_PK, JD_OrderNumber, JD_OA_BuyerAddress, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser) VALUES (newid(), '456', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.joborderheader (JD_PK, JD_OrderNumber, JD_OA_BuyerAddress, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser) VALUES (newid(), '777', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
					oldOrgHeader.MainAddress.PK.ToString(),
					newOrgHeader.MainAddress.PK.ToString());

			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				cmd.ExecuteNonQuery();
			}

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			BusinessObjectFactory assertFactory = new BusinessObjectFactory();

			var assertOldOrgHeader = assertFactory.Load<OrgHeader>(oldOrgHeader.PK);
			AssertNull("Old OrgHeader no longer exists", assertOldOrgHeader);

			var assertNewOrgHeader = assertFactory.Load<OrgHeader>(newOrgHeader.PK);
			var newOrdersQuery = new ZQuery(JobOrderHeaderSchema.JD_OA_BuyerAddress, assertNewOrgHeader.Addresses.Select(x => x.PK));
			BusinessObject[] ordersNew = (BusinessObject[])new BusinessObjectFactory().Load<Enterprise.Integration.Forwarding.IOrder>(newOrdersQuery);

			AssertEquals("all orders should be moved to new org", 6, ordersNew.Length);
			string shouldbe = ",123,456,666,777,1000,1001,";
			bool allPresent = true;
			foreach (BusinessObject order in ordersNew)
			{
				if (shouldbe.IndexOf("," + order[JobOrderHeaderSchema.JD_OrderNumber].ToString() + ",") == -1)
				{
					allPresent = false;
				}
			}
			Assert("not all orders are correct", allPresent);
		}

		public void TestMergeCusBondDetail()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			var oldPK = oldOrgHeader.PK;

			var cusBondDetail = factory.NewWithValidTestData<CusBondDetail>();
			cusBondDetail.PW_ParentID = oldPK;
			cusBondDetail.PW_ParentTableCode = "OH";

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "2";

			factory.Save();

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			var assertFactory = new BusinessObjectFactory();
			var bondDetailOld = assertFactory.Load<CusBondDetail>(new ZQuery(CusBondDetailSchema.PW_ParentID, oldPK));
			var bondDetailNew = assertFactory.Load<CusBondDetail>(new ZQuery(CusBondDetailSchema.PW_ParentID, newOrgHeader.PK));

			AssertEquals("old bond detail should be moved to new org", 0, bondDetailOld.Length);
			AssertEquals("All bond details should be moved to new org", 1, bondDetailNew.Length);
		}

		public void TestMoveOrgContactsWithOrgDocument()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZGuid menu = factory.LoadTop1<StmMenuItem>(new ZQuery() { OrderBy = StmMenuItemSchema.PK.Name }).PK;

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "~zzz1~";

			OrgContact cnt1 = oldOrgHeader.Contacts.AddNew();
			cnt1.OC_ContactName = "match name";
			cnt1.OC_Email = "matchemail@email.com";
			OrgDocument doc1 = cnt1.Documents.AddNew();
			doc1.OD_DocumentGroup = "A/R";
			doc1.OD_DefaultContact = true;

			OrgContact cnt2 = oldOrgHeader.Contacts.AddNew();
			cnt2.OC_ContactName = "blablabla";
			cnt2.OC_Email = "different@email.com";
			OrgDocument doc2 = cnt2.Documents.AddNew();
			doc2.OD_DocumentGroup = "WHS";
			doc2.OD_DefaultContact = true;

			OrgContact cnt3 = oldOrgHeader.Contacts.AddNew();
			cnt3.OC_ContactName = "other man";
			cnt3.OC_Email = "some@email.com";
			OrgDocument doc3 = cnt3.Documents.AddNew();
			doc3.OD_SU_MenuItem = menu;
			doc3.OD_DefaultContact = true;

			OrgContact cnt4 = oldOrgHeader.Contacts.AddNew();
			cnt4.OC_ContactName = "12345";
			cnt4.OC_Email = "12345@email.com";
			OrgDocument doc4 = cnt4.Documents.AddNew();
			doc4.OD_DocumentGroup = "CSV";
			doc4.OD_DefaultContact = true;

			ZGuid oldPK = oldOrgHeader.PK;

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "~zzz2~";

			OrgContact cnt1New = newOrgHeader.Contacts.AddNew();
			cnt1New.OC_ContactName = "match name";
			cnt1New.OC_Email = "matchemail@email.com";
			OrgDocument doc1New = cnt1New.Documents.AddNew();
			doc1New.OD_DocumentGroup = "A/R";
			doc1New.OD_DefaultContact = true;

			OrgContact cnt2New = newOrgHeader.Contacts.AddNew();
			cnt2New.OC_ContactName = "muhaha";
			cnt2New.OC_Email = "muhaha@email.com";
			OrgDocument doc2New = cnt2New.Documents.AddNew();
			doc2New.OD_DocumentGroup = "WHS";
			doc2New.OD_DefaultContact = true;

			OrgContact cnt3New = newOrgHeader.Contacts.AddNew();
			cnt3New.OC_ContactName = "developer";
			cnt3New.OC_Email = "developer@email.com";
			OrgDocument doc3New = cnt3New.Documents.AddNew();
			doc3New.OD_SU_MenuItem = menu;
			doc3New.OD_DefaultContact = true;

			factory.Save();

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.DeleteOldOrg = false;
			testMerger.Save();

			factory = new BusinessObjectFactory();
			cnt1 = factory.Load<OrgContact>(cnt1.PK);
			cnt2 = factory.Load<OrgContact>(cnt2.PK);
			cnt3 = factory.Load<OrgContact>(cnt3.PK);
			cnt4 = factory.Load<OrgContact>(cnt4.PK);
			cnt1New = factory.Load<OrgContact>(cnt1New.PK);
			cnt2New = factory.Load<OrgContact>(cnt2New.PK);
			cnt3New = factory.Load<OrgContact>(cnt3New.PK);

			AssertNotNull(cnt1);
			AssertNotNull(cnt2);
			AssertNotNull(cnt3);
			AssertNotNull(cnt4);
			AssertNotNull(cnt1New);
			AssertNotNull(cnt2New);
			AssertNotNull(cnt3New);

			AssertEquals(oldPK, cnt1.OC_OH);
			AssertEquals(1, cnt2.Documents.Count);
			AssertEquals(false, cnt2.Documents[0].OD_DefaultContact);
			AssertEquals(1, cnt3.Documents.Count);
			AssertEquals(false, cnt3.Documents[0].OD_DefaultContact);
			AssertEquals(1, cnt4.Documents.Count);
			AssertEquals(true, cnt4.Documents[0].OD_DefaultContact);
			AssertEquals(1, cnt1New.Documents.Count);
			AssertEquals(true, cnt1New.Documents[0].OD_DefaultContact);
			AssertEquals(1, cnt2New.Documents.Count);
			AssertEquals(true, cnt2New.Documents[0].OD_DefaultContact);
			AssertEquals(1, cnt3New.Documents.Count);
			AssertEquals(true, cnt3New.Documents[0].OD_DefaultContact);
		}

		public void TestMergeOrgContactDocumentsIfOldOrgHasANotifyPartyAndNewOrgDoesNot()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var oldContact = oldOrgHeader.Contacts.AddNew();
			oldContact.OC_ContactName = "match name1";
			oldContact.OC_Email = "matchmail@email.com";
			CreateDocWithDocGroup(oldContact, "NOT", "EML");

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newContact = newOrgHeader.Contacts.AddNew();
			newContact.OC_ContactName = "match name2";
			newContact.OC_Email = "matchmail@email.com";
			CreateDocWithDocGroup(newContact, "A/R", "FAX");

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertEquals("Precondition: There is 1 Contact to Merge.", 1, testMerger.mergeContactsCollectionForTest.Count);
			AssertEquals("Precondition: The Action is Merge, not Add.", true, testMerger.mergeContactsCollectionForTest.Cast<MergeOrgContact>().All(c => c.Action == "MRG"));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			newContact = factory.Load<OrgContact>(newContact.PK);
			var newContactDocuments = newContact.Documents;

			CombineAssertions("Results after Merge: ", () =>
			{
				AssertEquals("If there is no Notify Party in new Org, move the Notify Party from old Org to new Org.", 2, newContactDocuments.Count);
				AssertContainsExactElementsInAnyOrder(
					new List<(ZString, ZString)>
					{
						("A/R", "FAX"), ("NOT", "EML")
					},
					newContactDocuments.Cast<OrgDocument>().Select(d => (d.OD_DocumentGroup, d.OD_DeliverBy)));
			});
		}

		public void TestMergeOrgContactDocumentsIfBothNewOrgAndOldOrgHaveANotifyParty()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var oldContact1 = oldOrgHeader.Contacts.AddNew();
			oldContact1.OC_ContactName = "match name1";
			oldContact1.OC_Email = "matchmail@email.com";
			CreateDocWithDocGroup(oldContact1, "NOT", "EML");

			var oldContact2 = oldOrgHeader.Contacts.AddNew();
			oldContact2.OC_ContactName = "ABC DEF1";
			oldContact2.OC_Email = "ABCDEF@123.com";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newContact1 = newOrgHeader.Contacts.AddNew();
			newContact1.OC_ContactName = "match name2";
			newContact1.OC_Email = "matchmail@email.com";

			var newContact2 = newOrgHeader.Contacts.AddNew();
			newContact2.OC_ContactName = "ABC DEF2";
			newContact2.OC_Email = "ABCDEF@123.com";
			CreateDocWithDocGroup(newContact2, "NOT", "FAX");

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertEquals("Precondition: There are 2 Contacts to Merge.", 2, testMerger.mergeContactsCollectionForTest.Count);
			AssertEquals("Precondition: Both Actions are Merge, not Add.", true, testMerger.mergeContactsCollectionForTest.Cast<MergeOrgContact>().All(c => c.Action == "MRG"));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			newContact1 = factory.Load<OrgContact>(newContact1.PK);
			newContact2 = factory.Load<OrgContact>(newContact2.PK);
			var newContact1Documents = newContact1.Documents;
			var newContact2Documents = newContact2.Documents;

			CombineAssertions("If there is a Notify Party in new Org, discard the Notify Party in old Org.", () =>
			{
				AssertEquals(0, newContact1Documents.Count);
				AssertEquals(1, newContact2Documents.Count);
				AssertEquals(true, newContact2Documents.Cast<OrgDocument>().Any(d => d.OD_DocumentGroup == "NOT" && d.OD_DeliverBy == "FAX"));
			});
		}

		public void TestDifferentContactMergeDocumentsIfOldOrgHasNoNotifyParty()
		{
			var factory = new BusinessObjectFactory();

			var menuItem1 = factory.NewWithValidTestData<StmMenuItem>();
			var menuItem2 = factory.NewWithValidTestData<StmMenuItem>();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var oldContact1 = oldOrgHeader.Contacts.AddNew();
			oldContact1.OC_ContactName = "match name1";
			oldContact1.OC_Email = "matchmail@email.com";
			CreateDocWithDocGroup(oldContact1, "A/R", "EML", true);
			CreateDocWithDoc(oldContact1, menuItem1.PK, "EML", true);

			var oldContact2 = oldOrgHeader.Contacts.AddNew();
			oldContact2.OC_ContactName = "ABC DEF1";
			oldContact2.OC_Email = "ABCDEF@123.com";
			CreateDocWithDocGroup(oldContact2, "CNE", "EML", true);
			CreateDocWithDoc(oldContact2, menuItem2.PK, "EML", true);

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newContact1 = newOrgHeader.Contacts.AddNew();
			newContact1.OC_ContactName = "match name2";
			newContact1.OC_Email = "matchmail@email.com";
			CreateDocWithDocGroup(newContact1, "A/R", "FAX");
			CreateDocWithDocGroup(newContact1, "CNE", "FAX", true);

			var newContact2 = newOrgHeader.Contacts.AddNew();
			newContact2.OC_ContactName = "ABC DEF2";
			newContact2.OC_Email = "ABCDEF@123.com";
			CreateDocWithDoc(newContact2, menuItem1.PK, "FAX", true);
			CreateDocWithDoc(newContact2, menuItem2.PK, "FAX");

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertEquals("Precondition: There are 2 Contacts to Merge.", 2, testMerger.mergeContactsCollectionForTest.Count);
			AssertEquals("Precondition: Both Actions are Merge, not Add.", true, testMerger.mergeContactsCollectionForTest.Cast<MergeOrgContact>().All(c => c.Action == "MRG"));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			newContact1 = factory.Load<OrgContact>(newContact1.PK);
			newContact2 = factory.Load<OrgContact>(newContact2.PK);
			var newContact1Documents = newContact1.Documents;
			var newContact2Documents = newContact2.Documents;

			CombineAssertions("Results after Merge: ", () =>
			{
				AssertEquals(4, newContact1Documents.Count);
				AssertContainsExactElementsInAnyOrder(
					new List<(ZString, ZString, ZGuid, ZBool)>
					{
						("A/R", "EML", ZGuid.Empty, true), ("A/R", "FAX", ZGuid.Empty, false), ("CNE", "FAX", ZGuid.Empty, true), (string.Empty, "EML", menuItem1.PK, false)
					},
					newContact1Documents.Cast<OrgDocument>().Select(d => (d.OD_DocumentGroup, d.OD_DeliverBy, d.OD_SU_MenuItem, d.OD_DefaultContact)));

				AssertEquals(4, newContact2Documents.Count);
				AssertContainsExactElementsInAnyOrder(
					new List<(ZString, ZString, ZGuid, ZBool)>
					{
						(string.Empty, "FAX", menuItem1.PK, true), (string.Empty, "EML", menuItem2.PK, true), (string.Empty, "FAX", menuItem2.PK, false), ("CNE", "EML", ZGuid.Empty, false)
					},
					newContact2Documents.Cast<OrgDocument>().Select(d => (d.OD_DocumentGroup, d.OD_DeliverBy, d.OD_SU_MenuItem, d.OD_DefaultContact)));
			});
		}

		public void TestSameContactMergeDocumentsDiscardSameDocOrDocGroup()
		{
			var factory = new BusinessObjectFactory();

			var menuItem1 = factory.NewWithValidTestData<StmMenuItem>();
			var menuItem2 = factory.NewWithValidTestData<StmMenuItem>();
			var menuItem3 = factory.NewWithValidTestData<StmMenuItem>();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var oldContact = oldOrgHeader.Contacts.AddNew();
			oldContact.OC_ContactName = "match name";
			oldContact.OC_Email = "matchmail@email.com";
			CreateDocWithDocGroup(oldContact, "A/R", "EML", true);
			CreateDocWithDocGroup(oldContact, "A/P", "EML", true);
			CreateDocWithDocGroup(oldContact, "CNE", "EML", true);
			CreateDocWithDoc(oldContact, menuItem1.PK, "EML", true);
			CreateDocWithDoc(oldContact, menuItem2.PK, "EML", true);
			CreateDocWithDoc(oldContact, menuItem3.PK, "EML", true);

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newContact1 = newOrgHeader.Contacts.AddNew();
			newContact1.OC_ContactName = "match name";
			newContact1.OC_Email = "matchmail@email.com";
			CreateDocWithDocGroup(newContact1, "A/R", "FAX");
			CreateDocWithDoc(newContact1, menuItem1.PK, "FAX");

			var newContact2 = newOrgHeader.Contacts.AddNew();
			newContact2.OC_ContactName = "AAA BBB";
			newContact2.OC_Email = "AAA@BBB.com";
			CreateDocWithDocGroup(newContact2, "CNE", "FAX", true);
			CreateDocWithDoc(newContact2, menuItem3.PK, "FAX", true);

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertEquals("Precondition: There is 1 Contact to Merge.", 1, testMerger.mergeContactsCollectionForTest.Count);
			AssertEquals("Precondition: The Actions is Merge, not Add.", true, testMerger.mergeContactsCollectionForTest.Cast<MergeOrgContact>().All(c => c.Action == "MRG"));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			newContact1 = factory.Load<OrgContact>(newContact1.PK);
			newContact2 = factory.Load<OrgContact>(newContact2.PK);
			var newContact1Documents = newContact1.Documents;
			var newContact2Documents = newContact2.Documents;

			CombineAssertions("Results after Merge: ", () =>
			{
				AssertEquals("For Same Contact, discard the same Document/Document Group items from dissolved Org.", 6, newContact1Documents.Count);
				AssertContainsExactElementsInAnyOrder(
					new List<(ZString, ZString, ZGuid, ZBool)>
					{
						("A/R",  "FAX", ZGuid.Empty, false), ("A/P", "EML", ZGuid.Empty, true), ("CNE", "EML", ZGuid.Empty, false), (string.Empty, "FAX", menuItem1.PK, false), (string.Empty, "EML", menuItem2.PK, true), (string.Empty, "EML", menuItem3.PK, false)
					},
					newContact1Documents.Cast<OrgDocument>().Select(d => (d.OD_DocumentGroup, d.OD_DeliverBy, d.OD_SU_MenuItem, d.OD_DefaultContact)));

				AssertEquals("Remains the same as there is no Contact merged into it.", 2, newContact2Documents.Count);
				AssertContainsExactElementsInAnyOrder(
					new List<(ZString, ZString, ZGuid, ZBool)>
					{
						("CNE", "FAX", ZGuid.Empty, true), (string.Empty, "FAX", menuItem3.PK, true)
					},
					newContact2Documents.Cast<OrgDocument>().Select(d => (d.OD_DocumentGroup, d.OD_DeliverBy, d.OD_SU_MenuItem, d.OD_DefaultContact)));
			});
		}

		void CreateDocWithDocGroup(OrgContact orgContact, string group, string deliverBy, bool isDefaultContact = false)
		{
			var doc = orgContact.Documents.AddNew();
			doc.OD_DocumentGroup = group;
			doc.OD_DeliverBy = deliverBy;
			doc.OD_DefaultContact = isDefaultContact;
		}

		void CreateDocWithDoc(OrgContact orgContact, ZGuid menuItem, string deliverBy, bool isDefaultContact = false)
		{
			var doc = orgContact.Documents.AddNew();
			doc.OD_DocumentGroup = string.Empty;
			doc.OD_SU_MenuItem = menuItem;
			doc.OD_DeliverBy = deliverBy;
			doc.OD_DefaultContact = isDefaultContact;
		}

		public void TestMergeOrgContactWithCollectionNote()
		{
			#region Test data

			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var oldOrg = factory.NewWithValidTestData<OrgHeader>();
			var newOrg = factory.NewWithValidTestData<OrgHeader>();

			#region Contact A

			var contactOldA = oldOrg.Contacts.AddNew();
			contactOldA.OC_ContactName = "Andrew";
			var collectionNoteOldA = oldOrg.CollectionNotes.AddNew();
			collectionNoteOldA.PN_OC = contactOldA.PK;
			collectionNoteOldA.PN_CallDetailNote = "111";

			var contactNewA = newOrg.Contacts.AddNew();
			contactNewA.OC_ContactName = "Andrew";
			var collectionNoteNewA = newOrg.CollectionNotes.AddNew();
			collectionNoteNewA.PN_OC = contactNewA.PK;
			collectionNoteNewA.PN_CallDetailNote = "222";

			#endregion

			#region Contact B

			var contactOldB = oldOrg.Contacts.AddNew();
			contactOldB.OC_ContactName = "Brian";
			var collectionNoteOldB = oldOrg.CollectionNotes.AddNew();
			collectionNoteOldB.PN_OC = contactOldB.PK;
			collectionNoteOldB.PN_CallDetailNote = "333";

			#endregion

			factory.Save();

			#endregion

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrg, newOrg);
			var merger = new OrganisationMergerForTest(mergeOrgHeader);
			merger.Save();

			factory = new BusinessObjectFactory();

			AssertNull("Old org should be deleted", factory.Load<OrgHeader>(oldOrg.PK));
			AssertNull("Old contact A should be deleted", factory.Load<OrgContact>(contactOldA.PK));
			AssertEquals("Old contact B should be moved to new org", newOrg.PK, factory.Load<OrgContact>(contactOldB.PK).OC_OH);

			var newOrgReloaded = factory.Load<OrgHeader>(newOrg.PK);
			AssertNotNull(newOrgReloaded);
			AssertEquals(2, newOrgReloaded.Contacts.Count);
			AssertEquals(3, newOrgReloaded.CollectionNotes.Count);

			var collectionNotesReloaded = newOrgReloaded.CollectionNotes.Cast<OrgCollectionNote>().OrderBy(cn => cn.PN_CallDetailNote).ToArray();
			AssertEquals("111", collectionNotesReloaded[0].PN_CallDetailNote);
			AssertEquals("Old collection note A should be moved to new contact A", contactNewA.PK, collectionNotesReloaded[0].PN_OC);
			AssertEquals("222", collectionNotesReloaded[1].PN_CallDetailNote);
			AssertEquals("Should remain unchanged", contactNewA.PK, collectionNotesReloaded[1].PN_OC);
			AssertEquals("333", collectionNotesReloaded[2].PN_CallDetailNote);
			AssertEquals("Should remain unchanged", contactOldB.PK, collectionNotesReloaded[2].PN_OC);
		}

		public void TestMergeOrgContactWithOrgContactItems()
		{
			#region Test Data

			var factory = new BusinessObjectFactory();
			var oldOrg = factory.NewWithValidTestData<OrgHeader>();
			var newOrg = factory.NewWithValidTestData<OrgHeader>();

			#region Contact A

			var contactOldA = oldOrg.Contacts.AddNew();
			contactOldA.OC_ContactName = "Andrew";

			var emailItem1OldA = factory.NewWithValidTestData<OrgContactItem>();
			emailItem1OldA.OI_OC = contactOldA.PK;
			emailItem1OldA.OI_ContactItemType = "EML";
			emailItem1OldA.OI_Description = "";
			emailItem1OldA.OI_Address = "andrew@wisetechglobal.com";
			emailItem1OldA.OI_IsPrimary = true;

			var emailItem2OldA = factory.NewWithValidTestData<OrgContactItem>();
			emailItem2OldA.OI_OC = contactOldA.PK;
			emailItem2OldA.OI_ContactItemType = "EML";
			emailItem2OldA.OI_Description = "";
			emailItem2OldA.OI_Address = "andrew@cargowise.com";
			emailItem2OldA.OI_IsPrimary = false;

			var phoneItem1OldA = factory.NewWithValidTestData<OrgContactItem>();
			phoneItem1OldA.OI_OC = contactOldA.PK;
			phoneItem1OldA.OI_ContactItemType = "PHN";
			phoneItem1OldA.OI_Description = "SKP";
			phoneItem1OldA.OI_Address = "andrew@wisetechglobal.com";
			phoneItem1OldA.OI_IsPrimary = true;

			var phoneItem2OldA = factory.NewWithValidTestData<OrgContactItem>();
			phoneItem2OldA.OI_OC = contactOldA.PK;
			phoneItem2OldA.OI_ContactItemType = "PHN";
			phoneItem2OldA.OI_Description = "SKP";
			phoneItem2OldA.OI_Address = "andrew@cargowise.com";
			phoneItem2OldA.OI_IsPrimary = false;

			var contactNewA = newOrg.Contacts.AddNew();
			contactNewA.OC_ContactName = "Andrew";

			var emailItem1NewA = factory.NewWithValidTestData<OrgContactItem>();
			emailItem1NewA.OI_OC = contactNewA.PK;
			emailItem1NewA.OI_ContactItemType = "EML";
			emailItem1NewA.OI_Description = "";
			emailItem1NewA.OI_Address = "andrew@wisetechglobal.com";
			emailItem1NewA.OI_IsPrimary = false;

			var emailItem2NewA = factory.NewWithValidTestData<OrgContactItem>();
			emailItem2NewA.OI_OC = contactNewA.PK;
			emailItem2NewA.OI_ContactItemType = "EML";
			emailItem2NewA.OI_Description = "";
			emailItem2NewA.OI_Address = "andrew@live.com";
			emailItem2NewA.OI_IsPrimary = true;

			var phoneItem1NewA = factory.NewWithValidTestData<OrgContactItem>();
			phoneItem1NewA.OI_OC = contactNewA.PK;
			phoneItem1NewA.OI_ContactItemType = "PHN";
			phoneItem1NewA.OI_Description = "SKP";
			phoneItem1NewA.OI_Address = "andrew@live.com";
			phoneItem1NewA.OI_IsPrimary = false;

			var phoneItem2NewA = factory.NewWithValidTestData<OrgContactItem>();
			phoneItem2NewA.OI_OC = contactNewA.PK;
			phoneItem2NewA.OI_ContactItemType = "PHN";
			phoneItem2NewA.OI_Description = "SKP";
			phoneItem2NewA.OI_Address = "andrew@cargowise.com";
			phoneItem2NewA.OI_IsPrimary = true;

			#endregion

			#region Contact B

			var contactOldB = oldOrg.Contacts.AddNew();
			contactOldB.OC_ContactName = "Andrew 2";

			var emailItemOldB = factory.NewWithValidTestData<OrgContactItem>();
			emailItemOldB.OI_OC = contactOldB.PK;
			emailItemOldB.OI_ContactItemType = "EML";
			emailItemOldB.OI_Description = "";
			emailItemOldB.OI_Address = "andrew@wisetechglobal.com";
			emailItemOldB.OI_IsPrimary = true;

			var emailItem2OldB = factory.NewWithValidTestData<OrgContactItem>();
			emailItem2OldB.OI_OC = contactOldB.PK;
			emailItem2OldB.OI_ContactItemType = "EML";
			emailItem2OldB.OI_Description = "";
			emailItem2OldB.OI_Address = "andrew@cargowise.com";
			emailItem2OldB.OI_IsPrimary = false;

			var contactNewB = newOrg.Contacts.AddNew();
			contactNewB.OC_ContactName = "Andrew 2";

			var emailItem1NewB = factory.NewWithValidTestData<OrgContactItem>();
			emailItem1NewB.OI_OC = contactNewB.PK;
			emailItem1NewB.OI_ContactItemType = "EML";
			emailItem1NewB.OI_Description = "";
			emailItem1NewB.OI_Address = "andrew@wisetechglobal.com";
			emailItem1NewB.OI_IsPrimary = true;

			var emailItem2NewB = factory.NewWithValidTestData<OrgContactItem>();
			emailItem2NewB.OI_OC = contactNewB.PK;
			emailItem2NewB.OI_ContactItemType = "EML";
			emailItem2NewB.OI_Description = "";
			emailItem2NewB.OI_Address = "andrew@live.com";
			emailItem2NewB.OI_IsPrimary = false;

			#endregion

			factory.Save();

			#endregion

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrg, newOrg);
			var merger = new OrganisationMergerForTest(mergeOrgHeader);
			merger.Save();

			factory = new BusinessObjectFactory();

			var contactNewAItems = factory.Load<OrgContactItem>(new ZQuery(OrgContactItemSchema.OI_OC, contactNewA.PK)).OrderBy(item => item.OI_ContactItemType).ThenBy(item => item.OI_Description).ThenBy(item => item.OI_Address).ToArray();
			AssertOrgContactItemValues("Should have added emailItem2OldA", contactNewAItems[0], "EML", "", "andrew@cargowise.com", false);
			AssertOrgContactItemValues("Should still have emailItem2NewA", contactNewAItems[1], "EML", "", "andrew@live.com", true);
			AssertOrgContactItemValues("Should have merged emailItem1OldA with emailItem1NewA", contactNewAItems[2], "EML", "", "andrew@wisetechglobal.com", false);
			AssertOrgContactItemValues("Should have merged phoneItem2OldA with phoneItem2NewA", contactNewAItems[3], "PHN", "SKP", "andrew@cargowise.com", true);
			AssertOrgContactItemValues("Should still have phoneItem1NewA", contactNewAItems[4], "PHN", "SKP", "andrew@live.com", false);
			AssertOrgContactItemValues("Should have added phoneItem1OldA", contactNewAItems[5], "PHN", "SKP", "andrew@wisetechglobal.com", true);
			AssertEquals(6, contactNewAItems.Length);

			var contactNewBItems = factory.Load<OrgContactItem>(new ZQuery(OrgContactItemSchema.OI_OC, contactNewB.PK)).OrderBy(item => item.OI_ContactItemType).ThenBy(item => item.OI_Description).ThenBy(item => item.OI_Address).ToArray();
			AssertOrgContactItemValues("Should have added emailItem2OldB", contactNewBItems[0], "EML", "", "andrew@cargowise.com", false);
			AssertOrgContactItemValues("Should still have emailItem2NewB", contactNewBItems[1], "EML", "", "andrew@live.com", false);
			AssertOrgContactItemValues("Should have merged emailItem1OldB with emailItem1NewB", contactNewBItems[2], "EML", "", "andrew@wisetechglobal.com", true);
			AssertEquals(3, contactNewBItems.Length);
		}

		void AssertOrgContactItemValues(ZString message, OrgContactItem item, ZString expectedContactItemType, ZString expectedDescription, ZString expectedAddress, ZBool expectedIsPrimary)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("OI_ContactItemType", expectedContactItemType, item.OI_ContactItemType);
				AssertEquals("OI_Description", expectedDescription, item.OI_Description);
				AssertEquals("OI_Address", expectedAddress, item.OI_Address);
				AssertEquals("OI_IsPrimary", expectedIsPrimary, item.OI_IsPrimary);
			});
		}

		bool DoesOrgHeaderHaveAddress(OrgHeader organisation, ZGuid addressPk)
		{
			bool result = false;

			foreach (OrgAddress address in organisation.Addresses)
			{
				if (address.PK == addressPk)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		bool DoesOrgHeaderHaveContact(OrgHeader organisation, ZGuid contactPk)
		{
			bool result = false;

			foreach (OrgContact contact in organisation.Contacts)
			{
				if (contact.PK == contactPk)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		bool DoesAddressHaveMainCapability(ZGuid addressPk)
		{
			string sqlText = string.Format("SELECT count(*) FROM dbo.OrgAddressCapability WHERE PZ_OA = '{0}' AND PZ_IsMainAddress = 1", addressPk.ToString());
			using (var cmd = GetCommandOnMainConnection(sqlText))
			{
				int rowCount = (int)cmd.ExecuteScalar();
				return (rowCount > 0);
			}
		}

		public void TestMergeWhsDockets()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			var oldPK = oldOrgHeader.PK.ToGuid();
			var newPK = newOrgHeader.PK.ToGuid();

			factory.Save();

			var warehouse = new WhsWarehouse("W1", GlbBranch.CurrentBranch.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);
			var receive1 = new WhsDocket(oldPK, warehouse.PK, "INW", "REC", "ENT", "111", "REF-1") { WD_ExternalReferenceSplit = 1 }.InsertAndReturnObject(TestConnection);
			var receive2 = new WhsDocket(oldPK, warehouse.PK, "INW", "REC", "ENT", "222", "REF-1") { WD_ExternalReferenceSplit = 2 }.InsertAndReturnObject(TestConnection);
			var receive3 = new WhsDocket(oldPK, warehouse.PK, "INW", "REC", "ENT", "333", "REF-2") { WD_ExternalReferenceSplit = 3, WD_StartedReceivingTimeUtc = DateTime.UtcNow }.InsertAndReturnObject(TestConnection);
			var receive4 = new WhsDocket(newPK, warehouse.PK, "INW", "REC", "ENT", "444", "REF-1") { WD_ExternalReferenceSplit = 1 }.InsertAndReturnObject(TestConnection);
			var receive5 = new WhsDocket(newPK, warehouse.PK, "INW", "REC", "ENT", "555", "REF-1") { WD_ExternalReferenceSplit = 3, WD_StartedReceivingTimeUtc = DateTime.UtcNow }.InsertAndReturnObject(TestConnection);

			// adding some stock to make sure that inventory sync trigger will not cause any trouble
			var whsHelper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var product = whsHelper.CreateProduct(oldOrgHeader.PK, "P1");
			whsHelper.CreateWhsReceiveInventoryLine(receive1.PK, product.PK, 10, ZGuid.Empty);
			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			var updatedDockets = WhsDocket.ShallowLoadFromDB(TestConnection, d => d.WD_OH_Client == newPK);

			AssertEquals("All dockets should be moved to new org.", 0, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == oldPK));
			AssertEquals("All dockets should be moved to new org.", 5, updatedDockets.Length);

			var refnumOld1 = updatedDockets.Single(d => d.PK == receive1.PK).WD_ExternalReferenceSplit;
			var refnumOld2 = updatedDockets.Single(d => d.PK == receive2.PK).WD_ExternalReferenceSplit;
			var refnumOld3 = updatedDockets.Single(d => d.PK == receive3.PK).WD_ExternalReferenceSplit;
			var refnumNew1 = updatedDockets.Single(d => d.PK == receive4.PK).WD_ExternalReferenceSplit;
			var refnumNew2 = updatedDockets.Single(d => d.PK == receive5.PK).WD_ExternalReferenceSplit;

			AssertEquals((byte)4, refnumOld1);
			AssertEquals((byte)5, refnumOld2);
			AssertEquals((byte)3, refnumOld3);
			AssertEquals((byte)1, refnumNew1);
			AssertEquals((byte)3, refnumNew2);
		}

		public void TestMergeWhsDockets_DoesNotThrowForLargeQuery()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			var oldPK = oldOrgHeader.PK;
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			factory.Save();

			var oldOrgPk = oldOrgHeader.PK.ToGuid();

			var warehouse = new WhsWarehouse("W1", GlbBranch.CurrentBranch.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);

			var sqlCommandBuilder = new StringBuilder();
			var dockets = new WhsDocket[2101];
			for (int i = 0; i <= 2100; i++)
			{
				dockets[i] = new WhsDocket(oldOrgPk, warehouse.PK, "INW", "REC", "ENT", i.ToString(), $"REF-{i}");
			}

			TestConnection.ExecuteNonQuery(WhsDocket.GetBulkInsertStatement(dockets));

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);

			AssertNoExceptionThrown(() => testMerger.Save());
		}

		public void TestMergeWhsDockets_DuplicateRefOnlyInOldOrganisation()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			var oldPK = oldOrgHeader.PK.ToGuid();
			var newPK = newOrgHeader.PK.ToGuid();

			factory.Save();

			var warehouse = new WhsWarehouse("W1", GlbBranch.CurrentBranch.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);
			var receive1 = new WhsDocket(oldPK, warehouse.PK, "INW", "REC", "ENT", "111", "REF-1") { WD_ExternalReferenceSplit = 1 }.InsertAndReturnObject(TestConnection);
			var receive2 = new WhsDocket(oldPK, warehouse.PK, "INW", "REC", "ENT", "222", "REF-1") { WD_ExternalReferenceSplit = 2 }.InsertAndReturnObject(TestConnection);

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals("All dockets should be moved to new org.", 0, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == oldPK));
			AssertEquals("All dockets should be moved to new org.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == newPK));

			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals("WD_ExternalReferenceSplit", d => d.WD_ExternalReferenceSplit, 1)
				.VerifyAll();

			WhsDocket.AssertFromDB(TestConnection, receive2.PK)
				.ExpectEquals("WD_ExternalReferenceSplit", d => d.WD_ExternalReferenceSplit, 2)
				.VerifyAll();
		}

		public void TestMergeWhsDockets_WD_WP_ParentPickForReceive()
		{
			var factory = new BusinessObjectFactory();

			var part1 = new OrgSupplierPartDO("P1").InsertAndReturnObject(TestConnection);
			var part2 = new OrgSupplierPartDO("P2").InsertAndReturnObject(TestConnection);

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			var oldPK = oldOrgHeader.PK.ToGuid();
			var newPK = newOrgHeader.PK.ToGuid();

			factory.Save();

			var warehouse = new WhsWarehouse("W1", GlbBranch.CurrentBranch.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);

			var pick = new WhsPick(warehouse, "P1", "NEW").InsertAndReturnObject(TestConnection);

			var receive1 = new WhsDocket(oldPK, warehouse.PK, "INW", "REC", "ENT", "111", "REF-1") { WD_WP_ParentPickForReceive = pick }.InsertAndReturnObject(TestConnection);
			var receiveLine1 = new WhsDocketLine(receive1, part1.PK, 1m) { WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);

			var receive2 = new WhsDocket(newPK, warehouse.PK, "INW", "REC", "ENT", "222", "REF-1") { WD_WP_ParentPickForReceive = pick }.InsertAndReturnObject(TestConnection);
			var receiveLine2 = new WhsDocketLine(receive2, part2.PK, 2m) { WE_StockOnHand = 2m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);

			var receive3 = new WhsDocket(otherOrg.PK.ToGuid(), warehouse.PK, "INW", "REC", "ENT", "333", "REF-1") { WD_WP_ParentPickForReceive = pick }.InsertAndReturnObject(TestConnection);
			var receiveLine3 = new WhsDocketLine(receive3, part1.PK, 3m) { WE_StockOnHand = 3m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals("Old Receive deleted.", 0, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == oldPK));
			AssertEquals("New Receive remains one.", 1, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == newPK));
			AssertEquals("Other Receive remains one.", 1, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == otherOrg.PK));

			AssertEquals("Receive Lines on Old Receive moved to New Receive.", 2, WhsDocketLine.CountInDB(TestConnection, l => l.WE_WD == receive2.PK));
			AssertEquals("Other Receive Line remains one.", 1, WhsDocketLine.CountInDB(TestConnection, l => l.WE_WD == receive3.PK));

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine1.PK)
				.ExpectEquals("WE_OP", d => d.WE_OP, part1.PK)
				.ExpectEquals("WE_TransactionQuantity", d => d.WE_TransactionQuantity, 1m)
				.ExpectEquals("WE_StockOnHand", d => d.WE_StockOnHand, 1m)
				.ExpectEquals("WE_OriginalInventoryStatus", d => d.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals("WE_CurrentInventoryStatus", d => d.WE_CurrentInventoryStatus, "PND")
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine2.PK)
				.ExpectEquals("WE_OP", d => d.WE_OP, part2.PK)
				.ExpectEquals("WE_TransactionQuantity", d => d.WE_TransactionQuantity, 2m)
				.ExpectEquals("WE_StockOnHand", d => d.WE_StockOnHand, 2m)
				.ExpectEquals("WE_OriginalInventoryStatus", d => d.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals("WE_CurrentInventoryStatus", d => d.WE_CurrentInventoryStatus, "PND")
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine3.PK)
				.ExpectEquals("WE_OP", d => d.WE_OP, part1.PK)
				.ExpectEquals("WE_TransactionQuantity", d => d.WE_TransactionQuantity, 3m)
				.ExpectEquals("WE_StockOnHand", d => d.WE_StockOnHand, 3m)
				.ExpectEquals("WE_OriginalInventoryStatus", d => d.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals("WE_CurrentInventoryStatus", d => d.WE_CurrentInventoryStatus, "PND")
				.VerifyAll();
		}

		public void TestMergeWhsDockets_WD_WP_ParentPickForReceive_DifferentPicks()
		{
			var factory = new BusinessObjectFactory();

			var part1 = new OrgSupplierPartDO("P1").InsertAndReturnObject(TestConnection);
			var part2 = new OrgSupplierPartDO("P2").InsertAndReturnObject(TestConnection);

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			var oldPK = oldOrgHeader.PK.ToGuid();
			var newPK = newOrgHeader.PK.ToGuid();

			factory.Save();

			var warehouse = new WhsWarehouse("W1", GlbBranch.CurrentBranch.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);

			var pick1 = new WhsPick(warehouse, "P1", "NEW").InsertAndReturnObject(TestConnection);
			var pick2 = new WhsPick(warehouse, "P2", "NEW").InsertAndReturnObject(TestConnection);

			var receive1 = new WhsDocket(oldPK, warehouse.PK, "INW", "REC", "ENT", "111", "REF-1") { WD_WP_ParentPickForReceive = pick1 }.InsertAndReturnObject(TestConnection);
			var receiveLine1 = new WhsDocketLine(receive1, part1.PK, 1m) { WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);

			var receive2 = new WhsDocket(newPK, warehouse.PK, "INW", "REC", "ENT", "222", "REF-1") { WD_WP_ParentPickForReceive = pick2 }.InsertAndReturnObject(TestConnection);
			var receiveLine2 = new WhsDocketLine(receive2, part2.PK, 2m) { WE_StockOnHand = 2m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);

			var receive3 = new WhsDocket(otherOrg.PK.ToGuid(), warehouse.PK, "INW", "REC", "ENT", "333", "REF-1") { WD_WP_ParentPickForReceive = pick1 }.InsertAndReturnObject(TestConnection);
			var receiveLine3 = new WhsDocketLine(receive3, part1.PK, 3m) { WE_StockOnHand = 3m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals("WD_OH_Client of Old Receive changed to newPK.", 1, WhsDocket.CountInDB(TestConnection, d => d.PK == receive1.PK && d.WD_OH_Client == newPK));
			AssertEquals("2 Receives under new org.", 2, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == newPK));
			AssertEquals("Other Receive remains one.", 1, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == otherOrg.PK));

			AssertEquals("Lines of Old Receive remains still.", 1, WhsDocketLine.CountInDB(TestConnection, l => l.WE_WD == receive1.PK));
			AssertEquals("Lines of New Receive remains still.", 1, WhsDocketLine.CountInDB(TestConnection, l => l.WE_WD == receive2.PK));
			AssertEquals("Other Receive Line remains one.", 1, WhsDocketLine.CountInDB(TestConnection, l => l.WE_WD == receive3.PK));

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine1.PK)
				.ExpectEquals("WE_OP", d => d.WE_OP, part1.PK)
				.ExpectEquals("WE_TransactionQuantity", d => d.WE_TransactionQuantity, 1m)
				.ExpectEquals("WE_StockOnHand", d => d.WE_StockOnHand, 1m)
				.ExpectEquals("WE_OriginalInventoryStatus", d => d.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals("WE_CurrentInventoryStatus", d => d.WE_CurrentInventoryStatus, "PND")
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine2.PK)
				.ExpectEquals("WE_OP", d => d.WE_OP, part2.PK)
				.ExpectEquals("WE_TransactionQuantity", d => d.WE_TransactionQuantity, 2m)
				.ExpectEquals("WE_StockOnHand", d => d.WE_StockOnHand, 2m)
				.ExpectEquals("WE_OriginalInventoryStatus", d => d.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals("WE_CurrentInventoryStatus", d => d.WE_CurrentInventoryStatus, "PND")
				.VerifyAll();

			WhsDocketLine.AssertFromDB(TestConnection, receiveLine3.PK)
				.ExpectEquals("WE_OP", d => d.WE_OP, part1.PK)
				.ExpectEquals("WE_TransactionQuantity", d => d.WE_TransactionQuantity, 3m)
				.ExpectEquals("WE_StockOnHand", d => d.WE_StockOnHand, 3m)
				.ExpectEquals("WE_OriginalInventoryStatus", d => d.WE_OriginalInventoryStatus, "PND")
				.ExpectEquals("WE_CurrentInventoryStatus", d => d.WE_CurrentInventoryStatus, "PND")
				.VerifyAll();
		}

		public void TestMergeWhsSerialNumber()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			factory.Save();

			var warehouse = new WhsWarehouse("W1").WithDockDoor(TestConnection);
			var oldOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, oldOrgHeader.PK.ToGuid());
			var newOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, newOrgHeader.PK.ToGuid());
			var part = new OrgSupplierPartDO("P1").InsertAndReturnObject(TestConnection);
			var receive1 = new WhsDocket(oldOrgHeader.PK.ToGuid(), warehouse.PK, "INW", "REC", "ENT", "R1").InsertAndReturnObject(TestConnection);
			var receive2 = new WhsDocket(newOrgHeader.PK.ToGuid(), warehouse.PK, "INW", "REC", "ENT", "R2").InsertAndReturnObject(TestConnection);
			var receiveLine1 = new WhsDocketLine(receive1, part.PK, 1m) { WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);
			var receiveLine2 = new WhsDocketLine(receive2, part.PK, 1m) { WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);
			var serialNumber1 = new WhsSerialNumber(oldOrg, part, "S1").InsertAndReturnObject(TestConnection);
			var serialNumber2 = new WhsSerialNumber(newOrg, part, "S2").InsertAndReturnObject(TestConnection);
			new WhsSerialNumberPivot(receiveLine1.PK, "WE", serialNumber1.PK).Insert(TestConnection);
			new WhsSerialNumberPivot(receiveLine2.PK, "WE", serialNumber2.PK).Insert(TestConnection);

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals(0, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == oldOrg.PK));

			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals(nameof(WhsDocket.WD_OH_Client), d => d.WD_OH_Client, newOrg.PK)
				.VerifyAll();

			WhsSerialNumber.AssertFromDB(TestConnection, serialNumber1.PK)
				.ExpectEquals("Client should be updated.", s => s.WSN_OH_Client.FK, newOrg.PK)
				.VerifyAll();

			WhsSerialNumber.AssertFromDB(TestConnection, serialNumber2.PK)
				.ExpectEquals("Client should be unchanged.", s => s.WSN_OH_Client.FK, newOrg.PK)
				.VerifyAll();
		}

		public void TestMergeWhsSerialNumber_Duplicate()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			factory.Save();

			var warehouse = new WhsWarehouse("W1").WithDockDoor(TestConnection);
			var oldOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, oldOrgHeader.PK.ToGuid());
			var newOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, newOrgHeader.PK.ToGuid());
			var part = new OrgSupplierPartDO("P1").InsertAndReturnObject(TestConnection);
			var receive1 = new WhsDocket(oldOrgHeader.PK.ToGuid(), warehouse.PK, "INW", "REC", "ENT", "R1").InsertAndReturnObject(TestConnection);
			var receive2 = new WhsDocket(newOrgHeader.PK.ToGuid(), warehouse.PK, "INW", "REC", "ENT", "R2").InsertAndReturnObject(TestConnection);
			var receiveLine1 = new WhsDocketLine(receive1, part.PK, 1m) { WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);
			var receiveLine2 = new WhsDocketLine(receive2, part.PK, 1m) { WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);
			var serialNumber1 = new WhsSerialNumber(oldOrg, part, "S1").InsertAndReturnObject(TestConnection);
			var serialNumber2 = new WhsSerialNumber(newOrg, part, "S1").InsertAndReturnObject(TestConnection);
			new WhsSerialNumberPivot(receiveLine1.PK, "WE", serialNumber1.PK).Insert(TestConnection);
			new WhsSerialNumberPivot(receiveLine2.PK, "WE", serialNumber2.PK).Insert(TestConnection);

			// Cannot merge such that Duplicate Serials are created.
			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertExceptionThrown<SqlException>(() => testMerger.Save());
		}

		public void TestMergeWhsSerialNumber_Duplicate_NotInUse()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			factory.Save();

			var warehouse = new WhsWarehouse("W1").WithDockDoor(TestConnection);
			var oldOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, oldOrgHeader.PK.ToGuid());
			var newOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, newOrgHeader.PK.ToGuid());
			var part = new OrgSupplierPartDO("P1").InsertAndReturnObject(TestConnection);
			var receive1 = new WhsDocket(oldOrgHeader.PK.ToGuid(), warehouse.PK, "INW", "REC", "ENT", "R1").InsertAndReturnObject(TestConnection);
			var receive2 = new WhsDocket(newOrgHeader.PK.ToGuid(), warehouse.PK, "INW", "REC", "ENT", "R2").InsertAndReturnObject(TestConnection);
			var receiveLine1 = new WhsDocketLine(receive1, part.PK, 1m) { WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);
			var receiveLine2 = new WhsDocketLine(receive2, part.PK, 1m) { WE_StockOnHand = 1m, WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.InsertAndReturnObject(TestConnection);
			var serialNumber1 = new WhsSerialNumber(oldOrg, part, "S1") { WSN_IsInUse = false }.InsertAndReturnObject(TestConnection);
			var serialNumber2 = new WhsSerialNumber(newOrg, part, "S1").InsertAndReturnObject(TestConnection);
			new WhsSerialNumberPivot(receiveLine1.PK, "WE", serialNumber1.PK).Insert(TestConnection);
			new WhsSerialNumberPivot(receiveLine2.PK, "WE", serialNumber2.PK).Insert(TestConnection);

			// Cannot merge such that Duplicate Serials are created.
			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals(0, WhsDocket.CountInDB(TestConnection, d => d.WD_OH_Client == oldOrg.PK));

			WhsDocket.AssertFromDB(TestConnection, receive1.PK)
				.ExpectEquals(nameof(WhsDocket.WD_OH_Client), d => d.WD_OH_Client, newOrg.PK)
				.VerifyAll();

			WhsSerialNumber.AssertFromDB(TestConnection, serialNumber1.PK)
				.ExpectEquals("Client should be updated.", s => s.WSN_OH_Client.FK, newOrg.PK)
				.VerifyAll();

			WhsSerialNumber.AssertFromDB(TestConnection, serialNumber2.PK)
				.ExpectEquals("Client should be unchanged.", s => s.WSN_OH_Client.FK, newOrg.PK)
				.VerifyAll();
		}

		public void TestMergeWhsClientPickPackParamsByWhs()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";
			var oldPK = oldOrgHeader.PK;

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";
			var newPK = newOrgHeader.PK;

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			factory.Save();

			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = GlbCompany.CurrentCompany.Branches.First(g => g.PK != GlbBranch.CurrentBranch.PK);
			var salesChannel1 = new WhsSalesChannel("ECO", "eCommerce").InsertAndReturnObject(TestConnection);
			var salesChannel2 = new WhsSalesChannel("DIS", "Distribution").InsertAndReturnObject(TestConnection);
			var warehouse1 = new WhsWarehouse("WH1", branch1.PK.ToGuid()).WithDockDoor(TestConnection);
			var warehouse2 = new WhsWarehouse("WH2", branch2.PK.ToGuid()).WithDockDoor(TestConnection);
			var oldOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, oldPK.ToGuid());
			var newOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, newPK.ToGuid());

			var oldOrgWarehouse1Param = new WhsClientPickPackParamsByWhs(oldOrg, warehouse1).InsertAndReturnObject(TestConnection);
			var oldOrgWarehouse1SalesChannel1Param = new WhsClientPickPackParamsByWhs(oldOrg, warehouse1) { WPP_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection);
			var oldOrgWarehouse2Param = new WhsClientPickPackParamsByWhs(oldOrg, warehouse2).InsertAndReturnObject(TestConnection);
			var oldOrgWarehouse2SalesChannel2Param = new WhsClientPickPackParamsByWhs(oldOrg, warehouse2) { WPP_WSH_SalesChannel = salesChannel2.PK }.InsertAndReturnObject(TestConnection);
			var newOrgWarehouse1Param = new WhsClientPickPackParamsByWhs(newOrg, warehouse1).InsertAndReturnObject(TestConnection);
			var newOrgWarehouse1SalesChannel1Param = new WhsClientPickPackParamsByWhs(newOrg, warehouse1) { WPP_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection);
			var newOrgWarehouse2Param = new WhsClientPickPackParamsByWhs(newOrg, warehouse2).InsertAndReturnObject(TestConnection);
			var newOrgWarehouse2SalesChannel1Param = new WhsClientPickPackParamsByWhs(newOrg, warehouse2) { WPP_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection);

			var oldOrgWarehouse1Order = new WhsDocket(oldOrg.PK, warehouse1.PK, "ORD", "ORD", "ENT", "O1").InsertAndReturnObject(TestConnection);
			var oldOrgWarehouse1SalesChannel1Order = new WhsDocket(oldOrg.PK, warehouse1.PK, "ORD", "ORD", "ENT", "O2") { WD_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection);
			var oldOrgWarehouse2Order = new WhsDocket(oldOrg.PK, warehouse2.PK, "ORD", "ORD", "ENT", "O3").InsertAndReturnObject(TestConnection);
			var newOrgWarehouse1Order = new WhsDocket(newOrg.PK, warehouse1.PK, "ORD", "ORD", "ENT", "O4").InsertAndReturnObject(TestConnection);
			var newOrgWarehouse1SalesChannel1Order = new WhsDocket(newOrg.PK, warehouse1.PK, "ORD", "ORD", "ENT", "O5") { WD_WSH_SalesChannel = salesChannel1.PK }.InsertAndReturnObject(TestConnection);
			var newOrgWarehouse2Order = new WhsDocket(newOrg.PK, warehouse2.PK, "ORD", "ORD", "ENT", "O6").InsertAndReturnObject(TestConnection);
			var oldOrgWarehouse1PackageJob = new PkgPackageJob(oldOrgWarehouse1Order.PK, "PJ1", "WD").InsertAndReturnObject(TestConnection);
			new PkgPackageJob(oldOrgWarehouse1SalesChannel1Order.PK, "PJ2", "WD").InsertAndReturnObject(TestConnection);
			var oldOrgWarehouse2PackageJob = new PkgPackageJob(oldOrgWarehouse2Order.PK, "PJ3", "WD").InsertAndReturnObject(TestConnection);
			new PkgPackageJob(newOrgWarehouse1Order.PK, "PJ4", "WD").InsertAndReturnObject(TestConnection);
			var newOrgWarehouse1SalesChannel1PackageJob = new PkgPackageJob(newOrgWarehouse1SalesChannel1Order.PK, "PJ5", "WD").InsertAndReturnObject(TestConnection);
			new PkgPackageJob(newOrgWarehouse2Order.PK, "PJ6", "WD").InsertAndReturnObject(TestConnection);
			new PkgPackage(oldOrgWarehouse1PackageJob, "CTN", 1).Insert(TestConnection);
			new PkgPackage(oldOrgWarehouse2PackageJob, "CTN", 1).Insert(TestConnection);
			new PkgPackage(newOrgWarehouse1SalesChannel1PackageJob, "CTN", 1).Insert(TestConnection);

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			WhsClientPickPackParamsByWhs.AssertFromDB(TestConnection, oldOrgWarehouse1Param.PK)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_OH_Client), p => p.WPP_OH_Client, newOrg)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WW_Warehouse), p => p.WPP_WW_Warehouse, warehouse1)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WSH_SalesChannel), p => p.WPP_WSH_SalesChannel, null)
				.VerifyAll();

			AssertEquals(false, WhsClientPickPackParamsByWhs.ExistsInDB(TestConnection, oldOrgWarehouse1SalesChannel1Param.PK));

			WhsClientPickPackParamsByWhs.AssertFromDB(TestConnection, oldOrgWarehouse2Param.PK)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_OH_Client), p => p.WPP_OH_Client, newOrg)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WW_Warehouse), p => p.WPP_WW_Warehouse, warehouse2)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WSH_SalesChannel), p => p.WPP_WSH_SalesChannel, null)
				.VerifyAll();

			WhsClientPickPackParamsByWhs.AssertFromDB(TestConnection, oldOrgWarehouse2SalesChannel2Param.PK)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_OH_Client), p => p.WPP_OH_Client, newOrg)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WW_Warehouse), p => p.WPP_WW_Warehouse, warehouse2)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WSH_SalesChannel), p => p.WPP_WSH_SalesChannel, salesChannel2.PK)
				.VerifyAll();

			AssertEquals(false, WhsClientPickPackParamsByWhs.ExistsInDB(TestConnection, newOrgWarehouse1Param.PK));

			WhsClientPickPackParamsByWhs.AssertFromDB(TestConnection, newOrgWarehouse1SalesChannel1Param.PK)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_OH_Client), p => p.WPP_OH_Client, newOrg)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WW_Warehouse), p => p.WPP_WW_Warehouse, warehouse1)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WSH_SalesChannel), p => p.WPP_WSH_SalesChannel, salesChannel1.PK)
				.VerifyAll();

			AssertEquals(false, WhsClientPickPackParamsByWhs.ExistsInDB(TestConnection, newOrgWarehouse2Param.PK));

			WhsClientPickPackParamsByWhs.AssertFromDB(TestConnection, newOrgWarehouse2SalesChannel1Param.PK)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_OH_Client), p => p.WPP_OH_Client, newOrg)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WW_Warehouse), p => p.WPP_WW_Warehouse, warehouse2)
				.ExpectEquals(nameof(WhsClientPickPackParamsByWhs.WPP_WSH_SalesChannel), p => p.WPP_WSH_SalesChannel, salesChannel1.PK)
				.VerifyAll();
		}

		public void TestDeleteWhsClientParameterByWarehouse()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";
			var oldPK = oldOrgHeader.PK;

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";
			var newPK = newOrgHeader.PK;

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			factory.Save();

			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = GlbCompany.CurrentCompany.Branches.First(g => g.PK != GlbBranch.CurrentBranch.PK);
			var oldOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, oldPK.ToGuid());
			var newOrg = OrgHeaderDO.ShallowLoadFromDB(TestConnection, newPK.ToGuid());
			var warehouse1 = new WhsWarehouse("W1", branch1.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);
			var warehouse2 = new WhsWarehouse("W2", branch2.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);
			var param1 = new WhsClientParameterByWarehouse(oldOrg) { WY_WW_Whs = warehouse1 }.InsertAndReturnObject(TestConnection);
			new WhsClientParameterByWarehouse(oldOrg) { WY_WW_Whs = warehouse2 }.Insert(TestConnection);
			new WhsClientParameterByWarehouse(newOrg) { WY_WW_Whs = warehouse1 }.Insert(TestConnection);

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertDelete(mergeOrgHeader, oldPK, param1.PK, newPK, "WhsClientParameterByWarehouse", "WY_OH_Client");
		}

		public void TestDeleteWhsProductParamsByWhsAndClient()
		{
			var factory = new BusinessObjectFactory();

			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";
			var oldPK = oldOrgHeader.PK;

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";
			var newPK = newOrgHeader.PK;

			var otherOrg = factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "~TESTOTRORG~";
			otherOrg.MainAddress.OA_Code = "333";

			var part1 = factory.NewWithValidTestData<OrgSupplierPart>();
			var part2 = factory.NewWithValidTestData<OrgSupplierPart>();

			factory.Save();

			var branch1 = GlbBranch.CurrentBranch;
			var branch2 = GlbCompany.CurrentCompany.Branches.First(g => g.PK != GlbBranch.CurrentBranch.PK);

			var warehouse1 = new WhsWarehouse("W1", branch1.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);
			var warehouse2 = new WhsWarehouse("W2", branch2.PK.ToGuid(), otherOrg.MainAddress.PK.ToGuid()).WithDockDoor(TestConnection);
			var whsProductParamsByWhsAndClient1 = new WhsProductParamsByWhsAndClient(oldPK.ToGuid(), warehouse1.PK, part1.PK.ToGuid()).InsertAndReturnObject(TestConnection);
			var whsProductParamsByWhsAndClient2 = new WhsProductParamsByWhsAndClient(oldPK.ToGuid(), warehouse2.PK, part1.PK.ToGuid()).InsertAndReturnObject(TestConnection);
			var whsProductParamsByWhsAndClient3 = new WhsProductParamsByWhsAndClient(oldPK.ToGuid(), warehouse1.PK, part2.PK.ToGuid()).InsertAndReturnObject(TestConnection);
			var whsProductParamsByWhsAndClient4 = new WhsProductParamsByWhsAndClient(newPK.ToGuid(), warehouse1.PK, part1.PK.ToGuid()).InsertAndReturnObject(TestConnection);

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals(0, WhsProductParamsByWhsAndClient.CountInDB(TestConnection, o => o.W3_OH == oldPK));

			AssertEquals(3, WhsProductParamsByWhsAndClient.CountInDB(TestConnection, o => o.W3_OH == newPK));

			AssertEquals(false, WhsProductParamsByWhsAndClient.ExistsInDB(TestConnection, whsProductParamsByWhsAndClient1.PK));

			AssertEquals(true, WhsProductParamsByWhsAndClient.ExistsInDB(TestConnection, whsProductParamsByWhsAndClient3.PK));
		}

		public void TestDeleteRefPacks()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";
			ZGuid oldPK = oldOrgHeader.PK;

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";
			ZGuid newPK = newOrgHeader.PK;

			ZGuid pk1 = ZGuid.NewZGuid();

			factory.Save();

			string sql = @"
			insert into dbo.RefPacks (RP_PK, RP_OH_Supplier, RP_CommercialPack, RP_CustomsPack, RP_ConversionFactor, RP_CustomsCountry) values ('" + pk1 + "','" + oldPK + @"','AAA', 'BBB', '123', 'UA')
			insert into dbo.RefPacks (RP_PK, RP_OH_Supplier, RP_CommercialPack, RP_CustomsPack, RP_ConversionFactor, RP_CustomsCountry) values (NEWID(), '" + oldPK + @"','ZZZ', 'BBB', '123', 'UA')
			insert into dbo.RefPacks (RP_PK, RP_OH_Supplier, RP_CommercialPack, RP_CustomsPack, RP_ConversionFactor, RP_CustomsCountry) values (NEWID(), '" + newPK + @"','AAA', 'BBB', '123', 'UA')";

			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.ExecuteNonQuery();
			}
			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertDelete(mergeOrgHeader, oldPK, pk1, newPK, "RefPacks", "RP_OH_Supplier");
		}

		public void TestDeleteCusSeaManSlotOrg()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";
			ZGuid oldPK = oldOrgHeader.PK;
			ZGuid pk1 = ZGuid.NewZGuid();

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";
			ZGuid newPK = newOrgHeader.PK;

			ZGuid head1 = ZGuid.NewZGuid();
			ZGuid head2 = ZGuid.NewZGuid();

			factory.Save();

			string sql = @"
			insert into dbo.CusSeaManTranHead (BT_PK, BT_VoyageNum, BT_SystemCreateTimeUtc, BT_SystemCreateUser, BT_SystemLastEditTimeUtc, BT_SystemLastEditUser) values('" + head1 + @"', '98765', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.CusSeaManTranHead (BT_PK, BT_VoyageNum, BT_SystemCreateTimeUtc, BT_SystemCreateUser, BT_SystemLastEditTimeUtc, BT_SystemLastEditUser) values('" + head2 + @"', '43210', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.CusSeaManSlotOrg (BS_PK, BS_OH_SlotCharterer, BS_BT, BS_SystemCreateTimeUtc, BS_SystemCreateUser, BS_SystemLastEditTimeUtc, BS_SystemLastEditUser) values ('" + pk1 + "','" + oldPK + @"','" + head1 + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.CusSeaManSlotOrg (BS_PK, BS_OH_SlotCharterer, BS_BT, BS_SystemCreateTimeUtc, BS_SystemCreateUser, BS_SystemLastEditTimeUtc, BS_SystemLastEditUser) values (NEWID(), '" + oldPK + @"','" + head2 + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.CusSeaManSlotOrg (BS_PK, BS_OH_SlotCharterer, BS_BT, BS_SystemCreateTimeUtc, BS_SystemCreateUser, BS_SystemLastEditTimeUtc, BS_SystemLastEditUser) values (NEWID(), '" + newPK + @"','" + head1 + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.ExecuteNonQuery();
			}
			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertDelete(mergeOrgHeader, oldPK, pk1, newPK, "CusSeaManSlotOrg", "BS_OH_SlotCharterer");
		}

		public void TestOrgHeaderIsDeleted()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			oldOrgHeader.Delete();
			AssertNoExceptionThrown(() => testMerger.Save());
		}

		public void TestDeleteJobTradeLaneVoyage()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";
			ZGuid oldPK = oldOrgHeader.PK;
			ZGuid pk1 = ZGuid.NewZGuid();

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";
			ZGuid newPK = newOrgHeader.PK;

			ZGuid voyage1 = ZGuid.NewZGuid();
			ZGuid voyage2 = ZGuid.NewZGuid();

			factory.Save();

			string sql = @"
			insert into dbo.JobVoyage (JV_PK, JV_SystemCreateTimeUtc, JV_SystemCreateUser, JV_SystemLastEditTimeUtc, JV_SystemLastEditUser) values('" + voyage1 + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.JobVoyage (JV_PK, JV_SystemCreateTimeUtc, JV_SystemCreateUser, JV_SystemLastEditTimeUtc, JV_SystemLastEditUser) values('" + voyage2 + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.JobTradeLaneVoyage (NB_PK, NB_OH, NB_JV, NB_SystemCreateTimeUtc, NB_SystemCreateUser, NB_SystemLastEditTimeUtc, NB_SystemLastEditUser) values ('" + pk1 + "','" + oldPK + @"','" + voyage1 + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.JobTradeLaneVoyage (NB_PK, NB_OH, NB_JV, NB_SystemCreateTimeUtc, NB_SystemCreateUser, NB_SystemLastEditTimeUtc, NB_SystemLastEditUser) values (NEWID(), '" + oldPK + @"','" + voyage2 + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			insert into dbo.JobTradeLaneVoyage (NB_PK, NB_OH, NB_JV, NB_SystemCreateTimeUtc, NB_SystemCreateUser, NB_SystemLastEditTimeUtc, NB_SystemLastEditUser) values (NEWID(), '" + newPK + @"','" + voyage1 + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.ExecuteNonQuery();
			}
			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertDelete(mergeOrgHeader, oldPK, pk1, newPK, "JobTradeLaneVoyage", "NB_OH");
		}

		void AssertDelete(MergeOrgHeader mergeOrgHeader, ZGuid oldPK, ZGuid pk1, ZGuid newPK, string tableName, string fkName)
		{
			DataTable objOld = new DataTable();
			using (DbCommand cmd = Db.Connection.Command("select * from " + tableName + " where " + fkName + " =@OldOrgPk")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldPK.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(objOld);
				}
			}
			DataTable objNew = new DataTable();
			using (DbCommand cmd = Db.Connection.Command("select * from " + tableName + " where " + fkName + " =@NewOrgPk")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newPK.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(objNew);
				}
			}

			string prefix = fkName.Split('_')[0];
			DataTable deletedObj = new DataTable();
			using (DbCommand cmd = Db.Connection.Command("select * from " + tableName + " where " + prefix + "_PK =@PK")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk1.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(deletedObj);
				}
			}

			AssertEquals("all dockets should be moved to new org", 0, objOld.Rows.Count);
			AssertEquals("all dockets should be moved to new org", 2, objNew.Rows.Count);
			AssertEquals("should be deleted", 0, deletedObj.Rows.Count);
		}

		public void TestDeleteStmMenuDocumentConfig()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";
			ZGuid oldPK = oldOrgHeader.PK;
			ZGuid pk1 = ZGuid.NewZGuid();

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";
			ZGuid newPK = newOrgHeader.PK;

			ZGuid so = ZGuid.NewZGuid();
			ZGuid su = ZGuid.NewZGuid();
			ZGuid si = ZGuid.NewZGuid();
			ZGuid s3_1 = ZGuid.NewZGuid();
			ZGuid s3_2 = ZGuid.NewZGuid();
			ZGuid item1 = ZGuid.NewZGuid();
			ZGuid item2 = ZGuid.NewZGuid();

			factory.Save();

			string sql = @"
				insert into dbo.StmTemplate(SO_PK, SO_Name, SO_DataContext, SO_IsSystemDefined, SO_IsClientSpecific) values ('" + so + @"', 'abcd', 'some context', 1, 1)
				insert into dbo.StmMenuItem(SU_PK, SU_MenuName, SU_BusinessContext, SU_MenuPath, SU_IsSystemDefined, SU_IsClientSpecific, SU_FilterList)
					values ('" + su + @"', 'menu name', 'menu context', 'some path', 0, 0, 'abcd')
				insert into dbo.StmMenuTemplatePivot(SI_PK, SI_SU, SI_SO) values ('" + si + "', '" + su + "','" + so + @"')
				insert into dbo.StmMenuDocumentConfig(S3_PK, S3_SI, S3_OH, S3_IsSystem) values ('" + s3_1 + "', '" + si + "','" + oldPK + @"', 0)
				insert into dbo.StmMenuDocumentConfig(S3_PK, S3_SI, S3_OH, S3_IsSystem) values ('" + s3_2 + "', '" + si + "','" + newPK + @"', 0)
				insert into dbo.StmMenuDocumentConfigItem(S4_PK, S4_S3) values (NEWID(), '" + s3_1 + @"')
				insert into dbo.StmMenuDocumentConfigItem(S4_PK, S4_S3) values (NEWID(), '" + s3_1 + @"')
				insert into dbo.StmMenuDocumentConfigItem(S4_PK, S4_S3) values ('" + item1 + "','" + s3_2 + @"')
				insert into dbo.StmMenuDocumentConfigItem(S4_PK, S4_S3) values ('" + item2 + "','" + s3_2 + @"')
				";
			using (var cmd = GetCommandOnMainConnection(sql))
			{
				cmd.ExecuteNonQuery();
			}
			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			DataTable objOld = new DataTable();
			using (DbCommand cmd = Db.Connection.Command("select * from dbo.StmMenuDocumentConfig where S3_OH =@OldOrgPk")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldOrgPk", SqlDbType.UniqueIdentifier, oldPK.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(objOld);
				}
			}
			DataTable objNew = new DataTable();
			using (DbCommand cmd = Db.Connection.Command("select * from dbo.StmMenuDocumentConfig where S3_OH =@NewOrgPk")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewOrgPk", SqlDbType.UniqueIdentifier, newPK.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(objNew);
				}
			}

			DataTable itemsOld = new DataTable();
			using (DbCommand cmd = Db.Connection.Command("select * from dbo.StmMenuDocumentConfigItem where S4_S3 =@OldConfig")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@OldConfig", SqlDbType.UniqueIdentifier, s3_1.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(itemsOld);
				}
			}
			DataTable itemsNew = new DataTable();
			using (DbCommand cmd = Db.Connection.Command("select * from dbo.StmMenuDocumentConfigItem where S4_S3 =@NewConfig")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameter("@NewConfig", SqlDbType.UniqueIdentifier, s3_2.ToGuid());
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(itemsNew);
				}
			}

			AssertEquals(0, objOld.Rows.Count);
			AssertEquals(1, objNew.Rows.Count);
			AssertEquals(s3_2.ToString(), objNew.Rows[0][StmMenuDocumentConfigSchema.PK.Name].ToString());
			AssertEquals(0, itemsOld.Rows.Count);
			AssertEquals(2, itemsNew.Rows.Count);
			if (itemsNew.Rows[0][StmMenuDocumentConfigItemSchema.PK.Name].ToString() == item1.ToString())
			{
				AssertEquals(item1.ToString(), itemsNew.Rows[0][StmMenuDocumentConfigItemSchema.PK.Name].ToString());
				AssertEquals(item2.ToString(), itemsNew.Rows[1][StmMenuDocumentConfigItemSchema.PK.Name].ToString());
			}
			else
			{
				AssertEquals(item2.ToString(), itemsNew.Rows[0][StmMenuDocumentConfigItemSchema.PK.Name].ToString());
				AssertEquals(item1.ToString(), itemsNew.Rows[1][StmMenuDocumentConfigItemSchema.PK.Name].ToString());
			}
		}

		public void TestOrgServiceLevels()
		{
			AssertOrgServiceLevels(true);
			AssertOrgServiceLevels(false);
		}

		void AssertOrgServiceLevels(bool newOrgHasServiceLevels)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = Guid.NewGuid().ToString().Replace("-", "").Substring(0, OrgHeaderSchema.OH_Code.MaxLength);
			oldOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid oldPK = oldOrgHeader.PK;

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			newOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid newPK = newOrgHeader.PK;

			ActiveServiceLevelCollection refLevels = new ActiveServiceLevelCollection(factory);
			RefServiceLevel changingLevel = refLevels[0];

			RegistryServiceLevelCollection registryValue = WebDataRegistry.Instance.ServiceLevelVisibility.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			foreach (RegistryServiceLevel level in registryValue)
			{
				if (level.RefServiceLevelPK == changingLevel.PK)
				{
					level.Bool = false;
					break;
				}
			}

			WebDataRegistry.Instance.ServiceLevelVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			factory.Save();

			PublishServiceLevel(factory, oldOrgHeader, changingLevel);
			if (newOrgHasServiceLevels)
			{
				PublishServiceLevel(factory, newOrgHeader, changingLevel);
			}

			List<ZGuid> newServiceLevels = new List<ZGuid>();
			List<ZGuid> oldServiceLevels = new List<ZGuid>();
			foreach (var level in oldOrgHeader.OrgServiceLevels)
			{
				oldServiceLevels.Add(level.PK);
			}
			foreach (var level in newOrgHeader.OrgServiceLevels)
			{
				newServiceLevels.Add(level.PK);
			}

			if (newOrgHasServiceLevels)
			{
				Assert(newOrgHeader.IsServiceLevelOverridden);
			}
			else
			{
				Assert(!newOrgHeader.IsServiceLevelOverridden);
			}

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals("", mergeOrgHeader.DeleteError);
			BusinessObjectFactory assertFactory = new BusinessObjectFactory();
			oldOrgHeader = assertFactory.Load<OrgHeader>(oldPK);
			newOrgHeader = assertFactory.Load<OrgHeader>(newPK);
			OrgServiceLevel[] oldlevels = assertFactory.Load<OrgServiceLevel>(new ZQuery(OrgServiceLevelSchema.PM_OH, oldPK));
			OrgServiceLevel[] newlevels = assertFactory.Load<OrgServiceLevel>(new ZQuery(OrgServiceLevelSchema.PM_OH, newPK));
			AssertEquals(0, oldlevels.Length);
			if (newOrgHasServiceLevels)
			{
				AssertEquals(newServiceLevels.Count, newlevels.Length);
				foreach (var level in newlevels)
				{
					Assert(newServiceLevels.Contains(level.PK));
				}
			}
			else
			{
				AssertEquals(oldServiceLevels.Count, newlevels.Length);
				foreach (var level in newlevels)
				{
					Assert(oldServiceLevels.Contains(level.PK));
				}
			}
		}

		void PublishServiceLevel(BusinessObjectFactory factory, OrgHeader org, RefServiceLevel changingLevel)
		{
			foreach (OrgServiceLevel level in org.OrgServiceLevels)
			{
				if (level.PM_RS_NKSrvLvl == changingLevel.RS_Code)
				{
					Assert("This level was unpublished in registry", !level.PM_IsPublished);
					org.IsServiceLevelOverridden = true;
					level.PM_IsPublished = true;
					factory.Save();
					break;
				}
			}
		}

		public void TestApprovedLocations()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			RefCountry country1 = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "GB"));
			RefCountry country2 = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "UA"));

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "111";
			OrgAddress adr2 = oldOrgHeader.Addresses.AddNew();
			adr2.OA_Code = "123";
			adr2.OA_Address1 = "address number 2";
			ZGuid oldPK = oldOrgHeader.PK;

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "222";
			newOrgHeader.MainAddress.OA_Address1 = "new org header addr1";
			ZGuid newPK = newOrgHeader.PK;

			OrgHeader otherOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			otherOrgHeader.OH_Code = "~TESTOTRORG~";
			otherOrgHeader.MainAddress.OA_Code = "333";
			otherOrgHeader.MainAddress.OA_Address1 = "other org header addr1";
			ZGuid otherPK = otherOrgHeader.PK;

			OrgCountryData data1 = factory.New<OrgCountryData>();
			data1.OV_OH_OrgHeader = otherPK;
			data1.OV_OA_ApprovedLocation = oldOrgHeader.MainAddress.PK;
			data1.OV_RN_NKClientCountryRelation = country1.RN_Code;
			ZGuid ov1 = data1.PK;

			OrgCountryData data2 = factory.New<OrgCountryData>();
			data2.OV_OH_OrgHeader = otherPK;
			data2.OV_OA_ApprovedLocation = oldOrgHeader.MainAddress.PK;
			data2.OV_RN_NKClientCountryRelation = country2.RN_Code;
			ZGuid ov2 = data2.PK;

			OrgCountryData data3 = factory.New<OrgCountryData>();
			data3.OV_OH_OrgHeader = otherPK;
			data3.OV_OA_ApprovedLocation = newOrgHeader.MainAddress.PK;
			data3.OV_RN_NKClientCountryRelation = country2.RN_Code;
			ZGuid ov3 = data3.PK;

			OrgCountryData data4 = factory.New<OrgCountryData>();
			data4.OV_OH_OrgHeader = otherPK;
			data4.OV_OA_ApprovedLocation = adr2.PK;
			data4.OV_RN_NKClientCountryRelation = country1.RN_Code;
			ZGuid ov4 = data4.PK;

			OrgCountryData data5 = factory.New<OrgCountryData>();
			data5.OV_OH_OrgHeader = otherPK;
			data5.OV_OA_ApprovedLocation = adr2.PK;
			data5.OV_RN_NKClientCountryRelation = country2.RN_Code;
			ZGuid ov5 = data5.PK;
			factory.Save();

			MergeOrgAddressCollection adrCol = new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader);
			int i1 = 0, i2 = 1;
			if (adrCol[0].OldObjectCore.PK == adr2.PK)
			{
				i1 = 1;
				i2 = 0;
			}
			adrCol[i1].Action = MergeOrgAddress.ActionAdd;
			adrCol[i2].Action = MergeOrgAddress.ActionMerge;
			adrCol[i2].NewObjectPK = newOrgHeader.MainAddress.PK;
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, adrCol, new MergeOrgContactCollection(factory));
			testMerger.Save();

			factory = new BusinessObjectFactory();
			OrgCountryData assertData1 = factory.Load<OrgCountryData>(ov1);
			OrgCountryData assertData2 = factory.Load<OrgCountryData>(ov2);
			OrgCountryData assertData3 = factory.Load<OrgCountryData>(ov3);
			OrgCountryData assertData4 = factory.Load<OrgCountryData>(ov4);
			OrgCountryData assertData5 = factory.Load<OrgCountryData>(ov5);

			AssertNotNull(assertData1);
			AssertNotNull(assertData2);
			AssertNotNull(assertData3);
			AssertNotNull(assertData4);
			AssertNotNull(assertData5);

			AssertEquals(otherPK, assertData1.OV_OH_OrgHeader);
			AssertEquals(otherPK, assertData2.OV_OH_OrgHeader);
			AssertEquals(otherPK, assertData3.OV_OH_OrgHeader);
			AssertEquals(otherPK, assertData4.OV_OH_OrgHeader);
			AssertEquals(otherPK, assertData5.OV_OH_OrgHeader);

			OrgHeader assertNewOrg = factory.Load<OrgHeader>(newPK);
			AssertEquals(2, assertNewOrg.Addresses.Count);
			OrgAddress main = assertNewOrg.MainAddress;
			OrgAddress second = assertNewOrg.Addresses[0].PK == main.PK ? assertNewOrg.Addresses[1] : assertNewOrg.Addresses[0];

			AssertEquals(second.PK, assertData1.OV_OA_ApprovedLocation);
			AssertEquals(second.PK, assertData2.OV_OA_ApprovedLocation);
			AssertEquals(newOrgHeader.MainAddress.PK, assertData3.OV_OA_ApprovedLocation);
			AssertEquals(newOrgHeader.MainAddress.PK, assertData4.OV_OA_ApprovedLocation);
			AssertEquals(ZGuid.Empty, assertData5.OV_OA_ApprovedLocation);
		}

		public void TestDeleteOrgPatternMatchAddress()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			oldOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid oldPK = oldOrgHeader.PK;

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			newOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid newPK = newOrgHeader.PK;

			OrgPatternMatchAddress pat1 = factory.New<OrgPatternMatchAddress>();
			OrgPatternMatchAddress pat2 = factory.New<OrgPatternMatchAddress>();
			OrgPatternMatchAddress pat3 = factory.New<OrgPatternMatchAddress>();
			OrgPatternMatchAddress pat4 = factory.New<OrgPatternMatchAddress>();
			pat1.P3_OH_MatchOrg = oldPK;
			pat1.P3_ParentID = ZGuid.NewZGuid();
			pat2.P3_OH_MatchOrg = oldPK;
			pat2.P3_ParentID = ZGuid.NewZGuid();
			pat3.P3_OH_MatchOrg = newPK;
			pat3.P3_ParentID = ZGuid.NewZGuid();
			pat4.P3_OH_MatchOrg = newPK;
			pat4.P3_ParentID = ZGuid.NewZGuid();
			ZGuid item1 = pat1.PK;
			ZGuid item2 = pat2.PK;
			ZGuid item3 = pat3.PK;
			ZGuid item4 = pat4.PK;

			factory.Save();

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals("", mergeOrgHeader.DeleteError);
			BusinessObjectFactory assertFactory = new BusinessObjectFactory();

			AssertNull(assertFactory.Load<OrgPatternMatchAddress>(item1));
			AssertNull(assertFactory.Load<OrgPatternMatchAddress>(item2));
			AssertNotNull(assertFactory.Load<OrgPatternMatchAddress>(item3));
			AssertNotNull(assertFactory.Load<OrgPatternMatchAddress>(item4));
		}

		public void TestDeleteOrgMatchApproval()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			oldOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid oldPK = oldOrgHeader.PK;

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			newOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid newPK = newOrgHeader.PK;

			factory.Save();

			ZGuid item1 = ZGuid.NewZGuid();
			ZGuid item2 = ZGuid.NewZGuid();
			ZGuid item3 = ZGuid.NewZGuid();
			ZGuid item4 = ZGuid.NewZGuid();
			ZGuid item5 = ZGuid.NewZGuid();
			ZGuid item6 = ZGuid.NewZGuid();
			ZGuid item7 = ZGuid.NewZGuid();
			ZGuid item8 = ZGuid.NewZGuid();

			InsertOrgMatchApproval(item1, oldPK, 1);
			InsertOrgMatchApproval(item2, oldPK, 1);
			InsertOrgMatchApproval(item3, newPK, 1);
			InsertOrgMatchApproval(item4, newPK, 1);
			InsertOrgMatchApproval(item5, oldPK, 2);
			InsertOrgMatchApproval(item6, oldPK, 2);
			InsertOrgMatchApproval(item7, newPK, 2);
			InsertOrgMatchApproval(item8, newPK, 2);

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals("", mergeOrgHeader.DeleteError);
			BusinessObjectFactory assertFactory = new BusinessObjectFactory();

			AssertNull(assertFactory.Load<OrgMatchApproval>(item1));
			AssertNull(assertFactory.Load<OrgMatchApproval>(item2));
			AssertNotNull(assertFactory.Load<OrgMatchApproval>(item3));
			AssertNotNull(assertFactory.Load<OrgMatchApproval>(item4));
			AssertNull(assertFactory.Load<OrgMatchApproval>(item5));
			AssertNull(assertFactory.Load<OrgMatchApproval>(item6));
			AssertNotNull(assertFactory.Load<OrgMatchApproval>(item7));
			AssertNotNull(assertFactory.Load<OrgMatchApproval>(item8));
		}

		void InsertOrgMatchApproval(ZGuid pk, ZGuid org, int number)
		{
			Db.Connection.ExecuteNonQuery(string.Format("insert into dbo.orgmatchapproval (p2_pk, p2_oh_matchorg{0}, p2_parentid) values ('{1}', '{2}', newid())", number, pk, org));
		}

		public void TestMoveOrgDocument()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			oldOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid oldPK = oldOrgHeader.PK;

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			newOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid newPK = newOrgHeader.PK;

			OrgHeader otherOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			newOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid otherPK = otherOrgHeader.PK;

			OrgContact cnt1 = otherOrgHeader.Contacts.AddNew();
			cnt1.OC_ContactName = GetRandomString(OrgContactSchema.OC_ContactName.MaxLength);
			cnt1.OC_Email = "matchemail@email.com";
			OrgDocument doc1 = cnt1.Documents.AddNew();
			doc1.OD_DocumentGroup = "A/R";
			doc1.OD_DefaultContact = true;
			doc1.OD_OH_RelatedFilterByParty = oldPK;

			OrgContact cnt2 = otherOrgHeader.Contacts.AddNew();
			cnt2.OC_ContactName = GetRandomString(OrgContactSchema.OC_ContactName.MaxLength);
			cnt2.OC_Email = "different@email.com";
			OrgDocument doc2 = cnt2.Documents.AddNew();
			doc2.OD_DocumentGroup = "WHS";
			doc2.OD_DefaultContact = true;
			doc2.OD_OH_RelatedFilterByParty = newPK;

			OrgContact cnt3 = otherOrgHeader.Contacts.AddNew();
			cnt3.OC_ContactName = GetRandomString(OrgContactSchema.OC_ContactName.MaxLength);
			cnt3.OC_Email = "different@email.com";
			OrgDocument doc3 = cnt3.Documents.AddNew();
			doc3.OD_DocumentGroup = "WHS";
			doc3.OD_DefaultContact = true;
			doc3.OD_OH_RelatedFilterByParty = otherPK;

			factory.Save();

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			AssertEquals("", mergeOrgHeader.DeleteError);

			factory = new BusinessObjectFactory();
			cnt1 = factory.Load<OrgContact>(cnt1.PK);
			cnt2 = factory.Load<OrgContact>(cnt2.PK);
			cnt3 = factory.Load<OrgContact>(cnt3.PK);
			AssertNotNull(cnt1);
			AssertNotNull(cnt2);
			AssertNotNull(cnt3);
			AssertEquals(1, cnt1.Documents.Count);
			AssertEquals(1, cnt2.Documents.Count);
			AssertEquals(1, cnt3.Documents.Count);
			AssertEquals(newPK, cnt1.Documents[0].OD_OH_RelatedFilterByParty);
			AssertEquals(newPK, cnt2.Documents[0].OD_OH_RelatedFilterByParty);
			AssertEquals(otherPK, cnt3.Documents[0].OD_OH_RelatedFilterByParty);
		}

		public void TestMergeRatingConfigurationDifferInTariffType_SameCompany()
		{
			AssertMergeRatigConfiguration(("ABC", "DEF", "GHI"), ("123", "DEF", "GHI"), "Same Company, differ in Tariff Type", true);
		}

		public void TestMergeRatingConfigurationDifferInRatingMode_SameCompany()
		{
			AssertMergeRatigConfiguration(("ABC", "DEF", "GHI"), ("ABC", "123", "GHI"), "Same Company, differ in Rating Mode", true);
		}

		public void TestMergeRatingConfigurationDifferInRatingDirection_SameCompany()
		{
			AssertMergeRatigConfiguration(("ABC", "DEF", "GHI"), ("ABC", "DEF", "123"), "Same Company, differ in Rating Direction", true);
		}

		public void TestMergeRatingSameConfiguration_SameCompany()
		{
			AssertMergeRatigConfiguration(("ABC", "DEF", "GHI"), ("ABC", "DEF", "GHI"), "Same Company, same Rating Configuration", false);
		}

		public void TestMergeRatingConfiguration_DifferentCompany()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var oldOrgHeaderPK = oldOrgHeader.PK;

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgHeaderPK = newOrgHeader.PK;

			var company = factory.NewWithValidTestData<GlbCompany>();
			var companyPK = company.PK;

			var oldRateTariffLevel1 = CreateOrgRateTariffLevel(factory, oldOrgHeaderPK, Env.CurrentCompanyPK, "ABC", "DEF", "GHI");
			var oldRateTariffLevel2 = CreateOrgRateTariffLevel(factory, oldOrgHeaderPK, companyPK, "ABC", "DEF", "GHI");
			var newRateTariffLevel1 = CreateOrgRateTariffLevel(factory, newOrgHeaderPK, Env.CurrentCompanyPK, "ABC", "DEF", "GHI");
			var newRateTariffLevel2 = CreateOrgRateTariffLevel(factory, newOrgHeaderPK, companyPK, "123", "DEF", "GHI");

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			factory = new BusinessObjectFactory();
			AssertNull(factory.Load<OrgRateTariffLevel>(oldRateTariffLevel1.PK));
			AssertEquals(newOrgHeaderPK, factory.Load<OrgRateTariffLevel>(oldRateTariffLevel2.PK).P7_OH);
			AssertEquals(newOrgHeaderPK, factory.Load<OrgRateTariffLevel>(newRateTariffLevel1.PK).P7_OH);
			AssertEquals(newOrgHeaderPK, factory.Load<OrgRateTariffLevel>(newRateTariffLevel2.PK).P7_OH);
		}

		OrgRateTariffLevel CreateOrgRateTariffLevel(BusinessObjectFactory factory, ZGuid orgPK, ZGuid companyPK, string tariffType, string mode, string direction)
		{
			var orgRateTariffLevel = factory.NewWithValidTestData<OrgRateTariffLevel>();
			orgRateTariffLevel.P7_OH = orgPK;
			orgRateTariffLevel.P7_GC = companyPK;
			orgRateTariffLevel.P7_TariffType = tariffType;
			orgRateTariffLevel.P7_Mode = mode;
			orgRateTariffLevel.P7_Direction = direction;

			return orgRateTariffLevel;
		}

		void AssertMergeRatigConfiguration((string oldOrgRatingTariffType, string oldOrgRatingMode, string oldOrgRatingDirection) oldOrgRateTariffLevelCase,
			(string newOrgRatingTariffType, string newOrgRatingMode, string newOrgRatingDirection) newOrgRateTariffLevelCase, string message, bool shouldMove)
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();

			var oldOrgRateTariffLevel = factory.NewWithValidTestData<OrgRateTariffLevel>();
			oldOrgRateTariffLevel.P7_OH = oldOrgHeader.PK;
			oldOrgRateTariffLevel.P7_GC = Env.CurrentCompanyPK;
			oldOrgRateTariffLevel.P7_TariffType = oldOrgRateTariffLevelCase.oldOrgRatingTariffType;
			oldOrgRateTariffLevel.P7_Mode = oldOrgRateTariffLevelCase.oldOrgRatingMode;
			oldOrgRateTariffLevel.P7_Direction = oldOrgRateTariffLevelCase.oldOrgRatingDirection;

			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			var newOrgRateTariffLevel = factory.NewWithValidTestData<OrgRateTariffLevel>();
			newOrgRateTariffLevel.P7_OH = newOrgHeader.PK;
			newOrgRateTariffLevel.P7_GC = Env.CurrentCompanyPK;
			newOrgRateTariffLevel.P7_TariffType = newOrgRateTariffLevelCase.newOrgRatingTariffType;
			newOrgRateTariffLevel.P7_Mode = newOrgRateTariffLevelCase.newOrgRatingMode;
			newOrgRateTariffLevel.P7_Direction = newOrgRateTariffLevelCase.newOrgRatingDirection;

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			factory = new BusinessObjectFactory();
			oldOrgRateTariffLevel = factory.Load<OrgRateTariffLevel>(oldOrgRateTariffLevel.PK);

			if (shouldMove)
			{
				AssertEquals(message, newOrgHeader.PK, oldOrgRateTariffLevel.P7_OH);
			}
			else
			{
				AssertNull(message, oldOrgRateTariffLevel);
			}
		}

		public void TestMergeRateOneOffCarrier()
		{
			var factory = new BusinessObjectFactory();
			var oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";
			oldOrgHeader.MainAddress.OA_Code = "~zzz1~";
			var newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";
			newOrgHeader.MainAddress.OA_Code = "2";

			var otherOrgHeader = factory.NewWithValidTestData<OrgHeader>();

			var ratingOrg1 = factory.NewWithValidTestData<OrgHeader>();
			var ratingOrg2 = factory.NewWithValidTestData<OrgHeader>();

			factory.Save();

			var th1 = Guid.NewGuid();
			var th2 = Guid.NewGuid();

			var tt1 = Guid.NewGuid();
			var tt2 = Guid.NewGuid();

			InsertRatingHeader(th1, ratingOrg1.PK.ToGuid());
			InsertRatingHeader(th2, ratingOrg2.PK.ToGuid());
			InsertRateElement("RateOneOffShipment", "TT", th1, tt1);
			InsertRateElement("RateOneOffShipment", "TT", th2, tt2);
			InsertRateOneOffCarrier(tt1, newOrgHeader.PK);
			InsertRateOneOffCarrier(tt1, oldOrgHeader.PK);
			InsertRateOneOffCarrier(tt1, otherOrgHeader.PK);
			InsertRateOneOffCarrier(tt2, oldOrgHeader.PK);

			var testMerger = new OrganisationMergerForTest(oldOrgHeader.PK, newOrgHeader.PK, new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader), new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader));
			testMerger.Save();

			var rateOneOffCarrierDataTable = LoadDataTable(RateOneOffCarrierSchema.Constants.TableName);

			var quote1Rows = rateOneOffCarrierDataTable.Select("TTC_TT = '" + tt1.ToString() + "'");
			var quote2Rows = rateOneOffCarrierDataTable.Select("TTC_TT = '" + tt2.ToString() + "'");
			CombineAssertions(() =>
			{
				AssertEquals("Table row count", 3, rateOneOffCarrierDataTable.Rows.Count);
				AssertEquals("RateOneOffCarrier for quote 1", 2, quote1Rows.Length);
				AssertEquals("RateOneOffCarrier for quote 2", 1, quote2Rows.Length);
				AssertEquals("quote 1 has new carrier", 1, quote1Rows.Count(x => (Guid)x["TTC_OH_Carrier"] == newOrgHeader.PK));
				AssertEquals("quote 1 has other carrier", 1, quote1Rows.Count(x => (Guid)x["TTC_OH_Carrier"] == otherOrgHeader.PK));
				AssertEquals("quote 2 has new carrier", 1, quote1Rows.Count(x => (Guid)x["TTC_OH_Carrier"] == newOrgHeader.PK));
			});
		}

		void InsertRateOneOffCarrier(Guid rateOffShipmentPk, ZGuid carrierOrgPk)
		{
			string sql = $"insert into dbo.RateOneOffCarrier (TTC_PK, TTC_TT, TTC_OH_Carrier, TTC_SystemLastEditTimeUtc, TTC_SystemLastEditUser, TTC_SystemCreateTimeUtc, TTC_SystemCreateUser) " +
				$"values (newid(),'{rateOffShipmentPk}', '{carrierOrgPk}', GetUtcDate(), '{GlbStaff.CurrentUser.GS_Code}', GetUtcDate(), '{GlbStaff.CurrentUser.GS_Code}')";
			using (DbCommand cmd = Db.Connection.Command(sql)) // Need to use ODbCommand instead of BusinessObjectFactory due to circular reference with Rating solution.
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void TestCommissionAgreementCreateDraft()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);

			var opportunity1 = factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_OpportunityID = "O00001001";
			opportunity1.P8_OH = oldOrgHeader.PK;
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = oldOrgHeader.PK;
			agreement1.FillWithValidTestData();
			agreement1.Approve();

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);

			factory.Save();

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			var draftAgreement = factory.LoadTop1<OrgCommissionAgreement>(new ZQuery(OrgCommissionAgreementSchema.CA0_CA0_ParentVersion, agreement1.PK));

			AssertEquals("Draft Agreement must be Unapproved", ZDateTime.Empty, draftAgreement.CA0_LastApprovedDateUtc);
			AssertEquals("Draft Agreement doesnt have commissions", ZDateTime.Empty, draftAgreement.CA0_FirstUsageDateUtc);
		}

		string GetRandomString(int length)
		{
			return new ZString(Guid.NewGuid().ToString().Replace("-", "")).Left(length);
		}

		#region TestMergeProductStyles

		public void TestMergeProductStyles_WhenAllProductStylesDiffer()
		{
			var factory = new BusinessObjectFactory();
			var oldOrg = factory.NewWithValidTestData<OrgHeader>();
			var newOrg = factory.NewWithValidTestData<OrgHeader>();
			var differentOrg = factory.NewWithValidTestData<OrgHeader>();
			oldOrg.OH_Code = "oldOrg";
			newOrg.OH_Code = "newOrg";
			differentOrg.OH_Code = "differentOrg";
			factory.Save();

			var oldOrgPK = oldOrg.PK;
			var oldOrgCode = oldOrg.OH_Code;

			var whsProductStyle1 = new WhsProductStyle("A", "A - Style", oldOrgPK.ToGuid()).InsertAndReturnObject(TestConnection);
			var whsProductStyle2 = new WhsProductStyle("B", "B - Style", oldOrgPK.ToGuid()).InsertAndReturnObject(TestConnection);
			var whsProductStyle3 = new WhsProductStyle("C", "C - Style", newOrg.PK.ToGuid()).InsertAndReturnObject(TestConnection);
			var whsProductStyle4 = new WhsProductStyle("D", "D - Style", differentOrg.PK.ToGuid()).InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();
			AssertOrgMergeForProductStyle(whsProductStyle1.PK, newOrg.PK, "A");
			AssertLogsForProductStyleOwnerMerging(whsProductStyle1.PK, $"Owner has been changed. Old Owner PK was {oldOrgPK} and Code was {oldOrgCode}");

			AssertOrgMergeForProductStyle(whsProductStyle2.PK, newOrg.PK, "B");
			AssertLogsForProductStyleOwnerMerging(whsProductStyle2.PK, $"Owner has been changed. Old Owner PK was {oldOrgPK} and Code was {oldOrgCode}");

			AssertOrgMergeForProductStyle(whsProductStyle3.PK, newOrg.PK, "C");
			AssertLogsForProductStyleOwnerMerging(whsProductStyle3.PK);

			AssertOrgMergeForProductStyle(whsProductStyle4.PK, differentOrg.PK, "D");
			AssertLogsForProductStyleOwnerMerging(whsProductStyle4.PK);
		}

		public void TestMergeProductStyles_DoesNotThrowForLargeQuery()
		{
			var factory = new BusinessObjectFactory();
			var oldOrg = factory.NewWithValidTestData<OrgHeader>();
			var newOrg = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var sqlBuilder = new StringBuilder();
			var whsProductStyles = new WhsProductStyle[2101];
			for (int i = 0; i <= 2100; i++)
			{
				whsProductStyles[i] = new WhsProductStyle("A-" + i, "A-" + i + "-Style", oldOrg.PK.ToGuid());
			}
			TestConnection.ExecuteNonQuery(WhsProductStyle.GetBulkInsertStatement(whsProductStyles));

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));

			AssertNoExceptionThrown(() => testMerger.Save());
		}

		void AssertOrgMergeForProductStyle(ZGuid pk, ZGuid expectedOwnerPK, string expectedCode)
		{
			WhsProductStyle.AssertFromDB(TestConnection, pk.ToGuid())
				.ExpectEquals("There must be only one product style for the owner", s => s.WST_Code, expectedCode)
				.ExpectEquals("There must be only one product style for the owner", s => s.WST_OH_Owner, expectedOwnerPK)
				.VerifyAll();
		}

		void AssertLogsForProductStyleOwnerMerging(ZGuid stylePK, string expectedReference = null)
		{
			if (!string.IsNullOrEmpty(expectedReference))
			{
				AssertEquals("There must be a log entry for the old owner of the product style.", 1, StmALogDO.CountInDB(TestConnection, o => o.SL_Parent == stylePK && o.SL_Table == "WhsProductStyle" && o.SL_Reference == expectedReference));
			}
			else
			{
				AssertEquals("Owner hasn't been changed therefore there must not be a log.", 0, StmALogDO.CountInDB(TestConnection, o => o.SL_Parent == stylePK && o.SL_Table == "WhsProductStyle"));
			}
		}

		#endregion

		#region TestMergeWhsAdHocServiceJobsWorkItemBase

		public void TestMergeWhsAdHocServiceJobs()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var orgClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(orgClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);

			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);

			var warehouse = helper.CreateWarehouse("Whs1", "A");

			var whsAdHocJobWithOrgClient = helper.CreateWhsAdHocServiceJob(warehouse.PK, orgClientPK, ZDateTime.Today);
			var whsAdHocJobWithNewClient = helper.CreateWhsAdHocServiceJob(warehouse.PK, newClientPK, ZDateTime.Today);
			var whsAdHocJobWithDifClient = helper.CreateWhsAdHocServiceJob(warehouse.PK, difClientPK, ZDateTime.Today);

			factory.Save();

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 WhsAdHocServiceJob link to NewOrg, 0 to oldOrg, 1 to difOrg
			AssertOrgMergeForWhsAdHocServiceJob(newClientPK, 2);
			AssertOrgMergeForWhsAdHocServiceJob(orgClientPK, 0);
			AssertOrgMergeForWhsAdHocServiceJob(difClientPK, 1);
		}

		void AssertOrgMergeForWhsAdHocServiceJob(ZGuid orgHeaderPK, int expectedCount)
		{
			AssertEquals("Should find number of linked Adhoc Service records.", expectedCount, WhsAdHocServiceJob.CountInDB(TestConnection, o => o.WSJ_OH_Client == orgHeaderPK));
		}

		#endregion

		#region TestWhsAdHocServiceJobMerge

		public void TestWhsAdHocServiceJobMerge_NoDuplicateCustomerReference()
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("Whs", "Row");
			factory.Save();

			var whsAdHocServiceJob1 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgOld.PK.ToGuid(), "Job1", "CustomerReference0", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);
			var whsAdHocServiceJob2 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgNew.PK.ToGuid(), "Job2", "CustomerReference1", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);
			var whsAdHocServiceJob3 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgNew.PK.ToGuid(), "Job3", "CustomerReference2", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);

			AssertEquals("Precondition: There are 3 warehouse ad hoc service jobs.", 3, WhsAdHocServiceJob.CountInDB(TestConnection));

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			AssertNoExceptionThrown("Merge should be completed with no errors.", testMerger.Save);

			AssertNull("Old organisation should be deleted.", new BusinessObjectFactory().Load<OrgHeader>(orgOld.PK));
			AssertEquals("No ad hoc service jobs should be deleted.", 3, WhsAdHocServiceJob.CountInDB(TestConnection));
			WhsAdHocServiceJob.AssertFromDB(TestConnection, whsAdHocServiceJob1.PK)
				.ExpectEquals("Ad Hoc Service Job with old client should have client FK changed.", o => o.WSJ_OH_Client, orgNew.PK.ToGuid())
				.VerifyAll();
		}

		public void TestWhsAdHocServiceJobMerge_DuplicateCustomerReference()
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("Whs", "Row");
			factory.Save();

			var whsAdHocServiceJob1 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgOld.PK.ToGuid(), "Job1", "CustomerReference1", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);
			var whsAdHocServiceJob2 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgNew.PK.ToGuid(), "Job2", "CustomerReference1", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);
			var whsAdHocServiceJob3 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgNew.PK.ToGuid(), "Job3", "CustomerReference2", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);

			AssertEquals("Precondition: There are 3 warehouse ad hoc service jobs.", 3, WhsAdHocServiceJob.CountInDB(TestConnection));

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			AssertNoExceptionThrown("Merge should be completed with no errors.", testMerger.Save);

			AssertNull("Old organisation should be deleted.", new BusinessObjectFactory().Load<OrgHeader>(orgOld.PK));
			AssertEquals("No ad hoc service jobs should be deleted.", 3, WhsAdHocServiceJob.CountInDB(TestConnection));

			WhsAdHocServiceJob.AssertFromDB(TestConnection, whsAdHocServiceJob1.PK)
				.ExpectEquals("Ad Hoc Service Job with old client and duplicate customer reference should have customer reference changed.", o => o.WSJ_CustomerReference, "CustomerReference1_001")
				.ExpectEquals("Ad Hoc Service Job with old client and duplicate customer reference should have WSJ_OH_Client FK changed.", o => o.WSJ_OH_Client, orgNew.PK.ToGuid())
				.VerifyAll();
		}

		public void TestWhsAdHocServiceJobMerge_NewCustomerReferenceAlreadyExists()
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("Whs", "Row");
			factory.Save();

			var whsAdHocServiceJob1 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgOld.PK.ToGuid(), "Job1", "CustomerReference1", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);
			var whsAdHocServiceJob2 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgNew.PK.ToGuid(), "Job2", "CustomerReference1", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);
			var whsAdHocServiceJob3 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgNew.PK.ToGuid(), "Job3", "CustomerReference1_001", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);

			AssertEquals("Precondition: There are 3 warehouse ad hoc service jobs.", 3, WhsAdHocServiceJob.CountInDB(TestConnection));

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			AssertNoExceptionThrown("Merge should be completed with no errors.", testMerger.Save);

			AssertNull("Old organisation should be deleted.", new BusinessObjectFactory().Load<OrgHeader>(orgOld.PK));
			AssertEquals("No ad hoc service jobs should be deleted.", 3, WhsAdHocServiceJob.CountInDB(TestConnection));

			WhsAdHocServiceJob.AssertFromDB(TestConnection, whsAdHocServiceJob1.PK)
				.ExpectEquals("Ad Hoc Service Job with old client and duplicate customer reference should have customer reference changed to a new unique reference.", o => o.WSJ_CustomerReference, "CustomerReference1_002")
				.ExpectEquals("Ad Hoc Service Job with old client and duplicate customer reference should have WSJ_OH_Client FK changed.", o => o.WSJ_OH_Client, orgNew.PK.ToGuid())
				.VerifyAll();
		}

		public void TestWhsAdHocServiceJobMerge_DuplcateCustomerReferenceHitsCharacterLimit()
		{
			var factory = new BusinessObjectFactory();
			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("Whs", "Row");
			factory.Save();

			var whsAdHocServiceJob1 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgOld.PK.ToGuid(), "Job1", "CustomerReference890123456789012345", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);
			var whsAdHocServiceJob2 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgNew.PK.ToGuid(), "Job2", "CustomerReference890123456789012345", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);
			var whsAdHocServiceJob3 = new WhsAdHocServiceJob(whs.PK.ToGuid(), orgNew.PK.ToGuid(), "Job3", "CustomerReference89012345678901_001", new DateTime(2019, 10, 1)).InsertAndReturnObject(TestConnection);

			AssertEquals("Precondition: There are 3 warehouse ad hoc service jobs.", 3, WhsAdHocServiceJob.CountInDB(TestConnection));

			var testMerger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));
			AssertNoExceptionThrown("Merge should be completed with no errors.", testMerger.Save);

			AssertNull("Old organisation should be deleted.", new BusinessObjectFactory().Load<OrgHeader>(orgOld.PK));
			AssertEquals("No ad hoc service jobs should be deleted.", 3, WhsAdHocServiceJob.CountInDB(TestConnection));

			WhsAdHocServiceJob.AssertFromDB(TestConnection, whsAdHocServiceJob1.PK)
				.ExpectEquals("Ad Hoc Service Job with old client and duplicate customer reference should have customer reference changed to a new unique reference.", o => o.WSJ_CustomerReference, "CustomerReference89012345678901_002")
				.ExpectEquals("Ad Hoc Service Job with old client and duplicate customer reference should have WSJ_OH_Client FK changed.", o => o.WSJ_OH_Client, orgNew.PK.ToGuid())
				.VerifyAll();
		}

		#endregion

		public void TestGenAddOnColumn()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			oldOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid oldPK = oldOrgHeader.PK;

			OrgHeader newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			newOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid newPK = newOrgHeader.PK;

			OrgHeader otherOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			newOrgHeader.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			ZGuid otherPK = otherOrgHeader.PK;

			factory.Save();

			ZGuid g1 = ZGuid.NewZGuid();
			ZGuid g2 = ZGuid.NewZGuid();
			ZGuid g3 = ZGuid.NewZGuid();
			ZGuid g4 = ZGuid.NewZGuid();
			ZGuid g5 = ZGuid.NewZGuid();
			ZGuid g6 = ZGuid.NewZGuid();
			ZGuid g7 = ZGuid.NewZGuid();
			ZGuid g8 = ZGuid.NewZGuid();
			ZGuid g9 = ZGuid.NewZGuid();

			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g1 + "', 'US_OH_InBondCarrier', '" + oldPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g2 + "', 'OH_InBondCarrier', '" + oldPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g3 + "', 'US_OH', '" + oldPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g4 + "', 'USOH_InBondCarrier', '" + oldPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g5 + "', 'OH', '" + oldPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g6 + "', 'OH__InBondCarrier', '" + oldPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g7 + "', 'OHInBondCarrier', '" + oldPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g8 + "', 'US_OH_InBondCarrier', '" + newPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = GetCommandOnMainConnection("insert into dbo.GenAddOnColumn(XA_PK, XA_Name, XA_Data, XA_ParentID) values ('" + g9 + "', 'US_OH_InBondCarrier', '" + otherPK + "', newid())"))
			{
				cmd.ExecuteNonQuery();
			}

			MergeOrgHeader mergeOrgHeader = new MergeOrgHeader(factory, oldOrgHeader, newOrgHeader);
			OrganisationMergerForTest testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.Save();

			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g1 + "' and XA_Data = '" + newPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g2 + "' and XA_Data = '" + newPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g3 + "' and XA_Data = '" + newPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g4 + "' and XA_Data = '" + oldPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g5 + "' and XA_Data = '" + oldPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g6 + "' and XA_Data = '" + newPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g7 + "' and XA_Data = '" + oldPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g8 + "' and XA_Data = '" + newPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
			using (var cmd = GetCommandOnMainConnection("select count(*) from dbo.GenAddOnColumn where XA_PK = '" + g9 + "' and XA_Data = '" + otherPK + "'"))
			{
				AssertEquals(1, (int)cmd.ExecuteScalar());
			}
		}

		DbCommand GetCommandOnMainConnection(string sqlText)
		{
			return Db.Connection.Command(sqlText); // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
		}

		public void TestWarehouseInventoryTriggerDoesNotBlowUp()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var org1PK = helper.CreateClient("ORG1");
			var org2PK = helper.CreateClient("ORG2");
			var whs = helper.CreateWarehouse("WHS-1", "WH1");

			var product1 = helper.CreateProduct(org1PK, "P1");
			var product2 = helper.CreateProduct(org2PK, "P2");

			var receive1 = helper.CreateWhsReceive(org1PK, whs.PK, "R1", new NotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive1, product1.PK, 10, ZGuid.Empty);
			var receive2 = helper.CreateWhsReceive(org2PK, whs.PK, "R2", new NotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive2, product2.PK, 10, ZGuid.Empty);

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, factory.Load<OrgHeader>(org1PK), factory.Load<OrgHeader>(org2PK));
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertNoExceptionThrown("It seems that WhsInventory was updated before WhsDocket. Check ORDER BY clause in XT_MoveFkReferences.", () => testMerger.Save());
		}

		#region TestTGJobComInvoiceHeaderSupplierAddressTrigger

		public void TestTGJobComInvoiceHeaderSupplierAddressTrigger_AddressMovedToNewOrg_DoesNotThrowException()
		{
			var factory = new BusinessObjectFactory();

			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrgPk = dissolvedOrg.PK;

			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();

			var addr = dissolvedOrg.Addresses.AddNew();
			addr.OA_Address1 = "TEST_ADDRESS_1";

			factory.Save();

			var invHeaderPk = ZGuid.NewZGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				VALUES ('{invHeaderPk}', 'AU', '{GlbBranch.CurrentBranch.PK}', 'INV1', '{dissolvedOrg.PK}', '{addr.PK}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var invSupplier = (Guid)TestConnection.ExecuteScalar($"Select JZ_OH_Supplier from dbo.JobComInvoiceHeader WHERE JZ_PK='{invHeaderPk}'");
			CombineAssertions("Pre-condition:", () =>
			{
				AssertEquals("Invoice supplier points to dissolved org", dissolvedOrgPk, invSupplier);
				AssertEquals("Retained org does not have address from dissolved org", false, retainedOrg.Addresses.Contains(addr.PK));
			});

			var mergeOrgHeader = new MergeOrgHeader(factory, dissolvedOrg, retainedOrg);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertNoExceptionThrown("Should not throw exception", () => testMerger.Save());

			var newFactory = new BusinessObjectFactory();
			var retainedOrgReload = newFactory.Load<OrgHeader>(retainedOrg.PK);
			AssertNotNull("Retained org should exist", retainedOrgReload);
			AssertEquals("Retained org should have address from dissolved org", true, retainedOrgReload.Addresses.Contains(addr.PK));

			var addrReload = newFactory.Load<OrgAddress>(addr.PK);
			AssertNotNull("Address from dissolved org should exist", addrReload);
			AssertEquals("Address from dissolved org should point to new org", retainedOrgReload.PK, addrReload.OA_OH);

			AssertEquals("Invoice supplier should point to retained org", retainedOrgReload.PK.ToGuid(), GetInvSupplierGuid(invHeaderPk));
		}

		public void TestTGJobComInvoiceHeaderSupplierAddressTrigger_AddressMergedToNewOrg_DoesNotThrowException()
		{
			var factory = new BusinessObjectFactory();

			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrgPk = dissolvedOrg.PK;
			var dissolvedAddr = dissolvedOrg.Addresses.AddNew();
			dissolvedAddr.OA_Address1 = "TEST_ADDRESS_1";

			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();
			var retainedAddr = retainedOrg.Addresses.AddNew();
			retainedAddr.OA_Address1 = "TEST_ADDRESS_1";

			factory.Save();

			var invHeaderPk = ZGuid.NewZGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_CLusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				VALUES ('{invHeaderPk}', 'AU', '{GlbBranch.CurrentBranch.PK}', 'INV1', '{dissolvedOrg.PK}', '{dissolvedAddr.PK}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var invSupplier = (Guid)TestConnection.ExecuteScalar($"Select JZ_OH_Supplier from dbo.JobComInvoiceHeader WHERE JZ_PK='{invHeaderPk}'");
			CombineAssertions("Pre-condition:", () =>
			{
				AssertEquals("Invoice supplier points to dissolved org", dissolvedOrgPk, invSupplier);
				AssertEquals("Retained org does not have address from dissolved org", false, retainedOrg.Addresses.Contains(dissolvedAddr.PK));
			});

			var mergeOrgHeader = new MergeOrgHeader(factory, dissolvedOrg, retainedOrg);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertNoExceptionThrown("Should not throw exception", () => testMerger.Save());

			var newFactory = new BusinessObjectFactory();
			var retainedOrgReload = newFactory.Load<OrgHeader>(retainedOrg.PK);
			AssertNotNull("Retained org should exist", retainedOrgReload);
			AssertEquals("Retained org should not have address from dissolved org", false, retainedOrgReload.Addresses.Contains(dissolvedAddr.PK));

			AssertEquals("Invoice supplier should point to retained org", retainedOrgReload.PK.ToGuid(), GetInvSupplierGuid(invHeaderPk));

			AssertEquals("Invoice supplier address should be in retained org", true, retainedOrgReload.Addresses.Contains(GetInvSupplierAddressGuid(invHeaderPk)));
		}

		public void TestTGJobComInvoiceHeaderSupplierAddressTrigger_AddressMovedAndMergedToNewOrg_DoesNotThrowException()
		{
			var factory = new BusinessObjectFactory();

			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			var dissolvedOrgPk = dissolvedOrg.PK;

			var dissolvedAddr1 = dissolvedOrg.Addresses.AddNew();
			dissolvedAddr1.OA_Address1 = "TEST_ADDRESS_1"; //this will be merged (deleted) as it has the same address as retainedAddr
			var dissolvedAddr2 = dissolvedOrg.Addresses.AddNew();
			dissolvedAddr2.OA_Address1 = "TEST_ADDRESS_2"; //this will be moved

			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();
			var retainedAddr = retainedOrg.Addresses.AddNew();
			retainedAddr.OA_Address1 = "TEST_ADDRESS_1";

			factory.Save();

			var invHeader1Pk = ZGuid.NewZGuid();
			var invHeader2Pk = ZGuid.NewZGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				VALUES ('{invHeader1Pk}', 'AU', '{GlbBranch.CurrentBranch.PK}', 'INV1', '{dissolvedOrg.PK}', '{dissolvedAddr1.PK}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");
			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				VALUES ('{invHeader2Pk}', 'AU', '{GlbBranch.CurrentBranch.PK}', 'INV1', '{dissolvedOrg.PK}', '{dissolvedAddr2.PK}', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var invSupplier1 = (Guid)TestConnection.ExecuteScalar($"Select JZ_OH_Supplier from dbo.JobComInvoiceHeader WHERE JZ_PK='{invHeader1Pk}'");
			var invSupplier2 = (Guid)TestConnection.ExecuteScalar($"Select JZ_OH_Supplier from dbo.JobComInvoiceHeader WHERE JZ_PK='{invHeader2Pk}'");
			CombineAssertions("Pre-condition:", () =>
			{
				AssertEquals("Invoice supplier 1 points to dissolved org", dissolvedOrgPk, invSupplier1);
				AssertEquals("Invoice supplier 2 points to dissolved org", dissolvedOrgPk, invSupplier2);
				AssertEquals("Retained org does not have address 1 from dissolved org", false, retainedOrg.Addresses.Contains(dissolvedAddr1.PK));
				AssertEquals("Retained org does not have address 2 from dissolved org", false, retainedOrg.Addresses.Contains(dissolvedAddr2.PK));
			});

			var mergeOrgHeader = new MergeOrgHeader(factory, dissolvedOrg, retainedOrg);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertNoExceptionThrown("Should not throw exception", () => testMerger.Save());

			var newFactory = new BusinessObjectFactory();
			var retainedOrgReload = newFactory.Load<OrgHeader>(retainedOrg.PK);
			AssertNotNull("Retained org should exist", retainedOrgReload);
			AssertEquals("Retained org should not have address 1 from dissolved org", false, retainedOrgReload.Addresses.Contains(dissolvedAddr1.PK));
			AssertEquals("Retained org should have address 2 from dissolved org", true, retainedOrgReload.Addresses.Contains(dissolvedAddr2.PK));

			CombineAssertions(() =>
			{
				AssertEquals("Invoice supplier 1 should point to retained org", retainedOrgReload.PK.ToGuid(), GetInvSupplierGuid(invHeader1Pk));
				AssertEquals("Invoice supplier 2 should point to retained org", retainedOrgReload.PK.ToGuid(), GetInvSupplierGuid(invHeader2Pk));

				AssertEquals("Invoice supplier address 1 should be in retained org", true, retainedOrgReload.Addresses.Contains(GetInvSupplierAddressGuid(invHeader1Pk)));
				AssertEquals("Invoice supplier address 2 should be in retained org", true, retainedOrgReload.Addresses.Contains(GetInvSupplierAddressGuid(invHeader2Pk)));
			});
		}

		Guid GetInvSupplierGuid(ZGuid invHeaderPk) => (Guid)TestConnection.ExecuteScalar($"Select JZ_OH_Supplier from dbo.JobComInvoiceHeader WHERE JZ_PK='{invHeaderPk}'");

		Guid GetInvSupplierAddressGuid(ZGuid invHeaderPk) => (Guid)TestConnection.ExecuteScalar($"Select JZ_OA_SupplierAddress from dbo.JobComInvoiceHeader WHERE JZ_PK='{invHeaderPk}'");

		#endregion

		#region TestTGJobDeclarationImporterAddressTrigger/SupplierAddressTrigger

		public void TestJobDeclarationImpoterAndSupplier_AddressMovedToNewOrg_DoesNotThrowException()
		{
			var factory = new BusinessObjectFactory();

			var branch = factory.NewWithValidTestData<GlbBranch>();

			var dissolvedOrg1 = factory.NewWithValidTestData<OrgHeader>();
			var dissolvedAddr1 = dissolvedOrg1.Addresses.AddNew();
			dissolvedAddr1.OA_Address1 = "TEST_ADDRESS_1";
			var retainedOrg1 = factory.NewWithValidTestData<OrgHeader>();

			var dissolvedOrg2 = factory.NewWithValidTestData<OrgHeader>();
			var dissolvedAddr2 = dissolvedOrg2.Addresses.AddNew();
			dissolvedAddr2.OA_Address1 = "TEST_ADDRESS_1";
			var retainedOrg2 = factory.NewWithValidTestData<OrgHeader>();
			var retainedAddr2 = retainedOrg2.Addresses.AddNew();
			retainedAddr2.OA_Address1 = "TEST_ADDRESS_1";

			var dissolvedOrg3 = factory.NewWithValidTestData<OrgHeader>();
			var dissolvedAddr31 = dissolvedOrg3.Addresses.AddNew();
			dissolvedAddr31.OA_Address1 = "TEST_ADDRESS_1";
			var dissolvedAddr32 = dissolvedOrg3.Addresses.AddNew();
			dissolvedAddr32.OA_Address1 = "TEST_ADDRESS_2";
			var retainedOrg3 = factory.NewWithValidTestData<OrgHeader>();
			var retainedAddr3 = retainedOrg3.Addresses.AddNew();
			retainedAddr3.OA_Address1 = "TEST_ADDRESS_1";

			factory.Save();

			var decPk1 = ZGuid.NewZGuid();
			var decPk2 = ZGuid.NewZGuid();
			var decPk3 = ZGuid.NewZGuid();
			var decPk4 = ZGuid.NewZGuid();

			CreateJobDeclarationForTesting(decPk1, dissolvedOrg1.PK, dissolvedAddr1.PK, branch.PK, branch.GB_GC, 1);
			CreateJobDeclarationForTesting(decPk2, dissolvedOrg2.PK, dissolvedAddr2.PK, branch.PK, branch.GB_GC, 2);
			CreateJobDeclarationForTesting(decPk3, dissolvedOrg3.PK, dissolvedAddr31.PK, branch.PK, branch.GB_GC, 3);
			CreateJobDeclarationForTesting(decPk4, dissolvedOrg3.PK, dissolvedAddr32.PK, branch.PK, branch.GB_GC, 4);

			AssertNoExceptionFromMerger(factory, dissolvedOrg1, retainedOrg1);
			AssertNoExceptionFromMerger(factory, dissolvedOrg2, retainedOrg2);
			AssertNoExceptionFromMerger(factory, dissolvedOrg3, retainedOrg3);

			var newFactory = new BusinessObjectFactory();
			var retainedOrgReload1 = newFactory.Load<OrgHeader>(retainedOrg1.PK);
			AssertEquals("Retained org should have address from dissolved org", true, retainedOrgReload1.Addresses.Contains(dissolvedAddr1.PK));

			var addrReload1 = newFactory.Load<OrgAddress>(dissolvedAddr1.PK);
			AssertEquals("Address from dissolved org should point to new org", retainedOrgReload1.PK, addrReload1.OA_OH);
			AssertRetainedOrgHeaderOnImporterAndSupplier(retainedOrgReload1.PK, decPk1);

			var retainedOrgReload2 = newFactory.Load<OrgHeader>(retainedOrg2.PK);
			AssertEquals("Retained org should not have address from dissolved org", false, retainedOrgReload2.Addresses.Contains(dissolvedAddr2.PK));
			AssertRetainedOrgHeaderOnImporterAndSupplier(retainedOrgReload2.PK, decPk2);

			AssertRetainedOrgAddressCollectionOnImporterAndSupplier(retainedOrgReload2.Addresses, decPk2);

			var retainedOrgReload3 = newFactory.Load<OrgHeader>(retainedOrg3.PK);
			AssertEquals("Retained org should not have address 1 from dissolved org", false, retainedOrgReload3.Addresses.Contains(dissolvedAddr31.PK));
			AssertEquals("Retained org should have address 2 from dissolved org", true, retainedOrgReload3.Addresses.Contains(dissolvedAddr32.PK));
			AssertRetainedOrgHeaderOnImporterAndSupplier(retainedOrgReload3.PK, decPk3);
			AssertRetainedOrgHeaderOnImporterAndSupplier(retainedOrgReload3.PK, decPk4);
			AssertRetainedOrgAddressCollectionOnImporterAndSupplier(retainedOrgReload3.Addresses, decPk3);
			AssertRetainedOrgAddressCollectionOnImporterAndSupplier(retainedOrgReload3.Addresses, decPk4);
		}

		void AssertRetainedOrgAddressCollectionOnImporterAndSupplier(OrgAddressDependentCollection addressCollection, ZGuid decPK)
		{
			AssertEquals("supplier address should be in retained org", true, addressCollection.Contains(GetGuidValueFromDeclaration(decPK, JobDeclarationSchema.JE_OA_SupplierAddress)));
			AssertEquals("Importer address should be in retained org", true, addressCollection.Contains(GetGuidValueFromDeclaration(decPK, JobDeclarationSchema.JE_OA_ImporterAddress)));
		}

		void AssertRetainedOrgHeaderOnImporterAndSupplier(ZGuid expectedOrg, ZGuid decPK)
		{
			AssertEquals("Supplier should point to retained org", expectedOrg.ToGuid(), GetGuidValueFromDeclaration(decPK, JobDeclarationSchema.JE_OH_Supplier));
			AssertEquals("Importer should point to retained org", expectedOrg.ToGuid(), GetGuidValueFromDeclaration(decPK, JobDeclarationSchema.JE_OH_Importer));
		}

		void CreateJobDeclarationForTesting(ZGuid decPK, ZGuid dissolvedOrgPk, ZGuid dissolvedAddressPk, ZGuid branchPK, ZGuid companyPK, ZInt clusterKey)
		{
			var declarationRef = Guid.NewGuid().ToString("n");
			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_OH_Supplier, JE_OA_SupplierAddress, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_DeclarationReference)
				VALUES ('{decPK}', 'AU', '{dissolvedOrgPk}', '{dissolvedAddressPk}', '{dissolvedOrgPk}', '{dissolvedAddressPk}', '{branchPK}', '{companyPK}', {clusterKey}, GetUtcDate(), '~BP', GetUtcDate(), '~BP', '{declarationRef}')
			");
		}

		void AssertNoExceptionFromMerger(BusinessObjectFactory factory, OrgHeader dissolvedOrg, OrgHeader retainedOrg)
		{
			var mergeOrgHeader = new MergeOrgHeader(factory, dissolvedOrg, retainedOrg);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			AssertNoExceptionThrown("Should not throw exception", () => testMerger.Save());
		}

		Guid GetGuidValueFromDeclaration(ZGuid decPk, SchemaGuidColumn column) => (Guid)TestConnection.ExecuteScalar($"Select {column.Name} from dbo.JobDeclaration WHERE JE_PK='{decPk}'");

		#endregion

		public void TestCustomFieldsAreMerged()
		{
			var factory = new BusinessObjectFactory();
			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();

			factory.Save();

			var retainedOrgCustomField1 = factory.New<GenCustomAddOnValue>();
			retainedOrgCustomField1.XV_ParentID = retainedOrg.PK;
			retainedOrgCustomField1.XV_ParentTableCode = "OH";
			retainedOrgCustomField1.XV_Name = "Test1";
			retainedOrgCustomField1.XV_Type = "BOO";
			retainedOrgCustomField1.XV_Data = "Y";

			var retainedOrgCustomField2 = factory.New<GenCustomAddOnValue>();
			retainedOrgCustomField2.XV_ParentID = retainedOrg.PK;
			retainedOrgCustomField2.XV_ParentTableCode = "OH";
			retainedOrgCustomField2.XV_Name = "Test2";
			retainedOrgCustomField2.XV_Type = "STR";
			retainedOrgCustomField2.XV_Data = "ABC";

			var dissolvedOrgCustomField1 = factory.New<GenCustomAddOnValue>();
			dissolvedOrgCustomField1.XV_ParentID = dissolvedOrg.PK;
			dissolvedOrgCustomField1.XV_ParentTableCode = "OH";
			dissolvedOrgCustomField1.XV_Name = "Test2";
			dissolvedOrgCustomField1.XV_Type = "STR";
			dissolvedOrgCustomField1.XV_Data = "123";

			var dissolvedOrgCustomField2 = factory.New<GenCustomAddOnValue>();
			dissolvedOrgCustomField2.XV_ParentID = dissolvedOrg.PK;
			dissolvedOrgCustomField2.XV_ParentTableCode = "OH";
			dissolvedOrgCustomField2.XV_Name = "Test2";
			dissolvedOrgCustomField2.XV_Type = "BOO";
			dissolvedOrgCustomField2.XV_Data = "Y";

			var dissolvedOrgCustomField3 = factory.New<GenCustomAddOnValue>();
			dissolvedOrgCustomField3.XV_ParentID = dissolvedOrg.PK;
			dissolvedOrgCustomField3.XV_ParentTableCode = "OH";
			dissolvedOrgCustomField3.XV_Name = "Test3";
			dissolvedOrgCustomField3.XV_Type = "STR";
			dissolvedOrgCustomField3.XV_Data = "DEF";

			factory.Save();
			AssertEquals("Precondition: There are 2 Custom Fields Rows whose Parent is retained Org.", 2, (int)TestConnection.ExecuteScalar(string.Format("select count(*) from dbo.GenCustomAddOnValue where XV_ParentID = '{0}'", retainedOrg.PK.ToString())));
			AssertEquals("Precondition: There are 3 Custom Fields Rows whose Parent is dissolved Org.", 3, (int)TestConnection.ExecuteScalar(string.Format("select count(*) from dbo.GenCustomAddOnValue where XV_ParentID = '{0}'", dissolvedOrg.PK.ToString())));

			var mergeOrgHeader = new MergeOrgHeader(factory, dissolvedOrg, retainedOrg);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;
			testMerger.Save();

			var fieldData1 = TestConnection.ExecuteScalar(string.Format("select XV_Data from dbo.GenCustomAddOnValue where XV_ParentID = '{0}' and XV_Name = 'Test1' and XV_Type = 'BOO' and XV_ParentTableCode = 'OH'", retainedOrg.PK.ToString()));
			var fieldData2 = TestConnection.ExecuteScalar(string.Format("select XV_Data from dbo.GenCustomAddOnValue where XV_ParentID = '{0}' and XV_Name = 'Test2' and XV_Type = 'BOO' and XV_ParentTableCode = 'OH'", retainedOrg.PK.ToString()));
			var fieldData3 = TestConnection.ExecuteScalar(string.Format("select XV_Data from dbo.GenCustomAddOnValue where XV_ParentID = '{0}' and XV_Name = 'Test2' and XV_Type = 'STR' and XV_ParentTableCode = 'OH'", retainedOrg.PK.ToString()));
			var fieldData4 = TestConnection.ExecuteScalar(string.Format("select XV_Data from dbo.GenCustomAddOnValue where XV_ParentID = '{0}' and XV_Name = 'Test3' and XV_Type = 'STR' and XV_ParentTableCode = 'OH'", retainedOrg.PK.ToString()));
			CombineAssertions("If a custom field's value in retained org is not empty, then the value remains regardless of the same custom field in dissolved org. If a custom field's value in retained org is empty but not empty in dissolved org, then the value should be copied over.", () =>
			{
				AssertEquals("After Merge: There are 4 Custom Fields Rows whose Parent is retained Org.", 4, (int)TestConnection.ExecuteScalar(string.Format("select count(*) from dbo.GenCustomAddOnValue where XV_ParentID = '{0}'", retainedOrg.PK.ToString())));
				AssertEquals("After Merge: There are 0 Custom Fields Rows whose Parent is dissolved Org.", 0, (int)TestConnection.ExecuteScalar(string.Format("select count(*) from dbo.GenCustomAddOnValue where XV_ParentID = '{0}'", dissolvedOrg.PK.ToString())));
				AssertEquals("Y", fieldData1);
				AssertEquals("Y", fieldData2);
				AssertEquals("ABC", fieldData3);
				AssertEquals("DEF", fieldData4);
			});
		}

		public void TestCustomizationsAreMerged()
		{
			var factory = new BusinessObjectFactory();
			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();

			factory.Save();

			var customization1 = factory.NewWithValidTestData<OrgCustomLabels>();
			customization1.OT_FieldName = "OrderHeader.CustomDate1";
			customization1.OT_OH = retainedOrg.PK;
			customization1.OT_Caption = "retained caption";

			var customization2 = factory.NewWithValidTestData<OrgCustomLabels>();
			customization2.OT_FieldName = "OrderHeader.CustomDate2";
			customization2.OT_OH = dissolvedOrg.PK;
			customization2.OT_Caption = "dissolved caption";

			var customization3 = factory.NewWithValidTestData<OrgCustomLabels>();
			customization3.OT_FieldName = "OrderHeader.CustomDate1";
			customization3.OT_OH = dissolvedOrg.PK;
			customization3.OT_Caption = "duplicate dissolved caption";

			factory.Save();
			AssertEquals("Precondition: There is 1 customization Row whose Parent is retained Org.", 1, (int)TestConnection.ExecuteScalar(string.Format("select count(*) from dbo.OrgCustomLabels where OT_OH = '{0}'", retainedOrg.PK.ToString())));
			AssertEquals("Precondition: There are 2 customization Rows whose Parent is dissolved Org.", 2, (int)TestConnection.ExecuteScalar(string.Format("select count(*) from dbo.OrgCustomLabels where OT_OH = '{0}'", dissolvedOrg.PK.ToString())));

			var mergeOrgHeader = new MergeOrgHeader(factory, dissolvedOrg, retainedOrg);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;
			testMerger.Save();

			var caption1 = (string)TestConnection.ExecuteScalar(string.Format("select OT_Caption from dbo.OrgCustomLabels where OT_OH = '{0}' and OT_FieldName = 'OrderHeader.CustomDate1'", retainedOrg.PK.ToString()));
			var caption2 = (string)TestConnection.ExecuteScalar(string.Format("select OT_Caption from dbo.OrgCustomLabels where OT_OH = '{0}' and OT_FieldName = 'OrderHeader.CustomDate2'", retainedOrg.PK.ToString()));

			CombineAssertions("If a customization item's value in retained org exists, then the value remains regardless of the same customization in dissolved org. If a customization item's value in retained org does not exist but exists in dissolved org, then the value should be copied over.", () =>
			{
				AssertEquals("There are 2 customization Rows whose Parent is retained Org.", 2, (int)TestConnection.ExecuteScalar(string.Format("select count(*) from dbo.OrgCustomLabels where OT_OH = '{0}'", retainedOrg.PK.ToString())));
				AssertEquals("There are 0 customization Rows whose Parent is dissolved Org.", 0, (int)TestConnection.ExecuteScalar(string.Format("select count(*) from dbo.OrgCustomLabels where OT_OH = '{0}'", dissolvedOrg.PK.ToString())));
				AssertEquals("retained caption", caption1);
				AssertEquals("dissolved caption", caption2);
			});
		}

		public void TestOrgHeaderFlagsAreMerged()
		{
			var factory = new BusinessObjectFactory();

			var orgA = factory.NewWithValidTestData<OrgHeader>();
			orgA.OH_IsNationalAccount = true;
			orgA.OH_IsGlobalAccount = true;
			orgA.OH_IsConsignee = true;
			orgA.OH_IsConsignor = true;
			orgA.OH_IsTransportClient = true;
			orgA.OH_IsWarehouseClient = true;
			orgA.OH_IsShippingProvider = true;
			orgA.OH_IsForwarder = true;
			orgA.OH_IsBroker = true;
			orgA.OH_IsMiscFreightServices = true;
			orgA.OH_IsCompetitor = true;
			orgA.OH_IsSalesLead = true;
			orgA.OH_Code = "AAA";

			var orgB = factory.NewWithValidTestData<OrgHeader>();
			orgB.OH_Code = "BBB";

			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, orgA, orgB);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;
			testMerger.Save();

			var newOrg = new BusinessObjectFactory().Load<OrgHeader>(orgB.PK);
			AssertEquals(true, newOrg.OH_IsNationalAccount);
			AssertEquals(true, newOrg.OH_IsGlobalAccount);
			AssertEquals(true, newOrg.OH_IsConsignee);
			AssertEquals(true, newOrg.OH_IsConsignor);
			AssertEquals(true, newOrg.OH_IsTransportClient);
			AssertEquals(true, newOrg.OH_IsWarehouseClient);
			AssertEquals(true, newOrg.OH_IsShippingProvider);
			AssertEquals(true, newOrg.OH_IsForwarder);
			AssertEquals(true, newOrg.OH_IsBroker);
			AssertEquals(true, newOrg.OH_IsMiscFreightServices);
			AssertEquals(true, newOrg.OH_IsCompetitor);
			AssertEquals(true, newOrg.OH_IsSalesLead);
		}

		public void TestShouldStopMergeWhenExceptionOccurFromMoveFkReferences()
		{
			var factory = new BusinessObjectFactory();
			var orgA = factory.NewWithValidTestData<OrgHeader>();
			var orgB = factory.NewWithValidTestData<OrgHeader>();
			var whsAddress = factory.NewWithValidTestData<OrgAddress>();
			factory.Save();

			var warehouse = new WhsWarehouse("W1", "PRW", GlbBranch.CurrentBranch.PK.ToGuid(), whsAddress.PK.ToGuid(), new Guid()).WithDockDoor(TestConnection);

			var jobStorage1 = new JobStorage("1", warehouse, OrgHeaderDO.ShallowLoadFromDB(TestConnection, orgA.PK.ToGuid()), ZDateTime.BrettsBirthday.ToDateTime(), ZDateTime.BrettsBirthday.AddDays(1).ToDateTime()) { ET_BillingDate = ZDateTime.BrettsBirthday.ToDateTime() }.InsertAndReturnObject(TestConnection);
			var jobStorage2 = new JobStorage("2", warehouse, OrgHeaderDO.ShallowLoadFromDB(TestConnection, orgB.PK.ToGuid()), ZDateTime.BrettsBirthday.ToDateTime(), ZDateTime.BrettsBirthday.AddDays(1).ToDateTime()) { ET_BillingDate = ZDateTime.BrettsBirthday.ToDateTime() }.InsertAndReturnObject(TestConnection);

			var mergeOrgHeader = new MergeOrgHeader(factory, orgA, orgB);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;

			AssertExceptionThrown(typeof(SqlException), () => testMerger.Save());
			AssertNotNull(mergeOrgHeader.AddOverlappingDatesException);
		}

		public void TestUseRetryHandlerForDeleteOldOrganization()
		{
			var factory = new BusinessObjectFactory();
			var orgA = factory.NewWithValidTestData<OrgHeader>();
			var orgB = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, orgA, orgB);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.ThrowExceptionOnOrgDelete = true;
			testMerger.ThrowExceptionMaxTimes = 1;
			testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Merge is not interrupted by any exception.", () => testMerger.Save());
				AssertEquals("Should run 2 times when error occur at the first time", 2, testMerger.DeleteOldOrganizationRunTimes);
			});
		}

		public void TestShouldNotThrowException_WhenReMergeAfterExceptionOccurs()
		{
			var factory = new BusinessObjectFactory();
			var orgA = factory.NewWithValidTestData<OrgHeader>();
			var orgB = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var mergeOrgHeader = new MergeOrgHeader(factory, orgA, orgB);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.ThrowExceptionOnOrgDelete = true;
			testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var newOrgA = factory.Load<OrgHeader>(orgA.PK);
			newOrgA.OH_SystemLastEditTimeUtc = DateTime.Now;
			newFactory.Save();

			AssertExceptionThrown("Merge is interrupted by an exception.", typeof(SqlException), () => testMerger.Save());
			testMerger.ThrowExceptionOnOrgDelete = false;
			AssertNoExceptionThrown(() => testMerger.Save());
		}

		#region Org Address Additional Info
		public void Test_OrgMerge_OrgAddressAdditionalInfo()
		{
			// create two organisations, retained and dissolved
			var factory = new BusinessObjectFactory();
			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			dissolvedOrg.OH_Code = "~DissOrg~";
			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();
			retainedOrg.OH_Code = "~RetOrg~";

			var dissolvedOrgAddress = dissolvedOrg.MainAddress;
			dissolvedOrgAddress.OA_Address1 = "~DisAddress~";
			dissolvedOrgAddress.OA_Code = "ABC";

			var retainedOrgAddress = retainedOrg.MainAddress;
			retainedOrgAddress.OA_Address1 = "~RetAddress~";
			retainedOrgAddress.OA_Code = "XYZ";

			// create two additional infos, and assign to orgs
			var infoOfRetained = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfRetained.OAI_OA_Address = retainedOrgAddress.PK;
			infoOfRetained.OAI_IsPrimary = true;

			var infoOfDissolved = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfDissolved.OAI_OA_Address = dissolvedOrgAddress.PK;
			infoOfDissolved.OAI_IsPrimary = true;

			factory.Save();

			// assert there is one additional info corresponding to each org address
			var infoOfRetainedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, retainedOrgAddress.PK));
			var infoOfDissolvedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, dissolvedOrgAddress.PK));

			AssertEquals("Precondition: There is 1 OrgAddressAdditionalInfo Row whose Parent is retained OrgAddress.", 1, infoOfRetainedCount);
			AssertEquals("Precondition: There is 1 OrgAddressAdditionalInfo Row whose Parent is dissolved OrgAddress.", 1, infoOfDissolvedCount);

			// perform merge
			PerformMergeForTest(factory, dissolvedOrg, retainedOrg);

			// assert there is one additional info corresponding to each org address
			infoOfRetainedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, retainedOrgAddress.PK));
			infoOfDissolvedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, dissolvedOrgAddress.PK));

			AssertEquals("Postcondition: There is 1 OrgAddressAdditionalInfo Row whose Parent is retained OrgAddress.", 1, infoOfRetainedCount);
			AssertEquals("Postcondition: There is 1 OrgAddressAdditionalInfo Row whose Parent is dissolved OrgAddress.", 1, infoOfDissolvedCount);

			infoOfRetained.Reload();
			infoOfDissolved.Reload();

			Assert("Postcondition: Retained OrgAddressAdditionalInfo is primary additional address.", infoOfRetained.OAI_IsPrimary);
			Assert("Postcondition: Dissolved OrgAddressAdditionalInfo is primary additional address.", infoOfDissolved.OAI_IsPrimary);
		}

		public void Test_OrgMerge_OrgAddressAdditionalInfo_DuplicateAddress()
		{
			// create two organisations, retained and dissolved
			var factory = new BusinessObjectFactory();
			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			dissolvedOrg.OH_Code = "~DissOrg~";
			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();
			retainedOrg.OH_Code = "~RetOrg~";

			// make both orgs have same address, so dissolved org address will be deleted on merge
			var dissolvedOrgAddress = dissolvedOrg.MainAddress;
			dissolvedOrgAddress.OA_Address1 = "~OneAddress~";
			dissolvedOrgAddress.OA_Code = "ABC";

			var retainedOrgAddress = retainedOrg.MainAddress;
			retainedOrgAddress.OA_Address1 = "~OneAddress~";
			retainedOrgAddress.OA_Code = "ABC";

			// create two different additional infos, and assign to orgs
			var infoOfRetained = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfRetained.OAI_OA_Address = retainedOrgAddress.PK;
			infoOfRetained.OAI_IsPrimary = true;
			infoOfRetained.OAI_AdditionalInfo = "~Info 1~";

			var infoOfDissolved = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfDissolved.OAI_OA_Address = dissolvedOrgAddress.PK;
			infoOfDissolved.OAI_IsPrimary = true;
			infoOfDissolved.OAI_AdditionalInfo = "~Info 2~";

			factory.Save();

			// assert there is one additional info corresponding to each org address
			var infoOfRetainedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, retainedOrgAddress.PK));
			var infoOfDissolvedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, dissolvedOrgAddress.PK));

			AssertEquals("Precondition: There is 1 OrgAddressAdditionalInfo Row whose Parent is retained OrgAddress.", 1, infoOfRetainedCount);
			AssertEquals("Precondition: There is 1 OrgAddressAdditionalInfo Row whose Parent is dissolved OrgAddress.", 1, infoOfDissolvedCount);

			// perform merge
			PerformMergeForTest(factory, dissolvedOrg, retainedOrg);

			// assert there is 2 and 0 additional info corresponding to each org address respectively
			infoOfRetainedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, retainedOrgAddress.PK));
			infoOfDissolvedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, dissolvedOrgAddress.PK));

			AssertEquals("Postcondition: There are 2 OrgAddressAdditionalInfo Rows whose Parent is retained OrgAddress.", 2, infoOfRetainedCount);
			AssertEquals("Postcondition: There are no OrgAddressAdditionalInfo Row whose Parent is dissolved OrgAddress.", 0, infoOfDissolvedCount);

			infoOfRetained.Reload();
			infoOfDissolved.Reload();

			Assert("Postcondition: Retained OrgAddressAdditionalInfo is primary additional address.", infoOfRetained.OAI_IsPrimary);
			Assert("Postcondition: Dissolved OrgAddressAdditionalInfo is not primary additional address.", !infoOfDissolved.OAI_IsPrimary);
		}

		public void Test_OrgMerge_OrgAddressAdditionalInfo_DuplicateAddress_DuplicateInfo()
		{
			// create two organisations, retained and dissolved
			var factory = new BusinessObjectFactory();
			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			dissolvedOrg.OH_Code = "~DissOrg~";
			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();
			retainedOrg.OH_Code = "~RetOrg~";

			// make both orgs have same address, so dissolved org address will be deleted on merge
			var dissolvedOrgAddress = dissolvedOrg.MainAddress;
			dissolvedOrgAddress.OA_Address1 = "~OneAddress~";
			dissolvedOrgAddress.OA_Code = "ABC";

			var retainedOrgAddress = retainedOrg.MainAddress;
			retainedOrgAddress.OA_Address1 = "~OneAddress~";
			retainedOrgAddress.OA_Code = "ABC";

			// create two identitical additional infos, so dissolved info will be deleted on merge
			var infoOfRetained = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfRetained.OAI_OA_Address = retainedOrgAddress.PK;
			infoOfRetained.OAI_IsPrimary = true;
			infoOfRetained.OAI_AdditionalInfo = "~Info~";

			var infoOfDissolved = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfDissolved.OAI_OA_Address = dissolvedOrgAddress.PK;
			infoOfDissolved.OAI_IsPrimary = true;
			infoOfDissolved.OAI_AdditionalInfo = "~Info~";

			factory.Save();

			// assert there is one additional info corresponding to each org address
			var infoOfRetainedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, retainedOrgAddress.PK));
			var infoOfDissolvedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, dissolvedOrgAddress.PK));

			AssertEquals("Precondition: There is 1 OrgAddressAdditionalInfo Row whose Parent is retained OrgAddress.", 1, infoOfRetainedCount);
			AssertEquals("Precondition: There is 1 OrgAddressAdditionalInfo Row whose Parent is dissolved OrgAddress.", 1, infoOfDissolvedCount);

			var sql = $"SELECT COUNT(*) FROM dbo.OrgAddressAdditionalInfo WHERE OAI_OA_Address = '{dissolvedOrgAddress.PK}' AND NOT EXISTS (SELECT * FROM dbo.OrgAddressAdditionalInfo B WHERE B.OAI_AdditionalInfo = OAI_AdditionalInfo AND B.OAI_PK != OAI_PK) ";
			TestConnection.ExecuteScalar(sql);

			// perform merge
			PerformMergeForTest(factory, dissolvedOrg, retainedOrg);

			// assert there is 1 and 0 additional info corresponding to each org address respectively
			infoOfRetainedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, retainedOrgAddress.PK));
			infoOfDissolvedCount = factory.GetDatabaseCount(typeof(OrgAddressAdditionalInfo), new ZQuery(OrgAddressAdditionalInfoSchema.OAI_OA_Address, dissolvedOrgAddress.PK));

			AssertEquals("Postcondition: There are 1 OrgAddressAdditionalInfo Rows whose Parent is retained OrgAddress.", 1, infoOfRetainedCount);
			AssertEquals("Postcondition: There are no OrgAddressAdditionalInfo Row whose Parent is dissolved OrgAddress.", 0, infoOfDissolvedCount);

			infoOfRetained.Reload();
			Assert("Postcondition: Retained OrgAddressAdditionalInfo is primary additional address.", infoOfRetained.OAI_IsPrimary);
		}
		#endregion

		#region Org Address Translated Additional Info

		public void Test_OrgMerge_OrgTranslatedAddressAdditionalInfo()
		{
			// create two organisations, retained and dissolved
			var factory = new BusinessObjectFactory();
			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			dissolvedOrg.OH_Code = "~DissOrg~";
			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();
			retainedOrg.OH_Code = "~RetOrg~";

			var dissolvedOrgAddress = dissolvedOrg.MainAddress;
			dissolvedOrgAddress.OA_Address1 = "~DissAddress~";
			dissolvedOrgAddress.OA_Code = "ABC";

			var retainedOrgAddress = retainedOrg.MainAddress;
			retainedOrgAddress.OA_Address1 = "~RetAddress~";
			retainedOrgAddress.OA_Code = "XYZ";

			// create two different additional infos, and assign to orgs
			var infoOfRetained = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfRetained.OAI_OA_Address = retainedOrgAddress.PK;
			infoOfRetained.OAI_IsPrimary = true;
			infoOfRetained.OAI_AdditionalInfo = "~Info 1~";

			var infoOfDissolved = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfDissolved.OAI_OA_Address = dissolvedOrgAddress.PK;
			infoOfDissolved.OAI_IsPrimary = true;
			infoOfDissolved.OAI_AdditionalInfo = "~Info 2~";

			// create translated infos from above
			var translatedInfoOfRetained = factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedInfoOfRetained.OTI_OAI = infoOfRetained.PK;

			var translatedInfoOfDissolved1 = factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedInfoOfDissolved1.OTI_OAI = infoOfDissolved.PK;
			translatedInfoOfDissolved1.OTI_Language = Enterprise.Core.Constants.Languages.ChineseSimplified;

			var translatedInfoOfDissolved2 = factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedInfoOfDissolved2.OTI_OAI = infoOfDissolved.PK;
			translatedInfoOfDissolved1.OTI_Language = Enterprise.Core.Constants.Languages.French;

			factory.Save();

			// assert translated info counts
			var retainedTranslInfoCount = factory.GetDatabaseCount(typeof(OrgTranslatedAddressAdditionalInfo), new ZQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, infoOfRetained.PK));
			var dissolvedTranslInfoCount = factory.GetDatabaseCount(typeof(OrgTranslatedAddressAdditionalInfo), new ZQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, infoOfDissolved.PK));

			AssertEquals("Precondition: There is 1 OrgTranslatedAddressAdditionalInfo Row whose Parent is retained OrgAddressAdditionalInfo.", 1, retainedTranslInfoCount);
			AssertEquals("Precondition: There are 2 OrgTranslatedAddressAdditionalInfo Row whose Parent is dissolved OrgAddressAdditionalInfo.", 2, dissolvedTranslInfoCount);

			PerformMergeForTest(factory, dissolvedOrg, retainedOrg);

			// assert there is one translated info corresponding to each additional info
			retainedTranslInfoCount = factory.GetDatabaseCount(typeof(OrgTranslatedAddressAdditionalInfo), new ZQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, infoOfRetained.PK));
			dissolvedTranslInfoCount = factory.GetDatabaseCount(typeof(OrgTranslatedAddressAdditionalInfo), new ZQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, infoOfDissolved.PK));

			AssertEquals("Postcondition: There is 1 OrgTranslatedAddressAdditionalInfo Row whose Parent is retained OrgAddressAdditionalInfo.", 1, retainedTranslInfoCount);
			AssertEquals("Postcondition: There are 2 OrgTranslatedAddressAdditionalInfo Row whose Parent is dissolved OrgAddressAdditionalInfo.", 2, dissolvedTranslInfoCount);
		}

		public void Test_OrgMerge_OrgTranslatedAddressAdditionalInfo_DuplicateAddress()
		{
			// create two organisations, retained and dissolved
			var factory = new BusinessObjectFactory();
			var dissolvedOrg = factory.NewWithValidTestData<OrgHeader>();
			dissolvedOrg.OH_Code = "~DissOrg~";
			var retainedOrg = factory.NewWithValidTestData<OrgHeader>();
			retainedOrg.OH_Code = "~RetOrg~";

			// make both orgs have same address, so dissolved org address will be deleted on merge
			var dissolvedOrgAddress = dissolvedOrg.MainAddress;
			dissolvedOrgAddress.OA_Address1 = "~OneAddress~";
			dissolvedOrgAddress.OA_Code = "ABC";

			var retainedOrgAddress = retainedOrg.MainAddress;
			retainedOrgAddress.OA_Address1 = "~OneAddress~";
			retainedOrgAddress.OA_Code = "ABC";

			// create two different additional infos, and assign to orgs
			var infoOfRetained = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfRetained.OAI_OA_Address = retainedOrgAddress.PK;
			infoOfRetained.OAI_IsPrimary = true;
			infoOfRetained.OAI_AdditionalInfo = "~Info 1~";

			var infoOfDissolved = factory.NewWithValidTestData<OrgAddressAdditionalInfo>();
			infoOfDissolved.OAI_OA_Address = dissolvedOrgAddress.PK;
			infoOfDissolved.OAI_IsPrimary = true;
			infoOfDissolved.OAI_AdditionalInfo = "~Info 2~";

			// create translated infos from above
			var translatedInfoOfRetained = factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedInfoOfRetained.OTI_OAI = infoOfRetained.PK;

			var translatedInfoOfDissolved1 = factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedInfoOfDissolved1.OTI_OAI = infoOfDissolved.PK;
			translatedInfoOfDissolved1.OTI_Language = Enterprise.Core.Constants.Languages.ChineseSimplified;

			var translatedInfoOfDissolved2 = factory.NewWithValidTestData<OrgTranslatedAddressAdditionalInfo>();
			translatedInfoOfDissolved2.OTI_OAI = infoOfDissolved.PK;
			translatedInfoOfDissolved1.OTI_Language = Enterprise.Core.Constants.Languages.French;

			factory.Save();

			// assert translated info counts
			var retainedTranslInfoCount = factory.GetDatabaseCount(typeof(OrgTranslatedAddressAdditionalInfo), new ZQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, infoOfRetained.PK));
			var dissolvedTranslInfoCount = factory.GetDatabaseCount(typeof(OrgTranslatedAddressAdditionalInfo), new ZQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, infoOfDissolved.PK));

			AssertEquals("Precondition: There is 1 OrgTranslatedAddressAdditionalInfo Row whose Parent is retained OrgAddressAdditionalInfo.", 1, retainedTranslInfoCount);
			AssertEquals("Precondition: There are 2 OrgTranslatedAddressAdditionalInfo Row whose Parent is dissolved OrgAddressAdditionalInfo.", 2, dissolvedTranslInfoCount);

			// perform merge
			PerformMergeForTest(factory, dissolvedOrg, retainedOrg);

			// assert there is one translated info for each additional info
			retainedTranslInfoCount = factory.GetDatabaseCount(typeof(OrgTranslatedAddressAdditionalInfo), new ZQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, infoOfRetained.PK));
			dissolvedTranslInfoCount = factory.GetDatabaseCount(typeof(OrgTranslatedAddressAdditionalInfo), new ZQuery(OrgTranslatedAddressAdditionalInfoSchema.OTI_OAI, infoOfDissolved.PK));

			AssertEquals("Postcondition: There is 1 OrgTranslatedAddressAdditionalInfo Row whose Parent is retained OrgAddressAdditionalInfo.", 1, retainedTranslInfoCount);
			AssertEquals("Postcondition: There is 0 OrgTranslatedAddressAdditionalInfo Rows whose Parent is dissolved OrgAddressAdditionalInfo.", 0, dissolvedTranslInfoCount);
		}
		#endregion

		void PerformMergeForTest(BusinessObjectFactory factory, OrgHeader dissolvedOrg, OrgHeader retainedOrg)
		{
			var mergeOrgHeader = new MergeOrgHeader(factory, dissolvedOrg, retainedOrg);
			var testMerger = new OrganisationMergerForTest(mergeOrgHeader);
			testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;
			testMerger.Save();
		}

		DataTable LoadDataTable(string tableName)
		{
			var table = new DataTable(tableName);
			Db.Connection.Command($"SELECT * FROM {table.TableName}").NewDataAdapter().Fill(table);

			return table;
		}

		public void TestAdditionalMergeActionsCalled()
		{
			var factory = new BusinessObjectFactory();
			var orgB = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var mergeAction = new MergeActionForTest();
			try
			{
				((ArrayList)ObjectFactory.Get("OrganisationMergeActions")).Add(mergeAction);

				var orgA = factory.NewWithValidTestData<OrgHeader>();
				factory.Save();
				var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, orgA, orgB)) { ThrowExceptionOnOrgDelete = false };
				testMerger.ActionOnSave = OrganisationMergerActionOnSave.DeleteOnly;
				testMerger.Save();

				AssertEquals("No merge - no merge action should be executed", 0, mergeAction.Calls.Count);

				orgA = factory.NewWithValidTestData<OrgHeader>();
				factory.Save();
				testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, orgA, orgB)) { ThrowExceptionOnOrgDelete = false };
				testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeOnly;
				testMerger.Save();

				AssertEquals(1, mergeAction.Calls.Count);
				AssertSame(orgA, mergeAction.Calls[0].oldOrg);
				AssertSame(orgB, mergeAction.Calls[0].newOrg);
				AssertEquals(OrganisationMergerActionOnSave.MergeOnly, mergeAction.Calls[0].action);

				orgA = factory.NewWithValidTestData<OrgHeader>();
				factory.Save();
				testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, orgA, orgB)) { ThrowExceptionOnOrgDelete = false };
				testMerger.ActionOnSave = OrganisationMergerActionOnSave.MergeAndDelete;
				testMerger.Save();

				AssertEquals(2, mergeAction.Calls.Count);
				AssertSame(orgA, mergeAction.Calls[1].oldOrg);
				AssertSame(orgB, mergeAction.Calls[1].newOrg);
				AssertEquals(OrganisationMergerActionOnSave.MergeAndDelete, mergeAction.Calls[1].action);
			}
			finally
			{
				((ArrayList)ObjectFactory.Get("OrganisationMergeActions")).Remove(mergeAction);
			}
		}

		public void TestAddMergeLogs()
		{
			var factory = new BusinessObjectFactory();
			var orgA = factory.NewWithValidTestData<OrgHeader>();
			var orgB = factory.NewWithValidTestData<OrgHeader>();
			factory.Save();

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, orgA, orgB)) { ThrowExceptionOnOrgDelete = false, ActionOnSave = OrganisationMergerActionOnSave.MergeOnly };
			testMerger.Save();

			var newFactory = new BusinessObjectFactory();
			var orgA2 = newFactory.Load<OrgHeader>(orgA.PK);
			var orgB2 = newFactory.Load<OrgHeader>(orgB.PK);

			AssertNotNull("New org should have update log with reference about merging",
				orgB2.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.EditedARecordCode && log.SL_Reference == $"Organisation {orgA.OH_Code} was merged into this organisation.").Single());

			AssertNotNull("Old org should have Moved log with reference about merging and PK of new org",
				orgA2.Logs.Find(log => log.SL_SE_NKEvent == OrganisationMerger.MergedLogEvent.Code && log.SL_Reference == $"This organisation was merged into organisation {orgB.OH_Code}.|{orgB.PK}").Single());
		}

		#region RatingContractNamedAccountPivots

		public void TestMergeRatingContractNamedAccountPivots()
		{
			var factory = new BusinessObjectFactory();
			var orgA = factory.NewWithValidTestData<OrgHeader>();
			var orgB = factory.NewWithValidTestData<OrgHeader>();
			var contract1PK = ZGuid.NewZGuid();
			var contract2PK = ZGuid.NewZGuid();

			factory.Save();

			string sqlText = string.Format($@"
				insert into dbo.RatingContractNamedAccountPivot (RNP_PK, RNP_OH_NamedAccount, RNP_ParentID, RNP_ParentTableCode, RNP_SystemCreateTimeUtc, RNP_SystemCreateUser, RNP_SystemLastEditTimeUtc, RNP_SystemLastEditUser) values(newid(), '{orgA.PK}', '{contract1PK}', 'RCT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.RatingContractNamedAccountPivot (RNP_PK, RNP_OH_NamedAccount, RNP_ParentID, RNP_ParentTableCode, RNP_SystemCreateTimeUtc, RNP_SystemCreateUser, RNP_SystemLastEditTimeUtc, RNP_SystemLastEditUser) values(newid(), '{orgA.PK}', '{contract2PK}', 'RCT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.RatingContractNamedAccountPivot (RNP_PK, RNP_OH_NamedAccount, RNP_ParentID, RNP_ParentTableCode, RNP_SystemCreateTimeUtc, RNP_SystemCreateUser, RNP_SystemLastEditTimeUtc, RNP_SystemLastEditUser) values(newid(), '{orgA.PK}', '{contract1PK}', 'RCA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.RatingContractNamedAccountPivot (RNP_PK, RNP_OH_NamedAccount, RNP_ParentID, RNP_ParentTableCode, RNP_SystemCreateTimeUtc, RNP_SystemCreateUser, RNP_SystemLastEditTimeUtc, RNP_SystemLastEditUser) values(newid(), '{orgB.PK}', '{contract1PK}', 'RCT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.RatingContractNamedAccountPivot (RNP_PK, RNP_OH_NamedAccount, RNP_ParentID, RNP_ParentTableCode, RNP_SystemCreateTimeUtc, RNP_SystemCreateUser, RNP_SystemLastEditTimeUtc, RNP_SystemLastEditUser) values(newid(), '{orgB.PK}', '{contract2PK}', 'RCA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");
			TestConnection.ExecuteScalar(sqlText);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, orgA, orgB));
			testMerger.Save();

			int CountPivots(ZGuid orgPK, ZGuid contractPK, string parentTableCode)
			{
				return (int)TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.RatingContractNamedAccountPivot WHERE RNP_OH_NamedAccount = '{orgPK}' AND RNP_ParentID = '{contractPK}' and RNP_ParentTableCode = '{parentTableCode}'");
			}

			CombineAssertions("correct merge", () =>
			{
				AssertEquals("orgA has correct #pivots", 0, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.RatingContractNamedAccountPivot WHERE RNP_OH_NamedAccount = '{orgA.PK}'"));
				AssertEquals("orgB has correct #pivots", 4, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.RatingContractNamedAccountPivot WHERE RNP_OH_NamedAccount = '{orgB.PK}'"));
				AssertEquals(1, CountPivots(orgB.PK, contract1PK, "RCT"));
				AssertEquals(1, CountPivots(orgB.PK, contract2PK, "RCT"));
				AssertEquals(1, CountPivots(orgB.PK, contract1PK, "RCA"));
				AssertEquals(1, CountPivots(orgB.PK, contract2PK, "RCA"));
			});
		}

		#endregion

		#region AllocationRouteAgentPivots

		public void TestMergeAllocationRouteAgentPivots()
		{
			var factory = new BusinessObjectFactory();
			var contractOrg = factory.NewWithValidTestData<OrgHeader>();
			var orgA = factory.NewWithValidTestData<OrgHeader>();
			var orgB = factory.NewWithValidTestData<OrgHeader>();
			var allocationRoute1 = ZGuid.NewZGuid();
			var allocationRoute2 = ZGuid.NewZGuid();
			var ratingContract = ZGuid.NewZGuid();

			factory.Save();

			var sqlText = string.Format($@"
				insert into dbo.RatingContract (RCT_PK, RCT_ContractNumber, RCT_GS_NKContractOwner, RCT_StartDate, RCT_OH, RCT_ContractType, RCT_SystemCreateTimeUtc, RCT_SystemCreateUser, RCT_SystemLastEditTimeUtc, RCT_SystemLastEditUser) values('{ratingContract}','AAA', 'A', GetUtcDate(), '{contractOrg.PK}', 'PRO', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.RatingContractAllocationLine (RCA_PK, RCA_RCT_RatingContract, RCA_AllocationLineID, RCA_StartDate, RCA_LoadLocation, RCA_DischargeLocation, RCA_AllocatedQuantity, RCA_AllocatedUQ, RCA_SystemCreateTimeUtc, RCA_SystemCreateUser, RCA_SystemLastEditTimeUtc, RCA_SystemLastEditUser) values('{allocationRoute1}', '{ratingContract}', 'AAAA', GetUtcDate(), 'AUSYD', 'NZAKL', 10, 'TU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.RatingContractAllocationLine (RCA_PK, RCA_RCT_RatingContract, RCA_AllocationLineID, RCA_StartDate, RCA_LoadLocation, RCA_DischargeLocation, RCA_AllocatedQuantity, RCA_AllocatedUQ, RCA_SystemCreateTimeUtc, RCA_SystemCreateUser, RCA_SystemLastEditTimeUtc, RCA_SystemLastEditUser) values('{allocationRoute2}', '{ratingContract}', 'BBBB', GetUtcDate(), 'AUSYD', 'NZAKL', 10, 'TU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.AllocationRouteAgentPivot (ARA_PK, ARA_OH_Agent, ARA_RCA_AllocationLine, ARA_SystemCreateTimeUtc, ARA_SystemCreateUser, ARA_SystemLastEditTimeUtc, ARA_SystemLastEditUser) values(newid(), '{orgA.PK}', '{allocationRoute1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.AllocationRouteAgentPivot (ARA_PK, ARA_OH_Agent, ARA_RCA_AllocationLine, ARA_SystemCreateTimeUtc, ARA_SystemCreateUser, ARA_SystemLastEditTimeUtc, ARA_SystemLastEditUser) values(newid(), '{orgA.PK}', '{allocationRoute2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				insert into dbo.AllocationRouteAgentPivot (ARA_PK, ARA_OH_Agent, ARA_RCA_AllocationLine, ARA_SystemCreateTimeUtc, ARA_SystemCreateUser, ARA_SystemLastEditTimeUtc, ARA_SystemLastEditUser) values(newid(), '{orgB.PK}', '{allocationRoute1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");
			TestConnection.ExecuteScalar(sqlText);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, orgA, orgB));
			testMerger.Save();

			int CountPivots(ZGuid orgPK, ZGuid allocationRoutePK)
			{
				return (int)TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.AllocationRouteAgentPivot WHERE ARA_OH_Agent = '{orgPK}' AND ARA_RCA_AllocationLine = '{allocationRoutePK}'");
			}

			CombineAssertions("correct merge", () =>
			{
				AssertEquals("orgA has correct #pivots", 0, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.AllocationRouteAgentPivot WHERE ARA_OH_Agent = '{orgA.PK}'"));
				AssertEquals("orgB has correct #pivots", 2, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.AllocationRouteAgentPivot WHERE ARA_OH_Agent = '{orgB.PK}'"));
				AssertEquals(1, CountPivots(orgB.PK, allocationRoute1));
				AssertEquals(1, CountPivots(orgB.PK, allocationRoute2));
			});
		}

		#endregion

		#region OrgRefFacility

		public void TestMergeOrgRefFacilities()
		{
			var factory = new BusinessObjectFactory();
			var orgA = factory.NewWithValidTestData<OrgHeader>();
			var orgB = factory.NewWithValidTestData<OrgHeader>();

			var facility1 = factory.NewWithValidTestData<RefFacility>();
			var facility2 = factory.NewWithValidTestData<RefFacility>();

			var address1 = factory.NewWithValidTestData<OrgAddress>();
			var address2 = factory.NewWithValidTestData<OrgAddress>();

			var orgRefFacility1 = orgA.OrgRefFacilities.AddNew();
			orgRefFacility1.OFC_RFT_Facility = facility1.PK;
			orgRefFacility1.OFC_OA_PremisesAddress = address1.PK;

			var orgRefFacility2 = orgB.OrgRefFacilities.AddNew();
			orgRefFacility2.OFC_RFT_Facility = facility2.PK;
			orgRefFacility2.OFC_OA_PremisesAddress = address2.PK;

			factory.Save();

			var orgRefFacilityOrgACount = factory.GetDatabaseCount(typeof(OrgRefFacility), new ZQuery(OrgRefFacilitySchema.OFC_OH_Organization, orgA.PK));
			var orgRefFacilityOrgBCount = factory.GetDatabaseCount(typeof(OrgRefFacility), new ZQuery(OrgRefFacilitySchema.OFC_OH_Organization, orgB.PK));

			AssertEquals("Precondition: There is 1 OrgRefFacility Row for OrgA.", 1, orgRefFacilityOrgACount);
			AssertEquals("Precondition: There is 1 OrgRefFacility Row for OrgB.", 1, orgRefFacilityOrgBCount);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, orgA, orgB));
			testMerger.Save();

			orgRefFacilityOrgACount = factory.GetDatabaseCount(typeof(OrgRefFacility), new ZQuery(OrgRefFacilitySchema.OFC_OH_Organization, orgA.PK));
			orgRefFacilityOrgBCount = factory.GetDatabaseCount(typeof(OrgRefFacility), new ZQuery(OrgRefFacilitySchema.OFC_OH_Organization, orgB.PK));

			AssertEquals("Postcondition: There is 0 OrgRefFacility Row for OrgA.", 0, orgRefFacilityOrgACount);
			AssertEquals("Postcondition: There is 2 OrgRefFacility Row for OrgB.", 2, orgRefFacilityOrgBCount);
		}

		public void TestMergeOrgRefFacilitiesWhenExistBasedOnUniqueIndex()
		{
			var factory = new BusinessObjectFactory();
			var orgA = factory.NewWithValidTestData<OrgHeader>();
			var orgB = factory.NewWithValidTestData<OrgHeader>();

			var facility = factory.NewWithValidTestData<RefFacility>();
			var address = factory.NewWithValidTestData<OrgAddress>();

			var orgRefFacility1 = orgA.OrgRefFacilities.AddNew();
			orgRefFacility1.OFC_RFT_Facility = facility.PK;
			orgRefFacility1.OFC_OA_PremisesAddress = address.PK;

			var orgRefFacility2 = orgB.OrgRefFacilities.AddNew();
			orgRefFacility2.OFC_RFT_Facility = facility.PK;
			orgRefFacility2.OFC_OA_PremisesAddress = address.PK;

			factory.Save();

			var orgRefFacilityOrgACount = factory.GetDatabaseCount(typeof(OrgRefFacility), new ZQuery(OrgRefFacilitySchema.OFC_OH_Organization, orgA.PK));
			var orgRefFacilityOrgBCount = factory.GetDatabaseCount(typeof(OrgRefFacility), new ZQuery(OrgRefFacilitySchema.OFC_OH_Organization, orgB.PK));
			AssertEquals("Precondition: There is 1 OrgRefFacility Row for OrgA.", 1, orgRefFacilityOrgACount);
			AssertEquals("Precondition: There is 1 OrgRefFacility Row for OrgB.", 1, orgRefFacilityOrgBCount);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, orgA, orgB));
			testMerger.Save();

			orgRefFacilityOrgACount = factory.GetDatabaseCount(typeof(OrgRefFacility), new ZQuery(OrgRefFacilitySchema.OFC_OH_Organization, orgA.PK));
			orgRefFacilityOrgBCount = factory.GetDatabaseCount(typeof(OrgRefFacility), new ZQuery(OrgRefFacilitySchema.OFC_OH_Organization, orgB.PK));
			AssertEquals("Precondition: There is 0 OrgRefFacility Row for OrgA.", 0, orgRefFacilityOrgACount);
			AssertEquals("Precondition: There is 1 OrgRefFacility Row for OrgB.", 1, orgRefFacilityOrgBCount);
		}

		#endregion

		[TestDate(2022, 12, 1, 2, 3, 0)]
		[TestDateIncremental()]
		public void TestLastEditInfoUpdated()
		{
			var factory = new BusinessObjectFactory();
			var user1 = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>()) as IGlbStaff;
			user1.GS_Code = "XX1";
			var user2 = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>()) as IGlbStaff;
			user2.GS_Code = "XX2";
			var user3 = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>()) as IGlbStaff;
			user3.GS_Code = "XX3";
			factory.Save();

			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			using (Env.SetTemporaryUserContext(user1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				factory.Save();
			}
			AssertEquals("PreCondition", new ZDateTime(2022, 12, 1, 2, 3, 0), orgNew.OH_SystemLastEditTimeUtc);
			AssertEquals("PreCondition", "XX1", orgNew.OH_SystemLastEditUser);
			AssertEquals("PreCondition", new ZDateTime(2022, 12, 1, 2, 3, 0), orgOld.OH_SystemLastEditTimeUtc);
			AssertEquals("PreCondition", "XX1", orgOld.OH_SystemLastEditUser);

			var merger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));

			MergeOrganisationUnderSpecifiedDateTimeAndUser(user2, new ZDateTime(2022, 12, 2, 2, 3, 0), OrganisationMergerActionOnSave.MergeOnly);
			AssertEquals(new ZDateTime(2022, 12, 2, 2, 3, 0), orgNew.OH_SystemLastEditTimeUtc);
			AssertEquals("XX2", orgNew.OH_SystemLastEditUser);
			AssertEquals(new ZDateTime(2022, 12, 2, 2, 3, 0), orgOld.OH_SystemLastEditTimeUtc);
			AssertEquals("XX2", orgOld.OH_SystemLastEditUser);

			MergeOrganisationUnderSpecifiedDateTimeAndUser(user3, new ZDateTime(2022, 12, 3, 2, 3, 0), OrganisationMergerActionOnSave.MergeAndDelete);
			AssertEquals(new ZDateTime(2022, 12, 3, 2, 3, 0), orgNew.OH_SystemLastEditTimeUtc);
			AssertEquals("XX3", orgNew.OH_SystemLastEditUser);
			AssertNull(orgOld);

			void MergeOrganisationUnderSpecifiedDateTimeAndUser(IGlbStaff user, ZDateTime dateTime, OrganisationMergerActionOnSave actionOnSave)
			{
				TestDateIncrementalAttribute.Span = TimeSpan.FromDays(1);
				AssertEquals("PreCondition", dateTime, ZDateTime.UtcNow);
				TestDateIncrementalAttribute.Span = TimeSpan.Zero;

				merger.ActionOnSave = actionOnSave;
				using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					merger.Save();
				}

				orgNew = new BusinessObjectFactory().Load<OrgHeader>(orgNew.PK);
				orgOld = new BusinessObjectFactory().Load<OrgHeader>(orgOld.PK);
			}
		}

		[TestDate(2022, 12, 1, 2, 3, 0)]
		[TestDateIncremental()]
		public void TestLastEditInfoDoNotUpdated()
		{
			var factory = new BusinessObjectFactory();
			var user1 = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>()) as IGlbStaff;
			user1.GS_Code = "XX1";
			var user2 = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>()) as IGlbStaff;
			user2.GS_Code = "XX2";
			factory.Save();

			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			using (Env.SetTemporaryUserContext(user1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				factory.Save();
			}
			AssertEquals("PreCondition", new ZDateTime(2022, 12, 1, 2, 3, 0), orgNew.OH_SystemLastEditTimeUtc);
			AssertEquals("PreCondition", "XX1", orgNew.OH_SystemLastEditUser);
			AssertEquals("PreCondition", new ZDateTime(2022, 12, 1, 2, 3, 0), orgOld.OH_SystemLastEditTimeUtc);
			AssertEquals("PreCondition", "XX1", orgOld.OH_SystemLastEditUser);

			var merger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(new BusinessObjectFactory()), new MergeOrgContactCollection(new BusinessObjectFactory()));

			TestDateIncrementalAttribute.Span = TimeSpan.FromDays(1);
			AssertEquals("PreCondition", new ZDateTime(2022, 12, 2, 2, 3, 0), ZDateTime.UtcNow);
			TestDateIncrementalAttribute.Span = TimeSpan.Zero;

			merger.ActionOnSave = OrganisationMergerActionOnSave.DeleteOnly;
			using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				merger.Save();
			}
			orgNew = new BusinessObjectFactory().Load<OrgHeader>(orgNew.PK);
			orgOld = new BusinessObjectFactory().Load<OrgHeader>(orgOld.PK);

			AssertEquals(new ZDateTime(2022, 12, 1, 2, 3, 0), orgNew.OH_SystemLastEditTimeUtc);
			AssertEquals("XX1", orgNew.OH_SystemLastEditUser);
			AssertNull(orgOld);
		}

		public void TestMoveGlbGroupOrgLink()
		{
			var factory = new BusinessObjectFactory();
			factory.Save();

			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			var contact = orgOld.Contacts.AddNew();
			contact.OC_ContactName = "name";
			contact.OC_Email = "email@email.com";

			factory.Save();

			var group = factory.New<GlbGroup>();
			group.GG_Code = "Buyer";
			group.GG_Type = GlbGroupTypeList.Codes.Organisation;

			var orgLink = factory.New<GlbGroupOrgLink>();
			orgLink.GOK_GG_Group = group.PK;
			orgLink.GOK_OH_Org = orgOld.PK;

			var orgContactLink = factory.New<GlbGroupOrgContactLink>();
			orgContactLink.GCK_GG_Group = group.PK;
			orgContactLink.GCK_OC_Contact = contact.PK;

			factory.Save();

			AssertEquals("Precondition: Should have no contacts in new org", 0, orgNew.Contacts.Count);

			var merger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory, orgOld, orgNew));
			merger.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedOrgNew = newFactory.Load<OrgHeader>(orgNew.PK);
			var reloadedOrgLink = newFactory.Load<GlbGroupOrgLink>(orgLink.PK);
			var reloadedOrgContactLink = newFactory.Load<GlbGroupOrgContactLink>(orgContactLink.PK);

			AssertEquals("Precondition: Should have added contact in new org", 1, reloadedOrgNew.Contacts.Count);

			AssertEquals("Org link should be migrated to new org", orgNew.PK, reloadedOrgLink.GOK_OH_Org);
			AssertEquals("contact link should still point to the same group", group.PK, reloadedOrgContactLink.GCK_GG_Group);
			AssertEquals("contact link should point to moved contact", reloadedOrgNew.Contacts[0].PK, reloadedOrgContactLink.GCK_OC_Contact);
		}

		public void TestMoveGlbGroupOrgLinkCollision()
		{
			var factory = new BusinessObjectFactory();
			factory.Save();

			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			var contact = orgOld.Contacts.AddNew();
			contact.OC_ContactName = "name";
			contact.OC_Email = "email@email.com";

			factory.Save();

			var group = factory.New<GlbGroup>();
			group.GG_Code = "Seller";
			group.GG_Type = GlbGroupTypeList.Codes.Organisation;

			var oldOrgLink = factory.New<GlbGroupOrgLink>();
			oldOrgLink.GOK_GG_Group = group.PK;
			oldOrgLink.GOK_OH_Org = orgOld.PK;

			var newOrgLink = factory.New<GlbGroupOrgLink>();
			newOrgLink.GOK_GG_Group = group.PK;
			newOrgLink.GOK_OH_Org = orgNew.PK;

			factory.Save();

			AssertEquals("Precondition: Should have no contacts in new org", 0, orgNew.Contacts.Count);

			var merger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory, orgOld, orgNew));
			merger.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedOrgNew = newFactory.Load<OrgHeader>(orgNew.PK);

			AssertEquals("Precondition: Should have added contact in new org", 1, reloadedOrgNew.Contacts.Count);
			AssertEquals("Old org link should be deleted since there's already a link in the new org", null, newFactory.Load<GlbGroupOrgLink>(oldOrgLink.PK));
			AssertNotNull(newFactory.Load<GlbGroupOrgLink>(newOrgLink.PK));
		}

		public void TestMoveGlbGroupOrgLinkCollision_ShouldUpdateGlbOrgContactLink()
		{
			var factory = new BusinessObjectFactory();
			factory.Save();

			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			var contact = orgOld.Contacts.AddNew();
			contact.OC_ContactName = "name";
			contact.OC_Email = "email@email.com";

			factory.Save();

			var group = factory.New<GlbGroup>();
			group.GG_Code = "Seller";
			group.GG_Type = GlbGroupTypeList.Codes.Organisation;

			var oldOrgLink = factory.New<GlbGroupOrgLink>();
			oldOrgLink.GOK_GG_Group = group.PK;
			oldOrgLink.GOK_OH_Org = orgOld.PK;

			var oldOrgContactLink = factory.New<GlbGroupOrgContactLink>();
			oldOrgContactLink.GCK_OC_Contact = contact.PK;
			oldOrgContactLink.GCK_GG_Group = group.PK;

			var newOrgLink = factory.New<GlbGroupOrgLink>();
			newOrgLink.GOK_GG_Group = group.PK;
			newOrgLink.GOK_OH_Org = orgNew.PK;

			factory.Save();

			AssertEquals("Precondition: Should have no contacts in new org", 0, orgNew.Contacts.Count);

			var merger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory, orgOld, orgNew));
			merger.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedOrgNew = newFactory.Load<OrgHeader>(orgNew.PK);
			var reloadedOrgContactLink = newFactory.Load<GlbGroupOrgContactLink>(oldOrgContactLink.PK);

			AssertEquals("Precondition: Should have added contact in new org", 1, reloadedOrgNew.Contacts.Count);

			AssertEquals("Old org link should be deleted", null, newFactory.Load<GlbGroupOrgLink>(oldOrgLink.PK));
			AssertEquals("Contact link should still point to the same group", group.PK, reloadedOrgContactLink.GCK_GG_Group);
			AssertEquals("Contact link should point to moved contact", reloadedOrgNew.Contacts[0].PK, reloadedOrgContactLink.GCK_OC_Contact);
		}

		public void TestMoveGlbGroupOrgLinkCollision_OrgContactLinkCollison_ShouldDeleteDuplicate()
		{
			var factory = new BusinessObjectFactory();
			factory.Save();

			var orgOld = factory.NewWithValidTestData<OrgHeader>();
			var orgNew = factory.NewWithValidTestData<OrgHeader>();
			var oldContact = orgOld.Contacts.AddNew();
			oldContact.OC_ContactName = "match name";
			oldContact.OC_Email = "matchemail@email.com";
			var newContact = orgNew.Contacts.AddNew();
			newContact.OC_ContactName = "match name";
			newContact.OC_Email = "matchemail@email.com";

			factory.Save();

			var sellerGroup = factory.New<GlbGroup>();
			sellerGroup.GG_Code = "Seller";
			sellerGroup.GG_Type = GlbGroupTypeList.Codes.Organisation;

			var buyerGroup = factory.New<GlbGroup>();
			buyerGroup.GG_Code = "Buyer";
			buyerGroup.GG_Type = GlbGroupTypeList.Codes.Organisation;

			var importerGroup = factory.New<GlbGroup>();
			importerGroup.GG_Code = "Importer";
			importerGroup.GG_Type = GlbGroupTypeList.Codes.Organisation;

			var oldOrgSellerLink = factory.New<GlbGroupOrgLink>();
			oldOrgSellerLink.GOK_GG_Group = sellerGroup.PK;
			oldOrgSellerLink.GOK_OH_Org = orgOld.PK;

			var oldOrgContactSellerLink = factory.New<GlbGroupOrgContactLink>();
			oldOrgContactSellerLink.GCK_OC_Contact = oldContact.PK;
			oldOrgContactSellerLink.GCK_GG_Group = sellerGroup.PK;

			var oldOrgBuyerLink = factory.New<GlbGroupOrgLink>();
			oldOrgBuyerLink.GOK_GG_Group = buyerGroup.PK;
			oldOrgBuyerLink.GOK_OH_Org = orgOld.PK;

			var oldOrgContactBuyerLink = factory.New<GlbGroupOrgContactLink>();
			oldOrgContactBuyerLink.GCK_OC_Contact = oldContact.PK;
			oldOrgContactBuyerLink.GCK_GG_Group = buyerGroup.PK;

			var newOrgSellerLink = factory.New<GlbGroupOrgLink>();
			newOrgSellerLink.GOK_GG_Group = sellerGroup.PK;
			newOrgSellerLink.GOK_OH_Org = orgNew.PK;

			var newOrgContactSellerLink = factory.New<GlbGroupOrgContactLink>();
			newOrgContactSellerLink.GCK_OC_Contact = newContact.PK;
			newOrgContactSellerLink.GCK_GG_Group = sellerGroup.PK;

			var newOrgImporterSet = factory.New<GlbGroupOrgLink>();
			newOrgImporterSet.GOK_GG_Group = importerGroup.PK;
			newOrgImporterSet.GOK_OH_Org = orgNew.PK;

			var newOrgContactImporterSet = factory.New<GlbGroupOrgContactLink>();
			newOrgContactImporterSet.GCK_OC_Contact = newContact.PK;
			newOrgContactImporterSet.GCK_GG_Group = importerGroup.PK;

			factory.Save();

			AssertEquals("Precondition: Should have 1 contact in new org", 1, orgNew.Contacts.Count);

			var merger = new OrganisationMergerForTest(orgOld.PK, orgNew.PK, new MergeOrgAddressCollection(factory), new MergeOrgContactCollection(factory, orgOld, orgNew));
			merger.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedOrgNew = newFactory.Load<OrgHeader>(orgNew.PK);
			var reloadedOrgNewSellerLink = newFactory.Load<GlbGroupOrgLink>(newOrgSellerLink.PK);
			var reloadedOrgNewContactSellerLink = newFactory.Load<GlbGroupOrgContactLink>(newOrgContactSellerLink.PK);
			var reloadedOrgBuyerLink = newFactory.Load<GlbGroupOrgLink>(oldOrgBuyerLink.PK);
			var reloadedOrgContactBuyerLink = newFactory.Load<GlbGroupOrgContactLink>(oldOrgContactBuyerLink.PK);
			var reloadedOrgImporterLink = newFactory.Load<GlbGroupOrgLink>(newOrgImporterSet.PK);
			var reloadedOrgContactImporterLink = newFactory.Load<GlbGroupOrgContactLink>(newOrgContactImporterSet.PK);

			AssertEquals("Precondition: Should have merged to contact in new org", 1, reloadedOrgNew.Contacts.Count);

			AssertEquals("Old org seller link should be deleted because a seller link already exists in the new org", null, newFactory.Load<GlbGroupOrgLink>(oldOrgSellerLink.PK));
			AssertEquals("Old org contact seller link should be deleted since it already exists against new org contact", null, newFactory.Load<GlbGroupOrgContactLink>(oldOrgContactSellerLink.PK));
			AssertEquals("New Org seller link should remain unchanged", orgNew.PK, reloadedOrgNewSellerLink.GOK_OH_Org);
			AssertEquals("New Org seller link should remain unchanged", sellerGroup.PK, reloadedOrgNewSellerLink.GOK_GG_Group);
			AssertEquals("New Org contact seller link should remain unchanged", sellerGroup.PK, reloadedOrgNewContactSellerLink.GCK_GG_Group);
			AssertEquals("New Org contact seller link should remain unchanged", newContact.PK, reloadedOrgNewContactSellerLink.GCK_OC_Contact);

			AssertEquals("Old org buyer link should be updated to point to the new org", orgNew.PK, reloadedOrgBuyerLink.GOK_OH_Org);
			AssertEquals("Org buyer link should continue pointing to the same buyer group", buyerGroup.PK, reloadedOrgBuyerLink.GOK_GG_Group);
			AssertEquals("Contact buyer link should continue pointing to the same buyer group", buyerGroup.PK, reloadedOrgContactBuyerLink.GCK_GG_Group);
			AssertEquals("Contact buyer link should point to merged contact", reloadedOrgNew.Contacts[0].PK, reloadedOrgContactBuyerLink.GCK_OC_Contact);

			AssertEquals("Org importer set should remain unchanged", orgNew.PK, reloadedOrgImporterLink.GOK_OH_Org);
			AssertEquals("Org importer set should remain unchanged", importerGroup.PK, reloadedOrgImporterLink.GOK_GG_Group);
			AssertEquals("Contact importer set should remain unchanged", importerGroup.PK, reloadedOrgContactImporterLink.GCK_GG_Group);
			AssertEquals("Contact importer set should remain unchanged", newContact.PK, reloadedOrgContactImporterLink.GCK_OC_Contact);
		}

		public void TestMergeWhsVASOrders()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var orgClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(orgClientPK);
			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);
			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);
			var warehouse = helper.CreateWarehouse("Whs1", "A");
			factory.Save();

			var area1 = new WhsArea(warehouse.PK.ToGuid(), "area1").InsertAndReturnObject(TestConnection);
			var area2 = new WhsArea(warehouse.PK.ToGuid(), "area2").InsertAndReturnObject(TestConnection);
			var area3 = new WhsArea(warehouse.PK.ToGuid(), "area3").InsertAndReturnObject(TestConnection);

			var oldOrgDO = OrgHeaderDO.ShallowLoadFromDB(TestConnection, orgClientPK.ToGuid());
			var newOrgDO = OrgHeaderDO.ShallowLoadFromDB(TestConnection, newClientPK.ToGuid());
			var difOrgDo = OrgHeaderDO.ShallowLoadFromDB(TestConnection, difClientPK.ToGuid());

			var oldOrgAddress = OrgAddressDO.ShallowLoadFromDB(TestConnection, oldOrg.MainAddress.PK.ToGuid());
			var newOrgAddress = OrgAddressDO.ShallowLoadFromDB(TestConnection, newOrg.MainAddress.PK.ToGuid());
			var difOrgAddress = OrgAddressDO.ShallowLoadFromDB(TestConnection, difOrg.MainAddress.PK.ToGuid());

			var vasOrder1 = new WhsVASOrder(oldOrgDO, area1, "1", "REF3").InsertAndReturnObject(TestConnection);
			var vasOrder2 = new WhsVASOrder(difOrgDo, area2, "2", "REF2").InsertAndReturnObject(TestConnection);
			var vasOrder3 = new WhsVASOrder(newOrgDO, area3, "3", "REF3").InsertAndReturnObject(TestConnection);

			factory.Save();

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 VASOrders link to NewOrg, 0 to oldOrg, 1 to difOrg
			WhsVASOrder.AssertFromDB(TestConnection, vasOrder1.PK)
				.ExpectEquals("Address should be updated.", v => v.WVO_OH_Client.FK, newOrg.PK)
				.ExpectEquals("Customer Ref should be updated to Job ID.", v => v.WVO_CustomerReferenceNo, "1")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder2.PK)
				.ExpectEquals("Address should be unchanged.", v => v.WVO_OH_Client.FK, difOrg.PK)
				.ExpectEquals("Customer Ref should be unchanged.", v => v.WVO_CustomerReferenceNo, "REF2")
				.VerifyAll();

			WhsVASOrder.AssertFromDB(TestConnection, vasOrder3.PK)
				.ExpectEquals("Address should be unchanged.", v => v.WVO_OH_Client.FK, newOrg.PK)
				.ExpectEquals("Customer Ref should be unchanged.", v => v.WVO_CustomerReferenceNo, "REF3")
				.VerifyAll();
		}

		public void TestMergeWhsPickFaces()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var orgClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(orgClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);

			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);

			var warehouse = helper.CreateWarehouse("Whs1", "A", 2, 2);

			var oldProduct = helper.CreateProduct(oldOrg.PK, "oldProduct");
			var newProduct = helper.CreateProduct(newOrg.PK, "newProduct");
			var difProduct = helper.CreateProduct(difOrg.PK, "difProduct");

			var locations = factory.Load<IWhsLocation>(new ZQuery());
			var oldLocation = locations[0];
			var newLocation = locations[1];

			var pickface1 = helper.CreatePickface(orgClientPK, warehouse.PK, oldProduct.PK, oldLocation.PK);
			var pickface2 = helper.CreatePickface(newClientPK, warehouse.PK, newProduct.PK, newLocation.PK);
			var pickface3 = helper.CreatePickface(difClientPK, warehouse.PK, difProduct.PK, oldLocation.PK);
			var pickface4 = helper.CreatePickface(newClientPK, warehouse.PK, oldProduct.PK, oldLocation.PK);

			factory.Save();

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 pickFaces link to NewOrg, 0 to oldOrg, 1 to difOrg, 1 to delete
			AssertEquals("Should find number of linked Pick Face records.", 2, WhsPickFace.CountInDB(TestConnection, o => o.WF_OH_Client == newClientPK));
			AssertEquals("Should find number of linked Pick Face records.", 0, WhsPickFace.CountInDB(TestConnection, o => o.WF_OH_Client == orgClientPK));
			AssertEquals("Should find number of linked Pick Face records.", 1, WhsPickFace.CountInDB(TestConnection, o => o.WF_OH_Client == difClientPK));
			AssertEquals("Pick Face should be deleted.", false, WhsPickFace.ExistsInDB(TestConnection, pickface1.PK.ToGuid()));
		}

		public void TestBarcodeRuleUnique()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);
			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);
			factory.Save();

			var ruleSet = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);

			var rule1 = new BarcodeRule(ruleSet, "Test rule 1", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeRule(ruleSet, "Test rule 2", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeRule(ruleSet, "Test rule 3", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 rules update to new rule number, 1 keep the origin
			BarcodeRule.AssertFromDB(TestConnection, rule1.PK)
				.ExpectEquals("test rule 1 keep the origin", rule => rule.BRU_RuleNumber, 1)
				.VerifyAll();
			BarcodeRule.AssertFromDB(TestConnection, rule2.PK)
				.ExpectEquals("test rule 2 updated", rule => rule.BRU_RuleNumber, 2)
				.VerifyAll();
			BarcodeRule.AssertFromDB(TestConnection, rule3.PK)
				.ExpectEquals("test rule 3 updated", rule => rule.BRU_RuleNumber, 3)
				.VerifyAll();
		}

		public void TestMergeAndDeleteBarcodeRuleSets_BuyerIsOld()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);

			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);

			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeRule(ruleSet1, "Test rule 1", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeRule(ruleSet2, "Test rule 2", 2) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var ruleSet3 = new BarcodeRuleSet("DEF") { BRS_IsSystem = false, BRS_OH_Buyer = difClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeRule(ruleSet3, "Test rule 3", 3) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 rules link to NewOrg, 0 to oldOrg, 1 to difOrg
			AssertEquals("Should delete one ruleSet.", 2, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			AssertEquals("Shouldn't change ruleSet3.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => o.BRS_OH_Buyer == difClientPK));
			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => o.PK != ruleSet3.PK && !o.BRS_IsSystem).Select(o => o.PK).ToArray();

			BarcodeRule.AssertFromDB(TestConnection, rule1.PK)
				.ExpectEquals("rule1 merged error.", o => o.BRU_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
			BarcodeRule.AssertFromDB(TestConnection, rule2.PK)
				.ExpectEquals("rule2 merged error.", o => o.BRU_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
		}

		public void TestMergeAndDeleteBarcodeRuleSets_BuyerIsOldSupplierIsNew()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);

			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);

			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid(), BRS_OH_Supplier = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeRule(ruleSet1, "Test rule 1", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid(), BRS_OH_Supplier = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeRule(ruleSet2, "Test rule 2", 2) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var ruleSet3 = new BarcodeRuleSet("DEF") { BRS_IsSystem = false, BRS_OH_Buyer = difClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeRule(ruleSet3, "Test rule 3", 3) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 rules link to NewOrg, 0 to oldOrg, 1 to difOrg
			AssertEquals("Should delete one ruleSet.", 2, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			AssertEquals("Shouldn't change ruleSet3.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => o.BRS_OH_Buyer == difClientPK));
			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => o.PK != ruleSet3.PK && !o.BRS_IsSystem).Select(o => o.PK).ToArray();

			BarcodeRule.AssertFromDB(TestConnection, rule1.PK)
				.ExpectEquals("rule1 merged error.", o => o.BRU_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
			BarcodeRule.AssertFromDB(TestConnection, rule2.PK)
				.ExpectEquals("rule2 merged error.", o => o.BRU_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
		}

		public void TestMergeAndDeleteBarcodeRuleSets_SupplierIsOldBuyerIsNew()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);

			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);

			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid(), BRS_OH_Supplier = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeRule(ruleSet1, "Test rule 1", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid(), BRS_OH_Supplier = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeRule(ruleSet2, "Test rule 2", 2) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var ruleSet3 = new BarcodeRuleSet("DEF") { BRS_IsSystem = false, BRS_OH_Buyer = difClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeRule(ruleSet3, "Test rule 3", 3) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 rules link to NewOrg, 0 to oldOrg, 1 to difOrg
			AssertEquals("Should delete one ruleSet.", 2, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			AssertEquals("Shouldn't change ruleSet3.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => o.BRS_OH_Buyer == difClientPK));
			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => o.PK != ruleSet3.PK && !o.BRS_IsSystem).Select(o => o.PK).ToArray();

			BarcodeRule.AssertFromDB(TestConnection, rule1.PK)
				.ExpectEquals("rule1 merged error.", o => o.BRU_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
			BarcodeRule.AssertFromDB(TestConnection, rule2.PK)
				.ExpectEquals("rule2 merged error.", o => o.BRU_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
		}

		public void TestMergeBarcodeRuleSets_BarcodeValidationRule()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);
			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);
			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			AssertEquals("Should delete one ruleSet.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			AssertEquals("Should have one rule", 1, BarcodeValidationRule.CountInDB(TestConnection, o => o.PK == rule.PK));

			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => !o.BRS_IsSystem).Select(o => o.PK).ToArray();
			BarcodeValidationRule.AssertFromDB(TestConnection, rule.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
		}

		public void TestMergeBarcodeRuleSets_BarcodeValidationRule_AndParsingRule()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);
			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);
			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var parsingRule1 = new BarcodeRule(ruleSet1, "Test rule 1", 1) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var validationRule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var parsingRule2 = new BarcodeRule(ruleSet2, "Test rule 2", 2) { BRU_Terminator = "\u001D" }.InsertAndReturnObject(TestConnection);
			var validationRule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			AssertEquals("Should delete one ruleSet.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => !o.BRS_IsSystem).Select(o => o.PK).ToArray();

			BarcodeRule.AssertFromDB(TestConnection, parsingRule1.PK)
				.ExpectEquals("rule1 merged error.", o => o.BRU_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeRule.AssertFromDB(TestConnection, parsingRule2.PK)
				.ExpectEquals("rule2 merged error.", o => o.BRU_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, validationRule1.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, validationRule2.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
		}

		public void TestMergeBarcodeRuleSets_MultipleValidationRules()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);
			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);
			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var validationRule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var validationRule2 = new BarcodeValidationRule(ruleSet1, "ANY", "PA2").InsertAndReturnObject(TestConnection);

			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var validationRule3 = new BarcodeValidationRule(ruleSet2, "ANY", "PRC").InsertAndReturnObject(TestConnection);
			var validationRule4 = new BarcodeValidationRule(ruleSet2, "ANY", "SER").InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			AssertEquals("Should delete one ruleSet.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => !o.BRS_IsSystem).Select(o => o.PK).ToArray();

			BarcodeValidationRule.AssertFromDB(TestConnection, validationRule1.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, validationRule2.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, validationRule3.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, validationRule4.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
		}

		public void TestMergeBarcodeRuleSets_BarcodeValidationRule_DifferentTargeField()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);
			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);
			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			AssertEquals("Should delete one ruleSet.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => !o.BRS_IsSystem).Select(o => o.PK).ToArray();

			BarcodeValidationRule.AssertFromDB(TestConnection, rule1.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, rule2.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();
		}

		public void TestMergeBarcodeRuleSets_BarcodeValidationRule_BuyerIsOld()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);

			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);

			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var ruleSet3 = new BarcodeRuleSet("DEF") { BRS_IsSystem = false, BRS_OH_Buyer = difClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeValidationRule(ruleSet3, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 rules link to NewOrg, 0 to oldOrg, 1 to difOrg
			AssertEquals("Should delete one ruleSet.", 2, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			AssertEquals("Shouldn't change ruleSet3.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => o.BRS_OH_Buyer == difClientPK));

			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => o.PK != ruleSet3.PK && !o.BRS_IsSystem).Select(o => o.PK).ToArray();
			AssertEquals(1, newRuleSetPks.Length);

			BarcodeValidationRule.AssertFromDB(TestConnection, rule1.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, rule2.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, rule3.PK)
				.ExpectEquals("rule3 remains the same", o => o.BVR_BRS_RuleSet.FK, ruleSet3.PK)
				.VerifyAll();
		}

		public void TestMergeBarcodeRuleSets_BarcodeValidationRule_BuyerIsOldSupplierIsNew()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);

			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);

			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = oldClientPK.ToGuid(), BRS_OH_Supplier = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid(), BRS_OH_Supplier = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var ruleSet3 = new BarcodeRuleSet("DEF") { BRS_IsSystem = false, BRS_OH_Buyer = difClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeValidationRule(ruleSet3, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 rules link to NewOrg, 0 to oldOrg, 1 to difOrg
			AssertEquals("Should delete one ruleSet.", 2, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			AssertEquals("Shouldn't change ruleSet3.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => o.BRS_OH_Buyer == difClientPK));

			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => o.PK != ruleSet3.PK && !o.BRS_IsSystem).Select(o => o.PK).ToArray();
			AssertEquals(1, newRuleSetPks.Length);

			BarcodeValidationRule.AssertFromDB(TestConnection, rule1.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, rule2.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, rule3.PK)
				.ExpectEquals("rule3 remains the same", o => o.BVR_BRS_RuleSet.FK, ruleSet3.PK)
				.VerifyAll();
		}

		public void TestMergeBarcodeRuleSets_BarcodeValidationRule_SupplierIsOldBuyerIsNew()
		{
			var factory = new BusinessObjectFactory();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClientPK = helper.CreateClient("OrgClient");
			var oldOrg = factory.Load<OrgHeader>(oldClientPK);

			var newClientPK = helper.CreateClient("NewClient");
			var newOrg = factory.Load<OrgHeader>(newClientPK);

			var difClientPK = helper.CreateClient("DifClient");
			var difOrg = factory.Load<OrgHeader>(difClientPK);

			factory.Save();

			var ruleSet1 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid(), BRS_OH_Supplier = oldClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule1 = new BarcodeValidationRule(ruleSet1, "ANY", "PA1").InsertAndReturnObject(TestConnection);
			var ruleSet2 = new BarcodeRuleSet("XYZ") { BRS_IsSystem = false, BRS_OH_Buyer = newClientPK.ToGuid(), BRS_OH_Supplier = newClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule2 = new BarcodeValidationRule(ruleSet2, "ANY", "PA2").InsertAndReturnObject(TestConnection);
			var ruleSet3 = new BarcodeRuleSet("DEF") { BRS_IsSystem = false, BRS_OH_Buyer = difClientPK.ToGuid() }.InsertAndReturnObject(TestConnection);
			var rule3 = new BarcodeValidationRule(ruleSet3, "ANY", "PA1").InsertAndReturnObject(TestConnection);

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(new BusinessObjectFactory(), oldOrg, newOrg));
			testMerger.Save();

			// should have 2 rules link to NewOrg, 0 to oldOrg, 1 to difOrg
			AssertEquals("Should delete one ruleSet.", 2, BarcodeRuleSet.CountInDB(TestConnection, o => !o.BRS_IsSystem));
			AssertEquals("Shouldn't change ruleSet3.", 1, BarcodeRuleSet.CountInDB(TestConnection, o => o.BRS_OH_Buyer == difClientPK));

			var barcodeRuleSets = BarcodeRuleSet.ShallowLoadFromDB(TestConnection);
			var newRuleSetPks = barcodeRuleSets.Where(o => o.PK != ruleSet3.PK && !o.BRS_IsSystem).Select(o => o.PK).ToArray();
			AssertEquals(1, newRuleSetPks.Length);

			BarcodeValidationRule.AssertFromDB(TestConnection, rule1.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, rule2.PK)
				.ExpectEquals("Should merge to one ruleSet", rule => rule.BVR_BRS_RuleSet.FK, newRuleSetPks[0])
				.VerifyAll();

			BarcodeValidationRule.AssertFromDB(TestConnection, rule3.PK)
				.ExpectEquals("rule3 remains the same", o => o.BVR_BRS_RuleSet.FK, ruleSet3.PK)
				.VerifyAll();
		}

		public void TestMergeProductionRuleSets()
		{
			var factory = new BusinessObjectFactory(TestConnection);
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(factory);

			var oldClient = helper.CreateClient("OrgClient").ToGuid();
			var oldOrg = factory.Load<OrgHeader>(oldClient);

			var newClient = helper.CreateClient("NewClient").ToGuid();
			var newOrg = factory.Load<OrgHeader>(newClient);

			var testGuid = ZGuid.NewZGuid().ToGuid();

			factory.Save();

			var ruleSet = new ProductionRuleSet("JBR", "Test rule set") { PRS_IsLive = false, PRS_IsSystem = false }.InsertAndReturnObject(TestConnection);
			var ruleDefinition = new
			{
				action = new
				{
					propertyToUpdate = oldClient,
					propertyToLeave = testGuid
				},
				condition1 = new
				{
					fieldPath = "LocalClient",
					operation = "equals",
					value = oldClient
				},
				condition2 = new
				{
					fieldPath = "UnrelatedClient",
					operation = "equals",
					value = testGuid
				}
			};

			var rule = new ProductionRule(ruleSet, "Test rule", JsonConvert.SerializeObject(ruleDefinition)).InsertAndReturnObject(TestConnection);

			factory.Save();

			var getDynamic = (string data) => JsonConvert.DeserializeObject(data) as dynamic;

			// Validate the saved rule before merging

			ProductionRule.AssertFromDB(TestConnection, rule.PK)
				.ExpectEquals("Rule does not contain correct GUID for: action.propertyToUpdate", pr => (Guid)getDynamic(pr.PRL_RuleDefinition)?.action?.propertyToUpdate, oldClient)
				.ExpectEquals("Rule does not contain correct GUID for: action.propertyToLeave", pr => (Guid)getDynamic(pr.PRL_RuleDefinition)?.action?.propertyToLeave, testGuid)
				.ExpectEquals("Rule does not contain correct GUID for: condition1.value", pr => (Guid)getDynamic(pr.PRL_RuleDefinition)?.condition1?.value, oldClient)
				.ExpectEquals("Rule does not contain correct GUID for: condition2.value", pr => (Guid)getDynamic(pr.PRL_RuleDefinition)?.condition2?.value, testGuid)
				.VerifyAll();

			var testMerger = new OrganisationMergerForTest(new MergeOrgHeader(factory, oldOrg, newOrg)) { ActionOnSave = OrganisationMergerActionOnSave.MergeOnly };
			testMerger.Save();

			// Validate the correct GUIDs have been changed after merging

			ProductionRule.AssertFromDB(TestConnection, rule.PK)
				.ExpectEquals("Rule did not update GUID for: action.propertyToUpdate", pr => (Guid)getDynamic(pr.PRL_RuleDefinition)?.action?.propertyToUpdate, newClient)
				.ExpectEquals("Rule incorrectly changed GUID for: action.propertyToLeave", pr => (Guid)getDynamic(pr.PRL_RuleDefinition)?.action?.propertyToLeave, testGuid)
				.ExpectEquals("Rule did not update GUID for: condition1.value", pr => (Guid)getDynamic(pr.PRL_RuleDefinition)?.condition1?.value, newClient)
				.ExpectEquals("Rule incorrectly changed GUID for: condition2.value", pr => (Guid)getDynamic(pr.PRL_RuleDefinition)?.condition2?.value, testGuid)
				.VerifyAll();
		}
	}

	public class OrganisationMergerForTest : OrganisationMerger
	{
		public OrganisationMergerForTest(MergeOrgHeader mergeOrgHeader)
			: base(mergeOrgHeader)
		{ }

		public OrganisationMergerForTest(ZGuid oldOrg, ZGuid newOrg, MergeOrgAddressCollection mergeAddressCollection, MergeOrgContactCollection mergeContactsCollection)
			: base(oldOrg, newOrg, mergeAddressCollection, mergeContactsCollection)
		{
		}

		public MergeOrgContactCollection mergeContactsCollectionForTest => mergeContactsCollection;

		public ITransactionManager BeginTransaction() => BeginTransactionWithManager();

		public void Save()
		{
			SaveInTransaction();
		}

		public void MoveOrgReferences_Exposed()
		{
			MoveOrgReferences();
		}

		public void SetOldOrganisationInactive_Exposed()
		{
			SetOldOrganisationInactive();
		}

		public bool CheckIfEdocsDbExists_Exposed(string name)
		{
			return CheckIfEdocsDbExists(name);
		}

		protected override void MoveARAPRecords(Guid[] pks, bool isAR)
		{
			base.MoveARAPRecords(pks, isAR);
			MoveARAPRecordsCounter++;
		}

		public int MoveARAPRecordsCounter { get; set; }

		protected override void MergeARAPRecords(Guid[] pks, bool isAR)
		{
			base.MergeARAPRecords(pks, isAR);
			MergeARAPRecordsCounter++;
		}

		public int MergeARAPRecordsCounter { get; set; }

		int number = 1000;
		protected override string GetNewOrderNumber(BusinessObjectFactory factory)
		{
			return (number++).ToString();
		}

		bool deleteOldOrg = true;
		public bool DeleteOldOrg
		{
			get { return deleteOldOrg; }
			set { deleteOldOrg = value; }
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			if (factoryForDeletingOldOrg != null && base.ActionOnSave == OrganisationMergerActionOnSave.DeleteOnly)
			{
				return factoryForDeletingOldOrg;
			}

			return base.GetNewFactory();
		}

		public BusinessObjectFactory factoryForDeletingOldOrg;

		internal bool ThrowExceptionOnOrgDelete { get; set; }
		internal int ThrowExceptionMaxTimes { get; set; } = int.MaxValue;
		internal int DeleteOldOrganizationRunTimes { get; set; }

		protected override void DeleteOldOrganization()
		{
			if (DeleteOldOrg)
			{
				DeleteOldOrganizationRunTimes++;

				if (ThrowExceptionOnOrgDelete && DeleteOldOrganizationRunTimes <= ThrowExceptionMaxTimes)
				{
					ZGuid pk = ZGuid.NewZGuid();
					string sqlText = string.Format(@"insert into dbo.orgsupplierpart (op_pk) values ('{0}')
				insert into dbo.orgpartrelation (OU_PK, ou_relationship, ou_oh, ou_op)
				values
				(
					NEWID(),
					'OWN',
					'{1}',
					'{0}'
				)", pk, oldOrganisation);
					Db.Connection.ExecuteNonQuery(sqlText); // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
					base.DeleteOldOrganization();
				}
				else
				{
					base.DeleteOldOrganization();
				}
			}
		}
	}

	class MergeActionForTest : IMergeAction
	{
		public void Merge(OrgHeader oldOrg, OrgHeader newOrg, OrganisationMergerActionOnSave action)
		{
			Calls.Add((oldOrg, newOrg, action));
		}

		public List<(OrgHeader oldOrg, OrgHeader newOrg, OrganisationMergerActionOnSave action)> Calls { get; } = new List<(OrgHeader, OrgHeader, OrganisationMergerActionOnSave)>();
	}

	[UseSnapshotProtection]
	public class TestMergeEDocs_NonTransactionedTest : TestCase
	{
		#region TestMergeMoveEDocs

		public void TestMergeMoveEdocs_WithOldUnallocatedDocs()
		{
			Guid oldOrgStorage = Guid.NewGuid();
			Guid newOrgStorage = Guid.NewGuid();

			AssertEDocs(@"
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + newOrgStorage.ToString() + @"', 'ORG', '1', '{0}')
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('76DE07A3-0C30-4F16-95CA-89F4A45E897E', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('107DC0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + oldOrgStorage.ToString() + @"', 'ORG', '2', '{1}')
				INSERT INTO " + Db.DatabaseName + @"..StorageDocs (SC_PK, SC_SM, SC_ParentID, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('266E07C3-0C30-4F16-95CA-89F4A45E897E', '" + oldOrgStorage.ToString() + @"', '{1}', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"..StorageDocs (SC_PK, SC_SM, SC_ParentID, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('209DA0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + oldOrgStorage.ToString() + @"', '{1}', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())"
				, 2, newOrgStorage, 2);
		}

		public void TestMergeMoveEdocs_WithOldUnallocatedAndEmptyParentMainFk()
		{
			Guid oldOrgStorage = Guid.NewGuid();
			Guid newOrgStorage = Guid.NewGuid();

			AssertEDocs(@"
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + newOrgStorage.ToString() + @"', 'ORG', '1', '{0}')
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('76DE07A3-0C30-4F16-95CA-89F4A45E897E', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('107DC0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + oldOrgStorage.ToString() + @"', 'ORG', '2', '{1}')
				INSERT INTO " + Db.DatabaseName + @"..StorageDocs (SC_PK, SC_SM, SC_ParentID, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('266E07C3-0C30-4F16-95CA-89F4A45E897E', '00000000-0000-0000-0000-000000000000', '{1}', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"..StorageDocs (SC_PK, SC_SM, SC_ParentID, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('209DA0CB-91DA-4BEE-998F-B5B2118C6BA1', '00000000-0000-0000-0000-000000000000', '{1}', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())"
				, 2, newOrgStorage, 2);
		}

		public void TestMergeMoveEdocs_WithNoOldStorageMain()
		{
			Guid oldOrgStorage = Guid.NewGuid();
			Guid newOrgStorage = Guid.NewGuid();

			AssertEDocs(@"
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + newOrgStorage.ToString() + @"', 'ORG', '1', '{0}')
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('76DE07A3-0C30-4F16-95CA-89F4A45E897E', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('107DC0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"..StorageDocs (SC_PK, SC_SM, SC_ParentID, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('266E07C3-0C30-4F16-95CA-89F4A45E897E', '00000000-0000-0000-0000-000000000000', '{1}', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"..StorageDocs (SC_PK, SC_SM, SC_ParentID, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('209DA0CB-91DA-4BEE-998F-B5B2118C6BA1', '00000000-0000-0000-0000-000000000000', '{1}', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())"
				, 2, newOrgStorage, 2);
		}

		public void TestMergeMoveEdocs_WithNoNewDocs()
		{
			Guid oldOrgStorage = Guid.NewGuid();
			Guid newOrgStorage = Guid.NewGuid();

			AssertEDocs(@"
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + oldOrgStorage.ToString() + @"', 'ORG', '1', '{1}')
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('76DE07C3-0C30-4F16-95CA-89F4A45E897E', '" + oldOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('107DA0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + oldOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())"
				, 2, oldOrgStorage, 0);
		}

		public void TestMergeMoveEdocs_WithBothOldAndNewAllocatedDocs()
		{
			Guid oldOrgStorage = Guid.NewGuid();
			Guid newOrgStorage = Guid.NewGuid();

			AssertEDocs(@"
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + newOrgStorage.ToString() + @"', 'ORG', '1', '{0}')
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('76DE07C3-0C30-4F16-95CA-89F4A45E897E', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('107DA0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + oldOrgStorage.ToString() + @"', 'ORG', '2', '{1}')
				INSERT INTO " + Db.DatabaseName + @"_SD002..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('26DE07C3-0C30-4F16-95CA-89F4A45E897E', '" + oldOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"_SD002..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('207DA0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + oldOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())"
				, 4, newOrgStorage, 0);
		}

		public void TestMergeMoveEdocs_WithOldAndNewInTheSameDatabase()
		{
			Guid oldOrgStorage = Guid.NewGuid();
			Guid newOrgStorage = Guid.NewGuid();

			AssertEDocs(@"
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + newOrgStorage.ToString() + @"', 'ORG', '1', '{0}')
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('76DE07C3-0C30-4F16-95CA-89F4A45E897E', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('107DA0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + newOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO dbo.StorageMain (SM_PK, SM_Type, SM_DB, SM_ParentFK) VALUES ('" + oldOrgStorage.ToString() + @"', 'ORG', '1', '{1}')
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('26DE07C3-0C30-4F16-95CA-89F4A45E897E', '" + oldOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Commercial Invoice - S00001002', 'N', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())
				INSERT INTO " + Db.DatabaseName + @"_SD001..StorageDocs (SC_PK, SC_SM, SC_DocType, SC_Desc, SC_IsSystemGenerated, SC_ImageData, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES ('207DA0CB-91DA-4BEE-998F-B5B2118C6BA1', '" + oldOrgStorage.ToString() + @"', 'MSC', 'EDI (Sydney) - Arrival Notice - S00001001', 'Y', cast(' ' as varbinary(max)), getdate(), getdate(), getdate())"
				, 4, newOrgStorage, 0);
		}

		void AssertEDocs(string sqlEdocs, int qtyDocs, Guid assertStorage, int qtyUnallocated)
		{
			newOrgHeader.MainAddress.OA_Code = "2";
			Guid reqDoc = Guid.NewGuid();

			string sqlText = string.Format(@"
				INSERT dbo.JobRequiredDocument (
					EQ_PK, EQ_DocType, EQ_DocUsage, EQ_DocDescription, EQ_DocNumber, EQ_CreditControlDoc, EQ_ParentID, EQ_ParentTableCode,
					EQ_DocPeriod, EQ_TemplateTransportMode, EQ_DocumentNotes, EQ_IsValid, EQ_DocCategory, EQ_RN_NKRelatedCountry, EQ_OriginalDocRequired)
				VALUES ('{0}', 'MSC', 'BRK', 'blabla', '', 0, '{1}','OH','SHP','','',1,'CSR','', 0)",
				reqDoc.ToString(), oldOrgHeader.PK.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			bool shouldDrop2 = false;
			bool shouldDrop1 = false;
			string db1Name = GetDbName(1);
			string db2Name = GetDbName(2);

			try
			{
				if (!testMerger.CheckIfEdocsDbExists_Exposed(db2Name))
				{
					createDB.Invoke(docManagerDbHelperInstance, new object[] { db2Name, Db.NewAdminConnection() });
					shouldDrop2 = true;
				}

				if (!testMerger.CheckIfEdocsDbExists_Exposed(db1Name))
				{
					createDB.Invoke(docManagerDbHelperInstance, new object[] { db1Name, Db.NewAdminConnection() });
					shouldDrop1 = true;
				}

				factory.Save();

				DeleteStorageDocsTableContents(StorageMainSchema.Constants.TableName);
				DeleteStorageDocsTableContents(db1Name + ".dbo." + StorageDocsSchema.Constants.TableName);
				DeleteStorageDocsTableContents(db2Name + ".dbo." + StorageDocsSchema.Constants.TableName);
				TestConnection.ExecuteNonQuery("DROP INDEX " + StorageMainSchema.Constants.TableName + "." + StorageMainSchema.Constants.Indexes.NR_UC__SM_ParentFK);

				sqlText = string.Format(sqlEdocs, newOrgHeader.PK.ToString(), oldOrgHeader.PK.ToString());
				TestConnection.ExecuteNonQuery(sqlText);

				using (var manager = testMerger.BeginTransaction())
				{
					testMerger.Save();
					manager.CommitTransaction();
				}

				DataTable storageMain = new DataTable(StorageMainSchema.Constants.TableName);
				DataTable storageDocs = new DataTable(StorageDocsSchema.Constants.TableName);
				DataTable storageDocs_SD001 = new DataTable(StorageDocsSchema.Constants.TableName);
				DataTable storageDocs_SD002 = new DataTable(StorageDocsSchema.Constants.TableName);
				DataTable requiredDoc = new DataTable(JobRequiredDocumentSchema.Constants.TableName);
				TestConnection.Command("SELECT * FROM dbo.StorageMain").NewDataAdapter().Fill(storageMain);
				TestConnection.Command("SELECT * FROM dbo.StorageDocs").NewDataAdapter().Fill(storageDocs);
				TestConnection.Command("SELECT * FROM " + db1Name + ".dbo.StorageDocs").NewDataAdapter().Fill(storageDocs_SD001);
				TestConnection.Command("SELECT * FROM " + db2Name + ".dbo.StorageDocs").NewDataAdapter().Fill(storageDocs_SD002);
				TestConnection.Command("SELECT * FROM dbo.JobRequiredDocument where EQ_PK = '" + reqDoc.ToString() + "'").NewDataAdapter().Fill(requiredDoc);

				AssertEquals("Should be " + qtyDocs.ToString() + " records in the SD001 StorageDocs table - no deletions, 2 should be moved to SD001", qtyDocs, storageDocs_SD001.Rows.Count);
				AssertEquals("Should be no records in the SD002 StorageDocs table, 2 deleted", 0, storageDocs_SD002.Rows.Count);
				AssertEquals("Should be " + qtyUnallocated.ToString() + " records in the main database StorageDocs table", qtyUnallocated, storageDocs.Rows.Count);

				if (storageDocs.Rows.Count > 0)
				{
					foreach (DataRow edoc in storageDocs.Rows)
					{
						AssertEquals(newOrgHeader.PK, edoc[StorageDocsSchema.SC_ParentID.Name]);
					}
				}

				AssertEquals("Should be 1 records in the StorageMain table - 1 deleted", 1, storageMain.Rows.Count);
				AssertEquals(qtyDocs.ToString() + " records should be attached to single StorageMain record", qtyDocs, storageDocs_SD001.Select("SC_SM ='" + assertStorage.ToString() + "'").Length);

				AssertEquals("There should be one required document", 1, requiredDoc.Rows.Count);
				AssertEquals("The required document should reference new org", newOrgHeader.PK, requiredDoc.Rows[0][JobRequiredDocumentSchema.EQ_ParentID.Name]);
			}
			finally
			{
				if (shouldDrop2)
				{
					dropDB.Invoke(docManagerDbHelperInstance, new string[1] { db2Name });
				}

				if (shouldDrop1)
				{
					dropDB.Invoke(docManagerDbHelperInstance, new string[1] { db1Name });
				}
			}
		}

		void DeleteStorageDocsTableContents(string tableName)
		{
			string sqlText = string.Format("delete {0}", tableName);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region TesteMergeEdocsWithReadOnlyDatabases

		public void TesteMergeEdocsWithReadOnlyDatabases()
		{
			if (new Regex(@"-DAT\d").IsMatch(System.Environment.MachineName))
			{
				Assert(true);
				return;
			}
			int oldDbNumber = 11;
			int newDbNumber = 12;
			int writeableDbNumber = 999;
			Guid oldStoragePk = Guid.NewGuid();
			Guid newStoragePk = Guid.NewGuid();

			try
			{
				CreateTestDatabases(testMerger, oldDbNumber, newDbNumber, writeableDbNumber);
				CreateOldAndNewOrgDocs(oldDbNumber, newDbNumber, oldStoragePk, newStoragePk);
				SetDatabasesAsReadOnly(oldDbNumber, newDbNumber);

				TestConnection.BeginTransaction();

				try
				{
					// Save Orgs and create StorageMain records
					factory.Save();
					CreateStorageMain(oldOrgHeader.PK.ToGuid(), oldStoragePk, oldDbNumber);
					CreateStorageMain(newOrgHeader.PK.ToGuid(), newStoragePk, newDbNumber);

					// Merge
					testMerger.Save();

					// Assert
					AssertReadonlyEdocsMergeResults(oldDbNumber, newDbNumber, writeableDbNumber, oldStoragePk, newStoragePk);
				}
				finally
				{
					TestConnection.RollbackTransaction();
				}
			}
			finally
			{
				DropTestDatabases(testMerger, oldDbNumber, newDbNumber, writeableDbNumber);
			}
		}

		void AssertReadonlyEdocsMergeResults(int oldOrgDbNumber, int newOrgDbNumber, int writeableDbNumber, Guid oldOrgStoragePk, Guid newOrgStoragePk)
		{
			// Old Org Documents still in the previous read-only DB
			AssertStorageDocsCount(oldOrgDbNumber, oldOrgStoragePk, 2);

			// Old Org Storage Main removed
			AssertStorageMainDbNumber("old organisation", oldOrgStoragePk, null);

			// New Org Documents still in the previous read-only DB
			AssertStorageDocsCount(newOrgDbNumber, newOrgStoragePk, 3);

			// Old+New Org Docs in the writeable DB pointing to New Org Storage Main
			AssertStorageDocsCount(writeableDbNumber, newOrgStoragePk, 5);
			AssertStorageMainDbNumber("new organisation", newOrgStoragePk, writeableDbNumber);
		}

		void AssertStorageDocsCount(int docDbNumber, Guid storageMainPk, int expectedCount)
		{
			string docDbName = GetDbName(docDbNumber);
			string sqlText = string.Format("SELECT count(*) FROM [{0}]..StorageDocs WHERE SC_SM = '{1}'", docDbName, storageMainPk.ToString());
			int actualCount = (int)TestConnection.ExecuteScalar(sqlText);
			AssertEquals("Database [" + docDbName + "] eDocs count", expectedCount, actualCount);
		}

		void AssertStorageMainDbNumber(string storageMainDesc, Guid storageMainPk, int? expectedDbNumber)
		{
			string sqlText = string.Format("SELECT SM_DB FROM dbo.StorageMain WHERE SM_PK = '{0}'", storageMainPk.ToString());
			object objResult = TestConnection.ExecuteScalar(sqlText);

			if (expectedDbNumber == null)
			{
				AssertNull("StorageMain [" + storageMainDesc + "] should have been removed", objResult);
			}
			else
			{
				AssertEquals("StorageMain [" + storageMainDesc + "] database number", expectedDbNumber.Value, (int)objResult);
			}
		}

		void CreateStorageMain(Guid parentPk, Guid storageMainPk, int dbNumber)
		{
			string insertStorageMainSql = string.Format(
				"INSERT dbo.StorageMain (SM_PK, SM_ParentFK, SM_DB) VALUES ('{0}', '{1}', {2});",
				storageMainPk.ToString(), parentPk.ToString(), dbNumber);
			TestConnection.ExecuteNonQuery(insertStorageMainSql);
		}

		void CreateOldAndNewOrgDocs(int oldOrgDbNumber, int newOrgDbNumber, Guid oldStoragePk, Guid newStoragePk)
		{
			string oldDbName = GetDbName(oldOrgDbNumber);
			string newDbName = GetDbName(newOrgDbNumber);

			string sqlText = string.Format(@"
				INSERT [{0}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_ImageData, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES (newid(), '{1}', sysutcdatetime(), 0x0001, sysutcdatetime(), sysutcdatetime());
				INSERT [{0}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_ImageData, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES (newid(), '{1}', sysutcdatetime(), 0x0001, sysutcdatetime(), sysutcdatetime());
				INSERT [{2}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_ImageData, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES (newid(), '{3}', sysutcdatetime(), 0x0002, sysutcdatetime(), sysutcdatetime());
				INSERT [{2}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_ImageData, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES (newid(), '{3}', sysutcdatetime(), 0x0002, sysutcdatetime(), sysutcdatetime());
				INSERT [{2}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_ImageData, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc) VALUES (newid(), '{3}', sysutcdatetime(), 0x0002, sysutcdatetime(), sysutcdatetime());
				",
				oldDbName, oldStoragePk.ToString(), newDbName, newStoragePk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void CreateTestDatabases(OrganisationMergerForTest testMerger, params int[] dbNumbersToCreate)
		{
			foreach (int dbNumber in dbNumbersToCreate)
			{
				var dbName = GetDbName(dbNumber);

				if (testMerger.CheckIfEdocsDbExists_Exposed(dbName))
				{
					dropDB.Invoke(docManagerDbHelperInstance, new object[] { dbName });
				}

				createDB.Invoke(docManagerDbHelperInstance, new object[] { dbName, Db.NewAdminConnection() });
			}
		}

		void DropTestDatabases(OrganisationMergerForTest testMerger, params int[] dbNumbersToDrop)
		{
			foreach (int dbNumber in dbNumbersToDrop)
			{
				var dbName = GetDbName(dbNumber);

				if (testMerger.CheckIfEdocsDbExists_Exposed(dbName))
				{
					dropDB.Invoke(docManagerDbHelperInstance, new object[] { dbName });
				}
			}
		}

		void SetDatabasesAsReadOnly(params int[] dbNumbersToSetAsReadOnly)
		{
			foreach (int dbNumber in dbNumbersToSetAsReadOnly)
			{
				string dbName = GetDbName(dbNumber);
				TestConnection.AlterDbWriteableStateForDocManager(dbName, false);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			InitialiseDocDbHelperMethods();
			InitialiseFactoryAndTestOrgs();
		}

		void InitialiseFactoryAndTestOrgs()
		{
			factory = new BusinessObjectFactory();

			oldOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			oldOrgHeader.OH_Code = "~TESTOLDORG~";

			newOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			newOrgHeader.OH_Code = "~TESTNEWORG~";

			testMerger = new OrganisationMergerForTest(
				oldOrgHeader.PK, newOrgHeader.PK,
				new MergeOrgAddressCollection(factory, oldOrgHeader, newOrgHeader),
				new MergeOrgContactCollection(factory, oldOrgHeader, newOrgHeader));
		}

		void InitialiseDocDbHelperMethods()
		{
			docManagerDbHelperInstance = Type.GetType("Enterprise.DocumentScanning.Business.Testing.DocManagerDBHelperTestClass, Enterprise.DocumentScanning.Business.Test").GetConstructor(Type.EmptyTypes).Invoke(null);
			dbHelperType = docManagerDbHelperInstance.GetType();
			createDB = dbHelperType.GetMethod("CreateDatabase_Raw_UsingADifferentConnection", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			dropDB = dbHelperType.GetMethod("DropDatabase", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			getDbName = dbHelperType.GetMethod("GetDatabaseName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
		}

		string GetDbName(int dbNumber)
		{
			return (string)getDbName.Invoke(docManagerDbHelperInstance, new object[1] { dbNumber });
		}

		DbConnection TestConnection
		{
			get { return Db.Connection; } // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
		}

		BusinessObjectFactory factory;
		OrgHeader oldOrgHeader;
		OrgHeader newOrgHeader;
		OrganisationMergerForTest testMerger;

		Type dbHelperType;
		MethodInfo createDB;
		MethodInfo dropDB;
		MethodInfo getDbName;
		object docManagerDbHelperInstance;

		#endregion
	}
}
