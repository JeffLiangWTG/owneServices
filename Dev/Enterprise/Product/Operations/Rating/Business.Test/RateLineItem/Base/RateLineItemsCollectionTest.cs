using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class RateLineItemsCollectionTest : RatingTestCase
	{
		public void TestGlobalRateEntryDeepCloneToLocalRateEntry()
		{
			var globalChargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("GLB1");
			var globalChargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("GLB2");
			var globalChargeCode3 = Helper.ChargeCodes.CreateGlobalCharge("GLB3");
			Factory.Save();

			foreach (var chargeCode in globalChargeCode3.ChildChargeCodes.ToArray())
			{
				chargeCode.Delete();
			}
			Factory.Save();

			var localChargeCode = globalChargeCode2.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);

			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			globalRateEntry.RateLines.RemoveAndDeleteAll();
			var percentageCalculator = globalRateEntry.AddRateLine(globalChargeCode1.AC_Code, PercentageCalculator.Code).GetCalculator<PercentageCalculator>();
			percentageCalculator.Percent = 10.02m;
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode2.PK;
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode3.PK;
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.OriginCharges);

			var localClientRate = Helper.NewClientRate(client);
			var localRateEntry = globalRateEntry.DeepClone(localClientRate.LCLRateEntriesForBinding);

			var clonedItems = localRateEntry.RateLines[0].RateLineItems.Cast<RateLineItem>();

			CombineAssertions(() =>
			{
				AssertEquals(10.02m, localRateEntry.RateLines[0].GetCalculator<PercentageCalculator>().Percent);

				AssertEquals("Should have cloned the ApplyTo items", 3, clonedItems.Count(i => i.RateOperatorIsApplyTo()));
				AssertEquals(1, clonedItems.Count(i => i.TM_Text == CalculatorConstants.Text.OriginCharges));

				var localChargeCodeApplyToItems = clonedItems.Where(i => i.TM_AC == localChargeCode.PK).ToArray();
				AssertEquals("Should have mapped the local charge from the global charge", 1, localChargeCodeApplyToItems.Length);

				var globalChargeCodeApplyToItems = clonedItems.Where(i => i.TM_AC == globalChargeCode3.PK).ToArray();
				AssertEquals("Could not map local from global charge code so it should remain", 1, globalChargeCodeApplyToItems.Length);

				globalChargeCodeApplyToItems[0].Validation.ValidateTM_AC();
				AssertHasError(globalChargeCodeApplyToItems[0].TM_ACInfo, ErrorMessages.LocalChargeCodeIsRequired);
			});
		}

		public void TestLocalRateEntryDeepCloneToGlobalRateEntry()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLB1");
			Factory.Save();

			var localChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			var unlinkedLocalChargeCode = Helper.ChargeCodes["CAF"].PK;

			var client = Helper.NewOrgHeader();
			var localClientRate = Helper.NewClientRate(client);
			var localRateEntry = localClientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			localRateEntry.RateLines.RemoveAndDeleteAll();
			var percentageCalculator = localRateEntry.AddRateLine("BAF", PercentageCalculator.Code).GetCalculator<PercentageCalculator>();
			percentageCalculator.Percent = 10.02m;
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = unlinkedLocalChargeCode;
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = localChargeCode.PK;
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.DestinationCharges);

			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = localRateEntry.DeepClone(globalClientRate.LCLRateEntriesForBinding);

			var clonedItems = globalRateEntry.RateLines[0].RateLineItems.Cast<RateLineItem>();

			CombineAssertions(() =>
			{
				AssertEquals(10.02m, globalRateEntry.RateLines[0].GetCalculator<PercentageCalculator>().Percent);

				AssertEquals("Should have cloned the ApplyTo items", 3, clonedItems.Count(i => i.RateOperatorIsApplyTo()));
				AssertEquals(1, clonedItems.Count(i => i.TM_Text == CalculatorConstants.Text.DestinationCharges));

				var globalChargeCodeApplyToItems = clonedItems.Where(i => i.TM_AC == globalChargeCode.PK).ToArray();
				AssertEquals("Should have mapped the global charge from the local charge", 1, globalChargeCodeApplyToItems.Length);

				var localChargeCodeApplyToItems = clonedItems.Where(i => i.TM_AC == unlinkedLocalChargeCode).ToArray();
				AssertEquals("Could not map global from local charge code so it should remain", 1, localChargeCodeApplyToItems.Length);

				localChargeCodeApplyToItems[0].Validation.ValidateTM_AC();
				AssertHasError(localChargeCodeApplyToItems[0].TM_ACInfo, ErrorMessages.GlobalChargeCodeIsRequired);
			});
		}

		public void TestRateLineItemIsPartOfCollection()
		{
			var testObjectFactory = new BusinessObjectFactory();
			var rateEntry = testObjectFactory.NewWithValidTestData<RateEntry>();
			var rateline = rateEntry.RateLines.AddNew();
			var ratelineitem1 = rateline.RateLineItems.AddNew();
			var collection = rateline.RateLineItems.Count;
			AssertEquals("There should be only one rateline item", 1, collection);

			var ratelineitem2 = testObjectFactory.New<RateLineItem>();
			AssertEquals("Developer Exception should be raised", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestRateLineItemDeletedFromCollection()
		{
			var testObjectFactory = new BusinessObjectFactory();
			var rateEntry = testObjectFactory.NewWithValidTestData<RateEntry>();
			var rateline = rateEntry.RateLines.AddNew();
			var ratelineitem1 = rateline.RateLineItems.AddNew();
			AssertEquals("There should be one RateLineItem", 1, rateline.RateLineItems.Count);

			rateline.RateLineItems.RemoveAndDeleteAll();
			AssertEquals("No Developer Exception should be raised", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestCompanyTariffDiscount()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry(RatingConstants.RateCategory.ORG);
			entry1.TI_Mode = Core.Constants.RateMode.ALL;
			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			((UnitCalculator)line1.Calculator).PerUnit = 10m;
			line1.ViewAgentRates = true;
			((UnitCalculator)line1.Calculator).PerUnit = 16m;
			line1.ViewAgentRates = false;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			var entry2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var line2 = entry2.RateLines[0];
			AssertEquals(8m, ((UnitCalculator)line2.Calculator).PerUnit);

			line2.ViewAgentRates = true;
			AssertEquals(12.8m, ((UnitCalculator)line2.Calculator).PerUnit);

			var discountedLine = line2.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			discountedLine.HasChanges = true;

			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var tariff2f3 = factory3.Load<CompanyTariff>(tariff2.PK);
			var entry2f3 = tariff2f3.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var line2f3 = entry2f3.RateLines[0];

			AssertEquals(8m, ((UnitCalculator)line2f3.Calculator).PerUnit);
			line2f3.ViewAgentRates = true;
			AssertEquals(12.8m, ((UnitCalculator)line2f3.Calculator).PerUnit);
		}

		public void TestCompanyTariffDiscount_ShouldValueBeDiscounted()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry(RatingConstants.RateCategory.ORG);
			entry1.TI_Mode = Core.Constants.RateMode.ALL;

			var line1 = entry1.AddRateLine("FRT", AgencyCalculator.Code);

			((AgencyCalculator)line1.Calculator).AgencyRate = 20m;
			((AgencyCalculator)line1.Calculator).IncludedLines = 10;
			((AgencyCalculator)line1.Calculator).AgencyLineType = "XYZ";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			var entry2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var line2 = entry2.RateLines[0];
			line2.InitializeCalculator();
			AssertEquals(16m, ((AgencyCalculator)line2.Calculator).AgencyRate);
			AssertEquals(10, ((AgencyCalculator)line2.Calculator).IncludedLines);
		}

		public void TestDataRefreshBusDisabled()
		{
			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = NewClient.PK;
			var entry = testRate.AddRateEntry("ORG", "ALL", "", "");
			entry.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 20m;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var entryInFirstFactory = Factory.Load<ClientRate>(testRate.PK).GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var entryInSecondFactory = factory2.Load<ClientRate>(testRate.PK).GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];

			AssertEquals("Precondition: 1 Line Items in either rate", 1, entryInFirstFactory.RateLines[0].RateLineItems.Count);
			AssertEquals("Precondition: 1 Line Items in either rate", 1, entryInSecondFactory.RateLines[0].RateLineItems.Count);

			entryInFirstFactory.RateLines[0].TL_RateCalculator = FlatPlusPerUnitCalculator.Code;
			entryInFirstFactory.RateLines[0].Calculator[FlatPlusPerUnitCalculator.Items.Operator.UNT] = (ZDecimal)20m;
			entryInFirstFactory.RateLines[0].Calculator[FlatPlusPerUnitCalculator.Items.Operator.BAS] = (ZDecimal)50m;

			Factory.Save();

			AssertEquals("Line in first factory has 2 line items", 2, entryInFirstFactory.RateLines[0].RateLineItems.Count);
			AssertEquals("Line in second factory now has no line items as the collection is NOT published for datarefresh", 0, entryInSecondFactory.RateLines[0].RateLineItems.Count);

			entryInSecondFactory.RateLines[0].RateLineItems.Load();
			AssertEquals("Line in first factory still has 2 line items", 2, entryInFirstFactory.RateLines[0].RateLineItems.Count);
			AssertEquals("Line in second factory has 2 line items as it's been reloaded", 2, entryInSecondFactory.RateLines[0].RateLineItems.Count);
		}

		public void TestFreightDefaultLinesCreatedAIR()
		{
			var testRate = Factory.New<ClientRate>();
			var entry = testRate.AddRateEntry("AIR");

			AssertEquals("Count including accumulated item", 11, entry.RateLines[0].RateLineItems.Count);
		}

		public void TestFreightDefaultLinesCreatedAIRWithOneOffQuote()
		{
			var testQuote = Factory.New<Quote>();
			testQuote.TH_OneTimeQuote = true;
			var entry = testQuote.AddRateEntry("AIR");

			AssertEquals("Count", 11, entry.RateLines[0].RateLineItems.Count);
		}

		public void TestRemove()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();

			AccChargeCode rEVCharge;
			rEVCharge = Helper.ChargeCodes.New("TESTREV", "Test REV", FlatCalculator.Code, "ORG");
			line.TL_AC = rEVCharge.PK;
			line.TL_RateDesc = "DAPH MOO MOO NOLE JAMO";
			line.TL_RateCalculator = PercentageCalculator.Code;

			var item = line.RateLineItems.AddNew();
			item.TM_Type = CalculatorConstants.Type.ApplyTo;

			line.RunPreSaveValidation();

			item.Delete();
			AssertEquals(false, line.LightValidationIsValid);

			line.TL_RateCalculator = DisbursementInterestCalculator.Code;

			item = item = line.RateLineItems.AddNew();
			item.TM_Type = CalculatorConstants.Type.ApplyTo;

			line.RunPreSaveValidation();

			item.Delete();
			AssertEquals(false, line.LightValidationIsValid);
		}

		public void TestCTZRateLineItemLineOrder()
		{
			Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "zone1", "zone2", "zone3" });
			var tariff = Factory.New<CompanyTariff>();
			var line = tariff.AddRateEntry("DST", "FCL", "", "AUSYD").AddRateLine("DCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			line.InitializeCalculator();
			line.RateLineItems.RemoveAll();

			var collection = new CartageZoneCollection(line);
			collection.Load();

			//need to test CTZ calculator with more than 255 ratelineitems in total
			var zone1Items = new List<RateLineItem>();
			var lineItem1 = collection[1].ZoneRateLineItems.AddNew();
			lineItem1.TM_Type = Calculator.Items.Operator.Minus;
			lineItem1.TM_Value = 1m;
			lineItem1.TM_Break = 1m;
			zone1Items.Add(lineItem1);

			for (var i = 1; i < 80; i++)
			{
				var lineItem = collection[1].ZoneRateLineItems.AddNew();
				using (lineItem.GetValidationSuspender())
				{
					lineItem.TM_Type = Calculator.Items.Operator.Plus;
					lineItem.TM_Value = i;
					lineItem.TM_Break = i;
				}
				zone1Items.Add(lineItem);
			}

			var zone2Items = new List<RateLineItem>();
			var lineItem2 = collection[2].ZoneRateLineItems.AddNew();
			lineItem2.TM_Type = Calculator.Items.Operator.Minus;
			lineItem2.TM_Value = 1m;
			lineItem2.TM_Break = 1m;
			zone2Items.Add(lineItem2);

			for (var i = 1; i < 80; i++)
			{
				var lineItem = collection[2].ZoneRateLineItems.AddNew();
				using (lineItem.GetValidationSuspender())
				{
					lineItem.TM_Type = Calculator.Items.Operator.Plus;
					lineItem.TM_Value = i;
					lineItem.TM_Break = i;
				}
				zone2Items.Add(lineItem);
			}

			var zone3Items = new List<RateLineItem>();
			var lineItem3 = collection[3].ZoneRateLineItems.AddNew();
			lineItem3.TM_Type = Calculator.Items.Operator.Minus;
			lineItem3.TM_Value = 1m;
			lineItem3.TM_Break = 1m;
			zone3Items.Add(lineItem3);

			for (var i = 1; i < 100; i++)
			{
				var lineItem = collection[3].ZoneRateLineItems.AddNew();
				using (lineItem.GetValidationSuspender())
				{
					lineItem.TM_Type = Calculator.Items.Operator.Plus;
					lineItem.TM_Value = i;
					lineItem.TM_Break = i;
				}
				zone3Items.Add(lineItem);
			}

			for (var i = 0; i < zone1Items.Count; i++)
			{
				var item = line.RateLineItems.FindByPK(zone1Items[i].PK) as RateLineItem;
				AssertEquals(i, item.LocalLineOrder);
			}

			for (var i = 0; i < zone2Items.Count; i++)
			{
				var item = line.RateLineItems.FindByPK(zone2Items[i].PK) as RateLineItem;
				AssertEquals(i, item.LocalLineOrder);
			}

			for (var i = 0; i < zone3Items.Count; i++)
			{
				var item = line.RateLineItems.FindByPK(zone3Items[i].PK) as RateLineItem;
				AssertEquals(i, item.LocalLineOrder);
			}
		}

		[ExpectNoExceptions]
		public void TestAIRReadOnlyRateLine_CombinedCalculator_CheckOrCreateItems_NoNewItemsCreated()
		{
			var rate = Helper.NewClientRate(null);
			var entry = rate.AddRateEntry("AIR");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.RateLines.AddNew();

			line.TL_RateCalculator = CombinedCalculator.Code;
			line.TL_WeightVolume = "FI";
			line.TL_Rounding = RatingRoundingTypes.UpTo1;
			line.TL_AC = Env.Registry.FreightChargeCode;

			line.RateLineItems.RemoveAndDeleteAll();

			line.ReadOnly = true;

			line.Calculator.PerformPostConstructionActions();
			AssertEquals(0, line.RateLineItems.Count);
		}
	}

	[TestedType(typeof(RateLineItemsCollection))]
	public class RateLineItemsBizObjCollectionTest : BizObjectCollectionAddDeleteTestCase
	{
		protected override BusinessObjectCollection GetCollection()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();

			return new RateLineItemsCollection(rateLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}
	}
}
