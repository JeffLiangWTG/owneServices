using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USITNumberAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ITNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Bill master1 = declaration.Bills.AddNew();
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master1.CU_BillNum = "MB1";
			Bill master2 = declaration.Bills.AddNew();
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master2.CU_BillNum = "MB2";
			Bill house2 = master2.ChildBills.AddNew();
			house2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house2.ITAndSplitDetails.AddNew().US_ITNumber = "V123456";
			master2.ITNumber = "12345678";
			AssertHasError(master2.ITAndSplitDetails[0].US_ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatLowestBill);
			master2.ITAndSplitDetails.RemoveAndDeleteAll();
			master2.Validation.ValidateAll();
			AssertNoError(master2.ITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNoShouldBeEnteredatLowestBill);
		}

		public void TestCheckUS_NoOfPacks()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			declaration.JE_MasterBill = "OBL";
			var houseBill = declaration.PrimaryMasterBill.ChildBills.AddNew();
			houseBill.CU_NoOfPacks = 19m;
			var itNo = houseBill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "12456";
			itNo.US_NoOfPacks = 12;
			itNo = houseBill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V123456789";
			itNo.US_NoOfPacks = 32;
			AssertHasMessageError(itNo.US_NoOfPacksInfo, USITNumberAddInfoValidation.Qty);
			itNo.US_NoOfPacks = 19;
			AssertNoMessageError(itNo.US_NoOfPacksInfo, USITNumberAddInfoValidation.Qty);
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_InbondType = EntryTypeList.Codes.ImmediateTransportation;
			declaration.JE_MasterBill = "OBL";
			var houseBill = declaration.PrimaryMasterBill.ChildBills.AddNew();
			houseBill.US_SESplitShip = true;
			var split = houseBill.ITAndSplitDetails.AddNew();
			split.AddInfoValidation.ValidateAll();
			AssertHasMessageErrorContaining(split.US_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(split.US_ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(split.US_FlightNumberInfo, MandatoryValidation.YouHaveNotEntered);
			split.US_FlightNumber = "QWE3";
			split.US_CarrierCode = "APLU";
			split.US_ArrivalDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(split.US_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(split.US_ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(split.US_FlightNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
