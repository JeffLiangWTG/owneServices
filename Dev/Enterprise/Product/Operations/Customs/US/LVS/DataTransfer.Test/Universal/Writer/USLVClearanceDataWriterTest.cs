using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	sealed class USLVClearanceDataWriterTest : USLVDataWriterTestHelper
	{
		public void TestUSLVClearanceDataMapping()
		{
			var clearanceBO = SetupULSVClearanceBO();
			var clearanceData = new USLVClearanceDataWriter(new DataWritingManager(new ActionInfo(null, clearanceBO))).GetDataObject(clearanceBO);
			AssertTestToClearance(clearanceBO, clearanceData);
		}

		void AssertTestToClearance(CusUSLVClearance clearance, Shipment shipment)
		{
			CombineAssertions(() =>
			{
				AssertEquals("WayBillNumber", clearance.ULH_MasterBill, shipment.WayBillNumber);
				AssertEquals("WayBillType", WayBillTypeList.Codes.Master, shipment.WayBillType.Code);
				AssertEquals("TransportMode", clearance.ULH_TransportMode, shipment.TransportMode.Code);
				AssertEquals("CustomsContainerMode", clearance.ULH_ContainerMode, shipment.CustomsContainerMode.Code);
				AssertEquals("PortOfLoading", clearance.ULH_RL_NKPortOfLoading, shipment.PortOfLoading.Code);
				AssertEquals("PortOfDischarge", clearance.ULH_RL_NKPortOfDischarge, shipment.PortOfDischarge.Code);
				AssertEquals("VesselName", clearance.ULH_ConveyanceName, shipment.VesselName);
				AssertEquals("VoyageFlightNo", clearance.ULH_VoyageFlightNo, shipment.VoyageFlightNo);
				AssertEquals("Branch", clearance.Branch.GB_Code, shipment.Branch.Code);

				AssertOrganizationAddressContents(shipment, AddressTypes.SendersLocalClient, clearance.Client.MainAddress);
				AssertOrganizationAddressContents(shipment, nameof(DocAddressType.ImporterOfRecord), clearance.Importer.MainAddress);

				AssertDate("LoadingDate", shipment, DateType.LoadingDate, clearance.ULH_DepartureDate);
				AssertDate("DischargeDate", shipment, DateType.DischargeDate, clearance.ULH_DischargeDate);

				AssertMasterAdditionalBill("Master Additional Bill", clearance, shipment, clearance.ULH_MasterBill, new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });

				AssertAddInfo("EntryFilerCode", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.EntryFilerCode, clearance.ULH_EntryFilerCode);
				AssertAddInfo("FIRMSCode", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.US_NKLocationOfGoods, clearance.ULH_US_NKLocationOfGoods);
				AssertAddInfo("CentralizedExamSite", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.US_NKCentralizedExamSite, clearance.ULH_US_NKCentralizedExamSite);
				AssertAddInfo("MasterWayBillIssuerSCAC", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.MasterWayBillIssuerSCAC, clearance.ULH_MasterBillIssuerSCAC);
				AssertAddInfo("UI_NKCarrierSCAC", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.UI_NKCarrierSCAC, clearance.ULH_CarrierSCAC);
				AssertAddInfo("SchDLoading", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.SchDLoading, clearance.ULH_PortOfLoading);
				AssertAddInfo("SchDEntry", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.SchDEntry, clearance.ULH_PortOfEntry);
				AssertAddInfo("SchDArrival", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.SchDArrival, clearance.ULH_PortOfDischarge);
				AssertAddInfo("EntryMode", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.EntryMode, clearance.ULH_RemoteLocationFiling ? "RLF" : "");
				AssertAddInfo("ArrivalDate", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.EntryDate, "2020-08-10");
				AssertAddInfo("IORType", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.IORType, clearance.ULH_IORType);
				AssertAddInfo("IORReference", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.IORReference, clearance.ULH_IORReference);
				AssertAddInfo("ContactName", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.FilerName, clearance.ULH_ContactName);
				AssertAddInfo("ContactPhone", shipment.AddInfoCollection, LVSConstants.AddInfoConstants.FilerPhoneNumber, clearance.ULH_ContactPhone);
			});
		}

		public void TestDataObject_WhenIsExportingToConsolidatedDeclaration()
		{
			var clearanceBO = Factory.New<CusUSLVClearance>();
			var org = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			clearanceBO.ULH_OH_Importer = org.PK;
			var actionInfo = new ActionInfo(null, clearanceBO);
			var dataWriter = new USLVClearanceDataWriter(new DataWritingManager(actionInfo));

			clearanceBO.CusUSLVConsignmentBatches.MoveNextBatch();
			var clearanceData = dataWriter.GetDataObject(clearanceBO);

			CombineAssertions("Writer is not exporting to Consolidated Declaration", () =>
			{
				AssertNull(clearanceData.CommercialInfo);
				var addressData = clearanceData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ImporterDocumentaryAddress));
				AssertNull(addressData);
				AssertNullAddInfo("ConsolACE", clearanceData.AddInfoCollection, "ConsolACE");
				AssertNullAddInfo("EnableENS", clearanceData.AddInfoCollection, "EnableENS");
				AssertNullAddInfo("EntryType", clearanceData.AddInfoCollection, "EntryType");
				AssertNullAddInfo("ConsolidatedInformalIndicator", clearanceData.AddInfoCollection, "ConsolidatedInformalIndicator");
			});

			actionInfo.ActionType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;
			clearanceData = dataWriter.GetDataObject(clearanceBO);
			CombineAssertions("Writer is exporting to Consolidated Declaration", () =>
			{
				AssertNotNull(clearanceData.CommercialInfo);
				AssertOrganizationAddressContents(clearanceData, nameof(DocAddressType.ImporterDocumentaryAddress), org.MainAddress);
				AssertAddInfo("ConsolACE", clearanceData.AddInfoCollection, "ConsolACE", "Y");
				AssertAddInfo("EnableENS", clearanceData.AddInfoCollection, "EnableENS", "Y");
				AssertAddInfo("EntryType", clearanceData.AddInfoCollection, "EntryType", "11");
				AssertAddInfo("ConsolidatedInformalIndicator", clearanceData.AddInfoCollection, "ConsolidatedInformalIndicator", "P");
			});
		}

		public void TestCommercialInfo_ConsolidatedDeclaration()
		{
			var clearanceBO = Factory.New<CusUSLVClearance>();
			var consignment1 = clearanceBO.CusUSLVConsignments.AddNew();
			consignment1.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			var item1 = consignment1.CusUSLVItems.AddNew();
			var consignment2 = clearanceBO.CusUSLVConsignments.AddNew();
			consignment2.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			var item2 = consignment2.CusUSLVItems.AddNew();
			var item3 = consignment2.CusUSLVItems.AddNew();

			clearanceBO.CusUSLVConsignmentBatches.MoveNextBatch();
			var clearanceData = new USLVClearanceDataWriter(new DataWritingManager(new ActionInfo(null, clearanceBO) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage })).GetDataObject(clearanceBO);

			AssertContainsExactElementsInAnyOrder("Invoice Header Number not repeated", ["1", "2"], clearanceData.CommercialInfo.CommercialInvoiceCollection.Select(x => x.InvoiceNumber));
			AssertEquals(3, clearanceData.CommercialInfo.CommercialInvoiceCollection.SelectMany(x => x.CommercialInvoiceLineCollection).Count());
		}

		public void TestCommercialInfo_ConsolidatedDeclaration_OnlyProcessEntryTypeInformalFreeDutiableConsignments()
		{
			var clearanceBO = Factory.New<CusUSLVClearance>();

			var consignment1 = clearanceBO.CusUSLVConsignments.AddNew();
			consignment1.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			var item1 = consignment1.CusUSLVItems.AddNew();

			var consignment2 = clearanceBO.CusUSLVConsignments.AddNew();
			consignment2.ULB_EntryType = EntryTypeList.Codes.LowValue;
			var item2 = consignment2.CusUSLVItems.AddNew();

			clearanceBO.CusUSLVConsignmentBatches.MoveNextBatch();
			var clearanceData = new USLVClearanceDataWriter(new DataWritingManager(new ActionInfo(null, clearanceBO) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage })).GetDataObject(clearanceBO);
			AssertEquals("Only process entry type informal free dutiable consignments", 1, clearanceData.CommercialInfo.CommercialInvoiceCollection.SelectMany(x => x.CommercialInvoiceLineCollection).Count());
		}

		public static CusUSLVClearance SetupULSVClearanceBO()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60267", "60267 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			factory.Save();
			var clearance = factory.New<CusUSLVClearance>();
			var org = GetOrganizationBO_WUFSHIJNB(factory);
			var branch = factory.Load<IGlbBranch>(Env.CurrentBranchPK);
			clearance.ULH_JobNumber = "ULH00000001";
			clearance.ULH_EntryFilerCode = "AAA";
			clearance.ULH_OH_Client = org.PK;
			clearance.ULH_ContactName = "ALAN PARSON";
			clearance.ULH_ContactPhone = "02 9000 2224";
			clearance.ULH_OH_Importer = org.PK;
			clearance.ULH_IORType = "CBN";
			clearance.ULH_IORReference = "11111111";
			clearance.ULH_MasterBill = "MasterBill1";
			clearance.ULH_MasterBillIssuerSCAC = "SCAC";
			clearance.ULH_CarrierSCAC = "CCAC";
			clearance.ULH_TransportMode = Core.Constants.TransportModes.Sea;
			clearance.ULH_ContainerMode = ContainerModeList.Codes.Containerized;
			clearance.ULH_PortOfLoading = "AUBEN";
			clearance.ULH_RL_NKPortOfLoading = "AUSYD";
			clearance.ULH_PortOfEntry = "1233";
			clearance.ULH_PortOfDischarge = "2704";
			clearance.ULH_RL_NKPortOfDischarge = "USLAX";
			clearance.ULH_DepartureDate = new ZDate(2020, 08, 04);
			clearance.ULH_EntryDate = new ZDate(2020, 08, 10);
			clearance.ULH_DischargeDate = new ZDate(2020, 08, 11);
			clearance.ULH_ConveyanceName = "UNITED KINGDOM";
			clearance.ULH_VoyageFlightNo = "V1";
			clearance.ULH_RemoteLocationFiling = true;
			clearance.ULH_SystemCreateTimeUtc = ZDateTime.UtcNow;
			clearance.ULH_SystemCreateUser = "A";
			clearance.ULH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			clearance.ULH_SystemLastEditUser = "B";
			clearance.ULH_GB = branch.PK;
			clearance.ULH_US_NKLocationOfGoods = "B815";
			clearance.ULH_US_NKCentralizedExamSite = "B815";
			factory.Save();
			return clearance;
		}
	}
}
