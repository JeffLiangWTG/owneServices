using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal class PricingPageRateLineFactoryTest : RatingTestCase
	{
		#region LoadLineSetsLineRemoval

		#region NON Global Override Local LoadLineSetsLineRemoval

		public void TestLineRemoval_ORG_LoadInheritedRates_NoGlobaOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.ORG, isLoadInheritedRates: true, isGlobalOverrideLocal: false, (rateCategory) => GetExpectedLineForNoGlobalSellRatesOverride(rateCategory));
		public void TestLineRemoval_ORG_NoLoadInheritedRates_NoGlobaOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.ORG, isLoadInheritedRates: false, isGlobalOverrideLocal: false, (rateCategory) => GetExpectedLineWithQuoteFCC1(rateCategory));
		public void TestLineRemoval_DST_LoadInheritedRates_NoGlobaOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.DST, isLoadInheritedRates: true, isGlobalOverrideLocal: false, (rateCategory) => GetExpectedLineForNoGlobalSellRatesOverride(rateCategory));
		public void TestLineRemoval_DST_NoLoadInheritedRates_NoGlobaOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.DST, isLoadInheritedRates: false, isGlobalOverrideLocal: false, (rateCategory) => GetExpectedLineWithQuoteFCC1(rateCategory));
		public void TestLineRemoval_AIR_LoadInheritedRates_NoGlobaOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.AIR, isLoadInheritedRates: true, isGlobalOverrideLocal: false, (rateCategory) => GetExpectedLineForNoGlobalSellRatesOverride(rateCategory));
		public void TestLineRemoval_AIR_NoLoadInheritedRates_NoGlobaOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.AIR, isLoadInheritedRates: false, isGlobalOverrideLocal: false, (rateCategory) => GetExpectedLineForNoGlobalSellRatesOverride(rateCategory), "RateCategory.Air doesn't have isLoadInheritedRates flag, hence it includes all RatingHeader");

		#endregion

		#region Global Override Local LoadLineSetsLineRemoval

		public void TestLineRemoval_ORG_LoadInheritedRates_GlobalOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.ORG, isLoadInheritedRates: true, isGlobalOverrideLocal: true, (rateCategory) => GetExpectedLineForGlobalSellRatesOverride(rateCategory));
		public void TestLineRemoval_ORG_NoLoadInheritedRates_GlobalOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.ORG, isLoadInheritedRates: false, isGlobalOverrideLocal: true, (rateCategory) => GetExpectedLineWithQuoteFCC1(rateCategory));
		public void TestLineRemoval_DST_LoadInheritedRates_GlobalOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.DST, isLoadInheritedRates: true, isGlobalOverrideLocal: true, (rateCategory) => GetExpectedLineForGlobalSellRatesOverride(rateCategory));
		public void TestLineRemoval_DST_NoLoadInheritedRates_GlobalOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.DST, isLoadInheritedRates: false, isGlobalOverrideLocal: true, (rateCategory) => GetExpectedLineWithQuoteFCC1(rateCategory));
		public void TestLineRemoval_AIR_LoadInheritedRates_GlobalOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.AIR, isLoadInheritedRates: true, isGlobalOverrideLocal: true, (rateCategory) => GetExpectedLineForGlobalSellRatesOverride(rateCategory));
		public void TestLineRemoval_AIR_NoLoadInheritedRates_GlobalOverrideLocal() => AssertLineRemoval(RatingConstants.RateCategory.AIR, isLoadInheritedRates: false, isGlobalOverrideLocal: true, (rateCategory) => GetExpectedLineForGlobalSellRatesOverride(rateCategory), "RateCategory.Air doesn't have isLoadInheritedRates flag, hence it includes all RatingHeader");

		#endregion

		void AssertLineRemoval(string rateCategory, bool isLoadInheritedRates, bool isGlobalOverrideLocal, Func<string, string[]> getExpectedPricingPageLines, string message = default)
		{
			const string rateMode = Enterprise.Core.Constants.RateMode.AIR;

			using (RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isGlobalOverrideLocal))
			{
				Helper.ChargeCodes.CreateGlobalCharge("FCC1");
				Helper.ChargeCodes.CreateGlobalCharge("FCC2");
				Helper.ChargeCodes.CreateGlobalCharge("FCC3");
				Helper.ChargeCodes.CreateGlobalCharge("FCC4");
				Helper.ChargeCodes.CreateGlobalCharge("FCC5");

				var client = Helper.NewOrgHeader(companyTariffDefault: 1);
				client.OH_IsConsignee = true;
				Factory.Save();

				var origin = "AU";
				var destination = "NZ";

				var quote = Helper.NewQuote(client);
				quote.TH_PrintInheritedOriginCharges = isLoadInheritedRates;
				quote.TH_PrintInheritedDestinationCharges = isLoadInheritedRates;
				var quoteRateEntry = quote.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC1", 10);

				// Client Rate - Local
				Helper.NewClientRate(client).AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC1", 21);
				Helper.NewClientRate(client).AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC2", 20);

				// Tariff - Local
				var localTariff = Factory.New<CompanyTariff>();
				localTariff.TH_GlobalRateLevel = 1;
				localTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC1", 31);
				localTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC2", 32);
				localTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC3", 30);

				// Client Rate - Global
				Helper.NewGlobalClientRate(client).AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC1", 41);
				Helper.NewGlobalClientRate(client).AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC2", 42);
				Helper.NewGlobalClientRate(client).AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC3", 43);
				Helper.NewGlobalClientRate(client).AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC4", 40);

				// Tariff - Global
				var globalTariff = Factory.New<GlobalTariff>();
				globalTariff.TH_GlobalRateLevel = 1;
				globalTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC1", 51);
				globalTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC2", 52);
				globalTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC3", 53);
				globalTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC4", 54);
				globalTariff.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, "FCC5", 50);

				var page = new PricingPage(quoteRateEntry, Factory, PricingPageStyle.Standard);
				var lineSetFactory = new PricingPageRateLineFactory(GetEntryType(rateCategory));
				var actualPricingPageLines = RenderAsStringCollection(lineSetFactory.LoadLineSets(page, page.RateEntries));
				AssertContainsExactElementsInAnyOrder(message, getExpectedPricingPageLines(rateCategory), actualPricingPageLines);
			}
		}

		string[] GetExpectedLineWithQuoteFCC1(string rateCategory) => new[] { $"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC1|FCC1 Global Charge|FLT,10,,,|" }; // quote

		string[] GetExpectedLineForNoGlobalSellRatesOverride(string rateCategory) => new string[]
		{
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC1|FCC1 Global Charge|FLT,10,,,|", // quote
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC2|FCC2 Global Charge|FLT,20,,,|", // client-rate local
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC3|FCC3 Global Charge|FLT,43,,,|", // client-rate global
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC4|FCC4 Global Charge|FLT,40,,,|", // client-rate global
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC5|FCC5 Global Charge|FLT,50,,,|"  // tariff global
		};

		string[] GetExpectedLineForGlobalSellRatesOverride(string rateCategory) => new string[]
		{
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC1|FCC1 Global Charge|FLT,10,,,|", // quote
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC2|FCC2 Global Charge|FLT,42,,,|", // client-rate global - override FCC2 20 (client-rate local)
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC3|FCC3 Global Charge|FLT,43,,,|", // client-rate global
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC4|FCC4 Global Charge|FLT,40,,,|", // client-rate global - override FCC4 40 (tariff local)
			$"\r\n[ShowEquipmentType = False]\r\n{rateCategory}|AU->NZ|AIR||FCC5|FCC5 Global Charge|FLT,50,,,|"  // tariff-global
		};

		EntryTypes GetEntryType(string rateCategory)
		{
			switch (rateCategory)
			{
				case RatingConstants.RateCategory.ORG:
					return EntryTypes.Origin;
				case RatingConstants.RateCategory.DST:
					return EntryTypes.Destination;
				case RatingConstants.RateCategory.AIR:
					return EntryTypes.Freight;
				default:
					throw new NotImplementedException($"{rateCategory} is not implemented in this test yet");
			}
		}

		#endregion

		#region TestModeFilterFromFreight_LCL

		public void TestModeFilterFromFreight_LCL()
		{
			var page = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, null)[RatingConstants.RateCategory.LCL];

			TestModeFilterFromFreight_LCLForDestination(page);
			TestModeFilterFromFreight_LCLForOrigin(page);
		}

		void TestModeFilterFromFreight_LCLForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NL|ALL||DDOC|Destination Documentation Fee|FLT,101,,,|
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestModeFilterFromFreight_LCLForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->|ALL||ODOC|Origin Documentation Fee|FLT,101,,,|
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestModeFilterFromFreight_FCL

		public void TestModeFilterFromFreight_FCL()
		{
			var page = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, null)[RatingConstants.RateCategory.FCL];

			TestModeFilterFromFreight_FCLForDestination(page);
			TestModeFilterFromFreight_FCLForOrigin(page);
		}

		void TestModeFilterFromFreight_FCLForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NL|ALL||DDOC|Destination Documentation Fee|FLT,101,,,|20GP

[ShowEquipmentType = False]
DST|->NL|FCL||DPCH|Destination Port Charges|FLT,103,,,|20GP

[ShowEquipmentType = False]
DST|->NL|FCL||DLAB|Destination Labour Charges|FLT,104,,,|20GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestModeFilterFromFreight_FCLForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->|ALL||ODOC|Origin Documentation Fee|FLT,101,,,|20GP

[ShowEquipmentType = False]
ORG|AU->|FCL||OPCH|Origin Port Charges|FLT,103,,,|20GP

[ShowEquipmentType = False]
ORG|AU->|FCL||OLAB|Origin Labour Charges|FLT,104,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestModeFilterFromSupplementary_All

		public void TestModeFilterFromSupplementary_All()
		{
			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, null);

			TestModeFilterFromSupplementary_AllForDestination(pages[RatingConstants.RateCategory.DST]);
			TestModeFilterFromSupplementary_AllForOrigin(pages[RatingConstants.RateCategory.ORG]);
		}

		void TestModeFilterFromSupplementary_AllForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NL|ALL||DDOC|Destination Documentation Fee|FLT,101,,,|
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestModeFilterFromSupplementary_AllForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->|ALL||ODOC|Origin Documentation Fee|FLT,101,,,|
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestModeFilterFromSupplementary_LCL

		public void TestModeFilterFromSupplementary_LCL()
		{
			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, false);

			TestModeFilterFromSupplementary_LCLForDestination(pages[RatingConstants.RateCategory.DST]);
			TestModeFilterFromSupplementary_LCLForOrigin(pages[RatingConstants.RateCategory.ORG]);
		}

		void TestModeFilterFromSupplementary_LCLForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NL|ALL||DDOC|Destination Documentation Fee|FLT,101,,,|

[ShowEquipmentType = False]
DST|->NL|LCL||DPCH|Destination Port Charges|FLT,102,,,|
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestModeFilterFromSupplementary_LCLForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->|ALL||ODOC|Origin Documentation Fee|FLT,101,,,|

[ShowEquipmentType = False]
ORG|AU->|LCL||OPCH|Origin Port Charges|FLT,102,,,|
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestModeFilterFromSupplementary_FCL

		public void TestModeFilterFromSupplementary_FCL()
		{
			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryModeFiltering(Factory, true);

			TestModeFilterFromSupplementary_FCLForDestination(pages[RatingConstants.RateCategory.DST]);
			TestModeFilterFromSupplementary_FCLForOrigin(pages[RatingConstants.RateCategory.ORG]);
		}

		void TestModeFilterFromSupplementary_FCLForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NL|ALL||DDOC|Destination Documentation Fee|FLT,101,,,|

[ShowEquipmentType = False]
DST|->NL|FCL||DPCH|Destination Port Charges|FLT,103,,,|
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestModeFilterFromSupplementary_FCLForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->|ALL||ODOC|Origin Documentation Fee|FLT,101,,,|

[ShowEquipmentType = False]
ORG|AU->|FCL||OPCH|Origin Port Charges|FLT,103,,,|
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestPortFilterFromFreight

		public void TestPortFilterFromFreight()
		{
			var page = PricingPageTestHelper.SetupSampleRatesForSupplementaryPortFiltering(Factory, true)[RatingConstants.RateCategory.FCL];

			TestPortFilterFromFreightForDestination(page);
			TestPortFilterFromFreightForOrigin(page);
		}

		void TestPortFilterFromFreightForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NL|FCL||DDOC|Destination Documentation Fee|FLT,100,,,|20GP

[ShowEquipmentType = False]
DST|->NLAMS|FCL||DPCH|Destination Port Charges|FLT,100,,,|20GP

[ShowEquipmentType = False]
DST|AU->NL|FCL||DPCH|Destination Port Charges|FLT,100,,,|20GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestPortFilterFromFreightForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->|FCL||ODOC|Origin Documentation Fee|FLT,100,,,|20GP

[ShowEquipmentType = False]
ORG|AUSYD->|FCL||OPCH|Origin Port Charges|FLT,100,,,|20GP

