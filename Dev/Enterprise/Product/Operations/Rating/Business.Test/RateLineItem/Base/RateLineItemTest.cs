using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class RateLineItemTest : RatingTestCase
	{
		public void TestBreakAndBreakMinimum()
		{
			var client = Helper.NewOrgHeader();
			var testRate = Helper.NewClientRate(client);
			var rateEntry = testRate.AddRateEntry("DST", "AIR", "AUMEL", "AUSYD");
			var line = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			var rateLineItem = line.RateLineItems.AddNew();

			rateLineItem.TM_Break = 123456789.1234m;
			rateLineItem.TM_BreakMinimum = 123456789.1234m;
			Factory.Save();

			CombineAssertions("Break and BreakMinimum should have 13 precision and 4 scale", () =>
			{
				AssertEquals("Break", 123456789.1234m, rateLineItem.TM_Break);
				AssertEquals("BreakMinimum", 123456789.1234m, rateLineItem.TM_BreakMinimum);
			});
		}

		public void TestUniversalCopyRateLineItem()
		{
			var orgHeader = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(orgHeader);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], MinimumOrPerUnitCalculator.Code, "KG");

			Factory.Save();

			var copyTemplateTree = new CopyTemplateTree();
			var entityNode = new EntityCopyTemplateNode { Name = "RateEntry" };
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateEntrySchema.Constants.TI_OriginLRC, CopyMethod = CopyMethod.Copy });

			var rateLineItemEntityNode = new EntityCopyTemplateNode { Name = "RateLineItem" };
			var rateLineItemCollectionNode = new CollectionCopyTemplateNode
			{
				Name = "RateLineItems",
				ItemPropertyName = "RateLineItems",
				ItemsTableName = RateLineItemsSchema.Constants.TableName,
				InnerNode = rateLineItemEntityNode,
				CopyMethod = CollectionCopyMethod.All
			};

			var rateLineEntityNode = new EntityCopyTemplateNode { Name = "RateLine" };
			rateLineEntityNode.Nodes.Add(rateLineItemCollectionNode);

			var collectionNode = new CollectionCopyTemplateNode
			{
				Name = "RateLines",
				ItemPropertyName = "RateLines",
				ItemsTableName = RateLinesSchema.Constants.TableName,
				InnerNode = rateLineEntityNode,
				CopyMethod = CollectionCopyMethod.All
			};
			entityNode.Nodes.Add(collectionNode);

			copyTemplateTree.InnerNode = entityNode;

			var copyManager = new BusinessObjectCopyManager();

			var copiedItem = copyManager.Copy(rateEntry, copyTemplateTree).Object as RateEntry;
			AssertNotNull("New RateEntry created by copy BusinessObjectCopyManager", copiedItem);
			AssertNotEquals(rateEntry.PK, copiedItem.PK);

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals(rateEntry.TI_OriginLRC, copiedItem.TI_OriginLRC);
			AssertEquals(rateEntry.RateLines.Count, copiedItem.RateLines.Count);

			foreach (var line in copiedItem.RateLines)
			{
				var item = line as RateLine;
				AssertNotNull("New RateLine created by copy BusinessObjectCopyManager", item);
				AssertNotNull("New RateLine parent", item.Parent);

				Assert(item.RateLineItems.Count > 0);

				foreach (var rateLineItem in item.ChildRateLineItems)
				{
					AssertNotNull("New RateLineItem created by copy BusinessObjectCopyManager", rateLineItem);
					AssertNotNull("New RateLineItem parent", rateLineItem.ParentRateLine);
				}
			}
		}

		public void TestConstructNullObject()
		{
			AssertNoExceptionThrown("No exception should be thrown when getting a Null Object.", () => Factory.GetNull(typeof(RateLineItem)));
			AssertEquals("No Errors should be reported by creating a Null Object.", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestFactoryChangeSetLoadDatabaseEdition()
		{
			var client = Helper.NewOrgHeader();
			var testRate = Helper.NewClientRate(client);
			var rateEntry = testRate.AddRateEntry("DST", "AIR", "AUMEL", "AUSYD");
			var line = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			var rateLineItem = line.RateLineItems.AddNew();
			Factory.Save();

			rateLineItem.TM_Text = Guid.NewGuid().ToString();

			var notifier = new Mock<INotificationHandler>();
			var sql = $"SELECT * FROM dbo.RATELINEITEMS WHERE TM_PK = '{rateLineItem.PK.ToGuid()}'";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			var dataRow = dataTable.Rows[0];

			ExceptionReporterTestListener.Instance.Clear();
			var exception = new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("TestFactoryChangeSetLoadDatabaseEdition"), dataRow, Db.Connection), Factory);
			ZExceptionReporting.HandleZSaveConcurrencyException(exception, notifier.Object, false);

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestUNTRateLineIsUpdatedWhenRaquiresWeightVolumeAndTM_BreakWeightVolumeChanged()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "USLAX").RateLines[0];
			Assert(line.TL_ContainerOwnership.IsEmpty);
			line.TL_RateCalculator = HighestRateCalculator.Code;

			var rateLineItem = line.RateLineItems.AddNew();
			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals(string.Empty, line.TL_WeightVolume);

			rateLineItem.TM_BreakWeightVolume = RatingConstants.Units.KG;
			AssertEquals(RatingConstants.Units.KG, line.TL_WeightVolume);
		}

		public void TestRateLineItemMustbePartOfCollection()
		{
			var testObjectFactory = new BusinessObjectFactory();
			var entry = testObjectFactory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			var rateLineItem1 = line.RateLineItems.AddNew();
			AssertEquals("Rate line item should be created via Collection.AddNew()", line.RateLineItems.Cast<RateLineItem>().LastOrDefault().PK, rateLineItem1.PK);
			AssertEquals("No developer exception should be raised", 0, ExceptionReporterTestListener.Instance.Count);

			var ratelineItem2 = testObjectFactory.New<RateLineItem>();
			AssertEquals("Developer Exception should be raised when you a create a rateline item using Factory.New()", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUNTRateLineItemIsUpdatedWhenRaquiresWeightVolumeAndTM_UnitMultipleChanged()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "USLAX").RateLines[0];
			Assert(line.TL_ContainerOwnership.IsEmpty);
			line.TL_RateCalculator = HighestRateCalculator.Code;

			var rateLineItem = line.RateLineItems.AddNew();
			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals(0m, line.TL_WeightVolumeMultiple);

			rateLineItem.TM_UnitMultiple = 100;
			AssertEquals(100m, line.TL_WeightVolumeMultiple);
		}

		public void TestUNTRateLineItemIsUpdatedWhenRaquiresWeightVolumeAndTM_TypeChanged()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "USLAX").RateLines[0];
			Assert(line.TL_ContainerOwnership.IsEmpty);
			line.TL_RateCalculator = HighestRateCalculator.Code;

			var rateLineItem1 = line.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.UNT;

			var rateLineItem2 = line.RateLineItems.AddNew();
			rateLineItem2.TM_Type = Calculator.Items.Operator.UNT;

			AssertEquals(string.Empty, line.TL_WeightVolume);

			rateLineItem1.TM_BreakWeightVolume = RatingConstants.Units.KG;
			AssertEquals(string.Empty, line.TL_WeightVolume);

			rateLineItem1.TM_Type = Calculator.Items.Operator.MIN;
			rateLineItem2.TM_BreakWeightVolume = RatingConstants.Units.KG;
			AssertEquals(RatingConstants.Units.KG, line.TL_WeightVolume);
		}

		public void TestRateOperatorIsNonPrintedFlag()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();

			var item = line.RateLineItems.AddNew();
			item.TM_Type = HighestRateCalculator.Items.RatePickRule;
			AssertEquals(true, item.RateOperatorIsNonPrintedFlag());
		}

		public void TestRateOperatorIsUnitAsFreighted()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();

			var item = line.RateLineItems.AddNew();
			Assert(!item.RateOperatorIsRatePickRule());

			item.TM_Type = HighestRateCalculator.Items.RatePickRule;
			Assert(item.RateOperatorIsRatePickRule());
		}

		public void TestRateOperatorIsApplyTo()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();

			var item = line.RateLineItems.AddNew();
			Assert(!item.RateOperatorIsApplyTo());

			item.TM_Type = CalculatorConstants.Type.ApplyTo;
			Assert(item.RateOperatorIsApplyTo());

			item.TM_Text = Calculator.Items.Value.CalculationOrder;
			Assert(!item.RateOperatorIsApplyTo());
		}

		public void TestRateOperatorIsApplyToOrMNT()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = MinimumCalculator.Code;

			var items = line.RateLineItems.Cast<RateLineItem>();

			AssertNotNull(items.FirstOrDefault(x => x.RateOperatorIsApplyToOrMNT()));
			AssertNotNull(items.FirstOrDefault(x => !x.RateOperatorIsApplyToOrMNT()));
		}

		public void TestRateOperatorIsCalculationOrder()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = PercentageCalculator.Code;
			Assert(((PercentageCalculator)line.Calculator).AddApplyToItem(Calculator.Items.Value.CalculationOrder).RateOperatorIsCalculationOrder());
		}

		public void TestCalculationOrderOrPercentOf()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = PercentageCalculator.Code;

			var item = ((PercentageCalculator)line.Calculator).AddApplyToItem(Calculator.Items.Value.CalculationOrder);
			AssertEquals(nameof(FieldType.Text), item.CalculationOrderOrPercentOfFieldType);
			item.CalculationOrderOrPercentOf = "22";
			AssertEquals("22", item.CalculationOrderOrPercentOf);
			AssertEquals(22m, item.TM_Value);

			var code = Factory.NewWithValidTestData<AccChargeCode>();

			item.TM_Text = CalculatorConstants.Text.ChargeCode;
			AssertEquals(nameof(FieldType.Guid), item.CalculationOrderOrPercentOfFieldType);
			item.CalculationOrderOrPercentOf = code.PK.ToString();
			AssertEquals(code.PK.ToString(), item.CalculationOrderOrPercentOf);
			AssertEquals(code.PK, item.TM_AC);
		}

		public void TestInvalidGuidOn_CalculationOrderOrPercentOf()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = PercentageCalculator.Code;

			var item = ((PercentageCalculator)line.Calculator).AddApplyToItem(CalculatorConstants.Type.ApplyTo);
			item.TM_Text = CalculatorConstants.Text.ChargeCode;

			var nonGuidString = "Non Guid String";
			AssertNoExceptionThrown("Unrecognized Guid Format should not cause exception.", () => item.CalculationOrderOrPercentOf = nonGuidString);
			item.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertEquals(nonGuidString, item.CalculationOrderOrPercentOf);

			var emptyGuidString = ZString.Empty;
			AssertNoExceptionThrown("Unrecognized Guid Format should not cause exception.", () => item.CalculationOrderOrPercentOf = emptyGuidString);
		}

		public void TestSetMinusAndPlusWeightBreaksToSameValue()
		{
			var client = Helper.NewOrgHeader();
			var testRate = Helper.NewClientRate(client);
			var line = testRate.AddRateEntry("DST", "AIR", "AUMEL", "AUSYD").AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);

			var minusBreak = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 10m);
			var plusBreak1 = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 8m);
			var plusBreak2 = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 6m);

			Factory.Save();

			minusBreak.TM_Break = 50m;
			AssertEquals("Should sync with plus break 1", 50m, plusBreak1.TM_Break);

			plusBreak1.TM_Break = 45m;
			AssertEquals("Should sync with minus break", 45m, minusBreak.TM_Break);

			plusBreak2.TM_Break = 90m;
			AssertEquals("Should remain the same", 45m, minusBreak.TM_Break);
			AssertEquals("Should remain the same", 45m, plusBreak1.TM_Break);
			AssertEquals("Should only set itself", 90m, plusBreak2.TM_Break);

			plusBreak1.TM_Break = 120m;
			AssertEquals("Minus break should always be set to lowest plus break", 90m, minusBreak.TM_Break);

			var plusBreak3 = line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 200m, 2m);

			AssertEquals("New line should not affect minus break as it is > lowest plus break", 90m, minusBreak.TM_Break);

			plusBreak3.TM_Break = 0m;

			AssertEquals("Expected to be synced as break 3 is now the lowest break", 0m, minusBreak.TM_Break);
			AssertHasError("Expected an error if break is 0", plusBreak3.TM_BreakInfo, ErrorMessages.WeightBreakRequired);
			AssertHasError("Expected an error if break is 0", minusBreak.TM_BreakInfo, ErrorMessages.WeightBreakRequired);

			plusBreak3.TM_Break = 60m;

			AssertNoError(plusBreak3.TM_BreakInfo, ErrorMessages.WeightBreakRequired);
			AssertNoError(minusBreak.TM_BreakInfo, ErrorMessages.WeightBreakRequired);

			plusBreak3.Delete();

			AssertEquals("Since the previous matching break was deleted should fall back to next rate", 90m, minusBreak.TM_Break);
		}

		public void TestSetMinusAndPlusWeightBreaksToSameValue_WithCartageZones()
		{
			var client = Helper.NewOrgHeader();
			var zoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			zoneSet.TP_OH_RelatedParty = client.PK;
			zoneSet.TP_RN_NKCountry = "AU";

			var zone = zoneSet.Zones.AddNew();
			zone.TZ_ZoneName = "NewZone";

			var testRate = Helper.NewClientRate(client);
			var line = testRate.AddRateEntry("DST", "AIR", "AUMEL", "AUSYD").AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var stdZoneMinusItem = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 45m, 5m, ZGuid.Empty);
			var stdZonePlusItem1 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 45m, 4m, ZGuid.Empty);
			var stdZonePlusItem2 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 100m, 3m, ZGuid.Empty);

			var newZoneMinusItem = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 45m, 3.5m, zone.PK);
			var newZonePlusItem1 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 45m, 3m, zone.PK);
			var newZonePlusItem2 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 100m, 2.5m, zone.PK);

			Factory.Save();

			AssertNotNull("Pre-condition", stdZoneMinusItem.ParentZone);
			AssertNotNull("Pre-condition", newZoneMinusItem.ParentZone);

			stdZoneMinusItem.TM_Break = 50m;
			AssertEquals("Plus Break", 50m, stdZonePlusItem1.TM_Break);

			stdZonePlusItem1.TM_Break = 45m;
			AssertEquals("Minus Break", 45m, stdZoneMinusItem.TM_Break);

			stdZonePlusItem2.TM_Break = 90m;
			AssertEquals("Minus Break", 45m, stdZoneMinusItem.TM_Break);
			AssertEquals("Minus Break", 45m, stdZonePlusItem1.TM_Break);
			AssertEquals("Minus Break", 90m, stdZonePlusItem2.TM_Break);

			newZoneMinusItem.TM_Break = 60m;
			AssertEquals("PlusBreak", 60m, newZonePlusItem1.TM_Break);
			AssertEquals("Should not affect minus item from another zone", 45M, stdZoneMinusItem.TM_Break);

			newZonePlusItem1.TM_Break = 45m;
			AssertEquals("Minus Break", 45m, newZoneMinusItem.TM_Break);
			AssertEquals("Should not affect lowest plus item from another zone", 45m, stdZonePlusItem1.TM_Break);

			newZonePlusItem2.TM_Break = 200m;
		}

		public void TestIsHighestPlusLine()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var line = quote.AddRateEntry("DST").RateLines.AddNew();

			var rateLineItem1 = line.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.MIN;

			var rateLineItem2 = line.RateLineItems.AddNew();
			rateLineItem2.TM_Type = Calculator.Items.Operator.Minus;

			var rateLineItem3 = line.RateLineItems.AddNew();
			rateLineItem3.TM_Break = 100;
			rateLineItem3.TM_Type = Calculator.Items.Operator.Plus;

			var rateLineItem4 = line.RateLineItems.AddNew();
			rateLineItem4.TM_Break = 400;
			rateLineItem4.TM_Type = Calculator.Items.Operator.Plus;

			AssertEquals("Highest Line Order", false, rateLineItem1.IsHighestPlus());
			AssertEquals("Highest Line Order", false, rateLineItem2.IsHighestPlus());
			AssertEquals("Highest Line Order", false, rateLineItem3.IsHighestPlus());
			AssertEquals("Highest Line Order", true, rateLineItem4.IsHighestPlus());

			rateLineItem4.TM_Break = 50;

			AssertEquals("Expected to be true as it is now higher than rate line 4", true, rateLineItem3.IsHighestPlus());
			AssertEquals(false, rateLineItem4.IsHighestPlus());
		}

		int IndexByTM_Type(RateLineItemsCollection collection, string tmType)
		{
			for (var i = 0; i < collection.Count; i++)
			{
				if (collection[i].TM_Type == tmType)
				{
					return i;
				}
			}

			return -1;
		}

		public void TestGetNextWeightBreak()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];

			var index = IndexByTM_Type(rateLine.RateLineItems, Calculator.Items.Operator.MIN);
			var rateLineItem1 = rateLine.RateLineItems[index++];
			rateLineItem1.TM_Break = 45;
			rateLineItem1.TM_Type = Calculator.Items.Operator.Minus;
			var rateLineItem2 = rateLine.RateLineItems[index++];
			rateLineItem2.TM_Break = 45;
			rateLineItem2.TM_Type = Calculator.Items.Operator.Plus;
			var rateLineItem3 = rateLine.RateLineItems[index++];
			rateLineItem3.TM_Break = 100;
			rateLineItem3.TM_Type = Calculator.Items.Operator.Plus;
			var rateLineItem4 = rateLine.RateLineItems[index++];
			rateLineItem4.TM_Break = 250;
			rateLineItem4.TM_Type = Calculator.Items.Operator.Plus;
			var rateLineItem5 = rateLine.RateLineItems[index++];
			rateLineItem5.TM_Break = 500;
			rateLineItem5.TM_Type = Calculator.Items.Operator.Plus;
			var rateLineItem6 = rateLine.RateLineItems[index++];
			rateLineItem6.TM_Break = 1000;
			rateLineItem6.TM_Type = Calculator.Items.Operator.Plus;
			var rateLineItem7 = rateLine.RateLineItems[index++];
			rateLineItem7.TM_Break = 2000;
			rateLineItem7.TM_Type = Calculator.Items.Operator.Plus;

			AssertEquals("Next Weight Break", 45M, rateLineItem1.NextWeightBreak());
			AssertEquals("Next Weight Break", 100M, rateLineItem2.NextWeightBreak());
			AssertEquals("Next Weight Break", 250M, rateLineItem3.NextWeightBreak());
			AssertEquals("Next Weight Break", 500M, rateLineItem4.NextWeightBreak());
			AssertEquals("Next Weight Break", 1000M, rateLineItem5.NextWeightBreak());
			AssertEquals("Next Weight Break", 2000M, rateLineItem6.NextWeightBreak());
			AssertEquals("Next Weight Break", -1M, rateLineItem7.NextWeightBreak());
		}

		public void TestValidatePrice()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);

			var lineItem = line.Calculator.AddRateLineItem("-", 50m, 100m);
			AssertNoWarnings(lineItem.TM_ValueInfo);
			lineItem.TM_Value = 0m;
			AssertHasWarning(lineItem.TM_ValueInfo, ErrorMessages.RatePriceIsZero);
			lineItem.TM_FlatAmount = 100m;
			lineItem.Validation.ValidateTM_Value();
			AssertNoWarnings(lineItem.TM_ValueInfo);

			lineItem = line.Calculator.AddRateLineItem("+", 50m, 100m);
			AssertNoWarnings(lineItem.TM_ValueInfo);
			lineItem.TM_Value = 0m;
			AssertHasWarning(lineItem.TM_ValueInfo, ErrorMessages.RatePriceIsZero);

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			lineItem = line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS);
			lineItem.TM_Value = 55m;
			AssertNoWarnings(lineItem.TM_ValueInfo);
			lineItem.TM_Value = 0m;
			AssertNoWarnings(lineItem.TM_ValueInfo);

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			lineItem = line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			lineItem.TM_Value = 55m;
			AssertNoWarnings(lineItem.TM_ValueInfo);
			lineItem.TM_Value = 0m;
			AssertNoWarnings(lineItem.TM_ValueInfo);
		}

		public void TestRelevantValue()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var rateLineItem1 = line.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.MIN;

			rateLineItem1.TM_RelevantValue = 10;

			AssertEquals(0m, rateLineItem1.TM_AgentDeclaredRate);
			AssertEquals(10m, rateLineItem1.TM_Value);

			line.ViewAgentRates = true;
			rateLineItem1.TM_RelevantValue = 20;

			AssertEquals(20m, rateLineItem1.TM_AgentDeclaredRate);
			AssertEquals(10m, rateLineItem1.TM_Value);

			line.ViewAgentRates = false;
			rateLineItem1.TM_Value = 30;

			AssertEquals(30m, rateLineItem1.TM_RelevantValue);

			line.ViewAgentRates = true;

			AssertEquals(20m, rateLineItem1.TM_RelevantValue);
		}

		public void TestValuesFilterDownOnSlidingAndCombinedCalculator()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var aIRRateEntry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var indexOfMin = IndexByTM_Type(aIRRateEntry.RateLines[0].RateLineItems, Calculator.Items.Operator.MIN);
			var item1 = aIRRateEntry.RateLines[0].RateLineItems[indexOfMin];
			var item2 = aIRRateEntry.RateLines[0].RateLineItems[indexOfMin + 1];
			var item3 = aIRRateEntry.RateLines[0].RateLineItems[indexOfMin + 2];
			var item4 = aIRRateEntry.RateLines[0].RateLineItems[indexOfMin + 3];
			var item5 = aIRRateEntry.RateLines[0].RateLineItems[indexOfMin + 4];
			var item6 = aIRRateEntry.RateLines[0].RateLineItems[indexOfMin + 5];
			var item7 = aIRRateEntry.RateLines[0].RateLineItems[indexOfMin + 6];

			for (var i = 1; i <= 2; i++)
			{
				aIRRateEntry.RateLines[0].ViewAgentRates = i == 2;

				AssertEquals("MIN Line", 0M, item1.TM_RelevantValue);
				AssertEquals("-45 Line", 0M, item2.TM_RelevantValue);
				AssertEquals("+45 Line", 0M, item3.TM_RelevantValue);
				AssertEquals("+100 Line", 0M, item4.TM_RelevantValue);
				AssertEquals("+250 Line", 0M, item5.TM_RelevantValue);
				AssertEquals("+500 Line", 0M, item6.TM_RelevantValue);
				AssertEquals("+1000 Line", 0M, item7.TM_RelevantValue);

				item1.TM_RelevantValue = 50M;
				AssertEquals("MIN Line", 50M, item1.TM_RelevantValue);
				AssertEquals("-45 Line", 0M, item2.TM_RelevantValue);
				AssertEquals("+45 Line", 0M, item3.TM_RelevantValue);
				AssertEquals("+100 Line", 0M, item4.TM_RelevantValue);
				AssertEquals("+250 Line", 0M, item5.TM_RelevantValue);
				AssertEquals("+500 Line", 0M, item6.TM_RelevantValue);
				AssertEquals("+1000 Line", 0M, item7.TM_RelevantValue);

				item2.TM_RelevantValue = 2.75M;
				AssertEquals("MIN Line", 50M, item1.TM_RelevantValue);
				AssertEquals("-45 Line", 2.75M, item2.TM_RelevantValue);
				AssertEquals("+45 Line", 2.75M, item3.TM_RelevantValue);
				AssertEquals("+100 Line", 2.75M, item4.TM_RelevantValue);
				AssertEquals("+250 Line", 2.75M, item5.TM_RelevantValue);
				AssertEquals("+500 Line", 2.75M, item6.TM_RelevantValue);
				AssertEquals("+1000 Line", 2.75M, item7.TM_RelevantValue);

				item3.TM_RelevantValue = 2.50M;
				AssertEquals("MIN Line", 50M, item1.TM_RelevantValue);
				AssertEquals("-45 Line", 2.75M, item2.TM_RelevantValue);
				AssertEquals("+45 Line", 2.50M, item3.TM_RelevantValue);
				AssertEquals("+100 Line", 2.50M, item4.TM_RelevantValue);
				AssertEquals("+250 Line", 2.50M, item5.TM_RelevantValue);
				AssertEquals("+500 Line", 2.50M, item6.TM_RelevantValue);
				AssertEquals("+1000 Line", 2.50M, item7.TM_RelevantValue);

				item4.TM_RelevantValue = 2.25M;
				AssertEquals("MIN Line", 50M, item1.TM_RelevantValue);
				AssertEquals("-45 Line", 2.75M, item2.TM_RelevantValue);
				AssertEquals("+45 Line", 2.50M, item3.TM_RelevantValue);
				AssertEquals("+100 Line", 2.25M, item4.TM_RelevantValue);
				AssertEquals("+250 Line", 2.25M, item5.TM_RelevantValue);
				AssertEquals("+500 Line", 2.25M, item6.TM_RelevantValue);
				AssertEquals("+1000 Line", 2.25M, item7.TM_RelevantValue);

				item5.TM_RelevantValue = 2.00M;
				AssertEquals("MIN Line", 50M, item1.TM_RelevantValue);
				AssertEquals("-45 Line", 2.75M, item2.TM_RelevantValue);
				AssertEquals("+45 Line", 2.50M, item3.TM_RelevantValue);
				AssertEquals("+100 Line", 2.25M, item4.TM_RelevantValue);
				AssertEquals("+250 Line", 2.00M, item5.TM_RelevantValue);
				AssertEquals("+500 Line", 2.00M, item6.TM_RelevantValue);
				AssertEquals("+1000 Line", 2.00M, item7.TM_RelevantValue);

				item6.TM_RelevantValue = 1.75M;
				AssertEquals("MIN Line", 50M, item1.TM_RelevantValue);
				AssertEquals("-45 Line", 2.75M, item2.TM_RelevantValue);
				AssertEquals("+45 Line", 2.50M, item3.TM_RelevantValue);
				AssertEquals("+100 Line", 2.25M, item4.TM_RelevantValue);
				AssertEquals("+250 Line", 2.00M, item5.TM_RelevantValue);
				AssertEquals("+500 Line", 1.75M, item6.TM_RelevantValue);
				AssertEquals("+1000 Line", 1.75M, item7.TM_RelevantValue);

				item7.TM_RelevantValue = 1.50M;
				AssertEquals("MIN Line", 50M, item1.TM_RelevantValue);
				AssertEquals("-45 Line", 2.75M, item2.TM_RelevantValue);
				AssertEquals("+45 Line", 2.50M, item3.TM_RelevantValue);
				AssertEquals("+100 Line", 2.25M, item4.TM_RelevantValue);
				AssertEquals("+250 Line", 2.00M, item5.TM_RelevantValue);
				AssertEquals("+500 Line", 1.75M, item6.TM_RelevantValue);
				AssertEquals("+1000 Line", 1.50M, item7.TM_RelevantValue);
			}

			Factory.Save();

			item4.TM_RelevantValue = 2.38M;
			AssertEquals("MIN Line", 50M, item1.TM_RelevantValue);
			AssertEquals("-45 Line", 2.75M, item2.TM_RelevantValue);
			AssertEquals("+45 Line", 2.50M, item3.TM_RelevantValue);
			AssertEquals("+100 Line", 2.38M, item4.TM_RelevantValue);
			AssertEquals("+250 Line", 2.00M, item5.TM_RelevantValue);
			AssertEquals("+500 Line", 1.75M, item6.TM_RelevantValue);
			AssertEquals("+1000 Line", 1.50M, item7.TM_RelevantValue);
		}

		public void TestReadOnly()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;
			var lineItem = line.RateLineItems.AddNew();
			lineItem.TM_Type = Calculator.Items.Operator.Plus;

			lineItem = line.RateLineItems.AddNew();
			AssertReadOnly(lineItem, /*Value*/ false, /*AgentDeclaredRate*/ false, /*FlatAmount*/ true, /*Break*/ true, /*BreakMinimum*/ true, /*CallForPricing*/ true, /*Text*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.MIN;
			AssertReadOnly(lineItem, /*Value*/ false, /*AgentDeclaredRate*/ false, /*FlatAmount*/ true, /*Break*/ false, /*BreakMinimum*/ true, /*CallForPricing*/ true, /*Text*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.BAS;
			AssertReadOnly(lineItem, /*Value*/ false, /*AgentDeclaredRate*/ false, /*FlatAmount*/ true, /*Break*/ true, /*BreakMinimum*/ true, /*CallForPricing*/ true, /*Text*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertReadOnly(lineItem, /*Value*/ false, /*AgentDeclaredRate*/ false, /*FlatAmount*/ true, /*Break*/ true, /*BreakMinimum*/ true, /*CallForPricing*/ true, /*Text*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.Minus;
			AssertReadOnly(lineItem, /*Value*/ false, /*AgentDeclaredRate*/ false, /*FlatAmount*/ false, /*Break*/ false, /*BreakMinimum*/ false, /*CallForPricing*/ true, /*Text*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertReadOnly(lineItem, /*Value*/ false, /*AgentDeclaredRate*/ false, /*FlatAmount*/ false, /*Break*/ false, /*BreakMinimum*/ false, /*CallForPricing*/ false, /*Text*/ true);

			line.GetCalculator<CombinedCalculator>().IsAccumulated = true;

			lineItem.TM_Type = Calculator.Items.Operator.Minus;
			AssertReadOnly(lineItem, /*Value*/ false, /*AgentDeclaredRate*/ false, /*FlatAmount*/ false, /*Break*/ false, /*BreakMinimum*/ true, /*CallForPricing*/ true, /*Text*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertReadOnly(lineItem, /*Value*/ false, /*AgentDeclaredRate*/ false, /*FlatAmount*/ false, /*Break*/ false, /*BreakMinimum*/ true, /*CallForPricing*/ false, /*Text*/ true);

			line.GetCalculator<CombinedCalculator>().IsAccumulated = false;

			lineItem.TM_Value = 10m;
			lineItem.TM_AgentDeclaredRate = 10m;
			lineItem.TM_FlatAmount = 100m;
			lineItem.TM_CallForPricing = true;
			AssertReadOnly(lineItem, /*Value*/ true, /*AgentDeclaredRate*/ true, /*FlatAmount*/ true, /*Break*/ false, /*BreakMinimum*/ true, /*CallForPricing*/ false, /*Text*/ false);
			AssertEquals(0m, lineItem.TM_Value);
			AssertEquals(0m, lineItem.TM_AgentDeclaredRate);
			AssertEquals(0m, lineItem.TM_FlatAmount);
		}

		public void TestReadOnly_BreaksPer()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();

			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.ORG;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				Assert("Breaks per is only applicable for Forwarding tab", rateEntry.IsForwarding());
				AssertEquals(true, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_RateCalculator = CombinedCalculator.Code;
				AssertReadOnlyForWeightUnit(false);
				AssertReadOnlyForRounding(false);

				line.TL_RateCalculator = TimeCalculator.Code;
				AssertReadOnlyForWeightUnit(true);
				AssertReadOnlyForRounding(true);

				line.TL_RateCalculator = CartageCalculator.Code;
				AssertReadOnlyForWeightUnit(false);
				AssertReadOnlyForRounding(false);

				line.TL_RateCalculator = CartageZoneDistanceCalculator.Code;
				AssertReadOnlyForWeightUnit(false);
				AssertReadOnlyForRounding(false);

				rateEntry.TI_RateCategory = RatingConstants.RateCategory.PAC;
				Assert("Other than Forwarding tab", !rateEntry.IsForwarding());
				AssertReadOnlyForWeightUnit(true);
				AssertReadOnlyForRounding(true);
			}

			void AssertReadOnlyForWeightUnit(bool expectedReadOnlyForWeightUnit)
			{
				//pre-requisites
				line.TL_Rounding = RatingRoundingTypes.NoRounding;

				line.TL_WeightVolume = string.Empty;
				AssertEquals(true, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_WeightVolume = Core.Constants.Weight.Kilograms;
				AssertEquals(expectedReadOnlyForWeightUnit, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_WeightVolume = Core.Constants.Volume.CubicMetres;
				AssertEquals(true, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_WeightVolume = Core.Constants.LoadingLength.LoadingMeters;
				AssertEquals(true, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_WeightVolume = Core.Constants.Weight.Grams;
				AssertEquals(expectedReadOnlyForWeightUnit, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_WeightVolume = Core.Constants.Weight.Pounds;
				AssertEquals(expectedReadOnlyForWeightUnit, line.Calculator.BreaksPerInfo.ReadOnly);
			}

			void AssertReadOnlyForRounding(bool expectedReadOnlyForWeightUnit)
			{
				//pre-requisites
				line.TL_WeightVolume = Core.Constants.Weight.Kilograms;

				line.TL_Rounding = RatingRoundingTypes.Chargeable;
				AssertEquals(true, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_Rounding = RatingRoundingTypes.NoRounding;
				AssertEquals(expectedReadOnlyForWeightUnit, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;
				AssertEquals(true, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_Rounding = RatingRoundingTypes.Bankers;
				AssertEquals(expectedReadOnlyForWeightUnit, line.Calculator.BreaksPerInfo.ReadOnly);

				line.TL_Rounding = RatingRoundingTypes.NoRounding;
				AssertEquals(expectedReadOnlyForWeightUnit, line.Calculator.BreaksPerInfo.ReadOnly);
			}
		}

		[ExpectNoExceptions]
		public void TestCallForPricingOnDeletedBusinessObject()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			var lineItem = line.RateLineItems.AddNew();
			Assert(!lineItem.TM_CallForPricingInfo.ReadOnly);

			lineItem.Delete();
			AssertNotNull(lineItem);

			((IAccessBusinessObject)lineItem).IsPropertyReadOnly("TM_CallForPricing");
			Assert("Why is it still read-only?", !lineItem.TM_CallForPricingInfo.ReadOnly);
		}

		public void TestReadOnlyCallForPricing_FirstPlusIsReadOnlyIfNoMinus()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_RateCalculator = CombinedCalculator.Code;
			rateLine.GetCalculator<CombinedCalculator>().IsAccumulated = false;
			rateLine.RateLineItems.RemoveAndDeleteAll();

			AssertEquals("Pre-condition", 0, rateLine.RateLineItems.Count);

			var rateLineItem1 = rateLine.RateLineItems.AddNew();
			rateLineItem1.TM_Break = 10m;

			rateLineItem1.TM_Type = Calculator.Items.Operator.Minus;

			AssertEquals("Expected to be read only for any non Plus Operator rate line", true, rateLineItem1.TM_CallForPricingInfo.ReadOnly);

			rateLineItem1.TM_Type = Calculator.Items.Operator.BAS;

			AssertEquals("Expected to be read only for any non Plus Operator rate line", true, rateLineItem1.TM_CallForPricingInfo.ReadOnly);

			rateLineItem1.TM_Type = Calculator.Items.Operator.Plus;

			AssertEquals("Expected to be read only as this rate line is the first plus rate line and there is no minus rate line", true, rateLineItem1.TM_CallForPricingInfo.ReadOnly);

			var rateLineItem2 = rateLine.RateLineItems.AddNew();
			rateLineItem2.TM_Break = 20m;
			rateLineItem2.TM_Type = Calculator.Items.Operator.Plus;

			AssertEquals("Expected to NOT be read only as it's not the first plus operator", false, rateLineItem2.TM_CallForPricingInfo.ReadOnly);

			var rateLineItem3 = rateLine.RateLineItems.AddNew();
			rateLineItem3.TM_Break = 50m;
			rateLineItem3.TM_Type = Calculator.Items.Operator.Plus;

			AssertEquals("Again, expected to NOT be read only as it's not the first plus operator", false, rateLineItem3.TM_CallForPricingInfo.ReadOnly);

			rateLineItem1.TM_Type = Calculator.Items.Operator.Minus;
			rateLineItem2.TM_Type = Calculator.Items.Operator.Plus;

			AssertEquals("Expected to NOT be read only even though it's the first plus operator, there is a minus before it", false, rateLineItem2.TM_CallForPricingInfo.ReadOnly);

			rateLineItem1.Delete();

			AssertEquals("Expected now to be read as it is the first plus operator and there are no minus lines", true, rateLineItem2.TM_CallForPricingInfo.ReadOnly);
			AssertEquals("Still not the first plus", false, rateLineItem3.TM_CallForPricingInfo.ReadOnly);
		}

		void AssertReadOnly(RateLineItem lineItem, bool valueReadOnly, bool agentDeclaredRateReadOnly, bool flatAmountReadOnly, bool breakReadOnly, bool breakMinimumReadOnly, bool callForPricingReadOnly, bool textReadOnly)
		{
			AssertEquals("TM_ValueInfo.ReadOnly", valueReadOnly, lineItem.TM_ValueInfo.ReadOnly);
			AssertEquals("TM_AgentDeclaredRateInfo.ReadOnly", agentDeclaredRateReadOnly, lineItem.TM_AgentDeclaredRateInfo.ReadOnly);
			AssertEquals("TM_FlatAmountInfo.ReadOnly", flatAmountReadOnly, lineItem.TM_FlatAmountInfo.ReadOnly);
			AssertEquals("TM_BreakInfo.ReadOnly", breakReadOnly, lineItem.TM_BreakInfo.ReadOnly);
			AssertEquals("TM_BreakMinimumInfo.ReadOnly", breakMinimumReadOnly, lineItem.TM_BreakMinimumInfo.ReadOnly);
			AssertEquals("TM_CallForPricingInfo.ReadOnly", callForPricingReadOnly, lineItem.TM_CallForPricingInfo.ReadOnly);
			AssertEquals("TM_TextInfo.ReadOnly", textReadOnly, lineItem.TM_TextInfo.ReadOnly);
		}

		public void TestReadOnlyForPEBCalculator()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = PercentageBreaksCalculator.Code;
			var lineItem = line.RateLineItems.AddNew();
			AssertReadOnly(lineItem, /*break*/ true, /*rate*/ false, /*percent*/ true, /*flatamount*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.MIN;
			AssertReadOnly(lineItem, /*break*/ true, /*rate*/ false, /*percent*/ true, /*flatamount*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.BAS;
			AssertReadOnly(lineItem, /*break*/ true, /*rate*/ false, /*percent*/ true, /*flatamount*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertReadOnly(lineItem, /*break*/ true, /*rate*/ false, /*percent*/ true, /*flatamount*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.Minus;
			AssertReadOnly(lineItem, /*break*/ false, /*rate*/ true, /*percent*/ false, /*flatamount*/ false);

			lineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertReadOnly(lineItem, /*break*/ false, /*rate*/ true, /*percent*/ false, /*flatamount*/ false);

			line.GetCalculator<PercentageBreaksCalculator>().UseBreaksBasedOnValues = true;

			AssertEquals("Line.TL_WeightVolume defaulted", "", line.TL_WeightVolume);
			AssertEquals("Line.TL_WeightVolumeMultipleInfo defaulted", 0m, line.TL_WeightVolumeMultiple);
			AssertEquals("Line.TL_WeightVolumeInfo.ReadOnly", true, line.TL_WeightVolumeInfo.ReadOnly);
			AssertEquals(true, line.UnitMultipleAsStringInfo.ReadOnly);
		}

		void AssertReadOnly(RateLineItem lineItem, bool @break, bool rate, bool percent, bool flatAmount)
		{
			AssertEquals("Break", @break, lineItem.TM_BreakInfo.ReadOnly);
			AssertEquals("Rate", rate, lineItem.TM_RelevantValueInfo.ReadOnly);
			AssertEquals("Percent", percent, lineItem.TM_BreakMinimumInfo.ReadOnly);
			AssertEquals("FlatAmount", flatAmount, lineItem.TM_FlatAmountInfo.ReadOnly);
		}

		public void TestReadOnlyForHighestRateCalculator()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = HighestRateCalculator.Code;
			var lineItem = line.RateLineItems.AddNew();
			AssertReadOnly(lineItem, /*units*/ true, /*rate*/ false, /*flatamount*/ false);

			lineItem.TM_Type = Calculator.Items.Operator.MIN;
			AssertReadOnly(lineItem, /*units*/ true, /*rate*/ false, /*flatamount*/ true);

			lineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertReadOnly(lineItem, /*units*/ false, /*rate*/ false, /*flatamount*/ false);
		}

		void AssertReadOnly(RateLineItem lineItem, bool units, bool rate, bool flatAmount)
		{
			AssertEquals("Units", units, lineItem.TM_BreakWeightVolumeInfo.ReadOnly);
			AssertEquals("Rate", rate, lineItem.TM_RelevantValueInfo.ReadOnly);
			AssertEquals("FlatAmount", flatAmount, lineItem.TM_FlatAmountInfo.ReadOnly);
		}

		public void TestBreakUnit_Warehouse_PackageLine_CombinedCalculator()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS, Enterprise.Core.Constants.RateMode.ALL, "AU", "US");
			var rateLine = rateEntry.AddRateLine("WHSCHG", CombinedCalculator.Code, RatingConstants.Units.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
			var rateLineItem = rateLine.RateLineItems.AddNew();

			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("BreakUnit is available for Warehouse RateLine with PKL Unit Factor", false, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);
		}

		public void TestBreakUnit()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var quoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var rateLine = quoteEntry.AddRateLine("ODOC", CombinedCalculator.Code, RatingConstants.Units.M3);
			var rateLineItem = rateLine.RateLineItems.AddNew();

			rateLineItem.TM_Type = Calculator.Items.Operator.MIN;
			Assert("Is ReadOnly", rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			Assert("Is ReadOnly", rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals(false, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals(false, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLine.TL_WeightVolume = QuantityUnit.KM;

			rateLineItem.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals(true, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals(true, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals(true, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals(true, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLine.TL_WeightVolume = RatingConstants.Units.CN;

			rateLineItem.TM_Type = Calculator.Items.Operator.MIN;
			Assert("Is ReadOnly", rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			Assert("Is ReadOnly", rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;
			Assert("Is NOT ReadOnly as there are no preceeding minus or plus items", !rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			Assert("Is NOT ReadOnly", !rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_BreakWeightVolume = RatingConstants.Units.KG;
			Assert("Has no Errors", !rateLineItem.TM_BreakWeightVolumeInfo.HasErrors());

			rateLineItem.TM_BreakWeightVolume = RatingConstants.Units.M3;
			Assert("Has no Errors", !rateLineItem.TM_BreakWeightVolumeInfo.HasErrors());

			rateLineItem.TM_BreakWeightVolume = "##";
			Assert("Has Errors", rateLineItem.TM_BreakWeightVolumeInfo.HasErrors());
			AssertEquals("Error Message", "Enter a valid " + rateLineItem.TM_BreakWeightVolumeInfo.Description + ".", rateLineItem.TM_BreakWeightVolumeInfo.GetErrors().GetFirstMessage());

			rateLineItem.TM_BreakWeightVolume = "";
			Assert("Has no Errors", !rateLineItem.TM_BreakWeightVolumeInfo.HasErrors());

			rateLineItem.TM_BreakWeightVolume = RatingConstants.Units.CN;
			Assert("Has Errors", rateLineItem.TM_BreakWeightVolumeInfo.HasErrors());
			AssertEquals("Error Message", "Enter a valid " + rateLineItem.TM_BreakWeightVolumeInfo.Description + ".", rateLineItem.TM_BreakWeightVolumeInfo.GetErrors().GetFirstMessage());
		}

		public void TestTimeCalculatorBreakUnit()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var rateLine = rateEntry.AddRateLine("ODOC", TimeCalculator.Code, RatingConstants.Units.CN);
			rateLine.RateLineItems.RemoveAndDeleteAll();
			var rateLineItem = rateLine.RateLineItems.AddNew();

			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals(false, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Is ReadOnly as MIN is not a break unit", true, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("First plus should act like a minus", false, rateLineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			rateLineItem.TM_BreakWeightVolume = "##";

			var expectedError = ZString.Format("Enter a valid {0}.", rateLineItem.TM_BreakWeightVolumeInfo.Description);
			Assert("Has Errors", rateLineItem.TM_BreakWeightVolumeInfo.HasErrors());
			AssertHasError(rateLineItem.TM_BreakWeightVolumeInfo, expectedError);

			rateLineItem.TM_BreakWeightVolume = RatingConstants.Units.M3;

			Assert("Has Errors", rateLineItem.TM_BreakWeightVolumeInfo.HasErrors());
			AssertHasError(rateLineItem.TM_BreakWeightVolumeInfo, expectedError);

			rateLineItem.TM_BreakWeightVolume = RatingConstants.Units.HR;

			AssertEquals(false, rateLineItem.TM_BreakWeightVolumeInfo.HasErrors());
		}

		public void TestTimeUnit()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_RateCalculator = TimeCalculator.Code;

			var testQuote = Factory.New<Quote>();
			var testEntry = testQuote.AddRateEntry("ORG");
			var testRateLine = testEntry.RateLines.AddNew();
			testRateLine.TL_AC = chargeCode.PK;
			var lineItem = testRateLine.RateLineItems.AddNew();

			lineItem.TM_Type = Calculator.Items.Operator.MIN;
			Assert("Is ReadOnly", lineItem.TM_BreakWeightVolumeInfo.ReadOnly);
			Assert("Has no Errors", !lineItem.TM_BreakWeightVolumeInfo.HasErrors());

			lineItem.TM_Type = Calculator.Items.Operator.UNT;
			Assert("Is NOT ReadOnly", !lineItem.TM_BreakWeightVolumeInfo.ReadOnly);
			lineItem.RunPreSaveValidation();
			Assert("Has Errors", lineItem.TM_BreakWeightVolumeInfo.HasErrors());
			AssertEquals("Error Message", "Please enter a " + lineItem.TM_BreakWeightVolumeInfo.Description + ".", lineItem.TM_BreakWeightVolumeInfo.GetErrors().GetFirstMessage());

			lineItem.TM_BreakWeightVolume = RatingConstants.Units.DY;
			Assert("Has no Errors", !lineItem.TM_BreakWeightVolumeInfo.HasErrors());

			lineItem.TM_BreakWeightVolume = RatingConstants.Units.HR;
			Assert("Has no Errors", !lineItem.TM_BreakWeightVolumeInfo.HasErrors());

			lineItem.TM_Type = Calculator.Items.Operator.Minus;
			lineItem.TM_Break = 50m;
			Assert("Is not ReadOnly", !lineItem.TM_BreakWeightVolumeInfo.ReadOnly);
			lineItem.RunPreSaveValidation();
			Assert("Has no Errors", !lineItem.TM_BreakWeightVolumeInfo.HasErrors());

			lineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertNull("Pre-condition: expected no minus item", testRateLine.FindRateLineItem(Calculator.Items.Operator.Minus));
			Assert("Is not read only as it's the lowest plus", !lineItem.TM_BreakWeightVolumeInfo.ReadOnly);

			var newItem = testRateLine.RateLineItems.AddNew();
			newItem.TM_Type = Calculator.Items.Operator.Plus;
			newItem.TM_Break = 150m;
			Assert("Should be read only as it's not the highest plus", newItem.TM_BreakWeightVolumeInfo.ReadOnly);
		}

		public void TestValidateCalculationOrder()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_RateCalculator = CombinedCalculator.Code;

			var testQuote = Factory.New<Quote>();
			var testEntry = testQuote.AddRateEntry("ORG");
			var testRateLine = testEntry.RateLines.AddNew();
			testRateLine.TL_AC = chargeCode.PK;
			testRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

			var bas = testRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS);
			var unt = testRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			var per = testRateLine.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER);
			var calculationOrder = testRateLine.RateLineItems.FindByTM_Type(CompanyTariffOrCostBasedCalculator.Items.CalculationOrder);
			calculationOrder.Validation.ValidateTM_Text();
			Assert("Has no Errors", !calculationOrder.TM_TextInfo.HasErrors());

			bas.TM_Value = 100M;
			calculationOrder.Validation.ValidateTM_Text();
			Assert("Has no Errors", !calculationOrder.TM_TextInfo.HasErrors());

			unt.TM_Value = 10M;
			calculationOrder.Validation.ValidateTM_Text();
			Assert("Has no Errors", !calculationOrder.TM_TextInfo.HasErrors());

			per.TM_Value = 5M;
			calculationOrder.RunPreSaveValidation();
			Assert("Has Errors", calculationOrder.TM_TextInfo.HasErrors());
			AssertEquals("Error Message", "Calculation Order should be set when both Percentage and Fixed Change are set.", calculationOrder.TM_TextInfo.GetErrors().GetFirstMessage());

			bas.TM_Value = 0M;
			unt.TM_Value = 0M;
			calculationOrder.RunPreSaveValidation();
			Assert("Has no Errors", !calculationOrder.TM_TextInfo.HasErrors());

			calculationOrder.TM_Text = "XXX";
			Assert("Has Errors", calculationOrder.TM_TextInfo.HasErrors());
			AssertEquals("Error Message", "Enter a valid selection.", calculationOrder.TM_TextInfo.GetErrors().GetFirstMessage());

			calculationOrder.TM_Text = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			Assert("Has no Errors", !calculationOrder.TM_TextInfo.HasErrors());

			bas.TM_Value = 50M;
			calculationOrder.RunPreSaveValidation();
			Assert("Has no Errors", !calculationOrder.TM_TextInfo.HasErrors());

			calculationOrder.TM_Text = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			Assert("Has no Errors", !calculationOrder.TM_TextInfo.HasErrors());
		}

		public void TestEquipmentTypeDesc()
		{
			var container = Factory.New<RefContainer>();
			container.RC_ShippingMode = "ROA";
			container.RC_Code = "RZUB";
			container.RC_Description = "Zubin Truck";

			var container2 = Factory.New<RefContainer>();
			container2.RC_ShippingMode = "ROA";
			container2.RC_Code = "RRAK";
			container2.RC_Description = "Rakhsh Truck";

			Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			line.TL_RateCalculator = EquipmentHireCalculator.Code;

			var testLineItem = line.Calculator.AddRateLineItem("", 0m, 0m, "HR");
			testLineItem.TM_Text = "RZUB";
			AssertEquals("Zubin Truck", testLineItem.EquipmentTypeDesc);

			testLineItem.TM_Text = "###";
			AssertEquals("", testLineItem.EquipmentTypeDesc);

			testLineItem.TM_Text = "RRAK";
			AssertEquals("Rakhsh Truck", testLineItem.EquipmentTypeDesc);
		}

		public void TestWarehousePackageTypeDesc()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			line.TL_RateCalculator = WarehousePackCalculator.Code;

			var testLineItem = line.Calculator.AddRateLineItem("UNT", 0m, 0m);
			AssertEquals("Unit", testLineItem.WarehousePackageTypeDesc);

			testLineItem.TM_Type = "###";
			AssertEquals("", testLineItem.WarehousePackageTypeDesc);

			testLineItem.TM_Type = "PLT";
			AssertEquals("Pallet", testLineItem.WarehousePackageTypeDesc);
		}

		public void TestWarehouseLocationTypeDesc()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			line.TL_RateCalculator = WarehouseLocationTypeCalculator.Code;

			var locationType = (BusinessObject)new RateLineItemsLookups(line.RateLineItems.AddNew()).WarehouseLocationTypes[0];
			var testLineItem = line.Calculator.AddRateLineItem((ZString)locationType[WhsLocationTypeSchema.WLT_Code], 0m, 0m);
			AssertEquals(locationType[WhsLocationTypeSchema.WLT_Description], testLineItem.WarehouseLocationTypeDesc);

			testLineItem.TM_Type = "###";
			AssertEquals("", testLineItem.WarehouseLocationTypeDesc);

			testLineItem.TM_Type = "";
			AssertEquals("", testLineItem.WarehouseLocationTypeDesc);
		}

		#region Percentage Calc

		public void TestValidateAC_PercentWhenRateCalcIsPercentageCalc()
		{
			var dummyChargeCode1 = Factory.New<AccChargeCode>();
			dummyChargeCode1.AC_RateCalculator = PercentageCalculator.Code;

			var dummyChargeCode2 = Factory.New<AccChargeCode>();

			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_AC = dummyChargeCode1.PK;
			var item = line.RateLineItems.AddNew();
			item.TM_Type = CalculatorConstants.Type.ApplyTo;
			item.TM_Text = CalculatorConstants.Text.AllCharges;

			item.RunPreSaveValidation();
			AssertEquals("Has Errors", false, item.TM_ACInfo.HasErrors());

			item.TM_Text = CalculatorConstants.Text.ChargeCode;
			item.RunPreSaveValidation();
			AssertEquals("Has Errors", true, item.TM_ACInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.ChargeCodeRequiredForPercentageCalculator, item.TM_ACInfo.GetErrors().GetFirstMessage());

			item.TM_AC = dummyChargeCode1.PK;
			AssertEquals("Has Errors", true, item.TM_ACInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.SameChargeCodes, item.TM_ACInfo.GetErrors().GetFirstMessage());

			item.TM_AC = dummyChargeCode2.PK;
			item.RunPreSaveValidation();
			AssertEquals("Has Errors", false, item.TM_ACInfo.HasErrors());
		}

		public void TestTM_ACReadOnly()
		{
			var dummyChargeCode1 = Factory.New<AccChargeCode>();
			dummyChargeCode1.AC_RateCalculator = PercentageCalculator.Code;

			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_AC = dummyChargeCode1.PK;
			var item = line.RateLineItems.AddNew();
			item.TM_Type = CalculatorConstants.Type.ApplyTo;

			item.TM_Text = CalculatorConstants.Text.AllCharges;
			AssertEquals(true, item.TM_ACInfo.ReadOnly);

			item.TM_Text = CalculatorConstants.Text.ChargeCode;
			AssertEquals(false, item.TM_ACInfo.ReadOnly);

			item.TM_AC = Helper.ChargeCodes["ODOC"].PK;
			Assert(!item.TM_AC.IsEmpty);

			item.TM_Text = CalculatorConstants.Text.OriginCharges;
			Assert(item.TM_AC.IsEmpty);
			AssertEquals(true, item.TM_ACInfo.ReadOnly);
		}

		#endregion

		public void TestClone()
		{
			var entry = Factory.New<ClientRate>().AddRateEntry("ORG");
			var line = entry.AddRateLine("OCART", FlatCalculator.Code);
			var lineItem = line.RateLineItems.AddNew();
			lineItem.TM_Text = "Hello";

			var clonedLineItem = line.RateLineItems.CloneItem(lineItem);

			Assert("Different objects", lineItem != clonedLineItem);
		}

		#region Apply To Description

		public void TestApplyToDescription()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_AC = Helper.ChargeCodes.New("TESTREV", "Test REV", FlatCalculator.Code, "ORG").PK;
			line.TL_RateDesc = "DAPH MOO MOO NOLE JAMO";

			AssertApplyToDescription(line, PercentageCalculator.Code, true);
			AssertApplyToDescription(line, PercentageBreaksCalculator.Code, true);
			AssertApplyToDescription(line, DisbursementInterestCalculator.Code, true);
			AssertApplyToDescription(line, FlatCalculator.Code, false);
		}

		static void AssertApplyToDescription(RateLine line, string calculatorCode, bool expected)
		{
			line.TL_RateCalculator = calculatorCode;
			line.RateLineItems.RemoveAndDeleteAll();

			var item = line.RateLineItems.AddNew();
			item.TM_Type = CalculatorConstants.Type.ApplyTo;
			item.TM_Text = Calculator.Items.Value.DisbursementApplyToTypes.Disbursements;

			var expectedDescription = expected ? Calculator.Items.Value.DisbursementApplyToTypesDescription.DisbursementsDescription : "";
			AssertEquals(expectedDescription, item.ApplyToDescription);
		}

		#endregion

		public void TestCascadeBreakAmountsForCTZCalculator()
		{
			var client = Helper.NewOrgHeader();
			var provider = Factory.New<RateTransportProvider>();
			var zone1 = provider.Zones.AddNew();
			zone1.TZ_ZoneName = "zone1";
			var zone2 = provider.Zones.AddNew();
			zone2.TZ_ZoneName = "zone2";
			provider.TP_RN_NKCountry = "AU";
			provider.TP_OH_RelatedParty = client.PK;

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);

			var lineItem1 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 1m, 0m, zone1.PK);
			var lineItem2 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 2m, 0m, zone1.PK);
			var lineItem3 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 1m, 0m, zone2.PK);
			var lineItem4 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 2m, 0m, zone2.PK);
			var lineItem5 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 3m, 0m, zone1.PK);
			var lineItem6 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 3m, 0m, zone2.PK);

			AssertEquals(0m, lineItem1.TM_Value);
			AssertEquals(0m, lineItem2.TM_Value);
			AssertEquals(0m, lineItem3.TM_Value);
			AssertEquals(0m, lineItem4.TM_Value);
			AssertEquals(0m, lineItem5.TM_Value);
			AssertEquals(0m, lineItem6.TM_Value);

			lineItem1.TM_Value = 3m;
			AssertEquals(3m, lineItem1.TM_Value);
			AssertEquals(3m, lineItem2.TM_Value);
			AssertEquals(0m, lineItem3.TM_Value);
			AssertEquals(0m, lineItem4.TM_Value);
			AssertEquals(3m, lineItem5.TM_Value);
			AssertEquals(0m, lineItem6.TM_Value);

			lineItem3.TM_Value = 6m;
			AssertEquals(3m, lineItem1.TM_Value);
			AssertEquals(3m, lineItem2.TM_Value);
			AssertEquals(6m, lineItem3.TM_Value);
			AssertEquals(6m, lineItem4.TM_Value);
			AssertEquals(3m, lineItem5.TM_Value);
			AssertEquals(6m, lineItem6.TM_Value);
		}

		public void TestCascadeBreakAmountsForCTZCalculatorForACIZones()
		{
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("DST", "AIR", "AUSYD", "");
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			line.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = true;

			var aciZoneA = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneA.F1_Zone = "ACI-A";
			aciZoneA.F1_RL_NKLoco = "AUSYD";
			var aciZoneB = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneB.F1_Zone = "ACI-B";
			aciZoneB.F1_RL_NKLoco = "AUSYD";

			Factory.Save();

			var lineItem1 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 1m, 0m, "ACI-A");
			var lineItem2 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 2m, 0m, "ACI-A");
			var lineItem3 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 1m, 0m, "ACI-B");
			var lineItem4 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 2m, 0m, "ACI-B");
			var lineItem5 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 3m, 0m, "ACI-A");
			var lineItem6 = line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 3m, 0m, "ACI-B");

			AssertEquals(0m, lineItem1.TM_Value);
			AssertEquals(0m, lineItem2.TM_Value);
			AssertEquals(0m, lineItem3.TM_Value);
			AssertEquals(0m, lineItem4.TM_Value);
			AssertEquals(0m, lineItem5.TM_Value);
			AssertEquals(0m, lineItem6.TM_Value);

			lineItem1.TM_Value = 3m;
			AssertEquals(3m, lineItem1.TM_Value);
			AssertEquals(3m, lineItem2.TM_Value);
			AssertEquals(0m, lineItem3.TM_Value);
			AssertEquals(0m, lineItem4.TM_Value);
			AssertEquals(3m, lineItem5.TM_Value);
			AssertEquals(0m, lineItem6.TM_Value);

			lineItem3.TM_Value = 6m;
			AssertEquals(3m, lineItem1.TM_Value);
			AssertEquals(3m, lineItem2.TM_Value);
			AssertEquals(6m, lineItem3.TM_Value);
			AssertEquals(6m, lineItem4.TM_Value);
			AssertEquals(3m, lineItem5.TM_Value);
			AssertEquals(6m, lineItem6.TM_Value);
		}

		public void TestUpdateRateValue()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var rateLine = rateEntry.RateLines.AddNew();
			var lineItem = rateLine.RateLineItems.AddNew();
			lineItem.TM_Value = 10m;
			lineItem.TM_AgentDeclaredRate = 20m;
			rateLine.ViewAgentRates = false;

			lineItem.UpdateRateValue(x => { return x + 5m; }, RateLineItem.RateTypeToUpdate.Relevant);
			AssertEquals(15m, lineItem.TM_Value);
			AssertEquals(20m, lineItem.TM_AgentDeclaredRate);

			rateLine.ViewAgentRates = true;
			lineItem.UpdateRateValue(x => { return x - 10m; }, RateLineItem.RateTypeToUpdate.Relevant);
			AssertEquals(15m, lineItem.TM_Value);
			AssertEquals(10m, lineItem.TM_AgentDeclaredRate);

			lineItem.UpdateRateValue(x => { return x * 2m; }, RateLineItem.RateTypeToUpdate.Standard);
			AssertEquals(30m, lineItem.TM_Value);
			AssertEquals(10m, lineItem.TM_AgentDeclaredRate);

			lineItem.UpdateRateValue(x => { return x + 15m; }, RateLineItem.RateTypeToUpdate.Agent);
			AssertEquals(30m, lineItem.TM_Value);
			AssertEquals(25m, lineItem.TM_AgentDeclaredRate);

			lineItem.UpdateRateValue(x => { return x - (x / 5m); }, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(24m, lineItem.TM_Value);
			AssertEquals(20m, lineItem.TM_AgentDeclaredRate);
		}

		public void TestGetDiscountedValue()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry(RatingConstants.RateCategory.ORG);
			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			var item1 = line1.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			item1.TM_Value = 10m;
			item1.TM_AgentDeclaredRate = 20m;
			item1.TM_FlatAmount = 30m;
			Factory.Save();

			var loadedTariff1 = new BusinessObjectFactory().Load<CompanyTariff>(tariff1.PK);
			var loadedLine1 = loadedTariff1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0].RateLines[0];
			var loadedItem1 = loadedLine1.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			AssertEquals("Value is not discounted", 10m, loadedItem1.TM_Value);
			AssertEquals("Value is not discounted", 20m, loadedItem1.TM_AgentDeclaredRate);
			AssertEquals("Value is not discounted", 30m, loadedItem1.TM_FlatAmount);
			AssertEquals("GetDiscount() method is not called", false, loadedLine1.IsCalculatorInitialized);

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			var entry2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var line2 = entry2.RateLines[0];
			var item2 = line2.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);

			AssertEquals("Value is discounted by 20%", 8m, item2.TM_Value);
			AssertEquals("Value is discounted by 20%", 16m, item2.TM_AgentDeclaredRate);
			AssertEquals("Value is discounted by 20%", 24m, item2.TM_FlatAmount);
			AssertEquals("GetDiscount() method is called", true, line2.IsCalculatorInitialized);

			item2.TM_Value = 15m;
			item2.TM_AgentDeclaredRate = 25m;
			item2.TM_FlatAmount = 40m;
			AssertEquals("Value is discounted by 20%", 12m, item2.TM_Value);
			AssertEquals("Value is discounted by 20%", 20m, item2.TM_AgentDeclaredRate);
			AssertEquals("Value is discounted by 20%", 32m, item2.TM_FlatAmount);
		}

		public void TestGetDiscountedValueDoesNotApplyToFeesAndChargesLineItems()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			tariff1.TH_GlobalRateLevel = 1;
			var entry1 = tariff1.AddRateEntry(RatingConstants.RateCategory.ORG);

			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			var item1 = line1.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			item1.TM_Value = 10m;
			item1.TM_AgentDeclaredRate = 20m;

			var line2 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			line2.TL_FeeChargeType = "FSC";
			line2.TL_FeeChargeLevel = "STD";
			var item2 = line2.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			item2.TM_Value = 10m;
			item2.TM_AgentDeclaredRate = 20m;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var tariff2 = newFactory.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);

			var entry2 = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			var tariff2Line1 = entry2.RateLines[0];
			var line2Item1 = tariff2Line1.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);

			AssertEquals("Value is discounted by 20%", 8m, line2Item1.TM_Value);
			AssertEquals("Value is discounted by 20%", 16m, line2Item1.TM_AgentDeclaredRate);

			var tariff2Line2 = entry2.RateLines[1];
			var line2Item2 = tariff2Line2.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);

			AssertEquals("Should no be discounted as rate line has fees and charges applied", 10m, line2Item2.TM_Value);
			AssertEquals("Should no be discounted as rate line has fees and charges applied", 20m, line2Item2.TM_AgentDeclaredRate);
		}

		public void TestValuesAreRoundedAccordingToRegistrySettings()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var entry1 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			var entry2 = clientRate.AddRateEntry("LCL", "LCL", "AUSYD", "NZAKL");

			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			var item1 = line1.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			item1.TM_Value = 10.4567m;
			item1.TM_AgentDeclaredRate = 20.9876m;
			item1.TM_FlatAmount = 30.1234m;

			var line2 = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			var item2 = line2.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);
			item2.TM_Value = 11.3423;
			item2.TM_AgentDeclaredRate = 21.8843;
			item2.TM_FlatAmount = 33.9009;

			Factory.Save();

			var loadedRate1 = new BusinessObjectFactory().Load<ClientRate>(clientRate.PK);
			var loadedLine1 = loadedRate1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL)[0].RateLines[1];
			var loadedItem1 = loadedLine1.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);

			var loadedLine2 = loadedRate1.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL)[0].RateLines[1];
			var loadedItem2 = loadedLine2.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT);

			var newNumberOfDecimalsAllowed = new SellRatesDecimalsCollection()
				{
					new SellRatesDecimals() { Code = "FCL", Decimals = "3" },
					new SellRatesDecimals() { Code = "LCL", Decimals = "2" },
				};

			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newNumberOfDecimalsAllowed))
			{
				AssertEquals("Value is rounded", 10.457m, loadedItem1.TM_Value);
				AssertEquals("Value is rounded", 20.988m, loadedItem1.TM_AgentDeclaredRate);
				AssertEquals("Value is rounded", 30.123m, loadedItem1.TM_FlatAmount);
				AssertEquals("Value is rounded", 11.34m, loadedItem2.TM_Value);
				AssertEquals("Value is rounded", 21.88m, loadedItem2.TM_AgentDeclaredRate);
				AssertEquals("Value is rounded", 33.90m, loadedItem2.TM_FlatAmount);
			}
		}

		public void TestTMTextSetter_ValueIsUpdated_FirePropertyChangedEvent()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = PercentageBreaksCalculator.Code;

			var lineItemToTest = line.RateLineItems.AddNew();
			lineItemToTest.TM_Text = "COD";

			var propertyChangedEventRaised = false;
			lineItemToTest.PropertyUpdated += (o, e) =>
			{
				if (e.PropertyName == RateLineItem.Schema.CalculationOrderOrPercentOfFieldType)
				{
					propertyChangedEventRaised = true;
				}
			};

			lineItemToTest.TM_Text = "SEQ";
			AssertEquals("PropertyChanged event with expected propetyName is fired", true, propertyChangedEventRaised);
		}

		public void TestTMTextSetter_ValueIsUpdated_RefreshBinding()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = AgencyCalculator.Code;

			var calc = line.GetCalculator<AgencyCalculator>();

			var agencyFeeTypeLine = line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyFeeType);
			var agencyLineTypeLine = line.RateLineItems.FindByTM_Type(AgencyCalculator.Items.AgencyLineType);
			agencyFeeTypeLine.TM_Type = AgencyCalculator.Items.AgencyFeeType;

			agencyFeeTypeLine.TM_Text = RateFeeTypeList.Codes.PerShipment;
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().IncludedHeadersInfo.ReadOnly);
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().AdditionalRateInfo.ReadOnly);
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().IncludedLinesInfo.ReadOnly);
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().MaximumLinesInfo.ReadOnly);
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().PerAdditionalLineInfo.ReadOnly);

			agencyFeeTypeLine.TM_Text = RateFeeTypeList.Codes.PerEntry;
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().AdditionalRateInfo.ReadOnly);
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().IncludedHeadersInfo.ReadOnly);
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().IncludedLinesInfo.ReadOnly);
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().MaximumLinesInfo.ReadOnly);
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().PerAdditionalLineInfo.ReadOnly);

			agencyLineTypeLine.TM_Text = RateLineTypeList.Codes.PerTariffLinePerEntry;
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().AdditionalRateInfo.ReadOnly);
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().IncludedHeadersInfo.ReadOnly);
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().IncludedLinesInfo.ReadOnly);
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().MaximumLinesInfo.ReadOnly);
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().PerAdditionalLineInfo.ReadOnly);

			agencyFeeTypeLine.TM_Text = RateFeeTypeList.Codes.PerShipment;
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().IncludedHeadersInfo.ReadOnly);
			AssertEquals(true, line.GetCalculator<AgencyCalculator>().AdditionalRateInfo.ReadOnly);
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().IncludedLinesInfo.ReadOnly);
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().MaximumLinesInfo.ReadOnly);
			AssertEquals(false, line.GetCalculator<AgencyCalculator>().PerAdditionalLineInfo.ReadOnly);
		}

		public void TestTMTextSetter_CallsMarkAsNeedingValidationIncludingChildren()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var line = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], PercentageCalculator.Code, "", "AUD");
			line.RateLineItems.RemoveAndDeleteAll();

			var lineItem = line.RateLineItems.AddNew();
			lineItem.TM_Type = CalculatorConstants.Type.ApplyTo;

			Dictionary<string, int> markedAsNeedingValidationCount = new Dictionary<string, int>();
			markedAsNeedingValidationCount.Add(rateEntry.GetType().ToString(), 0);
			markedAsNeedingValidationCount.Add(line.GetType().ToString(), 0);
			markedAsNeedingValidationCount.Add(lineItem.GetType().ToString(), 0);

			Factory.MarkedAsNeedingValidation += (BusinessObject obj) =>
			{
				if (markedAsNeedingValidationCount.ContainsKey(obj.GetType().ToString()))
				{
					markedAsNeedingValidationCount[obj.GetType().ToString()]++;
				}
			};

			Assert("Precondition: lineItem.IsValidationSuspended should be false.", !lineItem.IsValidationSuspended);

			lineItem.TM_Text = "BAF";
			AssertMarkedAsNeedingValidationCount("MarkAsNeedingValidationIncludingChildren should be called as IsValidationSuspended is false. MarkedAsNeedingValidationCount should be 1.", 1);

			using (lineItem.GetValidationSuspender())
			{
				Assert("Precondition: lineItem.IsValidationSuspended should be true.", lineItem.IsValidationSuspended);

				lineItem.TM_Text = "CAF";
				AssertMarkedAsNeedingValidationCount("MarkAsNeedingValidationIncludingChildren should not be called as IsValidationSuspended is true. MarkedAsNeedingValidationCount should be 1.", 1);
			}
			Assert("Precondition: lineItem.IsValidationSuspended should be false.", !lineItem.IsValidationSuspended);

			lineItem.TM_Text = "WAR";
			AssertMarkedAsNeedingValidationCount("MarkAsNeedingValidationIncludingChildren should be called as IsValidationSuspended is false. MarkedAsNeedingValidationCount should be 2.", 2);

			void AssertMarkedAsNeedingValidationCount(string message, int expected)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("MarkedAsNeedingValidationCount:RateEntry", expected, markedAsNeedingValidationCount[rateEntry.GetType().ToString()]);
					AssertEquals("MarkedAsNeedingValidationCount:RateLine", expected, markedAsNeedingValidationCount[line.GetType().ToString()]);
					AssertEquals("MarkedAsNeedingValidationCount:RateLineItem", expected, markedAsNeedingValidationCount[lineItem.GetType().ToString()]);
				});
			}
		}

		public void TestTMTypeSetter_ValueIsUpdated_FirePropertyChangedEvent()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("ORG");
			var line = rateEntry.RateLines.AddNew();
			line.TL_RateCalculator = PercentageBreaksCalculator.Code;

			var lineItemToTest = line.RateLineItems.AddNew();
			lineItemToTest.TM_Type = Calculator.Items.Value.CalculationOrder;

			var propertyChangedEventRaised = false;
			lineItemToTest.PropertyUpdated += (o, e) =>
			{
				if (e.PropertyName == RateLineItem.Schema.CalculationOrderOrPercentOfFieldType)
				{
					propertyChangedEventRaised = true;
				}
			};

			lineItemToTest.TM_Type = CalculatorConstants.Type.ApplyTo;
			AssertEquals("PropertyChanged event with expected propetyName is fired", true, propertyChangedEventRaised);
		}

		public void TestClearReadOnlyFields()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			tariff1.TH_GlobalRateLevel = 1;
			var tariff1Entry = tariff1.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "", "AU");
			var tariff1Line = tariff1Entry.AddRateLine("DDOC", Calculator.Items.Operator.UNT, Core.Constants.Volume.CubicMetres);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var tariff2 = newFactory.New<CompanyTariff>();

			tariff2.TH_GlobalRateLevel = 2;
			var tariff2Line = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST)[0].RateLines[0];

			Assert("Pre-condition", tariff2Line.IsTariffLineInherited);

			var tariff2Item = tariff2Line.RateLineItems[0];

			AssertNotNull("Expected a UNT rate line item", tariff2Item);
			AssertNotNull("Expected to be read only as IsTariffLineInherited", tariff2Item.ReadOnly);
			AssertEquals("Should be the same UNT on both line", tariff1Line.RateLineItems[0].PK, tariff2Item.PK);

			tariff2Item.ClearReadOnlyFields();

			Assert("Expected no changes even though it's read only because IsTariffLineInherited", !tariff2Item.HasChanges);
		}

		#region Property Setter For RelatedRateLineItem DoesNotThrow

		public void TestTM_AC_SetterForRelatedRateLine_DoesNotThrow()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RelatedRateLines.AddNew();
			var lineItem = line.RateLineItems.AddNew();

			AssertNull("Precondition: lineItem.Parent.Parent should be null.", lineItem.Parent.Parent);
			AssertNoExceptionThrown("Should not throw exception.", () => lineItem.TM_AC = Helper.ChargeCodes["WAR"].PK);
		}

		#endregion
	}

	#region Business Object TestCase

	[TestedType(typeof(RateLineItem))]
	public class RateLineItemBizObjTest : BizObjectRateLineItemTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var ratingHelper = new TestHelper(Factory);
			var client = ratingHelper.NewOrgHeader();
			var clientRate = ratingHelper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LRA, "AU", "");
			var rateLineItem = rateEntry.RateLines[0].RateLineItems.Cast<RateLineItem>().FirstOrDefault();

			return rateLineItem;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rate = factory.New<Quote>();
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR);
			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			var item = line.RateLineItems.AddNew();

			return item;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}

	#endregion
}
