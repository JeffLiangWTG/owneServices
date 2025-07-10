using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class QuotationLineListHelperTest : TestCaseWithFactory
	{
		public void TestGetLines_GivenCTZWithDescendingCartageZone_ThenShouldNotHaveDuplicateHeaders()
		{
			var client = Helper.NewOrgHeader();
			var supplier = Helper.NewOrgHeader();

			Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia, zoneNames: new ZString[] { "AU1" });

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "");
			entry.TI_OH_Supplier = supplier.PK;
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Volume.CubicMetres);
			line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 10, 5m, "");
			line.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 10, 10m, "");

			Factory.Save();

			line.Calculator.CartageZones.Sort("ZoneName", System.ComponentModel.ListSortDirection.Descending);
			AssertContainsExactElementsInExactOrder
			(
				"Precondition: The blank(standard) zone should be at the bottom list",
				new[] { "AU1", "" },
				line.Calculator.CartageZones.Select(cartageZone => cartageZone.ZoneName)
			);

			var set = new PricingPageRateLineList { entry.RateLines[0] };
			var actual = Render(set);
			AssertContainsExactLinesInExactOrder
			(
				"QuotationLineListHelper.GetLines",
				@"
Pick Up Cartage|||
Less than 10 M3|AUD|5.00|per M3 (1 KG = 6000 CC)
10 M3 and above|AUD|10.00|per M3 (1 KG = 6000 CC)
",
				actual
			);
		}

		public void TestEmptySet()
		{
			AssertMultilineASCIIEquals("", "", Render(new PricingPageRateLineList()));
		}

		public void TestNonConsolidated()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.LCL);
			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.M3, Constants.CurrencyCodes.UnitedStates);
			line.TL_RateDesc = "Freight";
			((UnitCalculator)line.Calculator).PerUnit = 500m;

			var set = new PricingPageRateLineList();
			set.Add(line);

			const string expected = @"
Freight|USD|500.00|per M3 / 1000 KG
";

			AssertMultilineASCIIEquals("", expected, Render(set));
		}

		public void TestConsolidatedStandardFormat()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var tariff = Factory.New<CompanyTariff>();

			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP");
			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line1.TL_RateDesc = "Freight";
			((UnitCalculator)line1.Calculator).PerUnit = 300m;

			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20OT");
			var line2 = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line2.TL_RateDesc = "Freight";
			((UnitCalculator)line2.Calculator).PerUnit = 500m;

			var entry3 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20RE");
			var line3 = entry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line3.TL_RateDesc = "Freight";
			((UnitCalculator)line3.Calculator).PerUnit = 500m;

			var set = new PricingPageRateLineList();
			set.Add(line1);
			set.Add(line2);
			set.Add(line3);

			set.AddContainerType(line1, "20GP");
			set.AddContainerType(line2, "20OT");
			set.AddContainerType(line3, "20RE");

			const string expected = @"
Freight|||
20GP|USD|300.00|per Container
20OT, 20RE|USD|500.00|per Container
";

			AssertMultilineASCIIEquals("", expected, Render(set));
		}

		public void TestConsolidatedAlternativeFormat()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var tariff = Factory.New<CompanyTariff>();

			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP");
			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line1.TL_RateDesc = "Freight";
			((UnitCalculator)line1.Calculator).PerUnit = 300m;

			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20OT");
			var line2 = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line2.TL_RateDesc = "Freight";
			((UnitCalculator)line2.Calculator).PerUnit = 500m;

			var entry3 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20RE");
			var line3 = entry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line3.TL_RateDesc = "Freight";
			((UnitCalculator)line3.Calculator).PerUnit = 500m;

			var set = new PricingPageRateLineList();
			set.Add(line1);
			set.Add(line2);
			set.Add(line3);

			set.AddContainerType(line1, "20GP");
			set.AddContainerType(line2, "20OT");
			set.AddContainerType(line3, "20RE");

			const string expected = @"
Freight|USD|300.00|per 20GP Container
Freight|USD|500.00|per 20OT Container
Freight|USD|500.00|per 20RE Container
";

			AssertMultilineASCIIEquals("", expected, Render(set));
		}

		public void TestConsolidatedWithEquipmentType()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP");

			var line1 = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line1.TL_RateDesc = "Freight";
			((UnitCalculator)line1.Calculator).PerUnit = 300m;

			var line2 = entry.AddRateLine("OFORW", CartageCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line2.TL_RateDesc = "Onforwarding Charges";
			line2.Calculator[CartageCalculator.Items.Operator.UNT] = new ZDecimal(90m);
			line2.Calculator.EquipmentType = Constants.FCLEquipmentNeeded.Trailer;

			var set = new PricingPageRateLineList();
			set.Add(line1);
			set.Add(line2);
			set.AddContainerType(line1, "20GP");
			set.AddContainerType(line2, "20GP");

			const string expected = @"
Freight (No Equipment Specified)|USD|300.00|per 20GP Container
Onforwarding Charges 
-  Drop Trailer|USD|90.00|per 20GP Container
";

			AssertMultilineASCIIEquals("", expected, Render(set));
		}

		public void TestDetentionsUseChargeDescription()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var tariff = Factory.New<CompanyTariff>();

			var impEntry = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "SEA", "", "AU", "", "20GP");
			var impLine = impEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			impLine.TL_RateDesc = "{containerno} {type}, {days} day(s) in detention.";
			((UnitCalculator)impLine.Calculator).PerUnit = 300m;

			var expEntry = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "SEA", "AU", "", "", "20GP");
			var expLine = expEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			expLine.TL_RateDesc = "{containerno} {type}, {days} day(s) in detention.";
			((UnitCalculator)expLine.Calculator).PerUnit = 300m;

			var expSet = new PricingPageRateLineList();
			expSet.Add(expLine);
			expSet.AddContainerType(expLine, "20GP");

			var impSet = new PricingPageRateLineList();
			impSet.Add(expLine);
			impSet.AddContainerType(impLine, "20GP");

			const string expected = @"
