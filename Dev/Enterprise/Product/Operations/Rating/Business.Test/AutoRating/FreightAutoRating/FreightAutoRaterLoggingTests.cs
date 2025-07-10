using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Moq;

namespace Enterprise.Rating.Business.Testing
{
	public class FreightAutoRaterLoggingTests : RatingTestCase
	{
		public void TestAutorate_RateSelectorEnabled_LoggerShouldLogSelection()
		{
			var containerPK = Helper.Containers["20GP"].PK;
			var line = SetupRates(containerPK);

			var expectedLogs = Array.Empty<string>();

			AutoRateAndAssertLogs(containerPK, expectedLogs, true, supportsManualRateSelection: true, selectedLine: line);
		}

		public void TestAutorate_RateSelectorEnabled_RateSelectionSkipped_LoggerShouldSwitch()
		{
			var containerPK = Helper.Containers["20GP"].PK;
			var line = SetupRates(containerPK);

			var expectedLogs = new string[]
			{
				"Information: Rates Service: No Search Request is sent to Rates Service as Rates Service is NOT supported for ",
				"Information: RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1",
				"Information: RateLine Found FRT-UNT-CN-20GP-Standard Costs (TACT/General Rates)",
@"Information: Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Standard Costs (TACT/General Rates)
					Job's info:
					Container 20GP: 3 ContainerCount"
			};

			AutoRateAndAssertLogs(containerPK, expectedLogs, true, supportsManualRateSelection: true, selectedLine: null);
		}

		public void TestAutorate_RateSelectorEnabled_JobDoesntSupportRateSelector_LoggerShouldSwitch()
		{
			var containerPK = Helper.Containers["20GP"].PK;
			var line = SetupRates(containerPK);

			var expectedLogs = new string[]
			{
				"Information: Rates Service: No Search Request is sent to Rates Service as Rates Service is NOT supported for ",
				"Information: RatingHeader Found Standard Costs (TACT/General Rates) Entries: 1",
				"Information: RateLine Found FRT-UNT-CN-20GP-Standard Costs (TACT/General Rates)",
@"Information: Chargeable was added for
				RateLine FRT-UNT-CN-20GP-Standard Costs (TACT/General Rates)
					Job's info:
					Container 20GP: 3 ContainerCount"
			};

			AutoRateAndAssertLogs(containerPK, expectedLogs, true, supportsManualRateSelection: false, selectedLine: line);
		}

		void AutoRateAndAssertLogs(ZGuid containerPK, string[] expectedLogs, bool manualRateSelction, bool supportsManualRateSelection = false, RateLine selectedLine = null)
		{
			var dummyObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, containerPK, 3), 0m, 0m, NewClient, supportsManualRateSelection);

			var rateSelectionResult = selectedLine != null ? new List<AutoRateInfo> { new AutoRateInfo(Factory, selectedLine) } : (IEnumerable<AutoRateInfo>)null;

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object, isEqualization: false))
			{
				var context = RatingContext.CreateForManualSelect(new TestLogger(), new Mock<IDialogService>().Object);
				if (!manualRateSelction)
				{
					context = RatingContext.CreateInstance(Factory);
				}

				var autoRater = new FreightAutoRater(context);
				Mock.Get(_Rating.Interactor)
					.Setup(m => m.SelectRate(It.IsAny<IRatingContext>(), It.IsAny<RatingCriteria>()))
					.Returns(rateSelectionResult);

				autoRater.AutoRate(new AutoRatingProxy(dummyObject), CostSell.Cost);

				AssertContainsExactElementsInAnyOrder(expectedLogs, context.Logger.GetLogs());
			}
		}

		RateLine SetupRates(ZGuid containerPK)
		{
			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry("FCL", "SEA", "AU", "");
			costEntry.TI_RC = containerPK;

			var costLine = costEntry.RateLines[0];
			costLine.TL_RateCalculator = UnitCalculator.Code;
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			Factory.Save();

			return costLine;
		}
	}
}
