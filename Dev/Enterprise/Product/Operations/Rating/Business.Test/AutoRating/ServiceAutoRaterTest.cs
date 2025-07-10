using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Moq;
using WiseRates.Constants;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class ServiceAutoRaterTest : RatingTestCase
	{
		#region TestRemoveRatesOverridenByServiceSpotRates

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_OriginCarrierStorage()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CarrierStorage, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_OriginCartageDemurrageTotal()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_OriginContainerDetention()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.ContainerDetention, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_OriginLabor()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_OriginStorage()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_DestinationCarrierStorage()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CarrierStorage, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_DestinationCartageDemurrageTotal()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_DestinationContainerDetention()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_DestinationLabor()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Labor, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates__ShipmentPenaltyService_OverridesClientRateBasedOnChargeCode_DestinationStorage()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode);

		public void TestRemoveRatesOverridenByServiceSpotRates_ShipmentServiceOtherThanPenalty_OverridesClientRateBasedOnChargeGroupAndChargeSubGroup_Origin()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeGroupAndChargeSubGroup);

		public void TestRemoveRatesOverridenByServiceSpotRates_ShipmentServiceOtherThanPenalty_OverridesClientRateBasedOnChargeGroupAndChargeSubGroup_Destination()
			=> AssertRemoveRatesOverridenByServiceSpotRates(ChargeCodeGroupList.Codes.Destination, FreightServiceType.Codes.Fumigation, RemoveRatesOverridenByServiceSpotRates.BasedOnChargeGroupAndChargeSubGroup);

		void AssertRemoveRatesOverridenByServiceSpotRates(string chargeGroup, string chargeSubGroup, RemoveRatesOverridenByServiceSpotRates removeRatesOverridenByServiceSpotRates)
		{
			var chargeCode1 = Helper.ChargeCodes.New("CHG1", "Charge 1", UnitCalculator.Code, chargeGroup, chargeSubGroup);
			chargeCode1.AC_IsAdhocServiceCharge = true;
			var chargeCode2 = Helper.ChargeCodes.New("CHG2", "Charge 2", UnitCalculator.Code, chargeGroup, chargeSubGroup);
			Factory.Save();

			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			entry.RateLines.RemoveAndDeleteAll();
			var clientRateline1 = entry.AddFlatRateLine(chargeCode1.AC_Code, 200m);
			var clientRateline2 = entry.AddFlatRateLine(chargeCode2.AC_Code, 300m);

			var testRatingCriteria = new TestRatingCriteria();
			var jobService = new JobServiceInfo(true, chargeGroup, chargeSubGroup, "Job Service", totalCost: 100);
			testRatingCriteria.JobServices.Add(jobService);
			var spotRateEntryCreator = new SpotRateEntryCreator(testRatingCriteria);
			var serviceEntries = spotRateEntryCreator.GetAllJobServiceRates(false);

			var entries = new List<IRateEntry>();
			entries.Add(entry);
			entries.AddRange(serviceEntries);

			var lineRepository = new RateLinesRepository(testRatingCriteria, entries, new TestLogger());
			var serviceAutoRater = new ServiceAutoRater(testRatingCriteria, Factory, true);
			var jobServiceRateLines = entries.FirstOrDefault(x => x.JobServiceForSpotEntry != null && x.JobServiceForSpotEntry.IsEnabled).ChildRateLines.FirstOrDefault();

			AssertEquals("Pre-condition", "2xCHG1, 1xCHG2", lineRepository.DebuggerDisplay);
			serviceAutoRater.RemoveRatesOverridenByServiceSpotRates(lineRepository);

			if (removeRatesOverridenByServiceSpotRates == RemoveRatesOverridenByServiceSpotRates.BasedOnChargeCode)
			{
				AssertEquals("1xCHG1, 1xCHG2", lineRepository.DebuggerDisplay);
				AssertContainsExactElementsInAnyOrder(new List<IRateLine>() { jobServiceRateLines, clientRateline2 }, lineRepository.GetIRateLines());
			}
			else if (removeRatesOverridenByServiceSpotRates == RemoveRatesOverridenByServiceSpotRates.BasedOnChargeGroupAndChargeSubGroup)
			{
				AssertEquals("1xCHG1, 0xCHG2", lineRepository.DebuggerDisplay);
				AssertContainsExactElementsInAnyOrder(new List<IRateLine>() { jobServiceRateLines }, lineRepository.GetIRateLines());
			}
		}

		enum RemoveRatesOverridenByServiceSpotRates
		{
			BasedOnChargeCode,
			BasedOnChargeGroupAndChargeSubGroup
		}

		#endregion

		#region RemoveOtherOriginAndDestinationChargesWithInvalidLocation

		public void TestRemoveOtherOriginAndDestinationChargesWithInvalidLocation_Origin_FiltersNonCargoSphereRate()
			=> AssertTestRemoveOtherOriginAndDestinationChargesWithInvalidLocation(RatingConstants.RateCategory.ORG, "CNSHA", "CNSGH", false);

		public void TestRemoveOtherOriginAndDestinationChargesWithInvalidLocation_Origin_DoesntFilterCargoSphereRate()
			=> AssertTestRemoveOtherOriginAndDestinationChargesWithInvalidLocation(RatingConstants.RateCategory.ORG, "CNSHA", "CNSGH", true);

		public void TestRemoveOtherOriginAndDestinationChargesWithInvalidLocation_Destination_FiltersNonCargoSphereRate()
			=> AssertTestRemoveOtherOriginAndDestinationChargesWithInvalidLocation(RatingConstants.RateCategory.DST, "CNSHA", "CNSGH", false);

		public void TestRemoveOtherOriginAndDestinationChargesWithInvalidLocation_Destination_DoesntFilterCargoSphereRate()
			=> AssertTestRemoveOtherOriginAndDestinationChargesWithInvalidLocation(RatingConstants.RateCategory.DST, "CNSHA", "CNSGH", true);

		void AssertTestRemoveOtherOriginAndDestinationChargesWithInvalidLocation(string rateCategory, string location1, string location2, bool isCargoSphereRate)
		{
			var chargeCode1 = Helper.ChargeCodes.New("CHG1", "Charge 1", UnitCalculator.Code);
			Factory.Save();

			string entryOrigin, entryDestination, criteriaOrigin, criteriaDestination;
			if (rateCategory == RatingConstants.RateCategory.ORG)
			{
				entryOrigin = location1;
				entryDestination = "USLAX";
				criteriaOrigin = location2;
				criteriaDestination = "USLAX";
			}
			else
			{
				entryOrigin = "USLAX";
				entryDestination = location1;
				criteriaOrigin = "USLAX";
				criteriaDestination = location2;
			}

			var mockEntryParent = new Mock<IRatingHeader>();
			mockEntryParent.Setup(x => x.DisplayInfo()).Returns("Info");

			var mockLine = new Mock<IRateLine>();
			mockLine.Setup(m => m.ChargeCode).Returns(chargeCode1);

			var mockEntry = new Mock<IRateEntry>();
			mockEntry.Setup(x => x.TI_RateCategory).Returns(rateCategory);
			mockEntry.Setup(x => x.TI_Mode).Returns(RateMode.FCL);
			mockEntry.Setup(x => x.TI_OriginLRC).Returns(entryOrigin);
			mockEntry.Setup(x => x.TI_DestinationLRC).Returns(entryDestination);
			mockEntry.Setup(x => x.ChildRateLines).Returns(new[] { mockLine.Object });
			mockEntry.Setup(x => x.RateProvider).Returns(isCargoSphereRate ? WRConstants.RateProviders.CargoSphere : null);
			mockEntry.Setup(x => x.Factory).Returns(Factory);

			mockEntry.Setup(x => x.ParentRatingHeader).Returns(mockEntryParent.Object);
			mockLine.Setup(x => x.ParentRateEntry).Returns(mockEntry.Object);

			var testRatingCriteria = new TestRatingCriteria(criteriaOrigin, criteriaDestination, 0, null, null);
			var serviceAutoRater = new ServiceAutoRater(testRatingCriteria, Factory, true);

			var lineRepository1 = new RateLinesRepository(testRatingCriteria, new List<IRateEntry>() { mockEntry.Object }, new TestLogger());

			AssertEquals("Pre-condition", "1xCHG1", lineRepository1.DebuggerDisplay);
			serviceAutoRater.RemoveOtherOriginAndDestinationChargesWithInvalidLocation_ForTestOnly(lineRepository1);

			if (isCargoSphereRate)
			{
				var message = "CargoSphere Rates should NOT do additional ORG/DST validation as CS can return results from interchangeable origins/destinations";
				AssertEquals(message, "1xCHG1", lineRepository1.DebuggerDisplay);
			}
			else
			{
				var message = "Non CargoSphere Rates should have ORG/DST charges from non-matching origins/destinations filtered out";
				AssertEquals(message, "0xCHG1", lineRepository1.DebuggerDisplay);
			}
		}

		#endregion

		#region RemoveUnmatchedOrInvalidLines

		public void TestRemoveUnmatchedOrInvalidLines_ByLocation_WhenBothLocationMatch_ShouldAcceptCharge()
		{
			var chargeCode1 = CreateTestChargeCode(ChargeCodeGroupList.Codes.Origin, code: "CHG1");
			AssertRemoveUnmatchedOrInvalidLines_ByLocation("USLAX", "AUSYD", chargeCode1, expectedDebuggerDisplay: "1xCHG1");

			var chargeCode2 = CreateTestChargeCode(ChargeCodeGroupList.Codes.Destination, code: "CHG2");
			AssertRemoveUnmatchedOrInvalidLines_ByLocation("USLAX", "AUSYD", chargeCode2, expectedDebuggerDisplay: "1xCHG2");
		}

		public void TestRemoveUnmatchedOrInvalidLines_ByLocation_WhenOriginDoesNotMatch_ShouldFilterCharge()
		{
			var chargeCode = CreateTestChargeCode(ChargeCodeGroupList.Codes.Origin);
			AssertRemoveUnmatchedOrInvalidLines_ByLocation("HKHKG", "AUSYD", chargeCode, serviceLocation: "US",
				expectedDebuggerDisplay: "0xCHG1",
				expectedRemovalMessage: "Info:RateLine Filtered CHG1--Info	reason:	Service country/region US did not match Job Origin HK.");
		}

		public void TestRemoveUnmatchedOrInvalidLines_ByLocation_WhenDestinationDoesNotMatch_ShouldFilterCharge()
		{
			var chargeCode = CreateTestChargeCode(ChargeCodeGroupList.Codes.Destination);
			AssertRemoveUnmatchedOrInvalidLines_ByLocation("USLAX", "HKHKG", chargeCode, serviceLocation: "AU",
				expectedDebuggerDisplay: "0xCHG1",
				expectedRemovalMessage: "Info:RateLine Filtered CHG1--Info	reason:	Service country/region AU did not match Job Destination HK.");
		}

		public void TestRemoveUnmatchedOrInvalidLines_ByLocation_WhenCriteriaOriginIsEmpty_ShouldFilterCharge()
		{
			var chargeCode = CreateTestChargeCode(ChargeCodeGroupList.Codes.Origin);
			// Empty origin makes criteria.Origin null
			AssertRemoveUnmatchedOrInvalidLines_ByLocation("", "AUSYD", chargeCode,
				expectedDebuggerDisplay: "0xCHG1",
				expectedRemovalMessage: "Info:RateLine Filtered CHG1--Info	reason:	job origin and/or destination are blank.");
		}

		public void TestRemoveUnmatchedOrInvalidLines_ByLocation_WhenCriteriaDestinationIsEmpty_ShouldFilterCharge()
		{
			var chargeCode = CreateTestChargeCode(ChargeCodeGroupList.Codes.Destination);
			// Empty destination makes criteria.Destination null
			AssertRemoveUnmatchedOrInvalidLines_ByLocation("USLAX", "", chargeCode,
				expectedDebuggerDisplay: "0xCHG1",
				expectedRemovalMessage: "Info:RateLine Filtered CHG1--Info	reason:	job origin and/or destination are blank.");
		}

		/// <summary>
		/// Unreal test case because criteria Origin and Destination should not be null at the same time but let's test it
		/// </summary>
		public void TestRemoveUnmatchedOrInvalidLines_ByLocation_WhenBothCriteriaLocationsAreEmpty_ShouldFilterCharge()
		{
			var chargeCode1 = CreateTestChargeCode(ChargeCodeGroupList.Codes.Origin, code: "CHG1");
			AssertRemoveUnmatchedOrInvalidLines_ByLocation("", "", chargeCode1,
				expectedDebuggerDisplay: "0xCHG1",
				expectedRemovalMessage: "Info:RateLine Filtered CHG1--Info	reason:	job origin and/or destination are blank.");

			var chargeCode2 = CreateTestChargeCode(ChargeCodeGroupList.Codes.Destination, code: "CHG2");
			AssertRemoveUnmatchedOrInvalidLines_ByLocation("", "", chargeCode2,
				expectedDebuggerDisplay: "0xCHG2",
				expectedRemovalMessage: "Info:RateLine Filtered CHG2--Info	reason:	job origin and/or destination are blank.");
		}

		void AssertRemoveUnmatchedOrInvalidLines_ByLocation(string criteriaOrigin, string criteriaDestination, AccChargeCode testChargeCode, string serviceLocation = "", string expectedDebuggerDisplay = "", string expectedRemovalMessage = "")
		{
			const string origin = "USLAX";
			const string destination = "AUSYD";

			var mockEntryParent = new Mock<IRatingHeader>();
			mockEntryParent.Setup(x => x.DisplayInfo()).Returns("Info");

			var mockLine = new Mock<IRateLine>();
			mockLine.Setup(m => m.ChargeCode).Returns(testChargeCode);

			var mockEntry = new Mock<IRateEntry>();
			mockEntry.Setup(x => x.TI_RateCategory).Returns(RatingConstants.RateCategory.DST);
			mockEntry.Setup(x => x.TI_Mode).Returns(RateMode.FCL);
			mockEntry.Setup(x => x.TI_OriginLRC).Returns(origin);
			mockEntry.Setup(x => x.TI_DestinationLRC).Returns(destination);
			mockEntry.Setup(x => x.ChildRateLines).Returns(new[] { mockLine.Object });
			mockEntry.Setup(x => x.Factory).Returns(Factory);

			mockEntry.Setup(x => x.ParentRatingHeader).Returns(mockEntryParent.Object);
			mockLine.Setup(x => x.ParentRateEntry).Returns(mockEntry.Object);

			var testRatingCriteria = new TestRatingCriteria(criteriaOrigin, criteriaDestination, 0, null, null);
			var service = new JobServiceInfo(true, testChargeCode.AC_ChargeGroup, testChargeCode.AC_ChargeSubGroup, "Some Service", 1m, locationCode: serviceLocation);
			testRatingCriteria.JobServices.Add(service);
			var testLogger = new TestLogger();
			var lineRepository = new RateLinesRepository(testRatingCriteria, new List<IRateEntry> { mockEntry.Object }, testLogger);

			var serviceAutoRater = new ServiceAutoRater(testRatingCriteria, Factory, true);
			serviceAutoRater.RemoveUnmatchedOrInvalidLines(lineRepository, testChargeCode);

			AssertEquals("Line repository debugger display should match the expected value.", expectedDebuggerDisplay, lineRepository.DebuggerDisplay);
			if (!string.IsNullOrWhiteSpace(expectedRemovalMessage))
			{
				lineRepository.Dispose(); // for closing internal logs and writing items to the main log
				AssertCollectionContains("The log should contain the expected removal message.", expectedRemovalMessage, testLogger.Infos);
			}
		}

		#endregion

		#region Helpers

		AccChargeCode CreateTestChargeCode(string chargeGroup, string code = "CHG1")
		{
			var chargeSubGroup = string.Empty;
			switch (chargeGroup)
			{
				case ChargeCodeGroupList.Codes.Destination:
					chargeSubGroup = ChargeCodeSubGroupList.Storage;
					break;

				case ChargeCodeGroupList.Codes.Origin:
					chargeSubGroup = ChargeCodeSubGroupList.Labor;
					break;
			}

			var chargeCode = Helper.ChargeCodes.New(code: code, description: $"Desc for {code}", UnitCalculator.Code, chargeGroup, chargeSubGroup);
			Factory.Save();

			return chargeCode;
		}

		#endregion
	}
}
