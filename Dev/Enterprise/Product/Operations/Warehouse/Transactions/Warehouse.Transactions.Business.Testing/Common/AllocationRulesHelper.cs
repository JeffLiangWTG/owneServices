using CargoWise.EntityFramework;
using Enterprise.ProductionRules.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public static class AllocationRulesHelper
	{
		public static ProductionRuleSet MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(
			BusinessObjectFactory factory)
		{
			var allocationRuleSetQuery = new ZQuery(ProductionRuleSetSchema.PRS_IsLive, true);
			allocationRuleSetQuery.AddToFilter(ProductionRuleSetSchema.PRS_Context, "PWA");

			var existingRuleSet = factory.LoadTop1<ProductionRuleSet>(allocationRuleSetQuery);

			if (existingRuleSet != null)
			{
				existingRuleSet.PRS_IsLive = false;
			}

			var newRuleSet = factory.New<ProductionRuleSet>();
			newRuleSet.PRS_IsLive = true;
			newRuleSet.PRS_Context = "PWA";
			newRuleSet.PRS_Name = "TEST ALLOCATION RULESET";
			newRuleSet.PRS_Description = "TEST ALLOCATION RULESET";
			return newRuleSet;
		}

		public static void AddPickFaceRules(ProductionRuleSet ruleSet, short priority)
		{
			var pickFaceRule = ruleSet.Factory.New<ProductionRule>();
			pickFaceRule.PRL_Name = "PICK FACE";
			pickFaceRule.PRL_Description = "PICK FACE";
			pickFaceRule.PRL_Priority = priority;
			pickFaceRule.PRL_PRS_RuleSet = ruleSet.PK;
			pickFaceRule.PRL_RuleDefinition = $@"
{{
  ""action"": {{
    ""$type"": ""AllocateStockActionState"",
    ""allocateCases"": true,
    ""allocatePallets"": true,
    ""allocateSplitCase"": true,
    ""canBreakUOMs"": true,
    ""conditions"": [
      {{
        ""fieldPath"": ""Location.IsFixedPickFace"",
        ""operation"": ""equals"",
        ""value"": true
      }}
    ],
    ""sortByCriteria"": []
  }},
  ""conditions"": []
}}";

			var dynamicPickFaceRule = ruleSet.Factory.New<ProductionRule>();
			dynamicPickFaceRule.PRL_Name = "DYNAMIC";
			dynamicPickFaceRule.PRL_Description = "DYNAMIC";
			dynamicPickFaceRule.PRL_Priority = priority;
			dynamicPickFaceRule.PRL_PRS_RuleSet = ruleSet.PK;
			dynamicPickFaceRule.PRL_RuleDefinition = $@"
{{
  ""action"": {{
    ""$type"": ""AllocateStockActionState"",
    ""allocateCases"": true,
    ""allocatePallets"": true,
    ""allocateSplitCase"": true,
    ""canBreakUOMs"": true,
    ""conditions"": [
      {{
        ""fieldPath"": ""Location.IsDynamicPickFace"",
        ""operation"": ""equals"",
        ""value"": true
      }}
    ],
    ""sortByCriteria"": []
  }},
  ""conditions"": []
}}";
		}

		public static void AddFullPalletRule(ProductionRuleSet ruleSet, short priority)
		{
			var palletRule = ruleSet.Factory.New<ProductionRule>();
			palletRule.PRL_Name = "PALLET";
			palletRule.PRL_Description = "PALLET";
			palletRule.PRL_Priority = priority;
			palletRule.PRL_PRS_RuleSet = ruleSet.PK;
			palletRule.PRL_RuleDefinition = $@"
{{
  ""action"": {{
    ""$type"": ""AllocateStockActionState"",
    ""allocateCases"": false,
    ""allocatePallets"": true,
    ""allocateSplitCase"": false,
    ""canBreakUOMs"": false,
    ""conditions"": [],
    ""sortByCriteria"": []
  }},
  ""conditions"": []
}}";
		}

		public static void AddFifoRule(ProductionRuleSet ruleSet, short priority,
			bool preventPickingPickFacesFromBulk = false)
		{
			var preventBulk = !preventPickingPickFacesFromBulk
				? string.Empty
				: $@",{{""fieldPath"":""Product.HasPickFaces"",""operation"":""equals"",""value"":false}}";
			var fifoRule = ruleSet.Factory.New<ProductionRule>();
			fifoRule.PRL_Name = "FIFO";
			fifoRule.PRL_Description = "FIFO";
			fifoRule.PRL_Priority = priority;
			fifoRule.PRL_PRS_RuleSet = ruleSet.PK;
			fifoRule.PRL_RuleDefinition = $@"
{{
  ""action"": {{
    ""$type"": ""AllocateStockActionState"",
    ""allocateCases"": true,
    ""allocatePallets"": true,
    ""allocateSplitCase"": true,
    ""canBreakUOMs"": true,
    ""conditions"": [],
    ""sortByCriteria"": []
  }},
  ""conditions"":[{{""fieldPath"":""Product.IsDynamic"",""operation"":""equals"",""value"":false}}{preventBulk}]
}}";
		}

		public static void AddBrokenPalletsRule(ProductionRuleSet ruleSet, short priority)
		{
			var fifoRule = ruleSet.Factory.New<ProductionRule>();
			fifoRule.PRL_Name = "BROKEN PALLETS";
			fifoRule.PRL_Description = "BROKEN PALLETS";
			fifoRule.PRL_Priority = priority;
			fifoRule.PRL_PRS_RuleSet = ruleSet.PK;
			fifoRule.PRL_RuleDefinition = $@"
{{
  ""action"": {{
    ""$type"": ""AllocateStockActionState"",
    ""allocateCases"": true,
    ""allocatePallets"": false,
    ""allocateSplitCase"": true,
    ""canBreakUOMs"": false,
    ""conditions"": [
      {{
        ""fieldPath"": ""IsPalletOverflow"",
        ""operation"": ""equals"",
        ""value"": false
      }}
    ],
    ""sortByCriteria"": []
  }},
  ""conditions"":[{{""fieldPath"":""Product.HasPalletDefined"",""operation"":""equals"",""value"":true}}]
}}";
		}
	}
}