[ShowEquipmentType = False]
ORG|AU->NL|FCL||OPCH|Origin Port Charges|FLT,100,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestPortFilterFromSupplementary

		public void TestPortFilterFromSupplementary()
		{
			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryPortFiltering(Factory, false);

			TestPortFilterFromSupplementaryForDestination(pages[RatingConstants.RateCategory.DST]);
			TestPortFilterFromSupplementaryForOrigin(pages[RatingConstants.RateCategory.ORG]);
		}

		void TestPortFilterFromSupplementaryForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NL|FCL||DDOC|Destination Documentation Fee|FLT,100,,,|20GP
";

			var destinationPricingPageLineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(destinationPricingPageLineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestPortFilterFromSupplementaryForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->|FCL||ODOC|Origin Documentation Fee|FLT,100,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestPortFilterFromSupplementaryWithBothPorts

		public void TestPortFilterFromSupplementaryWithBothPorts()
		{
			var pages = PricingPageTestHelper.SetupSampleRatesForSupplementaryPortFiltering(Factory, true);

			TestPortFilterFromSupplementaryWithBothPortsForDestination(pages[RatingConstants.RateCategory.DST]);
			TestPortFilterFromSupplementaryWithBothPortsForOrigin(pages[RatingConstants.RateCategory.ORG]);
		}

		void TestPortFilterFromSupplementaryWithBothPortsForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NL|FCL||DDOC|Destination Documentation Fee|FLT,100,,,|20GP

[ShowEquipmentType = False]
DST|AU->NL|FCL||DPCH|Destination Port Charges|FLT,100,,,|20GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestPortFilterFromSupplementaryWithBothPortsForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->|FCL||ODOC|Origin Documentation Fee|FLT,100,,,|20GP

[ShowEquipmentType = False]
ORG|AU->NL|FCL||OPCH|Origin Port Charges|FLT,100,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestVisibility

		public void TestVisibilityForFreight()
		{
			const string expected = @"
[ShowEquipmentType = False]
FCL|AU->NL|SEA||CC1|Suppress Charge|UNT,,,50,|20GP

[ShowEquipmentType = False]
FCL|AU->NL|SEA||CC2|Show Charge|UNT,,,50,|20GP

[ShowEquipmentType = False]
FCL|AU->NL|SEA||CC2|Show Charge (Z)|UNT,,,0,|20GP
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityChecking(Factory, RatingConstants.RateCategory.FCL);
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestVisibilityForDestination()
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|AU->NL|FCL||CC1|Suppress Charge|UNT,,,50,|20GP

[ShowEquipmentType = False]
DST|AU->NL|FCL||CC2|Show Charge|UNT,,,50,|20GP

[ShowEquipmentType = False]
DST|AU->NL|FCL||CC2|Show Charge (Z)|UNT,,,0,|20GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityChecking(Factory, RatingConstants.RateCategory.DST);
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestVisibilityForOrigin()
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->NL|FCL||CC1|Suppress Charge|UNT,,,50,|20GP

[ShowEquipmentType = False]
ORG|AU->NL|FCL||CC2|Show Charge|UNT,,,50,|20GP

[ShowEquipmentType = False]
ORG|AU->NL|FCL||CC2|Show Charge (Z)|UNT,,,0,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();

			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityChecking(Factory, RatingConstants.RateCategory.ORG);
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestCombining

		public void TestCombining()
		{
			var page = PricingPageTestHelper.SetupSampleRatesForCombining(Factory);

			TestCombiningForDestination(page);
			TestCombiningForOrigin(page);
		}

		void TestCombiningForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,300,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,301,|
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestCombiningForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,200,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,201,|
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestMatchingFromSupplementary

		public void TestMatchingFromSupplementaryForFreight_NoSimilarRateEntries()
		{
			const string expectedFromEmpty = @"
";
			var emptyPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.DST, true);

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("Empty", expectedFromEmpty, Render(lineSetFactory.LoadLineSets(emptyPage, emptyPage.RateEntries)));
		}

		public void TestMatchingFromSupplementaryForFreight_ManySimilarRateEntries()
		{
			const string expectedFromFull = @"
";

			var fullPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.DST, false);
			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("Full", expectedFromFull, Render(lineSetFactory.LoadLineSets(fullPage, fullPage.RateEntries)));
		}

		public void TestMatchingFromSupplementaryForDestination_NoSimilarRateEntries()
		{
			const string expectedFromEmpty = @"
[ShowEquipmentType = False]
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,100,|
";
			var emptyPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.DST, true);
			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("Empty", expectedFromEmpty, Render(lineSetFactory.LoadLineSets(emptyPage, emptyPage.RateEntries)));
		}

		public void TestMatchingFromSupplementaryForDestination_ManySimilarRateEntries()
		{
			const string expectedFromFull = @"
[ShowEquipmentType = False]
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,200,|
";

			var fullPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.DST, false);
			var lineSetFactory = GetDestinationPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("Full", expectedFromFull, Render(lineSetFactory.LoadLineSets(fullPage, fullPage.RateEntries)));
		}

		public void TestMatchingFromSupplementaryForOrigin_NoSimilarRateEntries()
		{
			const string expectedFromEmpty = @"
[ShowEquipmentType = False]
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,100,|
";

			var emptyPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.ORG, true);

			var lineSetFactory = GetOriginPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("Empty", expectedFromEmpty, Render(lineSetFactory.LoadLineSets(emptyPage, emptyPage.RateEntries)));
		}

		public void TestMatchingFromSupplementaryForOrigin_ManySimilarRateEntries()
		{
			const string expectedFromFull = @"
[ShowEquipmentType = False]
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,200,|
";
			var fullPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.ORG, false);

			var lineSetFactory = GetOriginPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("Full", expectedFromFull, Render(lineSetFactory.LoadLineSets(fullPage, fullPage.RateEntries)));
		}

		#endregion

		#region TestMatchingFromSupplementary_DifferentServiceProviders

		public void TestMatchingFromSupplementary_DifferentServiceProviders()
		{
			TestMatchingFromSupplementary_DifferentServiceProvidersForDestination();
			TestMatchingFromSupplementary_DifferentServiceProvidersForOrigin();
		}

		void TestMatchingFromSupplementary_DifferentServiceProvidersForDestination()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("DCCT", "Destination Port Charge", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			secondHelper.ChargeCodes.New("DDDD", "Destination Document Processing Charge", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			var serviceProvider = createFactory.NewWithValidTestData<OrgHeader>();
			serviceProvider.OH_Code = "DSTSP1";

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "DSTCL";

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_OH_Supplier = ZGuid.Empty;
			var destinationRateLine1a = destinationEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine1a.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(110);
			var destinationRateLine1b = destinationEntry1.AddRateLine("DDDD", FlatCalculator.Code, "", "NZD");
			destinationRateLine1b.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry2.TI_OH_Supplier = serviceProvider.PK;
			var destinationRateLine2a = destinationEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine2a.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(160);

			createFactory.Save();

			var page = new PricingPage(destinationEntry2, Factory, PricingPageStyle.Standard);

			const string expected = @"
[ShowEquipmentType = False]
DST|->NZAKL|FCL||DCCT|Destination Port Charge|FLT,160,,,|20GP

[ShowEquipmentType = False]
DST|->NZAKL|FCL||DDDD|Destination Document Processing Charge|FLT,20,,,|20GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromSupplementary_DifferentServiceProvidersForOrigin()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("OCCT", "Origin Port Charge", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);
			secondHelper.ChargeCodes.New("ODDD", "Origin Document Processing Charge", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var serviceProvider = createFactory.NewWithValidTestData<OrgHeader>();
			serviceProvider.OH_Code = "ORGSP1";

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "ORGCL";

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry1.TI_OH_Supplier = ZGuid.Empty;
			var originRateLine1a = originEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", "NZD");
			originRateLine1a.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(150);
			var originRateLine1b = originEntry1.AddRateLine("ODDD", FlatCalculator.Code, "", "NZD");
			originRateLine1b.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(30);

			var originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry2.TI_OH_Supplier = serviceProvider.PK;
			var originRateLine2a = originEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", "NZD");
			originRateLine2a.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(180);

			createFactory.Save();

			var page = new PricingPage(originEntry2, Factory, PricingPageStyle.Standard);

			const string expected = @"
[ShowEquipmentType = False]
ORG|AUSYD->|FCL||OCCT|Origin Port Charge|FLT,180,,,|20GP

[ShowEquipmentType = False]
ORG|AUSYD->|FCL||ODDD|Origin Document Processing Charge|FLT,30,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestMatchingFromSupplementary_NoDuplicateRates

		public void TestMatchingFromSupplementary_NoDuplicateRates()
		{
			TestMatchingFromSupplementary_NoDuplicateRatesForDestination();
			TestMatchingFromSupplementary_NoDuplicateRatesForOrigin();
		}

		void TestMatchingFromSupplementary_NoDuplicateRatesForDestination()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("DCCT", "Destination Port Charges", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "DSTCL";

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var destinationRateLine1 = destinationEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var destinationRateLine2 = destinationEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var destinationEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "40GP");
			destinationEntry3.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var destinationRateLine3 = destinationEntry3.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine3.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(40);

			createFactory.Save();

			var page = new PricingPage(destinationEntry1, Factory, PricingPageStyle.Standard);
			page.AddRateEntry(destinationEntry2);
			page.AddRateEntry(destinationEntry3);

			const string expected = @"
[ShowEquipmentType = False]
DST|->NZAKL|FCL||DCCT|Destination Port Charges|FLT,20,,,|20GP
DST|->NZAKL|FCL||DCCT|Destination Port Charges|FLT,40,,,|40GP

[ShowEquipmentType = False]
DST|->NZAKL|FCL|STD|DCCT|Destination Port Charges|FLT,25,,,|20GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromSupplementary_NoDuplicateRatesForOrigin()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("OCCT", "Origin Port Charges", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "ORGCL";

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originEntry1.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var originRateLine1 = originEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", "HKD");
			originRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var originRateLine2 = originEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", "HKD");
			originRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var originEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "40GP");
			originEntry3.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var destinationRateLine3 = originEntry3.AddRateLine("OCCT", FlatCalculator.Code, "", "HKD");
			destinationRateLine3.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(40);

			createFactory.Save();

			var page = new PricingPage(originEntry1, Factory, PricingPageStyle.Standard);
			page.AddRateEntry(originEntry2);
			page.AddRateEntry(originEntry3);

			const string expected = @"
[ShowEquipmentType = False]
ORG|HKHKG->|FCL||OCCT|Origin Port Charges|FLT,20,,,|20GP
ORG|HKHKG->|FCL||OCCT|Origin Port Charges|FLT,40,,,|40GP

[ShowEquipmentType = False]
ORG|HKHKG->|FCL|STD|OCCT|Origin Port Charges|FLT,25,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestMatchingFromSupplementary_NoDuplicateRatesFromCompanyTariff

		public void TestMatchingFromSupplementary_NoDuplicateRatesFromCompanyTariff()
		{
			TestMatchingFromSupplementary_NoDuplicateRatesFromCompanyTariffForDestination();
			TestMatchingFromSupplementary_NoDuplicateRatesFromCompanyTariffForOrigin();
		}

		void TestMatchingFromSupplementary_NoDuplicateRatesFromCompanyTariffForDestination()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "DSTCL";

			secondHelper.ChargeCodes.New("DCCT", "Destination Port Charge", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var tariff = createFactory.New<CompanyTariff>();
			client.CompanyData.RateTariffLevels.SetLevel("DEF", tariff.TH_GlobalRateLevel);

			var destinationTariffEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "40GP");
			destinationTariffEntry1.TI_RS_NKServiceLevel_NI = "STD";
			var destinationTariffRateLine1 = destinationTariffEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationTariffRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(45.10);

			var destinationTariffEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationTariffEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var destinationTariffRateLine2 = destinationTariffEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationTariffRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25.10);

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var destinationRateLine1 = destinationEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var destinationRateLine2 = destinationEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var destinationEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "40GP");
			destinationEntry3.TI_RS_NKServiceLevel_NI = "STD";
			destinationEntry3.TI_CartageDeliveryAddressPostCode = "2000";

			createFactory.Save();

			var page = new PricingPage(destinationEntry1, Factory, PricingPageStyle.Standard);
			page.AddRateEntry(destinationEntry2);
			page.AddRateEntry(destinationEntry3);

			const string expected = @"
[ShowEquipmentType = False]
DST|->NZAKL|FCL||DCCT|Destination Port Charge|FLT,20,,,|20GP

[ShowEquipmentType = False]
DST|->NZAKL|FCL|STD|DCCT|Destination Port Charge|FLT,25,,,|20GP
DST|->NZAKL|FCL|STD|DCCT|Destination Port Charge|FLT,45.1,,,|40GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromSupplementary_NoDuplicateRatesFromCompanyTariffForOrigin()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "ORGCL";

			secondHelper.ChargeCodes.New("OCCT", "Origin Port Charge", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var tariff = createFactory.New<CompanyTariff>();
			client.CompanyData.RateTariffLevels.SetLevel("DEF", tariff.TH_GlobalRateLevel);

			var originTariffEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "40GP");
			originTariffEntry1.TI_RS_NKServiceLevel_NI = "STD";
			var originTariffRateLine1 = originTariffEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", "HKD");
			originTariffRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(45.10);

			var originTariffEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originTariffEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var originTariffRateLine2 = originTariffEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", "HKD");
			originTariffRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25.10);

			var originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originEntry1.TI_RS_NKServiceLevel_NI = ZString.Empty;
			var originRateLine1 = originEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", "HKD");
			originRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "20GP");
			originEntry2.TI_RS_NKServiceLevel_NI = "STD";
			var originRateLine2 = originEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", "HKD");
			originRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var originEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "HKHKG", "", "", "40GP");
			originEntry3.TI_RS_NKServiceLevel_NI = "STD";
			originEntry3.TI_CartagePickupAddressPostCode = "2000";

			createFactory.Save();

			var page = new PricingPage(originEntry1, Factory, PricingPageStyle.Standard);
			page.AddRateEntry(originEntry2);
			page.AddRateEntry(originEntry3);

			const string expected = @"
