using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BarcodeParsing.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetBarcodeValidationRulesTest : WhsSecureServiceTestCase
	{
		public void TestResponseType()
		{
			var webService = GetNewWebService();
			AssertEquals(typeof(BarcodeValidationRulesWebServiceResponse), webService.GetBarcodeValidationRules(string.Empty, string.Empty, Guid.Empty).GetType());
		}

		public void TestGetBarcodeValidationRules_None()
		{
			var webService = GetNewWebService();
			CreateTestData();

			var response = webService.GetBarcodeValidationRules(string.Empty, string.Empty, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(N)" }, response.Rules.Select(r => r.Prefix));
		}

		public void TestGetBarcodeValidationRules_Supplier()
		{
			var webService = GetNewWebService();
			CreateTestData();

			var response = webService.GetBarcodeValidationRules(string.Empty, "Supplier", Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(S)" }, response.Rules.Select(r => r.Prefix));
		}

		public void TestGetBarcodeValidationRules_Buyer()
		{
			var webService = GetNewWebService();
			CreateTestData();

			var response = webService.GetBarcodeValidationRules("Buyer", string.Empty, Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(B)" }, response.Rules.Select(r => r.Prefix));
		}

		public void TestGetBarcodeValidationRules_BuyerPart()
		{
			var webService = GetNewWebService();
			var partPK = CreateTestData();

			var response = webService.GetBarcodeValidationRules("Buyer", string.Empty, partPK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(BP)" }, response.Rules.Select(r => r.Prefix));
		}

		public void TestGetBarcodeValidationRules_BuyerSupplier()
		{
			var webService = GetNewWebService();
			CreateTestData();

			var response = webService.GetBarcodeValidationRules("Buyer", "Supplier", Guid.Empty);
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(BS)" }, response.Rules.Select(r => r.Prefix));
		}

		public void TestGetBarcodeValidationRules_BuyerSupplierPart()
		{
			var webService = GetNewWebService();
			var partPK = CreateTestData();

			var response = webService.GetBarcodeValidationRules("Buyer", "Supplier", partPK.ToGuid());
			AssertSuccessfulResponse(response, webService);
			AssertContainsExactElementsInAnyOrder(new[] { "(BSP)" }, response.Rules.Select(r => r.Prefix));
		}

		ZGuid CreateTestData()
		{
			var barcodeRulesHelper = new BarcodeParsingTestHelper(Helper.Factory);
			var buyer = barcodeRulesHelper.CreateOrg("Buyer");
			var supplier = barcodeRulesHelper.CreateOrg("Supplier");
			var part = barcodeRulesHelper.CreatePart("P1", buyer);

			var ruleSet1 = barcodeRulesHelper.CreateRuleSet();
			var ruleSet2 = barcodeRulesHelper.CreateRuleSet(null, supplier);
			var ruleSet3 = barcodeRulesHelper.CreateRuleSet(buyer);
			var ruleSet4 = barcodeRulesHelper.CreateRuleSet(buyer, null, part);
			var ruleSet5 = barcodeRulesHelper.CreateRuleSet(buyer, supplier);
			var ruleSet6 = barcodeRulesHelper.CreateRuleSet(buyer, supplier, part);

			var ruleNone = barcodeRulesHelper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			ruleNone.BVR_Prefix = "(N)";

			var ruleWithSupplier = barcodeRulesHelper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			ruleWithSupplier.BVR_Prefix = "(S)";

			var ruleWithBuyer = barcodeRulesHelper.CreateValidationRule(ruleSet3, DummyTargetFields.Codes.TargetField1);
			ruleWithBuyer.BVR_Prefix = "(B)";

			var ruleWithBuyerAndPart = barcodeRulesHelper.CreateValidationRule(ruleSet4, DummyTargetFields.Codes.TargetField1);
			ruleWithBuyerAndPart.BVR_Prefix = "(BP)";

			var ruleWithBuyerAndSupplier = barcodeRulesHelper.CreateValidationRule(ruleSet5, DummyTargetFields.Codes.TargetField1);
			ruleWithBuyerAndSupplier.BVR_Prefix = "(BS)";

			var ruleWithBuyerSupplierAndPart = barcodeRulesHelper.CreateValidationRule(ruleSet6, DummyTargetFields.Codes.TargetField1);
			ruleWithBuyerSupplierAndPart.BVR_Prefix = "(BSP)";

			Helper.Factory.Save();
			return part.PK;
		}
	}
}
