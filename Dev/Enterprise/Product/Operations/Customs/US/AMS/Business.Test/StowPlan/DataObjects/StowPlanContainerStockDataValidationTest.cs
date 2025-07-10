using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class StowPlanContainerStockDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckContainerOperator()
		{
			stockData.Validation.ValidateContainerOperator();
			AssertHasMessageErrorContaining(stockData.ContainerOperatorInfo, StowPlanContainerStockDataValidation.MissingSCACMsg);
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWNER";
			owner.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			stock.R6_OH_Owner = owner.PK;
			stockData.Validation.ValidateContainerOperator();
			AssertNoMessageErrorContaining(stockData.ContainerOperatorInfo, StowPlanContainerStockDataValidation.MissingSCACMsg);
		}

		RefContainerStock stock;
		StowPlanContainerStockData stockData;
		protected override void SetUp()
		{
			base.SetUp();
			stock = Factory.New<RefContainerStock>();
			stockData = new StowPlanContainerStockData(stock);
		}
	}
}
