using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	class UniversalValidationRuleStoreTest : TestCaseWithFactory
	{
		public void TestGetActiveRulesByCode()
		{
			var helper = new RuleHelper(Factory);
			var ruleSet1 = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			helper.AddRule(ruleSet1, "<ux.CarrierContractNumber> = \"12345\"");
			var inactiveRule = helper.AddRule(ruleSet1, "<ux.CarrierContractNumber> = \"99999\"");
			inactiveRule.VR_IsActive = false;

			var ruleSet2 = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			helper.AddRule(ruleSet2, "<ux.ActualChargeable> = 20");
			helper.AddRule(ruleSet2, "<ux.ActualChargeable> = 21");

			var ruleSet3WithNoRules = helper.CreateRuleSet(DataContextType.ForwardingShipment);

			var inactiveRuleSet = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			inactiveRuleSet.VRS_IsActive = false;

			Factory.Save();

			var ruleStore = new UniversalValidationRuleStore();
			var codesToLoad = new[] { (string)ruleSet1.VRS_Code, (string)ruleSet2.VRS_Code, "R999" };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var actualRuleSets = ruleStore.GetActiveRulesByCode(factory2, nameof(DataContextType.ForwardingShipment), codesToLoad)
				.OrderBy(x => x.Code)
				.ToList();
			var actualRuleSets2 = ruleStore.GetActiveRulesByCode(factory3, nameof(DataContextType.ForwardingShipment), codesToLoad)
				.OrderBy(x => x.Code)
				.ToList();

			var actualCodes = string.Join(", ", actualRuleSets.Select(x => x.Code).OrderBy(x => x));
			var actual2Codes = string.Join(", ", actualRuleSets2.Select(x => x.Code).OrderBy(x => x));

			var expectedCodes = string.Join(", ", new[] { ruleSet1.VRS_Code, ruleSet2.VRS_Code });
			AssertEquals("codes, doesn't include inactive", expectedCodes, actualCodes);
			AssertEquals("codes again", expectedCodes, actual2Codes);
			AssertEquals("only active rule is loaded", "1 - Active", string.Join(", ", actualRuleSets[0].ActiveRules
				.Select(x => x.Sequence.ToString() + " - " + (x.IsActive ? "Active" : "Inactive"))));

			// 1 hit per table for first call
			AssertDbHits(new Dictionary<string, int>
			{
				{ UniversalValidationRuleSetSchema.Constants.TableName, 1 },
				{ UniversalValidationRuleSchema.Constants.TableName, 1 },
			}, factory2);

			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ UniversalValidationRuleSetSchema.Constants.TableName, 0 },
				{ UniversalValidationRuleSchema.Constants.TableName, 0 },
			}, factory3);
		}

		public void TestRulesByCodeMustMatchDataContext()
		{
			var helper = new RuleHelper(Factory);
			var ruleSet1 = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			helper.AddRule(ruleSet1, "<ux.CarrierContractNumber> = \"12345\"");

			Factory.Save();

			var ruleStore = new UniversalValidationRuleStore();
			var codesToLoad = new[] { (string)ruleSet1.VRS_Code };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var rulesFromCache = ruleStore.GetActiveRulesByCode(factory2, nameof(DataContextType.ForwardingConsol), codesToLoad)
				.OrderBy(x => x.Code)
				.ToList();

			AssertEquals("Should not match rule if code does not match", 0, rulesFromCache.Count);
		}

		public void TestGetActiveRulesByDataContext()
		{
			var helper = new RuleHelper(Factory);
			var ruleSet1 = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			helper.AddRule(ruleSet1, "<ux.CarrierContractNumber> = \"12345\"");

			var ruleSet2 = helper.CreateRuleSet(DataContextType.ForwardingConsol);
			helper.AddRule(ruleSet2, "<ux.ActualChargeable> = 20");

			var ruleSet3WithNoRules = helper.CreateRuleSet(DataContextType.ForwardingShipment);

			var inactiveRuleSet = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			inactiveRuleSet.VRS_IsActive = false;

			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var ruleStore = new UniversalValidationRuleStore();
			var actual = ruleStore.GetActiveRulesByDataContext(factory2, nameof(DataContextType.ForwardingShipment));
			var actual2 = ruleStore.GetActiveRulesByDataContext(factory3, nameof(DataContextType.ForwardingShipment));
			var actualCodes = string.Join(", ", actual.Select(x => x.Code).OrderBy(x => x));
			var actual2Codes = string.Join(", ", actual2.Select(x => x.Code).OrderBy(x => x));
			var expectedCodes = string.Join(", ", new[] { ruleSet1.VRS_Code, ruleSet3WithNoRules.VRS_Code });
			AssertEquals("codes", expectedCodes, actualCodes);
			AssertEquals("codes again", expectedCodes, actual2Codes);
			// 1 hit per table for first call
			AssertDbHits(new Dictionary<string, int>
			{
				{ UniversalValidationRuleSetSchema.Constants.TableName, 1 },
				{ UniversalValidationRuleSchema.Constants.TableName, 1 },
			}, factory2);

			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ UniversalValidationRuleSetSchema.Constants.TableName, 0 },
				{ UniversalValidationRuleSchema.Constants.TableName, 0 },
			}, factory3);
		}

		public void TestGetActiveRulesByCode_CacheExpiry()
		{
			eAdaptorRegistry.Instance.ValidationRuleMacroCacheInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var helper = new RuleHelper(Factory);
			var ruleSet1 = helper.CreateRuleSet(DataContextType.ForwardingShipment);
			ruleSet1.VRS_Name = "Name1";
			var rule1 = helper.AddRule(ruleSet1, "<ux.CarrierContractNumber> = \"12345\"");

			Factory.Save();

			var codesToLoad = new[] { (string)ruleSet1.VRS_Code };
			var utcNow = DateTime.UtcNow;
			var ruleStore = new UniversalValidationRuleStore();
			var actualRuleSets = ruleStore.GetActiveRulesByCode(new BusinessObjectFactory() { RefreshEnabled = false }, nameof(DataContextType.ForwardingShipment), codesToLoad)
				.OrderBy(x => x.Code)
				.ToList();

			ruleSet1.VRS_Name = "Name2";
			Factory.Save();

			var actualRuleSets3 = ruleStore.GetActiveRulesByCode(new BusinessObjectFactory() { RefreshEnabled = false }, nameof(DataContextType.ForwardingShipment), codesToLoad)
				.OrderBy(x => x.Code)
				.ToList();

			AssertEquals("calling after cache expiry gets new name", "Name2", actualRuleSets3[0].Name);
		}

		public void TestGetActiveRulesByCode_EmptyResult()
		{
			var ruleStore = new UniversalValidationRuleStore();
			var codetoLoad = new[] { "R001" };
			var actual = ruleStore.GetActiveRulesByCode(Factory, nameof(DataContextType.ForwardingShipment), codetoLoad);
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var actualAgain = ruleStore.GetActiveRulesByCode(factory2, nameof(DataContextType.ForwardingShipment), codetoLoad);

			AssertEquals(0, actual.Count());
			AssertEquals(0, actualAgain.Count());

			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ UniversalValidationRuleSetSchema.Constants.TableName, 0 },
				{ UniversalValidationRuleSchema.Constants.TableName, 0 },
			}, factory2);
		}

		public void TestGetActiveRulesByCode_EmptyCodeList()
		{
			var ruleStore = new UniversalValidationRuleStore();
			var actual = ruleStore.GetActiveRulesByCode(Factory, nameof(DataContextType.ForwardingShipment), Enumerable.Empty<string>());

			AssertEquals(0, actual.Count());

			// no hits
			AssertDbHits(new Dictionary<string, int>
			{
				{ UniversalValidationRuleSetSchema.Constants.TableName, 0 },
				{ UniversalValidationRuleSchema.Constants.TableName, 0 },
			}, Factory);
		}

		public void TestGetActiveRulesByDataContext_EmptyResult()
		{
			var ruleStore = new UniversalValidationRuleStore();
			var actual = ruleStore.GetActiveRulesByDataContext(Factory, nameof(DataContextType.ForwardingBooking));
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var actualAgain = ruleStore.GetActiveRulesByDataContext(factory2, nameof(DataContextType.ForwardingBooking));

			AssertEquals(0, actual.Count());
			AssertEquals(0, actualAgain.Count());

			// no hits for second call since cached
			AssertDbHits(new Dictionary<string, int>
			{
				{ UniversalValidationRuleSetSchema.Constants.TableName, 0 },
				{ UniversalValidationRuleSchema.Constants.TableName, 0 },
			}, factory2);
		}
	}
}
