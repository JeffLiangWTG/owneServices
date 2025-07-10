using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public class HouseBillValidationTest : Customs.Business.Testing.CusDecHouseBillValidationTest
	{
		public void TestBillNumberPackageCountValidationGetsFiredAndResetForMasterBillWhenNewHouseBillIsAdded()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "";
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_MasterBill = "081-11111111";
			AssertHasMessageError(declaration.PrimaryMasterBill.CU_BillNumInfo, BillValidation.NoPackagesEnteredForThisHouseBill);

			declaration.JE_HouseBill = "HBL234908-5";
			AssertNoMessageError(declaration.PrimaryMasterBill.CU_BillNumInfo, BillValidation.NoPackagesEnteredForThisHouseBill);
		}

		public void TestECIWriteOffOnlyAllowsOneHouseBill()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			AssertEquals("Precondition: Declaration.IsECIWriteoff", false, declaration.IsECIWriteoff);
			Bill masterBill1 = declaration.Bills.AddNew();
			Bill houseBill1 = declaration.Bills.AddNew();
			AssertNoMessageError(masterBill1.CU_BillTypeInfo, BillValidation.ErrorOneHouseBillOnlyOnECIWriteOff);
			AssertNoMessageError(houseBill1.CU_BillTypeInfo, BillValidation.ErrorOneHouseBillOnlyOnECIWriteOff);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertNoMessageError(masterBill1.CU_BillTypeInfo, BillValidation.ErrorOneHouseBillOnlyOnECIWriteOff);
			AssertNoMessageError(houseBill1.CU_BillTypeInfo, BillValidation.ErrorOneHouseBillOnlyOnECIWriteOff);

			Bill houseBill2 = declaration.Bills.AddNew();
			AssertHasMessageError(houseBill2.CU_BillTypeInfo, BillValidation.ErrorOneHouseBillOnlyOnECIWriteOff);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertNoMessageError(masterBill1.CU_BillTypeInfo, BillValidation.ErrorOneHouseBillOnlyOnECIWriteOff);
			AssertNoMessageError(houseBill1.CU_BillTypeInfo, BillValidation.ErrorOneHouseBillOnlyOnECIWriteOff);
			AssertNoMessageError(houseBill2.CU_BillTypeInfo, BillValidation.ErrorOneHouseBillOnlyOnECIWriteOff);
		}

		public void TestMoreThanOneMasterIsMessageError()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertNoMessageError(masterBill.CU_BillTypeInfo, Customs.Business.CusDecHouseBillValidation.MultiMastersNotAllowed);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertHasMessageError(bill2.CU_BillTypeInfo, Customs.Business.CusDecHouseBillValidation.MultiMastersNotAllowed);

			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertNoMessageError(bill2.CU_BillTypeInfo, Customs.Business.CusDecHouseBillValidation.MultiMastersNotAllowed);
		}

		public void TestCU_HouseBillMessageErrorsIfEmpty()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "123";

			AssertEquals(false, bill.CU_BillNumInfo.HasMessageErrors());
			bill.CU_BillNum = "";
			AssertEquals(true, bill.IsDeleted);
		}

		public void TestCU_HouseBillDoesntMessageErrorOnPeriodicEntries()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "123";

			AssertEquals(false, bill.CU_BillNumInfo.HasMessageErrors());
			bill.CU_BillNum = "";
			AssertEquals(true, bill.IsDeleted);
		}

		public void TestCU_HouseBillMessageErrorsIfNoPackagesOnFormalEntryOnly()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "GIMMEALLYOURLOVIN";
			PackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			AssertNoMessageErrorContaining(houseBill.CU_BillNumInfo, BillValidation.NoPackagesEnteredForThisHouseBill);
			packingGroup.Packages.AddNew();
			AssertNoMessageErrorContaining(houseBill.CU_BillNumInfo, BillValidation.NoPackagesEnteredForThisHouseBill);
			packingGroup.Packages.RemoveAndDeleteAll();

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			packingGroup.Packages.AddNew();
			AssertNoMessageErrorContaining(houseBill.CU_BillNumInfo, BillValidation.NoPackagesEnteredForThisHouseBill);
			packingGroup.Packages.RemoveAndDeleteAll();
			AssertHasMessageErrorContaining(houseBill.CU_BillNumInfo, BillValidation.NoPackagesEnteredForThisHouseBill);
		}

		public void TestCU_HouseBillDoesntThrowErrorsOnPeriodicEntries()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "GIMMEALLYOURLOVIN";
			PackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			AssertNoMessageErrorContaining(houseBill.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
			packingGroup.Packages.AddNew();
			AssertNoMessageErrorContaining(houseBill.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
			packingGroup.Packages.RemoveAndDeleteAll();

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			packingGroup.Packages.AddNew();
			AssertNoMessageErrorContaining(houseBill.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
			packingGroup.Packages.RemoveAndDeleteAll();
			AssertHasMessageErrorContaining(houseBill.CU_BillNumInfo, Customs.Business.CusDecHouseBillValidation.NoPackagesEnteredForThisHouseBill);
		}

		public void TestCU_BillNumDoesNotHaveCharactersLengthCheck()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "123456789ABCDEFG00";
			AssertNoMessageErrors(houseBill.CU_BillNumInfo);

			houseBill.CU_BillNum = "123456789B123456789C123456789D12345";
			AssertNoMessageErrors(houseBill.CU_BillNumInfo);
		}

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
	}
}