[ShowEquipmentType = False]
ORG|HKHKG->|FCL||OCCT|Origin Port Charge|FLT,20,,,|20GP

[ShowEquipmentType = False]
ORG|HKHKG->|FCL|STD|OCCT|Origin Port Charge|FLT,25,,,|20GP
ORG|HKHKG->|FCL|STD|OCCT|Origin Port Charge|FLT,45.1,,,|40GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestMatchingFromFreight

		public void TestMatchingFromFreight()
		{
			var emptyPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.LCL, true);
			var fullPage = PricingPageTestHelper.SetupSampleRatesForEmptyMatching(Factory, RatingConstants.RateCategory.LCL, false);

			TestMatchingFromFreightForFreight(emptyPage, fullPage);
			TestMatchingFromFreightForDestination(emptyPage, fullPage);
			TestMatchingFromFreightForOrigin(emptyPage, fullPage);
		}

		void TestMatchingFromFreightForFreight(PricingPage emptyPage, PricingPage fullPage)
		{
			const string expectedFromEmpty = @"
[ShowEquipmentType = False]
LCL|AU->NL|LCL||FRT|FRT Charge|UNT,,,100,|
";

			const string expectedFromFull = @"
[ShowEquipmentType = False]
LCL|AU->NL|LCL||FRT|FRT Charge|UNT,,,200,|
";

			CombineAssertions(delegate
			{
				var lineSetFactory = GetFreightPricingPageLineSetFactory();
				AssertMultilineASCIIEquals("Empty", expectedFromEmpty, Render(lineSetFactory.LoadLineSets(emptyPage, emptyPage.RateEntries)));
				AssertMultilineASCIIEquals("Full", expectedFromFull, Render(lineSetFactory.LoadLineSets(fullPage, fullPage.RateEntries)));
			});
		}

		void TestMatchingFromFreightForDestination(PricingPage emptyPage, PricingPage fullPage)
		{
			const string expectedFromEmpty = @"
[ShowEquipmentType = False]
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,100,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,200,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,201,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,202,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,203,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,204,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,205,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,206,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,207,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,208,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,209,|
";

			const string expectedFromFull = @"
[ShowEquipmentType = False]
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,200,|
DST|AU->NL|ALL||DPCH|DPCH Charge|UNT,,,201,|
";  //DST-TI_ContractNumber(201) represents the "best matched" rate line with EMPTY service provider

			CombineAssertions(delegate
			{
				var lineSetFactory = GetDestinationPricingPageLineSetFactory();
				AssertMultilineASCIIEquals("Empty", expectedFromEmpty, Render(lineSetFactory.LoadLineSets(emptyPage, emptyPage.RateEntries)));
				AssertMultilineASCIIEquals("Full", expectedFromFull, Render(lineSetFactory.LoadLineSets(fullPage, fullPage.RateEntries)));
			});
		}

		void TestMatchingFromFreightForOrigin(PricingPage emptyPage, PricingPage fullPage)
		{
			const string expectedFromEmpty = @"
[ShowEquipmentType = False]
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,100,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,200,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,201,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,202,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,203,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,204,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,205,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,206,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,207,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,208,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,209,|
";

			const string expectedFromFull = @"
[ShowEquipmentType = False]
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,200,|
ORG|AU->NL|ALL||OPCH|OPCH Charge|UNT,,,201,|
";  //ORG-TI_OH_Consignor(201) represents the "best matched" rate line with EMPTY service provider

			CombineAssertions(delegate
			{
				var lineSetFactory = GetOriginPricingPageLineSetFactory();
				AssertMultilineASCIIEquals("Empty", expectedFromEmpty, Render(lineSetFactory.LoadLineSets(emptyPage, emptyPage.RateEntries)));
				AssertMultilineASCIIEquals("Full", expectedFromFull, Render(lineSetFactory.LoadLineSets(fullPage, fullPage.RateEntries)));
			});
		}

		#endregion

		#region TestMatchingFromFreight_DifferentServiceProviders

		public void TestMatchingFromFreight_DifferentServiceProviders()
		{
			TestMatchingFromFreight_DifferentServiceProvidersForFreight();
			TestMatchingFromFreight_DifferentServiceProvidersForDestination();
			TestMatchingFromFreight_DifferentServiceProvidersForOrigin();
		}

		void TestMatchingFromFreight_DifferentServiceProvidersForFreight()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("FCC1", "Freight Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			secondHelper.ChargeCodes.New("FCC2", "Freight Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			secondHelper.ChargeCodes.New("FCC3", "Freight Charge Code 3", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);

			var serviceProvider1 = createFactory.NewWithValidTestData<OrgHeader>();
			serviceProvider1.OH_Code = "FRTSP1";

			var serviceProvider2 = createFactory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "FRTSP2";

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "FRTCL";

			var rate = createFactory.New<ClientRate>();
			rate.TH_OH = client.PK;

			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "DEHAM", "AUSYD", "STD", "20GP");
			rateEntry1.TI_OH_Supplier = serviceProvider1.PK;
			var rateLine1 = rateEntry1.AddRateLine("FCC1", FlatCalculator.Code);
			rateLine1.TL_RateDesc = "Client Rate - Service Provider 1 - Charge 1";
			rateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(1200);

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "DE", "AU", "STD", "20GP");
			var rateLine21 = rateEntry2.AddRateLine("FCC2", FlatCalculator.Code);
			rateLine21.TL_RateDesc = "Client Rate - No Service Provider - Charge 2";
			rateLine21.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(50);
			var rateLine22 = rateEntry2.AddRateLine("FCC1", FlatCalculator.Code);
			rateLine22.TL_RateDesc = "Client Rate - No Service Provider - Charge 1";
			rateLine22.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(1500);

			var rateEntry3 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "DE", "AU", "STD", "20GP");
			rateEntry3.TI_OH_Supplier = serviceProvider2.PK;
			var rateLine3 = rateEntry3.AddRateLine("FCC3", FlatCalculator.Code);
			rateLine3.TL_RateDesc = "Client Rate - Service Provider 2 - Charge 3";
			rateLine3.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(60);

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var quoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "DEHAM", "AUSYD", "STD", "20GP");
			quoteEntry.TI_OH_Supplier = serviceProvider1.PK;
			var quoteLine = quoteEntry.AddRateLine("FCC1", FlatCalculator.Code);
			quoteLine.TL_RateDesc = "Quote - Service Provider 1 - Charge 1";
			quoteLine.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(1000);

			createFactory.Save();

			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);

			const string expected = @"
[ShowEquipmentType = False]
FCL|DEHAM->AUSYD|SEA|STD|FCC1|Quote - Service Provider 1 - Charge 1|FLT,1000,,,|20GP

[ShowEquipmentType = False]
FCL|DE->AU|SEA|STD|FCC2|Client Rate - No Service Provider - Charge 2|FLT,50,,,|20GP
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromFreight_DifferentServiceProvidersForDestination()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("FCCD", "Freight Charge", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			secondHelper.ChargeCodes.New("DCCT", "Destination Port Charge", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			var serviceProvider1 = createFactory.NewWithValidTestData<OrgHeader>();
			serviceProvider1.OH_Code = "DSTSP1";

			var serviceProvider2 = createFactory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "DSTSP2";

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "DSTCL";

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var freightEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "NZAKL", "STD", "20GP");
			freightEntry.TI_OH_Supplier = serviceProvider1.PK;
			var freightRateLine = freightEntry.AddRateLine("FCCD", FlatCalculator.Code);
			freightRateLine.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(1200);

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry1.TI_OH_Supplier = serviceProvider1.PK;
			var destinationRateLine1 = destinationEntry1.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(250);

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry2.TI_OH_Supplier = serviceProvider2.PK;
			var destinationRateLine2 = destinationEntry2.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(350);

			var destinationEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.DST, "FCL", "", "NZAKL", "", "20GP");
			destinationEntry3.TI_OH_Supplier = ZGuid.Empty;
			var destinationRateLine3 = destinationEntry3.AddRateLine("DCCT", FlatCalculator.Code, "", "NZD");
			destinationRateLine3.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(200);

			createFactory.Save();

			var page = new PricingPage(freightEntry, Factory, PricingPageStyle.Standard);

			const string expected = @"
[ShowEquipmentType = False]
DST|->NZAKL|FCL||DCCT|Destination Port Charge|FLT,200,,,|20GP
DST|->NZAKL|FCL||DCCT|Destination Port Charge|FLT,250,,,|20GP
DST|->NZAKL|FCL||DCCT|Destination Port Charge|FLT,350,,,|20GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromFreight_DifferentServiceProvidersForOrigin()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			secondHelper.ChargeCodes.New("FCCO", "Freight Charge Code", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			secondHelper.ChargeCodes.New("OCCT", "Origin Port Charge", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var serviceProvider1 = createFactory.NewWithValidTestData<OrgHeader>();
			serviceProvider1.OH_Code = "ORGSP1";

			var serviceProvider2 = createFactory.NewWithValidTestData<OrgHeader>();
			serviceProvider2.OH_Code = "ORGSP2";

			var client = createFactory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "ORGCL";

			var quote = createFactory.New<Quote>();
			quote.TH_OH = client.PK;

			var freightEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "NZAKL", "STD", "20GP");
			freightEntry.TI_OH_Supplier = serviceProvider1.PK;
			var freightRateLine = freightEntry.AddRateLine("FCCO", FlatCalculator.Code);
			freightRateLine.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(1000);

			var originEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry1.TI_OH_Supplier = serviceProvider1.PK;
			var originRateLine1 = originEntry1.AddRateLine("OCCT", FlatCalculator.Code, "", "NZD");
			originRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(150);

			var originEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry2.TI_OH_Supplier = serviceProvider2.PK;
			var originRateLine2 = originEntry2.AddRateLine("OCCT", FlatCalculator.Code, "", "NZD");
			originRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(180);

			var originEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "", "", "20GP");
			originEntry3.TI_OH_Supplier = ZGuid.Empty;
			var originRateLine3 = originEntry3.AddRateLine("OCCT", FlatCalculator.Code, "", "NZD");
			originRateLine3.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(120);

			createFactory.Save();

			var page = new PricingPage(freightEntry, Factory, PricingPageStyle.Standard);

			const string expected = @"
