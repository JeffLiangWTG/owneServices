using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(USConsignmentCombined))]
	class USConsignmentCombinedTest : EnterpriseBusinessObjectTestCase
	{
		#region TestGetParentBizos

		public void TestGetConsignment()
		{
			var consignment = CreateValidConsignment();
			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			AssertEquals(consignment, viewFromConsignment.Consignment);
		}

		public void TestGetConsignment_WhenCreatedFromDeclaration_IsNull()
		{
			var declaration = CreateValidDeclaration();
			Factory.Save();

			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);
			AssertNull(viewFromDeclaration.Consignment);
		}

		public void TestGetDeclaration()
		{
			var declaration = CreateValidDeclaration();
			Factory.Save();

			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);
			AssertEquals(declaration, viewFromDeclaration.Declaration);
		}

		public void TestGetDeclaration_WhenCreatedFromConsignment_IsNull()
		{
			var consignment = CreateValidConsignment();
			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			AssertNull(viewFromConsignment.Declaration);
		}

		#endregion

		#region Load View from Consignment/Declaration Maps Properties

		public void TestLoadView_FromConsignment_MapsConsignmentProperties()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "8542996328";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDate.Today;
			tariff.UE_PGACodes = OGARequirementList.Codes.FD1;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "TSTCONSIGNEE";
			seller.OH_FullName = "TSTSELLER";

			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			var sellerAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_Code = "CONS";
			sellerAddress.OA_Code = "SELL";
			consigneeAddress.OA_OH = consignee.PK;
			sellerAddress.OA_OH = seller.PK;

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_JobNumber = "JOB1234";
			clearance.ULH_MasterBill = "1122";
			clearance.ULH_GB = Env.CurrentBranchPK;
			clearance.ULH_DepartureDate = new ZDate(2020, 01, 01);
			clearance.ULH_DischargeDate = new ZDate(2020, 02, 02);
			clearance.ULH_EntryDate = new ZDate(2020, 03, 03);
			clearance.ULH_TransportMode = Core.Constants.TransportModes.Air;
			clearance.ULH_ConveyanceName = "CONVYNC";
			clearance.ULH_VoyageFlightNo = "VYG123";
			clearance.ULH_RL_NKPortOfDischarge = "USCHI";
			clearance.ULH_RL_NKPortOfLoading = "USDEC";
			clearance.ULH_PortOfDischarge = "1199";
			clearance.ULH_PortOfLoading = "2288";
			clearance.ULH_PortOfEntry = "3377";
			clearance.ULH_OH_Client = client.PK;
			clearance.ULH_OH_Importer = importer.PK;

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "4455";
			consignment.ULB_MessageStatus = ImportMessageStatusList.Codes.ClearArrival;
			consignment.ULB_SubmittedDate = new ZDate(2020, 04, 04);
			consignment.ULB_OA_Consignee = consigneeAddress.PK;
			consignment.ULB_OA_Seller = sellerAddress.PK;
			consignment.ULB_IsActive = false;

			var item = consignment.CusUSLVItems.AddNew();
			item.ULI_AntiDumping = false;
			item.ULI_Tariff = tariff.UE_Tariff;

			consignment.CE_EntryNum = "1234";
			consignment.CE_EntryStatus = ImportEntryStatusList.Codes.CRL;
			consignment.CE_IssueDate = new ZDate(2020, 05, 05);

			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(consignment.PK);
			AssertNotNull("View instance created:", view);

			CombineAssertions("View should have properties been populated from Consignment", () =>
			{
				AssertEquals("UBV_JobType: ", USConsignmentCombinedJobTypes.Codes.Consignment, view.UBV_JobType);
				AssertEquals("UBV_GB: ", clearance.ULH_GB, view.UBV_GB);
				AssertEquals("UBV_HouseBill: ", consignment.ULB_HouseBill, view.UBV_HouseBill);
				AssertEquals("UBV_EntryNum: ", consignment.CE_EntryNum, view.UBV_EntryNum);
				AssertEquals("UBV_JobReference: ", clearance.ULH_JobNumber, view.UBV_JobReference);
				AssertEquals("UBV_MasterBill: ", clearance.ULH_MasterBill, view.UBV_MasterBill);
				AssertEquals("UBV_MessageStatus: ", consignment.ULB_MessageStatus, view.UBV_MessageStatus);
				AssertEquals("UBV_ReleaseStatus: ", consignment.CE_EntryStatus, view.UBV_ReleaseStatus);
				AssertEquals("UBV_DepartureDate: ", clearance.ULH_DepartureDate, view.UBV_DepartureDate);
				AssertEquals("UBV_DischargeDate: ", clearance.ULH_DischargeDate, view.UBV_DischargeDate);
				AssertEquals("UBV_EntryDate: ", clearance.ULH_EntryDate, view.UBV_EntryDate);
				AssertEquals("UBV_ReleaseDate: ", consignment.CE_IssueDate, view.UBV_ReleaseDate);
				AssertEquals("UBV_SubmittedDate: ", consignment.ULB_SubmittedDate, view.UBV_SubmittedDate);
				AssertEquals("UBV_TransportMode: ", clearance.ULH_TransportMode, view.UBV_TransportMode);
				AssertEquals("UBV_ConveyanceName: ", clearance.ULH_ConveyanceName, view.UBV_ConveyanceName);
				AssertEquals("UBV_VoyageFlightNo: ", clearance.ULH_VoyageFlightNo, view.UBV_VoyageFlightNo);
				AssertEquals("UBV_RL_NKPortOfLoading: ", clearance.ULH_RL_NKPortOfLoading, view.UBV_RL_NKPortOfLoading);
				AssertEquals("UBV_RL_NKPortOfDischarge: ", clearance.ULH_RL_NKPortOfDischarge, view.UBV_RL_NKPortOfDischarge);
				AssertEquals("UBV_PortOfLoading: ", clearance.ULH_PortOfLoading, view.UBV_PortOfLoading);
				AssertEquals("UBV_PortOfDischarge: ", clearance.ULH_PortOfDischarge, view.UBV_PortOfDischarge);
				AssertEquals("UBV_PortOfEntry: ", clearance.ULH_PortOfEntry, view.UBV_PortOfEntry);
				AssertEquals("UBV_OH_Client: ", clearance.ULH_OH_Client, view.UBV_OH_Client);
				AssertEquals("UBV_OH_Importer: ", clearance.ULH_OH_Importer, view.UBV_OH_Importer);
				AssertEquals("UBV_OH_Consignee: ", consignee.PK, view.UBV_OH_Consignee);
				AssertEquals("UBV_ConsigneeName: ", consignee.OH_FullName, view.UBV_ConsigneeName);
				AssertEquals("UBV_OH_Seller: ", seller.PK, view.UBV_OH_Seller);
				AssertEquals("UBV_SellerName: ", seller.OH_FullName, view.UBV_SellerName);
				AssertEquals("UBV_IsActive: ", consignment.ULB_IsActive, view.UBV_IsActive);
				AssertEquals("UBV_HasPGAPending: ", consignment.ULB_HasPGAPending, view.UBV_HasPGAPending);
				AssertEquals("UBV_PGANotSupported: ", consignment.ULB_PGANotSupported, view.UBV_PGANotSupported);
			});
		}

		public void TestLoadView_FromConsignment_WithNoCusEntryNum_DefaultsToEmptyValues()
		{
			var consignment = CreateValidConsignment();

			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(consignment.PK);
			AssertNotNull("View instance created:", view);

			CombineAssertions("View should have defaulted CusEntryNum fields to empty", () =>
			{
				AssertEquals("UBV_EntryNum", string.Empty, view.UBV_EntryNum);
				AssertEquals("UBV_ReleaseStatus", string.Empty, view.UBV_ReleaseStatus);
			});
		}

		public void TestLoadView_FromConsignment_WithIncorrectCusEntryNumCategory_DefaultsToEmptyValues()
		{
			var consignment = CreateValidConsignment();

			var otherEntryNumber = Factory.New<CusEntryNumber>();
			otherEntryNumber.CE_ParentID = consignment.PK;
			otherEntryNumber.CE_ParentTable = consignment.TableName;
			otherEntryNumber.CE_EntryType = CusEntryNumberTypes.UnitedStates.EntrySummary;
			otherEntryNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			otherEntryNumber.CE_EntryNum = "4567";
			otherEntryNumber.CE_EntryStatus = ImportEntryStatusList.Codes.CRF;
			otherEntryNumber.CE_IssueDate = new ZDate(2020, 06, 06);

			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(consignment.PK);
			AssertNotNull("View instance created:", view);

			CombineAssertions("View should have defaulted CusEntryNum fields to empty", () =>
			{
				AssertEquals("UBV_EntryNum", string.Empty, view.UBV_EntryNum);
				AssertEquals("UBV_ReleaseStatus", string.Empty, view.UBV_ReleaseStatus);
			});
		}

		public void TestLoadView_FromConsignment_WithMultipleCusEntryNumbers_OnlyUsesCustomsPermitClearanceNumber()
		{
			var consignment = CreateValidConsignment();
			consignment.CE_EntryNum = "1234";
			consignment.CE_EntryStatus = ImportEntryStatusList.Codes.CRL;
			consignment.CE_IssueDate = new ZDate(2020, 05, 05);

			consignment.CE_RailReferenceNumber = "RailReference";
			var otherEntryNumber = consignment.OTHEntryNumber;
			otherEntryNumber.CE_EntryNum = "4567";
			otherEntryNumber.CE_EntryStatus = ImportEntryStatusList.Codes.CRF;
			otherEntryNumber.CE_IssueDate = new ZDate(2020, 06, 06);

			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(consignment.PK);
			AssertNotNull("View instance created:", view);

			CombineAssertions("View should have only used fields from Customs Permit Clearance Number", () =>
			{
				AssertEquals("UBV_EntryNum", consignment.CE_EntryNum, view.UBV_EntryNum);
				AssertEquals("UBV_ReleaseStatus", consignment.CE_EntryStatus, view.UBV_ReleaseStatus);
				AssertEquals("UBV_ReleaseDate: ", consignment.CE_IssueDate, view.UBV_ReleaseDate);
			});
		}

		public void TestLoadView_FromConsignment_WithNoOrgShipperConsigneeNames()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = null;
			seller.OH_FullName = null;

			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			var sellerAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_Code = "CONS";
			sellerAddress.OA_Code = "SELL";
			consigneeAddress.OA_OH = consignee.PK;
			sellerAddress.OA_OH = seller.PK;

			var consignment = CreateValidConsignment();
			consignment.ULB_ConsigneeName = "TSTCONSNAME";
			consignment.ULB_SellerName = "TSTSELLNAME";

			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(consignment.PK);
			AssertNotNull("View instance created:", view);

			CombineAssertions("View should have fallen back to Consignment name values when missing Org names", () =>
			{
				AssertEquals("UBV_ConsigneeName", consignment.ULB_ConsigneeName, view.UBV_ConsigneeName);
				AssertEquals("UBV_SellerName", consignment.ULB_SellerName, view.UBV_SellerName);
			});
		}

		public void TestLoadView_FromDeclaration()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "TSTCONSIGNEE";
			seller.OH_FullName = "TSTSELLER";

			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			var sellerAddress = Factory.NewWithValidTestData<OrgAddress>();
			var importerAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_Code = "CONS";
			sellerAddress.OA_Code = "SELL";
			importerAddress.OA_Code = "IMPR";

			consigneeAddress.OA_OH = consignee.PK;
			sellerAddress.OA_OH = seller.PK;
			importerAddress.OA_OH = importer.PK;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel";
			vessel.RV_LloydsNumber = "Lloyds";
			vessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;

			var jobDeclaration = CreateValidDeclaration();
			jobDeclaration.JE_GB = Env.CurrentBranchPK;
			jobDeclaration.JE_DeclarationReference = "DEC1234";
			jobDeclaration.JE_MasterBill = "9988";
			jobDeclaration.ReleaseStatus = CRLReleaseStatusList.Codes.NRL;
			jobDeclaration.JE_HouseBill = "1122";
			jobDeclaration.JE_ExportDate = new ZDate(2020, 01, 01);
			jobDeclaration.JE_DateOfArrival = new ZDate(2020, 02, 02);
			jobDeclaration.US_EntryDate = new ZDate(2020, 03, 03);
			jobDeclaration.JE_EntryAuthorisationDate = new ZDate(2020, 04, 04);
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			jobDeclaration.JE_VesselName = vessel.RV_Code;
			jobDeclaration.JE_VoyageFlightNo = "5566";
			jobDeclaration.JE_RL_NKPortOfLoading = "USCHI";
			jobDeclaration.JE_RL_NKPortOfArrival = "USDEC";
			jobDeclaration.US_SchDArrival = "1101";
			jobDeclaration.US_SchDLoading = "58201";
			jobDeclaration.US_SchDEntry = "1102";
			jobDeclaration.JE_OA_DeclarantAddress = importerAddress.PK;
			jobDeclaration.JE_OA_ConsigneeAddress = consigneeAddress.PK;
			jobDeclaration.JE_OA_SellerAddress = sellerAddress.PK;
			jobDeclaration.IsCancelled = true;

			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_JE = jobDeclaration.PK;
			cusEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			cusEntryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			cusEntryHeader.CH_EntrySubmittedDate = new ZDate(2020, 05, 05);

			var cusEntryNum = Factory.New<CusEntryNumber>();
			cusEntryNum.CE_ParentID = jobDeclaration.PK;
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.UnitedStates.EntrySummary;
			cusEntryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNum.CE_EntryNum = "1234";
			cusEntryNum.CE_EntryStatus = ImportEntryStatusList.Codes.CRL;

			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(jobDeclaration.PK);
			AssertNotNull("View instance created:", view);

			CombineAssertions("View properties should have been populated from Job Declaration", () =>
			{
				AssertEquals("UBV_JobType: ", USConsignmentCombinedJobTypes.Codes.Declaration, view.UBV_JobType);
				AssertEquals("UBV_GB: ", jobDeclaration.JE_GB, view.UBV_GB);
				AssertEquals("UBV_HouseBill: ", jobDeclaration.JE_HouseBill, view.UBV_HouseBill);
				AssertEquals("UBV_EntryNum: ", cusEntryNum.CE_EntryNum, view.UBV_EntryNum);
				AssertEquals("UBV_JobReference: ", jobDeclaration.JE_DeclarationReference, view.UBV_JobReference);
				AssertEquals("UBV_MasterBill: ", jobDeclaration.JE_MasterBill, view.UBV_MasterBill);
				AssertEquals("UBV_MessageStatus: ", cusEntryHeader.CH_Status, view.UBV_MessageStatus);
				AssertEquals("UBV_ReleaseStatus: ", jobDeclaration.ReleaseStatus, view.UBV_ReleaseStatus);
				AssertEquals("UBV_DepartureDate: ", jobDeclaration.JE_ExportDate, view.UBV_DepartureDate);
				AssertEquals("UBV_DischargeDate: ", jobDeclaration.JE_DateOfArrival, view.UBV_DischargeDate);
				AssertEquals("UBV_EntryDate: ", jobDeclaration.US_EntryDate, view.UBV_EntryDate);
				AssertEquals("UBV_ReleaseDate: ", jobDeclaration.JE_EntryAuthorisationDate, view.UBV_ReleaseDate);
				AssertEquals("UBV_SubmittedDate: ", cusEntryHeader.CH_EntrySubmittedDate, view.UBV_SubmittedDate);
				AssertEquals("UBV_TransportMode: ", jobDeclaration.JE_TransportMode, view.UBV_TransportMode);
				AssertEquals("UBV_ConveyanceName: ", jobDeclaration.JE_VesselName, view.UBV_ConveyanceName);
				AssertEquals("UBV_VoyageFlightNo: ", jobDeclaration.JE_VoyageFlightNo, view.UBV_VoyageFlightNo);
				AssertEquals("UBV_RL_NKPortOfLoading: ", jobDeclaration.JE_RL_NKPortOfLoading, view.UBV_RL_NKPortOfLoading);
				AssertEquals("UBV_RL_NKPortOfDischarge: ", jobDeclaration.JE_RL_NKPortOfArrival, view.UBV_RL_NKPortOfDischarge);
				AssertEquals("UBV_PortOfLoading: ", jobDeclaration.US_SchDLoading, view.UBV_PortOfLoading);
				AssertEquals("UBV_PortOfDischarge: ", jobDeclaration.US_SchDArrival, view.UBV_PortOfDischarge);
				AssertEquals("UBV_PortOfEntry: ", jobDeclaration.US_SchDEntry, view.UBV_PortOfEntry);
				AssertEquals("UBV_OH_Client: ", ZGuid.Empty, view.UBV_OH_Client);
				AssertEquals("UBV_OH_Importer: ", importer.PK, view.UBV_OH_Importer);
				AssertEquals("UBV_OH_Consignee: ", consignee.PK, view.UBV_OH_Consignee);
				AssertEquals("UBV_ConsigneeName: ", consignee.OH_FullName, view.UBV_ConsigneeName);
				AssertEquals("UBV_OH_Seller: ", seller.PK, view.UBV_OH_Seller);
				AssertEquals("UBV_SellerName: ", seller.OH_FullName, view.UBV_SellerName);
				AssertEquals("UBV_IsActive: ", !jobDeclaration.JE_IsCancelled, view.UBV_IsActive);
				AssertEquals("UBV_HasPGAPending: ", false, view.UBV_HasPGAPending);
				AssertEquals("UBV_PGANotSupported: ", false, view.UBV_PGANotSupported);
			});
		}

		public void TestLoadView_FromDeclaration_WithNoCusEntryNumOrHeader_DefaultsToEmptyValues()
		{
			var jobDeclaration = CreateValidDeclaration();
			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(jobDeclaration.PK);
			AssertNotNull("View instance created:", view);

			CombineAssertions("View should have defaulted CusEntryNum fields to empty", () =>
			{
				AssertEquals("UBV_EntryNum", string.Empty, view.UBV_EntryNum);
				AssertEquals("UBV_MessageStatus", string.Empty, view.UBV_MessageStatus);
			});
		}

		public void TestLoadView_FromDeclaration_WithIncorrectCusEntryNumCategory_DefaultsToEmptyValues()
		{
			var jobDeclaration = CreateValidDeclaration();

			var otherEntryNumber = Factory.New<CusEntryNumber>();
			otherEntryNumber.CE_ParentID = jobDeclaration.PK;
			otherEntryNumber.CE_ParentTable = jobDeclaration.TableName;
			otherEntryNumber.CE_EntryType = CusEntryNumberTypes.UnitedStates.EntrySummary;
			otherEntryNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			otherEntryNumber.CE_EntryNum = "4567";

			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(jobDeclaration.PK);
			AssertNotNull("View instance created:", view);
			AssertEquals("UBV_EntryNum", string.Empty, view.UBV_EntryNum);
		}

		public void TestLoadView_FromDeclaration_WithMultipleCusEntryNumbers_OnlyUsesCustomsPermitClearanceNumber()
		{
			var jobDeclaration = CreateValidDeclaration();

			var cusPermitClearanceNumber = Factory.New<CusEntryNumber>();
			cusPermitClearanceNumber.CE_ParentID = jobDeclaration.PK;
			cusPermitClearanceNumber.CE_ParentTable = jobDeclaration.TableName;
			cusPermitClearanceNumber.CE_EntryType = CusEntryNumberTypes.UnitedStates.EntrySummary;
			cusPermitClearanceNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusPermitClearanceNumber.CE_EntryNum = "1234";

			var otherEntryNumber = Factory.New<CusEntryNumber>();
			otherEntryNumber.CE_ParentID = jobDeclaration.PK;
			otherEntryNumber.CE_ParentTable = jobDeclaration.TableName;
			otherEntryNumber.CE_EntryType = CusEntryNumberTypes.UnitedStates.EntrySummary;
			otherEntryNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			otherEntryNumber.CE_EntryNum = "4567";

			Factory.Save();

			var view = Factory.Load<USConsignmentCombined>(jobDeclaration.PK);
			AssertNotNull("View instance created:", view);
			AssertEquals("UBV_EntryNum", cusPermitClearanceNumber.CE_EntryNum, view.UBV_EntryNum);
		}

		#endregion

		#region Non Schema Properties

		public void TestCarrierSCAC()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.Shipment.ULH_CarrierSCAC = "AAA";
			declaration.US_UI_NKCarrierSCAC = "BBB";

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment CarrierSCAC:", consignment.Shipment.ULH_CarrierSCAC, viewFromConsignment.CarrierSCAC);
				AssertEquals("Declaration CarrierSCAC:", declaration.US_UI_NKCarrierSCAC, viewFromDeclaration.CarrierSCAC);
			});
		}

		public void TestContactName()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.Shipment.ULH_ContactName = "AAA";

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment ContactName:", consignment.Shipment.ULH_ContactName, viewFromConsignment.ContactName);
				AssertEquals("Declaration ContactName:", string.Empty, viewFromDeclaration.ContactName);
			});
		}

		public void TestContactPhone()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.Shipment.ULH_ContactPhone = "1234";

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment ContactPhone:", consignment.Shipment.ULH_ContactPhone, viewFromConsignment.ContactPhone);
				AssertEquals("Declaration ContactPhone:", string.Empty, viewFromDeclaration.ContactPhone);
			});
		}

		public void TestContainerMode()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.Shipment.ULH_ContainerMode = ContainerModeList.Codes.Liquid;
			declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment ContainerMode:", consignment.Shipment.ULH_ContainerMode, viewFromConsignment.ContainerMode);
				AssertEquals("Declaration ContainerMode:", declaration.JE_ContainerMode, viewFromDeclaration.ContainerMode);
			});
		}

		public void TestEntryFilerCode()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.Shipment.ULH_EntryFilerCode = "AAA";
			declaration.US_EntryFilerCode = "BBB";

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment EntryFilerCode:", consignment.Shipment.ULH_EntryFilerCode, viewFromConsignment.EntryFilerCode);
				AssertEquals("Declaration EntryFilerCode:", declaration.US_EntryFilerCode, viewFromDeclaration.EntryFilerCode);
			});
		}

		public void TestIORReference()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.Shipment.ULH_IORReference = "AAA";
			consignment.Shipment.ULH_IORType = OrgCusCode.USACodeTypes.SocialSecurityNumber;

			var declarationIOR = Factory.NewWithValidTestData<OrgHeader>();
			var declarationIORCusCode = declarationIOR.CustomsCodes.AddNew();
			declarationIORCusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			declarationIORCusCode.OK_CustomsRegNo = "BBB";
			var declarationIORAddress = Factory.NewWithValidTestData<OrgAddress>();
			declarationIORAddress.OA_Code = "ADRESS";
			declarationIORAddress.OA_OH = declarationIOR.PK;
			declaration.JE_OA_DeclarantAddress = declarationIORAddress.PK;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment IORReference:", consignment.Shipment.ULH_IORReference, viewFromConsignment.IORReference);
				AssertEquals("Declaration IORReference:", declarationIORCusCode.OK_CustomsRegNo, viewFromDeclaration.IORReference);
			});
		}

		public void TestIORType()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.Shipment.ULH_IORReference = "AAA";
			consignment.Shipment.ULH_IORType = OrgCusCode.USACodeTypes.SocialSecurityNumber;

			var declarationIOR = Factory.NewWithValidTestData<OrgHeader>();
			var declarationIORCusCode = declarationIOR.CustomsCodes.AddNew();
			declarationIORCusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			declarationIORCusCode.OK_CustomsRegNo = "BBB";
			var declarationIORAddress = Factory.NewWithValidTestData<OrgAddress>();
			declarationIORAddress.OA_Code = "ADRESS";
			declarationIORAddress.OA_OH = declarationIOR.PK;
			declaration.JE_OA_DeclarantAddress = declarationIORAddress.PK;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment IORType:", consignment.Shipment.ULH_IORType, viewFromConsignment.IORType);
				AssertEquals("Declaration IORType:", declarationIORCusCode.OK_CodeType, viewFromDeclaration.IORType);
			});
		}

		public void TestMasterBillIssuerSCAC()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.Shipment.ULH_MasterBillIssuerSCAC = "AAA";
			declaration.JE_MasterBillIssuerSCAC = "BBB";

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment MasterBillIssuerSCAC:", consignment.Shipment.ULH_MasterBillIssuerSCAC, viewFromConsignment.MasterBillIssuerSCAC);
				AssertEquals("Declaration MasterBillIssuerSCAC:", declaration.JE_MasterBillIssuerSCAC, viewFromDeclaration.MasterBillIssuerSCAC);
			});
		}

		public void TestHouseBillIssuerSCAC()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.ULB_HouseBillIssuerSCAC = "AAA";
			declaration.JE_HouseBillIssuerSCAC = "BBB";

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment HouseBillIssuerSCAC:", consignment.ULB_HouseBillIssuerSCAC, viewFromConsignment.HouseBillIssuerSCAC);
				AssertEquals("Declaration HouseBillIssuerSCAC:", declaration.JE_HouseBillIssuerSCAC, viewFromDeclaration.HouseBillIssuerSCAC);
			});
		}

		public void TestOwnerReferenceNumber()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.ULB_OwnerReferenceNumber = "AAA";
			declaration.JE_OwnerRef = "BBB";

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment OwnerReferenceNumber:", consignment.ULB_OwnerReferenceNumber, viewFromConsignment.OwnerReferenceNumber);
				AssertEquals("Declaration OwnerReferenceNumber:", declaration.JE_OwnerRef, viewFromDeclaration.OwnerReferenceNumber);
			});
		}

		public void TestEquipmentNumber()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();
			var declarationWithMultipleContainers = CreateValidDeclaration();
			consignment.ULB_EquipmentNumber = "AAA";

			var declarationContainerA = declaration.CusContainers.AddNew();
			declarationContainerA.CO_ContainerNumber = "BBB";

			var declarationContainerB1 = declarationWithMultipleContainers.CusContainers.AddNew();
			var declarationContainerB2 = declarationWithMultipleContainers.CusContainers.AddNew();
			declarationContainerB1.CO_ContainerNumber = "XXX";
			declarationContainerB2.CO_ContainerNumber = "YYY";
			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);
			var viewFromDeclarationWithMultipleContainers = Factory.Load<USConsignmentCombined>(declarationWithMultipleContainers.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment EquipmentNumber:", consignment.ULB_EquipmentNumber, viewFromConsignment.EquipmentNumber);
				AssertEquals("Declaration EquipmentNumber:", declarationContainerA.CO_ContainerNumber, viewFromDeclaration.EquipmentNumber);
				AssertEquals("Declaration EquipmentNumber - With multiple containers:", "MULTIPLE", viewFromDeclarationWithMultipleContainers.EquipmentNumber);
			});
		}

		public void TestGoodsValue()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			var item = consignment.CusUSLVItems.AddNew();
			item.ULI_RX_NKCurrency = CurrencyCodes.UnitedStates;
			item.ULI_GoodsValue = 3;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.US_98GoodsValue = 7;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment GoodsValue:", consignment.ULB_GoodsValue, viewFromConsignment.GoodsValue);
				AssertEquals("Declaration GoodsValue:", declaration.CustomsValue, viewFromDeclaration.GoodsValue);
			});
		}

		public void TestCurrency()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment Currency:", consignment.ULB_Currency, viewFromConsignment.Currency);
				AssertEquals("Declaration Currency:", CurrencyCodes.UnitedStates, viewFromDeclaration.Currency);
			});
		}

		public void TestNumberOfPacks()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.ULB_NumberOfPacks = 3;
			declaration.JE_TotalNoOfPacks = 7;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment NumberOfPacks:", consignment.ULB_NumberOfPacks, viewFromConsignment.NumberOfPacks);
				AssertEquals("Declaration NumberOfPacks:", declaration.JE_TotalNoOfPacks, viewFromDeclaration.NumberOfPacks);
			});
		}

		public void TestPackType()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.ULB_PackType = PkgUnit.Drum;
			declaration.JE_TotalNoOfPacksPackType = PkgUnit.Cylinder;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment PackType:", consignment.ULB_PackType, viewFromConsignment.PackType);
				AssertEquals("Declaration PackType:", declaration.JE_TotalNoOfPacksPackType, viewFromDeclaration.PackType);
			});
		}

		public void TestNonAMSIndicator()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.ULB_NonAMSIndicator = true;
			declaration.US_NonAMS = true;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment NonAMSIndicator:", consignment.ULB_NonAMSIndicator, viewFromConsignment.NonAMSIndicator);
				AssertEquals("Declaration NonAMSIndicator:", declaration.US_NonAMS, viewFromDeclaration.NonAMSIndicator);
			});
		}

		public void TestConsigneeAddress()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			var consigneeForConsignment = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeAddressForConsignment = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddressForConsignment.OA_Code = "ConsAddr";
			consigneeAddressForConsignment.OA_OH = consigneeForConsignment.PK;

			var consigneeForDeclaration = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeAddressForDeclaration = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddressForDeclaration.OA_Code = "DecAddr";
			consigneeAddressForDeclaration.OA_OH = consigneeForDeclaration.PK;

			consignment.ULB_OA_Consignee = consigneeAddressForConsignment.PK;
			declaration.JE_OA_ConsigneeAddress = consigneeAddressForDeclaration.PK;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment ConsigneeAddress:", consignment.ULB_OA_Consignee, viewFromConsignment.ConsigneeAddress);
				AssertEquals("Declaration ConsigneeAddress:", declaration.JE_OA_ConsigneeAddress, viewFromDeclaration.ConsigneeAddress);
			});
		}

		public void TestSellerAddress()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			var sellerForConsignment = Factory.NewWithValidTestData<OrgHeader>();
			var sellerAddressForConsignment = Factory.NewWithValidTestData<OrgAddress>();
			sellerAddressForConsignment.OA_Code = "ConsAddr";
			sellerAddressForConsignment.OA_OH = sellerForConsignment.PK;

			var sellerForDeclaration = Factory.NewWithValidTestData<OrgHeader>();
			var sellerAddressForDeclaration = Factory.NewWithValidTestData<OrgAddress>();
			sellerAddressForDeclaration.OA_Code = "DecAddr";
			sellerAddressForDeclaration.OA_OH = sellerForDeclaration.PK;

			consignment.ULB_OA_Seller = sellerAddressForConsignment.PK;
			declaration.JE_OA_SellerAddress = sellerAddressForDeclaration.PK;

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment SellerAddress:", consignment.ULB_OA_Seller, viewFromConsignment.SellerAddress);
				AssertEquals("Declaration SellerAddress:", declaration.JE_OA_SellerAddress, viewFromDeclaration.SellerAddress);
			});
		}

		public void TestRailReferencceNumber()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			consignment.CE_RailReferenceNumber = "AAA";
			var declarationReferenceNumber = declaration.AdditionalReferenceNumbers.AddNew();
			declarationReferenceNumber.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.RRN;
			declarationReferenceNumber.CE_EntryNum = "BBB";

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Consignment RailReferenceNumber:", consignment.CE_RailReferenceNumber, viewFromConsignment.RailReferenceNumber);
				AssertEquals("Declaration RailReferenceNumber:", declarationReferenceNumber.CE_EntryNum, viewFromDeclaration.RailReferenceNumber);
			});
		}

		#endregion

		#region Helper Properties

		public void TestIsConsignment()
		{
			var consignment = CreateValidConsignment();
			var declaration = CreateValidDeclaration();

			Factory.Save();

			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);
			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);

			CombineAssertions(() =>
			{
				AssertEquals("View created from consignment:", true, viewFromConsignment.IsConsignment);
				AssertEquals("View created from declarartion:", false, viewFromDeclaration.IsConsignment);
			});
		}

		public void TestIsDeclaration()
		{
			var declaration = CreateValidDeclaration();
			var consignment = CreateValidConsignment();

			Factory.Save();

			var viewFromDeclaration = Factory.Load<USConsignmentCombined>(declaration.PK);
			var viewFromConsignment = Factory.Load<USConsignmentCombined>(consignment.PK);

			CombineAssertions(() =>
			{
				AssertEquals("View created from declarartion:", true, viewFromDeclaration.IsDeclaration);
				AssertEquals("View created from consignment:", false, viewFromConsignment.IsDeclaration);
			});
		}

		#endregion

		#region BizO Test Overrides for View

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert("SetDefaultValues is never called - this BizObj is based on a view.", true);
		}

		public override void TestBizObjectFields()
		{
			Assert("This BizObj is based on a view, these BusinessObject fields are always readonly and will never be saved.", true);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("You can't save this and you can't delete it - its based on a view.", true);
		}

		[DeveloperOnlyTest]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Cannot save the factory for a view.", true);
		}

		#endregion

		#region Implementation

		CusUSLVConsignment CreateValidConsignment()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			return consignment;
		}

		JobDeclaration CreateValidDeclaration()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.US_EntryType = EntryTypeList.Codes.LowValue;
			jobDeclaration.JE_MessageType = USJobMessageTypeList.Codes.Import;

			return jobDeclaration;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var consignment = CreateValidConsignment();
			Factory.Save();
			return Factory.Load<USConsignmentCombined>(consignment.PK);
		}

		#endregion
	}
}
