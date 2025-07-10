using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ITNumberValidatorTest : TestCaseWithFactory
	{
		public void TestValidateITNumberFormat()
		{
			DummyBusinessObject obj = Factory.New<DummyBusinessObject>();
			obj.Z0_VarCharMax = "123";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			obj.Z0_VarCharMax = DeclarationTestHelper.ValidITNumber1ForTesting;
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			obj.Z0_VarCharMax = "Vblah-blah";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, true);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "12365";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, true);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "vblah-blah";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, true);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "V236542J144";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "V2365425144";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "v2365425144";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "VHH65425144";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "vHH65425144";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "VH765425144";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			obj.Z0_VarCharMax = "V7603245271";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertHasMessageErrorContaining(obj.Z0_VarCharMaxInfo, "Invalid check digit. The last digit should be");
			obj.Z0_VarCharMax = "V7603245275";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertNoMessageErrorContaining(obj.Z0_VarCharMaxInfo, "Invalid check digit. The last digit should be");
			obj.Z0_VarCharMax = "v7603245275";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertNoMessageErrorContaining(obj.Z0_VarCharMaxInfo, "Invalid check digit. The last digit should be");
			//if Air then AWB number format is also correct (11n)
			obj.Z0_VarCharMax = "12345678";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, true);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			obj.Z0_VarCharMax = "1111G111111";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, true);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			obj.Z0_VarCharMax = "11111111111";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, true);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			obj.Z0_VarCharMax = "11111111111";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, false);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			obj.Z0_VarCharMax = "123456789";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, true);
			AssertHasWarning(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("2"));
			obj.Z0_VarCharMax = "123456782";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidateITNumberFormat(obj.Z0_VarCharMaxInfo, true);
			AssertNoWarning(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("2"));
			if (ErrorReporter.LastKeyReported == "Validation:Z0_VarCharMax")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestInvalidITNumber()
		{
			Declaration.JE_PrimaryITNumber = "123";
			AssertHasMessageError(Declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			Declaration.JE_PrimaryITNumber = DeclarationTestHelper.ValidITNumber1ForTesting;
			AssertNoMessageError(Declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.JE_PrimaryITNumber = "Vblah-blah";
			AssertHasMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_PrimaryITNumber = "12365";
			AssertNoMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_PrimaryITNumber = "vblah-blah";
			AssertHasMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			Declaration.JE_PrimaryITNumber = "V236542J144";
			AssertHasMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_PrimaryITNumber = "V2365425144";
			AssertNoMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_PrimaryITNumber = "v2365425144";
			AssertNoMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_PrimaryITNumber = "VHH65425144";
			AssertNoMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_PrimaryITNumber = "vHH65425144";
			AssertNoMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_PrimaryITNumber = "VH765425144";
			AssertNoMessageError(Declaration.JE_PrimaryITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			Declaration.JE_PrimaryITNumber = "V7603245271";
			AssertHasMessageErrorContaining(Declaration.JE_PrimaryITNumberInfo, "Invalid check digit. The last digit should be");
			Declaration.JE_PrimaryITNumber = "V7603245275";
			AssertNoMessageErrorContaining(Declaration.JE_PrimaryITNumberInfo, "Invalid check digit. The last digit should be");
			Declaration.JE_PrimaryITNumber = "v7603245275";
			AssertNoMessageErrorContaining(Declaration.JE_PrimaryITNumberInfo, "Invalid check digit. The last digit should be");
			//if Air then AWB number format is also correct (11n)
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.JE_PrimaryITNumber = "12345678";
			AssertHasMessageError(Declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			Declaration.JE_PrimaryITNumber = "1111G111111";
			AssertHasMessageError(Declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			Declaration.JE_PrimaryITNumber = "11111111111";
			AssertNoMessageError(Declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertHasMessageError(Declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
		}

		public void TestValidatePreviousITNumber()
		{
			DummyBusinessObject obj = Factory.New<DummyBusinessObject>();
			obj.Z0_VarCharMax = "IT32342";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidatePreviousITNumberFormat(obj.Z0_VarCharMaxInfo);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			obj.Z0_VarCharMax = "";
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			obj.Z0_VarCharMax = "123234223";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidatePreviousITNumberFormat(obj.Z0_VarCharMaxInfo);
			AssertNoMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			AssertHasWarning(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("6"));
			obj.Z0_VarCharMax = "123234226";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidatePreviousITNumberFormat(obj.Z0_VarCharMaxInfo);
			AssertNoWarning(obj.Z0_VarCharMaxInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("6"));
			AssertNoNotifications(obj.Z0_VarCharMaxInfo);
			obj.Z0_VarCharMax = "VAB12345675";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidatePreviousITNumberFormat(obj.Z0_VarCharMaxInfo);
			AssertNoNotifications(obj.Z0_VarCharMaxInfo);
			obj.Z0_VarCharMax = "08112345678";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidatePreviousITNumberFormat(obj.Z0_VarCharMaxInfo);
			AssertNoNotifications(obj.Z0_VarCharMaxInfo);
			obj.Z0_VarCharMax = "081123456783";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidatePreviousITNumberFormat(obj.Z0_VarCharMaxInfo);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			obj.Z0_VarCharMax = "HK8-175464663";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidatePreviousITNumberFormat(obj.Z0_VarCharMaxInfo);
			AssertHasMessageError(obj.Z0_VarCharMaxInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			obj.Z0_VarCharMax = "HK8-17546466";
			obj.Z0_VarCharMaxInfo.ClearAllNotifications();
			ITNumberValidator.ValidatePreviousITNumberFormat(obj.Z0_VarCharMaxInfo);
			AssertNoNotifications(obj.Z0_VarCharMaxInfo);
			if (ErrorReporter.LastKeyReported == "Validation:Z0_VarCharMax")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestITNumbersEnteredForAllLowestBills()
		{
			Bill master1 = Declaration.Bills.AddNew();
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master1.CU_BillNum = "MB1";
			Bill master2 = Declaration.Bills.AddNew();
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master2.CU_BillNum = "MB2";
			Bill house2 = master2.ChildBills.AddNew();
			house2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house2.ITAndSplitDetails.AddNew().US_ITNumber = "V123456";
			AssertNoMessageError(house2.ITAndSplitDetails[0].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredForAllLowestBills);
			master1.Validation.ValidateAll();
			AssertHasMessageError(master1.ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredForAllLowestBills);
		}

		public void TestITNoShouldBeEnteredOnLowestBill()
		{
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Bill master1 = Declaration.Bills.AddNew();
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master1.CU_BillNum = "MB1";
			master1.ITNumber = "V12456789";
			AssertNoError(master1.ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatLowestBill);
			Bill master2 = Declaration.Bills.AddNew();
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master2.CU_BillNum = "MB2";
			master2.ITNumber = "12345678";
			AssertNoError(master2.ITAndSplitDetails[0].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatLowestBill);
			Bill house2 = master2.ChildBills.AddNew();
			house2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house2.ITAndSplitDetails.AddNew().US_ITNumber = "V123456";
			AssertHasError(master2.ITAndSplitDetails[0].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatLowestBill);
		}

		public void TestITNoShouldBeEnteredAtTheMasterLevel()
		{
			Declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			Bill master1 = Declaration.Bills.AddNew();
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master1.CU_BillNum = "MB1";
			master1.ITNumber = "V12456789";
			Bill house1 = master1.ChildBills.AddNew();
			house1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house1.ITAndSplitDetails.AddNew().US_ITNumber = "V123456";
			AssertHasError(house1.ITAndSplitDetails[0].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatMasterLevel);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Bill master2 = Declaration.Bills.AddNew();
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master2.CU_BillNum = "MB2";
			master2.ITNumber = "V12456789";
			Bill house2 = master2.ChildBills.AddNew();
			house2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house2.ITAndSplitDetails.AddNew().US_ITNumber = "V123456";
			AssertNoError(house2.ITAndSplitDetails[0].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatMasterLevel);
		}

		public void TestValidateITNumbersOnPrimaryBill()
		{
			Bill master1 = Declaration.Bills.AddNew();
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master1.CU_BillNum = "MB1";
			ITNumberValidator.ValidateITNumbersOnPrimaryBill(master1);
			AssertNoError(master1.ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatLowestBill);
			Bill master2 = Declaration.Bills.AddNew();
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master2.CU_BillNum = "MB2";
			master2.ITNumber = "12345678";
			Bill house2 = master2.ChildBills.AddNew();
			house2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house2.ITAndSplitDetails.AddNew().US_ITNumber = "V123456";
			ITNumberValidator.ValidateITNumbersOnPrimaryBill(master2);
			AssertHasError(master2.ITAndSplitDetails[0].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatLowestBill);
		}

		public void TestDuplicateNumber()
		{
			Bill master1 = Declaration.Bills.AddNew();
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master1.CU_BillNum = "MB1";
			master1.ITAndSplitDetails.AddNew().US_ITNumber = "V12456789";
			AssertNoMessageError(master1.ITAndSplitDetails[0].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.DuplicateNumber);
			master1.ITAndSplitDetails.AddNew().US_ITNumber = "V12456789";
			AssertHasMessageError(master1.ITAndSplitDetails[1].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.DuplicateNumber);
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				}

				return declaration;
			}
		}
	}
}
