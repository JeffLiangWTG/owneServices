using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using ValidationConstant = Enterprise.Customs.US.Business.ValidationConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusDecHouseBillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestFlags()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.ValidationModes = ValidationModes.None;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Bill bill = declaration.Bills.AddNew();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;
			AssertEquals(true, bill.Validation.IsEntrySummaryValidationMode);
			AssertEquals(false, bill.Validation.IsCargoReleaseValidationMode);
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			AssertEquals(false, bill.Validation.IsEntrySummaryValidationMode);
			AssertEquals(false, bill.Validation.IsCargoReleaseValidationMode);
			declaration.US_EnableINB = false;
			declaration.US_EnableCRL = true;
			AssertEquals(false, bill.Validation.IsEntrySummaryValidationMode);
			AssertEquals(true, bill.Validation.IsCargoReleaseValidationMode);
		}

		public void TestIsManifestQuantityMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = "";
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			declaration.US_EnableENS = false;
			AssertEquals(false, bill.Validation.IsManifestQuantityMandatory);
			declaration.US_EnableENS = true;
			var childBill = bill.ChildBills.AddNew();
			childBill.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals(false, bill.Validation.IsManifestQuantityMandatory);
			AssertEquals(true, childBill.Validation.IsManifestQuantityMandatory);
			declaration.US_EnableINB = true;
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = false;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			AssertEquals(false, bill.Validation.IsManifestQuantityMandatory);
			AssertEquals(false, childBill.Validation.IsManifestQuantityMandatory);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals(false, bill.Validation.IsManifestQuantityMandatory);
			AssertEquals(false, childBill.Validation.IsManifestQuantityMandatory);
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			AssertEquals(false, bill.Validation.IsManifestQuantityMandatory);
			AssertEquals(true, childBill.Validation.IsManifestQuantityMandatory);
			declaration.US_EntryType = ZString.Empty;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals(true, bill.Validation.IsManifestQuantityMandatory);
			AssertEquals(false, childBill.Validation.IsManifestQuantityMandatory);
		}

		public void TestDuplicateOnlyFiredWhenSubHouseTheSame()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "MBL";
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "HBL";
			bill2.CU_CU_ParentBill = bill1.PK;
			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			bill3.CU_BillNum = "SHBL1";
			bill3.CU_CU_ParentBill = bill2.PK;
			Bill bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			bill4.CU_CU_ParentBill = bill2.PK;
			bill4.CU_BillNum = "SHBL1";
			AssertHasRowError(bill4, Customs.Business.CusDecHouseBillValidation.DuplicateBillNumberTypeParent);
			bill4.CU_BillNum = "SHBL2";
			AssertNoRowError(bill4, Customs.Business.CusDecHouseBillValidation.DuplicateBillNumberTypeParent);
		}

		public void TestCheckUS_BillNumForInvoicesLinked()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "";
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			AssertEquals("PreCondition:IsInBondOnly", true, declaration.IsInBondOnly);
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
			Bill houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "1";
			AssertNoMessageErrors(houseBill.CU_BillNumInfo); //House Bills are not validated for inbond only jobs
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals("PreCondition:IsInBondOnly", false, declaration.IsInBondOnly);
			masterBill.CU_BillNum = "1";
			AssertNoMessageErrors(masterBill.CU_BillNumInfo); //master bill is not lowest any more;
		}

		public void TestDuplicateMasterBill()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			AssertEquals("InBond", true, declaration.IsInBond);
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "1";
			AssertNoRowError(bill, Customs.Business.CusDecHouseBillValidation.DuplicateBillNumberTypeParent);
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "1";
			AssertHasRowError(bill2, Customs.Business.CusDecHouseBillValidation.DuplicateBillNumberTypeParent);
			bill2.CU_BillNum = "2";
			AssertNoRowError(bill2, Customs.Business.CusDecHouseBillValidation.DuplicateBillNumberTypeParent);
		}

		public void TestCheckInvoiceLinesForBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.TemporaryDeposit;
			var bill = declaration.Bills.AddNew();
			bill.US_UI_NKBillIssuerSCAC = "AAAD";
			bill.CU_BillNum = "Bill1";
			AssertNoMessageError(bill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			bill.Validation.ValidateCU_BillNum();
			AssertHasMessageError(bill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			var houseBill = bill.ChildBills.AddNew();
			houseBill.Validation.ValidateCU_BillNum();
			AssertHasMessageError(houseBill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			var houseBill2 = bill.ChildBills.AddNew();
			houseBill2.Validation.ValidateCU_BillNum();
			AssertHasMessageError(houseBill2.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice.JobComInvoiceLines.AddNew();
			bill.Validation.ValidateCU_BillNum();
			AssertNoMessageError(bill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			houseBill.Validation.ValidateCU_BillNum();
			AssertNoMessageError(houseBill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice2.JobComInvoiceLines.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = houseBill2.PK;
			houseBill2.Validation.ValidateCU_BillNum();
			AssertNoMessageError(houseBill2.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			invoice.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			invoice2.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			bill.Validation.ValidateCU_BillNum();
			AssertHasMessageError(bill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			AssertNoMessageError(houseBill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			AssertNoMessageError(houseBill2.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				invoice = declaration.Invoices.AddNew();
				invoice.JobComInvoiceLines.AddNew();
				bill = declaration.Bills.AddNew();
				bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				bill.CU_BillNum = "1";
				AssertHasMessageError(bill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
				invoice.JZ_CU_RelatedHouseBill = bill.PK;
				bill.Validation.ValidateCU_BillNum();
				AssertNoMessageError(bill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);

				houseBill = bill.ChildBills.AddNew();
				houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
				houseBill.CU_BillNum = "2";
				AssertNoMessageError(bill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
				AssertHasMessageError(houseBill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
				invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
				houseBill.Validation.ValidateCU_BillNum();
				AssertNoMessageError(houseBill.CU_BillNumInfo, CusDecHouseBillValidation.LinesMissing);
			}
		}

		public void TestCheckCU_PackType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			AssertEquals("InBond", true, declaration.IsInBond);
			Bill masterbill = declaration.Bills.AddNew();
			masterbill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterbill.CU_BillNum = "1";
			masterbill.CU_PackType = "KG";
			AssertNoMessageErrors(masterbill.CU_PackTypeInfo);
			Bill houseBill = masterbill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "2";
			houseBill.CU_PackType = "KG";
			AssertNoMessageErrors(houseBill.CU_PackTypeInfo);
			masterbill.CU_PackType = "KG";
			AssertNoMessageErrors(masterbill.CU_PackTypeInfo);
		}

		public void TestHouseBill()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_HouseBillIssuerSCAC = "APLU";
			AssertNotNull(dec.PrimaryHouseBill);
			// as long as SCAC is entered, house bill is mandatory regardless of transport mode
			dec.PrimaryHouseBill.CU_BillNum = ZString.Empty;
			AssertHasMessageErrorContaining(dec.PrimaryHouseBill.CU_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(dec.JE_HouseBillInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCU_ParentBillUniqueCode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Bill subHouseBill = declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.Validation.ValidateCU_ParentBillUniqueCode();
			AssertHasMessageErrorContaining(subHouseBill.CU_ParentBillUniqueCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "2";
			subHouseBill.CU_ParentBillUniqueCode = houseBill.CU_BillUniqueCode;
			subHouseBill.Validation.ValidateCU_ParentBillUniqueCode();
			AssertNoMessageErrorContaining(subHouseBill.CU_ParentBillUniqueCodeInfo, MandatoryValidation.YouHaveNotEntered);
			houseBill.Validation.ValidateCU_ParentBillUniqueCode();
			AssertHasMessageErrorContaining(houseBill.CU_ParentBillUniqueCodeInfo, MandatoryValidation.YouHaveNotEntered);
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "1";
			houseBill.CU_ParentBillUniqueCode = masterBill.CU_BillUniqueCode;
			houseBill.Validation.ValidateCU_ParentBillUniqueCode();
			AssertNoMessageErrorContaining(houseBill.CU_ParentBillUniqueCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateManifestQtyAndUQ()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			declaration.US_EnableINB = true;
			declaration.JE_MasterBill = "OBL";
			declaration.PrimaryMasterBill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
			Bill houseBill = declaration.PrimaryMasterBill.ChildBills.AddNew();
			declaration.PrimaryMasterBill.CU_NoOfPacks = 0;
			houseBill.CU_PackType = "KG";
			declaration.PrimaryMasterBill.CU_NoOfPacks = 10;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_ManifestQty = 10;
			declaration.PrimaryMasterBill.CU_NoOfPacks = 10;
			declaration.PrimaryMasterBill.CU_PackType = "";
			AssertHasMessageErrorContaining(declaration.PrimaryMasterBill.CU_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.PrimaryMasterBill.CU_PackType = "Z!";
			AssertHasWarning(declaration.PrimaryMasterBill.CU_PackTypeInfo, ValidationConstant.InvalidPackTypeMessage);
			declaration.PrimaryMasterBill.CU_PackType = Enterprise.Customs.US.Messaging.Business.ShippingOrPackingingUnitList.Codes.Package;
			AssertNoWarning(declaration.PrimaryMasterBill.CU_PackTypeInfo, ValidationConstant.InvalidPackTypeMessage);
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			dec.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			dec.JE_TransportMode = TransportTypeList.Codes.Air;
			var masterBill = dec.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			var ftzHouseBill = masterBill.ChildBills.AddNew();
			ftzHouseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			ftzHouseBill.CU_NoOfPacks = ZDecimal.Zero;
			ftzHouseBill.Validation.ValidateCU_NoOfPacks();
			AssertHasMessageErrorContaining(ftzHouseBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			dec.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.StatusChange;
			ftzHouseBill.Validation.ValidateCU_NoOfPacks();
			AssertNoMessageErrorContaining(ftzHouseBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			var itNo = ftzHouseBill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "4578965";
			ftzHouseBill.Validation.ValidateCU_NoOfPacks();
			AssertHasMessageErrorContaining(ftzHouseBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCU_NoOfPacksForOverflow()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_MasterBill = "MASTER";
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = int.MaxValue + (1m);
			primaryMasterBill.Validation.ValidateCU_NoOfPacks();
			Assert(declaration.PrimaryMasterBill.CU_NoOfPacksInfo.HasMessageError(CusDecHouseBillValidation.NoOfPacksValueIsInvalid));
			primaryMasterBill.CU_NoOfPacks = 7777m;
			primaryMasterBill.Validation.ValidateCU_NoOfPacks();
			AssertNoMessageErrors(declaration.PrimaryMasterBill.CU_NoOfPacksInfo);
		}

		public void TestCheckCU_NoOfPacksForBCR()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mexico;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.JE_HouseBill = "HOUSETEST1";
			declaration.JE_TotalNoOfPacks = 12;
			declaration.JE_TotalNoOfPacksPackType = ABIUnitOfMeasureList.Codes.Pairs;
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.PrimaryMasterBill.Validation.ValidateCU_NoOfPacks();
			AssertNoMessageErrorContaining(declaration.PrimaryMasterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.PrimaryHouseBill.Delete();
			declaration.PrimaryMasterBill.Validation.ValidateCU_NoOfPacks();
			AssertNoMessageErrorContaining(declaration.PrimaryMasterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.PrimaryMasterBill.CU_NoOfPacks = ZDecimal.Zero;
			AssertHasMessageErrorContaining(declaration.PrimaryMasterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCU_NoOfPacksForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.ZoneToZone;
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_MasterBill = "MASTER";
			declaration.JE_HouseBill = "HOUSETEST1";
			declaration.JE_TotalNoOfPacks = 12;
			declaration.JE_TotalNoOfPacksPackType = ABIUnitOfMeasureList.Codes.Pairs;
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.Validation.ValidateCU_NoOfPacks();
			var primaryHouseBill = declaration.PrimaryHouseBill;
			primaryHouseBill.Validation.ValidateCU_NoOfPacks();
			AssertNoMessageErrorContaining(primaryMasterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(primaryHouseBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			primaryHouseBill.CU_NoOfPacks = ZDecimal.Zero;
			AssertHasMessageErrorContaining(primaryHouseBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			primaryMasterBill.CU_NoOfPacks = ZDecimal.Zero;
			AssertNoMessageErrorContaining(primaryMasterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCU_NoOfPacksForACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_TotalNoOfPacksPackType = "";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_MasterBill = "MB001TEST";
			var bill = declaration.PrimaryMasterBill;
			bill.Validation.ValidateCU_NoOfPacks();
			AssertHasMessageErrorContaining(bill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			bill.CU_NoOfPacks = 23m;
			bill.ITAndSplitDetails.AddNew();
			AssertHasMessageErrorContaining(bill.CU_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.CU_NoOfPacks = ZDecimal.Zero;
			AssertNoWarningContaining(bill.CU_NoOfPacksInfo, CusDecHouseBillValidation.ACECargoReleaseQTY);
			bill.CU_PackType = "PC";
			AssertNoMessageErrorContaining(bill.CU_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EnableENS = false;
			bill.CU_NoOfPacks = 23m;
			AssertHasWarningContaining(bill.CU_NoOfPacksInfo, CusDecHouseBillValidation.ACECargoReleaseQTY);
			declaration.US_NonAMS = true;
			bill.Validation.ValidateCU_NoOfPacks();
			AssertNoWarningContaining(bill.CU_NoOfPacksInfo, CusDecHouseBillValidation.ACECargoReleaseQTY);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			bill.CU_NoOfPacks = ZDecimal.Zero;
			AssertHasMessageErrorContaining(bill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			bill.CU_NoOfPacks = ZDecimal.Zero;
			AssertHasMessageErrorContaining(bill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.CU_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCU_NoOfPacksForLowValueEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.US_EnableCRL = true;
			declaration.JE_MasterBill = "1234214211";
			var masterBill = declaration.Bills[0];
			masterBill.Validation.ValidateCU_NoOfPacks();
			AssertHasMessageErrorContaining(masterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			masterBill.CU_NoOfPacks = 100;
			masterBill.Validation.ValidateCU_NoOfPacks();
			AssertNoMessageErrorContaining(masterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(masterBill.CU_NoOfPacksInfo, CusDecHouseBillValidation.ACECargoReleaseQTY);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = false;
			masterBill.CU_NoOfPacks = 0;
			masterBill.Validation.ValidateCU_NoOfPacks();
			AssertNoMessageErrorContaining(masterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			masterBill.CU_NoOfPacks = 100;
			AssertNoMessageErrorContaining(masterBill.CU_NoOfPacksInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(masterBill.CU_NoOfPacksInfo, CusDecHouseBillValidation.ACECargoReleaseQTY);
		}

		public void TestCU_NoOfPacksAgainstITNosPacks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			declaration.JE_MasterBill = "OBL";
			var houseBill = declaration.PrimaryMasterBill.ChildBills.AddNew();
			houseBill.CU_NoOfPacks = 18m;
			ITAndSplitDetails itNo = houseBill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "12456";
			itNo.US_NoOfPacks = 12;
			itNo = houseBill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V123456789";
			itNo.US_NoOfPacks = 10;
			AssertHasWarning(houseBill.CU_NoOfPacksInfo, string.Format(CusDecHouseBillValidation.NoOfPacksNotEqualITNumbersNoOfPacks, 22));
			houseBill.CU_NoOfPacks = 22;
			AssertNoWarning(houseBill.CU_NoOfPacksInfo, string.Format(CusDecHouseBillValidation.NoOfPacksNotEqualITNumbersNoOfPacks, 22));
		}

		public void TestValidateITNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			ValidateITNumberFormat(declaration);
		}

		public void TestCheckCU_BillType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.SubHouseBill;
			AssertHasMessageError(bill.CU_BillTypeInfo, CusDecHouseBillValidation.SubHouseIsNotAllowedForACE);
		}

		public void TestCheckCU_BillNumForExpressTracking()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = "QF710";
			declaration.JE_MasterBillExpressTracking = true;
			var bill = declaration.Bills[0];
			bill.CU_BillNum = "21787541783";
			AssertNoWarningContaining(bill.CU_BillNumInfo, BillValidator.AirlinePrefixValidationMessage);
			declaration.JE_MasterBillExpressTracking = false;
			bill.CU_BillNum = "21787541783";
			AssertHasWarningContaining(bill.CU_BillNumInfo, BillValidator.AirlinePrefixValidationMessage);
		}

		public void TestCheckBU_BillTypeForLowValueEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			var billOne = declaration.Bills.AddNew();
			billOne.CU_BillType = BillTypeList.Codes.MasterBill;
			billOne.CU_BillNum = "111111111";
			AssertNoMessageErrorContaining(billOne.CU_BillTypeInfo, string.Format(BillValidator.Constants.OnlyOneBillAllowedForLowValueEntries, BillTypeList.Descriptions.MasterBill));
			var billTwo = declaration.Bills.AddNew();
			billTwo.CU_BillType = BillTypeList.Codes.MasterBill;
			billTwo.CU_BillNum = "2222222222";
			billOne.Validation.ValidateCU_BillType();
			AssertHasMessageErrorContaining(billOne.CU_BillTypeInfo, string.Format(BillValidator.Constants.OnlyOneBillAllowedForLowValueEntries, BillTypeList.Descriptions.MasterBill));
			billTwo.CU_BillType = BillTypeList.Codes.HouseBill;
			billOne.Validation.ValidateCU_BillType();
			AssertNoMessageErrorContaining(billOne.CU_BillTypeInfo, string.Format(BillValidator.Constants.OnlyOneBillAllowedForLowValueEntries, BillTypeList.Descriptions.MasterBill));
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var result = (JobDeclaration)base.GetJobDeclaration();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.DisableDefaultPackingInformation = true;
			result.US_EnableINB = true;
			result.JE_TransportMode = TransportTypeList.Codes.Truck;
			result.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			var bill = result.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "1";
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
			return result;
		}

		void ValidateITNumberFormat(JobDeclaration declaration)
		{
			declaration.JE_MasterBill = "OBC2";
			declaration.PrimaryMasterBill.ITNumber = "123";
			AssertHasMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			declaration.PrimaryMasterBill.ITNumber = DeclarationTestHelper.ValidITNumber1ForTesting;
			AssertNoMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.PrimaryMasterBill.ITNumber = "Vblah-blah";
			AssertHasMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.PrimaryMasterBill.ITNumber = "12365";
			AssertNoMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.PrimaryMasterBill.ITNumber = "vblah-blah";
			AssertHasMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.PrimaryMasterBill.ITNumber = "V236542J144";
			AssertHasMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.PrimaryMasterBill.ITNumber = "V2365425144";
			AssertNoMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.PrimaryMasterBill.ITNumber = "v2365425144";
			AssertNoMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.PrimaryMasterBill.ITNumber = "VHH65425144";
			AssertNoMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.PrimaryMasterBill.ITNumber = "vHH65425144";
			AssertNoMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.PrimaryMasterBill.ITNumber = "VH765425144";
			AssertNoMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ValidationConstant.AllocateInBondNumber.InvalidPaperless);
			declaration.PrimaryMasterBill.ITNumber = "V7603245271";
			AssertHasMessageErrorContaining(declaration.PrimaryMasterBill.ITNumberInfo, "Invalid check digit. The last digit should be");
			declaration.PrimaryMasterBill.ITNumber = "V7603245275";
			AssertNoMessageErrorContaining(declaration.PrimaryMasterBill.ITNumberInfo, "Invalid check digit. The last digit should be");
			declaration.PrimaryMasterBill.ITNumber = "v7603245275";
			AssertNoMessageErrorContaining(declaration.PrimaryMasterBill.ITNumberInfo, "Invalid check digit. The last digit should be");
			declaration.PrimaryMasterBill.ITNumber = "123456789";
			AssertHasWarningContaining(declaration.PrimaryMasterBill.ITNumberInfo, "Invalid check digit. The last digit should be");
			//if Air then AWB number format is also correct (11n)
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.PrimaryMasterBill.ITNumber = "12345678";
			AssertHasMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			declaration.PrimaryMasterBill.ITNumber = "1111G111111";
			AssertHasMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			declaration.PrimaryMasterBill.ITNumber = "11111111111";
			AssertNoMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.PrimaryMasterBill.ITNumber = "11111111111";
			AssertHasMessageError(declaration.PrimaryMasterBill.ITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
		}
	}
}
