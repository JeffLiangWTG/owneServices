using System;
using System.Linq;
using Enterprise.BarcodeParsing.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetBarcodeParsingRulesTest : WhsSecureServiceTestCase
	{
		#region TestGetBarcodeParsingRules

		public void TestGetBarcodeParsingRulesNotGS1()
		{
			var webService = GetNewWebService();
			AssertEquals(typeof(BarcodeParsingRulesWebServiceResponse), webService.GetBarcodeParsingRules("", "", Guid.Empty, isGS1: false).GetType());

			var barcodeRulesHelper = new BarcodeParsingTestHelper(webService.Factory);
			var buyer = barcodeRulesHelper.CreateOrg("Buyer");
			var supplier = barcodeRulesHelper.CreateOrg("Supplier");
			var part = barcodeRulesHelper.CreatePart("P1", buyer);

			var ruleSet1 = barcodeRulesHelper.CreateRuleSet(buyer);
			var ruleSet2 = barcodeRulesHelper.CreateRuleSet(buyer, supplier);
			var ruleSet3 = barcodeRulesHelper.CreateRuleSet(null, supplier);
			var ruleSet4 = barcodeRulesHelper.CreateRuleSet(buyer, supplier, part);

			var nonGS1RuleWithBuyer = barcodeRulesHelper.CreateRule(ruleSet1, isGS1: false);
			nonGS1RuleWithBuyer.BRU_Terminator = "(B)";

			var nonGS1RuleWithBuyerAndSupplier = barcodeRulesHelper.CreateRule(ruleSet2, isGS1: false);
			nonGS1RuleWithBuyerAndSupplier.BRU_Terminator = "(BS)";

			var nonGS1RuleWithSupplier = barcodeRulesHelper.CreateRule(ruleSet3, isGS1: false);
			nonGS1RuleWithSupplier.BRU_Terminator = "(S)";

			var nonGS1RuleWithBuyerSupplierAndPart = barcodeRulesHelper.CreateRule(ruleSet4, isGS1: false);
			nonGS1RuleWithBuyerSupplierAndPart.BRU_Terminator = "(BSP)";

			var gS1RuleWithBuyer = barcodeRulesHelper.CreateRule(ruleSet1, isGS1: true);
			gS1RuleWithBuyer.BRU_RuleNumber = 105;
			var gS1RuleWithBuyerAndSupplier = barcodeRulesHelper.CreateRule(ruleSet2, isGS1: true);
			gS1RuleWithBuyerAndSupplier.BRU_RuleNumber = 106;
			var gS1RuleWithSupplier = barcodeRulesHelper.CreateRule(ruleSet3, isGS1: true);
			gS1RuleWithSupplier.BRU_RuleNumber = 107;
			var gS1RuleWithBuyerSupplierAndPart = barcodeRulesHelper.CreateRule(ruleSet4, isGS1: true);
			gS1RuleWithBuyerSupplierAndPart.BRU_RuleNumber = 108;

			webService.Factory.Save();

			var response1 = webService.GetBarcodeParsingRules("Buyer", "", Guid.Empty, isGS1: false);
			AssertSuccessfulResponse(response1, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(B)" }, response1.Rules.Select(r => r.Terminator));

			var response2 = webService.GetBarcodeParsingRules("", "Supplier", Guid.Empty, isGS1: false);
			AssertSuccessfulResponse(response2, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(S)" }, response2.Rules.Select(r => r.Terminator));

			var response3 = webService.GetBarcodeParsingRules("Buyer", "Supplier", Guid.Empty, isGS1: false);
			AssertSuccessfulResponse(response3, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(BS)" }, response3.Rules.Select(r => r.Terminator));

			var response4 = webService.GetBarcodeParsingRules("Buyer", "Supplier", part.PK.ToGuid(), isGS1: false);
			AssertSuccessfulResponse(response4, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(BSP)" }, response4.Rules.Select(r => r.Terminator));

			var response5 = webService.GetBarcodeParsingRules("", "", Guid.Empty, isGS1: false);
			AssertSuccessfulResponse(response5, webService);
			Assert("Should have no predefined non-GS1 rules.", !response5.Rules.Any());
		}

		#endregion

		#region TestGetBarcodeParsingRulesGS1

		public void TestGetBarcodeParsingRulesGS1()
		{
			var webService = GetNewWebService();
			AssertEquals(typeof(BarcodeParsingRulesWebServiceResponse), webService.GetBarcodeParsingRules("", "", Guid.Empty, isGS1: true).GetType());

			var barcodeRulesHelper = new BarcodeParsingTestHelper(webService.Factory);
			var buyer = barcodeRulesHelper.CreateOrg("Buyer");
			var supplier = barcodeRulesHelper.CreateOrg("Supplier");
			var part = barcodeRulesHelper.CreatePart("P1", buyer);

			var ruleSet1 = barcodeRulesHelper.CreateRuleSet(buyer);
			var ruleSet2 = barcodeRulesHelper.CreateRuleSet(buyer, supplier);
			var ruleSet3 = barcodeRulesHelper.CreateRuleSet(null, supplier);
			var ruleSet4 = barcodeRulesHelper.CreateRuleSet(buyer, supplier, part);

			var gS1RuleWithBuyer = barcodeRulesHelper.CreateRule(ruleSet1, isGS1: true);
			gS1RuleWithBuyer.BRU_RuleNumber = 105;
			var gS1RuleWithBuyerAndSupplier = barcodeRulesHelper.CreateRule(ruleSet2, isGS1: true);
			gS1RuleWithBuyerAndSupplier.BRU_RuleNumber = 106;
			var gS1RuleWithSupplier = barcodeRulesHelper.CreateRule(ruleSet3, isGS1: true);
			gS1RuleWithSupplier.BRU_RuleNumber = 107;
			var gS1RuleWithBuyerSupplierAndPart = barcodeRulesHelper.CreateRule(ruleSet4, isGS1: true);
			gS1RuleWithBuyerSupplierAndPart.BRU_RuleNumber = 108;

			var nonGS1RuleWithBuyer = barcodeRulesHelper.CreateRule(ruleSet1, isGS1: false);
			nonGS1RuleWithBuyer.BRU_Terminator = "(B)";
			var nonGS1RuleWithBuyerAndSupplier = barcodeRulesHelper.CreateRule(ruleSet2, isGS1: false);
			nonGS1RuleWithBuyerAndSupplier.BRU_Terminator = "(BS)";
			var nonGS1RuleWithSupplier = barcodeRulesHelper.CreateRule(ruleSet3, isGS1: false);
			nonGS1RuleWithSupplier.BRU_Terminator = "(S)";
			var nonGS1RuleWithBuyerSupplierAndPart = barcodeRulesHelper.CreateRule(ruleSet4, isGS1: false);
			nonGS1RuleWithBuyerSupplierAndPart.BRU_Terminator = "(BSP)";

			webService.Factory.Save();

			var response1 = webService.GetBarcodeParsingRules("Buyer", "", Guid.Empty, isGS1: true);
			AssertSuccessfulResponse(response1, webService);
			AssertContainsExactElementsInAnyOrder(new[] { (short)105 }, response1.Rules.Select(r => r.RuleNumber));

			var response2 = webService.GetBarcodeParsingRules("Buyer", "Supplier", Guid.Empty, isGS1: true);
			AssertSuccessfulResponse(response2, webService);
			AssertContainsExactElementsInAnyOrder(new[] { (short)106 }, response2.Rules.Select(r => r.RuleNumber));

			var response3 = webService.GetBarcodeParsingRules("", "Supplier", Guid.Empty, isGS1: true);
			AssertSuccessfulResponse(response3, webService);
			AssertContainsExactElementsInAnyOrder(new[] { (short)107 }, response3.Rules.Select(r => r.RuleNumber));

			var response4 = webService.GetBarcodeParsingRules("Buyer", "Supplier", part.PK.ToGuid(), isGS1: true);
			AssertSuccessfulResponse(response4, webService);
			AssertContainsExactElementsInAnyOrder(new[] { (short)108 }, response4.Rules.Select(r => r.RuleNumber));

			var response5 = webService.GetBarcodeParsingRules("", "", Guid.Empty, isGS1: true);
			AssertSuccessfulResponse(response5, webService);
			Assert("Should have some predefined GS1 rules.", response5.Rules.Any());
		}

		#endregion
	}
}