[ShowEquipmentType = False]
ORG|AUSYD->|FCL||OCCT|Origin Port Charge|FLT,120,,,|20GP
ORG|AUSYD->|FCL||OCCT|Origin Port Charge|FLT,150,,,|20GP
ORG|AUSYD->|FCL||OCCT|Origin Port Charge|FLT,180,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestMatchingFromFreight_ChargesFromCompanyTariff

		public void TestMatchingFromFreight_ChargesFromCompanyTariff_RegistryDefaultCompanyTariffLevel0()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			NewLevel(orgHeader, tariffType: "FRT", mode: "SEA", direction: nameof(OrgRateTariffLevel.Directions.EXP), level: 1);

			Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.FCL, "SEA", "AU", "US", "BAF", 10m);

			var quote = Helper.NewQuote(orgHeader);
			quote.AddRateEntryWithFlatRateLine("FCL", "SEA", "AUSYD", "USNYC", "FRT", 20m);

			AssertEquals("Precondition: GlobalTariffDefault should be set to 0 (do not use)", 0, (int)Env.Registry.GlobalTariffDefault);

			var quoteEntry = quote.AllEntries.Single();
			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);
			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN CompanyTariff with level=1 has FCL rate WHEN printing quote for org with level=1 THEN should show the Company Tariff FCL rate",
				expected: new[]
				{
					@"
[ShowEquipmentType = False]
FCL|AUSYD->USNYC|SEA||FRT|International Freight|FLT,20,,,|",
					@"
[ShowEquipmentType = False]
FCL|AU->US|SEA||BAF|Bunker Adjustment Factor|FLT,10,,,|"
				},
				actual: RenderLineSets(GetFreightPricingPageLineSetFactory().LoadLineSets(page, page.RateEntries))
			);
		}

		static OrgRateTariffLevel NewLevel(OrgHeader orgHeader, ZString tariffType, ZString mode, ZString direction, ZByte level)
		{
			var orgRateTariffLevel = orgHeader.CompanyData.RateTariffLevels.AddNew();
			orgRateTariffLevel.P7_TariffType = tariffType;
			orgRateTariffLevel.P7_Mode = mode;
			orgRateTariffLevel.P7_Direction = direction;
			orgRateTariffLevel.P7_TariffLevel = level;

			return orgRateTariffLevel;
		}

		public void TestMatchingFromFreight_ChargesFromCompanyTariff()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Level 1";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var level0 = org.CompanyData.RateTariffLevels.AddNew();
			level0.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;
			level0.P7_Mode = "ALL";
			level0.P7_Direction = "ALL";
			level0.P7_TariffLevel = 0;

			TestMatchingFromFreight_ChargesFromCompanyTariffForFreight(tariff, org);
			TestMatchingFromFreight_ChargesFromCompanyTariffForDestination(tariff, org);
			TestMatchingFromFreight_ChargesFromCompanyTariffForOrigin(tariff, org);
		}

		void TestMatchingFromFreight_ChargesFromCompanyTariffForFreight(CompanyTariff tariff, OrgHeader org)
		{
			Helper.ChargeCodes.New("FSSS", "Freight Charge Code", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);

			var tariffEntry1 = tariff.AddRateEntry("FCL", "SEA", "AU", "US");
			var tariffRate1 = tariffEntry1.AddRateLine("FSSS", FlatCalculator.Code);
			tariffRate1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			var level1 = org.CompanyData.RateTariffLevels.AddNew();
			level1.P7_TariffType = "FRT";
			level1.P7_Mode = "SEA";
			level1.Validation.ValidateP7_Mode();
			AssertNoErrors(level1.P7_ModeInfo);
			level1.P7_Direction = "IMP";
			level1.P7_TariffLevel = 1;

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = org.PK;
			var quoteEntry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USNYC");
			var quoteRate1 = quoteEntry1.RateLines[0];
			quoteRate1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1500m;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";

			var page = new PricingPage(quoteEntry1, Factory, PricingPageStyle.Standard);

			var expected = @"
[ShowEquipmentType = False]
FCL|AUSYD->USNYC|SEA||FRT|International Freight|UNT,,,1500,|

[ShowEquipmentType = False]
FCL|AU->US|SEA||FSSS|Freight Charge Code|FLT,50,,,|
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			level1.P7_Direction = "EXP";

			page = new PricingPage(quoteEntry1, Factory, PricingPageStyle.Standard);
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromFreight_ChargesFromCompanyTariffForDestination(CompanyTariff tariff, OrgHeader org)
		{
			Helper.ChargeCodes.New("DSSS", "Destination Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			var tariffEntry1 = tariff.AddRateEntry("DST", "FCL", "", "US");
			var tariffRate1 = tariffEntry1.AddRateLine("DSSS", FlatCalculator.Code);
			tariffRate1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)120m;
			var tariffEntry2 = tariff.AddRateEntry("DST", "AIR", "", "US");
			var tariffRate2 = tariffEntry2.AddRateLine("DSSS", FlatCalculator.Code);
			tariffRate2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)150m;

			var level1 = org.CompanyData.RateTariffLevels.AddNew();
			level1.P7_TariffType = "DST";
			level1.P7_Mode = "FCL";
			level1.P7_Direction = "IMP";
			level1.P7_TariffLevel = 1;

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = org.PK;
			var quoteEntry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USNYC");
			var quoteRate1 = quoteEntry1.RateLines[0];
			quoteRate1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1200m;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";

			var page = new PricingPage(quoteEntry1, Factory, PricingPageStyle.Standard);

			var expected1 = @"
[ShowEquipmentType = False]
DST|->US|FCL||DSSS|Destination Charge Code|FLT,120,,,|
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected1, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			level1.P7_Mode = "LSE";
			level1.P7_Direction = "EXP";
			var quoteEntry2 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USNYC");
			var quoteRate2 = quoteEntry2.RateLines[0];
			quoteRate2.TL_RateCalculator = UnitCalculator.Code;
			quoteRate2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2200m;

			page = new PricingPage(quoteEntry2, Factory, PricingPageStyle.Standard);
			var expected2 = @"
[ShowEquipmentType = False]
DST|->US|AIR||DSSS|Destination Charge Code|FLT,150,,,|
";
			AssertMultilineASCIIEquals("", expected2, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromFreight_ChargesFromCompanyTariffForOrigin(CompanyTariff tariff, OrgHeader org)
		{
			Helper.ChargeCodes.New("OSSS", "Origin Charge Code", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var tariffEntry1 = tariff.AddRateEntry("ORG", "FCL", "AU", "");
			var tariffRate1 = tariffEntry1.AddRateLine("OSSS", FlatCalculator.Code);
			tariffRate1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)120m;
			var tariffEntry2 = tariff.AddRateEntry("ORG", "AIR", "AU", "");
			var tariffRate2 = tariffEntry2.AddRateLine("OSSS", FlatCalculator.Code);
			tariffRate2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)150m;

			var level1 = org.CompanyData.RateTariffLevels.AddNew();
			level1.P7_TariffType = "ORG";
			level1.P7_Mode = "FCL";
			level1.P7_Direction = "IMP";
			level1.P7_TariffLevel = 1;

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = org.PK;
			var quoteEntry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USNYC");
			var quoteRate1 = quoteEntry1.RateLines[0];
			quoteRate1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1200m;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";

			var page = new PricingPage(quoteEntry1, Factory, PricingPageStyle.Standard);

			var expected1 = @"
[ShowEquipmentType = False]
ORG|AU->|FCL||OSSS|Origin Charge Code|FLT,120,,,|
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected1, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			level1.P7_Mode = "LSE";
			level1.P7_Direction = "EXP";
			var quoteEntry2 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USNYC");
			var quoteRate2 = quoteEntry2.RateLines[0];
			quoteRate2.TL_RateCalculator = UnitCalculator.Code;
			quoteRate2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2200m;

			page = new PricingPage(quoteEntry2, Factory, PricingPageStyle.Standard);
			var expected2 = @"
[ShowEquipmentType = False]
ORG|AU->|AIR||OSSS|Origin Charge Code|FLT,150,,,|
";
			AssertMultilineASCIIEquals("", expected2, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestMatchingFromFreight_ChargesFromCompanyTariffLevel2()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Level 1";

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.DST, 20m);
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.FCL, 20m);

			factory2.Save();

			var org = Helper.NewOrgHeader(2);

			TestMatchingFromFreight_ChargesFromCompanyTariffForFreightL2(tariff, org);
			TestMatchingFromFreight_ChargesFromCompanyTariffForDestinationL2(tariff, org);
			TestMatchingFromFreight_ChargesFromCompanyTariffForOriginL2(tariff, org);
		}

		void TestMatchingFromFreight_ChargesFromCompanyTariffForFreightL2(CompanyTariff tariff, OrgHeader org)
		{
			Helper.ChargeCodes.New("FSSS", "Freight Charge Code", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);

			var tariffEntry1 = tariff.AddRateEntry("FCL", "SEA", "AU", "US");
			var tariffRate1 = tariffEntry1.AddRateLine("FSSS", FlatCalculator.Code);
			tariffRate1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			var quote = Helper.NewQuote(org);
			var quoteEntry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USNYC");
			var quoteRate1 = quoteEntry1.RateLines[0];
			quoteRate1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1500m;
			Factory.Save();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";

			var page = new PricingPage(quoteEntry1, new BusinessObjectFactory(), PricingPageStyle.Standard);

			var expected = @"
[ShowEquipmentType = False]
FCL|AUSYD->USNYC|SEA||FRT|International Freight|UNT,,,1500,|

[ShowEquipmentType = False]
FCL|AU->US|SEA||FSSS|Freight Charge Code|FLT,80,,,|
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromFreight_ChargesFromCompanyTariffForDestinationL2(CompanyTariff tariff, OrgHeader org)
		{
			Helper.ChargeCodes.New("DSSS", "Destination Charge Code", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			var tariffEntry1 = tariff.AddRateEntry("DST", "FCL", "", "US");
			var tariffRate1 = tariffEntry1.AddRateLine("DSSS", FlatCalculator.Code);
			tariffRate1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;
			var tariffEntry2 = tariff.AddRateEntry("DST", "AIR", "", "US");
			var tariffRate2 = tariffEntry2.AddRateLine("DSSS", FlatCalculator.Code);
			tariffRate2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

			var quote = Helper.NewQuote(org);
			var quoteEntry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USNYC");
			var quoteRate1 = quoteEntry1.RateLines[0];
			quoteRate1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1200m;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";
			Factory.Save();

			var page = new PricingPage(quoteEntry1, new BusinessObjectFactory(), PricingPageStyle.Standard);

			var expected1 = @"
[ShowEquipmentType = False]
DST|->US|FCL||DSSS|Destination Charge Code|FLT,80,,,|
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected1, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			var quoteEntry2 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USNYC");
			var quoteRate2 = quoteEntry2.RateLines[0];
			quoteRate2.TL_RateCalculator = UnitCalculator.Code;
			quoteRate2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2200m;
			Factory.Save();

			page = new PricingPage(quoteEntry2, new BusinessObjectFactory(), PricingPageStyle.Standard);
			var expected2 = @"
[ShowEquipmentType = False]
DST|->US|AIR||DSSS|Destination Charge Code|FLT,160,,,|
";
			AssertMultilineASCIIEquals("", expected2, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestMatchingFromFreight_ChargesFromCompanyTariffForOriginL2(CompanyTariff tariff, OrgHeader org)
		{
			Helper.ChargeCodes.New("OSSS", "Origin Charge Code", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var tariffEntry1 = tariff.AddRateEntry("ORG", "FCL", "AU", "");
			var tariffRate1 = tariffEntry1.AddRateLine("OSSS", FlatCalculator.Code);
			tariffRate1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;
			var tariffEntry2 = tariff.AddRateEntry("ORG", "AIR", "AU", "");
			var tariffRate2 = tariffEntry2.AddRateLine("OSSS", FlatCalculator.Code);
			tariffRate2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

			var quote = Helper.NewQuote(org);
			var quoteEntry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USNYC");
			var quoteRate1 = quoteEntry1.RateLines[0];
			quoteRate1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1200m;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";
			Factory.Save();

			var page = new PricingPage(quoteEntry1, new BusinessObjectFactory(), PricingPageStyle.Standard);

			var expected1 = @"
[ShowEquipmentType = False]
ORG|AU->|FCL||OSSS|Origin Charge Code|FLT,80,,,|
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected1, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			var quoteEntry2 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USNYC");
			var quoteRate2 = quoteEntry2.RateLines[0];
			quoteRate2.TL_RateCalculator = UnitCalculator.Code;
			quoteRate2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2200m;
			Factory.Save();

			page = new PricingPage(quoteEntry2, new BusinessObjectFactory(), PricingPageStyle.Standard);
			var expected2 = @"
[ShowEquipmentType = False]
ORG|AU->|AIR||OSSS|Origin Charge Code|FLT,160,,,|
";
			AssertMultilineASCIIEquals("", expected2, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestCompanyTariffUsingCTBCalcDoesNotTriggerInfiniteRecursion()
		{
			var tarrif1 = new BusinessObjectFactory().New<CompanyTariff>();
			var tariff1Entry = tarrif1.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var tariff1Line = tariff1Entry.AddRateLine("ODOC", FlatCalculator.Code);
			tariff1Line.GetCalculator<FlatCalculator>().BaseRate = 50m;
			tarrif1.Factory.Save();

			var tarrif2 = new BusinessObjectFactory().New<CompanyTariff>();
			var tarrif2Entry = tarrif2.ORGRateEntriesForBinding[0];
			var index = tarrif2Entry.RateLines.OverrideTariffLines(new[] { tarrif2Entry.RateLines[0] });
			var tariff2Line = tarrif2Entry.RateLines[index];
			tariff2Line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			tariff2Line.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 100;
			tarrif2.Factory.Save();

			NewClient2.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);
			NewClient2.Factory.Save();

			var quote = Helper.NewQuote(NewClient2);
			var quoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var quoteLine = quoteEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			quoteLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = -10;

			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);
			var expected = @"
[ShowEquipmentType = False]
ORG|AU->|LSE||ODOC|Origin Documentation Fee|FLT,90,,,|
";
			var actual = Render(GetOriginPricingPageLineSetFactory().LoadLineSets(page, page.RateEntries));

			AssertEquals("Pre-condition: client should be using level 2 company tariff", 2, NewClient2.CompanyData.RateTariffLevels.DefaultLevel);
			AssertMultilineASCIIEquals("Quote should be able return amount for CTB calc and should return 90", expected, actual);
		}

		public void TestCompanyTariffForAllLevelsIsLoadedOnPricingPage()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USNYC";
			Helper.ChargeCodes.New("FSSS", "Freight Charge Code", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight, string.Empty, true, true);
			Helper.ChargeCodes.New("DSSS", "Destination Charge Code", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination, string.Empty, true, true);
			Helper.ChargeCodes.New("OSSS", "Origin Charge Code", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, string.Empty, true, true);

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			AssertEquals((byte)1, tariff.TH_GlobalRateLevel);
			tariff.TH_GlobalRateDescription = "Level 1";

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.DST, 20m);
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.FCL, 20m);

			factory2.Save();

			var freightRateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "US", string.Empty, "20GP");
			var freightRateLine = freightRateEntry.AddRateLine("FSSS", UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)freightRateLine.Calculator).PerUnit = 500;

			var destinationRateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.FCL, string.Empty, "US", string.Empty, "40GP");
			var destinationRateLine = destinationRateEntry.AddRateLine("DSSS", UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)destinationRateLine.Calculator).PerUnit = 100;

			var originRateEntry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.FCL, "AU", string.Empty);
			var originRateLine = originRateEntry.AddRateLine("OSSS", UnitCalculator.Code, QuantityUnit.CN);
			((UnitCalculator)originRateLine.Calculator).PerUnit = 10;

			Factory.Save();

			var freightRateEntryCollection = tariff2.EntryCollections[RatingConstants.RateCategory.FCL];
			freightRateEntryCollection.LazyLoadingCollection.LoadAndSortForGUI();
			var freightRateEntryFromTariff2 = freightRateEntryCollection.LazyLoadingCollection.Cast<RateEntry>().First();

			var destinationRateEntryCollection = tariff2.EntryCollections[RatingConstants.RateCategory.DST];
			destinationRateEntryCollection.LazyLoadingCollection.LoadAndSortForGUI();
			var destinationRateEntryFromTariff2 = destinationRateEntryCollection.LazyLoadingCollection.Cast<RateEntry>().First();

			var originRateEntryCollection = tariff2.EntryCollections[RatingConstants.RateCategory.ORG];
			originRateEntryCollection.LazyLoadingCollection.LoadAndSortForGUI();
			var originRateEntryFromTariff2 = originRateEntryCollection.LazyLoadingCollection.Cast<RateEntry>().First();

			var expectedTariffLevel2PricingPageLineSetForFreight = @"
