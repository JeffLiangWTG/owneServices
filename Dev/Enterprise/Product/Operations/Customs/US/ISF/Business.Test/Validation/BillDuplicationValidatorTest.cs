using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class BillDuplicationValidatorTest : TestCaseWithFactory
	{
		public void TestValidateBillIsNotDuplicated()
		{
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			header2.BF_MasterBill = "MB2343223";
			header2.BF_HouseBill = "HB2644434";
			header2.BF_OceanBill = "OB2342322";
			CusISFBill bill2 = header2.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "MB56694465";
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_MasterBill = "MB2343223";
			header.BF_HouseBill = "HB3234234";
			header.BF_OceanBill = "OB5684554";
			Factory.Save();
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = "";
			string messageError = "has already been used in ISF Job";
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusISFBill newBill = newFactory.New<CusISFBill>();
			ZPropertyInfo info = newBill.BB_BillNumInfo;
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertNoMessageErrorContaining(info, messageError);
			bill.BB_BillNum = "HB3234234";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertNoMessageError(info, messageError);
			bill.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertHasMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertNoMessageErrorContaining(info, messageError);
			bill.BB_BillNum = "HB2644434";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertHasMessageErrorContaining(info, messageError);
			bill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertHasMessageErrorContaining(info, messageError);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertHasMessageErrorContaining(info, messageError);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill.BB_BillNum = "OB5684554";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertHasMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertNoMessageErrorContaining(info, messageError);
			bill.BB_BillNum = "HB6955445";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertNoMessageErrorContaining(info, messageError);
			bill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill.BB_BillNum = "OB5684554";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertHasMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertNoMessageErrorContaining(info, messageError);
			bill.BB_BillNum = "OB2342322";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertHasMessageErrorContaining(info, messageError);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill.BB_BillNum = "MB2343223";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertHasMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertNoMessageErrorContaining(info, messageError);
			bill.BB_BillNum = "MB56694465";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, ValidationConstants.Bill.ReferenceDataAlreadyExists);
			AssertNoMessageErrorContaining(info, messageError);
			if (ErrorReporter.LastKeyReported == "Validation:BB_BillNum")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestValidateSuretyBondReferenceNumberIsNotDuplicated()
		{
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			header2.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			header2.BF_SuretyCode = "971";
			header2.BF_BondReferenceNumber = "BF12664";
			CusISFHeader header3 = Factory.New<CusISFHeader>();
			header3.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			header3.BF_SuretyCode = "971";
			header3.BF_BondReferenceNumber = "BF12664";
			Factory.Save();
			string messageError = ValidationConstants.Bill.ReferenceDataAlreadyExistsOnAnotherISF(BillTypeList.Descriptions.BondReferenceNumber, "BF12664", header3.HumanReadableName);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusISFHeader header = newFactory.New<CusISFHeader>();
			header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			header.BF_SuretyCode = "971";
			CusISFBill bill = header.ReferenceDatas.AddNew();
			ZPropertyInfo info = bill.BB_BillNumInfo;
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, messageError);
			bill.BB_BillNum = "BF12664";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, messageError);
			bill.BB_BillType = BillTypeList.Codes.BondReferenceNumber;
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertHasMessageError(info, messageError);
			header.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bill.BB_BillNum = "BF12664";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, messageError);
			header3.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			Factory.Save();
			header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bill.BB_BillNum = "BF12664";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, messageError);
			header3.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			Factory.Save();
			header.BF_SuretyCode = "979";
			bill.BB_BillNum = "BF12664";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, messageError);
			header3.BF_SuretyCode = "978";
			Factory.Save();
			header.BF_SuretyCode = "971";
			bill.BB_BillNum = "BF12664";
			info.ClearAllNotifications();
			BillDuplicationValidator.ValidateBillIsNotDuplicated(bill, info);
			AssertNoMessageError(info, messageError);
			if (ErrorReporter.LastKeyReported == "Validation:BB_BillNum")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