International Freight|USD|300.00|per 20GP Container
";

			AssertMultilineASCIIEquals("", expected, Render(impSet));
			AssertMultilineASCIIEquals("", expected, Render(expSet));
		}

		public void TestShippingFreightShowsContractNumbers()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AU", "NZ", "", "20GP");
			entry1.TI_ContractNumber = "contract1";
			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line1.TL_RateDesc = "Freight";
			((UnitCalculator)line1.Calculator).PerUnit = 300m;

			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AU", "NZ", "", "20OT");
			entry2.TI_ContractNumber = "contract1";
			var line2 = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line2.TL_RateDesc = "Freight";
			((UnitCalculator)line2.Calculator).PerUnit = 500m;

			var entry3 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AU", "NZ", "", "20RE");
			entry3.TI_ContractNumber = "contract2";
			var line3 = entry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line3.TL_RateDesc = "Freight";
			((UnitCalculator)line3.Calculator).PerUnit = 500m;

			var set = new PricingPageRateLineList();
			set.Add(line1);
			set.Add(line2);
			set.Add(line3);

			set.AddContainerType(line1, "20GP");
			set.AddContainerType(line2, "20OT");
			set.AddContainerType(line3, "20RE");

			const string expected = @"
Freight|USD|300.00|per 20GP Container
Contract Number: contract1|||
Freight|USD|500.00|per 20OT Container
Contract Number: contract1|||
Freight|USD|500.00|per 20RE Container
Contract Number: contract2|||
";

			AssertMultilineASCIIEquals("", expected, Render(set));
		}

		public void TestShippingFreightShowsNoteText()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AU", "NZ", "", "20GP");
			var line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line1.ChargeInformationNoteText = "Blaticus 1";
			line1.TL_RateDesc = "Freight";
			((UnitCalculator)line1.Calculator).PerUnit = 300m;

			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AU", "NZ", "", "20OT");
			var line2 = entry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line2.ChargeInformationNoteText = "Blaticus 1";
			line2.TL_RateDesc = "Freight";
			((UnitCalculator)line2.Calculator).PerUnit = 500m;

			var entry3 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AU", "NZ", "", "20RE");
			var line3 = entry3.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line3.ChargeInformationNoteText = "Blaticus 2";
			line3.TL_RateDesc = "Freight";
			((UnitCalculator)line3.Calculator).PerUnit = 500m;

			var set = new PricingPageRateLineList();
			set.Add(line1);
			set.Add(line2);
			set.Add(line3);

			set.AddContainerType(line1, "20GP");
			set.AddContainerType(line2, "20OT");
			set.AddContainerType(line3, "20RE");

			const string expected = @"
Freight|USD|300.00|per 20GP Container
Note: Blaticus 1|||
Freight|USD|500.00|per 20OT Container
Note: Blaticus 1|||
Freight|USD|500.00|per 20RE Container
Note: Blaticus 2|||
";

			AssertMultilineASCIIEquals("", expected, Render(set));
		}

		public void TestContractNumberLineDependsOnRegistrySettings()
		{
			DocumentsDataRegistry.Instance.ShowContractNumbersOnShippingQuotationDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());

			var entry = quote.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AU", "NZ", "", "20GP");
			entry.TI_ContractNumber = "contract1";
			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line.TL_RateDesc = "Freight";
			((UnitCalculator)line.Calculator).PerUnit = 300m;

			var set = new PricingPageRateLineList();
			set.Add(line);

			const string expected1 = @"
Freight|USD|300.00|per 20GP Container
Contract Number: contract1|||
";

			AssertMultilineASCIIEquals("", expected1, Render(set));

			DocumentsDataRegistry.Instance.ShowContractNumbersOnShippingQuotationDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			entry.RateLines.RemoveAndDeleteAll();
			line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, Constants.CurrencyCodes.UnitedStates);
			line.TL_RateDesc = "Freight";
			((UnitCalculator)line.Calculator).PerUnit = 100m;

			set = new PricingPageRateLineList();
			set.Add(line);

			const string expected2 = @"
Freight|USD|100.00|per 20GP Container
";

			AssertMultilineASCIIEquals("", expected2, Render(set));
		}

		#region Implementation

		string Render(PricingPageRateLineList set)
		{
			var list = ListHelper.GetLines(set, false);
			var builder = new StringBuilder();
			builder.AppendLine();

			foreach (var line in list)
			{
				builder.AppendLine(line.ToString());
			}

			return builder.ToString();
		}

		QuotationLineListHelper ListHelper
		{
			get { return listHelper ?? (listHelper = new QuotationLineListHelper()); }
		}
		QuotationLineListHelper listHelper;

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}

		TestHelper helper;

		#endregion
	}
}
