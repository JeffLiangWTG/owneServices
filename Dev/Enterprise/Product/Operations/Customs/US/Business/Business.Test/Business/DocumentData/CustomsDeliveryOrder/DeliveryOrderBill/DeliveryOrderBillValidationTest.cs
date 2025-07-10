using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DeliveryOrderBillValidationTest : TestCaseWithFactory
	{
		public void TestCheckCY_Code()
		{
			DeliveryOrderBill bizObj = Factory.New<DeliveryOrderBill>();
			bizObj.CY_Code = ZString.Empty;
			string warningMessage = MandatoryValidation.YouHaveNotEntered + " a Bill Type.";
			AssertHasWarning(bizObj.CY_CodeInfo, warningMessage);
			AssertNoWarningContaining(bizObj.CY_CodeInfo, ListValidation.InvalidCodeMessage);
			bizObj.CY_Code = "ZZ";
			AssertNoWarningContaining(bizObj.CY_CodeInfo, warningMessage);
			AssertHasWarningContaining(bizObj.CY_CodeInfo, ListValidation.InvalidCodeMessage);
			foreach (CodeDescriptionPair pair in new Customs.Business.BillTypeList())
			{
				bizObj.CY_Code = pair.Code;
				AssertNoWarningContaining(bizObj.CY_CodeInfo, warningMessage);
				AssertNoWarningContaining(bizObj.CY_CodeInfo, ListValidation.InvalidCodeMessage);
			}
		}

		public void TestCheckCY_Data()
		{
			DeliveryOrderBill bizObj = Factory.New<DeliveryOrderBill>();
			bizObj.CY_Data = ZString.Empty;
			AssertHasWarningContaining(bizObj.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			bizObj.CY_Data = "BH23423";
			AssertNoWarningContaining(bizObj.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
