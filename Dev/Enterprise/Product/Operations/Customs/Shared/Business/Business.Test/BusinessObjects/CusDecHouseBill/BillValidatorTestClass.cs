using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	[CountrySpecificTest("AU")]
	public class BillValidatorTestClass : TestCaseWithDummy
	{
		public void TestCheckMandatory()
		{
			Bill.Bill = "";

			Bill.CheckMandatory(false);
			AssertEquals(false, Bill.BillInfo.HasMessageErrors());

			Bill.CheckMandatory(true);
			AssertEquals(true, Bill.BillInfo.HasMessageErrors());

			Bill.Bill = "TEST";

			Bill.CheckMandatory(false);
			AssertEquals(false, Bill.BillInfo.HasMessageErrors());

			Bill.CheckMandatory(true);
			AssertEquals(false, Bill.BillInfo.HasMessageErrors());
		}

		public void TestCheckForDuplicateDeclaration()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var masterBill1 = declaration1.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill1.CU_MasterBill = MasterNum_1_ForTestCheckForDuplicateDeclaration;

			var houseBill1 = masterBill1.ChildBills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = HouseNum_1_ForTestCheckForDuplicateDeclaration;

			var declaration2 = Factory.New<BaseJobDeclaration>();
			var masterBill2 = declaration2.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_MasterBill = MasterNum_1_ForTestCheckForDuplicateDeclaration;

			var houseBill2 = masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = HouseNum_1_ForTestCheckForDuplicateDeclaration;

			var declaration3 = Factory.New<BaseJobDeclaration>();
			var masterBill3 = declaration3.Bills.AddNew();
			masterBill3.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill3.CU_MasterBill = MasterNum_1_ForTestCheckForDuplicateDeclaration;

			var houseBill3 = masterBill3.ChildBills.AddNew();
			houseBill3.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill3.CU_HouseBill = HouseNum_1_ForTestCheckForDuplicateDeclaration;

			declaration1.JE_SystemCreateTimeUtc = ZDateTime.Today;
			declaration2.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);

			Factory.Save();

			Bill.CheckDuplicate(Bill.BillInfo, declaration3, houseBill3.CU_HouseBill, masterBill3.CU_MasterBill);
			var alreadyContainsTheSameMasterAndHouseBillFormat1 = BillValidator.AlreadyContainsMasterAndHouseBills(declaration1.JE_DeclarationReference, declaration1.Company.GC_Name, declaration1.Branch.GB_BranchName);
			AssertNoWarning(Bill.BillInfo, alreadyContainsTheSameMasterAndHouseBillFormat1);
			var alreadyContainsTheSameMasterAndHouseBillFormat2 = BillValidator.AlreadyContainsMasterAndHouseBills(declaration2.JE_DeclarationReference, declaration2.Company.GC_Name, declaration2.Branch.GB_BranchName);
			AssertHasWarning(Bill.BillInfo, alreadyContainsTheSameMasterAndHouseBillFormat2);

			declaration1.JE_SystemCreateTimeUtc = ZDateTime.Today;
			declaration2.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

			Factory.Save();

			Bill.CheckDuplicate(Bill.BillInfo, declaration3, houseBill3.CU_HouseBill, masterBill3.CU_MasterBill);
			AssertHasWarning(Bill.BillInfo, alreadyContainsTheSameMasterAndHouseBillFormat1);
			AssertNoWarning(Bill.BillInfo, alreadyContainsTheSameMasterAndHouseBillFormat2);

			houseBill2.CU_HouseBill = HouseNum_2_ForTestCheckForDuplicateDeclaration;
			houseBill3.CU_HouseBill = HouseNum_3_ForTestCheckForDuplicateDeclaration;
			Bill.CheckDuplicate(Bill.BillInfo, declaration3, houseBill3.CU_HouseBill, masterBill3.CU_MasterBill);
			AssertNoWarning(Bill.BillInfo, alreadyContainsTheSameMasterAndHouseBillFormat1);
			AssertNoWarning(Bill.BillInfo, alreadyContainsTheSameMasterAndHouseBillFormat2);

			houseBill2.CU_HouseBill = HouseNum_1_ForTestCheckForDuplicateDeclaration;
			houseBill3.CU_HouseBill = HouseNum_1_ForTestCheckForDuplicateDeclaration;
			declaration3.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			Factory.Save();

			Bill.CheckDuplicate(Bill.BillInfo, declaration3, houseBill3.CU_HouseBill, masterBill3.CU_MasterBill);
			AssertNoWarnings("ExWarehouse declaration should has no warnings", Bill.BillInfo);

			#region Asserts for HouseBill without MasterBill

			declaration1.Bills.RemoveAndDeleteAll();
			var houseBillWithoutParent1 = declaration1.Bills.AddNew();
			houseBillWithoutParent1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBillWithoutParent1.CU_HouseBill = HouseNum_1_ForTestCheckForDuplicateDeclaration;

			declaration2.Bills.RemoveAndDeleteAll();
			var houseBillWithoutParent2 = declaration2.Bills.AddNew();
			houseBillWithoutParent2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBillWithoutParent2.CU_HouseBill = HouseNum_1_ForTestCheckForDuplicateDeclaration;

			Factory.Save();

			var alreadyContainsTheSameHouseBillFormat = BillValidator.AlreadyContainsHouseBill(declaration1.JE_DeclarationReference, declaration1.Company.GC_Name, declaration1.Branch.GB_BranchName);
			Bill.CheckDuplicate(Bill.BillInfo, declaration2, houseBillWithoutParent2.CU_HouseBill, ZString.Empty);
			AssertHasWarning(Bill.BillInfo, alreadyContainsTheSameHouseBillFormat);

			houseBillWithoutParent2.CU_HouseBill = HouseNum_2_ForTestCheckForDuplicateDeclaration;
			Bill.CheckDuplicate(Bill.BillInfo, declaration2, houseBillWithoutParent2.CU_HouseBill, ZString.Empty);
			AssertNoWarning(Bill.BillInfo, alreadyContainsTheSameHouseBillFormat);

			#endregion
		}
		const string MasterNum_1_ForTestCheckForDuplicateDeclaration = "08111111110";
		const string HouseNum_1_ForTestCheckForDuplicateDeclaration = "1000";
		const string HouseNum_2_ForTestCheckForDuplicateDeclaration = "2000";
		const string HouseNum_3_ForTestCheckForDuplicateDeclaration = "3000";

		public void TestMasterBillPrefixMatchesAirline()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "";
			declaration.JE_MasterBill = "08298739457";
			Bill.CheckAirWayBill(declaration, declaration.JE_MasterBillInfo);
			AssertNoWarning(Bill.BillInfo, BillValidator.AirlinePrefixValidationMessage);
			declaration.JE_VoyageFlightNo = "HW652";
			Bill.CheckAirWayBill(declaration, declaration.JE_MasterBillInfo);
			AssertNoWarning(Bill.BillInfo, BillValidator.AirlinePrefixValidationMessage);
			declaration.JE_VoyageFlightNo = "QF112";
			Bill.CheckAirWayBill(declaration, declaration.JE_MasterBillInfo);
			AssertHasWarning(Bill.BillInfo, BillValidator.AirlinePrefixValidationMessage);
			declaration.JE_MasterBill = "08149837497";
			Bill.CheckAirWayBill(declaration, declaration.JE_MasterBillInfo);
			AssertNoWarning(Bill.BillInfo, BillValidator.AirlinePrefixValidationMessage);
			declaration.JE_MasterBill = "5555555LOL5";
			Bill.CheckAirWayBill(declaration, declaration.JE_MasterBillInfo);
			AssertHasWarning(Bill.BillInfo, BillValidator.AirlinePrefixValidationMessage);
		}

		public void TestWarningOnDuplicatedMasterBill()
		{
			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			GlbCompany.CurrentCompany.Factory.Save();

			var dec1 = BaseJobDeclaration.New(Factory);
			dec1.DisableDefaultPackingInformation = true;
			dec1.JE_TransportMode = dec1.TransportModeAirCodeForTesting;
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;

			var dec1MasterBill1 = dec1.Bills.AddNew();
			dec1MasterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			dec1MasterBill1.CU_MasterBill = MasterNum_1_ForTestWarningOnDuplicatedMasterBill;

			var dec1MasterBill2 = dec1.Bills.AddNew();
			dec1MasterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			dec1MasterBill2.CU_MasterBill = MasterNum_2_ForTestWarningOnDuplicatedMasterBill;

			var dec1MasterBill1PackingGroup1 = dec1MasterBill1.PackingGroups.AddNew();
			var dec1MasterBill2PackingGroup2 = dec1MasterBill2.PackingGroups.AddNew();

			var dec2 = BaseJobDeclaration.New(Factory);
			dec2.DisableDefaultPackingInformation = true;
			dec2.JE_TransportMode = dec2.TransportModeAirCodeForTesting;
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;

			var dec2MasterBill1 = dec2.Bills.AddNew();
			dec2MasterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			dec2MasterBill1.CU_MasterBill = MasterNum_2_ForTestWarningOnDuplicatedMasterBill;

			var dec3 = BaseJobDeclaration.New(Factory);
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;

			var dec3MasterBill1 = dec3.Bills.AddNew();
			dec3MasterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			dec3MasterBill1.CU_MasterBill = MasterNum_2_ForTestWarningOnDuplicatedMasterBill;

			var dec4 = BaseJobDeclaration.New(Factory);
			dec4.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var dec4MasterBill1 = dec4.Bills.AddNew();
			dec4MasterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			dec4MasterBill1.CU_MasterBill = MasterNum_2_ForTestWarningOnDuplicatedMasterBill;
			Factory.Save();

			dec1.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			dec2.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			dec3.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			dec4.JE_SystemCreateTimeUtc = ZDateTime.Today;

			Factory.Save();

			var dec2MasterBill1PackingGroup3 = dec2MasterBill1.PackingGroups.AddNew();

			Bill.CheckDeclarationWithSameDirectMasterBill(dec2, dec2MasterBill1.CU_BillNumInfo);
			AssertHasWarning("Has warning for duplicate master. Warning should show number of most recent declaration", Bill.BillInfo, BillValidator.AlreadyContainsMasterBill(dec3.JE_DeclarationReference, dec3.Company.GC_Name, dec3.Branch.GB_BranchName));

			dec2MasterBill1.CU_MasterBill = MasterNum_1_ForTestWarningOnDuplicatedMasterBill;
			Bill.CheckDeclarationWithSameDirectMasterBill(dec2, dec2MasterBill1.CU_BillNumInfo);
			AssertHasWarning("Has warning for duplicate master", Bill.BillInfo, BillValidator.AlreadyContainsMasterBill(dec1.JE_DeclarationReference, dec1.Company.GC_Name, dec1.Branch.GB_BranchName));

			dec2MasterBill1.CU_MasterBill = MasterNum_3_ForTestWarningOnDuplicatedMasterBill;
			Bill.CheckDeclarationWithSameDirectMasterBill(dec2, dec2MasterBill1.CU_BillNumInfo);
			AssertNoWarnings("Has no warnings", Bill.BillInfo);

			var masterBill5 = dec4.Bills.AddNew();
			masterBill5.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill5.CU_MasterBill = MasterNum_2_ForTestWarningOnDuplicatedMasterBill;
			Bill.CheckDeclarationWithSameDirectMasterBill(dec4, masterBill5.CU_BillNumInfo);
			AssertNoWarnings("ExWarehouse declaration should has no warnings", Bill.BillInfo);
		}
		const string MasterNum_1_ForTestWarningOnDuplicatedMasterBill = "08111111110";
		const string MasterNum_2_ForTestWarningOnDuplicatedMasterBill = "08111111121";
		const string MasterNum_3_ForTestWarningOnDuplicatedMasterBill = "08111111133";

		public void TestCheckForDuplicateMasterAndHouseBills()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_DeclarationReference = "B00005120";
			declaration1.IsCancelled = true;
			var masterBill1 = declaration1.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill1.CU_MasterBill = "MASTERBILL1";

			var houseBill1 = masterBill1.ChildBills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "HOUSEBILL1";

			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.GC_Code = "U!@";
			anotherCompany.GC_Name = "ANOTHER COMPANY";
			anotherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			anotherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var anotherBranch = anotherCompany.Branches.AddNew();
			anotherBranch.GB_Code = "U!@";
			anotherBranch.GB_BranchName = "ANOTHER BRANCH";
			anotherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_GB = anotherBranch.PK;
			declaration2.JE_DeclarationReference = "B00005122";
			var masterBill2 = declaration2.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_MasterBill = "MASTERBILL1";

			var houseBill2 = masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "HOUSEBILL1";
			Factory.Save();

			var alreadyContainsTheSameMasterAndHouseBillFormat = BillValidator.AlreadyContainsMasterAndHouseBills(declaration1.JE_DeclarationReference, declaration1.Company.GC_Name, declaration1.Branch.GB_BranchName);
			Bill.CheckDuplicate(Bill.BillInfo, declaration2, houseBill2.CU_HouseBill, masterBill2.CU_MasterBill);
			AssertNoWarning(Bill.BillInfo, alreadyContainsTheSameMasterAndHouseBillFormat);

			declaration1.IsCancelled = false;
			Factory.Save();

			Bill.CheckDuplicate(Bill.BillInfo, declaration2, houseBill2.CU_HouseBill, masterBill2.CU_MasterBill);
			AssertHasWarning(Bill.BillInfo, alreadyContainsTheSameMasterAndHouseBillFormat);
		}

		#region Implementation

		BillBusinessObject Bill
		{
			get { return bill ?? (bill = new BillBusinessObject()); }
		}
		BillBusinessObject bill;

		class BillBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
		{
			#region Bill

			public ZString Bill
			{
				get { return bill; }
				set { SetNonPersistentPropertyValue(BillInfo, ref bill, value); }
			}
			ZString bill;

			public ZPropertyInfo BillInfo
			{
				get { return GetZPropertyInfo(nameof(Bill)); }
			}

			public void CheckMandatory(bool isMandatory)
			{
				BillInfo.ClearAllNotifications();
				new BillValidator().CheckMandatory(isMandatory, BillInfo, "");
			}

			public void CheckDuplicate(ZPropertyInfo billInfo, BaseJobDeclaration declaration, ZString houseBill, ZString masterBill)
			{
				BillInfo.ClearAllNotifications();
				new BillValidator().CheckDuplicateDeclaration(billInfo, declaration, houseBill, masterBill);
			}

			public void CheckAirWayBill(BaseJobDeclaration declaration, ZPropertyInfo masterBillInfo)
			{
				BillInfo.ClearAllNotifications();
				Bill = (ZString)masterBillInfo.Value;
				new BillValidator().CheckAirWayBill(declaration, BillInfo, CargoWise.EntityFramework.NotificationType.Warning);
			}

			public void CheckDeclarationWithSameDirectMasterBill(BaseJobDeclaration declaration, ZPropertyInfo masterBillInfo)
			{
				BillInfo.ClearAllNotifications();
				Bill = (ZString)masterBillInfo.Value;
				new BillValidator().CheckDeclarationWithSameDirectMasterBill(declaration, BillInfo);
			}

			#endregion

		}

		#endregion
	}
}
