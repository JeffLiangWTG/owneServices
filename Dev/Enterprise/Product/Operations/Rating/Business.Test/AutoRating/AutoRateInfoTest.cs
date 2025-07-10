using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using MeasureInfo = Enterprise.MasterFiles.Business.MeasureInfo;
using MeasureType = Enterprise.Rating.Integration.MeasureType;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business.Testing
{
	public class AutoRateInfoTest : RatingTestCase
	{
		#region IsSell

		public void TestIsSell_Costing() => TestIsSell(Helper.NewCosting(Helper.NewOrgHeader()), expectedIsSell: false);

		public void TestIsSell_InterCompanyTariff() => TestIsSell(Helper.NewIntercompanyTariff(), expectedIsSell: false);

		public void TestIsSell_ClientRate() => TestIsSell(Helper.NewClientRate(Helper.NewOrgHeader()), expectedIsSell: true);

		public void TestIsSell_CompanyTariff() => TestIsSell(Helper.NewCompanyTariff(), expectedIsSell: true);

		void TestIsSell(RatingHeader ratingHeader, bool expectedIsSell)
		{
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.ULD, "AUSYD", "USLAX", "FRT", 100m);

			var criteria = new TestRatingCriteria();
			var calculationResult = CalculationResult.CreateForTest(rateEntry.RateLines[0], 10, 20, 30, criteria);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var rateInfo = new AutoRateInfo(calculationResult, parameters, Factory);
			AssertEquals("HasResult", true, rateInfo.HasResult);
			AssertEquals("IsSell", expectedIsSell, rateInfo.IsSell);
		}

		#endregion

		public void TestHasResult()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, PkgUnit.Spool, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var rateInfo1 = new AutoRateInfo("Calculation error", rateLine, parameters, Factory);
			AssertEquals("RateInfo1 should not have a result.", false, rateInfo1.HasResult);

			var calculationResult = CalculationResult.CreateForTest(rateLine, 10, 20, 30, criteria);
			var rateInfo2 = new AutoRateInfo(calculationResult, parameters, Factory);
			AssertEquals("RateInfo2 should have a result.", true, rateInfo2.HasResult);

			var rateInfo3 = new AutoRateInfo(Factory, rateLine);
			AssertEquals("RateInfo3 should not have a result initially.", false, rateInfo3.HasResult);

			rateInfo3.IsFromRatesService = true;
			AssertEquals("RateInfo3 should have a result after setting IsFromRatesService to true.", true, rateInfo3.HasResult);
		}

		public void TestAddAmount_CalculationDescription()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, PkgUnit.Spool, "AUD");
			var rateLine2 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, PkgUnit.Pallet, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 10, 20, 30, criteria);
			var calculationResult2 = CalculationResult.CreateForTest(rateLine2, 40, 50, 60, criteria);

			var autoRateInfo1 = new AutoRateInfo(calculationResult1, parameters, Factory);
			var autoRateInfo2 = new AutoRateInfo(calculationResult2, parameters, Factory);

			AssertEquals("AutoRateInfo1", "Minimum AUD 20.00", autoRateInfo1.CalculationDescription);
			AssertEquals("AutoRateInfo2", "Minimum AUD 50.00", autoRateInfo2.CalculationDescription);

			autoRateInfo1.AddAmount(autoRateInfo2);
			AssertEquals("Minimum AUD 20.00, Minimum AUD 50.00", autoRateInfo1.CalculationDescription);
		}

		public void TestAddAmountsWithinSameRateEntryWithMinAndMax()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResultMin1 = CalculationResult.CreateForTest(rateLine1, 100, 110, decimal.MaxValue, criteria);
			var calculationResultMin2 = CalculationResult.CreateForTest(rateLine1, 200, 120, decimal.MaxValue, criteria);
			var calculationResultAgentMin1 = CalculationResult.CreateForTest(rateLine1, 100, 110, decimal.MaxValue, criteria);
			var calculationResultAgentMin2 = CalculationResult.CreateForTest(rateLine1, 200, 400, decimal.MaxValue, criteria);

			var infoMin1 = new AutoRateInfo(calculationResultMin1, calculationResultAgentMin1, parameters, Factory);
			var infoMin2 = new AutoRateInfo(calculationResultMin2, calculationResultAgentMin2, parameters, Factory);

			AssertEquals("Minimum value should be used", 110m, infoMin1.Amount);
			AssertEquals("Flat value should be used as exceeds minimum", 200m, infoMin2.Amount);
			AssertEquals(110m, infoMin1.AgentAmount);
			AssertEquals(400m, infoMin2.AgentAmount);

			Assert(infoMin1.IsCalculatedWithMinimumRate);

			infoMin1.AddAmount(infoMin2);

			AssertEquals("Sum of two values should apply", 310m, infoMin1.Amount);
			Assert(!infoMin1.IsCalculatedWithMinimumRate);

			AssertEquals("Sum of two values should apply", 510m, infoMin1.AgentAmount);

			var calculationResultMax1 = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, 400, criteria);
			var calculationResultMax2 = CalculationResult.CreateForTest(rateLine1, 200, decimal.MinValue, 290, criteria);
			var calculationResultAgentMax1 = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, 400, criteria);
			var calculationResultAgentMax2 = CalculationResult.CreateForTest(rateLine1, 200, decimal.MinValue, 290, criteria);

			var infoMax1 = new AutoRateInfo(calculationResultMax1, calculationResultAgentMax1, parameters, Factory);
			var infoMax2 = new AutoRateInfo(calculationResultMax2, calculationResultAgentMax2, parameters, Factory);

			AssertEquals("Flat value should be used", 100m, infoMax1.Amount);
			AssertEquals("Flat value should be used", 200m, infoMax2.Amount);
			AssertEquals("Flat value should be used", 100m, infoMax1.AgentAmount);
			AssertEquals("Flat value should be used", 200m, infoMax2.AgentAmount);

			infoMax1.AddAmount(infoMax2);

			AssertEquals("Sum of two values should apply", 300m, infoMax1.Amount);
			AssertEquals("Sum of two values should apply", 300m, infoMax1.AgentAmount);
		}

		public void TestMultiply()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, decimal.MaxValue, criteria);

			var info = new AutoRateInfo(calculationResult, parameters, Factory);
			AssertEquals(100m, info.Amount);

			info.Multiply(0.5m);
			AssertEquals(50m, info.Amount);

			rateLine1.TL_RX_NKCurrency = "JPY";
			calculationResult = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, decimal.MaxValue, criteria);
			info = new AutoRateInfo(calculationResult, parameters, Factory);

			info.Multiply(0.4571m);
			AssertEquals(46m, info.Amount);
		}

		public void TestAutoRatedChargeableValues()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.AddRateLine(Factory.New<AccChargeCode>(), UnitCalculator.Code, "CN");

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			ratingCriteria.AutoRatedFor = new Collection<IBusiness>() { rateEntry };
			// Is a container count with no containers really needed for this test?
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetQuantity(MeasureType.ContainerCount, 0, string.Empty);
			ratingCriteria.RateableMeasures = measures;
			ratingCriteria.ChargeCodeGroups = new ChargeCodeGroupCollection();

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var calculationResult = new CalculationResult(rateLine, new CalculatorOutput(new List<PaymentBasis> { parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, "AUD"), new Quantity(5, "CN")) }));

			var autoRateInfo = new AutoRateInfo(calculationResult, parameters, Factory);

			AssertEquals(5m, autoRateInfo.GetChargeableFromBasisTest.Amount);
			AssertEquals("CN", autoRateInfo.GetChargeableFromBasisTest.Unit);
		}

		public void TestIsCalculatedWithMinimumRate()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, decimal.MaxValue, criteria);

			var info = new AutoRateInfo(calculationResult, parameters, Factory);
			AssertEquals(100m, info.Amount);
			AssertEquals(false, info.IsCalculatedWithMinimumRate);

			info.Bases.Add(criteria.CreatePaymentBasis(RateInfo.CreateMIN(500, "AUD"), default));
			AssertEquals(500m, info.Amount);
			AssertEquals(true, info.IsCalculatedWithMinimumRate);

			info.Bases.Add(criteria.CreatePaymentBasis(RateInfo.CreateFLT(600, "AUD"), default));
			AssertEquals(700m, info.Amount);
			AssertEquals(false, info.IsCalculatedWithMinimumRate);
		}

		public void TestExchangeRates()
		{
			using (_Rating.Start(new LoggerDecorator()))
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				var iNRRate = Helper.NewExchangeRate("INR", Core.Constants.ExchangeRateTypes.Code.SellRate, 25m);

				var costing = Helper.NewCosting(Helper.NewOrgHeader());
				var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
				var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "INR");

				var criteria = new TestRatingCriteria();
				criteria.CurrencyConverter = Helper.CurrencyConverter;
				var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

				var calculationResult = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, decimal.MaxValue, criteria);
				var calculationAgentResult = CalculationResult.CreateForTest(rateLine1, 200, decimal.MinValue, decimal.MaxValue, criteria);

				var info = new AutoRateInfo(calculationResult, calculationAgentResult, parameters, Factory);

				AssertEquals(100M, info.Amount);
				AssertEquals(4M, info.LocalAmount);
				AssertEquals(200M, info.AgentAmount);

				iNRRate.Delete();

				AssertEquals(100M, info.Amount);
				AssertEquals(0M, info.LocalAmount);
				AssertEquals(200M, info.AgentAmount);
			}
		}

		public void TestSellChargesFilter()
		{
			var chargeCode1 = Helper.ChargeCodes.New("NCLORG", "Non Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var chargeCode2 = Helper.ChargeCodes.New("NCLFRT", "Non Consol Level Freight", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var chargeCode3 = Helper.ChargeCodes.New("NCLDST", "Non Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			var chargeCode4 = Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var chargeCode5 = Helper.ChargeCodes.NewConsolChargeCode("CLFRT", "Consol Level Freight", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var chargeCode6 = Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			chargeCode2.AC_IsGroupageCharge = false;
			Factory.Save();

			AssertEquals("Precondition", false, chargeCode1.AC_IsGroupageCharge);
			AssertEquals("Precondition", false, chargeCode2.AC_IsGroupageCharge);
			AssertEquals("Precondition", false, chargeCode3.AC_IsGroupageCharge);
			AssertEquals("Precondition", true, chargeCode4.AC_IsGroupageCharge);
			AssertEquals("Precondition", true, chargeCode5.AC_IsGroupageCharge);
			AssertEquals("Precondition", true, chargeCode6.AC_IsGroupageCharge);

			var clientRate = Helper.NewClientRate(NewClient);
			var orgEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.LSE, "AU", "US", chargeCode1.AC_Code, 10m);
			var frtEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AU", "US", chargeCode2.AC_Code, 20m);
			var dstEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LSE, "AU", "US", chargeCode3.AC_Code, 30m);

			orgEntry.AddRateLine(chargeCode4).GetCalculator<FlatCalculator>().BaseRate = 40m;
			frtEntry.AddRateLine(chargeCode5).GetCalculator<FlatCalculator>().BaseRate = 50m;
			dstEntry.AddRateLine(chargeCode6).GetCalculator<FlatCalculator>().BaseRate = 60m;
			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 100m, 1m, NewClient);
			criteria.ChargeCodeGroups.SellChargesFilter = ChargeCodeFilter.AutorateAll;

			var message = "Expected to AutoRate all charges";
			var expectedCharges = new[] { chargeCode1.PK, chargeCode2.PK, chargeCode3.PK, chargeCode4.PK, chargeCode5.PK, chargeCode6.PK };
			var actualCharges = autoRater.AutoRate(criteria, CostSell.Revenue).RateInfoCollection.Select(x => x.ChargeCode.PK);

			AssertContainsExactElementsInAnyOrder(message, expectedCharges, actualCharges);

			expectedCharges = Array.Empty<ZGuid>();
			criteria.ChargeCodeGroups.SellChargesFilter = ChargeCodeFilter.AutorateNothing;
			actualCharges = autoRater.AutoRate(criteria, CostSell.Revenue).RateInfoCollection.Select(x => x.ChargeCode.PK);

			AssertContainsExactElementsInAnyOrder(message, expectedCharges, actualCharges);

			expectedCharges = new[] { chargeCode1.PK, chargeCode2.PK, chargeCode3.PK };
			criteria.ChargeCodeGroups.SellChargesFilter = ChargeCodeFilter.AutorateNonConsolLevelOnly;
			actualCharges = autoRater.AutoRate(criteria, CostSell.Revenue).RateInfoCollection.Select(x => x.ChargeCode.PK);

			AssertContainsExactElementsInAnyOrder(message, expectedCharges, actualCharges);

			expectedCharges = new[] { chargeCode4.PK, chargeCode5.PK, chargeCode6.PK };
			criteria.ChargeCodeGroups.SellChargesFilter = ChargeCodeFilter.AutorateConsolLevelOnly;
			actualCharges = autoRater.AutoRate(criteria, CostSell.Revenue).RateInfoCollection.Select(x => x.ChargeCode.PK);

			AssertContainsExactElementsInAnyOrder(message, expectedCharges, actualCharges);
		}

		public void TestCanBeMergedWith_IgnoresMergeOrder()
		{
			var chargeCode = Factory.New<AccChargeCode>();

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG, "INR");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, decimal.MaxValue, criteria);
			var calculationResult2 = CalculationResult.CreateForTest(rateLine1, 200, decimal.MinValue, decimal.MaxValue, criteria);

			var autoRateInfo1 = new AutoRateInfo(calculationResult1, parameters, Factory);
			autoRateInfo1.Attributes.Add(JobChargeAttribTypeList.Codes.ItemsToRate, "10");
			autoRateInfo1.Attributes.Add(JobChargeAttribTypeList.Codes.Product, "PROD");

			var autoRateInfo2 = new AutoRateInfo(calculationResult2, parameters, Factory);
			autoRateInfo1.Attributes.Add(JobChargeAttribTypeList.Codes.ItemsToRate, "10");

			AssertEquals(false, autoRateInfo1.CanBeMergedWith(autoRateInfo2));
			AssertEquals(false, autoRateInfo2.CanBeMergedWith(autoRateInfo1));
		}

		public void TestRateInfoMerge()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 40, 30, 60, criteria);
			var calculationResult1Agent = CalculationResult.CreateForTest(rateLine1, 4000, 3000, 6000, criteria);
			var info1 = new AutoRateInfo(calculationResult1, calculationResult1Agent, parameters, Factory);

			var calculationResult2 = CalculationResult.CreateForTest(rateLine1, 50, 20, 70, criteria);
			var calculationResult2Agent = CalculationResult.CreateForTest(rateLine1, 5000, 2000, 7000, criteria);
			var info2 = new AutoRateInfo(calculationResult2, calculationResult2Agent, parameters, Factory);

			info1.AddAmount(info2);

			var calculationResult11 = CalculationResult.CreateForTest(rateLine1, 10, 40, 60, criteria);
			var calculationResult11Agent = CalculationResult.CreateForTest(rateLine1, 1000, 4000, 6000, criteria);
			var info11 = new AutoRateInfo(calculationResult11, calculationResult11Agent, parameters, Factory);

			var calculationResult12 = CalculationResult.CreateForTest(rateLine1, 20, 30, 70, criteria);
			var calculationResult12Agent = CalculationResult.CreateForTest(rateLine1, 2000, 3000, 7000, criteria);
			var info12 = new AutoRateInfo(calculationResult12, calculationResult12Agent, parameters, Factory);

			info11.AddAmount(info12);

			AssertEquals(90m, info1.Amount);
			AssertEquals(9000m, info1.AgentAmount);

			AssertEquals(70m, info11.Amount);
			AssertEquals(7000m, info11.AgentAmount);
		}

		public void TestRateInfoMergeDebtorPK()
		{
			var chargeCode = Factory.New<AccChargeCode>();

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG, "INR");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, decimal.MaxValue, criteria);
			var calculationResult2 = CalculationResult.CreateForTest(rateLine1, 200, decimal.MinValue, decimal.MaxValue, criteria);

			var autoRateInfo1 = new AutoRateInfo(calculationResult1, parameters, Factory);
			var autoRateInfo2 = new AutoRateInfo(calculationResult2, parameters, Factory);

			autoRateInfo1.DebtorOverridePK = new ZGuid("00000000-0000-0000-0000-000000000001");
			autoRateInfo2.DebtorOverridePK = new ZGuid("00000000-0000-0000-0000-000000000001");
			AssertEquals(true, autoRateInfo1.CanBeMergedWith(autoRateInfo2));

			autoRateInfo2.DebtorOverridePK = ZGuid.Empty;
			AssertEquals(true, autoRateInfo1.CanBeMergedWith(autoRateInfo2));
			AssertEquals(true, autoRateInfo2.CanBeMergedWith(autoRateInfo1));

			autoRateInfo2.DebtorOverridePK = new ZGuid("00000000-0000-0000-0000-000000000002");
			AssertEquals(false, autoRateInfo1.CanBeMergedWith(autoRateInfo2));
		}

		public void TestAddAttributes()
		{
			var collection = new AutoRateInfoCollection(Factory);

			var ocartCode = Helper.ChargeCodes["OCART"];
			var info1 = collection.AddNew(ocartCode, "AUD", 0);
			info1.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Zone 1");
			info1.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Zone 2");

			var info2 = collection.AddNew(ocartCode, "AUD", 40);
			info2.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Zone 3");

			var info3 = collection.AddNew(ocartCode, "AUD", 80);
			info3.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Zone 4");

			info1.AddAttributes(info2);

			var expectedAttributesAfterFirstMerge = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Zone 3")
			};
			AssertContainsExactElementsInAnyOrder(
				"Info1 is dummy so, Info2 overwrites its attributes",
				expectedAttributesAfterFirstMerge,
				info1.Attributes.Attributes
			);

			info1.AddAmount(info2);
			info1.AddAttributes(info3);

			var expectedAttributesAfterSecondMerge = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Zone 3")
			};
			AssertContainsExactElementsInAnyOrder(
				"Info1 is not dummy anymore, so, it should not add new attributes",
				expectedAttributesAfterSecondMerge,
				info1.Attributes.Attributes
			);
		}

		public void TestAddAmountWithDescriptions()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			var rateLine2 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			var rateLine3 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 40, 30, 60, criteria);
			var calculationResult2 = CalculationResult.CreateForTest(rateLine2, 40, 30, 60, criteria);
			var calculationResult3 = CalculationResult.CreateForTest(rateLine3, 40, 30, 60, criteria);

			var info1 = new AutoRateInfo(calculationResult1, parameters, Factory);
			var info2 = new AutoRateInfo(calculationResult2, parameters, Factory);

			info1.AddAmount(info2);
			AssertEquals("FRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00", info1.SingleLineDescription);

			info2.AddAmount(info1);
			AssertEquals("FRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00", info2.SingleLineDescription);

			info1.AddAmount(info2);
			AssertEquals("FRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00", info1.SingleLineDescription);

			var info3 = new AutoRateInfo(calculationResult3, parameters, Factory);
			info1.AddAmount(info3);
			AssertEquals("FRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00\n\tFRT: Base Rate AUD 40.00", info1.SingleLineDescription);
		}

		public void TestAutoRatedForString()
		{
			var autoRateInfo = new AutoRateInfo(Factory);

			Assert("Cannot determine Auto Rated For without criteria", autoRateInfo.AutoRatedForString.IsEmpty);

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			ratingCriteria.AutoRatedFor = new Collection<IBusiness> { rateEntry };
			// Is a container count with no containers really needed for this test?
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetQuantity(MeasureType.ContainerCount, 0, string.Empty);
			ratingCriteria.RateableMeasures = measures;
			ratingCriteria.ChargeCodeGroups = new ChargeCodeGroupCollection();

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));
			var calculationResult = CalculationResult.CreateForTest(rateLine, 0m, 0m, 0m, parameters.Criteria);

			autoRateInfo = new AutoRateInfo(calculationResult, parameters, Factory);

			AssertEquals(rateEntry.HumanReadableName, autoRateInfo.AutoRatedForString);
		}

		public void TestAppendUniqueInvoiceLineDescriptions()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "NEW1";
			chargeCode.AC_Desc = "New Charge Description";

			Func<string, AutoRateInfo> createAutoRateInfo = desc => new AutoRateInfo(Factory)
			{
				ChargeCode = chargeCode,
				InvoiceLineDescription = desc
			};

			var info = createAutoRateInfo(chargeCode.AC_Desc);
			AssertEquals("Precondition", chargeCode.AC_Desc, info.InvoiceLineDescription);

			var newInfo = createAutoRateInfo("Overriden");
			info.AppendUniqueInvoiceLineDescriptions(newInfo);

			AssertEquals("Expected to only include the overriden description", "Overriden", info.InvoiceLineDescription);

			newInfo = createAutoRateInfo("Overriden");
			info.AppendUniqueInvoiceLineDescriptions(newInfo);

			AssertEquals("Should not repeat overriden description ", "Overriden", info.InvoiceLineDescription);

			newInfo = createAutoRateInfo("");
			info.AppendUniqueInvoiceLineDescriptions(newInfo);

			AssertEquals("Should not create new line for empty description", "Overriden", info.InvoiceLineDescription);

			newInfo = createAutoRateInfo("Another overriden description");
			info.AppendUniqueInvoiceLineDescriptions(newInfo);

			AssertEquals("Overriden\r\nAnother overriden description", info.InvoiceLineDescription);

			var appendDescription = "Another overriden description".PadRight(3000, '0');
			newInfo = createAutoRateInfo(appendDescription);
			info.AppendUniqueInvoiceLineDescriptions(newInfo);

			AssertEquals("Overriden\r\nAnother overriden description\r\n" + appendDescription, info.InvoiceLineDescription);
		}

		public void TestAppendCalculationDescription()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "NEW1";

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 4, decimal.MinValue, decimal.MaxValue, criteria);
			var info1 = new AutoRateInfo(calculationResult1, parameters, Factory);

			AssertContains("Only one Rate Info", "NEW1: Base Rate AUD 4.00", info1.Description);

			var calculationResult2 = CalculationResult.CreateForTest(rateLine1, 5, decimal.MinValue, decimal.MaxValue, criteria);
			var info2 = new AutoRateInfo(calculationResult2, parameters, Factory);

			info1.AddAmount(info2);
			AssertContains("Expected description formed from 2 rate infos", @"This charge is calculated from multiple rates

NEW1: Base Rate AUD 4.00", info1.Description);
			AssertContains("Expected description formed from 2 rate infos", @"NEW1: Base Rate AUD 5.00", info1.Description);

			var calculationResult3 = CalculationResult.CreateForTest(rateLine1, 10, decimal.MinValue, decimal.MaxValue, criteria);
			var info3 = new AutoRateInfo(calculationResult3, parameters, Factory);
			info1.AddAmount(info3);

			AssertContains("Expected description formed from 3 rate infos", @"NEW1: Base Rate AUD 10.00", info1.Description);
		}

		public void TestAppendCalculationDescriptionWithMinimums()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 4, 10, decimal.MaxValue, criteria);
			var info1 = new AutoRateInfo(calculationResult1, parameters, Factory);

			info1.Description = "Minimum Rate 10 used";
			AssertEquals("Precondition", (ZDecimal)10m, info1.Amount);

			var calculationResult2 = CalculationResult.CreateForTest(rateLine1, 4, 0, decimal.MaxValue, criteria);
			var newInfo = new AutoRateInfo(calculationResult2, parameters, Factory);
			newInfo.Description = "\r\nAnother Rate";

			info1.AddAmount(newInfo);

			AssertEquals((ZDecimal)14m, info1.Amount);
			AssertContains(@"This charge is calculated from multiple rates

Minimum Rate 10 used
Another Rate", info1.Description);
		}

		public void TestAppendCalculationDescriptionWithMaximums()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine1 = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 100, decimal.MinValue, 30, criteria);
			var info1 = new AutoRateInfo(calculationResult1, parameters, Factory);
			info1.Description = "Maximum Rate 30 used";

			AssertEquals("Precondition", (ZDecimal)30m, info1.Amount);

			var calculationResult2 = CalculationResult.CreateForTest(rateLine1, 15, decimal.MinValue, decimal.MaxValue, criteria);

			var newInfo = new AutoRateInfo(calculationResult2, parameters, Factory);
			newInfo.Description = "\r\nAnother Rate";

			info1.AddAmount(newInfo);

			AssertEquals("Should add 15", (ZDecimal)45m, info1.Amount);
			AssertContains(@"This charge is calculated from multiple rates

Maximum Rate 30 used
Another Rate", info1.Description);
		}

		#region GetDescription

		public void TestGetDescription()
		{
			var helper = new TestHelper(Factory);

			var localDebtor = helper.NewOrgHeader("LOCALDEBTOR", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			AssertEquals("Local Debtor", true, localDebtor.IsLocalCountry);
			var nonLocalDebtor = helper.NewOrgHeader("NONLOCALDEBT", "USLAX");
			AssertEquals("Non Local Dbtor", false, nonLocalDebtor.IsLocalCountry);
			var localClient = helper.NewOrgHeader("LOCALCLIENT", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			AssertEquals("Local Client", true, localClient.IsLocalCountry);
			var nonLocalClient = helper.NewOrgHeader("NONLOCALCLNT", "USLAX");
			AssertEquals("NonLocal Client", false, nonLocalClient.IsLocalCountry);

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var clientRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AU", "US", "BAF", 10m);
			var clientRateLine = (RateLine)clientRateEntry.RateLines.Single();
			clientRateLine.TL_RateDesc = "BAF Updated Description";
			clientRateLine.TL_RateDescLocal = "BAF Updated Local Description åœ°æ–¹";

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(clientRateLine, 10, decimal.MinValue, decimal.MaxValue, criteria);
			var autoRateInfo = new AutoRateInfo(calculationResult, parameters, Factory);

			// Registry is disable
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, localDebtor, localClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, localDebtor, nonLocalClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, localDebtor, null, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, localClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, nonLocalClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, null, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, null, localClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, null, nonLocalClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: false, JobInvoicingConsumerTypes.Consol.Code, null, null, autoRateInfo, "BAF Updated Description");
			foreach (var jobType in new[] { JobInvoicingConsumerTypes.OneOffQuotation.Code, JobInvoicingConsumerTypes.QuotedBooking.Code, JobInvoicingConsumerTypes.Shipment.Code })
			{
				AssertGetDescription(enableLocalDescRegistry: false, jobType, localDebtor, localClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: false, jobType, localDebtor, nonLocalClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: false, jobType, localDebtor, null, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: false, jobType, nonLocalDebtor, localClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: false, jobType, nonLocalDebtor, nonLocalClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: false, jobType, nonLocalDebtor, null, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: false, jobType, null, localClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: false, jobType, null, nonLocalClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: false, jobType, null, null, autoRateInfo, "BAF Updated Description");
			}

			// Registry is enable
			// Non OOQ, BWQ, QB and shipment
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, localDebtor, localClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, localDebtor, nonLocalClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, localDebtor, null, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, localClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, nonLocalClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, null, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, null, localClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, null, nonLocalClient, autoRateInfo, "BAF Updated Description");
			AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, null, null, autoRateInfo, "BAF Updated Description");
			// Registry ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is true
			var applyLocalChargeCodeDescriptionDefaultToForeignDebtors = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors");
			using (applyLocalChargeCodeDescriptionDefaultToForeignDebtors.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors is true", true, applyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value);

				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, localDebtor, localClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, localDebtor, nonLocalClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, localDebtor, null, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, localClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, nonLocalClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, nonLocalDebtor, null, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, null, localClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, null, nonLocalClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, JobInvoicingConsumerTypes.Consol.Code, null, null, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
			}
			// OOQ, BWQ, QB and shipment - use Debtor then fallback to LocalClient
			foreach (var jobType in new[] { JobInvoicingConsumerTypes.OneOffQuotation.Code, JobInvoicingConsumerTypes.QuotedBooking.Code, JobInvoicingConsumerTypes.Shipment.Code })
			{
				AssertGetDescription(enableLocalDescRegistry: true, jobType, localDebtor, localClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, jobType, localDebtor, nonLocalClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, jobType, localDebtor, null, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, jobType, nonLocalDebtor, localClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: true, jobType, nonLocalDebtor, nonLocalClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: true, jobType, nonLocalDebtor, null, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: true, jobType, null, localClient, autoRateInfo, "BAF Updated Local Description åœ°æ–¹");
				AssertGetDescription(enableLocalDescRegistry: true, jobType, null, nonLocalClient, autoRateInfo, "BAF Updated Description");
				AssertGetDescription(enableLocalDescRegistry: true, jobType, null, null, autoRateInfo, "BAF Updated Description");
			}
		}

		void AssertGetDescription(bool enableLocalDescRegistry, string jobType, OrgHeader debtor, OrgHeader localClient, AutoRateInfo autoRateInfo, string expectedDescription)
		{
			var enableLocalChargeCodeDescription = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableLocalDescRegistry))
			{
				AssertEquals
				(
					$"EnableLocalDescRegistry: {enableLocalDescRegistry}, jobType: {jobType}, debtor: {GetOrgHeaderInfo(debtor)}, localClient: {GetOrgHeaderInfo(localClient)}",
					expectedDescription,
					autoRateInfo.GetInvoiceLineDescriptionOrLocalDescription(jobType, debtor, localClient).Item1
				);
			}
		}

		static string GetOrgHeaderInfo(OrgHeader orgHeader)
		{
			if (orgHeader == null)
			{
				return "NULL";
			}
			else if (orgHeader.IsLocalCountry)
			{
				return "Local";
			}
			else
			{
				return "NonLocal";
			}
		}

		#endregion

		public void TestCorrectChargeCodeForCurrentCompany()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, "CN");

			var calculator = CalculatorFactory.GetCalculator(rateLine);

			var ratingCriteria = new RatingCriteria(null, Factory);
			ratingCriteria.ValuesCanBeSet = true;
			ratingCriteria.AutoRatedFor = new Collection<IBusiness>() { rateEntry };
			// Is a container count with no containers really needed for this test?
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			measures.SetQuantity(MeasureType.ContainerCount, 0, string.Empty);
			ratingCriteria.RateableMeasures = measures;
			ratingCriteria.ChargeCodeGroups = new ChargeCodeGroupCollection();

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(ratingCriteria, new FreightAutoRater(new RatingContext()));

			var calculationResult = new CalculationResult(rateLine, new CalculatorOutput(new List<PaymentBasis>()));

			var autoRateInfo = new AutoRateInfo(calculationResult, parameters, Factory);
			AssertEquals("Should get correct chargeCode for current company", normalChargeCodeLinked.PK, autoRateInfo.ChargeCode.PK);
		}

		public void TestCalculationLogs()
		{
			var rateInfo = new AutoRateInfo(Factory);
			AssertEquals("Empty by default", true, rateInfo.CalculationLogs.IsEmpty);

			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(line, 1m, 2m, 3m, criteria);

			AssertEquals("Precondition", true, calculationResult.FreightChargeCodeCalculationLog.IsEmpty);
			AssertEquals("Calculation log was empty => not added", true, rateInfo.CalculationLogs.IsEmpty);

			calculationResult = CalculationResult.CreateForTest(line, 1m, 2m, 3m, criteria);
			calculationResult.FreightChargeCodeCalculationLog.BaseRate = 100m;

			rateInfo = new AutoRateInfo(calculationResult, parameters, Factory);
			AssertEquals("Precondition", false, calculationResult.FreightChargeCodeCalculationLog.IsEmpty);
			AssertEquals("Calculation log not null and not empty => added", false, rateInfo.CalculationLogs.IsEmpty);
		}

		public void TestAdditionalJobRef()
		{
			var rateInfo = new AutoRateInfo(Factory);
			AssertEquals("Empty by default", true, rateInfo.AdditionalJobRef.IsEmpty);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("DST");
			rateEntry.JobServiceForSpotEntry = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, "OSTOR", "Store", 1m);
			rateEntry.TI_ContractNumber = "REF90832";
			var rateLine = rateEntry.AddRateLine("OSTOR", UnitCalculator.Code, "SV");

			var calculator = CalculatorFactory.GetCalculator(rateLine);

			var criteria = new TestRatingCriteria();
			criteria.AutoRatedFor = new Collection<IBusiness> { rateEntry };
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(rateLine, 1m, 1m, 1m, criteria);

			rateInfo = new AutoRateInfo(calculationResult, parameters, Factory);

			AssertEquals("Should match the service reference", "REF90832", rateInfo.AdditionalJobRef);
		}

		public void TestProviderPK_WhenChargeCodeIsForService_Cost()
		{
			var destinationStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESTOR", "Dest Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);
			var costingProvider = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(costingProvider);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.FCL, "NZ", "AU");
			var rateLine = rateEntry.AddRateLine(destinationStorageChargeCode, FlatCalculator.Code);
			var calculator = CalculatorFactory.GetCalculator(rateLine);
			var serviceContractor = Helper.NewOrgHeader();

			var criteria = new TestRatingCriteria();
			criteria.RateTypeToUse = RateType.Forwarding;
			criteria.Creditors = Creditors.New(OrgWithSource.New(costingProvider, new List<string>() { "Provider" }));
			criteria.AutoRatedFor = new Collection<IBusiness> { rateEntry };
			var service = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Some Service", 1m, null, serviceContractor);
			service.IsContractorCreditor = true;
			criteria.JobServices.Add(service);

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				Assert("PRE: costingProvider is in provider list", criteria.GetTransportProvidersAndContractorPKs(destinationStorageChargeCode).Contains(costingProvider.PK));
				Assert("PRE", _Rating.Cost);

				var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
				var calculationResult = CalculationResult.CreateForTest(rateLine, 100m, 100m, 100m, criteria);

				var rateInfo1 = new AutoRateInfo(calculationResult, parameters, Factory);

				service.IsContractorCreditor = false;
				var rateInfo2 = new AutoRateInfo(calculationResult, parameters, Factory);

				AssertEquals("ProviderPK should be the service contractor when IsContractorCreditor, not the costing provider", serviceContractor.PK, rateInfo1.ProviderPK);
				AssertEquals("ProviderPK should be the costing provider when IsContractorCreditor is false", costingProvider.PK, rateInfo2.ProviderPK);
			}
		}

		public void TestProviderPK_WhenChargeCodeIsForService_Sell()
		{
			var destinationStorageChargeCode = Helper.ChargeCodes.NewConsolChargeCode("DESTOR", "Dest Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage);
			var rateProvider = Helper.NewOrgHeader();
			var supplier = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(rateProvider);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.FCL, "NZ", "AU");
			rateEntry.TI_OH_Supplier = supplier.PK;
			var rateLine = rateEntry.AddRateLine(destinationStorageChargeCode, FlatCalculator.Code);
			var calculator = CalculatorFactory.GetCalculator(rateLine);
			var serviceContractor = Helper.NewOrgHeader();

			var criteria = new TestRatingCriteria();
			criteria.RateTypeToUse = RateType.Forwarding;
			criteria.Creditors = Creditors.New(OrgWithSource.New(rateProvider, new List<string>() { "Provider" }), OrgWithSource.New(supplier, new List<string>() { "Supplier" }));
			criteria.AutoRatedFor = new Collection<IBusiness> { rateEntry };
			var service = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage, "Some Service", 1m, null, serviceContractor);
			service.IsContractorCreditor = true;
			criteria.JobServices.Add(service);

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();

				Assert("PRE: rate's supplier is in provider list", criteria.GetTransportProvidersAndContractorPKs(destinationStorageChargeCode).Contains(supplier.PK));
				Assert("PRE", _Rating.Sell);

				var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
				var calculationResult = CalculationResult.CreateForTest(rateLine, 100m, 100m, 100m, criteria);

				var rateInfo1 = new AutoRateInfo(calculationResult, parameters, Factory);

				service.IsContractorCreditor = false;
				var rateInfo2 = new AutoRateInfo(calculationResult, parameters, Factory);

				AssertEquals("ProviderPK should be the service contractor when IsContractorCreditor, not the rate's supplier", serviceContractor.PK, rateInfo1.ProviderPK);
				AssertEquals("ProviderPK should be the rate's supplier when IsContractorCreditor is false", supplier.PK, rateInfo2.ProviderPK);
			}
		}

		#region TestWarehouseRateDescription

		public void TestWarehouseRateDescription()
		{
			AssertWarehouseRateDescription(RatingConstants.RateCategory.WHS);
			AssertWarehouseRateDescription(RatingConstants.RateCategory.TRW);
			AssertWarehouseRateDescription(RatingConstants.RateCategory.TWU);
		}

		void AssertWarehouseRateDescription(string ratingCategory)
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var supplier = Factory.New<OrgHeader>();

			var rateEntry = Factory.New<CompanyTariff>().EntryCollections[ratingCategory].LazyLoadingCollection.AddNew();
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_RateCalculator = CalculatorForTest.Code;
			rateLine.TL_AC = chargeCode.PK;
			rateEntry.TI_OH_Supplier = supplier.PK;

			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse[WhsWarehouseSchema.WW_WarehouseCode] = "W1";
			rateEntry.TI_WW_Warehouse = warehouse.PK;

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			rateLine.TL_OP_ProductNumber = part1.PK;

			rateEntry.TI_RS_NKServiceLevel_NI = "STD";
			rateEntry.TI_RH_NKCommodityCode = "GEN";

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(calculationResult, parameters, Factory);
			var description = info.Description;
			AssertContains("Warehouse:", description);
			if (ratingCategory == RatingConstants.RateCategory.WHS)
			{
				AssertContains("Product:", description);
			}
			else
			{
				AssertNotContains("Product:", description);
			}
			AssertContains("Service Level:", description);
			AssertContains("Commodity Code:", description);
			AssertContains("Svc. Provider:", description);
			AssertNotContains("Leg:", description);
		}

		#endregion

		#region TestChargeNotes

		public void TestChargeNotes()
		{
			var publicNote = "ChargeInformationNoteText";
			var internalNote = "ChargeInternalNoteText";
			var expectedPublic = $"{DescriptionHelpers.FormatWithTab("Public Note:")}{publicNote}";
			var expectedInternal = $"{DescriptionHelpers.FormatWithTab("Internal Note:")}{internalNote}";

			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, new TestRatingCriteria());
			var info = new AutoRateInfo(result, parameters, Factory);

			AssertEquals(false, info.Description.Contains(expectedPublic));
			AssertEquals(false, info.Description.Contains(expectedInternal));

			rateLine.ChargeInformationNoteText = publicNote;
			info = new AutoRateInfo(result, parameters, Factory);
			AssertEquals(true, info.Description.Contains(expectedPublic));
			AssertEquals(false, info.Description.Contains(expectedInternal));

			rateLine.ChargeInternalNoteText = internalNote;
			info = new AutoRateInfo(result, parameters, Factory);
			AssertEquals(true, info.Description.Contains(expectedPublic));
			AssertEquals(true, info.Description.Contains(expectedInternal));

			var rateLineConditionDescription = DescriptionHelpers.FormatWithTab("Rate Line Condition:");

			rateLine.TL_Condition = RateLineConditions.ForwardingAndBrokerage;
			info = new AutoRateInfo(result, parameters, Factory);
			AssertEquals(true, info.Description.Contains($"{rateLineConditionDescription}{RateLineConditions.Descriptions.ForwardingAndBrokerage}"));

			rateLine.TL_Condition = RateLineConditions.UserDefined;
			rateLine.TL_ConditionalExpression = "MOD=FSA";
			info = new AutoRateInfo(result, parameters, Factory);
			AssertEquals(true, info.Description.Contains($"{rateLineConditionDescription}MOD=FSA"));
		}

		// Fails here
		public void TestChargeNotes_ChargeType()
		{
			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var rateLine1 = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			var rateLine2 = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			var result1 = CalculationResult.CreateForTest(rateLine1, 10m, 5m, 20m, new TestRatingCriteria());
			var result2 = CalculationResult.CreateForTest(rateLine2, 10m, 5m, 20m, new TestRatingCriteria());
			var info1 = new AutoRateInfo(result1, parameters, Factory);
			var info2 = new AutoRateInfo(result2, parameters, Factory);

			rateLine1.TL_FeeChargeType = "DWY";
			rateLine1.TL_FeeChargeLevel = "STD";

			var feeChargeTypeDescription = DescriptionHelpers.FormatWithTab("Fee Charge Type:");
			var feeChargeLevelDescription = DescriptionHelpers.FormatWithTab("Fee Charge Level:");
			Assert(info1.Description.Contains($"{feeChargeTypeDescription}DWY - Domestic Warranty"));
			Assert(info1.Description.Contains($"{feeChargeLevelDescription}STD - Standard Level"));

			rateLine2.TL_FeeChargeType = "DWY";
			rateLine2.TL_FeeChargeLevel = "XXX";
			Assert(info2.Description.Contains($"{feeChargeTypeDescription}DWY - Domestic Warranty"));
			Assert(info2.Description.Contains($"{feeChargeLevelDescription}XXX"));
		}

		#endregion

		#region TestContainerOwnership

		public void TestContainerOwnership()
		{
			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP").RateLines[0];
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, new TestRatingCriteria());
			var info = new AutoRateInfo(result, parameters, Factory);
			var containerDescription = DescriptionHelpers.FormatWithTab("Container:");
			Assert(info.Description.Contains($"{containerDescription}20GP"));
			Assert(!info.Description.Contains($"{containerDescription}20GP Shipper Owned"));

			rateLine.TL_WeightVolume = RatingConstants.Units.CN;
			rateLine.TL_ContainerOwnership = Constants.ContainerOwnership.Codes.ShipperOwned;

			info = new AutoRateInfo(result, parameters, Factory);
			Assert(info.Description.Contains($"{containerDescription}20GP Shipper Owned"));
		}

		#endregion

		#region TestContractNumber

		public void TestContractNumber()
		{
			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var contractNumber = "PAI000";
			var contractNumberMessage = $"{DescriptionHelpers.FormatWithTab("Client Contract Number:")}{contractNumber}";
			var rateEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			var rateLine = rateEntry.RateLines[0];

			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, new TestRatingCriteria());
			var info = new AutoRateInfo(result, parameters, Factory);
			Assert(!info.Description.Contains(contractNumberMessage));

			rateEntry.TI_ContractNumber = contractNumber;
			info = new AutoRateInfo(result, parameters, Factory);
			Assert(info.Description.Contains(contractNumberMessage));
		}

		void AssertContractNumberDescription(IRateEntry rateEntry, string expectedDescription)
		{
			var rateLine = rateEntry.ChildRateLines.First();
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, new TestRatingCriteria());
			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var info = new AutoRateInfo(result, parameters, Factory);
			AssertContains("The description should contain the expected text.", expectedDescription, info.Description.ToString());
		}

		public void TestContractNumber_ClientRate()
		{
			var clientRateEntry = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			clientRateEntry.TI_ContractNumber = "ContractNo1";
			AssertContractNumberDescription(rateEntry: clientRateEntry, expectedDescription: $"{DescriptionHelpers.FormatWithTab("Client Contract Number:")}ContractNo1");
		}

		public void TestContractNumber_Costing()
		{
			var costRateEntry = Helper
				.NewCosting(Helper.NewOrgHeader())
				.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			costRateEntry.TI_ContractNumber = "ContractNo1";
			AssertContractNumberDescription(rateEntry: costRateEntry, expectedDescription: $"{DescriptionHelpers.FormatWithTab("Carrier Contract Number:")}ContractNo1");
		}

		public void TestContractNumber_CompanyTariff()
		{
			var companyTariffRateEntry = Helper
				.NewCompanyTariff()
				.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			companyTariffRateEntry.TI_ContractNumber = "ContractNo1";
			AssertContractNumberDescription(rateEntry: companyTariffRateEntry, expectedDescription: $"{DescriptionHelpers.FormatWithTab("Client Contract Number:")}ContractNo1");
		}

		public void TestContractNumber_Quote()
		{
			var quoteRateEntry = Helper
				.NewQuote(Helper.NewOrgHeader())
				.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			quoteRateEntry.TI_ContractNumber = "ContractNo1";
			AssertContractNumberDescription(rateEntry: quoteRateEntry, expectedDescription: $"{DescriptionHelpers.FormatWithTab("Client Contract Number:")}ContractNo1");
		}

		public void TestContractNumber_IntercompanyTariff_Revenue()
		{
			var intercompanyTariffRateEntry = Helper
				.NewIntercompanyTariff(Helper.NewOrgHeader())
				.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			intercompanyTariffRateEntry.TI_ContractNumber = "ContractNo1";
			using (_Rating.Start(new TestInteractor(), isEqualization: false))
			using (_Rating.StartSell())
			{
				AssertContractNumberDescription(rateEntry: intercompanyTariffRateEntry, expectedDescription: $"{DescriptionHelpers.FormatWithTab("Client Contract Number:")}ContractNo1");
			}
		}

		public void TestContractNumber_IntercompanyTariff_Cost()
		{
			var intercompanyTariffRateEntry = Helper
				.NewIntercompanyTariff(Helper.NewOrgHeader())
				.AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP");
			intercompanyTariffRateEntry.TI_ContractNumber = "ContractNo1";
			using (_Rating.Start(new TestInteractor(), isEqualization: false))
			using (_Rating.StartCost())
			{
				AssertContractNumberDescription(rateEntry: intercompanyTariffRateEntry, expectedDescription: $"{DescriptionHelpers.FormatWithTab("Carrier Contract Number:")}ContractNo1");
			}
		}

		public void TestAutoRateRevenue_RateAuditInfoShouldContainFMCTariffID()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var supplier = Factory.New<OrgHeader>();
			var ratingCategory = RatingConstants.RateCategory.FCL;
			var rateEntry = Factory.New<CompanyTariff>().EntryCollections[ratingCategory].LazyLoadingCollection.AddNew();
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_RateCalculator = CalculatorForTest.Code;
			rateLine.TL_AC = chargeCode.PK;
			rateEntry.TI_OH_Supplier = supplier.PK;
			rateEntry.TI_FMCTariffID = "1234";

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(calculationResult, parameters, Factory);
			var description = info.Description;

			AssertContains($"{DescriptionHelpers.FormatWithTab("FMC Tariff ID:")}1234", description);
		}

		public void TestAutoRateCosting_RateAuditInfoShouldNotContainFMCTariffID()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var ratingCategory = RatingConstants.RateCategory.FCL;
			var rateEntry = Factory.New<Costing>().EntryCollections[ratingCategory].LazyLoadingCollection.AddNew();
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_RateCalculator = CalculatorForTest.Code;
			rateLine.TL_AC = chargeCode.PK;
			rateEntry.TI_FMCTariffID = "1234";

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(calculationResult, parameters, Factory);
			var description = info.Description;

			AssertNotContains($"{DescriptionHelpers.FormatWithTab("FMC Tariff ID:")}1234", description);
		}

		public void TestContractNumber_WiseCost()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var charge = new Charge()
			{
				ChargeCode = chargeCodeFRT.AC_Code,
				Currency = "AUD",
				FlatRate = 100,
				PerUnitRate = null
			};

			var wiseLine = new WiseLine(Factory, charge) { TL_AC = chargeCodeFRT.PK };

			var wiseEntry = new WiseEntry(new Rate(), Factory)
			{
				TI_Mode = CW1Constants.RateMode.SEA,
				TI_RateCategory = RatingConstants.RateCategory.FCL,
				TI_OriginLRC = "HKHKG",
				TI_DestinationLRC = "USLAX",
				TI_RC = GP20.PK,
				RateProvider = WRConstants.RateProviders.CargoGuide,
				TI_RateStartDate = new ZDate(2020, 06, 06),
				TI_RateEndDate = new ZDate(2030, 06, 06),
				TI_ContractNumber = "ContractNo1",
				ChildRateLines = new[] { wiseLine },
			};

			var wiseHeader = new WiseHeader(Factory)
			{
				TH_OH = carrier.PK,
				ChildRateEntries = new[] { wiseEntry },
			};

			AssertContractNumberDescription(rateEntry: wiseEntry, expectedDescription: $"{DescriptionHelpers.FormatWithTab("Carrier Contract Number:")}ContractNo1");
		}

		#endregion

		#region Wise cost descriptions

		public void TestWiseCost_CGReference()
		{
			var customField = new CustomField()
			{
				Code = Rate.CustomFields.Cargoguide.Reference,
				Description = "Reference",
				Value = "Potato"
			};

			SetupAndRunTestWiseCostLocationDescriptions(
				"Description should have the CG reference",
				new[] { customField },
				WRConstants.ChargeCustomCategory.Outland,
				$"{DescriptionHelpers.FormatWithTab("CG Reference:")}Potato");

			Assert(true);
		}

		public void TestWiseCostLocationDescriptions_OverallInland()
		{
			var customFields = GetFullLocationCustomFields();

			SetupAndRunTestWiseCostLocationDescriptions(
				"Location descriptions should have only inland route",
				customFields,
				WRConstants.ChargeCustomCategory.Inland,
				$"{DescriptionHelpers.FormatWithTab("Inland Route:")}inlandInfo.unloCode [inlandInfo.locationType]");

			Assert(true);
		}

		public void TestWiseCostLocationDescriptions_OverallOutland()
		{
			var customFields = GetFullLocationCustomFields();

			SetupAndRunTestWiseCostLocationDescriptions("Location descriptions should have only outland route", customFields,
				WRConstants.ChargeCustomCategory.Outland,
				$"{DescriptionHelpers.FormatWithTab("Outland Route:")}outlandInfo.unloCode [outlandInfo.locationType]");

			Assert(true);
		}

		public void TestWiseCostLocationDescriptions_OverallBOL_InOrder()
		{
			var customFields = GetFullLocationCustomFields();

			SetupAndRunTestWiseCostLocationDescriptions("Location descriptions should be in the right orders", customFields,
				WRConstants.ChargeCustomCategory.BOL,
				$@"{DescriptionHelpers.FormatWithTab("Inland Route:")}inlandInfo.unloCode [inlandInfo.locationType]
{DescriptionHelpers.FormatWithTab("Ocean Route:")}origin > destination
{DescriptionHelpers.FormatWithTab("Origin:")}OriginUNLOCO [OriginLocType]
{DescriptionHelpers.FormatWithTab("Origin Terminal:")}baseOceanLocInfo.origUNLoCodeTerminal
{DescriptionHelpers.FormatWithTab("Origin Routing:")}baseOceanLocInfo.origUNLoCodeRouting
{DescriptionHelpers.FormatWithTab("Destination:")}baseOceanLocInfo.destUNLoCode [baseOceanLocInfo.destLocType]
{DescriptionHelpers.FormatWithTab("Destination Terminal:")}baseOceanLocInfo.destUNLoCodeTerminal
{DescriptionHelpers.FormatWithTab("Destination Routing:")}baseOceanLocInfo.destUNLoCodeRouting
{DescriptionHelpers.FormatWithTab("Outland Route:")}outlandInfo.unloCode [outlandInfo.locationType]");

			Assert(true);
		}

		public void TestWiseCostLocationDescriptions_OverallOcean_InOrder()
		{
			var customFields = GetFullLocationCustomFields();

			SetupAndRunTestWiseCostLocationDescriptions("Location descriptions should be in the right orders", customFields,
				WRConstants.ChargeCustomCategory.Ocean,
				$@"{DescriptionHelpers.FormatWithTab("Ocean Route:")}origin > destination
{DescriptionHelpers.FormatWithTab("Origin:")}OriginUNLOCO [OriginLocType]
{DescriptionHelpers.FormatWithTab("Origin Terminal:")}baseOceanLocInfo.origUNLoCodeTerminal
{DescriptionHelpers.FormatWithTab("Origin Routing:")}baseOceanLocInfo.origUNLoCodeRouting
{DescriptionHelpers.FormatWithTab("Destination:")}baseOceanLocInfo.destUNLoCode [baseOceanLocInfo.destLocType]
{DescriptionHelpers.FormatWithTab("Destination Terminal:")}baseOceanLocInfo.destUNLoCodeTerminal
{DescriptionHelpers.FormatWithTab("Destination Routing:")}baseOceanLocInfo.destUNLoCodeRouting");

			Assert(true);
		}

		public void TestWiseCostLocationDescriptions_IndividualOcean()
		{
			CombineAssertions("Location Wise Cost Descriptions", () =>
			{
				SetupAndRunTestWiseCostLocationDescriptions(
					"Ocean Route Custom Field should have expected text",
					new[]
					{
						new CustomField()
						{
							Code = Rate.CustomFields.CargoSphere.OceanRouting,
							Description = "Ocean Route",
							Value = "origin > destination"
						}
					},
					WRConstants.ChargeCustomCategory.Ocean,
					$"{DescriptionHelpers.FormatWithTab("Ocean Route:")}origin > destination"
				);

				SetupAndRunTestWiseCostLocationDescriptions(
					"Origin Custom Field should have expected text",
					new[]
					{
						new CustomField()
						{
							Code = Rate.CustomFields.CargoSphere.Origin,
							Description = "Origin",
							Value = "OriginUNLOCO [OriginLocType]"
						}
					},
					WRConstants.ChargeCustomCategory.Ocean,
					$"{DescriptionHelpers.FormatWithTab("Origin:")}OriginUNLOCO [OriginLocType]"
				);

				SetupAndRunTestWiseCostLocationDescriptions(
					"Origin Terminal Custom Field should have expected text",
					new[]
					{
						new CustomField()
						{
							Code = Rate.CustomFields.CargoSphere.OriginTerminal,
							Description = "Origin Terminal",
							Value = "baseOceanLocInfo.origUNLoCodeTerminal"
						}
					},
					WRConstants.ChargeCustomCategory.Ocean,
					$"{DescriptionHelpers.FormatWithTab("Origin Terminal:")}baseOceanLocInfo.origUNLoCodeTerminal"
				);

				SetupAndRunTestWiseCostLocationDescriptions(
					"Destination Custom Field should have expected text",
					new[]
					{
						new CustomField()
						{
							Code = Rate.CustomFields.CargoSphere.Destination,
							Description = "Destination",
							Value = "baseOceanLocInfo.destUNLoCode [baseOceanLocInfo.destLocType]"
						}
					},
					WRConstants.ChargeCustomCategory.Ocean,
					$"{DescriptionHelpers.FormatWithTab("Destination:")}baseOceanLocInfo.destUNLoCode [baseOceanLocInfo.destLocType]"
				);

				SetupAndRunTestWiseCostLocationDescriptions(
					"Destination Terminal Custom Field should have expected text",
					new[]
					{
						new CustomField()
						{
							Code = Rate.CustomFields.CargoSphere.DestinationTerminal,
							Description = "Destination Terminal",
							Value = "baseOceanLocInfo.destUNLoCodeTerminal"
						}
					},
					WRConstants.ChargeCustomCategory.Ocean,
					$"{DescriptionHelpers.FormatWithTab("Destination Terminal:")}baseOceanLocInfo.destUNLoCodeTerminal"
				);

				SetupAndRunTestWiseCostLocationDescriptions(
					"Destination Routing Custom Field should have expected text",
					new[]
					{
						new CustomField()
						{
							Code = Rate.CustomFields.CargoSphere.DestinationRouting,
							Description = "Destination Routing",
							Value = "baseOceanLocInfo.destUNLoCodeRouting"
						}
					},
					WRConstants.ChargeCustomCategory.Ocean,
					$"{DescriptionHelpers.FormatWithTab("Destination Routing:")}baseOceanLocInfo.destUNLoCodeRouting"
				);

				SetupAndRunTestWiseCostLocationDescriptions(
					"Destination Routing Custom Field should have expected text",
					new[]
					{
						new CustomField()
						{
							Code = Rate.CustomFields.CargoSphere.OceanRouting,
							Description = "Ocean Routing",
							Value = "origin > destination"
						}
					},
					WRConstants.ChargeCustomCategory.Ocean,
					$"{DescriptionHelpers.FormatWithTab("Ocean Route:")}origin > destination"
				);
			});

			Assert(true);
		}

		CustomField[] GetFullLocationCustomFields() => new[]
		{
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.InlandRouting,
				Description = "Inland Route",
				Value = "inlandInfo.unloCode [inlandInfo.locationType]"
			},
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.OutlandRouting,
				Description = "Outland Route",
				Value = "outlandInfo.unloCode [outlandInfo.locationType]"
			},
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.Origin,
				Description = "Origin",
				Value = "OriginUNLOCO [OriginLocType]"
			},
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.OriginTerminal,
				Description = "Origin Terminal",
				Value = "baseOceanLocInfo.origUNLoCodeTerminal"
			},
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.OriginRouting,
				Description = "Origin Routing",
				Value = "baseOceanLocInfo.origUNLoCodeRouting"
			},
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.Destination,
				Description = "Destination",
				Value = "baseOceanLocInfo.destUNLoCode [baseOceanLocInfo.destLocType]"
			},
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.DestinationTerminal,
				Description = "Destination Terminal",
				Value = "baseOceanLocInfo.destUNLoCodeTerminal"
			},
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.DestinationRouting,
				Description = "Destination Routing",
				Value = "baseOceanLocInfo.destUNLoCodeRouting"
			},
			new CustomField()
			{
				Code = Rate.CustomFields.CargoSphere.OceanRouting,
				Description = "Ocean Routing",
				Value = "origin > destination"
			},
		};

		void SetupAndRunTestWiseCostLocationDescriptions(string becauseMessage, CustomField[] customFields, string chargeCustomCategory, string expectedText)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_IsShippingProvider = true;

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var charge = new Charge()
			{
				ChargeCode = chargeCodeFRT.AC_Code,
				Currency = "AUD",
				FlatRate = 100,
				PerUnitRate = null,
				CustomCategory = chargeCustomCategory
			};

			var wiseLine = new WiseLine(Factory, charge)
			{
				TL_AC = chargeCodeFRT.PK
			};

			var wiseEntry = new WiseEntry(new Rate(), Factory)
			{
				TI_Mode = CW1Constants.RateMode.SEA,
				TI_RateCategory = RatingConstants.RateCategory.FCL,
				TI_OriginLRC = "HKHKG",
				TI_DestinationLRC = "USLAX",
				TI_RC = gp20.PK,
				RateProvider = WRConstants.RateProviders.CargoGuide,
				TI_RateStartDate = new ZDate(2020, 06, 06),
				TI_RateEndDate = new ZDate(2030, 06, 06),
				TI_RH_NKCommodityCode = "MCLAREN",
				TI_PL_NKCarrierServiceLevel = "GOD",
				TI_ContractNumber = "ZXC02192",
				CommodityGroup = "CM1",
				ChildRateLines = new[] { wiseLine },
				CustomFields = customFields,
			};

			var wiseHeader = new WiseHeader(Factory)
			{
				TH_OH = carrier.PK,
				ChildRateEntries = new[] { wiseEntry },
			};

			var criteria = new TestRatingCriteria();
			var result = CalculationResult.CreateForTest(wiseLine, 100, 0, 0, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);
			AssertContains(
				becauseMessage,
				expectedText,
				info.Description.ToString()
			);
		}

		#endregion

		#region Rate type descriptions

		public void TestRateTypeDescriptions_ClientRate()
		{
			var client = Helper.NewOrgHeader("CLIENTXXX");

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria();
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

			AssertContains(
				"Charge located in CLIENTXXX client rate with the following details",
				info.Description.ToString()
			);
		}

		public void TestRateTypeDescriptions_ClientRateWithClientSubsidiaryRelations()
		{
			var client = Helper.NewOrgHeader("CLIENTXXX");
			var subClient = Helper.NewOrgHeader("SUBCLIENT");
			var orgManagementRelatedParty = client.RelatedManagementSubsidiaryRelations.AddNew();
			orgManagementRelatedParty.PR_OH_Parent = subClient.PK;
			orgManagementRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			orgManagementRelatedParty.PR_OH_RelatedParty = client.PK;

			var orgRateTariffLevelCollection = new OrgRateTariffLevelCollection(subClient, Env.CurrentCompanyPK);
			var tariffLevel = orgRateTariffLevelCollection.AddNew();
			tariffLevel.P7_TariffType = "FRT";
			tariffLevel.P7_TariffLevel = 1;
			tariffLevel.P7_ApplyGroupRate = true;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria();
			criteria.LocalClient = subClient;
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

			AssertContains(
				"Charge description should indicate location in CLIENTXXX group client rate linked to SUBCLIENT.",
				"Charge located in CLIENTXXX group client rate (Linked to client rate: SUBCLIENT) with the following details",
				info.Description.ToString()
			);
		}

		public void TestRateTypeDescriptions_GlobalClientRate()
		{
			var globalCharge = Helper.ChargeCodes.CreateGlobalCharge("FRT");
			var client = Helper.NewOrgHeader("CLIENTXXX");

			var rate = Helper.NewGlobalClientRate(client);
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", globalCharge.AC_Code, 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria();
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

			AssertContains(
				"Charge located in CLIENTXXX global client rate with the following details",
				info.Description.ToString()
			);
		}

		public void TestRateTypeDescriptions_Costing()
		{
			var client = Helper.NewOrgHeader("CLIENTXXX");

			var rate = Helper.NewCosting(client);
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria();
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

			AssertContains(
				"Charge located in CLIENTXXX cost with the following details",
				info.Description.ToString()
			);
		}

		public void TestRateTypeDescriptions_GlobalCosting()
		{
			var globalCharge = Helper.ChargeCodes.CreateGlobalCharge("FRT");
			var client = Helper.NewOrgHeader("CLIENTXXX");

			var rate = Helper.NewGlobalCosting(client);
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", globalCharge.AC_Code, 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria();
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

			AssertContains(
				"Charge located in CLIENTXXX global cost with the following details",
				info.Description.ToString()
			);
		}

		public void TestRateTypeDescriptions_Quote()
		{
			var client = Helper.NewOrgHeader("CLIENTXXX");

			var rate = Helper.NewQuote(client);
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria();
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

			AssertContains(
				"Charge located in CLIENTXXX quotation with the following details",
				info.Description.ToString()
			);
		}

		public void TestRateTypeDescriptions_OneOffQuote()
		{
			var client = Helper.NewOrgHeader("CLIENTXXX");

			var rate = Helper.NewQuote(client);
			rate.TH_OneTimeQuote = true;
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria();
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

			AssertContains(
				"Charge located in CLIENTXXX one off quotation 00000999 with the following details",
				info.Description.ToString()
			);
		}

		public void TestRateTypeDescriptions_CompanyTariff()
		{
			var client = Helper.NewOrgHeader("CLIENTXXX");
			var orgRateTariffLevelCollection = new OrgRateTariffLevelCollection(client, Env.CurrentCompanyPK);
			var tariffLevel = orgRateTariffLevelCollection.AddNew();
			tariffLevel.P7_TariffType = "FRT";
			tariffLevel.P7_TariffLevel = 1;
			tariffLevel.P7_ApplyGroupRate = true;

			var rate = Helper.NewCompanyTariff();
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			var criteria = new TestRatingCriteria();
			criteria.LocalClient = client;
			var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
			var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

			AssertContains(
				"Charge located in Company Tariff Level 1 (Linked to: CLIENTXXX) with the following details",
				info.Description.ToString()
			);
		}

		public void TestRateTypeDescriptions_InterCompanyTariff()
		{
			var globalCharge = Helper.ChargeCodes.CreateGlobalCharge("FRT");
			var client = Helper.NewOrgHeader("CLIENTXXX");

			var rate = Helper.NewIntercompanyTariff(client);
			var rateEntry = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", globalCharge.AC_Code, 1000, Constants.CurrencyCodes.Australia);
			var rateLine = rateEntry.RateLines[0];

			using (_Rating.Start(new TestInteractor(), isEqualization: false))
			using (_Rating.StartCost())
			{
				var criteria = new TestRatingCriteria();
				var result = CalculationResult.CreateForTest(rateLine, 10m, 5m, 20m, criteria);
				var info = new AutoRateInfo(result, new AutoRatingCalculatorParametersForTesting(criteria), Factory);

				AssertContains(
					"Charge located in CLIENTXXX Intercompany Tariff with the following details",
					info.Description.ToString()
				);
			}
		}

		#endregion

		#region RateId

		public void TestRateId_CW1Rate_ReturnEntryPK()
		{
			var header = Factory.New<Costing>();
			var entry = header.AddRateEntry("FCL");
			var line = entry.AddRateLine("FRT", lineUnit: "KG");

			var info = new AutoRateInfo(Factory, line);
			AssertEquals("The RateId should match the entry's PK as a string.", entry.PK.ToString(), info.RateId);
		}

		public void TestRateId_WiseRate_ReturnWiseRateId()
		{
			var rate = new Rate();
			rate.Id = "McLaren";

			var line = new WiseLine(Factory, new Charge());
			line.ParentRateEntry = new WiseEntry(rate, Factory);

			var info = new AutoRateInfo(Factory, line);
			AssertEquals(
				"For rates from rates service we use the id provided by rate provider",
				"McLaren",
				info.RateId
			);
		}

		#endregion

		public void TestRateDescription_ContainsCargoSphereLclCustomFields()
		{
			var chargeCodeFRT = Helper.ChargeCodes["FRT"];

			var charge = new Charge()
			{
				ChargeCode = chargeCodeFRT.AC_Code,
				Currency = "AUD",
				FlatRate = 100,
				PerUnitRate = null
			};

			var wiseLine = new WiseLine(Factory, charge) { TL_AC = chargeCodeFRT.PK };

			var wiseEntry = new WiseEntry(new Rate(), Factory)
			{
				TI_Mode = CW1Constants.RateMode.SEA,
				TI_RateCategory = RatingConstants.RateCategory.LCL,
				TI_OriginLRC = "HKHKG",
				TI_DestinationLRC = "USLAX",
				RateProvider = WRConstants.RateProviders.CargoSphere,
				TI_RateStartDate = new ZDate(2022, 06, 06),
				TI_RateEndDate = new ZDate(2023, 06, 06),
				ChildRateLines = new[] { wiseLine },
			};

			var wiseHeader = new WiseHeader(Factory)
			{
				ChildRateEntries = new[] { wiseEntry },
			};

			wiseLine.ParentRateEntry = wiseEntry;
			wiseEntry.ParentRatingHeader = wiseHeader;

			wiseLine.CustomFields = new[] { new CustomField() { Code = Rate.CustomFields.CargoSphere.PriceBy, Value = "Cubic Meter/1,000 KGS (W/M)" } };
			wiseEntry.CustomFields = new[] { new CustomField() { Code = Rate.CustomFields.CargoSphere.LclUnit, Value = "Cubic Meter 10+ W/M" } };

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(wiseLine, 10, 20, 30, criteria);
			var info = new AutoRateInfo(calculationResult, parameters, Factory);
			AssertContains(System.Environment.NewLine + $"{DescriptionHelpers.FormatWithTab("CS Price Break:")}Cubic Meter/1,000 KGS (W/M) applicable to Cubic Meter 10+ W/M" + System.Environment.NewLine, info.RateDescription);
		}

		public void TestLineDescriptionAndSellReferenceNumber_UnitFactorIsCTN()
		{
			var gp20 = Helper.Containers["20GP"];
			var gp40 = Helper.Containers["40GP"];

			var rateInfo = new AutoRateInfo(Factory);
			AssertEquals("Empty by default", true, rateInfo.SellReferenceNumber.IsEmpty);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RatingConstants.TransportMode.SEA, container: gp20.RC_Code, removeLines: true);
			var line20GPFRTWithCTN = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line20GPFRTWithCTN.TL_UnitFactor = UnitFactorList.Codes.CTN;
			var line20GPBAF = rateEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			var line20GPBAFWithSAM = rateEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			line20GPBAFWithSAM.TL_UnitFactor = UnitFactorList.Codes.SAM;

			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RatingConstants.TransportMode.SEA, container: gp40.RC_Code, removeLines: true);
			var line40GPFRTWithCTN = rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line40GPFRTWithCTN.TL_UnitFactor = UnitFactorList.Codes.CTN;

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var measures = criteria.RateableMeasures;

			measures.CreateContainerList();
			measures.AddContainerGroup(gp20.PK, new[] {
				new MeasureInfo.ContainerInfo(weight: 50, Weight.Kilograms, container: gp20),
				new MeasureInfo.ContainerInfo(weight: 40, Weight.Kilograms, container: gp20)
			});

			measures.AddContainerGroup(gp40.PK, new[] {
				new MeasureInfo.ContainerInfo(weight: 50, Weight.Kilograms, container: gp40)
			});

			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, line20GPFRTWithCTN, 0);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, line20GPFRTWithCTN, 1);
			parameters.AddLineMeasureMatch(MeasureType.ContainerCount, line40GPFRTWithCTN, 2);

			AssertContainerNumber(line20GPFRTWithCTN, "20GP (2)", "International Freight - Container #20GP (2)", "International Freight - Container #20GP (2)");
			AssertContainerNumber(line20GPBAF, "", "Bunker Adjustment Factor", "Bunker Adjustment Factor");
			AssertContainerNumber(line20GPBAFWithSAM, "", "Bunker Adjustment Factor", "Bunker Adjustment Factor");
			AssertContainerNumber(line40GPFRTWithCTN, "40GP (1)", "International Freight - Container #40GP (1)", "International Freight - Container #40GP (1)");

			line20GPFRTWithCTN.OverrideChargeDescription = true;
			line20GPFRTWithCTN.TL_RateDescLocal = "Local FRT";
			line40GPFRTWithCTN.OverrideChargeDescription = true;
			line40GPFRTWithCTN.TL_RateDescLocal = "Local BAF";
			AssertContainerNumber(line20GPFRTWithCTN, "20GP (2)", "International Freight - Container #20GP (2)", "Local FRT - Container #20GP (2)");
			AssertContainerNumber(line40GPFRTWithCTN, "40GP (1)", "International Freight - Container #40GP (1)", "Local BAF - Container #40GP (1)");

			void AssertContainerNumber(RateLine rateLine, ZString expectedContainerNumber, ZString chargeCodeDesc, ZString chargeCodeLocalDesc)
			{
				var calculationResult = CalculationResult.CreateForTest(rateLine, 1m, 1m, 1m, criteria);

				rateInfo = new AutoRateInfo(calculationResult, parameters, Factory);

				AssertEquals("SellReferenceNumber should have Container number set.", expectedContainerNumber, rateInfo.SellReferenceNumber);
				AssertEquals("Charge Code Desc", chargeCodeDesc, rateInfo.InvoiceLineDescription);
				AssertEquals("Charge Code Local Desc", chargeCodeLocalDesc, rateInfo.InvoiceLineLocalDescription);
			}
		}
	}

	public static class AutoRateInfoTestExtensions
	{
		public static void AddFlatPaymentBasis(this AutoRateInfo rateInfo, ZDecimal amount, ZString adapterID, string currency = "")
		{
			if (string.IsNullOrEmpty(currency))
			{
				currency = rateInfo.Currency;
			}
			rateInfo.Bases.Add(new PaymentBasis(default, RateInfo.CreateFLT(amount, currency), AdapterType.Shipment, adapterID));
		}

		public static void AddPerUnitPaymentBasis(this AutoRateInfo rateInfo, Quantity chargeable, ZDecimal rate, ZString unit, ZString adapterID, string currency = "")
		{
			if (string.IsNullOrEmpty(currency))
			{
				currency = rateInfo.Currency;
			}
			rateInfo.Bases.Add(new PaymentBasis(chargeable, RateInfo.CreateUNT(rate, unit, currency), AdapterType.Shipment, adapterID));
		}

		public static void AddAgentFlatPaymentBasis(this AutoRateInfo rateInfo, ZDecimal amount, ZString adapterID, string currency = "")
		{
			if (string.IsNullOrEmpty(currency))
			{
				currency = rateInfo.Currency;
			}
			rateInfo.AgentBases.Add(new PaymentBasis(default, RateInfo.CreateFLT(amount, currency), AdapterType.Shipment, adapterID));
		}
	}
}