[ShowEquipmentType = False]
FCL|AU->US|SEA||FSSS|Freight Charge Code|UNT,,,400,|20GP
";
			var expectedTariffLevel2PricingPageLineSetForDestination = @"
[ShowEquipmentType = False]
DST|->US|FCL||DSSS|Destination Charge Code|UNT,,,80,|40GP
";
			var expectedTariffLevel2PricingPageLineSetForOrigin = @"
[ShowEquipmentType = False]
ORG|AU->|FCL||OSSS|Origin Charge Code|UNT,,,8,|
";

			TestPricingPageLineSetForCompanyTariffOnPricingPage(freightRateEntryFromTariff2, GetFreightPricingPageLineSetFactory(), expectedTariffLevel2PricingPageLineSetForFreight);
			TestPricingPageLineSetForCompanyTariffOnPricingPage(destinationRateEntryFromTariff2, GetDestinationPricingPageLineSetFactory(), expectedTariffLevel2PricingPageLineSetForDestination);
			TestPricingPageLineSetForCompanyTariffOnPricingPage(originRateEntryFromTariff2, GetOriginPricingPageLineSetFactory(), expectedTariffLevel2PricingPageLineSetForOrigin);
		}

		void TestPricingPageLineSetForCompanyTariffOnPricingPage(RateEntry rateEntry, PricingPageRateLineFactory lineSetFactory, string expected)
		{
			var page = new PricingPage(rateEntry, Factory, PricingPageStyle.Standard);
			AssertMultilineASCIIEquals(string.Empty, expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestInheritOtherChargesFromLessSpecificDestinations

		public void TestInheritOtherChargesFromLessSpecificDestinations()
		{
			var createFactory = new BusinessObjectFactory();
			var secondHelper = new TestHelper(createFactory);

			AccChargeCode[] destinationChargeCodes =
			{
				secondHelper.ChargeCodes.New("DCC1", "Destination Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true),
				secondHelper.ChargeCodes.New("DCC2", "Destination Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true),
			};

			AccChargeCode[] originChargeCodes =
			{
				secondHelper.ChargeCodes.New("OCC1", "Origin Charge Code 1", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true),
				secondHelper.ChargeCodes.New("OCC2", "Origin Charge Code 2", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true),
			};

			var tariff = createFactory.New<CompanyTariff>();
			var originTariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "");
			originTariffEntry.TI_LineOrder = 1;
			PricingPageTestHelper.AddChargeLines(originTariffEntry, 0, originChargeCodes);

			var destinationTariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "NLAMS");
			destinationTariffEntry.TI_LineOrder = 1;
			PricingPageTestHelper.AddChargeLines(destinationTariffEntry, 0, destinationChargeCodes);

			var quote = secondHelper.NewQuote(secondHelper.NewOrgHeader(1));

			var originQuoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "", "", "");
			originQuoteEntry.TI_LineOrder = 2;
			PricingPageTestHelper.AddChargeLines(originQuoteEntry, 1, originChargeCodes);

			var destinationQuoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "", "NLAMS", "", "");
			destinationQuoteEntry.TI_LineOrder = 2;
			PricingPageTestHelper.AddChargeLines(destinationQuoteEntry, 1, destinationChargeCodes);

			createFactory.Save();

			var reloadedQuote = Factory.Load<Quote>(quote.PK);
			var pages = new PricingPageCollection(reloadedQuote);
			pages.LoadStandard();

			AssertEquals("precondition: page count", 2, pages.Count);

			TestInheritOtherChargesFromLessSpecificOriginsForOrigin(pages[0]);
			TestInheritOtherChargesFromLessSpecificDestinationsForDestination(pages[1]);
		}

		void TestInheritOtherChargesFromLessSpecificDestinationsForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NLAMS|ALL||DCC1|Destination Charge Code 1|FLT,100,,,|

[ShowEquipmentType = False]
DST|->NLAMS|ALL||DCC2|Destination Charge Code 2|FLT,90,,,|
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestInheritOtherChargesFromLessSpecificOriginsForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AUBNE->|ALL||OCC1|Origin Charge Code 1|FLT,100,,,|

