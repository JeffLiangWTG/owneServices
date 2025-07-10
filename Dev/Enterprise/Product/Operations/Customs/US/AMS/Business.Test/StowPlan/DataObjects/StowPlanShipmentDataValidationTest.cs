using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class StowPlanShipmentDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckPortOfDischarge()
		{
			bill.JS_NKDischargePort = "USLAX";
			bill.JS_NKDischargePort = ZString.Empty;
			billData.Validation.ValidatePortOfDischarge();
			AssertHasMessageErrorContaining(billData.PortOfDischargeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.JS_NKDischargePort = "USLAX";
			billData.Validation.ValidatePortOfDischarge();
			AssertNoMessageErrorContaining(billData.PortOfDischargeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckPortOfLading()
		{
			bill.JS_NKLoadPort = "AUSYD";
			bill.JS_NKLoadPort = ZString.Empty;
			billData.Validation.ValidatePortOfLading();
			AssertHasMessageErrorContaining(billData.PortOfLadingInfo, MandatoryValidation.YouHaveNotEntered);
			bill.JS_NKLoadPort = "AUSYD";
			billData.Validation.ValidatePortOfLading();
			AssertNoMessageErrorContaining(billData.PortOfLadingInfo, MandatoryValidation.YouHaveNotEntered);
		}

		BillOfLading bill;
		StowPlanShipmentData billData;
		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.New<BillOfLading>();
			billData = new StowPlanShipmentData(bill);
		}
	}
}
