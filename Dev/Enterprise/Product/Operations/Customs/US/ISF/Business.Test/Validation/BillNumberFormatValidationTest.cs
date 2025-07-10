using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US.ISF;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class BillNumberFormatValidationTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ZPropertyInfoString info = (ZPropertyInfoString)header.BF_MasterBillInfo;
			info.Value = "Z!Z";
			info.ClearAllNotifications();
			string messageError = ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.MasterBillOfLading, 16);
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.MasterBillOfLading);
			AssertNoMessageError(info, messageError);
			info.Value = "A".PadLeft(16 + 1);
			info.ClearAllNotifications();
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.MasterBillOfLading);
			AssertHasMessageError(info, messageError);
			info.Value = "";
			info.ClearAllNotifications();
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.MasterBillOfLading);
			AssertNoMessageError(info, messageError);
			messageError = ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.HouseBillOfLading, 16);
			info.Value = "Z!Z";
			info.ClearAllNotifications();
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.HouseBillOfLading);
			AssertNoMessageError(info, messageError);
			info.Value = "A".PadLeft(17);
			info.ClearAllNotifications();
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.HouseBillOfLading);
			AssertHasMessageError(info, messageError);
			info.Value = "";
			info.ClearAllNotifications();
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.HouseBillOfLading);
			AssertNoMessageError(info, messageError);
			messageError = ValidationConstants.Bill.BillNumberMaxLength(BillTypeList.Descriptions.OceanBillOfLading, 16);
			info.Value = "Z!Z";
			info.ClearAllNotifications();
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.OceanBillOfLading);
			AssertNoMessageError(info, messageError);
			info.Value = "A".PadLeft(17);
			info.ClearAllNotifications();
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.OceanBillOfLading);
			AssertHasMessageError(info, messageError);
			info.Value = "";
			info.ClearAllNotifications();
			BillNumberFormatValidation.ValidateLength(info, BillTypeList.Codes.OceanBillOfLading);
			AssertNoMessageError(info, messageError);
			if (ErrorReporter.LastKeyReported == "Validation:BF_MasterBill")
			{
				ErrorReporter.Clear();
			}
		}
	}
}