[ShowEquipmentType = False]
ORG|AUBNE->|ALL||OCC2|Origin Charge Code 2|FLT,90,,,|
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestInherit

		public void TestInherit()
		{
			var page = PricingPageTestHelper.SetupRates(Factory);

			TestInheritForFreight(page);
			TestInheritForDestination(page);
			TestInheritForOrigin(page);
		}

		void TestInheritForFreight(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
FCL|AUBNE->NLAMS|SEA|STD|FCC1|Freight Charge Code 1|FLT,100,,,|20GP
FCL|AUBNE->NLAMS|SEA|STD|FCC1|Freight Charge Code 1|FLT,100,,,|20RE
FCL|AUBNE->NLAMS|SEA|STD|FCC1|Freight Charge Code 1|FLT,100,,,|40GP
FCL|AUBNE->NLAMS|SEA|STD|FCC1|Freight Charge Code 1|FLT,100,,,|40RE

[ShowEquipmentType = False]
FCL|AUBNE->NLAMS|SEA|STD|FCC2|Freight Charge Code 2|FLT,90,,,|20GP
FCL|AUBNE->NLAMS|SEA|STD|FCC2|Freight Charge Code 2|FLT,90,,,|20RE
FCL|AUBNE->NLAMS|SEA|STD|FCC2|Freight Charge Code 2|FLT,90,,,|40GP
FCL|AUBNE->NLAMS|SEA|STD|FCC2|Freight Charge Code 2|FLT,90,,,|40RE

[ShowEquipmentType = False]
FCL|AUBNE->NLAMS|SEA|STD|FCC3|Freight Charge Code 3 desc overriden|FLT,80,,,|20GP
FCL|AUBNE->NLAMS|SEA|STD|FCC3|Freight Charge Code 3 desc overriden|FLT,80,,,|20RE
FCL|AUBNE->NLAMS|SEA|STD|FCC3|Freight Charge Code 3 desc overriden|FLT,80,,,|40GP
FCL|AUBNE->NLAMS|SEA|STD|FCC3|Freight Charge Code 3 desc overriden|FLT,80,,,|40RE
FCL|AUBNE->NLAMS|SEA|STD|FCC3|Freight Charge Code 3 desc overriden|FLT,980,,,|20GP
FCL|AUBNE->NLAMS|SEA|STD|FCC3|Freight Charge Code 3 desc overriden|FLT,980,,,|20RE
FCL|AUBNE->NLAMS|SEA|STD|FCC3|Freight Charge Code 3 desc overriden|FLT,980,,,|40GP
FCL|AUBNE->NLAMS|SEA|STD|FCC3|Freight Charge Code 3 desc overriden|FLT,980,,,|40RE
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestInheritForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NLAMS|ALL||DCC1|Destination Charge Code 1|FLT,100,,,|20GP, 20RE, 40GP, 40RE

[ShowEquipmentType = False]
DST|->NLAMS|ALL||DCC2|Destination Charge Code 2|FLT,90,,,|20GP, 20RE, 40GP, 40RE

[ShowEquipmentType = False]
DST|->NLAMS|ALL||DCC3|Destination Charge Code 3|FLT,80,,,|20GP, 20RE, 40GP, 40RE
";

			var firstOrDefault = page.RateEntries.FirstOrDefault();
			if (firstOrDefault != null)
			{
				var header = firstOrDefault.Parent;
				header.TH_PrintRateLevelDestinationCharges = true;
				header.TH_PrintInheritedDestinationCharges = true;
			}

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestInheritForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AUBNE->|ALL||OCC1|Origin Charge Code 1|FLT,100,,,|20GP, 20RE, 40GP, 40RE

[ShowEquipmentType = False]
ORG|AUBNE->|ALL||OCC2|Origin Charge Code 2|FLT,90,,,|20GP, 20RE, 40GP, 40RE

[ShowEquipmentType = False]
ORG|AUBNE->|ALL||OCC3|Origin Charge Code 3|FLT,80,,,|20GP, 20RE, 40GP, 40RE
";

			var header = page.RateEntries.First().Parent;
			header.TH_PrintRateLevelOriginCharges = true;
			header.TH_PrintInheritedOriginCharges = true;

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestNoInherit

		public void TestNoInherit()
		{
			var page = PricingPageTestHelper.SetupRates(Factory);

			TestNoInheritForDestination(page);
			TestNoInheritForOrigin(page);
		}

		void TestNoInheritForDestination(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
DST|->NLAMS|ALL||DCC3|Destination Charge Code 3|FLT,80,,,|20GP, 20RE, 40GP, 40RE
";

			var header = page.RateEntries.First().Parent;
			header.TH_PrintRateLevelDestinationCharges = true;
			header.TH_PrintInheritedDestinationCharges = false;

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestNoInheritForOrigin(PricingPage page)
		{
			const string expected = @"
[ShowEquipmentType = False]
ORG|AUBNE->|ALL||OCC3|Origin Charge Code 3|FLT,80,,,|20GP, 20RE, 40GP, 40RE
";

			var header = page.RateEntries.First().Parent;
			header.TH_PrintRateLevelOriginCharges = true;
			header.TH_PrintInheritedOriginCharges = false;

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestNoInherit

		public void TestNoLoad()
		{
			var page = PricingPageTestHelper.SetupRates(Factory);

			TestNoLoadForDestination(page);
			TestNoLoadForOrigin(page);
		}

		void TestNoLoadForDestination(PricingPage page)
		{
			const string expected = @"
";

			var header = page.RateEntries.First().Parent;
			header.TH_PrintRateLevelDestinationCharges = false;

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		void TestNoLoadForOrigin(PricingPage page)
		{
			const string expected = @"
";

			var header = page.RateEntries.First().Parent;
			header.TH_PrintRateLevelOriginCharges = false;

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region TestViaFilter

		public void TestViaFilter()
		{
			TestViaFilterForFreight();
			TestViaFilterForDestination();
			TestViaFilterForOrigin();
		}

		void TestViaFilterForFreight()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Helper.NewQuote(client);

			var entry1 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", "", "20GP");
			entry1.TI_ViaLRC = "SGSIN";
			entry1.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry1.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)540m;

			var entry2 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", "", "20GP");
			entry2.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry2.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;

			Factory.Save();

			var page1 = new PricingPage(entry1, Factory, PricingPageStyle.Standard);
			var page2 = new PricingPage(entry2, Factory, PricingPageStyle.Standard);

			var expected1 = @"
[ShowEquipmentType = False]
FCL|AUSYD->SGSIN->NZAKL|SEA||FRT|International Freight|UNT,,,540,|20GP";

			var expected2 = @"
[ShowEquipmentType = False]
FCL|AUSYD->NZAKL|SEA||FRT|International Freight|UNT,,,300,|20GP
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected1, Render(lineSetFactory.LoadLineSets(page1, page1.RateEntries)));
			AssertMultilineASCIIEquals("", expected2, Render(lineSetFactory.LoadLineSets(page2, page2.RateEntries)));
		}

		void TestViaFilterForDestination()
		{
			var createFactory = new BusinessObjectFactory();

			Helper.ChargeCodes.New("DCC1", "Destination Fee 1", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);
			Helper.ChargeCodes.New("DCC2", "Destination Fee 2", "FLT", ChargeCodeGroupList.Codes.Destination, "", true, true);

			createFactory.Save();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Helper.NewQuote(client);

			var entry1 = quote.AddRateEntry("DST", "FCL", "", "NZAKL", "", "20GP");
			entry1.TI_ViaLRC = "SGSIN";
			var rate1 = entry1.AddRateLine("DCC1", UnitCalculator.Code, "CN");
			rate1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var entry2 = quote.AddRateEntry("DST", "FCL", "", "NZAKL", "", "20GP");
			var rate2 = entry2.AddRateLine("DCC2", FlatCalculator.Code, "", "AUD");
			rate2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)52m;

			Factory.Save();

			var page1 = new PricingPage(entry1, Factory, PricingPageStyle.Standard);
			var page2 = new PricingPage(entry2, Factory, PricingPageStyle.Standard);

			var expected1 = @"
[ShowEquipmentType = False]
DST|->SGSIN->NZAKL|FCL||DCC1|Destination Fee 1|UNT,,,40,|20GP

[ShowEquipmentType = False]
DST|->NZAKL|FCL||DCC2|Destination Fee 2|FLT,52,,,|20GP
";

			var expected2 = @"
[ShowEquipmentType = False]
DST|->NZAKL|FCL||DCC2|Destination Fee 2|FLT,52,,,|20GP
";

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected1, Render(lineSetFactory.LoadLineSets(page1, page1.RateEntries)));
			AssertMultilineASCIIEquals("", expected2, Render(lineSetFactory.LoadLineSets(page2, page2.RateEntries)));
		}

		void TestViaFilterForOrigin()
		{
			var createFactory = new BusinessObjectFactory();

			Helper.ChargeCodes.New("OCC1", "Origin Fee 1", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);
			Helper.ChargeCodes.New("OCC2", "Origin Fee 2", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			createFactory.Save();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Helper.NewQuote(client);

			var entry1 = quote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			entry1.TI_ViaLRC = "SGSIN";
			var rate1 = entry1.AddRateLine("OCC1", UnitCalculator.Code, "CN", "AUD");
			rate1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var entry2 = quote.AddRateEntry("ORG", "FCL", "AUSYD", "", "", "20GP");
			var rate2 = entry2.AddRateLine("OCC2", FlatCalculator.Code, "", "AUD");
			rate2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)52m;

			Factory.Save();

			var page1 = new PricingPage(entry1, Factory, PricingPageStyle.Standard);
			var page2 = new PricingPage(entry2, Factory, PricingPageStyle.Standard);

			var expected1 = @"
[ShowEquipmentType = False]
ORG|AUSYD->SGSIN->|FCL||OCC1|Origin Fee 1|UNT,,,40,|20GP

[ShowEquipmentType = False]
ORG|AUSYD->|FCL||OCC2|Origin Fee 2|FLT,52,,,|20GP
";

			var expected2 = @"
[ShowEquipmentType = False]
ORG|AUSYD->|FCL||OCC2|Origin Fee 2|FLT,52,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected1, Render(lineSetFactory.LoadLineSets(page1, page1.RateEntries)));
			AssertMultilineASCIIEquals("", expected2, Render(lineSetFactory.LoadLineSets(page2, page2.RateEntries)));
		}

		#endregion

		#region TestCalculatorChangedInZeroRateLineMethod

		public void TestCalculatorChangedInZeroRateLineMethod()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");
			var line = entry.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)line.Calculator).BaseRate = 50;

			AssertNoExceptionThrown(() =>
			{
				var calculator = line.Calculator;

				foreach (RateLineItem item in line.RateLineItems)
				{
					line.RateCalculatorChanged = true;
					calculator.ShouldValueBeDiscounted(item);
				}
			});
		}

		#endregion

		#region Test Grouping

		public void TestGrouping_RegistryEnabled_LocalClient_NotOverridenRateLineDescription_EmptyChargeCodeLocalDescription()
		{
			AssertNullOrEmpty("Precondition", Helper.ChargeCodes["DDOC"].AC_LocalLanguageDescription);
			AssertEquals("Local Client", true, NewClient.IsLocalCountry);

			var quote = Helper.NewQuote(NewClient);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "40GP");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 10m);
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 20m);

			AssertGrouping
			(
				quoteRateEntry,
				isRegistryEnabled: true,
				expectedResult: @"[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Destination Documentation Fee|FLT,10,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Destination Documentation Fee|FLT,20,,,|40GP",
				message: "GIVEN same description (empty chargeCode localDescription should fallback to chargeCode description) THEN should be grouped"
			);
		}

		public void TestGrouping_RegistryEnabled_LocalClient_NotOverridenRateLineDescription_NotEmptyChargeCodeLocalDescription()
		{
			Helper.ChargeCodes["DDOC"].AC_LocalLanguageDescription = "DDOC Local Description";
			AssertEquals("Local Client", true, NewClient.IsLocalCountry);

			var quote = Helper.NewQuote(NewClient);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "40GP");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 10m);
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 20m);

			AssertGrouping
			(
				quoteRateEntry,
				isRegistryEnabled: true,
				expectedResult: @"[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|DDOC Local Description|FLT,10,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee|DDOC Local Description|FLT,20,,,|40GP",
				message: "GIVEN same description THEN should be grouped"
			);
		}

		public void TestGrouping_RegistryEnabled_LocalClient_OverridenRateLineDescription()
		{
			AssertEquals("Local Client", true, NewClient.IsLocalCountry);

			var quote = Helper.NewQuote(NewClient);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "40GP");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 10m, "Overriden Local Description 1");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 20m, "Overriden Local Description 2");

			AssertGrouping
			(
				quoteRateEntry,
				isRegistryEnabled: true,
				expectedResult: @"[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 1|FLT,10,,,|40GP

[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 2|FLT,20,,,|40GP",
				message: "GIVEN different local-description THEN should not be grouped"
			);
		}

		public void TestGrouping_RegistryEnabled_LocalClient_EmptyOverridenRateLineDescription()
		{
			AssertEquals("Local Client", true, NewClient.IsLocalCountry);

			var quote = Helper.NewQuote(NewClient);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "40GP");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 10m, "Overriden Local Description 1");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 20m, "");

			AssertGrouping
			(
				quoteRateEntry,
				isRegistryEnabled: true,
				expectedResult: @"[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 1|FLT,10,,,|40GP

[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee||FLT,20,,,|40GP",
				message: "GIVEN different local-description THEN should not be grouped"
			);
		}

		public void TestGrouping_RegistryEnabled_LocalClient_EmptyOverridenRateLineDescription_BothRateLines()
		{
			AssertEquals("Local Client", true, NewClient.IsLocalCountry);

			var quote = Helper.NewQuote(NewClient);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "40GP");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 10m, "");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 20m, "");

			AssertGrouping
			(
				quoteRateEntry,
				isRegistryEnabled: true,
				expectedResult: @"[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee||FLT,10,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee||FLT,20,,,|40GP",
				message: "GIVEN empty local-description THEN should fallback to description and grouped because they are the same"
			);
		}

		public void TestGrouping_RegistryEnabled_NonLocalClient()
		{
			var nonLocalClient = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("NonLocal Debtor", false, nonLocalClient.IsLocalCountry);

			Factory.Save();

			var quote = Helper.NewQuote(nonLocalClient);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "40GP");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 10m, "Overriden Local Description 1");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 20m, "Overriden Local Description 2");

			AssertGrouping
			(
				quoteRateEntry,
				isRegistryEnabled: true,
				expectedResult: @"[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 1|FLT,10,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 2|FLT,20,,,|40GP",
				message: "GIVEN same description THEN should be grouped"
			);
		}

		public void TestGrouping_RegistryDisabled_LocalClient()
		{
			AssertEquals("Local Client", true, NewClient.IsLocalCountry);

			Factory.Save();

			var quote = Helper.NewQuote(NewClient);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "40GP");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 10m, "Overriden Local Description 1");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 20m, "Overriden Local Description 2");

			AssertGrouping
			(
				quoteRateEntry,
				isRegistryEnabled: false,
				expectedResult: @"[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 1|FLT,10,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 2|FLT,20,,,|40GP",
				message: "GIVEN same description THEN should be grouped"
			);
		}

		public void TestGrouping_RegistryEnabled_LocalClient_MultipleRateLines()
		{
			AssertEquals("Local Client", true, NewClient.IsLocalCountry);

			var quote = Helper.NewQuote(NewClient);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "", "40GP");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 10m, "Overriden Local Description 1");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 20m, "Overriden Local Description 1");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 30m, "Overriden Local Description 2");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 40m, "Overriden Local Description 2");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 50m);
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 60m);
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 70m, "");
			AddRateLineWithFlatCalculatorToRateEntry(quoteRateEntry, "DDOC", 80m, "");

			AssertGrouping
			(
				quoteRateEntry,
				isRegistryEnabled: true,
				expectedResult: @"[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 1|FLT,10,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 1|FLT,20,,,|40GP

[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 2|FLT,30,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Overriden Local Description 2|FLT,40,,,|40GP

[ShowEquipmentType = False]
FCL|AU->|SEA||DDOC|Destination Documentation Fee||FLT,70,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee||FLT,80,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Destination Documentation Fee|FLT,50,,,|40GP
FCL|AU->|SEA||DDOC|Destination Documentation Fee|Destination Documentation Fee|FLT,60,,,|40GP",
				message: "Same description should be grouped, empty local-description should default to description"
			);
		}

		RateLine AddRateLineWithFlatCalculatorToRateEntry(RateEntry rateEntry, ZString chargeCode, ZDecimal baseRate, string localDescription = null)
		{
			var rateLine = Helper.AddRateLineWithFlatCalculatorToRateEntry(rateEntry, chargeCode, baseRate);

			if (localDescription != null)
			{
				rateLine.OverrideChargeDescription = true;
				rateLine.TL_RateDescLocal = localDescription;
			}

			return rateLine;
		}

		void AssertGrouping(RateEntry rateEntry, bool isRegistryEnabled, string expectedResult, string message = default)
		{
			var page = new PricingPage(rateEntry, Factory, PricingPageStyle.Standard);
			var pricingPageFactory = new PricingPageRateLineFactory(EntryTypes.Freight);

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled))
			{
				var result = pricingPageFactory.LoadLineSets(page, page.RateEntries);
				var resultAsString = Render(result, showLocalDescription: true).Trim();
				AssertMultilineASCIIEquals(message, expectedResult, resultAsString);
			}
		}

		#endregion

		#region Origin Only Tests

		public void TestGroupingForOrigin()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");

			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "");
			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUSYD", "");
			var entry3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "");

			var line1a = entry1.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)line1a.Calculator).BaseRate = 50;

			var line1b = entry1.AddRateLine("OPCH", FlatCalculator.Code);
			((FlatCalculator)line1b.Calculator).BaseRate = 50;

			var line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)line2.Calculator).BaseRate = 51;

			var line3 = entry3.AddRateLine("ODOC", FlatCalculator.Code);
			((FlatCalculator)line3.Calculator).BaseRate = 52;

			Factory.Save();

			var page = new PricingPage(entry, Factory, PricingPageStyle.Landscape);

			var strategy = new Mock<IPricingPageRateLineListGroupStrategy<string>>();

			strategy.Setup(m => m.GroupKey(It.Is<RateEntry>(p => p == entry1))).Returns("Base");
			strategy.Setup(m => m.GroupKey(It.Is<RateEntry>(p => p == entry2))).Returns("A");
			strategy.Setup(m => m.GroupKey(It.Is<RateEntry>(p => p == entry3))).Returns("B");

			Action<IDictionary<string, List<RateLine>>> crossPolinateDelegate = d =>
			{
				var list = d["Base"];
				d["A"].AddRange(list);
				d["B"].AddRange(list);
			};

			strategy.Setup(m => m.CrossPollinate(It.IsAny<IDictionary<string, List<RateLine>>>())).Callback(crossPolinateDelegate);

			Action<IDictionary<string, List<PricingPageRateLineList>>> purgeDelegate = d => d.Remove("Base");

			strategy.Setup(m => m.Purge(It.IsAny<IDictionary<string, List<PricingPageRateLineList>>>())).Callback(purgeDelegate);

			IDictionary<string, List<PricingPageRateLineList>> result;

			var lineSetFactory = GetOriginPricingPageLineSetFactory();

			result = lineSetFactory.LoadLineSets(strategy.Object, page, page.RateEntries);

			const string expected = @"
--- A ---

[ShowEquipmentType = False]
ORG|AU->|ALL||ODOC|Origin Documentation Fee|FLT,50,,,|

[ShowEquipmentType = False]
ORG|AUSYD->|ALL||ODOC|Origin Documentation Fee|FLT,51,,,|

[ShowEquipmentType = False]
ORG|AU->|ALL||OPCH|Origin Port Charges|FLT,50,,,|

--- B ---

[ShowEquipmentType = False]
ORG|AU->|ALL||ODOC|Origin Documentation Fee|FLT,50,,,|

[ShowEquipmentType = False]
ORG|AUBNE->|ALL||ODOC|Origin Documentation Fee|FLT,52,,,|

