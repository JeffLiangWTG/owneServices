using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbGroupFilterBusinessObject))]
	sealed class GlbGroupFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		[TestDate(2007, 1, 1)]
		public void TestSearchBySecurityModified()
		{
			GlbGroupFilterBusinessObject filterObject = new GlbGroupFilterBusinessObject();
			ModuleDateFilter securityModifiedFilter = (ModuleDateFilter)filterObject["Security Modified"];
			AssertEquals("Security Modified filter should have HideFutureDates set to true.", true, securityModifiedFilter.HideFutureDates);

			GlbGroup group1 = Factory.NewWithValidTestData<GlbGroup>();
			GlbGroup group2 = Factory.NewWithValidTestData<GlbGroup>();
			Factory.Save();

			securityModifiedFilter.PropertySearch = "Today";
			securityModifiedFilter.IsActive = true;

			GlbGroupCollection collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();

			AssertEquals("Collection should contain group1.", true, collection.Contains(group1.PK));
			AssertEquals("Collection should contain group2.", true, collection.Contains(group2.PK));

			TestDateAttribute.Date = new DateTime(2007, 2, 1);
			group1.SecurityPermissions.AddNew();

			Factory.Save();

			collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();

			AssertEquals("Collection should contain group1.", true, collection.Contains(group1.PK));
			AssertEquals("Collection should not contain group2.", false, collection.Contains(group2.PK));
		}

		[ExpectNoExceptions]
		public void TestSearchBySecurityModifiedDoesNotCrashIfParametersAreEmpty()
		{
			GlbGroupFilterBusinessObject filterObject = new GlbGroupFilterBusinessObject();

			ModuleDateFilter securityModifiedFilter = (ModuleDateFilter)filterObject["Security Modified"];
			securityModifiedFilter.IsActive = true;
			securityModifiedFilter.PropertySearch = "Date range";

			securityModifiedFilter.Property1 = ZDateTime.Now;
			GlbGroupCollection groups = new GlbGroupCollection(Factory);
			groups.Load(filterObject.Filter);

			securityModifiedFilter.Property1 = ZDateTime.Empty;
			securityModifiedFilter.Property2 = ZDateTime.Now;
			groups.Load(filterObject.Filter);
		}

		public void TestNSGFilter()
		{
			var filterObject = new GlbGroupFilterBusinessObject();
			var nsgFilter = (ModuleTextFilter)filterObject["Is Non-Security Group"];

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.IsNonSecurityGroup = true;
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.IsNonSecurityGroup = false;
			Factory.Save();

			nsgFilter.Property = "All";
			nsgFilter.IsActive = true;

			var collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();

			AssertEquals(true, collection.Contains(group1.PK));
			AssertEquals(true, collection.Contains(group2.PK));

			nsgFilter.Property = "Non-Security Group";
			nsgFilter.IsActive = true;

			collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();

			AssertEquals(true, collection.Contains(group1.PK));
			AssertEquals(false, collection.Contains(group2.PK));

			nsgFilter.Property = "Security Group";
			nsgFilter.IsActive = true;

			collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();

			AssertEquals(false, collection.Contains(group1.PK));
			AssertEquals(true, collection.Contains(group2.PK));
		}

		public void TestOSMGFilter()
		{
			var filterObject = new GlbGroupFilterBusinessObject();
			var osmgFilter = (ModuleFlagsFilter)filterObject["OM_GG_OrgSecurityGroup"];

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = group1.PK;
			Factory.Save();

			osmgFilter.Property0 = false;
			osmgFilter.IsActive = true;

			var collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();

			AssertEquals(true, collection.Contains(group1.PK));
			AssertEquals(true, collection.Contains(group2.PK));

			osmgFilter.Property0 = true;
			osmgFilter.IsActive = true;

			collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();

			AssertEquals(true, collection.Contains(group1.PK));
			AssertEquals(false, collection.Contains(group2.PK));
		}

		public void TestExternalIdFilter()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_ExternalId = "888000888";
			group1.GG_IsSales = false;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_ExternalId = "666000666";
			group2.GG_IsSales = false;

			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_ExternalId = string.Empty;
			group3.GG_IsSales = false;
			Factory.Save();

			var glbGroupFilter = new GlbGroupFilterBusinessObject();
			var externalIdFilter = ((ModuleTextFilter)glbGroupFilter["External Id"]);
			externalIdFilter.Property = "888000888";
			externalIdFilter.IsActive = true;
			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var collection1 = new GlbGroupCollection(Factory, glbGroupFilter.Filter);
			collection1.Load();
			Assert("Collection should contain Group1", collection1.Contains(group1.PK));

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			collection1 = new GlbGroupCollection(Factory, glbGroupFilter.Filter);
			collection1.Load();
			Assert("Collection should contain Group2", collection1.Contains(group2.PK));
			Assert("Collection should contain Group3", collection1.Contains(group3.PK));

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			collection1 = new GlbGroupCollection(Factory, glbGroupFilter.Filter);
			collection1.Load();
			Assert("Collection should contain Group3", collection1.Contains(group3.PK));

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			collection1 = new GlbGroupCollection(Factory, glbGroupFilter.Filter);
			collection1.Load();
			Assert("Collection should contain Group1", collection1.Contains(group1.PK));
			Assert("Collection should contain Group2", collection1.Contains(group2.PK));

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection1 = new GlbGroupCollection(Factory, glbGroupFilter.Filter);
			collection1.Load();
			Assert("Collection should contain Group1", collection1.Contains(group1.PK));

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			collection1 = new GlbGroupCollection(Factory, glbGroupFilter.Filter);
			collection1.Load();
			Assert("Collection should contain Group2", collection1.Contains(group2.PK));
			Assert("Collection should contain Group3", collection1.Contains(group3.PK));

			externalIdFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection1 = new GlbGroupCollection(Factory, glbGroupFilter.Filter);
			collection1.Load();
			Assert("Collection should contain Group1", collection1.Contains(group1.PK));
		}

		public void TestCategory()
		{
			var validCategories = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("WER", "Things that were"),
				new CodeDescriptionPair("ARE", "Things that are"),
				new CodeDescriptionPair("NOT", "And some things that have not yet come to pass"),
			};
			SystemDataRegistry.Instance.GroupCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validCategories);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Category = "WER";
			group2.GG_Category = "ARE";

			Factory.Save();

			var bizo = new GlbGroupFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo["Category"];

			filter.IsActive = true;
			filter.Property = "WER";
			AssertContainsExactElementsInAnyOrder(new[] { group1 }, Factory.Load<GlbGroup>(bizo.Filter));

			filter.Property = "ARE";
			AssertContainsExactElementsInAnyOrder(new[] { group2 }, Factory.Load<GlbGroup>(bizo.Filter));
		}

		public void TestParentGroup()
		{
			var grandparentGroup = MasterFilesTestHelper.CreateGroup(Factory, "A");
			var parentGroup = MasterFilesTestHelper.CreateGroup(Factory, "B");
			var childGroup = MasterFilesTestHelper.CreateGroup(Factory, "C");

			childGroup.GG_GG_ParentGroup = parentGroup.PK;
			parentGroup.GG_GG_ParentGroup = grandparentGroup.PK;

			grandparentGroup.GG_IsSecurityEnabled = false;
			parentGroup.GG_IsSecurityEnabled = false;
			childGroup.GG_IsSecurityEnabled = false;

			Factory.Save();

			var bizo = new GlbGroupFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo["GG_GG_ParentGroup"];

			filter.IsActive = true;

			AssertResults(ModuleGuidFilter.ComparisonConstants.Exact, grandparentGroup.PK, parentGroup);
			AssertResults(ModuleGuidFilter.ComparisonConstants.Exact, parentGroup.PK, childGroup);
			AssertResults(ModuleGuidFilter.ComparisonConstants.Exact, childGroup.PK);

			AssertResults(ModuleGuidFilter.ComparisonConstants.NotEqual, grandparentGroup.PK, grandparentGroup, childGroup);
			AssertResults(ModuleGuidFilter.ComparisonConstants.IsNotBlank, ZGuid.Empty, parentGroup, childGroup);
			AssertResults(ModuleGuidFilter.ComparisonConstants.IsBlank, ZGuid.Empty, grandparentGroup);

			void AssertResults(string comparisonOperator, ZGuid value, params GlbGroup[] expectedGroups)
			{
				filter.ComparisonOperator = comparisonOperator;
				filter.Property = value;
				var query = new ZQuery(GlbGroupSchema.GG_IsSystemDefined, false);
				query.AddToFilter(GlbGroupSchema.GG_Type, SQLComparisonOperator.NotEqual, "ORG");
				query.AddToFilter(bizo.Filter);
				var groups = Factory.Load<GlbGroup>(query);
				AssertContainsExactElementsInAnyOrder(expectedGroups, groups);
			}
		}

		public void TestFilter_FactoryLoad()
		{
			var result = Factory.Load(typeof(GlbGroup), new GlbGroupFilterBusinessObject().Filter);

			Assert(result.All(g => g.PK != GlbGroup.DbDeveloperGroupPK));
			Assert(result.All(g => g.PK != GlbGroup.DbReaderGroupPK));
			Assert(result.All(g => g.PK != GlbGroup.BackupOperatorGroupPK));
		}

		#region Active Directory filters

		public void TestADLinkedFilter()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			var glbGroupFilterBizo = new GlbGroupFilterBusinessObject();
			var adLinkedFilter = (ModuleFlagsFilter)glbGroupFilterBizo["ADLinked"];
			adLinkedFilter.Property0 = false;
			adLinkedFilter.IsActive = true;

			var unlinkedGroups = new GlbGroupCollection(Factory, glbGroupFilterBizo.Filter);
			unlinkedGroups.Load();
			Assert("Collection should contain group1", unlinkedGroups.Contains(group1));
			Assert("Collection should contain group2", unlinkedGroups.Contains(group2));
			Assert("Collection should not contain group3", !unlinkedGroups.Contains(group3));

			adLinkedFilter.Property0 = true;
			var linkedGroups = new GlbGroupCollection(Factory, glbGroupFilterBizo.Filter);
			linkedGroups.Load();
			Assert("Collection should not contain group1", !linkedGroups.Contains(group1));
			Assert("Collection should not contain group2", !linkedGroups.Contains(group2));
			Assert("Collection should contain group3", linkedGroups.Contains(group3));
		}

		public void TestDomainNameFilter()
		{
			var filterObject = new GlbGroupFilterBusinessObject();
			var domainFilter = (ModuleTextFilter)filterObject[GlbGroupSchema.Constants.GG_DomainName];
			AssertNotNull("Domain Filter should always be shown.", domainFilter);

			var domain1 = ObjectFactory.Get<IDomainCredentials>();
			domain1.DomainName = "domain1";
			var aDRegistry = ObjectFactory.Get<IADRegistry>();
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1 };
			filterObject = new GlbGroupFilterBusinessObject();
			domainFilter = (ModuleTextFilter)filterObject[GlbGroupSchema.Constants.GG_DomainName];
			AssertNotNull("Domain Filter should always be shown.", domainFilter);

			var domain2 = ObjectFactory.Get<IDomainCredentials>();
			domain2.DomainName = "domain2";
			aDRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1, domain2 };
			filterObject = new GlbGroupFilterBusinessObject();
			domainFilter = (ModuleTextFilter)filterObject[GlbGroupSchema.Constants.GG_DomainName];
			AssertNotNull("Domain Filter should always be shown.", domainFilter);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_DomainName = "domain1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_DomainName = "domain2";
			Factory.Save();

			var collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();
			AssertEquals(true, collection.Contains(group1.PK));
			AssertEquals(true, collection.Contains(group2.PK));

			domainFilter.Property = "domain1";
			domainFilter.IsActive = true;
			collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();
			AssertEquals(true, collection.Contains(group1.PK));
			AssertEquals(false, collection.Contains(group2.PK));

			domainFilter.Property = "domain2";
			domainFilter.IsActive = true;
			collection = new GlbGroupCollection(Factory, filterObject.Filter);
			collection.Load();
			AssertEquals(false, collection.Contains(group1.PK));
			AssertEquals(true, collection.Contains(group2.PK));
		}

		#endregion

		#region  Workflow Custom Fields Filter

		public void TestCustomFieldFilter()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var template = CreateWorkflowTemplate(WorkflowDescriptors.GlbGroupWorkflowDescriptorCode);
			template.P0_OH_Client = client.PK;
			AddCustomField(template, "Custom StringField", AddOnColumnDataType.Codes.String);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.SetUserDefinedValue("Custom StringField", (ZString)"111");

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.SetUserDefinedValue("Custom StringField", (ZString)"222");

			Factory.Save();

			var filter = new GlbGroupFilterBusinessObject();
			var customFilter = (ModuleTextFilter)filter["Custom StringField"];
			AssertNotNull(customFilter);
			customFilter.Property = "111";
			customFilter.IsActive = true;

			var collection = new GlbGroupCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals("Should have 1 GlbGroup", 1, collection.Count);
			AssertCollectionNotContains("Should not have group2", group2, collection);
			AssertCollectionContains("Should have group1", group1, collection);
		}

		public void TestUnrelatedWorkflowFiltersDisabled()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var template = CreateWorkflowTemplate(WorkflowDescriptors.GlbGroupWorkflowDescriptorCode);
			template.P0_OH_Client = client.PK;
			AddCustomField(template, "Custom StringField", AddOnColumnDataType.Codes.String);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.SetUserDefinedValue("Custom StringField", (ZString)"111");

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.SetUserDefinedValue("Custom StringField", (ZString)"222");

			Factory.Save();

			var filter = new GlbGroupFilterBusinessObject();
			AssertNull(filter["Tasks"]);
			AssertNull(filter["Milestones"]);
			AssertNull(filter["Triggers"]);
			AssertNull(filter["Exceptions"]);
			AssertNull(filter["Milestone Date"]);
		}

		ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			return result;
		}

		GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbGroupFilterBusinessObject();
		}

		#endregion
	}
}
