using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class BulkUpdateActionTest : TestCaseWithFactory
	{
		#region LoadPreviewEntries

		public void TestLoadPreviewEntries()
		{
			var chargeCode = Helper.ChargeCodes["ODOC"];
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1a.AddRateLine(chargeCode, FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry2a.AddRateLine(chargeCode, FlatCalculator.Code);
			var entry2b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "GBLON");
			entry2b.AddRateLine(chargeCode, FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			((RateEntry)TestUpdater.Entries.FindByPK(entry2a.PK)).IncludeInUpdate = false;
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode.PK;
			TestUpdater.UpdateFirstRateEntriesBatch();

			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(3, previewEntries.Count);
			Assert(previewEntries.Contains(entry1a));
			Assert(previewEntries.Contains(entry1b));
			Assert(previewEntries.Contains(entry2b));
			AssertEquals(3, TestUpdater.AllEntriesToUpdateCount);
			AssertEquals(false, TestUpdater.HasAdditionalBatchesToProcess);

			TestUpdater.Action = BulkRateUpdater.Actions.IncreaseDecreaseCharge;
			TestUpdater.UpdateFirstRateEntriesBatch();

			AssertEquals(2, previewEntries.Count);
			Assert(previewEntries.Contains(entry1a));
			Assert(previewEntries.Contains(entry2b));
			AssertEquals(2, TestUpdater.AllEntriesToUpdateCount);
			AssertEquals(false, TestUpdater.HasAdditionalBatchesToProcess);

			TestUpdater.ActionsLine.TL_AC = ZGuid.Empty;
			TestUpdater.UpdateFirstRateEntriesBatch();

			AssertEquals(0, previewEntries.Count);
			AssertEquals(0, TestUpdater.AllEntriesToUpdateCount);
			AssertEquals(false, TestUpdater.HasAdditionalBatchesToProcess);
		}

		public void TestLoadPreviewEntries_FCLEntryWithEmptyContainer_ShouldNotLoadIfCalculatorDoesntSupportEmptyContainer()
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entryWithContainerHasFLTLine = CreateNewEntry("AUSYD", "USLAX", calculatorCode: FlatCalculator.Code, container: "20GP");
			var entryWithContainerHasPERLine = CreateNewEntry("USLAX", "INIXE", calculatorCode: PercentageCalculator.Code, container: "20GP");
			var entryWithContainerHasNoLine = CreateNewEntry("INIXE", "AUSYD", container: "20GP");

			var entryWithoutContainerHasFLTLine = CreateNewEntry("AUSYD", "USLAX", calculatorCode: FlatCalculator.Code);
			var entryWithoutContainerHasPERLine = CreateNewEntry("USLAX", "INIXE", calculatorCode: PercentageCalculator.Code);
			var entryWithoutContainerHasNoLine = CreateNewEntry("INIXE", "AUSYD");

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "FCL";
			TestUpdater.Mode = "SEA";
			TestUpdater.LoadEntries();
			AssertEquals("Pre-condition: all 6 entries should load", 6, TestUpdater.Entries.Count);

			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode.PK;

			CombineAssertions("Preview entries do not match for AddOrReplaceCharge.", () =>
			{
				AssertPreviewEntries(FlatCalculator.Code, new[]
				{
					entryWithContainerHasFLTLine,
					entryWithContainerHasPERLine,
					entryWithContainerHasNoLine,
					entryWithoutContainerHasFLTLine,
					entryWithoutContainerHasPERLine,
					entryWithoutContainerHasNoLine
				});

				AssertPreviewEntries(PercentageCalculator.Code, new[]
				{
					entryWithContainerHasFLTLine,
					entryWithContainerHasPERLine,
					entryWithContainerHasNoLine,
					entryWithoutContainerHasFLTLine,
					entryWithoutContainerHasPERLine,
					entryWithoutContainerHasNoLine
				});

				AssertPreviewEntries(UnitCalculator.Code, new[]
				{
					entryWithContainerHasFLTLine,
					entryWithContainerHasPERLine,
					entryWithContainerHasNoLine
				});
			});

			TestUpdater.Action = BulkRateUpdater.Actions.ReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode.PK;

			CombineAssertions("Preview entries do not match for ReplaceCharge.", () =>
			{
				AssertPreviewEntries(FlatCalculator.Code, new[]
				{
					entryWithContainerHasFLTLine,
					entryWithContainerHasPERLine,
					entryWithoutContainerHasFLTLine,
					entryWithoutContainerHasPERLine
				});

				AssertPreviewEntries(PercentageCalculator.Code, new[]
				{
					entryWithContainerHasFLTLine,
					entryWithContainerHasPERLine,
					entryWithoutContainerHasFLTLine,
					entryWithoutContainerHasPERLine
				});

				AssertPreviewEntries(UnitCalculator.Code, new[]
				{
					entryWithContainerHasFLTLine,
					entryWithContainerHasPERLine
				});
			});

			TestUpdater.Action = BulkRateUpdater.Actions.IncreaseDecreaseCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode.PK;

			AssertPreviewEntriesWhereCalculatorIsNotUpdatable("Preview entries do not match for IncreaseDecreaseCharge.", new[]
			{
				entryWithContainerHasFLTLine,
				entryWithContainerHasPERLine,
				entryWithoutContainerHasFLTLine,
				entryWithoutContainerHasPERLine
			});

			TestUpdater.Action = BulkRateUpdater.Actions.DeleteCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode.PK;

			AssertPreviewEntriesWhereCalculatorIsNotUpdatable("Preview entries do not match for DeleteCharge.", new[]
			{
				entryWithContainerHasFLTLine,
				entryWithContainerHasPERLine,
				entryWithoutContainerHasFLTLine,
				entryWithoutContainerHasPERLine
			});

			void AssertPreviewEntries(string rateCalculator, RateEntry[] expectedRateEntries)
			{
				TestUpdater.ActionsLine.TL_RateCalculator = rateCalculator;
				TestUpdater.UpdateFirstRateEntriesBatch();

				var message = $"PreviewEntries do not match for {rateCalculator} calculator.";
				AssertEquals(message, expectedRateEntries.Length, TestUpdater.PreviewEntries.Count);
				Assert(message, expectedRateEntries.All(x => TestUpdater.PreviewEntries.Contains(x)));
			}

			void AssertPreviewEntriesWhereCalculatorIsNotUpdatable(string message, RateEntry[] expectedRateEntries)
			{
				TestUpdater.UpdateFirstRateEntriesBatch();

				AssertEquals(message, expectedRateEntries.Length, TestUpdater.PreviewEntries.Count);
				Assert(message, expectedRateEntries.All(x => TestUpdater.PreviewEntries.Contains(x)));
			}

			RateEntry CreateNewEntry(string origin, string destination, string calculatorCode = "", string container = "")
			{
				var entry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, origin, destination, container: container, removeLines: true);
				if (!string.IsNullOrEmpty(calculatorCode))
				{
					entry.AddRateLine(chargeCode, calculatorCode);
				}
				return entry;
			}
		}

		#endregion

		#region UpdateFirstRateEntriesBatch

		public void TestUpdateFirstRateEntriesBatch()
		{
			var chargeCode = Helper.ChargeCodes["ODOC"];
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1a.AddRateLine(chargeCode, FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry2a.AddRateLine(chargeCode, FlatCalculator.Code);
			var entry2b = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "GBLON");
			entry2b.AddRateLine(chargeCode, FlatCalculator.Code);
			var entry2c = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "HKHKG");
			entry2c.AddRateLine(chargeCode, FlatCalculator.Code);

			Factory.Save();

			var testUpdater = new BulkRateUpdaterForTest();
			testUpdater.Module = "FWD";
			testUpdater.Type = "ORG";
			testUpdater.Mode = "AIR";
			testUpdater.LoadEntries();
			testUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			testUpdater.ActionsLine.TL_AC = chargeCode.PK;
			testUpdater.UpdateFirstRateEntriesBatch();

			AssertEquals(true, testUpdater.HasAdditionalBatchesToProcess);

			var previewEntries = testUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEquals(5, testUpdater.AllEntriesToUpdateCount);
		}

		#endregion

		#region Action: Delete Charge

		public void TestActionDeleteCharge()
		{
			var chargeCode1 = Helper.ChargeCodes["ODOC"];
			var chargeCode2 = Helper.ChargeCodes["OCART"];
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1a.AddRateLine(chargeCode1, FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			entry1b.AddRateLine(chargeCode1, FlatCalculator.Code);
			entry1b.AddRateLine(chargeCode2, FlatCalculator.Code);

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry2a.AddRateLine(chargeCode2, FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.DeleteCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode2.PK;
			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);

			var previewEntry1b = ((RateEntry)previewEntries.FindByPK(entry1b.PK));
			AssertEquals(1, previewEntry1b.RateLines.Count);
			AssertEquals(chargeCode1.PK, previewEntry1b.RateLines[0].TL_AC);

			var previewEntry2a = ((RateEntry)previewEntries.FindByPK(entry2a.PK));
			AssertEquals(0, previewEntry2a.RateLines.Count);
		}

		public void TestActionDeleteCharge_MultipleRateLineWithSameChargeCode()
		{
			var chargeCode1 = Helper.ChargeCodes["ODOC"];
			var chargeCode2 = Helper.ChargeCodes["OCART"];
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1.AddRateLine(chargeCode1).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;
			var entry2 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			entry2.AddRateLine(chargeCode2, FlatCalculator.Code);
			entry2.AddRateLine(chargeCode1).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)75m;
			entry2.AddRateLine(chargeCode1).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.DeleteCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);

			var previewEntry1 = ((RateEntry)previewEntries.FindByPK(entry1.PK));
			AssertEquals(0, previewEntry1.RateLines.Count);

			var previewEntry2 = ((RateEntry)previewEntries.FindByPK(entry2.PK));
			AssertEquals(1, previewEntry2.RateLines.Count);
			AssertEquals(chargeCode2.PK, previewEntry2.RateLines[0].TL_AC);
		}

		#endregion

		#region Action: Replace Charge

		public void TestActionReplaceCharge()
		{
			var chargeCode1 = Helper.ChargeCodes["ODOC"];
			var chargeCode2 = Helper.ChargeCodes["OCART"];
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1a.AddRateLine(chargeCode1, FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			entry1b.AddRateLine(chargeCode2, FlatCalculator.Code);
			entry1b.AddRateLine(chargeCode1, FlatCalculator.Code);

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry2a.AddRateLine(chargeCode2, FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.ReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode2.PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;
			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);

			var previewEntry1b = ((RateEntry)previewEntries.FindByPK(entry1b.PK));
			AssertEquals(2, previewEntry1b.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry1b.RateLines[0].TL_RateCalculator);
			AssertEquals(50m, previewEntry1b.RateLines[0].Calculator[Calculator.Items.Operator.BAS]);

			var previewEntry2a = ((RateEntry)previewEntries.FindByPK(entry2a.PK));
			AssertEquals(1, previewEntry2a.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry2a.RateLines[0].TL_RateCalculator);
			AssertEquals(50m, previewEntry2a.RateLines[0].Calculator[Calculator.Items.Operator.BAS]);
		}

		public void TestActionReplaceCharge_IntercompanyTariff()
		{
			var chargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("ODOC");
			var chargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("OCART");

			var rate1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1a.AddRateLine(chargeCode1, FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			entry1b.AddRateLine(chargeCode2, FlatCalculator.Code);
			entry1b.AddRateLine(chargeCode1, FlatCalculator.Code);

			var rate2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry2a.AddRateLine(chargeCode2, FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowIntercompanyTariffs = true;
			TestUpdater.LoadEntries();

			TestUpdater.Action = BulkRateUpdater.Actions.ReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode2.PK;
			TestUpdater.ActionsLine.TL_UnitFactor = UnitFactorList.Codes.SAM;
			TestUpdater.UpdateFirstRateEntriesBatch();

			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);

			var previewEntry1b = ((RateEntry)previewEntries.FindByPK(entry1b.PK));
			AssertEquals(2, previewEntry1b.RateLines.Count);
			AssertEquals(UnitFactorList.Codes.SAM, previewEntry1b.RateLines[0].TL_UnitFactor);
			AssertEquals("", previewEntry1b.RateLines[1].TL_UnitFactor);

			var previewEntry2a = ((RateEntry)previewEntries.FindByPK(entry2a.PK));
			AssertEquals(1, previewEntry2a.RateLines.Count);
			AssertEquals(UnitFactorList.Codes.SAM, previewEntry2a.RateLines[0].TL_UnitFactor);
		}

		#endregion

		#region Action: Add or Replace Charge

		public void TestActionAddOrReplaceChargePreserveAllHiddenValues()
		{
			var org = Helper.NewOrgHeader();
			var part = Helper.NewOrgSupplierPart(org);

			var rate = Helper.NewClientRate(org);
			var entry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");

			var line = entry.AddRateLine("OCART", FlatCalculator.Code);
			line.TL_OP_ProductNumber = part.PK;
			line.TL_IsWhsJobLevelCharge = true;
			line.TL_IsOnPallets = true;
			line.TL_FeeChargeLevel = "STD";

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			var resultLine = previewEntries.Cast<RateEntry>().FirstOrDefault().RateLines.Cast<RateLine>().FirstOrDefault();

			AssertEquals(part.PK, resultLine.TL_OP_ProductNumber);
			AssertEquals(true, resultLine.TL_IsWhsJobLevelCharge);
			AssertEquals(true, resultLine.TL_IsOnPallets);
			AssertEquals("STD", resultLine.TL_FeeChargeLevel);
		}

		public void TestActionReplaceCharge_MultipleRateLineWithSameChargeCode()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1.AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			var entry2 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			var line1 = entry2.AddRateLine("OCART", FlatCalculator.Code);

			var line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)75m;

			var line3 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.ReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = UnitCalculator.Code;
			TestUpdater.ActionsLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);

			var previewEntry1 = ((RateEntry)previewEntries.FindByPK(entry1.PK));
			AssertEquals(1, previewEntry1.RateLines.Count);
			AssertEquals(UnitCalculator.Code, previewEntry1.RateLines[0].TL_RateCalculator);

			var previewEntry2 = ((RateEntry)previewEntries.FindByPK(entry2.PK));
			AssertEquals(3, previewEntry2.RateLines.Count);

			AssertEquals(line2.PK, previewEntry2.RateLines[1].PK);
			AssertEquals(FlatCalculator.Code, previewEntry2.RateLines[1].TL_RateCalculator);
			AssertEquals(75m, previewEntry2.RateLines[1].Calculator[Calculator.Items.Operator.BAS]);
			AssertHasRowWarning(previewEntry2.RateLines[1], "Rate lines with the same Charge Code will not be replaced.");

			AssertEquals(line3.PK, previewEntry2.RateLines[2].PK);
			AssertEquals(FlatCalculator.Code, previewEntry2.RateLines[2].TL_RateCalculator);
			AssertEquals(15m, previewEntry2.RateLines[2].Calculator[Calculator.Items.Operator.BAS]);
			AssertHasRowWarning(previewEntry2.RateLines[2], "Rate lines with the same Charge Code will not be replaced.");
		}

		public void TestActionAddOrReplaceCharge()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1a.AddRateLine("ODOC", FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			entry1b.AddRateLine("OCART", FlatCalculator.Code);
			entry1b.AddRateLine("ODOC", FlatCalculator.Code);

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry2a.AddRateLine("OCART", FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;
			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(3, previewEntries.Count);

			var previewEntry1a = (RateEntry)previewEntries.FindByPK(entry1a.PK);
			AssertEquals(2, previewEntry1a.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry1a.RateLines[1].TL_RateCalculator);
			AssertEquals(50m, previewEntry1a.RateLines[1].Calculator[Calculator.Items.Operator.BAS]);

			var previewEntry1b = (RateEntry)previewEntries.FindByPK(entry1b.PK);
			AssertEquals(2, previewEntry1b.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry1b.RateLines[0].TL_RateCalculator);
			AssertEquals(50m, previewEntry1b.RateLines[0].Calculator[Calculator.Items.Operator.BAS]);

			var previewEntry2a = (RateEntry)previewEntries.FindByPK(entry2a.PK);
			AssertEquals(1, previewEntry2a.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry2a.RateLines[0].TL_RateCalculator);
			AssertEquals(50m, previewEntry2a.RateLines[0].Calculator[Calculator.Items.Operator.BAS]);
		}

		public void TestActionAddOrReplaceCharge_IntercompanyTariff()
		{
			var chargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("ODOC");
			var chargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("OCART");

			var rate1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1a.AddRateLine(chargeCode1, FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			entry1b.AddRateLine(chargeCode2, FlatCalculator.Code);
			entry1b.AddRateLine(chargeCode1, FlatCalculator.Code);

			var rate2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry2a.AddRateLine(chargeCode2, FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowIntercompanyTariffs = true;
			TestUpdater.LoadEntries();

			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode2.PK;
			TestUpdater.ActionsLine.TL_UnitFactor = UnitFactorList.Codes.SAM;
			TestUpdater.UpdateFirstRateEntriesBatch();

			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(3, previewEntries.Count);

			var previewEntry1a = (RateEntry)previewEntries.FindByPK(entry1a.PK);
			AssertEquals(2, previewEntry1a.RateLines.Count);
			AssertEquals("", previewEntry1a.RateLines[0].TL_UnitFactor);
			AssertEquals(UnitFactorList.Codes.SAM, previewEntry1a.RateLines[1].TL_UnitFactor);

			var previewEntry1b = (RateEntry)previewEntries.FindByPK(entry1b.PK);
			AssertEquals(2, previewEntry1b.RateLines.Count);
			AssertEquals(UnitFactorList.Codes.SAM, previewEntry1b.RateLines[0].TL_UnitFactor);
			AssertEquals("", previewEntry1b.RateLines[1].TL_UnitFactor);

			var previewEntry2a = (RateEntry)previewEntries.FindByPK(entry2a.PK);
			AssertEquals(1, previewEntry2a.RateLines.Count);
			AssertEquals(UnitFactorList.Codes.SAM, previewEntry2a.RateLines[0].TL_UnitFactor);
		}

		[TestDate(2008, 11, 11)]
		public void TestActionAddOrReplaceCharge_OverridenDescription()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateLine1 = clientRate.AddRateEntry("AIR", "LSE", "AUMEL", "").RateLines[0];
			rateLine1.TL_RateCalculator = UnitCalculator.Code;
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var rateLine2 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "").RateLines[0];
			rateLine2.TL_RateCalculator = UnitCalculator.Code;
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 8m;
			rateLine2.OverrideChargeDescription = true;
			rateLine2.TL_RateDesc = "overriden rateLine2";

			Factory.Save();

			var chargeCode = Helper.ChargeCodes["FRT"];

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode.PK;
			TestUpdater.ActionsLine.TL_RateCalculator = UnitCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<UnitCalculator>().PerUnit = 10m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;

			AssertEquals("Pre-condition: should find both rate entries", 2, previewEntries.Count);

			var previewRateLine1 = ((RateEntry)previewEntries.FindByPK(rateLine1.TL_TI)).RateLines[0];
			AssertEquals(false, previewRateLine1.OverrideChargeDescription);
			AssertEquals(false, previewRateLine1.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals("Should be default charge code description", previewRateLine1.ChargeCode.AC_Desc, previewRateLine1.TL_RateDesc);

			var previewRateLine2 = ((RateEntry)previewEntries.FindByPK(rateLine2.TL_TI)).RateLines[0];
			AssertEquals("Should be overriden by bulk updater", false, previewRateLine2.OverrideChargeDescription);
			AssertEquals(false, previewRateLine2.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals(previewRateLine1.ChargeCode.AC_Desc, previewRateLine2.TL_RateDesc);

			TestUpdater.ActionsLine.OverrideChargeDescription = true;
			TestUpdater.ActionsLine.TL_RateDesc = "bulk rate updater was here";
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;

			previewRateLine1 = ((RateEntry)previewEntries.FindByPK(rateLine1.TL_TI)).RateLines[0];
			AssertEquals(true, previewRateLine1.OverrideChargeDescription);
			AssertEquals(false, previewRateLine1.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals("bulk rate updater was here", previewRateLine1.TL_RateDesc);

			previewRateLine2 = ((RateEntry)previewEntries.FindByPK(rateLine2.TL_TI)).RateLines[0];
			AssertEquals(true, previewRateLine2.OverrideChargeDescription);
			AssertEquals(false, previewRateLine2.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals("bulk rate updater was here", previewRateLine2.TL_RateDesc);

			TestUpdater.ActionsLine.OverrideChargeDescription = false;
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;

			previewRateLine1 = ((RateEntry)previewEntries.FindByPK(rateLine1.TL_TI)).RateLines[0];
			AssertEquals(false, previewRateLine1.OverrideChargeDescription);
			AssertEquals(false, previewRateLine1.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals("Should revert back to default description", previewRateLine1.ChargeCode.AC_Desc, previewRateLine1.TL_RateDesc);

			previewRateLine2 = ((RateEntry)previewEntries.FindByPK(rateLine2.TL_TI)).RateLines[0];
			AssertEquals(false, previewRateLine2.OverrideChargeDescription);
			AssertEquals(false, previewRateLine2.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals(previewRateLine2.ChargeCode.AC_Desc, previewRateLine2.TL_RateDesc);

			Env.Security.ClientRatesChargeDescriptionOverride.IsAllowed = false;
			TestUpdater.ActionsLine.OverrideChargeDescription = true;
			TestUpdater.ActionsLine.TL_RateDesc = "no security - text should be ignored";
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;

			previewRateLine1 = ((RateEntry)previewEntries.FindByPK(rateLine1.TL_TI)).RateLines[0];
			AssertEquals("Should not have changed as security no longer allows it", false, previewRateLine1.OverrideChargeDescription);
			AssertEquals(true, previewRateLine1.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals(chargeCode.AC_Desc, previewRateLine1.TL_RateDesc);

			previewRateLine2 = ((RateEntry)previewEntries.FindByPK(rateLine2.TL_TI)).RateLines[0];
			AssertEquals("OverrideChargeDescription should not be affected by security permission", true, previewRateLine2.OverrideChargeDescription);
			AssertEquals(true, previewRateLine2.OverrideChargeDescriptionInfo.ReadOnly);
			AssertEquals("Should go back to originally saved description", "overriden rateLine2", previewRateLine2.TL_RateDesc);
		}

		public void TestOverrideChargeDescriptionInfo_ReadOnly()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			clientRate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUMEL", "", "FRT", 10m);

			Factory.Save();

			var chargeCode = Helper.ChargeCodes["FRT"];

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCode.PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 20m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;

			AssertEquals("Pre-condition", 1, previewEntries.Count);

			AssertEquals
			(
				"BulkUpdateAction OverrideChargeDescription should not readonly",
				false,
				TestUpdater.ActionsLine.OverrideChargeDescriptionInfo.ReadOnly
			);
		}

		#endregion

		#region Action: Increase/Decrease Charge

		public void TestActionIncreaseDecreaseCharge()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1a.AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;
			var entry1b = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			entry1b.AddRateLine("OCART", FlatCalculator.Code);
			entry1b.AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)75m;

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry2a.AddRateLine("OCART", FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.IncreaseDecreaseCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			TestUpdater.ActionsLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)5m;
			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);

			var previewEntry1a = ((RateEntry)previewEntries.FindByPK(entry1a.PK));
			AssertEquals(1, previewEntry1a.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry1a.RateLines[0].TL_RateCalculator);
			AssertEquals(55m, previewEntry1a.RateLines[0].Calculator[Calculator.Items.Operator.BAS]);

			var previewEntry2b = ((RateEntry)previewEntries.FindByPK(entry1b.PK));
			AssertEquals(2, previewEntry2b.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry2b.RateLines[1].TL_RateCalculator);
			AssertEquals(80m, previewEntry2b.RateLines[1].Calculator[Calculator.Items.Operator.BAS]);
		}

		public void TestActionIncreaseDecreaseCharge_MultipleRateLineWithSameChargeCode()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			entry1.AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;
			var entry2 = rate.AddRateEntry("ORG", "AIR", "AUSYD", "USSFO");
			entry2.AddRateLine("OCART", FlatCalculator.Code);
			entry2.AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)75m;
			entry2.AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.IncreaseDecreaseCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			TestUpdater.ActionsLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)5m;
			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);

			var previewEntry1 = ((RateEntry)previewEntries.FindByPK(entry1.PK));
			AssertEquals(1, previewEntry1.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry1.RateLines[0].TL_RateCalculator);
			AssertEquals(55m, previewEntry1.RateLines[0].Calculator[Calculator.Items.Operator.BAS]);

			var previewEntry2 = ((RateEntry)previewEntries.FindByPK(entry2.PK));
			AssertEquals(3, previewEntry2.RateLines.Count);
			AssertEquals(FlatCalculator.Code, previewEntry2.RateLines[1].TL_RateCalculator);
			AssertEquals(80m, previewEntry2.RateLines[1].Calculator[Calculator.Items.Operator.BAS]);
			AssertEquals(FlatCalculator.Code, previewEntry2.RateLines[2].TL_RateCalculator);
			AssertEquals(20m, previewEntry2.RateLines[2].Calculator[Calculator.Items.Operator.BAS]);
		}

		#endregion

		#region CreateNewEntry

		[TestDate(2010, 10, 10)]
		public void TestActionCreateNewEntry_AddOrReplaceExisting()
		{
			TestActionCreateNewEntry(BulkRateUpdater.Actions.AddOrReplaceCharge);
		}

		[TestDate(2010, 10, 10)]
		public void TestActionCreateNewEntry_ReplaceExisting()
		{
			TestActionCreateNewEntry(BulkRateUpdater.Actions.ReplaceCharge);
		}

		void TestActionCreateNewEntry(BulkRateUpdater.Actions actionToTest)
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntryWithFlatRateLine("ORG", "AIR", "AUSYD", "USLAX", "OCART", 20m);
			entry1a.TI_RateStartDate = ZDate.Today;
			entry1a.TI_RateEndDate = ZDate.Today.AddMonths(1);

			Factory.Save();

			var chargeCodePK = Helper.ChargeCodes["OCART"].PK;

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = actionToTest;
			TestUpdater.ActionsLine.TL_AC = chargeCodePK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			TestUpdater.CreateNewEntry = true;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals("Invalid without dates, should return no results", 0, previewEntries.Count);

			TestUpdater.NewEntryStartDate = ZDate.Today;
			TestUpdater.NewEntryEndDate = ZDate.Today.AddMonths(1);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals("Expected to override the only OCART rateLine on the rate entry", 1, previewEntries.Count);
			AssertEntry(previewEntries, ZDate.Today, ZDate.Today.AddMonths(1), true, chargeCodePK);

			TestUpdater.NewEntryStartDate = ZDate.Today.AddDays(-1);
			TestUpdater.NewEntryEndDate = ZDate.Today.AddMonths(2);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(1, previewEntries.Count);
			AssertEntry(previewEntries, ZDate.Today, ZDate.Today.AddMonths(1), true, chargeCodePK);

			TestUpdater.NewEntryEndDate = ZDate.Empty;
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(1, previewEntries.Count);
			AssertEntry(previewEntries, ZDate.Today, ZDate.Today.AddMonths(1), true, chargeCodePK);

			TestUpdater.NewEntryStartDate = ZDate.Today.AddMonths(2);
			TestUpdater.NewEntryEndDate = ZDate.Today.AddMonths(3);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals("Dates do not overlap so there is nothing to replace", 0, previewEntries.Count);

			TestUpdater.NewEntryEndDate = ZDate.Empty;
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(0, previewEntries.Count);

			TestUpdater.NewEntryStartDate = ZDate.Today.AddMonths(-3);
			TestUpdater.NewEntryEndDate = ZDate.Today.AddMonths(-2);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(0, previewEntries.Count);

			TestUpdater.NewEntryStartDate = ZDate.Today.AddDays(1);
			TestUpdater.NewEntryEndDate = ZDate.Today.AddMonths(1);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, ZDate.Today, ZDate.Today, true, chargeCodePK);
			AssertEntry(previewEntries, ZDate.Today.AddDays(1), ZDate.Today.AddMonths(1), true, chargeCodePK);

			TestUpdater.NewEntryEndDate = ZDate.Today.AddMonths(2);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, ZDate.Today, ZDate.Today, true, chargeCodePK);
			AssertEntry(previewEntries, ZDate.Today.AddDays(1), ZDate.Today.AddMonths(1), true, chargeCodePK);

			TestUpdater.NewEntryEndDate = ZDate.Empty;
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, ZDate.Today, ZDate.Today, true, chargeCodePK);
			AssertEntry(previewEntries, ZDate.Today.AddDays(1), ZDate.Today.AddMonths(1), true, chargeCodePK);

			TestUpdater.NewEntryStartDate = ZDate.Today.AddDays(-1);
			TestUpdater.NewEntryEndDate = ZDate.Today.AddMonths(1).AddDays(-1);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(1), true, chargeCodePK);
			AssertEntry(previewEntries, ZDate.Today, ZDate.Today.AddMonths(1).AddDays(-1), true, chargeCodePK);

			TestUpdater.NewEntryStartDate = ZDate.Today.AddDays(1);
			TestUpdater.NewEntryEndDate = ZDate.Today.AddMonths(1).AddDays(-1);

			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;

			AssertEquals(3, previewEntries.Count);
			AssertEntry(previewEntries, ZDate.Today, ZDate.Today, true, chargeCodePK);
			AssertEntry(previewEntries, ZDate.Today.AddDays(1), ZDate.Today.AddMonths(1).AddDays(-1), true, chargeCodePK);
			AssertEntry(previewEntries, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(1), true, chargeCodePK);
		}

		[TestDate(2017, 08, 01)]
		public void TestActionCreateNewEntry_AddOrReplace_WithNoExistingEndDate()
		{
			var today = ZDate.Today;
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 10m);
			rateEntry.TI_RateStartDate = today;
			rateEntry.TI_RateEndDate = ZDate.Empty;

			Factory.Save();

			var chargeCodePK = Helper.ChargeCodes["OCART"].PK;
			TestUpdater.Type = RatingConstants.RateCategory.ORG;
			TestUpdater.Mode = Core.Constants.RateMode.AIR;
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = chargeCodePK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			TestUpdater.CreateNewEntry = true;

			TestUpdater.NewEntryStartDate = today;
			TestUpdater.NewEntryEndDate = today.AddMonths(1);

			// Old  |============
			// New  |---|
			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, today.AddMonths(1).AddDays(1), ZDate.Empty, false, chargeCodePK);
			AssertEntry(previewEntries, today, today.AddMonths(1), true, chargeCodePK);

			// Old   |============
			// New |------|
			TestUpdater.NewEntryStartDate = today.AddDays(-1);
			TestUpdater.NewEntryEndDate = today.AddMonths(2);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, today.AddMonths(2).AddDays(1), ZDate.Empty, false, chargeCodePK);
			AssertEntry(previewEntries, today, today.AddMonths(2), true, chargeCodePK);

			// Old   |============
			// New |--------------
			TestUpdater.NewEntryEndDate = ZDate.Empty;
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals("Old rate is completely replaced as the new rate encompases the entire date range", 1, previewEntries.Count);
			AssertEntry(previewEntries, today, ZDate.Empty, true, chargeCodePK);

			// Old  |============
			// New  |------------
			TestUpdater.NewEntryStartDate = today;
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals("Old rate is completely replaced as the new rate has the exact same date range", 1, previewEntries.Count);
			AssertEntry(previewEntries, today, ZDate.Empty, true, chargeCodePK);

			// Old  |============
			// New      |---|
			TestUpdater.NewEntryStartDate = today.AddMonths(2);
			TestUpdater.NewEntryEndDate = today.AddMonths(3);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(3, previewEntries.Count);
			AssertEntry(previewEntries, today, today.AddMonths(2).AddDays(-1), false, chargeCodePK);
			AssertEntry(previewEntries, today.AddMonths(2), today.AddMonths(3), true, chargeCodePK);
			AssertEntry(previewEntries, today.AddMonths(3).AddDays(1), ZDate.Empty, false, chargeCodePK);

			// Old   |============
			// New       |--------
			TestUpdater.NewEntryEndDate = ZDate.Empty;
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, today, today.AddMonths(2).AddDays(-1), false, chargeCodePK);
			AssertEntry(previewEntries, today.AddMonths(2), ZDate.Empty, true, chargeCodePK);

			// Old        |============
			// New |---|
			TestUpdater.NewEntryStartDate = today.AddMonths(-3);
			TestUpdater.NewEntryEndDate = today.AddMonths(-2);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals("As dates don't overlap, there is nothing to update", 0, previewEntries.Count);

			// Old  |============
			// New   |---|
			TestUpdater.NewEntryStartDate = today.AddDays(1);
			TestUpdater.NewEntryEndDate = today.AddMonths(1);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(3, previewEntries.Count);
			AssertEntry(previewEntries, today, today, false, chargeCodePK);
			AssertEntry(previewEntries, today.AddDays(1), today.AddMonths(1), true, chargeCodePK);
			AssertEntry(previewEntries, today.AddMonths(1).AddDays(1), ZDate.Empty, false, chargeCodePK);

			// Old  |============
			// New   |-----------
			TestUpdater.NewEntryEndDate = ZDate.Empty;
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, today, today, false, chargeCodePK);
			AssertEntry(previewEntries, today.AddDays(1), ZDate.Empty, true, chargeCodePK);

			// Old   |============
			// New |---|
			TestUpdater.NewEntryStartDate = today.AddDays(-1);
			TestUpdater.NewEntryEndDate = today.AddMonths(1).AddDays(-1);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);
			AssertEntry(previewEntries, today.AddMonths(1), ZDate.Empty, false, chargeCodePK);
			AssertEntry(previewEntries, today, today.AddMonths(1).AddDays(-1), true, chargeCodePK);

			// Old  |============
			// New   |----|
			TestUpdater.NewEntryStartDate = today.AddDays(1);
			TestUpdater.NewEntryEndDate = today.AddMonths(1).AddDays(-1);
			TestUpdater.UpdateFirstRateEntriesBatch();
			previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(3, previewEntries.Count);
			AssertEntry(previewEntries, today, today, false, chargeCodePK);
			AssertEntry(previewEntries, today.AddDays(1), today.AddMonths(1).AddDays(-1), true, chargeCodePK);
			AssertEntry(previewEntries, today.AddMonths(1), ZDate.Empty, false, chargeCodePK);
		}

		[TestDate(2008, 11, 11)]
		public void TestActionCreateNewEntry_AddOrReplaceCharge_WithExistingChargeRemainsSame()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1a.TI_RateStartDate = new ZDate(2008, 4, 1);
			entry1a.TI_RateEndDate = new ZDate(2008, 4, 30);
			entry1a.RateLines.RemoveAndDeleteAll();

			var frtLine = entry1a.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var bafLine = entry1a.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			bafLine.GetCalculator<UnitCalculator>().PerUnit = 80m;

			Factory.Save();

			TestUpdater.CreateNewEntry = true;
			TestUpdater.NewEntryStartDate = new ZDate(2008, 4, 15);
			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["BAF"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = UnitCalculator.Code;
			TestUpdater.ActionsLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)113m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var previewEntries = TestUpdater.PreviewEntries;
			AssertEquals(2, previewEntries.Count);

			var previewEntry1a = ((RateEntry)previewEntries.FindByPK(entry1a.PK));
			AssertEquals(2, previewEntry1a.RateLines.Count);
			AssertEquals(UnitCalculator.Code, previewEntry1a.RateLines[0].TL_RateCalculator);
			AssertEquals(100m, previewEntry1a.RateLines[0].Calculator[Calculator.Items.Operator.UNT]);
			AssertEquals(1, previewEntry1a.RateLines[0].RateLineItems.Count);

			AssertEquals(UnitCalculator.Code, previewEntry1a.RateLines[1].TL_RateCalculator);
			AssertEquals(80m, previewEntry1a.RateLines[1].Calculator[Calculator.Items.Operator.UNT]);
			AssertEquals(1, previewEntry1a.RateLines[1].RateLineItems.Count);

			var previewEntryNew = previewEntries.Cast<RateEntry>().First(e => e.PK != entry1a.PK);
			AssertEquals(2, previewEntryNew.RateLines.Count);
			AssertEquals(UnitCalculator.Code, previewEntryNew.RateLines[0].TL_RateCalculator);
			AssertEquals(100m, previewEntryNew.RateLines[0].Calculator[Calculator.Items.Operator.UNT]);
			AssertEquals(1, previewEntryNew.RateLines[0].RateLineItems.Count);

			AssertEquals(UnitCalculator.Code, previewEntryNew.RateLines[1].TL_RateCalculator);
			AssertEquals(113m, previewEntryNew.RateLines[1].Calculator[Calculator.Items.Operator.UNT]);
			AssertEquals(1, previewEntryNew.RateLines[1].RateLineItems.Count);
		}

		static void AssertEntry(SimpleRateEntryCollection entries, ZDate start, ZDate end, bool containsChargeCode, ZGuid chargeCodePK)
		{
			CombineAssertions(() =>
			{
				var entry = entries.Cast<RateEntry>().Single(e => e.TI_RateStartDate == start && e.TI_RateEndDate == end);
				var message = $"Entry from {start.ToShortDateString()} to {end.ToShortDateString()} does not exist in collection";

				AssertNotNull(message, entry);
				AssertEquals(containsChargeCode, entry.RateLines.Cast<RateLine>().Any(r => r.TL_AC == chargeCodePK));
				Assert(entry.Parent.HasChanges);
			}
			);
		}

		#endregion

		#region Implementation

		BulkRateUpdater TestUpdater
		{
			get { return fTestUpdater ?? (fTestUpdater = new BulkRateUpdater()); }
		}

		BulkRateUpdater fTestUpdater;

		TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		#endregion
	}
}