[ShowEquipmentType = False]
ORG|AU->|ALL||OPCH|Origin Port Charges|FLT,50,,,|
";

			var builder = new StringBuilder();

			foreach (var pair in result)
			{
				builder.AppendLine();
				builder.Append("--- ");
				builder.Append(pair.Key);
				builder.Append(" ---");
				builder.AppendLine();

				foreach (var set in pair.Value)
				{
					builder.AppendLine(Render(set));
				}
			}

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestClientRatesOverrideCompanyTariffWhenHavingDifferentServiceLevel()
		{
			Helper.ChargeCodes.New("FCCT", "Freight Charge", "FLT", ChargeCodeGroupList.Codes.Freight, "", true, true);
			Helper.ChargeCodes.New("OCCT", "Origin Documentation Fee", "FLT", ChargeCodeGroupList.Codes.Origin, "", true, true);

			var client = Helper.NewOrgHeader(1);

			var tariff = Factory.New<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "STD", "20GP");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			var line1 = tariffEntry.AddRateLine("OCCT", FlatCalculator.Code);
			((FlatCalculator)line1.Calculator).BaseRate = 50;

			var rate = Helper.NewClientRate(client);
			var clientRateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "NZAKL", "", "20GP");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var line2 = clientRateEntry.AddRateLine("OCCT", FlatCalculator.Code);
			((FlatCalculator)line2.Calculator).BaseRate = 100;

			var quote = Helper.NewQuote(client);

			var freightEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "NZAKL", "STD", "20GP");
			var freightRateLine = freightEntry.AddRateLine("FCCT", FlatCalculator.Code);
			freightRateLine.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(1000);

			quote.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "NZAKL", "", "20GP");

			var page = new PricingPage(freightEntry, Factory, PricingPageStyle.Standard);

			const string expected = @"
[ShowEquipmentType = False]
ORG|AUSYD->NZAKL|FCL||OCCT|Origin Documentation Fee|FLT,100,,,|20GP
";

			var lineSetFactory = GetOriginPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));

			quote.EntryCollections[RatingConstants.RateCategory.FCL].LazyLoadingCollection.RemoveAll();
			freightEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "NZAKL", "", "20GP");
			freightRateLine = freightEntry.AddRateLine("FCCT", FlatCalculator.Code);
			freightRateLine.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(1000);

			AssertMultilineASCIIEquals("The client rate has a more general service level, so it will override the company tariff",
				expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));

			tariff.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.RemoveAll();
			tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AU", "", "", "20GP");
			tariffEntry.RateLines.RemoveAndDeleteAll();
			line1 = tariffEntry.AddRateLine("OCCT", FlatCalculator.Code);
			((FlatCalculator)line1.Calculator).BaseRate = 50;

			clientRateEntry.TI_RS_NKServiceLevel_NI = "STD";
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			line2 = clientRateEntry.AddRateLine("OCCT", FlatCalculator.Code);
			((FlatCalculator)line2.Calculator).BaseRate = 100;

			Factory.Save();

			const string expected2 = @"
