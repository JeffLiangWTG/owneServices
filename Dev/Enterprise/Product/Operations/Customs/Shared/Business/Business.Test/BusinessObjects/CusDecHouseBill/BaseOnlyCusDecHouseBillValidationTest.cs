using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseOnlyCusDecHouseBillValidationTest : TestCaseWithFactory
	{
		public void TestDuplicateBillNumberTypeParent()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = mockDeclaration.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "MBL";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "HBL";
			bill2.CU_CU_ParentBill = bill1.PK;

			var bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.SubHouseBill;
			bill3.CU_BillNum = "SHBL1";
			bill3.CU_CU_ParentBill = bill2.PK;

			var bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = BillTypeList.Codes.SubHouseBill;
			bill4.CU_CU_ParentBill = bill2.PK;
			bill4.CU_BillNum = "ShbL1";
			AssertHasRowError(bill4, CusDecHouseBillValidation.DuplicateBillNumberTypeParent);

			bill4.CU_BillNum = "SHBL2";
			AssertNoRowError(bill4, CusDecHouseBillValidation.DuplicateBillNumberTypeParent);

			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(false);
			bill4.CU_BillNum = "SHBL1";
			AssertNoRowError(bill4, CusDecHouseBillValidation.DuplicateBillNumberTypeParent);
		}

		public void TestMasterBillValidation()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_MasterBill = "081-11111112";
			Assert("Bad Masterbill CheckDigit did not show error", declaration.JE_MasterBillInfo.HasWarnings());

			declaration.JE_MasterBill = "081-11111111";
			Assert("Good Masterbill CheckDigit should not show error", !declaration.JE_MasterBillInfo.HasWarnings());
		}

		public void TestMasterBillPrefixMatchesAirline()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "";
			declaration.JE_MasterBill = "08298739457";
			declaration.PrimaryMasterBill.Validation.ValidateCU_BillNum();
			AssertNoWarning(declaration.PrimaryMasterBill.CU_BillNumInfo, BillValidator.AirlinePrefixValidationMessage);

			declaration.JE_VoyageFlightNo = "HW652";
			declaration.PrimaryMasterBill.Validation.ValidateCU_BillNum();
			AssertNoWarning(declaration.PrimaryMasterBill.CU_BillNumInfo, BillValidator.AirlinePrefixValidationMessage);

			declaration.JE_VoyageFlightNo = "QF112";
			declaration.PrimaryMasterBill.Validation.ValidateCU_BillNum();
			AssertHasWarning(declaration.PrimaryMasterBill.CU_BillNumInfo, BillValidator.AirlinePrefixValidationMessage);

			declaration.JE_MasterBill = "08149837497";
			declaration.PrimaryMasterBill.Validation.ValidateCU_BillNum();
			AssertNoWarning(declaration.PrimaryMasterBill.CU_BillNumInfo, BillValidator.AirlinePrefixValidationMessage);

			declaration.JE_MasterBill = "5555555LOL5";
			declaration.PrimaryMasterBill.Validation.ValidateCU_BillNum();
			AssertHasWarning(declaration.PrimaryMasterBill.CU_BillNumInfo, BillValidator.AirlinePrefixValidationMessage);
		}

		public void TestHouseBillNumberIsMandatoryWithCustomsNeedToKnowPackDetails()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);

			BaseJobDeclaration declaration = mockDeclaration.Object;

			var mockHouseBill = Factory.NewMoq<Bill>();
			Bill houseBill = mockHouseBill.Object;
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_JE = declaration.PK;

			mockDeclaration.Protected().Setup<bool>("IsHouseBillMandatory").Returns(true);
			houseBill.CU_BillNum = "";
			AssertEquals("Housebill mandatory", true, houseBill.CU_BillNumInfo.HasMessageErrors());
			houseBill.CU_BillNum = "123455";
			AssertEquals("Now no message error", false, houseBill.CU_BillNumInfo.HasMessageErrors());

			mockDeclaration.Protected().Setup<bool>("IsHouseBillMandatory").Returns(false);
			houseBill.CU_BillNum = "";
			AssertEquals("Housebill not mandatory", false, houseBill.CU_BillNumInfo.HasMessageErrors());
		}

		public void TestHouseBillPackingLinesValidation()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			BaseJobDeclaration declaration = mockDeclaration.Object;

			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "12345";
			AssertEquals("HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill)", true, houseBill.CU_BillNumInfo.HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill));

			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			houseBill.Validation.ValidateCU_BillNum();
			AssertEquals("HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill)", true, houseBill.CU_BillNumInfo.HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill));

			packingGroup.Packages.AddNew();
			AssertEquals("HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill)", false, houseBill.CU_BillNumInfo.HasMessageError(CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill));
		}

		public void TestCU_ParentBillUniqueCodeIsMandatoryWhenMasterBillMandatory()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			var mockValidation = new Mock<BaseJobDeclarationValidation>(mockDeclaration.Object);
			mockValidation.Setup(m => m.IsMasterBillMandatory).Returns(true);
			mockDeclaration.Protected().Setup<JobDeclarationValidation>("GetNewValidation").Returns(mockValidation.Object);

			Bill bill = mockDeclaration.Object.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_ParentBillUniqueCode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.CU_ParentBillUniqueCodeInfo, MandatoryValidation.YouHaveNotEntered);

			Bill masterBill = mockDeclaration.Object.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "1";
			bill.CU_ParentBillUniqueCode = masterBill.CU_BillUniqueCode;
			AssertNoMessageErrorContaining(bill.CU_ParentBillUniqueCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestShouldAllowMultiMaster()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;

			var mockBill = Factory.NewMoq<Bill>();
			mockBill.Object.CU_JE = declaration.PK;

			var mockValidation = new Mock<CusDecHouseBillValidation>(mockBill.Object) { CallBase = true };
			mockValidation.Protected().Setup<bool>("ShouldAllowMultiMaster").Returns(false);
			mockBill.Protected().Setup<CusDecHouseBillValidation>("GetNewValidation").Returns(mockValidation.Object);
			mockBill.Object.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertHasMessageError(mockBill.Object.CU_BillTypeInfo, CusDecHouseBillValidation.MultiMastersNotAllowed);

			mockBill.Object.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertNoMessageError(mockBill.Object.CU_BillTypeInfo, CusDecHouseBillValidation.MultiMastersNotAllowed);
		}

		public void TestCheckCU_PackType()
		{
			Bill bill = Factory.New<Bill>();
			bill.CU_PackType = "~";
			string message = "Please enter a valid Manifest UQ code. The code you have selected is not in the Manifest UQ codes List.";
			AssertHasMessageError(bill.CU_PackTypeInfo, message);

			bill.CU_PackType = "KG";
			AssertNoMessageError(bill.CU_PackTypeInfo, message);
		}

		#region Test duplicate declaration warnings

		public void TestWarningOnHouseBillForDuplicate()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "Master Bill 2";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_MasterBill = "Master Bill 2";
			houseBill2.CU_HouseBill = "House Bill 2";
			Factory.Save();

			var declaration2 = BaseJobDeclaration.New(Factory);
			declaration2.DisableDefaultPackingInformation = true;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			var dec2MasterBill2 = declaration2.Bills.AddNew();
			dec2MasterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			dec2MasterBill2.CU_MasterBill = "Master Bill 2";

			var dec2HouseBill2 = declaration2.Bills.AddNew();
			dec2HouseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			dec2HouseBill2.CU_MasterBill = "Master Bill 2";
			dec2HouseBill2.CU_HouseBill = "House Bill 2";
			var msgText = BillValidator.AlreadyContainsMasterAndHouseBills(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName);
			AssertHasWarning("Has warning for duplicate", dec2HouseBill2.CU_BillNumInfo, msgText);

			declaration2.JE_GB = ZGuid.Empty;
			dec2HouseBill2.Validation.ValidateCU_BillNum();
			AssertNoWarning("No warning for duplicate", dec2HouseBill2.CU_BillNumInfo, msgText);
		}

		public void TestWarningOnDuplicatedMasterBill()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill1.CU_MasterBill = "08111111110";

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_MasterBill = "08111111121";

			var packingGroup1 = masterBill1.PackingGroups.AddNew();
			var packingGroup2 = masterBill2.PackingGroups.AddNew();
			Factory.Save();

			var declaration2 = BaseJobDeclaration.New(Factory);
			declaration2.DisableDefaultPackingInformation = true;
			declaration2.JE_TransportMode = declaration2.TransportModeAirCodeForTesting;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			var masterBill3 = declaration2.Bills.AddNew();
			masterBill3.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill3.CU_MasterBill = "08111111121";

			var packingGroup3 = masterBill3.PackingGroups.AddNew();

			masterBill3.Validation.ValidateAll();
			var msgText = BillValidator.AlreadyContainsMasterBill(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName);
			AssertHasWarning("Has warning for duplicate master", masterBill3.CU_BillNumInfo, msgText);

			masterBill3.CU_MasterBill = "08111111110";
			masterBill3.Validation.ValidateAll();
			AssertHasWarning("Has warning for duplicate master", masterBill3.CU_BillNumInfo, msgText);

			masterBill3.CU_MasterBill = "08111111133";
			masterBill3.Validation.ValidateAll();
			AssertNoWarnings("Has no warnings", masterBill3.CU_BillNumInfo);
		}

		public void TestNoWarningOnDuplicatedMasterForConsolDeclarations()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill1.CU_MasterBill = "08111111110";
			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_MasterBill = "08111111110";
			houseBill1.CU_HouseBill = "House Bill 1";

			var packingGroup1 = houseBill1.PackingGroups.AddNew();
			Factory.Save();

			var declaration2 = BaseJobDeclaration.New(Factory);
			declaration2.DisableDefaultPackingInformation = true;
			declaration2.JE_TransportMode = declaration2.TransportModeAirCodeForTesting;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			var masterBill3 = declaration2.Bills.AddNew();
			masterBill3.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill3.CU_MasterBill = "08111111110";
			var houseBill3 = declaration2.Bills.AddNew();
			houseBill3.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill3.CU_MasterBill = "08111111110";
			houseBill3.CU_HouseBill = "House Bill 2";

			var packingGroup3 = houseBill3.PackingGroups.AddNew();
			houseBill3.Validation.ValidateAll();
			AssertNoWarnings("Has no warnings", houseBill3.CU_BillNumInfo);

			houseBill3.CU_HouseBill = "House Bill 1";
			houseBill3.Validation.ValidateAll();
			var msgText = BillValidator.AlreadyContainsMasterAndHouseBills(declaration.JE_DeclarationReference, declaration.Company.GC_Name, declaration.Branch.GB_BranchName);
			AssertHasWarning("Has warning for duplicate", houseBill3.CU_BillNumInfo, msgText);
		}

		#endregion
	}
}
