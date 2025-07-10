using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateCommodityFMCSelectorControllerTest : RatingTestCase
	{
		public void TestPickMessageAppearsWhenNoResult()
		{
			var criteria = GetCriteria("AAA");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;
			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns<CommodityFMCPair>(null);
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);

			var possibleMatcherFormShown = false;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs
			(
				form =>
				{
					if (form is RateCommodityFMCSelectorForm selector)
					{
						possibleMatcherFormShown = true;
					}
				}
			);

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy());

			mockDialogService.Verify(x => x.ShowNoFMCTariffIDCombinationFound(It.Is<string>(y => y == "AAA")));
			controller.Verify(
				x => x.PickPair(
					It.IsAny<BusinessObjectFactory>(),
					It.IsAny<IEnumerable<CommodityFMCPair>>()
				),
				Times.Never
			);

			AssertEquals(false, possibleMatcherFormShown);
		}

		public void TestPickPopupAppears()
		{
			CreateCommodityAndRatingCodesForFMCTest(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);
			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;
			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(new[] {
					new CommodityFMCPair() { CommodityCode = "BBB" },
					new CommodityFMCPair() { CommodityCode = "BBB1" },
				});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);

			var possibleMatcherFormShown = false;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(
				form =>
				{
					if (form is RateCommodityFMCSelectorForm selector)
					{
						possibleMatcherFormShown = true;
						var expectedCodes = new[] { "BBB", "BBB1" };
						var actualCodes = selector.Model.CompanyTariffs.Select(x => x.CommodityPair.CommodityCode);

						AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
					}
				}
			);

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy());

			controller.Verify(
				x => x.PickPair(
					It.IsAny<BusinessObjectFactory>(),
					It.Is(
						(IEnumerable<CommodityFMCPair> y) => y.Select(z => z.CommodityCode).EqualIgnoringOrder(new[] { "BBB", "BBB1" }, null, false))
				));

			AssertEquals(true, possibleMatcherFormShown);
		}

		public void TestLoggerMessageAppearsWhenPairsFound()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);
			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;
			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(new[] {
					new CommodityFMCPair() { CommodityCode = "BBB" },
					new CommodityFMCPair() { CommodityCode = "BBB1" },
				});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy());

			mockLogger.Verify(x => x.ToString(), Times.Once());
			Assert(true);
		}

		public void TestLoggerMessageAppearsWhenPairsNotFound()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);
			var criteria = GetCriteria("BBL");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;
			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(new[] {
					new CommodityFMCPair() { CommodityCode = "BBB" },
					new CommodityFMCPair() { CommodityCode = "BBB1" },
				});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy());

			mockLogger.Verify(x => x.ToString(), Times.Once());
			Assert(true);
		}

		public void TestUpdate_FMCTariffIdAndCommodity_WhenPicked()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;
			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetUpdater(It.IsAny<IAutoRating>()))
				.Returns(mockUpdater.Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(
					new[] {
						new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" }
					});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);
			controller
				.Setup(x => x.PickPair(Factory, It.IsAny<IEnumerable<CommodityFMCPair>>()))
				.Returns(new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" });

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy());
			mockUpdater.Verify(x => x.UpdateRateCommodityCodeAndFMCTariffID("BBB1", "4444"), Times.Once());

			Assert(true);
		}

		public void TestUpdate_NeitherFMCTariffIdNorCommodity_WhenNotPicked()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);
			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;
			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetUpdater(It.IsAny<IAutoRating>()))
				.Returns(mockUpdater.Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(
					new[] {
						new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" }
					});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);
			controller
				.Setup(x => x.PickPair(Factory, It.IsAny<IEnumerable<CommodityFMCPair>>()))
				.Returns<CommodityFMCPair>(null); // no picking occurred

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy());
			mockUpdater.Verify(x => x.UpdateRateCommodityCodeAndFMCTariffID(It.IsAny<ZString>(), It.IsAny<ZString>()), Times.Never);

			Assert(true);
		}

		public void TestUpdate_DetailedGoodsDescription_PromptSet_WhenEmptyAndUnavailable()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;

			mockDialogService
				.Setup(x => x.PromptToUpdateOrReplaceDetailedGoodsDescription(It.IsAny<bool>()))
				.Returns(ZDialogResult.OK);

			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetUpdater(It.IsAny<IAutoRating>()))
				.Returns(mockUpdater.Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(
					new[] {
						new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" }
					});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);
			controller
				.Setup(x => x.PickPair(Factory, It.IsAny<IEnumerable<CommodityFMCPair>>()))
				.Returns(new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" });

			controller.Object.SelectAndUpdate(
				Factory,
				null,
				new DetailedGoodsDescriptionProxy() { IsAvailable = false, IsEmpty = true });
			mockUpdater.Verify(x => x.UpdateDetailedGoodsDescription("Desc: BBB1", It.IsAny<bool>()), Times.Never());

			Assert(true);
		}

		public void TestUpdate_DetailedGoodsDescription_PromptSet_WhenEmptyAndAvailable()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;

			mockDialogService
				.Setup(x => x.PromptToUpdateOrReplaceDetailedGoodsDescription(It.IsAny<bool>()))
				.Returns(ZDialogResult.OK);

			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetUpdater(It.IsAny<IAutoRating>()))
				.Returns(mockUpdater.Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(
					new[] {
						new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" }
					});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);
			controller
				.Setup(x => x.PickPair(Factory, It.IsAny<IEnumerable<CommodityFMCPair>>()))
				.Returns(new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" });

			controller.Object.SelectAndUpdate(
				Factory,
				null,
				new DetailedGoodsDescriptionProxy() { IsAvailable = true, IsEmpty = true });
			mockUpdater.Verify(x => x.UpdateDetailedGoodsDescription("Desc: BBB1", It.IsAny<bool>()), Times.Once());

			Assert(true);
		}

		public void TestUpdate_DetailedGoodsDescription_PromptKeep()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;

			mockDialogService
				.Setup(x => x.PromptToUpdateOrReplaceDetailedGoodsDescription(It.IsAny<bool>()))
				.Returns(ZDialogResult.Cancel);

			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetUpdater(It.IsAny<IAutoRating>()))
				.Returns(mockUpdater.Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(
					new[] {
						new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" }
					});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);
			controller
				.Setup(x => x.PickPair(Factory, It.IsAny<IEnumerable<CommodityFMCPair>>()))
				.Returns(new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" });

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy());
			mockUpdater.Verify(x => x.UpdateDetailedGoodsDescription("Desc: BBB1", It.IsAny<bool>()), Times.Never());

			Assert(true);
		}

		public void TestUpdate_DetailedGoodsDescription_PromptAppend()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;
			mockDialogService
				.Setup(x => x.PromptToUpdateOrReplaceDetailedGoodsDescription(It.IsAny<bool>()))
				.Returns(ZDialogResult.Yes);

			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetUpdater(It.IsAny<IAutoRating>()))
				.Returns(mockUpdater.Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(
					new[] {
						new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" }
					});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);
			controller
				.Setup(x => x.PickPair(Factory, It.IsAny<IEnumerable<CommodityFMCPair>>()))
				.Returns(new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" });

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy() { IsAvailable = true });
			mockUpdater.Verify(x => x.UpdateDetailedGoodsDescription("Desc: BBB1", true), Times.Once());

			Assert(true);
		}

		public void TestUpdate_DetailedGoodsDescription_PromptReplace()
		{
			CreateCommodityAndRatingCodesForFMCTest
			(
				new RatingCode { commodity = "BBB", childCommodities = new[] { "BBB1", "BBB2" } }
			);

			var criteria = GetCriteria("BBB");
			var mockUpdater = new Mock<IJobDataUpdater>();
			var mockDialogService = new Mock<IDialogService>();
			var mockLogger = new Mock<RateCommodityFMCPairLogger>();
			mockLogger.Setup(x => x.ToString()).Returns("Valid Return Logs");
			var controller = new Mock<RateCommodityFMCSelectorController>(mockDialogService.Object, mockLogger.Object);
			controller.CallBase = true;
			mockDialogService
				.Setup(x => x.PromptToUpdateOrReplaceDetailedGoodsDescription(It.IsAny<bool>()))
				.Returns(ZDialogResult.No);
			controller
				.Setup(x => x.GetAdapter(It.Is<IRatingSupporter>(p => p == null)))
				.Returns(new Mock<IAutoRating>().Object);
			controller
				.Setup(x => x.GetUpdater(It.IsAny<IAutoRating>()))
				.Returns(mockUpdater.Object);
			controller
				.Setup(x => x.GetMatches(It.Is<BusinessObjectFactory>(p => p == Factory), It.Is<RatingCriteria>(p => p == criteria)))
				.Returns(
					new[] {
						new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" }
					});
			controller
				.Setup(x => x.GetCriteria(It.Is<BusinessObjectFactory>(p => p == Factory), It.IsAny<IAutoRating>()))
				.Returns(criteria);
			controller
				.Setup(x => x.PickPair(Factory, It.IsAny<IEnumerable<CommodityFMCPair>>()))
				.Returns(new CommodityFMCPair() { CommodityCode = "BBB1", FMCTariffID = "4444" });

			controller.Object.SelectAndUpdate(Factory, null, new DetailedGoodsDescriptionProxy() { IsAvailable = true });
			mockUpdater.Verify(x => x.UpdateDetailedGoodsDescription("Desc: BBB1", false), Times.Once());

			Assert(true);
		}

		#region Helpers

		void CreateCommodityAndRatingCodesForFMCTest(params RatingCode[] ratingCodes)
		{
			foreach (var ratingCode in ratingCodes)
			{
				Helper.NewCommodity(ratingCode.commodity, "FRT").RH_Description = "Desc: " + ratingCode.commodity;
				foreach (var child in ratingCode.childCommodities)
				{
					Helper.NewCommodity(child, "FRT").RH_Description = "Desc: " + child;
					Helper.NewCommodityRatingCode(ratingCode.commodity, child);
				}
			}
			Factory.Save();
		}

		TestRatingCriteria GetCriteria(string rateCommodity)
		{
			var criteria = new TestRatingCriteria("AUSYD", "NZAKL", FreightMode.FCL, 1, 1, null);
			using (criteria.TemporarilyAllowCriteriaChanging())
			{
				criteria.OverriddenCommodity = new[] { Helper.NewCommodity(rateCommodity, "FRT") };
			}

			return criteria;
		}

		class RatingCode
		{
			public string[] childCommodities = Array.Empty<string>();
			public string commodity;
		}

		#endregion
	}
}
