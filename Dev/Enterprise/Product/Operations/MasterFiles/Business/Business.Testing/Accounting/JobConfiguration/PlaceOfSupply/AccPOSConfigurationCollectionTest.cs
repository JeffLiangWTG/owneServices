using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(AccPOSConfigurationCollection))]
	sealed class AccPOSConfigurationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNullLevelCollection()
		{
			// A Null Level collection is a read-only empty placeholder for global charge codes.
			CreateAndSaveStandardTestObjects();

			var collection = new AccPOSConfigurationCollection(Factory);
			AssertEquals(AccPOSConfigurationLevel.Null, collection.Level);
			Assert("A Null Level collection is always read only", collection.ReadOnly);

			collection.Load();
			Assert(collection.IsLoaded);
			AssertEquals("A Null Level collection will never load any business objects", 0, collection.Count);

			AssertExceptionThrown<NotSupportedException>("A Null Level collection throws when new children are created", () => collection.AddNew());
		}

		public void TestCompanyLevelCollection_ContainsCompany_AndNotGroup_AndNotChargeCode()
		{
			CreateAndSaveStandardTestObjects();

			var collection = new AccPOSConfigurationCollection(OtherCompany);
			AssertEquals(AccPOSConfigurationLevel.Company, collection.Level);

			collection.Load();
			Assert(collection.IsLoaded);
			AssertEquals(1, collection.Count);

			var cfg = (AccPOSConfiguration)collection.FindByPK(OtherCompanyConfigPK);
			AssertNotNull(cfg);
			AssertEquals(AccPOSConfigurationLevel.Company, cfg.Level);

			AssertSequencesEqual(new[] { cfg }, collection);
		}

		public void TestAddNew_ToCompanyLevelCollection()
		{
			var aCompany = Factory.New<GlbCompany>();
			aCompany.GC_Code = "C01";

			var collection = new AccPOSConfigurationCollection(aCompany);
			AssertEquals(AccPOSConfigurationLevel.Company, collection.Level);

			var config = collection.AddNew();
			AssertEquals(1, collection.Count);
			AssertEquals("Collection and Bizo Level should be the same", collection.Level, config.Level);
			AssertEquals(string.Empty, config.PSC_ParentTableCode);
			AssertEquals("Collection Company PK should be taken from constructor, not Current Company", aCompany.PK, config.PSC_GC);
		}

		public void TestGroupLevelCollection_ContainsCompany_AndGroup_AndNotChargeCode()
		{
			CreateAndSaveStandardTestObjects();

			var collection = new AccPOSConfigurationCollection(OtherCompanyChargeCodeGroup);
			AssertEquals(AccPOSConfigurationLevel.ChargeCodeGroup, collection.Level);

			collection.Load();
			Assert(collection.IsLoaded);
			AssertEquals(2, collection.Count);

			var companyCfg = (AccPOSConfiguration)collection.FindByPK(OtherCompanyConfigPK);
			AssertNotNull(companyCfg);
			AssertEquals(AccPOSConfigurationLevel.Company, companyCfg.Level);

			var groupCfg = (AccPOSConfiguration)collection.FindByPK(OtherCompanyChargeGroupConfigPK);
			AssertNotNull(groupCfg);
			AssertEquals(AccPOSConfigurationLevel.ChargeCodeGroup, groupCfg.Level);

			AssertSequencesEqual(new[] { companyCfg, groupCfg }, collection);
		}

		public void TestAddNew_ToGroupLevelCollection()
		{
			var aCompany = Factory.New<GlbCompany>();
			aCompany.GC_Code = "C01";
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			group.GRO_GC = aCompany.PK;

			var collection = new AccPOSConfigurationCollection(group);
			AssertEquals(AccPOSConfigurationLevel.ChargeCodeGroup, collection.Level);

			var config = collection.AddNew();
			AssertEquals(1, collection.Count);
			AssertEquals(collection.Level, config.Level);
			AssertEquals(AccPOSChargeCodeGroupViewSchema.Constants.Prefix, config.PSC_ParentTableCode);
			AssertEquals(group.PK, config.PSC_ParentId);
			AssertEquals("Collection Company PK should be taken from constructor, not Current Company", aCompany.PK, config.PSC_GC);
		}

		public void TestChargeCodeLevelCollection_ContainsCompany_AndGroup_AndChargeCode()
		{
			CreateAndSaveStandardTestObjects();

			var collection = new AccPOSConfigurationCollection(OtherCompanyChargeCode);
			AssertEquals(AccPOSConfigurationLevel.ChargeCode, collection.Level);

			collection.Load();
			Assert(collection.IsLoaded);
			AssertEquals(3, collection.Count);

			var companyCfg = (AccPOSConfiguration)collection.FindByPK(OtherCompanyConfigPK);
			AssertNotNull(companyCfg);
			AssertEquals(AccPOSConfigurationLevel.Company, companyCfg.Level);

			var groupCfg = (AccPOSConfiguration)collection.FindByPK(OtherCompanyChargeGroupConfigPK);
			AssertNotNull(groupCfg);
			AssertEquals(AccPOSConfigurationLevel.ChargeCodeGroup, groupCfg.Level);

			var chargeCodeCfg = (AccPOSConfiguration)collection.FindByPK(OtherCompanyChargeCodeConfigPK);
			AssertNotNull(chargeCodeCfg);
			AssertEquals(AccPOSConfigurationLevel.ChargeCode, chargeCodeCfg.Level);

			AssertSequencesEqual(new[] { companyCfg, groupCfg, chargeCodeCfg }, collection);
		}

		public void TestAddNew_ToChargeCodeLevelCollection()
		{
			var aCompany = Factory.New<GlbCompany>();
			aCompany.GC_Code = "C01";
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_GC = aCompany.PK;

			var collection = new AccPOSConfigurationCollection(chargeCode);
			AssertEquals(AccPOSConfigurationLevel.ChargeCode, collection.Level);

			var config = collection.AddNew();
			AssertEquals(1, collection.Count);
			AssertEquals(collection.Level, config.Level);
			AssertEquals(AccChargeCodeSchema.Constants.Prefix, config.PSC_ParentTableCode);
			AssertEquals(chargeCode.PK, config.PSC_ParentId);
			AssertEquals("Collection Company PK should be taken from constructor, not Current Company", aCompany.PK, config.PSC_GC);
		}

		public void TestSecurityCheckpointsAreAppliedByLevel()
		{
			var aCompany = Factory.New<GlbCompany>();
			var chargeGroup = Factory.New<AccPOSChargeCodeGroup>();
			var chargeCode = Factory.New<AccChargeCode>();
			var companyCollection = new AccPOSConfigurationCollection(aCompany);
			var groupCollection = new AccPOSConfigurationCollection(chargeGroup);
			var chargeCodeCollection = new AccPOSConfigurationCollection(chargeCode);

			Env.Security.CompaniesModifyPlaceOfSupplyConfiguration.IsAllowed = true;
			Env.Security.POSChargeCodeGroupsModify.IsAllowed = true;
			Env.Security.ChargeCodesPlaceOfSupplyConfiguration.IsAllowed = true;
			Assert(!companyCollection.ReadOnly);
			Assert(!groupCollection.ReadOnly);
			Assert(!chargeCodeCollection.ReadOnly);

			Env.Security.CompaniesModifyPlaceOfSupplyConfiguration.IsAllowed = false;
			Env.Security.POSChargeCodeGroupsModify.IsAllowed = true;
			Env.Security.ChargeCodesPlaceOfSupplyConfiguration.IsAllowed = true;
			Assert(companyCollection.ReadOnly);
			Assert(!groupCollection.ReadOnly);
			Assert(!chargeCodeCollection.ReadOnly);

			Env.Security.CompaniesModifyPlaceOfSupplyConfiguration.IsAllowed = true;
			Env.Security.POSChargeCodeGroupsModify.IsAllowed = false;
			Env.Security.ChargeCodesPlaceOfSupplyConfiguration.IsAllowed = true;
			Assert(!companyCollection.ReadOnly);
			Assert(groupCollection.ReadOnly);
			Assert(!chargeCodeCollection.ReadOnly);

			Env.Security.CompaniesModifyPlaceOfSupplyConfiguration.IsAllowed = true;
			Env.Security.POSChargeCodeGroupsModify.IsAllowed = true;
			Env.Security.ChargeCodesPlaceOfSupplyConfiguration.IsAllowed = false;
			Assert(!companyCollection.ReadOnly);
			Assert(!groupCollection.ReadOnly);
			Assert(chargeCodeCollection.ReadOnly);
		}

		public void TestSortingByPriority()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TSTCOD";
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			group.GRO_Code = "TSTGRP";

			var expectedMostGenericToMostSpecific = new List<AccPOSConfiguration>();
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("ALL", "ALL", "ALL"));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "ALL", "ALL"));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "ALL"));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB"));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA"));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON"));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE"));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", supplyType: "LOC"));

			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("ALL", "ALL", "ALL", group: group));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "ALL", "ALL", group: group));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "ALL", group: group));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB", group: group));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", group: group));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", group: group));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", group: group));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", supplyType: "LOC", group: group));

			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("ALL", "ALL", "ALL", chargeCode: chargeCode));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "ALL", "ALL", chargeCode: chargeCode));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "ALL", chargeCode: chargeCode));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB", chargeCode: chargeCode));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", chargeCode: chargeCode));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", chargeCode: chargeCode));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", chargeCode: chargeCode));
			expectedMostGenericToMostSpecific.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", supplyType: "LOC", chargeCode: chargeCode));

			var expectedMostSpecificToMostGeneric = expectedMostGenericToMostSpecific.AsEnumerable().Reverse().ToList();

			var collectionToSort = new AccPOSConfigurationCollection(chargeCode);
			collectionToSort.AddRange(expectedMostGenericToMostSpecific.OrderBy(x => x.PK));

			collectionToSort.Sort<AccPOSConfiguration>(AccPOSConfiguration.CompareByRulePriorityGenericToSpecific);
			AssertSequencesEqual(expectedMostGenericToMostSpecific, collectionToSort);
			collectionToSort.Sort<AccPOSConfiguration>(AccPOSConfiguration.CompareByRulePrioritySpecificToGeneric);
			AssertSequencesEqual(expectedMostSpecificToMostGeneric, collectionToSort);
		}

		public void TestLookupBestMatch_ConfigurationWithouIncoTermTransportModeSupplyType()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TSTCOD";
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			group.GRO_Code = "TSTGRP";

			var ruleCollection = new AccPOSConfigurationCollection(chargeCode);
			ruleCollection.Add(CreateConfigurationRule("ALL", "ALL", "ALL"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "ALL", "ALL"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", branchCode: "BNE"));

			ruleCollection.Add(CreateConfigurationRule("ALL", "ALL", "ALL", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "ALL", "ALL", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", branchCode: "BNE", group: group));

			ruleCollection.Add(CreateConfigurationRule("ALL", "ALL", "ALL", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "ALL", "ALL", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", taxReg: "NON", branchCode: "BNE", chargeCode: chargeCode));

			assertRuleMatching("ChargeCode:TSTCOD", chargeCode.PK, group.PK, ZGuid.Empty);

			assertRuleMatching("ChargeCodeGroup:TSTGRP", ZGuid.NewZGuid(), group.PK, ZGuid.Empty);

			assertRuleMatching("Company:EDI", ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.Empty);

			assertRuleMatching("Company:EDI", ZGuid.NewZGuid(), ZGuid.Empty);

			void assertRuleMatching(string expectedLevel, params ZGuid[] parentIds)
			{
				var matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV||IMP|ALL|NON|BNE|", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "SEA", "NON", "BNE", "LOA");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV||IMP|ALL|NON|BNE|", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FC1", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV||IMP|ALL|NON|BNE|", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "SEA", "NON", "SYD", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV||IMP|ALL|NON||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "SEA", "FRO", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV||IMP|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "EXP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV||ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "COS", "FOB", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|ALL||ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "FCN", "REV", "", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|ALL|ALL||ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "FCN", "REV", "", "IMP", "SEA", "NON", null, null);
				AssertEquals($"EDI|{expectedLevel}|ALL|ALL||ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "QSH", "REV", "", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|ALL|ALL||ALL|ALL|||", matchedRule.LookupKey);
			}
		}

		public void TestLookupBestMatch()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TSTCOD";
			var group = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			group.GRO_Code = "TSTGRP";

			var ruleCollection = new AccPOSConfigurationCollection(chargeCode);

			ruleCollection.Add(CreateConfigurationRule("ALL", "ALL", "ALL"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "ALL", "ALL"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE"));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", supplyType: "LOC"));

			ruleCollection.Add(CreateConfigurationRule("ALL", "ALL", "ALL", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "ALL", "ALL", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", group: group));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", supplyType: "LOC", group: group));

			ruleCollection.Add(CreateConfigurationRule("ALL", "ALL", "ALL", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "ALL", "ALL", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "ALL", incoTerm: "FOB", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", chargeCode: chargeCode));
			ruleCollection.Add(CreateConfigurationRule("SHP", "REV", "IMP", incoTerm: "FOB", transportMode: "SEA", taxReg: "NON", branchCode: "BNE", supplyType: "LOC", chargeCode: chargeCode));

			assertRuleMatching("ChargeCode:TSTCOD", chargeCode.PK, group.PK, ZGuid.Empty);

			assertRuleMatching("ChargeCodeGroup:TSTGRP", ZGuid.NewZGuid(), group.PK, ZGuid.Empty);

			assertRuleMatching("Company:EDI", ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.Empty);

			assertRuleMatching("Company:EDI", ZGuid.NewZGuid(), ZGuid.Empty);

			void assertRuleMatching(string expectedLevel, params ZGuid[] parentIds)
			{
				var matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV|FOB|IMP|SEA|NON|BNE|LOC", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "SEA", "NON", "BNE", "LOA");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV|FOB|IMP|SEA|NON|BNE|", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FC1", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV||ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "SEA", "NON", "SYD", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV|FOB|IMP|SEA|NON||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "SEA", "FRO", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV|FOB|IMP|SEA|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "IMP", "AIR", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV|FOB|ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "REV", "FOB", "EXP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|REV|FOB|ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "SHP", "COS", "FOB", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|SHP|ALL||ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "FCN", "REV", "", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|ALL|ALL||ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "FCN", "REV", "", "IMP", "SEA", "NON", null, null);
				AssertEquals($"EDI|{expectedLevel}|ALL|ALL||ALL|ALL|||", matchedRule.LookupKey);
				matchedRule = ruleCollection.LookupBestMatch(parentIds, "QSH", "REV", "", "IMP", "SEA", "NON", "BNE", "LOC");
				AssertEquals($"EDI|{expectedLevel}|ALL|ALL||ALL|ALL|||", matchedRule.LookupKey);
			}

			var emptyRuleCollection = new AccPOSConfigurationCollection(chargeCode);
			var nullRule = emptyRuleCollection.LookupBestMatch(new[] { ZGuid.NewZGuid(), ZGuid.Empty }, "SHP", "REV", "FOB", "IMP", "SEA", "NON", "BNE", "LOC");
			AssertNull(nullRule);
		}

		public void TestLogsWithChargeCode_WhenAddOrDelete()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var logs = chargeCode.Logs.GetAllLogs();
			AssertEquals(0, logs.Count);

			Factory.Save();
			AssertEquals(0, logs.Count);

			var placeOfSupplyConfiguration = GetNewPOSConfigurationWithAllPropertiesPreset(chargeCode);
			Factory.Save();
			AssertEquals(1, logs.Count);
			AssertEquals(placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.AddedLogStatus), logs[0].SL_Reference);

			var deletedExpected = placeOfSupplyConfiguration.GetLogByStatus(BusinessObjectLoggingExtension.DeleteLogStatus);
			chargeCode.PlaceOfSupplyConfigurations.RemoveAndDeleteAll();
			AssertEquals(2, logs.Count);
			AssertEquals(deletedExpected, logs[1].SL_Reference);
		}

		AccPOSConfiguration GetNewPOSConfigurationWithAllPropertiesPreset(AccChargeCode chargeCode)
		{
			var placeOfSupplyConfiguration = chargeCode.PlaceOfSupplyConfigurations.AddNew();
			placeOfSupplyConfiguration.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			placeOfSupplyConfiguration.PSC_JobType = "SHP";
			placeOfSupplyConfiguration.PSC_ChargeType = "COS";
			placeOfSupplyConfiguration.PSC_IncoTerm = "FOB";
			placeOfSupplyConfiguration.PSC_ServiceDirection = "EXP";
			placeOfSupplyConfiguration.PSC_TransportMode = "SEA";
			placeOfSupplyConfiguration.PSC_TaxRegistrationType = "NON";
			placeOfSupplyConfiguration.PSC_PlaceOfSupplyRule = "SUP";
			placeOfSupplyConfiguration.PSC_NK_Branch = "SYD";
			placeOfSupplyConfiguration.PSC_SupplyType = "LOC";
			return placeOfSupplyConfiguration;
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccPOSConfigurationCollection(GlbCompany.CurrentCompany);
		}

		GlbCompany OtherCompany;
		AccPOSChargeCodeGroup ChargeCodeGroup;
		AccPOSChargeCodeGroup OtherCompanyChargeCodeGroup;
		AccChargeCode ChargeCode;
		AccChargeCode OtherCompanyChargeCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "The mehod CreatePOSConfig for assigning field has side effects (creating database records), we should neithor remove the assign method nor remove the field. ")]
		ZGuid ConfigPK;
		ZGuid OtherCompanyConfigPK;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "The mehod CreatePOSConfig for assigning field has side effects (creating database records), we should neithor remove the assign method nor remove the field. ")]
		ZGuid ChargeGroupConfigPK;
		ZGuid OtherCompanyChargeGroupConfigPK;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "The mehod CreatePOSConfig for assigning field has side effects (creating database records), we should neithor remove the assign method nor remove the field. ")]
		ZGuid ChargeCodeConfigPK;
		ZGuid OtherCompanyChargeCodeConfigPK;

		void CreateAndSaveStandardTestObjects()
		{
			TestCaseHelper.ClearTable(AccPOSConfigurationViewSchema.Constants.TableName);

			OtherCompany = Factory.NewWithValidTestData<GlbCompany>();
			OtherCompany.GC_Code = "C01";

			ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			ChargeCode.AC_Code = "CHGCD1";
			ChargeCode.AC_GC = Env.CurrentCompany.PK;
			OtherCompanyChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			OtherCompanyChargeCode.AC_Code = "CHGCD2";
			OtherCompanyChargeCode.AC_GC = OtherCompany.PK;

			ChargeCodeGroup = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			ChargeCodeGroup.GRO_Code = "GRP1";
			ChargeCodeGroup.GRO_GC = Env.CurrentCompany.PK;
			OtherCompanyChargeCodeGroup = Factory.NewWithValidTestData<AccPOSChargeCodeGroup>();
			OtherCompanyChargeCodeGroup.GRO_Code = "GRP2";
			OtherCompanyChargeCodeGroup.GRO_GC = OtherCompany.PK;

			var pivot = ChargeCodeGroup.ChargeCodePivots.AddNew();
			pivot.GRP_GroupType = "POS";
			pivot.GRP_MemberTableCode = AccChargeCodeSchema.Constants.Prefix;
			pivot.GRP_MemberID = ChargeCode.PK;
			var pivot2 = OtherCompanyChargeCodeGroup.ChargeCodePivots.AddNew();
			pivot2.GRP_GroupType = "POS";
			pivot2.GRP_MemberTableCode = AccChargeCodeSchema.Constants.Prefix;
			pivot2.GRP_MemberID = OtherCompanyChargeCode.PK;

			Factory.Save();

			ConfigPK = CreatePOSConfig(Env.CurrentCompany.PK, ZString.Empty, ZGuid.Empty).PK;
			OtherCompanyConfigPK = CreatePOSConfig(OtherCompany.PK, ZString.Empty, ZGuid.Empty).PK;

			ChargeGroupConfigPK = CreatePOSConfig(Env.CurrentCompany.PK, AccPOSChargeCodeGroupViewSchema.Constants.Prefix, ChargeCodeGroup.PK).PK;
			OtherCompanyChargeGroupConfigPK = CreatePOSConfig(OtherCompany.PK, AccPOSChargeCodeGroupViewSchema.Constants.Prefix, OtherCompanyChargeCodeGroup.PK).PK;

			ChargeCodeConfigPK = CreatePOSConfig(Env.CurrentCompany.PK, AccChargeCodeSchema.Constants.Prefix, ChargeCode.PK).PK;
			OtherCompanyChargeCodeConfigPK = CreatePOSConfig(OtherCompany.PK, AccChargeCodeSchema.Constants.Prefix, OtherCompanyChargeCode.PK).PK;

			Factory.Save();
		}

		AccPOSConfiguration CreatePOSConfig(ZGuid companyPK, string parentTableCode, ZGuid parentPK)
		{
			var result = Factory.NewWithValidTestData<AccPOSConfiguration>();
			result.PSC_GC = companyPK;
			result.PSC_ParentTableCode = parentTableCode;
			result.PSC_ParentId = parentPK;
			return result;
		}

		AccPOSConfiguration CreateConfigurationRule(ZString jobType, ZString chargeType, ZString direction, string incoTerm = "", string transportMode = "ALL", string taxReg = "", string branchCode = "", string supplyType = "",
			AccPOSChargeCodeGroup group = null, AccChargeCode chargeCode = null)
		{
			var config = Factory.NewWithValidTestData<AccPOSConfiguration>();
			if (group != null)
			{
				config.PSC_ParentId = group.PK;
				config.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
			}
			else if (chargeCode != null)
			{
				config.PSC_ParentId = chargeCode.PK;
				config.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			}
			config.PSC_JobType = jobType;
			config.PSC_ChargeType = chargeType;
			config.PSC_IncoTerm = incoTerm;
			config.PSC_ServiceDirection = direction;
			config.PSC_TransportMode = transportMode;
			config.PSC_TaxRegistrationType = taxReg;
			config.PSC_NK_Branch = branchCode;
			config.PSC_SupplyType = supplyType;
			config.PSC_PlaceOfSupplyRule = "BIL";
			return config;
		}

		#endregion
	}
}
