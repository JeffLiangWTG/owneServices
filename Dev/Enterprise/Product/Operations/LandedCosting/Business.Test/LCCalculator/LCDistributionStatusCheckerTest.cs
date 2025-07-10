using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LCDistributionStatusCheckerTest : TestCaseWithFactory
	{
		public void TestCheckPreConditionsForLandedCostDistribution()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();

			DummyExchangeRateHolder dummyExRateHolder = Factory.New<DummyExchangeRateHolder>();
			dummyExRateHolder.CurrencyCodeExposed = "AUD";
			testHelper.DummyHeader.ExchangeRateHoldersExposed = new ILandedCostExchangeRateHolder[] { dummyExRateHolder };

			LandedCostingExRate exRate = lCHeader.ExchangeRates[0];
			exRate.ExchangeRate = 0m;
			AssertEquals("Empty Exchange rate is an error", true, exRate.ExchangeRateInfo.HasErrors());

			Factory.Save();

			LandedCostHeader lCHeaderLoaded = Factory.Load<LandedCostHeader>(lCHeader.PK);
			LCDistributionStatusChecker checker2 = new LCDistributionStatusChecker(lCHeaderLoaded);
			StatusCheckResult result = checker2.GetStatus();
			AssertEquals("PreCondition Error", true, result.Message.Contains("\nError - ExchangeRate:"));
		}

		public void TestNoLandedCostChargesWarning()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			lCHeader.CostInputs.RemoveAndDeleteAll();

			LCDistributionStatusChecker checker = new LCDistributionStatusChecker(lCHeader);
			StatusCheckResult result = checker.GetStatus();
			AssertEquals("No landed cost input", LCDistributionStatusChecker.LandedCostingNoChargeRowWarningMessage, result.Message);
			AssertEquals("It is an error", NotificationTypes.Warning, result.NotificationType);
		}

		public void TestCheckWarningsForLandedCostDistributionForPreviousLCJob()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			var landedHistory = lCHeader.Histories.AddNew();
			landedHistory.LH_ParentID = ZGuid.NewZGuid();
			LCDistributionStatusChecker checker = new LCDistributionStatusChecker(lCHeader);
			StatusCheckResult warnings = checker.GetStatus();
			AssertEquals("Previous job exists", true, warnings.Message.Contains(LCDistributionStatusChecker.ExistingLandedCostJob));
		}

		public void TestLandedCostHeaderErrors()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
			lCHeader.CostInputs[0].LI_ServiceExRate = -10;
			AssertEquals("An error", true, lCHeader.CostInputs[0].LI_ServiceExRateInfo.HasErrors());
			AssertEquals("An error", true, lCHeader.HasErrors);
			LCDistributionStatusChecker checker = new LCDistributionStatusChecker(lCHeader);
			StatusCheckResult result = checker.GetStatus();
			AssertEquals("PreCondition Error", true, result.Message.Contains(LCDistributionStatusChecker.ErrorMessage));
		}

		public void TestMissingDistributionFieldByChecking()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();

			StatusCheckResult result = new LCDistributionStatusChecker(lCHeader).GetStatus();
			AssertNull("Job is in a valid status", result.Message);

			testHelper.LCInput3.LI_DistributeCostBy = CostDistributionMechanismList.Codes.ActualVolume;
			result = new LCDistributionStatusChecker(lCHeader).GetStatus();
			AssertEquals("No lines have volume", LCDistributionStatusChecker.DistributionFieldErrorMessage, result.Message);
			AssertEquals("Should be an error", NotificationTypes.Error, result.NotificationType);

			testHelper.Ultimate1.VolumeExposed = 10m;
			result = new LCDistributionStatusChecker(lCHeader).GetStatus();
			AssertEquals("Have one line with volume", LCDistributionStatusChecker.DistributionFieldWarningMessage, result.Message);
			AssertEquals("Have one line with volume", NotificationTypes.Warning, result.NotificationType);
		}
	}
}