[ShowEquipmentType = False]
ORG|AU->|FCL||OCCT|Origin Documentation Fee|FLT,50,,,|20GP
ORG|AUSYD->NZAKL|FCL|STD|OCCT|Origin Documentation Fee|FLT,100,,,|20GP
";
			page = new PricingPage(freightEntry, new BusinessObjectFactory(), PricingPageStyle.Standard);

			AssertMultilineASCIIEquals("The client rate has a more specific service level, so it will not override the company tariff because the service level of starting entry is not specified",
				expected2, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestOriginRelatedRates_ClientRatesOverrideLevel1CompanyTariff()
		{
			var level1Tariff = Factory.New<CompanyTariff>();
			var tariffEntry = level1Tariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			var tariffLine = tariffEntry.AddRateLine("ODOC", FlatCalculator.Code);
			tariffLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			tariffLine.TL_RateDesc = "Tariff Origin";
			Factory.Save();

			var client = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AU", "");
			var rateLine = rateEntry.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 80m;
			rateLine.TL_RateDesc = "Client Rate Origin";

			var quotation = Helper.NewQuote(client);
			var quoteFreightEntry = quotation.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "", "FRT", 100m);
			quoteFreightEntry.TI_RC = GP20.PK;

			Factory.Save();

			var collection = new PricingPageCollection(quotation);
			collection.Load(PricingPaginationStrategy.StandardStyle);
			var pricingPage = collection.Cast<PricingPage>().Single();

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			var lineSets = lineSetFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);
			var actualResults = lineSets.SelectMany(lineSet => lineSet).Select(line => line.PK).ToArray();

			var message = "When charge codes overlap and rate entries match, Client Rate should be preferred to Company Tariff Rates";

			AssertContainsExactElementsInAnyOrder(message, new[] { rateLine.PK }, actualResults);
		}

		public void TestOriginRelatedRates_ClientRatesOverrideLevel2CompanyTariff()
		{
			var level1Tariff = Factory.New<CompanyTariff>();
			var tariffEntry = level1Tariff.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "");
			var tariffLine = tariffEntry.AddRateLine("ODOC", FlatCalculator.Code);
			tariffLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			tariffLine.TL_RateDesc = "Tariff Origin - Documents";

			var tariffLine2 = tariffEntry.AddRateLine("OCART", FlatCalculator.Code);
			tariffLine2.GetCalculator<FlatCalculator>().BaseRate = 50m;
			tariffLine2.TL_RateDesc = "Tariff Origin - Cartage";

			var level2Tariff = Factory.New<CompanyTariff>();
			Factory.Save();

			var client = Helper.NewOrgHeader(2);
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "");
			var rateLine = rateEntry.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 80m;
			rateLine.TL_RateDesc = "Client Rate Origin";

			var quotation = Helper.NewQuote(client);
			var quoteFreightEntry = quotation.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AU", "", "FRT", 100m);
			quoteFreightEntry.TI_RC = GP20.PK;

			Factory.Save();

			var collection = new PricingPageCollection(quotation);
			collection.Load(PricingPaginationStrategy.StandardStyle);
			var pricingPage = collection.Cast<PricingPage>().Single();

			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			var lineSets = lineSetFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);
			var actualResults = lineSets.SelectMany(lineSet => lineSet).Select(line => line.PK).ToArray();

			var message = "When charge codes overlap and rate entries match, Client Rate should be preferred to Company Tariff Rates";

			AssertContainsExactElementsInAnyOrder(message, new[] { rateLine.PK, tariffLine2.PK }, actualResults);
		}

		public void TestFromAndToSuburbsShouldBePrintedAsSeparateRateLines()
		{
			var chargeCode = Helper.ChargeCodes.New("TBCSTD", "Transport Standard", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			var cityTown1 = Factory.NewWithValidTestData<RefCityTown>();
			var cityTown2 = Factory.NewWithValidTestData<RefCityTown>();
			cityTown1.R9_RN_NKCountry = CountryCodes.Australia;
			cityTown2.R9_RN_NKCountry = CountryCodes.Australia;
			Factory.Save();

			var quotation = Helper.NewQuote(NewClient);
			var category = RatingConstants.RateCategory.TBC;
			var quoteEntry1 = quotation.AddRateEntry(category, RateMode.FRO, "AU", "");
			quoteEntry1.OriginSuburbPK = cityTown1.PK;
			var quoteLine1 = quoteEntry1.AddRateLine(chargeCode);
			quoteLine1.GetCalculator<FlatCalculator>().BaseRate = 10m;

			var quoteEntry2 = quotation.AddRateEntry(category, RateMode.FRO, "AU", "");
			quoteEntry2.DestinationSuburbPK = cityTown2.PK;
			var quoteLine2 = quoteEntry2.AddRateLine(chargeCode);
			quoteLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var quoteEntry3 = quotation.AddRateEntry(category, RateMode.FRO, "AU", "");
			var quoteLine3 = quoteEntry3.AddRateLine(chargeCode);
			quoteLine3.GetCalculator<FlatCalculator>().BaseRate = 30m;

			Factory.Save();

			var collection = new PricingPageCollection(quotation);
			collection.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			var pricingPage = collection.Cast<PricingPage>().Single();

			var expectedResults = new[] { quoteLine1.PK, quoteLine2.PK, quoteLine3.PK, };
			var lineSetFactory = GetOriginPricingPageLineSetFactory();
			var lineSets = lineSetFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);
			var actualResults = lineSets.SelectMany(lineSet => lineSet).Select(line => line.PK);

			AssertContainsExactElementsInAnyOrder(expectedResults, actualResults);
		}

		#endregion

		#region Freight Only Tests

		public void TestVisibilityWhenBaseIsZeroForFreight()
		{
			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityCheckingWhenBaseIsZero(Factory);

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", @"
[ShowEquipmentType = False]
FCL|AUBNE->NLAMS|SEA|STD|FCC1|Freight Charge Code 1|FLT,100,,,|20GP
",
									Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestVisibilityWhenBaseIsNotZeroAgencyCalculatorForFreight()
		{
			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityCheckingWhenBaseIsZeroAgencyCalculator(Factory, 10);

			var lineSetFactory = GetFreightPricingPageLineSetFactory();
			var expectedLines = new[]
			{
				@"
[ShowEquipmentType = False]
FCL|AUBNE->NLAMS|SEA|STD|FCC|Freight Charge Code|FLT,100,,,|20GP",
				@"
[ShowEquipmentType = False]
FCL|AUBNE->NLAMS|SEA|STD|CCC|Customs Charge Code|AGY,,,,100000|20GP"
			};

			AssertContainsExactElementsInAnyOrder(
				expectedLines,
				RenderLineSets(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestVisibilityWhenBaseIsZeroAgencyCalculatorForFreight()
		{
			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityCheckingWhenBaseIsZeroAgencyCalculator(Factory, -100);

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", @"
[ShowEquipmentType = False]
FCL|AUBNE->NLAMS|SEA|STD|FCC|Freight Charge Code|FLT,100,,,|20GP
",
			Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestVisibilityWithOverrideForFreight()
		{
			var page = PricingPageTestHelper.SetupSampleRatesForVisibilityCheckingWithOverride(Factory);

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", @"
[ShowEquipmentType = False]
FCL|AUBNE->NLAMS|SEA|STD|FCC1|Freight Charge Code 1|FLT,100,,,|20GP
",
			Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestSortByPrintSequenceForFreight()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "CLIENT";
			PricingPageTestHelper.SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.Sequence);

			var page = SetupForFreightSortTest(client);

			Factory.Save();

			var expected = @"
[ShowEquipmentType = False]
FCL|AU->NL|SEA||XD1|Code D1|UNT,,,51,|

[ShowEquipmentType = False]
FCL|AU->NL|SEA||XB2|Code B2|UNT,,,52,|

[ShowEquipmentType = False]
FCL|AU->NL|SEA||XC3|Code C3|UNT,,,53,|

[ShowEquipmentType = False]
FCL|AU->NL|SEA||XA4|Code A4|UNT,,,54,|
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestSortAlphabeticalForFreight()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "CLIENT";
			PricingPageTestHelper.SetSortOrder(client, OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical);

			var page = SetupForFreightSortTest(client);

			Factory.Save();

			var expected = @"
[ShowEquipmentType = False]
FCL|AU->NL|SEA||XA4|Code A4|UNT,,,54,|

[ShowEquipmentType = False]
FCL|AU->NL|SEA||XB2|Code B2|UNT,,,52,|

[ShowEquipmentType = False]
FCL|AU->NL|SEA||XC3|Code C3|UNT,,,53,|

[ShowEquipmentType = False]
FCL|AU->NL|SEA||XD1|Code D1|UNT,,,51,|
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestInternationalZoneRatesForFreight()
		{
			var zone = Helper.NewInternationalZone("AUTS", null, "AUSYD", "AUMEL");
			var containerPK = Helper.Containers["20GP"].PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cost = Helper.NewCosting(org);

			var entry1 = cost.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			entry1.TI_RC = containerPK;
			entry1.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry1.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1100m;

			var entry2 = cost.AddRateEntry("FCL", "SEA", "AUMEL", "NZAKL");
			entry2.TI_RC = containerPK;
			entry2.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry2.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2200m;

			var entry3 = cost.AddRateEntry("FCL", "SEA", "AUTS", "NZAKL");
			entry3.TI_RC = containerPK;
			entry3.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry3.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3300m;

			var page = new PricingPage(entry3, Factory, PricingPageStyle.Landscape);

			var expected = @"
[ShowEquipmentType = False]
FCL|AUTS->NZAKL|SEA||FRT|International Freight|UNT,,,3300,|20GP
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();

			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		public void TestNotMatchingByFrequencyForFreight()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_OH = carrier.PK;

			var costingEntry1 = costing.AddRateEntry("FCL", "SEA", "DE", "NZ");
			costingEntry1.TI_Frequency = 1;
			costingEntry1.TI_FrequencyUnit = FrequencyList.Codes.Week;
			costingEntry1.TI_OH_TransportProvider = carrier.PK;
			costingEntry1.RateLines.RemoveAndDeleteAll();
			var costingRate1 = costingEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			costingRate1.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)350m;

			var costingEntry2 = costing.AddRateEntry("FCL", "SEA", "DEHAM", "NZAKL");
			costingEntry2.TI_Frequency = 1;
			costingEntry2.TI_FrequencyUnit = FrequencyList.Codes.Week;
			costingEntry2.TI_OH_TransportProvider = carrier.PK;
			costingEntry2.RateLines.RemoveAndDeleteAll();
			var costingRate2 = costingEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			costingRate2.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)785m;

			Factory.Save();

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Level 1";

			var tariffEntry1 = tariff.AddRateEntry("FCL", "SEA", "DEHAM", "NZAKL");
			tariffEntry1.TI_OH_TransportProvider = carrier.PK;
			tariffEntry1.TI_Frequency = 1;
			tariffEntry1.TI_FrequencyUnit = FrequencyList.Codes.Week;
			tariffEntry1.RateLines.RemoveAndDeleteAll();
			var tariffRate1 = tariffEntry1.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffRate1.Calculator[CompanyTariffOrCostBasedCalculator.Items.Operator.UNT] = (ZDecimal)150m;
			var tariffRate2 = tariffEntry1.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CostBasedCode);

			var tariffEntry2 = tariff.AddRateEntry("FCL", "SEA", "", "NZ");
			tariffEntry2.RateLines.RemoveAndDeleteAll();
			var tariffRate3 = tariffEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			tariffRate3.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)1111m;

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var level0 = org.CompanyData.RateTariffLevels.AddNew();
			level0.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;
			level0.P7_Mode = "ALL";
			level0.P7_Direction = "ALL";
			level0.P7_TariffLevel = 0;
			var level1 = org.CompanyData.RateTariffLevels.AddNew();
			level1.P7_TariffType = "FRT";
			level1.P7_Mode = "SEA";
			level1.Validation.ValidateP7_Mode();
			AssertNoErrors(level1.P7_ModeInfo);
			level1.P7_Direction = "ALL";
			level1.P7_TariffLevel = 1;

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = org.PK;

			var quoteEntry = quote.AddRateEntry("FCL", "SEA", "DEHAM", "NZAKL");
			quoteEntry.TI_OH_TransportProvider = carrier.PK;
			quoteEntry.TI_RC = GP20.PK;
			var quoteRateLine = quoteEntry.RateLines[0];
			quoteRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			var page = new PricingPage(quoteEntry, Factory, PricingPageStyle.Standard);

			var expected = @"
[ShowEquipmentType = False]
FCL|DEHAM->NZAKL|SEA||FRT|International Freight|UNT,,,500,|20GP

[ShowEquipmentType = False]
FCL|DEHAM->NZAKL|SEA||BAF|Bunker Adjustment Factor|UNT,,,785,|20GP
";

			var lineSetFactory = GetFreightPricingPageLineSetFactory();
			AssertMultilineASCIIEquals("", expected, Render(lineSetFactory.LoadLineSets(page, page.RateEntries)));
		}

		#endregion

		#region Destination Only Tests

		public void TestDestinationRelatedRates_ClientRatesOverrideLevel1CompanyTariff()
		{
			var level1Tariff = Factory.New<CompanyTariff>();
			var tariffEntry = level1Tariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU");
			var tariffLine = tariffEntry.AddRateLine("ODOC", FlatCalculator.Code);
			tariffLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			tariffLine.TL_RateDesc = "Tariff Origin";

			var client = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU");
			var rateLine = rateEntry.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 80m;
			rateLine.TL_RateDesc = "Client Rate Origin";

			var quotation = Helper.NewQuote(client);
			var quoteFreightEntry = quotation.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "", "AU", "FRT", 100m);
			quoteFreightEntry.TI_RC = GP20.PK;

			Factory.Save();

			var collection = new PricingPageCollection(quotation);
			collection.Load(PricingPaginationStrategy.StandardStyle);
			var pricingPage = collection.Cast<PricingPage>().Single();

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			var lineSets = lineSetFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);
			var actualResults = lineSets.SelectMany(lineSet => lineSet).Select(line => line.PK).ToArray();

			var message = "When charge codes overlap and rate entries match, Client Rate should be preferred to Company Tariff Rates";

			AssertContainsExactElementsInAnyOrder(message, new[] { rateLine.PK }, actualResults);
		}

		public void TestDestinationRelatedRates_ClientRatesOverrideLevel2CompanyTariff()
		{
			var level1Tariff = Factory.New<CompanyTariff>();
			var tariffEntry = level1Tariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "", "AU");
			var tariffLine = tariffEntry.AddRateLine("ODOC", FlatCalculator.Code);
			tariffLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			tariffLine.TL_RateDesc = "Tariff Origin";

			var level2Tariff = Factory.New<CompanyTariff>();
			Factory.Save();

			var client = Helper.NewOrgHeader(2);
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.LCL, "", "AU");
			var rateLine = rateEntry.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 80m;
			rateLine.TL_RateDesc = "Client Rate Origin";

			var quotation = Helper.NewQuote(client);
			var quoteFreightEntry = quotation.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "", "AU", "FRT", 100m);
			quoteFreightEntry.TI_RC = GP20.PK;

			Factory.Save();

			var collection = new PricingPageCollection(quotation);
			collection.Load(PricingPaginationStrategy.StandardStyle);
			var pricingPage = collection.Cast<PricingPage>().Single();

			var lineSetFactory = GetDestinationPricingPageLineSetFactory();
			var lineSets = lineSetFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);
			var actualResults = lineSets.SelectMany(lineSet => lineSet).Select(line => line.PK).ToArray();

			var message = "When charge codes overlap and rate entries match, Client Rate should be preferred to Company Tariff Rates";

			AssertContainsExactElementsInAnyOrder(message, new[] { rateLine.PK }, actualResults);
		}

		#endregion

		#region Checking arguments for null

		public void TestIsVisibleOnDocumentsChecksForNulls()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var rate = Factory.New<ClientRate>();
			rate.TH_OH = client.PK;

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.FCL);
			entry.RateLines.RemoveAndDeleteAll();

			var rateLine = entry.RateLines.AddNew();
			AssertNull(rateLine.ChargeCode);

			var page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			AssertNoExceptionThrown("Null exception for RateLine.ChargeCode", () =>
				{
					var lineSetFactory = GetFreightPricingPageLineSetFactory();
					var pageLineSetList = lineSetFactory.LoadLineSets(page, page.RateEntries);
				});
		}

		public void TestAddRateLinesDoesNotIncludeLinesWithoutChargeCodes()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var fclEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "STD", "20GP");

			fclEntry.RateLines.RemoveAndDeleteAll();

			var rateLineWithChargeCode = fclEntry.AddRateLine(Helper.ChargeCodes["FRT"], FlatCalculator.Code);
			AssertNotNull(rateLineWithChargeCode.ChargeCode);

			var rateLineWithoutChargeCode = fclEntry.RateLines.AddNew();
			AssertNull(rateLineWithoutChargeCode.ChargeCode);

			var page = new PricingPage(fclEntry, Factory, PricingPageStyle.Standard);

			AssertNoExceptionThrown("Null exception for RateLine.ChargeCode", () =>
			{
				var lineSetFactory = GetFreightPricingPageLineSetFactory();
				var pageLineSetList = lineSetFactory.LoadLineSets(page, page.RateEntries);
			});
		}

		#endregion

		#region Load Entries does not included Deleted Entries

		public void TestLoadEntriesDoesNotIncludedDeletedEntries()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var destinationEntry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "", "", "40GP");
			var destinationRateLine1 = destinationEntry1.AddRateLine("DDOC", FlatCalculator.Code);
			destinationRateLine1.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(20);

			var destinationEntry2 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "", "", "40HC");
			var destinationRateLine2 = destinationEntry2.AddRateLine("DDOC", FlatCalculator.Code);
			destinationRateLine2.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(25);

			var destinationEntry3 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "", "", "20GP");
			var destinationRateLine3 = destinationEntry3.AddRateLine("DDOC", FlatCalculator.Code);
			destinationRateLine3.Calculator[Calculator.Items.Operator.BAS] = new ZDecimal(40);

			Factory.Save();

			var page = new PricingPage(destinationEntry1, Factory, PricingPageStyle.Standard);
			page.AddRateEntry(destinationEntry2);
			page.AddRateEntry(destinationEntry3);

			var pricingPageFactory = new PricingPageRateLineFactory(EntryTypes.Freight);
			var results = pricingPageFactory.LoadLineSets(page, page.RateEntries);

			AssertEquals("Should have the main entry", 1, results.Count);

			destinationEntry3.Delete();
			results = pricingPageFactory.LoadLineSets(page, page.RateEntries);

			AssertEquals("No exception expected", 1, results.Count);
		}

		#endregion

		#region Implementation

		PricingPage SetupForFreightSortTest(OrgHeader client)
		{
			var xa4 = Helper.ChargeCodes.New("XA4", "Code A4", "FLT", "FRT", "", true, true);
			xa4.AC_PrintSequence = 4;

			var xb2 = Helper.ChargeCodes.New("XB2", "Code B2", "FLT", "FRT", "", true, true);
			xb2.AC_PrintSequence = 2;

			var xc3 = Helper.ChargeCodes.New("XC3", "Code C3", "FLT", "FRT", "", true, true);
			xc3.AC_PrintSequence = 3;

			var xd1 = Helper.ChargeCodes.New("XD1", "Code D1", "FLT", "FRT", "", true, true);
			xd1.AC_PrintSequence = 1;

			var rate = Factory.New<ClientRate>();
			rate.TH_OH = client.PK;

			var entry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NL");
			entry.RateLines.RemoveAndDeleteAll();
			entry.AddRateLine(xc3, UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 53;
			entry.AddRateLine(xa4, UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 54;
			entry.AddRateLine(xd1, UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 51;
			entry.AddRateLine(xb2, UnitCalculator.Code, QuantityUnit.CN).GetCalculator<UnitCalculator>().PerUnit = 52;

			return new PricingPage(entry, Factory, PricingPageStyle.Standard);
		}

		static IEnumerable<string> RenderLineSets(IEnumerable<PricingPageRateLineList> list) => list.Select(set => Render(set));

		static IEnumerable<string> RenderAsStringCollection(IEnumerable<PricingPageRateLineList> pricingPageLineSetCollection)
		{
			foreach (var pricingPageLineSet in pricingPageLineSetCollection)
			{
				yield return Render(pricingPageLineSet);
			}
		}

		static string Render(IEnumerable<PricingPageRateLineList> list, bool showLocalDescription = false)
		{
			var builder = new StringBuilder();

			foreach (var set in list)
			{
				builder.AppendLine(Render(set, showLocalDescription));
			}

			return builder.ToString();
		}

		static string Render(PricingPageRateLineList set, bool showLocalDescription = false)
		{
			var builder = new StringBuilder();
			var lines = new List<string>();

			foreach (var line in set)
			{
				var entry = line.Parent;

				builder.Append(entry.TI_RateCategory);
				builder.Append("|");

				builder.Append(entry.TI_OriginLRC);
				builder.Append("->");

				if (!entry.TI_ViaLRC.IsEmpty)
				{
					builder.Append(entry.TI_ViaLRC);
					builder.Append("->");
				}

				var calculator = line.Calculator;

				builder.Append(entry.TI_DestinationLRC);
				builder.Append("|");
				builder.Append(entry.TI_Mode);
				builder.Append("|");
				builder.Append(entry.TI_RS_NKServiceLevel_NI);
				builder.Append("|");
				builder.Append(line.ChargeCode.AC_Code);
				builder.Append("|");
				builder.Append(line.TL_RateDesc);
				builder.Append("|");
				if (showLocalDescription)
				{
					builder.Append(line.TL_RateDescLocal);
					builder.Append("|");
				}
				builder.Append(line.TL_RateCalculator);
				builder.Append(",");
				builder.Append(GetValue(calculator, Calculator.Items.Operator.BAS));
				builder.Append(",");
				builder.Append(GetValue(calculator, Calculator.Items.Operator.MIN));
				builder.Append(",");
				builder.Append(GetValue(calculator, Calculator.Items.Operator.UNT));
				builder.Append(",");
				builder.Append(GetValue(calculator, Calculator.Items.Operator.MAX));
				builder.Append("|");
				builder.Append(set.GetContainerType(line));

				if (!line.Parent.TI_ContractNumber.IsEmpty)
				{
					builder.Append(" <");
					builder.Append(line.Parent.TI_ContractNumber);
					builder.Append(">");
				}

				lines.Add(builder.ToString());
				builder.Length = 0;
			}

			lines.Sort();

			builder.AppendLine();
			builder.Append("[ShowEquipmentType = ");
			builder.Append(set.ShowEquipmentType.ToString());
			builder.Append("]");

			foreach (var line in lines)
			{
				builder.AppendLine();
				builder.Append(line);
			}

			return builder.ToString();
		}

		static string GetValue(Calculator calculator, string calcOperator)
		{
			if (calculator != null && calculator[calcOperator] != null)
			{
				var value = (ZDecimal)calculator[calcOperator];
				if (value.IsValid)
				{
					return value.ToStringTrimZeros();
				}
			}

			return string.Empty;
		}

		static PricingPageRateLineFactory GetDestinationPricingPageLineSetFactory()
		{
			return new PricingPageRateLineFactory(EntryTypes.Destination);
		}

		static PricingPageRateLineFactory GetFreightPricingPageLineSetFactory()
		{
			return new PricingPageRateLineFactory(EntryTypes.Freight);
		}

		static PricingPageRateLineFactory GetOriginPricingPageLineSetFactory()
		{
			return new PricingPageRateLineFactory(EntryTypes.Origin);
		}

		#endregion
	}
}
