using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.BR;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrganisationFilterBusinessObject))]
	public class OrganisationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSettingDefaultsWithoutSecurityRight()
		{
			using (var module = ZModuleFactory.Instance.Create(ModuleIDs.Organisation))
			{
				var orgFilterBO = new OrganisationFilterBusinessObject();
				module.LimitedColumns = new ZLimitedColumnsProvider(typeof(OrgHeader));
				orgFilterBO.ParentModule = module;
				ExceptionReporterTestListener.Instance.Clear();
				orgFilterBO.SetExternalDefaults(new ConsigneeCollection(Factory));
				AssertEquals("No developer exceptions thrown", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		#region Number Filters

		public void TestEmployeeCountFilter()
		{
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.MiscServ.OM_CMNoOfEmployees = 234;
			organisation2.MiscServ.OM_CMNoOfEmployees = 789;
			organisation3.MiscServ.OM_CMNoOfEmployees = 1000;

			Factory.Save();

			OrganisationFilterBusinessObject filter = new OrganisationFilterBusinessObject();
			((ModuleNumberRangeFilter)filter["Related Staff"]).Property1 = 510;
			((ModuleNumberRangeFilter)filter["Related Staff"]).Property2 = 901;
			((ModuleNumberRangeFilter)filter["Related Staff"]).IsActive = true;

			AssertEquals(ZCalcEditPropertyType.Int, ((ModuleNumberRangeFilter)filter["Related Staff"]).PropertyType);

			OrgHeaderCollection headers = new OrgHeaderCollection(Factory, filter.Filter);
			headers.Load();

			AssertCollectionNotContains(organisation1, headers);
			AssertCollectionContains(organisation2, headers);
			AssertCollectionNotContains(organisation3, headers);
		}

		public void TestAchievableBusiness()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MiscServ.OM_CMAcheivableClientRevenue = 4;
			org2.MiscServ.OM_CMAcheivableClientRevenue = 7;
			org3.MiscServ.OM_CMAcheivableClientRevenue = 11;
			Factory.Save();

			BusinessObjectFactory clearFactory = new BusinessObjectFactory();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterStripBizO["Achievable Business"];
			filter.IsActive = true;
			filter.Property1 = 1;
			filter.Property2 = 6;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property1 = 1;
			filter.Property2 = 10;
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property1 = 5;
			filter.Property2 = 12;
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		public void TestDeliveryRouteSequence()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";

			var orgAddress3 = org3.Addresses.AddNew();
			orgAddress3.OA_Address1 = "Sydney, just above the Harbor bridge.";

			orgAddress1.OA_DeliveryRoute = "ABC";
			orgAddress2.OA_DeliveryRoute = "ABC";
			orgAddress3.OA_DeliveryRoute = "ABC";
			orgAddress1.OA_DeliveryRouteSequence = 1;
			orgAddress2.OA_DeliveryRouteSequence = 2;
			orgAddress3.OA_DeliveryRouteSequence = 3;
			Factory.Save();

			var clearFactory = new BusinessObjectFactory();

			var filter = (ModuleNumberRangeFilter)FilterStripBizO["Delivery Route Sequence"];
			filter.IsActive = true;
			filter.Property1 = 2;
			filter.Property2 = 3;

			var orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder("Assert should not fail under normal conditions", orgCollection.Select(o => o.PK), new[] { org2.PK, org3.PK });

			filter.Property1 = 2 * short.MinValue;
			filter.Property2 = 2 * short.MaxValue;

			Assert(filter.HasNotifications());
			AssertEquals(2, filter.Notifications.Count());
			if (filter.Notifications.Count() == 2)
			{
				AssertEquals("Error - Property1: Please enter a value greater than or equal to 0.", filter.Notifications.ElementAt(0).Message);
				AssertEquals("Error - Property2: Please enter a value less than or equal to 32,767.", filter.Notifications.ElementAt(1).Message);
			}
		}

		public void TestClientNumberFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;
			org3.OH_IsDebtor = false;
			org1.CompanyData.OB_ARClientNumber = "00001000";
			org2.CompanyData.OB_ARClientNumber = "00001001";
			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Client Number"];
			filter.IsActive = true;
			filter.Property = "00001000";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property = "00001";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
		}

		public void TestApprovalNumberFilter_NonUsCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Macau))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.CountryData.OV_EXApprovalNumber = "1111";

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.CountryData.OV_EXApprovalNumber = "1112";
				Factory.Save();

				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved ID Number"];
				AssertNotNull(filter);
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "1111";
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				Assert("Expect collection to contain org1", orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

				filter.Property = "11";
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				orgCollection.Load(FilterStripBizO.Filter);

				Assert("Expect collection to contain org1", orgCollection.Contains(org1));
				Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			}
		}

		public void TestApprovalNumberFilter_UsCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.CountryData.OV_EXApprovalNumber = "1111";

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.CountryData.OV_EXApprovalNumber = "1112";
				Factory.Save();

				var filter = (ModuleTextFilter)FilterStripBizO["TSA ID Number"];
				AssertNotNull(filter);
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "1111";
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				Assert("Expect collection to contain org1", orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

				filter.Property = "11";
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				orgCollection.Load(FilterStripBizO.Filter);

				Assert("Expect collection to contain org1", orgCollection.Contains(org1));
				Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			}
		}

		#region Code Mapping

		#region Foreign Code

		public void TestCodeMappingForeignFilter()
		{
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("PPP");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				var org4 = Factory.NewWithValidTestData<OrgHeader>();

				var codeOverride1 = CreateCodeOverride(Factory, org1, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");
				var codeOverride2 = CreateCodeOverride(Factory, org2, "AAB", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");
				var codeOverride3 = CreateCodeOverride(Factory, org3, "CAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");
				var codeOverride4 = CreateCodeOverride(Factory, org4, "ADD", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");

				Factory.Save();

				var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory());
				var filter = (OrgCodeMappingForeignModuleFilter)FilterStripBizO["Code Mapping (Foreign Code)"];

				filter.IsActive = true;
				filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				filter.Context = "PPP";

				filter.ForeignCode = "AAA";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Foreign Code starts with AAA", new[] { org1.PK }, orgCollection.Select(org => org.PK));

				filter.ForeignCode = "AA";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Foreign Code starts with AA", new[] { org1.PK, org2.PK }, orgCollection.Select(org => org.PK));

				filter.ForeignCode = "A";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Foreign Code starts with A", new[] { org1.PK, org2.PK, org4.PK }, orgCollection.Select(org => org.PK));

				filter.ForeignCode = "C";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Foreign Code starts with C", new[] { org3.PK }, orgCollection.Select(org => org.PK));

				filter.ForeignCode = "D";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Foreign Code starts with D (none)", Array.Empty<ZGuid>(), orgCollection.Select(org => org.PK));

				filter.ForeignCode = "";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Foreign Code wildcard (any)", new[] { org1.PK, org2.PK, org3.PK, org4.PK }, orgCollection.Select(org => org.PK));
			}
		}

		public void TestCodeMappingForeignFilter_Relationship()
		{
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("PPP");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				var org4 = Factory.NewWithValidTestData<OrgHeader>();

				var codeOverride1 = CreateCodeOverride(Factory, org1, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");
				var codeOverride2 = CreateCodeOverride(Factory, org2, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Commodities, "PPP");
				var codeOverride3 = CreateCodeOverride(Factory, org3, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Warehouse, "PPP");
				var codeOverride4 = CreateCodeOverride(Factory, org4, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");

				Factory.Save();

				var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory());
				var filter = (OrgCodeMappingForeignModuleFilter)FilterStripBizO["Code Mapping (Foreign Code)"];

				filter.IsActive = true;
				filter.ForeignCode = "AAA";
				filter.Context = "PPP";

				filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Relationship = Organisation", new[] { org1.PK, org4.PK }, orgCollection.Select(org => org.PK));

				filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Commodities;
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Relationship = Commodities", new[] { org2.PK, }, orgCollection.Select(org => org.PK));

				filter.RelationshipType = "";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Relationship wildcard (any)", new[] { org1.PK, org2.PK, org3.PK, org4.PK }, orgCollection.Select(org => org.PK));
			}
		}

		public void TestCodeMappingForeignFilter_Context()
		{
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("OOO");
			validCodes.AddPair("PPP");
			validCodes.AddPair("QQQ");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				var org4 = Factory.NewWithValidTestData<OrgHeader>();

				var codeOverride1 = CreateCodeOverride(Factory, org1, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "OOO");
				var codeOverride2 = CreateCodeOverride(Factory, org2, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "OOO");
				var codeOverride3 = CreateCodeOverride(Factory, org3, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "QQQ");
				var codeOverride4 = CreateCodeOverride(Factory, org4, "AAA", "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");

				Factory.Save();

				var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory());
				var filter = (OrgCodeMappingForeignModuleFilter)FilterStripBizO["Code Mapping (Foreign Code)"];

				filter.IsActive = true;
				filter.ForeignCode = "AAA";
				filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;

				filter.Context = "OOO";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Context = OOO", new[] { org1.PK, org2.PK }, orgCollection.Select(org => org.PK));

				filter.Context = "PPP";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Context = PPP", new[] { org4.PK }, orgCollection.Select(org => org.PK));

				filter.Context = "";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Context wildcard (any)", new[] { org1.PK, org2.PK, org3.PK, org4.PK }, orgCollection.Select(org => org.PK));
			}
		}

		#endregion

		#region Local Code

		public void TestCodeMappingLocalFilter()
		{
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("PPP");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();

				var overrideOrg1 = Factory.NewWithValidTestData<OrgHeader>();
				var overrideOrg2 = Factory.NewWithValidTestData<OrgHeader>();
				var overrideOrg3 = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				var codeOverride1 = CreateCodeOverride(Factory, org1, overrideOrg1, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");
				var codeOverride2 = CreateCodeOverride(Factory, org2, overrideOrg1, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");
				var codeOverride3 = CreateCodeOverride(Factory, org3, overrideOrg2, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");

				Factory.Save();

				var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory());
				var filter = (OrgCodeMappingLocalModuleFilter)FilterStripBizO["Code Mapping (Local Code)"];

				filter.IsActive = true;
				filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				filter.Context = "PPP";

				filter.LocalCode = overrideOrg1.PK;
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Local Code 1", new[] { org1.PK, org2.PK }, orgCollection.Select(org => org.PK));

				filter.LocalCode = overrideOrg2.PK;
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Local Code 2", new[] { org3.PK }, orgCollection.Select(org => org.PK));

				filter.LocalCode = overrideOrg3.PK;
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Local Code 3 (none)", Array.Empty<ZGuid>(), orgCollection.Select(org => org.PK));

				filter.LocalCode = ZGuid.Empty;
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for any Local Code", new[] { org1.PK, org2.PK, org3.PK }, orgCollection.Select(org => org.PK));
			}
		}

		public void TestCodeMappingLocalFilter_Relationship()
		{
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("PPP");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				var org4 = Factory.NewWithValidTestData<OrgHeader>();

				var overrideOrg = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				var codeOverride1 = CreateCodeOverride(Factory, org1, overrideOrg, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");
				var codeOverride2 = CreateCodeOverride(Factory, org2, overrideOrg, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Commodities, "PPP");
				var codeOverride3 = CreateCodeOverride(Factory, org3, overrideOrg, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Warehouse, "PPP");
				var codeOverride4 = CreateCodeOverride(Factory, org4, overrideOrg, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");

				Factory.Save();

				var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory());
				var filter = (OrgCodeMappingLocalModuleFilter)FilterStripBizO["Code Mapping (Local Code)"];

				filter.IsActive = true;
				filter.LocalCode = overrideOrg.PK;
				filter.Context = "PPP";

				filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Relationship = Organisation", new[] { org1.PK, org4.PK }, orgCollection.Select(org => org.PK));

				filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Commodities;
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Relationship = Commodities", new[] { org2.PK, }, orgCollection.Select(org => org.PK));

				filter.RelationshipType = "";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Relationship wildcard (any)", new[] { org1.PK, org2.PK, org3.PK, org4.PK }, orgCollection.Select(org => org.PK));
			}
		}

		public void TestCodeMappingLocalFilter_Context()
		{
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("OOO");
			validCodes.AddPair("PPP");
			validCodes.AddPair("QQQ");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				var org4 = Factory.NewWithValidTestData<OrgHeader>();

				var overrideOrg = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				var codeOverride1 = CreateCodeOverride(Factory, org1, overrideOrg, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "OOO");
				var codeOverride2 = CreateCodeOverride(Factory, org2, overrideOrg, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "OOO");
				var codeOverride3 = CreateCodeOverride(Factory, org3, overrideOrg, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "QQQ");
				var codeOverride4 = CreateCodeOverride(Factory, org4, overrideOrg, "ZZZ", Constants.OrgPatternMatchOverrideRelationships.Organisation, "PPP");

				Factory.Save();

				var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory());
				var filter = (OrgCodeMappingLocalModuleFilter)FilterStripBizO["Code Mapping (Local Code)"];

				filter.IsActive = true;
				filter.LocalCode = overrideOrg.PK;
				filter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;

				filter.Context = "OOO";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Context = OOO", new[] { org1.PK, org2.PK }, orgCollection.Select(org => org.PK));

				filter.Context = "PPP";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Context = PPP", new[] { org4.PK }, orgCollection.Select(org => org.PK));

				filter.Context = "";
				orgCollection.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder("Filter for Context wildcard (any)", new[] { org1.PK, org2.PK, org3.PK, org4.PK }, orgCollection.Select(org => org.PK));
			}
		}

		#endregion

		static OrgPatternMatchOverride CreateCodeOverride(BusinessObjectFactory factory, OrgHeader org, string foreignCode, string localCode, string relationship, string context)
		{
			var code = factory.New<OrgPatternMatchOverride>();
			code.OO_OH = org.PK;
			code.OO_Relationship = relationship;
			code.OO_Context = context;
			code.OO_ForeignCode = foreignCode;
			code.OO_LocalCode = localCode;
			return code;
		}

		static OrgPatternMatchOverride CreateCodeOverride(BusinessObjectFactory factory, OrgHeader org, OrgHeader overrideOrg, string localCode, string relationship, string context)
		{
			var code = factory.New<OrgPatternMatchOverride>();
			code.OO_OH = org.PK;
			code.OO_Relationship = relationship;
			code.OO_Context = context;
			code.OO_LocalGuid = overrideOrg.PK;
			code.OO_LocalCode = localCode;
			return code;
		}

		#endregion

		#endregion

		#region Other filters

		public void TestTaxConfigurationFilterIsPresent()
		{
			var filter = FilterStripBizO["Tax Configuration"];
			AssertNull("No Tax Configuration filter presenet yet", filter);
			FilterStripBizO.ResetModuleFilters();

			var taxConfig = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			filter = FilterStripBizO["Tax Configuration"];
			AssertNotNull("Tax Configuration filter should be visible now.", filter);
			AssertType<OrgTaxConfigurationModuleFilter>(filter);
		}

		public void TestNoTransaction()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			AccTransactionHeader acc1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc4 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc5 = Factory.NewWithValidTestData<AccTransactionHeader>();

			acc1.AH_OH = org1.PK;
			acc1.AH_PostDate = new ZDateTime(2005, 2, 2);

			acc2.AH_OH = org2.PK;
			acc2.AH_PostDate = new ZDateTime(2005, 2, 22);

			acc3.AH_OH = org3.PK;
			acc3.AH_PostDate = new ZDateTime(2005, 4, 12);

			acc4.AH_OH = org1.PK;
			acc4.AH_PostDate = new ZDateTime(2005, 5, 12);

			acc5.AH_PostDate = new ZDateTime(2005, 2, 2); // In scope but null org

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["No Transaction"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 3, 3);
			filter.IsActive = true;

			ZQuery findOrgQuery = new ZQuery(OrgHeaderSchema.PK, new ZGuid[] { org1.PK, org2.PK, org3.PK });

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, findOrgQuery);
			orgCollection.Load();
			AssertEquals("Precondition: 3 orgs are loaded", 3, orgCollection.Count);

			findOrgQuery.AddToFilter(FilterStripBizO.Filter);
			orgCollection = new OrgHeaderCollection(Factory, findOrgQuery);
			orgCollection.Load();

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
		}

		public void TestNoTransactionInCurrentCompany()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();

			AccTransactionHeader acc1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc4 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc5 = Factory.NewWithValidTestData<AccTransactionHeader>();

			GlbCompany otherCompany = Factory.NewWithValidTestData<GlbCompany>();

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();

			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch3.GB_GC = otherCompany.PK;

			acc1.AH_OH = org1.PK;
			acc1.AH_GB = branch1.PK;
			acc1.AH_PostDate = new ZDateTime(2005, 2, 2);
			acc1.AH_Ledger = "AR";

			acc2.AH_OH = org2.PK;
			acc2.AH_GB = branch2.PK;
			acc2.AH_PostDate = new ZDateTime(2005, 2, 22);
			acc2.AH_Ledger = "AR";

			acc3.AH_OH = org3.PK;
			acc3.AH_GB = branch3.PK;
			acc3.AH_PostDate = new ZDateTime(2005, 2, 12);
			acc3.AH_Ledger = "AR";

			acc4.AH_OH = org1.PK;
			acc4.AH_GB = branch3.PK;
			acc4.AH_PostDate = new ZDateTime(2005, 2, 12);
			acc4.AH_Ledger = "AR";

			acc5.AH_OH = org5.PK;
			acc5.AH_GB = branch1.PK;
			acc5.AH_PostDate = new ZDateTime(2006, 2, 12);
			acc5.AH_Ledger = "AR";

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["No Transaction in Current Company"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 3, 3);
			filter.IsActive = true;

			ZQuery findOrgQuery = new ZQuery(OrgHeaderSchema.PK, new ZGuid[] { org1.PK, org2.PK, org3.PK, org4.PK, org5.PK });

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, findOrgQuery);
			orgCollection.Load();
			AssertEquals("Precondition: 5 orgs are loaded", 5, orgCollection.Count);

			findOrgQuery.AddToFilter(FilterStripBizO.Filter);
			orgCollection = new OrgHeaderCollection(Factory, findOrgQuery);
			orgCollection.Load();

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
			Assert("Expect collection to contain org4", orgCollection.Contains(org4));
			Assert("Expect collection to contain org5", orgCollection.Contains(org5));
		}

		public void TestNoTransactionInCurrentCompanyFilterWithNoneARAPTransactionExists()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader acc1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc2 = Factory.NewWithValidTestData<AccTransactionHeader>();

			acc1.AH_OH = org1.PK;
			acc1.AH_GB = branch1.PK;
			acc1.AH_PostDate = new ZDateTime(2005, 2, 2);
			acc1.AH_Ledger = LedgerTypes.AccountsReceivable;
			acc1.AH_TransactionType = TransactionTypes.Invoice;

			acc2.AH_OH = ZGuid.Empty;
			acc2.AH_GB = branch1.PK;
			acc2.AH_PostDate = new ZDateTime(2005, 2, 22);
			acc2.AH_Ledger = LedgerTypes.CashBook;
			acc2.AH_TransactionType = TransactionTypes.DirectReceipt;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["No Transaction in Current Company"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 3, 3);
			filter.IsActive = true;

			ZQuery findOrgQuery = new ZQuery(OrgHeaderSchema.PK, new ZGuid[] { org1.PK, org2.PK });

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, findOrgQuery);
			orgCollection.Load();
			AssertEquals("Precondition: 2 orgs are loaded", 2, orgCollection.Count);

			findOrgQuery.AddToFilter(FilterStripBizO.Filter);
			orgCollection = new OrgHeaderCollection(Factory, findOrgQuery);
			orgCollection.Load();

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		#endregion

		#region Text filters

		public void TestAccountSecurityCode_CA()
		{
			string originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);

				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				var cdt1 = Factory.New<OrgCountryData>();
				cdt1.OV_OH_OrgHeader = org1.PK;
				cdt1.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Canada;
				cdt1.ImpAddInfo["ZO_AccountSecurityNumber"] = "12345";

				var cdt2 = Factory.New<OrgCountryData>();
				cdt2.OV_OH_OrgHeader = org2.PK;
				cdt2.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Canada;
				cdt2.ImpAddInfo["ZO_AccountSecurityNumber"] = "23456";

				var cdt3 = Factory.New<OrgCountryData>();
				cdt3.OV_OH_OrgHeader = org3.PK;
				cdt3.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Canada;
				Factory.Save();

				var filter = (ModuleTextFilter)FilterStripBizO["Account Security Number"];
				filter.Property = "12345";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();

				Assert("Expect collection to contain org1", orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

				filter.Property = "123";
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

				var orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection2.Load();

				Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
				Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

				filter.Property = "2345";
				filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

				var orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection3.Load();

				Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
				Assert("Expect collection to contain org2", orgCollection3.Contains(org2));

				filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

				var orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection4.Load();

				Assert("Expect collection to contain org1", orgCollection4.Contains(org1));
				Assert("Expect collection to contain org2", orgCollection4.Contains(org2));
				Assert("Expect collection not to contain org3", !orgCollection4.Contains(org3));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestOrgCode()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Code"];
			filter.Property = "XXXYYY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org2));

			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));

			filter.Property = "XY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection3.Contains(org2));
		}

		public void TestOrgFullName()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_FullName = "XXXYYY";
			org2.OH_FullName = "XXXXXX";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Name"];
			AssertEquals("Prefix", "N", filter.Prefix);
			filter.Property = "XXXYYY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));

			filter.Property = "XY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection3.Contains(org2));
		}

		public void TestOrgNameOverrides()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			var adr1 = org1.Addresses.AddNew();
			var adr2 = org1.Addresses.AddNew();
			var adr3 = org2.Addresses.AddNew();

			org1.OH_FullName = "XXXYYY";
			org2.OH_FullName = "XXXXXX";

			adr1.OA_Address1 = ZGuid.NewZGuid().ToString();
			adr2.OA_Address1 = ZGuid.NewZGuid().ToString();
			adr3.OA_Address1 = ZGuid.NewZGuid().ToString();
			adr1.OA_Code = ZGuid.NewZGuid().ToString().Substring(0, 25);
			adr2.OA_Code = ZGuid.NewZGuid().ToString().Substring(0, 25);
			adr3.OA_Code = ZGuid.NewZGuid().ToString().Substring(0, 25);
			adr1.OA_CompanyNameOverride = "~AAAAAA";
			adr2.OA_CompanyNameOverride = "~ABAAAA";
			adr3.OA_CompanyNameOverride = "~BAAAAA";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Additional Company Names"];
			filter.Property = "XXXYYY";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "~AAAAAA";

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "~A";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection3.Contains(org2));

			filter.Property = "BAAAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection to contain org1", orgCollection4.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection4.Contains(org2));
		}

		public void TestOrgBrandOrRelatedName()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			OrgBrandOrRelatedName brandOrRelatedName1 = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			brandOrRelatedName1.P1_RelatedName = "Brand 1";
			org1.BrandsOrRelatedNames.Add(brandOrRelatedName1);

			OrgBrandOrRelatedName brandOrRelatedName2 = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			brandOrRelatedName2.P1_RelatedName = "Brand 2";
			org2.BrandsOrRelatedNames.Add(brandOrRelatedName2);

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Related Company Name"];
			filter.Property = "Brand 1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Brand";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));

			filter.Property = "and 1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection3.Contains(org2));
		}

		public void TestOrgRegistrationNumber()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "XXXZZZ";

			OrgCusCode orgCusCode1 = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode1.OK_CustomsRegNo = "12345";
			orgCusCode1.OK_OH = org1.PK;

			OrgCusCode orgCusCode2 = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode2.OK_CustomsRegNo = "54321";
			orgCusCode2.OK_OH = org2.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Code"];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			filter = (ModuleTextFilter)FilterStripBizO["Registration Number"];
			filter.Property = "12345";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection2.Contains(org3));

			filter.Property = "3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection2.Contains(org3));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;

			OrgHeaderCollection orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection not to contain org1", !orgCollection4.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection4.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection4.Contains(org3));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			OrgHeaderCollection orgCollection5 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection5.Load();

			Assert("Expect collection to contain org1", orgCollection5.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection5.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection5.Contains(org3));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;

			OrgHeaderCollection orgCollection6 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection6.Load();

			Assert("Expect collection not to contain org1", !orgCollection6.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection6.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection6.Contains(org3));
		}

		public void TestOrgAddress1()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XAAAAA";
			org2.OH_Code = "XBBBBB";
			org3.OH_Code = "XCCCCC";

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "42 Wallaby Way, Sydney, Australia";
			orgAddress1.OA_Address2 = "740 Evergreen Terrace, Springfield, USA";

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "221B Baker Street, London, UK";
			orgAddress2.OA_Address2 = "425 Grove Street, New York, USA ";

			var orgAddress3 = org3.Addresses.AddNew();
			orgAddress3.OA_Address1 = "711 Maple Street, Indiana, USA";
			orgAddress3.OA_Address2 = "12 Grimmauld Place, London, UK";

			Factory.Save();

			var filter = (OrgAddressWithActiveStatusModuleTextFilter)FilterStripBizO["Address 1"];
			filter.Property = "42 Wallaby Way, Sydney, Australia";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			AssertEquals("Default status value", OrgAddressWithActiveStatusModuleTextFilter.StatusActive, filter.ActiveStatus);

			var orgCollection1 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			CombineAssertions("1 result", () =>
			{
				AssertEquals("Expect collection to contain org1", true, orgCollection1.Contains(org1));
				AssertEquals("Expect collection to not contain org2", false, orgCollection1.Contains(org2));
				AssertEquals("Expect collection to not contain org3", false, orgCollection1.Contains(org3));
			});

			filter.Property = "740 Evergreen Terrace, Springfield, USA";

			var orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			CombineAssertions("No results", () =>
			{
				AssertEquals("Expect collection to not contain org1", false, orgCollection2.Contains(org1));
				AssertEquals("Expect collection to not contain org2", false, orgCollection2.Contains(org2));
				AssertEquals("Expect collection to not contain org3", false, orgCollection2.Contains(org3));
			});

			filter.Property = "221B";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			var orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			CombineAssertions("1 result", () =>
			{
				AssertEquals("Expect collection to not contain org1", false, orgCollection3.Contains(org1));
				AssertEquals("Expect collection to contain org2", true, orgCollection3.Contains(org2));
				AssertEquals("Expect collection to not contain org3", false, orgCollection3.Contains(org3));
			});

			filter.Property = "Street";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			var orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			CombineAssertions("2 results", () =>
			{
				AssertEquals("Expect collection to not contain org1", false, orgCollection4.Contains(org1));
				AssertEquals("Expect collection to contain org2", true, orgCollection4.Contains(org2));
				AssertEquals("Expect collection to contain org3", true, orgCollection4.Contains(org3));
			});
		}

		public void TestOrgAddress2()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XAAAAA";
			org2.OH_Code = "XBBBBB";
			org3.OH_Code = "XCCCCC";

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "42 Wallaby Way, Sydney, Australia";
			orgAddress1.OA_Address2 = "740 Evergreen Terrace, Springfield, USA";

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "221B Baker Street, London, UK";
			orgAddress2.OA_Address2 = "425 Grove Street, New York, USA ";

			var orgAddress3 = org3.Addresses.AddNew();
			orgAddress3.OA_Address1 = "711 Maple Street, Indiana, USA";
			orgAddress3.OA_Address2 = "12 Grimmauld Place, London, UK";

			Factory.Save();

			var filter = (OrgAddressWithActiveStatusModuleTextFilter)FilterStripBizO["Address 2"];
			filter.Property = "740 Evergreen Terrace, Springfield, USA";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			AssertEquals("Default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);

			var orgCollection1 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			CombineAssertions("1 result", () =>
			{
				AssertEquals("Expect collection to contain org1", true, orgCollection1.Contains(org1));
				AssertEquals("Expect collection to not contain org2", false, orgCollection1.Contains(org2));
				AssertEquals("Expect collection to not contain org3", false, orgCollection1.Contains(org3));
			});

			filter.Property = "42 Wallaby Way, Sydney, Australia";

			var orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			CombineAssertions("No results", () =>
			{
				AssertEquals("Expect collection to not contain org1", false, orgCollection2.Contains(org1));
				AssertEquals("Expect collection to not contain org2", false, orgCollection2.Contains(org2));
				AssertEquals("Expect collection to not contain org3", false, orgCollection2.Contains(org3));
			});

			filter.Property = "42";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			var orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			CombineAssertions("1 result", () =>
			{
				AssertEquals("Expect collection to not contain org1", false, orgCollection3.Contains(org1));
				AssertEquals("Expect collection to contain org2", true, orgCollection3.Contains(org2));
				AssertEquals("Expect collection to not contain org3", false, orgCollection3.Contains(org3));
			});

			filter.Property = "USA";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			var orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			CombineAssertions("2 results", () =>
			{
				AssertEquals("Expect collection to contain org1", true, orgCollection4.Contains(org1));
				AssertEquals("Expect collection to contain org2", true, orgCollection4.Contains(org2));
				AssertEquals("Expect collection to not contain org3", false, orgCollection4.Contains(org3));
			});
		}

		public void TestOrgAddress_OnlyAddress1()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";

			Factory.Save();

			var filter = (OrgAddressWithActiveStatusModuleTextFilter)FilterStripBizO["Address"];
			filter.Property = "Paris, just under the bridge of Alexander III.";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			AssertEquals("Default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Paris";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			var orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "bridge";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			var orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgAddress_BothAddress1AndAddress2()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "Sydney, just under the Harbor bridge.";

			Factory.Save();

			var filter = (OrgAddressWithActiveStatusModuleTextFilter)FilterStripBizO["Address"];
			filter.Property = "Paris, just under the bridge of Alexander III.";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			AssertEquals("Default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Paris";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			var orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "bridge";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			var orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));

			filter.Property = "tiger";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			var orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection not to contain org1", !orgCollection4.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection4.Contains(org2));
		}

		public void TestOrgAddress_FilterAddressLinesWithActiveStatus()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_IsActive = false;

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "Paris, just under the bridge of Alexander III.";

			Factory.Save();

			var filter = (OrgAddressWithActiveStatusModuleTextFilter)FilterStripBizO["Address"];
			var filterAddress1 = (ModuleTextFilter)FilterStripBizO["Address 1"];
			AssertEquals("Default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);
			AssertEquals("Should have same sub group", filterAddress1.SubGroup, filter.SubGroup);

			filter.Property = "Paris, just under the bridge of Alexander III.";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org2.PK }, orgCollection.GetPKs());

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK }, orgCollection.GetPKs());

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK, org2.PK }, orgCollection.GetPKs());

			filter.Clear();
			AssertEquals("Restore to default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);
		}

		public void TestOrgAddress_FilterAddress1WithActiveStatus()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_IsActive = false;

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "Paris, just under the bridge of Alexander III.";

			Factory.Save();

			var filter = (OrgAddressWithActiveStatusModuleTextFilter)FilterStripBizO["Address 1"];
			AssertEquals("Default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);

			filter.Property = "Paris, just under the bridge of Alexander III.";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals(false, orgCollection.Any());

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK }, orgCollection.GetPKs());

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK }, orgCollection.GetPKs());

			filter.Clear();
			AssertEquals("Restore to default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);
		}

		public void TestOrgAddress_FilterAddress2WithActiveStatus()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "Paris, just under the bridge of Alexander III.";
			orgAddress2.OA_IsActive = false;

			Factory.Save();

			var filter = (OrgAddressWithActiveStatusModuleTextFilter)FilterStripBizO["Address 2"];
			AssertEquals("Default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);

			filter.Property = "Paris, just under the bridge of Alexander III.";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals(false, orgCollection.Any());

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org2.PK }, orgCollection.GetPKs());

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org2.PK }, orgCollection.GetPKs());

			filter.Clear();
			AssertEquals("Restore to default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);
		}

		public void TestOrgAdditionalAddress()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Address1";
			orgAddress1.OA_AdditionalAddressInformation = "Paris, just under the bridge of Alexander III.";

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Address2";
			orgAddress2.OA_AdditionalAddressInformation = "Sydney, just under the Harbor bridge.";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Additional Address Info"];
			filter.Property = "Paris, just under the bridge of Alexander III.";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Paris";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			var orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "bridge";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			var orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgAdditionalAddress_WithActiveStatus()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			var orgAddress1 = org1.Addresses.AddNew();
			var addressInfo1 = Factory.New<OrgAddressAdditionalInfo>();
			addressInfo1.OAI_OA_Address = orgAddress1.PK;
			addressInfo1.OAI_IsPrimary = true;
			addressInfo1.OAI_AdditionalInfo = "Paris, just under the bridge of Alexander III.";
			orgAddress1.Address1 = "Address1";
			var orgAddress2 = org2.Addresses.AddNew();
			var addressInfo2 = Factory.New<OrgAddressAdditionalInfo>();
			addressInfo2.OAI_OA_Address = orgAddress2.PK;
			addressInfo2.OAI_IsPrimary = true;
			addressInfo2.OAI_AdditionalInfo = "Paris, just under the bridge of Alexander III.";
			orgAddress2.Address1 = "Address2";
			orgAddress2.OA_IsActive = false;
			Factory.Save();

			AssertFilterWithActiveStatus(org1, org2, "Additional Address Info", "Paris, just under the bridge of Alexander III.");
		}

		void AssertFilterWithActiveStatus(OrgHeader activeOrg, OrgHeader inactiveOrg, string filterDescription, string filterValue)
		{
			var filter = (OrgAddressWithActiveStatusModuleTextFilter)FilterStripBizO[filterDescription];
			AssertEquals("Default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code, filter.ActiveStatus);

			filter.Property = filterValue;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection to contain activeOrg", orgCollection.Contains(activeOrg));

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection to contain inactiveOrg", orgCollection.Contains(inactiveOrg));

			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain activeOrg", orgCollection.Contains(activeOrg));
			Assert("Expect collection to contain inactiveOrg", orgCollection.Contains(inactiveOrg));

			filter.Clear();
			AssertEquals("Restore to default status value", filter.ActiveStatusList[OrgAddressWithActiveStatusModuleTextFilter.StatusActive].Code,
				filter.ActiveStatus);
		}

		public void TestOrgCity()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_City = "Paris";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_City = "Sydney";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["City"];
			filter.Property = "Paris";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Par";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "s";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgCity_WithActiveStatus()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_City = "Sydney";
			orgAddress1.Address1 = "Address1";
			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_City = "Sydney";
			orgAddress2.Address1 = "Address2";
			orgAddress2.OA_IsActive = false;
			Factory.Save();

			AssertFilterWithActiveStatus(org1, org2, "City", "Sydney");
		}

		public void TestOrgState()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_State = "France";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_State = "Australia";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["State"];
			filter.Property = "France";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Fra";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "ra";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgState_WithActiveStatus()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_State = "Australia";
			orgAddress1.Address1 = "Address1";
			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_State = "Australia";
			orgAddress2.Address1 = "Address2";
			orgAddress2.OA_IsActive = false;
			Factory.Save();

			AssertFilterWithActiveStatus(org1, org2, "State", "Australia");
		}

		public void TestOrgPostCode()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_PostCode = "12345";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_PostCode = "54321";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Post Code"];
			filter.Property = "12345";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgPostCode_WithActiveStatus()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_PostCode = "12345";
			orgAddress1.Address1 = "Address1";
			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_PostCode = "12345";
			orgAddress2.Address1 = "Address2";
			orgAddress2.OA_IsActive = false;
			Factory.Save();

			AssertFilterWithActiveStatus(org1, org2, "Post Code", "12345");
		}

		public void TestOrgPhone()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_Phone = "12345";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_Phone = "54321";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Phone"];
			filter.Property = "12345";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgMobile()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_Mobile = "12345";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_Mobile = "54321";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Mobile"];
			filter.Property = "12345";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgFax()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_Fax = "12345";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_Fax = "54321";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Fax"];
			filter.Property = "12345";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgEmail()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";
			orgAddress1.OA_Email = "alexander.korotun@cargowise.com";

			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";
			orgAddress2.OA_Email = "alexander.korotun@edi.com";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Email"];
			filter.Property = "alexander.korotun@cargowise.com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "alex";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));

			filter.Property = ".com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgAddressSubgroup()
		{
			var addressSubGroup = FilterStripBizO["Address"].SubGroup;

			CombineAssertions("All text address filters should use the same subgroup: ", () =>
			{
				AssertEquals("Additional Company Names", addressSubGroup, FilterStripBizO["Additional Company Names"].SubGroup);
				AssertEquals("Address 1", addressSubGroup, FilterStripBizO["Address 1"].SubGroup);
				AssertEquals("Address 2", addressSubGroup, FilterStripBizO["Address 2"].SubGroup);
				AssertEquals("Additional Address Info", addressSubGroup, FilterStripBizO["Additional Address Info"].SubGroup);
				AssertEquals("City", addressSubGroup, FilterStripBizO["City"].SubGroup);
				AssertEquals("State", addressSubGroup, FilterStripBizO["State"].SubGroup);
				AssertEquals("Post Code", addressSubGroup, FilterStripBizO["Post Code"].SubGroup);
				AssertEquals("Phone", addressSubGroup, FilterStripBizO["Phone"].SubGroup);
				AssertEquals("Mobile", addressSubGroup, FilterStripBizO["Mobile"].SubGroup);
				AssertEquals("Fax", addressSubGroup, FilterStripBizO["Fax"].SubGroup);
				AssertEquals("Email", addressSubGroup, FilterStripBizO["Email"].SubGroup);
			});
		}

		public void TestOrgWeb()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.MainWebURL.PU_URL = "http://www.cargowise.com";
			org2.MainWebURL.PU_URL = "http://www.edi.com";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Web"];
			filter.Property = "http://www.cargowise.com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "http://www.";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));

			filter.Property = ".com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgContact_FilterContactNamesWithActiveStatus()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			var orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_ContactName = "Person A";
			orgContact1.OC_IsActive = true;

			var orgContact2 = org2.Contacts.AddNew();
			orgContact2.OC_ContactName = "Person B";
			orgContact2.OC_IsActive = false;

			Factory.Save();

			var filter = (OrgContactsActiveStatusAndInfoModuleFilter)FilterStripBizO["Contact Name"];
			AssertEquals("Default status value", OrgContactsActiveStatusAndInfoModuleFilter.StatusActive, filter.ActiveStatus);

			filter.Property = "Person A";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK }, orgCollection.GetPKs());

			filter.Property = "Person B";
			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusInactive;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org2.PK }, orgCollection.GetPKs());

			filter.Property = "Person";
			filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusAll;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK, org2.PK }, orgCollection.GetPKs());

			filter.Clear();
			AssertEquals("Restore to default status value", OrgContactsActiveStatusAndInfoModuleFilter.StatusActive, filter.ActiveStatus);
		}

		public void TestOrgContactName()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgContact orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_ContactName = "Postman Pechkin";

			OrgContact orgContact2 = org2.Contacts.AddNew();
			orgContact2.OC_ContactName = "Kot Matroskin";

			Factory.Save();

			var filter = (OrgContactsActiveStatusAndInfoModuleFilter)FilterStripBizO["Contact Name"];
			filter.Property = "Postman Pechkin";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "Postman";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			filter.Property = "kin";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
		}

		public void TestOrgContactPhone()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "XXXXXW";

			OrgContact orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_ContactName = "Postman Pechkin";
			orgContact1.OC_Phone = "1234567";

			OrgContact orgContact2 = org2.Contacts.AddNew();
			orgContact2.OC_ContactName = "Kot Matroskin";
			orgContact2.OC_Phone = "7654321";

			OrgContact orgContact3 = org3.Contacts.AddNew();
			orgContact3.OC_ContactName = "Andrew";
			orgContact3.OC_Phone = "99999999";
			OrgContactItem orgContact3SecondaryPhone = Factory.New<OrgContactItem>();
			orgContact3SecondaryPhone.OI_ContactItemType = OrgContactItemTypes.Codes.Phone;
			orgContact3SecondaryPhone.OI_OC = orgContact3.PK;
			orgContact3SecondaryPhone.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			orgContact3SecondaryPhone.OI_Address = "1234567";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Contact Work Phone"];
			filter.Property = "1234567";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection2.Contains(org3));

			filter.Property = "3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection3.Contains(org3));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;

			OrgHeaderCollection orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection not to contain org1", !orgCollection4.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection4.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection4.Contains(org3));
		}

		public void TestOrgContactMobile()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "XXXXXW";

			OrgContact orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_ContactName = "Postman Pechkin";
			orgContact1.OC_Mobile = "1234567";

			OrgContact orgContact2 = org2.Contacts.AddNew();
			orgContact2.OC_ContactName = "Kot Matroskin";
			orgContact2.OC_Mobile = "7654321";

			OrgContact orgContact3 = org3.Contacts.AddNew();
			orgContact3.OC_ContactName = "Andrew";
			orgContact3.OC_Mobile = "99999999";
			OrgContactItem orgContact3SecondaryMobile = Factory.New<OrgContactItem>();
			orgContact3SecondaryMobile.OI_ContactItemType = OrgContactItemTypes.Codes.Phone;
			orgContact3SecondaryMobile.OI_OC = orgContact3.PK;
			orgContact3SecondaryMobile.OI_Description = PhoneContactItemDescriptionList.Codes.Mobile;
			orgContact3SecondaryMobile.OI_Address = "1234567";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Contact Mobile"];
			filter.Property = "1234567";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection2.Contains(org3));

			filter.Property = "3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection3.Contains(org3));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;

			OrgHeaderCollection orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection not to contain org1", !orgCollection4.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection4.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection4.Contains(org3));
		}

		public void TestOrgContactFax()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "XXXXXW";

			OrgContact orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_ContactName = "Postman Pechkin";
			orgContact1.OC_Fax = "1234567";

			OrgContact orgContact2 = org2.Contacts.AddNew();
			orgContact2.OC_ContactName = "Kot Matroskin";
			orgContact2.OC_Fax = "7654321";

			OrgContact orgContact3 = org3.Contacts.AddNew();
			orgContact3.OC_ContactName = "Andrew";
			orgContact3.OC_Fax = "99999999";
			OrgContactItem orgContact3SecondaryFax = Factory.New<OrgContactItem>();
			orgContact3SecondaryFax.OI_ContactItemType = OrgContactItemTypes.Codes.Phone;
			orgContact3SecondaryFax.OI_OC = orgContact3.PK;
			orgContact3SecondaryFax.OI_Description = PhoneContactItemDescriptionList.Codes.Fax;
			orgContact3SecondaryFax.OI_Address = "1234567";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Contact Fax"];
			filter.Property = "1234567";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection2.Contains(org3));

			filter.Property = "3";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection3.Contains(org3));

			filter.Property = "123";
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;

			OrgHeaderCollection orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection not to contain org1", !orgCollection4.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection4.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection4.Contains(org3));
		}

		public void TestOrgContactEmail()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "XXXXXW";

			OrgContact orgContact1 = org1.Contacts.AddNew();
			orgContact1.OC_ContactName = "Postman Pechkin";
			orgContact1.OC_Email = "alexander.korotun@cargowise.com";

			OrgContact orgContact2 = org2.Contacts.AddNew();
			orgContact2.OC_ContactName = "Kot Matroskin";
			orgContact2.OC_Email = "alexander.korotun@edi.com.au";

			OrgContact orgContact3 = org3.Contacts.AddNew();
			orgContact3.OC_ContactName = "Andrew";
			orgContact3.OC_Email = "andrew@wistechglobal.com";
			OrgContactItem orgContact3SecondaryEmail = Factory.New<OrgContactItem>();
			orgContact3SecondaryEmail.OI_ContactItemType = OrgContactItemTypes.Codes.Email;
			orgContact3SecondaryEmail.OI_OC = orgContact3.PK;
			orgContact3SecondaryEmail.OI_Address = "alex@test.com";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Contact Email"];
			filter.Property = "alexander.korotun@cargowise.com";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property = "alex";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection2.Contains(org3));

			filter.Property = "@";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection3.Contains(org3));

			filter.Property = "alex";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			OrgHeaderCollection orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection to not contain org1", !orgCollection4.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection4.Contains(org2));
			Assert("Expect collection to not contain org3", !orgCollection4.Contains(org3));
		}

		public void TestOrgContactSubgroup()
		{
			CombineAssertions("All text contact filters should not use the subgroup: ", () =>
			{
				AssertNull("Contact Email", FilterStripBizO["Contact Email"].SubGroup);
				AssertNull("Contact Fax", FilterStripBizO["Contact Fax"].SubGroup);
				AssertNull("Contact Work Phone", FilterStripBizO["Contact Work Phone"].SubGroup);
				AssertNull("Contact Mobile", FilterStripBizO["Contact Mobile"].SubGroup);
			});
		}

		public void TestOrgBranch()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "XXXZZZ";

			GlbBranch glbBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch1.GB_Code = "BR1";
			glbBranch1.GB_GC = GlbCompany.CurrentCompany.PK;

			OrgCompanyData orgCompanyData1 = org1.CompanyData;
			orgCompanyData1.OB_GB_ControllingBranch = glbBranch1.PK;

			GlbBranch glbBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch2.GB_Code = "BR2";
			glbBranch2.GB_GC = GlbCompany.CurrentCompany.PK;

			OrgCompanyData orgCompanyData2 = org2.CompanyData;
			orgCompanyData2.OB_GB_ControllingBranch = glbBranch2.PK;

			OrgCompanyData orgCompanyData3 = org3.CompanyData;
			orgCompanyData3.OB_GB_ControllingBranch = ZGuid.Empty;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Code"];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			filter = (ModuleTextFilter)FilterStripBizO["Branch"];
			filter.Property = "BR1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property = "BR";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection2.Contains(org3));

			filter.Property = "R";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection3.Contains(org3));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			OrgHeaderCollection orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection not to contain org1", !orgCollection4.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection4.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection4.Contains(org3));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			OrgHeaderCollection orgCollection5 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection5.Load();

			Assert("Expect collection to contain org1", orgCollection5.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection5.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection5.Contains(org3));
		}

		public void TestBranchManagementCodeFilter()
		{
			var codeCollection = new BranchManagementCodeDescriptionBoolCollection();
			codeCollection.Add("BRA", null, true);
			codeCollection.Add("BRB", null, true);
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, codeCollection);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_AccountingGroupCode = "BRA";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_AccountingGroupCode = "BRB";

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.CompanyData.OB_IsDebtor = ZBool.True;
			organisation1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			organisation1.CompanyData.OB_GB_ControllingBranch = branch1.PK;

			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.CompanyData.OB_IsDebtor = ZBool.True;
			organisation2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			organisation2.CompanyData.OB_GB_ControllingBranch = branch2.PK;

			Factory.Save();

			var branchManagementCodeFilter = (ModuleTextFilter)FilterStripBizO["Branch Management Code"];
			branchManagementCodeFilter.Property = "BRA";
			branchManagementCodeFilter.IsActive = true;
			var collection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain organisation1", new[] { organisation1 }, collection);

			branchManagementCodeFilter.Property = "BRB";
			collection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Collection should Contain organisation2", new[] { organisation2 }, collection);
		}

		public void TestOrgAddressDeliveryRoute()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var orgAddress1 = org1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Paris, just under the bridge of Alexander III.";

			var orgAddress2 = org2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Sydney, just under the Harbor bridge.";

			var orgAddress3 = org3.Addresses.AddNew();
			orgAddress3.OA_Address1 = "Sydney, just above the Harbor bridge.";

			orgAddress1.OA_DeliveryRoute = "ABC";
			orgAddress2.OA_DeliveryRoute = "ABC";
			orgAddress3.OA_DeliveryRoute = "XXX";
			orgAddress1.OA_DeliveryRouteSequence = 1;
			orgAddress2.OA_DeliveryRouteSequence = 2;
			orgAddress3.OA_DeliveryRouteSequence = 3;
			Factory.Save();

			var clearFactory = new BusinessObjectFactory();

			var filter = (ModuleTextFilter)FilterStripBizO["Delivery Route"];
			filter.IsActive = true;
			filter.Property = "ABC";

			var orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertContainsExactElementsInAnyOrder(orgCollection.Select(o => o.PK), new[] { org1.PK, org2.PK });
		}

		public void TestExclusiveGatewayServiceFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var exclusiveGatewayService = gatewayAgentPort.ExclusiveGatewayServices.AddNew();
			exclusiveGatewayService.O7_RS_NKGatewayService = "STD";
			exclusiveGatewayService.O7_RS_NKShipmentServiceLevel = "DIR";

			org1.AppointedGatewayAgentPorts.Add(gatewayAgentPort);

			Factory.Save();

			var clearFactory = new BusinessObjectFactory();

			var filter = (ModuleNkFilter)FilterStripBizO["Exclusive Gateway Service"];
			filter.IsActive = true;
			filter.Property = "STD";

			var orgCollection = new OrgHeaderCollection(clearFactory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertContainsExactElementsInAnyOrder(orgCollection.Select(o => o.PK), new[] { org1.PK });
		}

		public void TestExternalCreditorCodeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_APExternalCreditorCode = "TestCreditorCode1";
			org2.CompanyData.OB_APExternalCreditorCode = "OtherCode";
			Factory.Save();

			ApplyAndAssertTextFilter("External Creditor Code", "TestCreditorCode1", OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, org1.PK);
		}

		public void TestExternalDebtorCodeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_ARExternalDebtorCode = "TestDebtorCode1";
			org2.CompanyData.OB_ARExternalDebtorCode = "OtherCode";
			Factory.Save();

			ApplyAndAssertTextFilter("External Debtor Code", "TestDebtorCode1", OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, org1.PK);
		}

		public void TestExternalCreditorCodeFormCustomsRegNoFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("ExternalCreditorCode should be Empty", string.Empty, org.CompanyData.OB_APExternalCreditorCode);

			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ExternalCreditorAccountCode;
			orgCusCode.OK_RN_NKCodeCountry = Env.CurrentCompany.Country.Code;
			orgCusCode.OK_CustomsRegNo = "customsRegNo1";
			org.CustomsCodes.Add(orgCusCode);

			AssertEquals("ExternalCreditorCode should be set.", "customsRegNo1", org.CompanyData.OB_APExternalCreditorCode);
			Factory.Save();

			ApplyAndAssertTextFilter("External Creditor Code", "customsRegNo1", OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, org.PK);
		}

		public void TestExternalDebtorCodeFormCustomsRegNoFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("ExternalDebtorCode should be Empty", string.Empty, org.CompanyData.OB_ARExternalDebtorCode);

			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ExternalDebtorAccountCode;
			orgCusCode.OK_RN_NKCodeCountry = Env.CurrentCompany.Country.Code;
			orgCusCode.OK_CustomsRegNo = "customsRegNo1";
			org.CustomsCodes.Add(orgCusCode);

			AssertEquals("ExternalDebtorCode should be set.", "customsRegNo1", org.CompanyData.OB_ARExternalDebtorCode);
			Factory.Save();

			ApplyAndAssertTextFilter("External Debtor Code", "customsRegNo1", OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, org.PK);
		}

		void ApplyAndAssertTextFilter(string filterName, string searchValue, int expectedMaxLength, ZGuid expectedOrgPK)
		{
			var filter = (ModuleTextFilter)FilterStripBizO[filterName];
			filter.IsActive = true;
			filter.Property = searchValue;
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			AssertEquals(expectedMaxLength, filter.MaxLength);
			var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory(), FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals("Expected one matching result.", 1, orgCollection.Count);
			AssertContainsExactElementsInAnyOrder(orgCollection.Select(o => o.PK), new[] { expectedOrgPK });
		}

		#endregion

		#region Flags filters

		[StressTest]
		public void TestKnownApprovedShipperFilter_NonUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Macau))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.CountryData.OV_EXApprovedOrMajorExporter = "YES";

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.CountryData.OV_EXApprovedOrMajorExporter = "NO";

				Factory.Save();

				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
				AssertNotNull(filter);
				AssertNull(FilterStripBizO["TSA Known Shipper Status"]);

				filter.Property = "YES";
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals(true, orgCollection.Contains(org1));
				AssertEquals(false, orgCollection.Contains(org2));

				filter.Property = "NO";
				filter.IsActive = true;
				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals(false, orgCollection.Contains(org1));
				AssertEquals(true, orgCollection.Contains(org2));
			}
		}

		[StressTest]
		public void TestKnownApprovedShipperFilter_EU()
		{
			OrgHeader org1;
			OrgHeader org2;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				org1 = SetupOrgForSupplyChainSecurity("EU", "RA");
				org2 = SetupOrgForSupplyChainSecurity("EU", "NO");

				Factory.Save();

				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
				filter.Property = "RA";
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals(true, orgCollection.Contains(org1));
				AssertEquals(false, orgCollection.Contains(org2));

				filter.Property = "NO";
				filter.IsActive = true;

				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals(false, orgCollection.Contains(org1));
				AssertEquals(true, orgCollection.Contains(org2));
			}
		}

		[StressTest]
		public void TestKnownApprovedShipperFilter_EU_ValidInOtherEUCountry()
		{
			OrgHeader org1;
			OrgHeader org2;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				org1 = SetupOrgForSupplyChainSecurity("EU", "RA");
				org2 = SetupOrgForSupplyChainSecurity("EU", "NO");

				Factory.Save();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CH"))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
				filter.Property = "RA";
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals("EU scheme applies in Switzerland", true, orgCollection.Contains(org1));
				AssertEquals("Not RA", false, orgCollection.Contains(org2));

				filter.Property = "NO";
				filter.IsActive = true;

				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals("Has approval", false, orgCollection.Contains(org1));
				AssertEquals("EU scheme applies in Switzerland", true, orgCollection.Contains(org2));
			}
		}

		[StressTest]
		public void TestKnownApprovedShipperFilter_EU_ApprovalIsNotValidOutsideEU()
		{
			OrgHeader org1;
			OrgHeader org2;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				org1 = SetupOrgForSupplyChainSecurity("EU", "RA");
				org2 = SetupOrgForSupplyChainSecurity("EU", "NO");

				Factory.Save();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
				filter.Property = "RA";
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals("Approvals from the EU do not apply in Australia", false, orgCollection.Contains(org1));
				AssertEquals("Approvals from the EU do not apply in Australia", false, orgCollection.Contains(org2));

				filter.Property = "NO";
				filter.IsActive = true;

				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals("Approvals from the EU do not apply in Australia", true, orgCollection.Contains(org1));
				AssertEquals("Approvals from the EU do not apply in Australia", true, orgCollection.Contains(org2));
			}
		}

		OrgHeader SetupOrgForSupplyChainSecurity(string countryCode, string approvalCode)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CountryData.OV_RN_NKClientCountryRelation = countryCode;
			org.CountryData.OV_EXApprovedOrMajorExporter = approvalCode;
			org.CountryData.OV_OA_ApprovedLocation = org.MainAddress.PK;

			return org;
		}

		[StressTest]
		public void TestKnownApprovedShipperFilter_US()
		{
			string originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);

				var usa = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates);
				var jamaica = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Jamaica);

				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org1Address1 = org1.Addresses.AddNew();
				org1Address1.FillWithValidTestData();
				org1.CountryData.OV_EXApprovedOrMajorExporter = "YES";
				org1.CountryData.OV_OA_ApprovedLocation = org1Address1.PK;
				org1.CountryData.OV_RN_NKClientCountryRelation = usa.RN_Code;

				var org1CountryData2 = org1.CountryDataCollectionForThisCompany.AddNew();
				org1CountryData2.OV_EXApprovedOrMajorExporter = "NO";
				org1CountryData2.OV_OA_ApprovedLocation = org1.MainAddress.PK;
				org1CountryData2.OV_RN_NKClientCountryRelation = jamaica.RN_Code;

				var org1Address2 = org1.Addresses.AddNew();
				org1Address2.FillWithValidTestData();
				var org1CountryData3 = org1.CountryDataCollectionForThisCompany.AddNew();
				org1CountryData3.OV_EXApprovedOrMajorExporter = "YES";
				org1CountryData3.OV_OA_ApprovedLocation = org1Address2.PK;
				org1CountryData3.OV_RN_NKClientCountryRelation = jamaica.RN_Code;

				var org1CountryData4 = org1.CountryDataCollectionForThisCompany.AddNew();
				org1CountryData4.OV_EXApprovedOrMajorExporter = "NO";
				org1CountryData4.OV_RN_NKClientCountryRelation = usa.RN_Code;

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org2Address1 = org2.Addresses.AddNew();
				org2Address1.FillWithValidTestData();
				org2.CountryData.OV_EXApprovedOrMajorExporter = "YES";
				org2.CountryData.OV_OA_ApprovedLocation = org2Address1.PK;
				org2.CountryData.OV_RN_NKClientCountryRelation = usa.RN_Code;

				var org2CountryData2 = org2.CountryDataCollectionForThisCompany.AddNew();
				org2CountryData2.OV_EXApprovedOrMajorExporter = "YES";
				org2CountryData2.OV_OA_ApprovedLocation = org2.MainAddress.PK;

				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				var org3Address1 = org2.Addresses.AddNew();
				org3Address1.FillWithValidTestData();
				org3.CountryData.OV_EXApprovedOrMajorExporter = "NO";
				org3.CountryData.OV_OA_ApprovedLocation = org3Address1.PK;
				org3.CountryData.OV_RN_NKClientCountryRelation = usa.RN_Code;

				var org3CountryData2 = org3.CountryDataCollectionForThisCompany.AddNew();
				org3CountryData2.OV_EXApprovedOrMajorExporter = "NO";
				org3CountryData2.OV_OA_ApprovedLocation = org3.MainAddress.PK;

				var org4 = Factory.NewWithValidTestData<OrgHeader>();
				org4.CountryData.OV_EXApprovedOrMajorExporter = "NO";
				org4.CountryData.OV_RN_NKClientCountryRelation = usa.RN_Code;

				Factory.Save();

				var filter = (ModuleTextFilter)FilterStripBizO["TSA Known Shipper Status"];
				AssertNotNull(filter);
				AssertNull(FilterStripBizO["Known/Approved Status"]);

				filter.Property = "YES";
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals(true, orgCollection.Contains(org1));
				AssertEquals(true, orgCollection.Contains(org2));
				AssertEquals(false, orgCollection.Contains(org3));
				AssertEquals(false, orgCollection.Contains(org4));

				filter.Property = "NO";
				filter.IsActive = true;
				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals(false, orgCollection.Contains(org1));
				AssertEquals(false, orgCollection.Contains(org2));
				AssertEquals(true, orgCollection.Contains(org3));
				AssertEquals(true, orgCollection.Contains(org4));

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Jamaica);

				filter.Property = "NO";
				filter.IsActive = true;
				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals(true, orgCollection.Contains(org1));
				AssertEquals(true, orgCollection.Contains(org2));
				AssertEquals(true, orgCollection.Contains(org3));
				AssertEquals(true, orgCollection.Contains(org4));

				filter.Property = "YES";
				filter.IsActive = true;
				orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();
				AssertEquals(true, orgCollection.Contains(org1));
				AssertEquals(false, orgCollection.Contains(org2));
				AssertEquals(false, orgCollection.Contains(org3));
				AssertEquals(false, orgCollection.Contains(org4));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			}
		}

		public void TestKnownApprovedShipperFilterDescriptions_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["TSA Known Shipper Status"];
				var list = (CodeDescriptionPairList)filter.List;
				AssertEquals("Unknown Shipper", list["NO"].Description);
				AssertEquals("Known Shipper", list["YES"].Description);
			}
		}

		public void TestKnownApprovedShipperFilter_HK_Disabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
				AssertNotNull(filter);

				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("ALL"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("YES"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("NO"));
				AssertEquals(3, filter.List.Count);
			}
		}

		public void TestKnownApprovedShipperFilter_HK_Enabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
				AssertNotNull(filter);

				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("ALL"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("AC"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("KC"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("RA"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("NO"));
				AssertEquals(5, filter.List.Count);
			}
		}

		public void TestKnownApprovedShipperFilter_EU_Disabled()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				foreach (var countryCode in new[] { "IE", "PL", "CH" })
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
						AssertNotNull(filter);

						Assert(((CodeDescriptionPairList)filter.List).ContainsCode("ALL"));
						Assert(((CodeDescriptionPairList)filter.List).ContainsCode("YES"));
						Assert(((CodeDescriptionPairList)filter.List).ContainsCode("NO"));
						AssertEquals(3, filter.List.Count);
					}
				}
			}
		}

		public void TestKnownApprovedShipperFilter_EU_Enabled()
		{
			foreach (var countryCode in new[] { "IE", "PL", "CH" })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
					AssertNotNull(filter);

					Assert(((CodeDescriptionPairList)filter.List).ContainsCode("ALL"));
					Assert(((CodeDescriptionPairList)filter.List).ContainsCode("AC"));
					Assert(((CodeDescriptionPairList)filter.List).ContainsCode("AH"));
					Assert(((CodeDescriptionPairList)filter.List).ContainsCode("KC"));
					Assert(((CodeDescriptionPairList)filter.List).ContainsCode("RA"));
					Assert(((CodeDescriptionPairList)filter.List).ContainsCode("NO"));
					Assert(((CodeDescriptionPairList)filter.List).ContainsCode("CH"));
					AssertEquals(7, filter.List.Count);
				}
			}
		}

		public void TestKnownApprovedShipperFilter_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
				AssertNotNull(filter);

				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("ALL"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("RA"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("RE"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("AA"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("KC"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("RC"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("NO"));
				AssertEquals(7, filter.List.Count);
			}
		}

		public void TestKnownApprovedShipperFilter_SG()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["Known/Approved Status"];
				AssertNotNull(filter);

				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("ALL"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("KC"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("RA"));
				Assert(((CodeDescriptionPairList)filter.List).ContainsCode("NO"));
				AssertEquals(4, filter.List.Count);
			}
		}

		public void TestOrgSecondaryType()
		{
			//Secondary Type
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			// ARQualityAssured
			org1.OH_IsDebtor = true;
			org1.CompanyData.OB_ARQualityAssured = true;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Secondary Type"];
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.ARQualityAssured;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			// APQualityAssured
			org1.OH_IsCreditor = true;
			org1.CompanyData.OB_APQualityAssured = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.APQualityAssured;
			OrgHeaderCollection orgCollection1 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			Assert("Expect collection to contain org1", orgCollection1.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection1.Contains(org2));

			// CreditOnHold
			org1.OH_IsDebtor = true;
			org1.CompanyData.OB_AROnCreditHold = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.CreditOnHold;
			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection2.Contains(org2));

			// IncludedInAutoRateUpdate
			org1.OH_IsDebtor = true;
			org1.CompanyData.OB_ARAutoUpdateRates = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.IncludedInAutoRateUpdate;
			OrgHeaderCollection orgCollection3 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection3.Contains(org2));

			//NotIncludedInAutoRateUpdate
			org1.OH_IsDebtor = true;
			org1.CompanyData.OB_ARAutoUpdateRates = false;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.NotIncludedInAutoRateUpdate;
			OrgHeaderCollection orgCollection4 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection to contain org1", orgCollection4.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection4.Contains(org2));

			// LocalTransport
			org1.OH_IsShippingProvider = true;
			org1.OH_IsLocalTransport = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.LocalTransport;
			OrgHeaderCollection orgCollection5 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection5.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			// ShippingLine
			org1.OH_IsShippingProvider = true;
			org1.OH_IsShippingLine = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.ShippingLine;
			OrgHeaderCollection orgCollection6 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection6.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			// Airline
			org1.OH_IsShippingProvider = true;
			org1.OH_IsAirLine = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.Airline;
			OrgHeaderCollection orgCollection7 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection7.Load();

			Assert("Expect collection to contain org1", orgCollection7.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection7.Contains(org2));

			// Rail
			org1.OH_IsShippingProvider = true;
			org1.OH_IsRailProvider = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.Rail;
			OrgHeaderCollection orgCollection8 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection8.Load();

			Assert("Expect collection to contain org1", orgCollection8.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection8.Contains(org2));

			// AirWholesaler
			org1.OH_IsShippingProvider = true;
			org1.OH_IsAirWholesaler = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.AirWholesaler;
			OrgHeaderCollection orgCollection9 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection9.Load();

			Assert("Expect collection to contain org1", orgCollection9.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection9.Contains(org2));

			// SeaWholesaler
			org1.OH_IsShippingProvider = true;
			org1.OH_IsSeaWholesaler = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.SeaWholesaler;
			OrgHeaderCollection orgCollection10 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection10.Load();

			Assert("Expect collection to contain org1", orgCollection10.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection10.Contains(org2));

			// LineHaul
			org1.OH_IsShippingProvider = true;
			org1.OH_IsLineHaulProvider = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.LineHaul;
			OrgHeaderCollection orgCollection11 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection11.Load();

			Assert("Expect collection to contain org1", orgCollection11.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection11.Contains(org2));

			// VesselConsortium
			org1.OH_IsShippingProvider = true;
			org1.OH_IsShippingConsortium = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.VesselConsortium;
			OrgHeaderCollection orgCollection12 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection12.Load();

			Assert("Expect collection to contain org1", orgCollection12.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection12.Contains(org2));

			// Principal
			org1.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.Principal;
			OrgHeaderCollection orgCollection13 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection13.Load();

			Assert("Expect collection to contain org1", orgCollection13.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection13.Contains(org2));

			//HandlesAirFreight

			OrgAppointedAgentPorts airAppointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			airAppointedAgentPorts.O5_AirAgentStatus = AgentStatusList.Codes.Handles;

			org1.AppointedAgentPorts.Add(airAppointedAgentPorts);
			Factory.Save();

			OrgHeaderCollection orgCollection14 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection14.Load();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.HandlesAirFreight;

			Assert("Expect collection to contain org1", orgCollection14.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection14.Contains(org2));

			// HandlesSeaFreight
			OrgAppointedAgentPorts seaAppointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			seaAppointedAgentPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			org1.OH_IsForwarder = true;
			org1.AppointedAgentPorts.Add(seaAppointedAgentPorts);

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.HandlesSeaFreight;
			OrgHeaderCollection orgCollection15 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection15.Load();

			Assert("Expect collection to contain org1", orgCollection15.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection15.Contains(org2));

			// HandlesRoadFreight
			OrgAppointedAgentPorts roadAppointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			roadAppointedAgentPorts.O5_RoadAgentStatus = AgentStatusList.Codes.Handles;
			org1.OH_IsForwarder = true;
			org1.AppointedAgentPorts.Add(roadAppointedAgentPorts);

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.HandlesRoadFreight;
			OrgHeaderCollection orgCollection16 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection16.Load();

			Assert("Expect collection to contain org1", orgCollection16.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection16.Contains(org2));

			// HandlesRailFreight
			OrgAppointedAgentPorts railAppointedAgentPorts = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			railAppointedAgentPorts.O5_RailAgentStatus = AgentStatusList.Codes.Handles;
			org1.OH_IsForwarder = true;
			org1.AppointedAgentPorts.Add(railAppointedAgentPorts);

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.HandlesRailFreight;
			OrgHeaderCollection orgCollection17 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection17.Load();

			Assert("Expect collection to contain org1", orgCollection17.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection17.Contains(org2));

			// Depot
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsUnpackDepot = true;
			org1.OH_IsPackDepot = false;

			org2.OH_IsUnpackDepot = false;
			org2.OH_IsPackDepot = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.Depot;
			OrgHeaderCollection orgCollection18 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection18.Load();

			Assert("Expect collection to contain org1", orgCollection18.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection18.Contains(org2));

			// PackingDepot
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsUnpackDepot = false;
			org1.OH_IsPackDepot = true;

			org2.OH_IsUnpackDepot = true;
			org2.OH_IsPackDepot = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.PackingDepot;
			OrgHeaderCollection orgCollection19 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection19.Load();

			Assert("Expect collection to contain org1", orgCollection19.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection19.Contains(org2));

			// UnpackingDepot
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsUnpackDepot = true;
			org1.OH_IsPackDepot = false;

			org2.OH_IsUnpackDepot = false;
			org2.OH_IsPackDepot = true;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.UnpackingDepot;
			OrgHeaderCollection orgCollection20 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection20.Load();

			Assert("Expect collection to contain org1", orgCollection20.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection20.Contains(org2));

			// CTO
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsAirCTO = true;
			org1.OH_IsSeaCTO = false;

			org2.OH_IsAirCTO = false;
			org2.OH_IsSeaCTO = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.CTO;
			OrgHeaderCollection orgCollection21 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection21.Load();

			Assert("Expect collection to contain org1", orgCollection21.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection21.Contains(org2));

			// AirCTO
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsAirCTO = true;
			org1.OH_IsSeaCTO = false;

			org2.OH_IsAirCTO = false;
			org2.OH_IsSeaCTO = true;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.AirCTO;
			OrgHeaderCollection orgCollection22 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection22.Load();

			Assert("Expect collection to contain org1", orgCollection22.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection22.Contains(org2));

			// SeaCTO
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsAirCTO = false;
			org1.OH_IsSeaCTO = true;

			org2.OH_IsAirCTO = true;
			org2.OH_IsSeaCTO = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.SeaCTO;
			OrgHeaderCollection orgCollection23 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection23.Load();

			Assert("Expect collection to contain org1", orgCollection23.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection23.Contains(org2));

			// RoadDepotTransitShed
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsRoadFreightDepot = true;

			org2.OH_IsMiscFreightServices = true;
			org2.OH_IsRoadFreightDepot = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.RoadDepotTransitShed;
			OrgHeaderCollection orgCollectionRoad = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollectionRoad.Load();

			Assert("Expect collection to contain org1", orgCollectionRoad.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollectionRoad.Contains(org2));

			// RailHeadDepot
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsRailHead = true;

			org2.OH_IsMiscFreightServices = true;
			org2.OH_IsRailHead = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.RailHeadDepot;
			OrgHeaderCollection orgCollectionRail = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollectionRail.Load();

			Assert("Expect collection to contain org1", orgCollectionRail.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollectionRail.Contains(org2));

			// ContainerYard
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsContainerYard = true;

			org2.OH_IsMiscFreightServices = true;
			org2.OH_IsContainerYard = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.ContainerYard;
			OrgHeaderCollection orgCollection24 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection24.Load();

			Assert("Expect collection to contain org1", orgCollection24.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection24.Contains(org2));

			// FumigationContractor
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsFumigationContractor = true;

			org2.OH_IsMiscFreightServices = true;
			org2.OH_IsFumigationContractor = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.FumigationContractor;
			OrgHeaderCollection orgCollection25 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection25.Load();

			Assert("Expect collection to contain org1", orgCollection25.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection25.Contains(org2));

			// ContainerLeasingCompany
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsContainerLeasingCompany = true;

			org2.OH_IsMiscFreightServices = true;
			org2.OH_IsContainerLeasingCompany = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.ContainerLeasingCompany;
			OrgHeaderCollection orgCollection26 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection26.Load();

			Assert("Expect collection to contain org1", orgCollection26.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection26.Contains(org2));

			// InlandWaterway
			org1.OH_IsShippingProvider = true;
			org1.OH_IsInlandWaterwayProvider = true;

			Factory.Save();
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.InlandWaterway;
			OrgHeaderCollection orgCollection27 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection27.Load();

			Assert("Expect collection to contain org1", orgCollection27.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection27.Contains(org2));

			// VGM Contractor
			org1.OH_IsMiscFreightServices = true;
			org1.OH_IsVGMContractor = true;

			org2.OH_IsMiscFreightServices = true;
			org2.OH_IsVGMContractor = false;

			Factory.Save();

			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.VGMContractor;
			OrgHeaderCollection orgCollection28 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection28.Load();

			Assert("Expect collection to contain org1", orgCollection28.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection28.Contains(org2));

			// Sales User Flags
			foreach (var userFlagType in OrgUserFlagType.All)
			{
				AssertUserFlag(userFlagType.OrgHeaderColumn.Name, "Sales - " + userFlagType.Label);
			}
		}

		public void TestOrgSecondaryType_FerryWaterTerminal()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "OHCODEORG1";
			org1.OH_IsMiscFreightServices = true;
			org2.OH_Code = "OHCODEORG2";
			org2.OH_IsMiscFreightServices = true;
			org3.OH_Code = "OHCODEORG3";
			org3.OH_IsMiscFreightServices = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Secondary Type"];
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.FerryWaterTerminal;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("The count of collection should be zero", 0, orgCollection.Count);

			org1.OH_IsFerryWaterTerminal = true;
			org3.OH_IsFerryWaterTerminal = true;
			Factory.Save();

			orgCollection.Load();

			AssertEquals("The count of collection should be two", 2, orgCollection.Count);
			AssertCollectionContains("The collection should have org1", org1, orgCollection);
			AssertCollectionContains("The collection should have org3", org3, orgCollection);
		}

		public void TestOrgSecondaryType_CTO()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var org5 = Factory.NewWithValidTestData<OrgHeader>();
			var org6 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "OHCODEORG1";
			org1.OH_IsMiscFreightServices = true;
			org2.OH_Code = "OHCODEORG2";
			org2.OH_IsMiscFreightServices = true;
			org3.OH_Code = "OHCODEORG3";
			org3.OH_IsMiscFreightServices = true;
			org4.OH_Code = "OHCODEORG4";
			org4.OH_IsMiscFreightServices = true;
			org5.OH_Code = "OHCODEORG5";
			org5.OH_IsMiscFreightServices = true;
			org6.OH_Code = "OHCODEORG6";
			org6.OH_IsMiscFreightServices = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Secondary Type"];
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.CTO;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("The count of collection should be zero", 0, orgCollection.Count);

			org1.OH_IsSeaCTO = true;
			org2.OH_IsAirCTO = true;
			org3.OH_IsFerryWaterTerminal = true;
			org4.OH_IsRoadFreightDepot = true;
			org5.OH_IsRailHead = true;
			Factory.Save();

			orgCollection.Load();

			AssertEquals("The count of collection should be five", 5, orgCollection.Count);
			AssertCollectionNotContains("The collection should not have org6", org6, orgCollection);

			org1.OH_IsSeaCTO = false;
			org6.OH_IsSeaCTO = true;
			org6.OH_IsFerryWaterTerminal = true;
			Factory.Save();

			orgCollection.Load();

			AssertEquals("The count of collection should be five", 5, orgCollection.Count);
			AssertCollectionNotContains("The collection should not have org1", org1, orgCollection);
		}

		public void TestOrgSecondaryType_ContainerLeasingCompany()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "OHCODEORG1";
			org1.OH_IsMiscFreightServices = true;
			org2.OH_Code = "OHCODEORG2";
			org2.OH_IsMiscFreightServices = true;
			org3.OH_Code = "OHCODEORG3";
			org3.OH_IsMiscFreightServices = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Secondary Type"];
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.ContainerLeasingCompany;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("The count of collection should be zero", 0, orgCollection.Count);

			org1.OH_IsContainerLeasingCompany = true;
			org3.OH_IsContainerLeasingCompany = true;
			Factory.Save();

			orgCollection.Load();

			AssertEquals("The count of collection should be two", 2, orgCollection.Count);
			AssertCollectionContains("The collection should have org1", org1, orgCollection);
			AssertCollectionContains("The collection should have org3", org3, orgCollection);
		}

		public void TestOrgSecondaryType_VGMContractor()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "OHCODEORG1";
			org1.OH_IsMiscFreightServices = true;
			org2.OH_Code = "OHCODEORG2";
			org2.OH_IsMiscFreightServices = true;
			org3.OH_Code = "OHCODEORG3";
			org3.OH_IsMiscFreightServices = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Secondary Type"];
			filter.Property = OrgConstants.FilterControl.SecondaryOrgType.VGMContractor;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals("The count of collection should be zero", 0, orgCollection.Count);

			org1.OH_IsVGMContractor = true;
			org3.OH_IsVGMContractor = true;
			Factory.Save();

			orgCollection.Load();

			AssertEquals("The count of collection should be two", 2, orgCollection.Count);
			AssertCollectionContains("The collection should have org1", org1, orgCollection);
			AssertCollectionContains("The collection should have org3", org3, orgCollection);
		}

		void AssertUserFlag(string columnName, string filterProperty)
		{
			//Secondary Type
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org1[columnName] = true;

			org1.OH_IsSalesLead = true;
			org2[columnName] = false;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Secondary Type"];
			filter.Property = filterProperty;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			collection.Load();

			Assert("Expect collection to contain org1", collection.Contains(org1));
			Assert("Expect collection not to contain org2", !collection.Contains(org2));
		}

		public void TestMarketingOptions()
		{
			foreach (var userFlagType in OrgUserFlagType.All)
			{
				AssertMarketingOption(userFlagType.OrgHeaderColumn.Name, userFlagType.Label);
			}
		}

		void AssertMarketingOption(string columnName, string filterProperty)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsSalesLead = true;
			org1[columnName] = true;

			org2.OH_IsSalesLead = true;
			org2[columnName] = false;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Marketing Options"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = filterProperty;

			var collection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			collection.Load();
			AssertCollectionContains(org1, collection);
			AssertCollectionNotContains(org2, collection);
		}

		public void TestMarketingOptions_OnlyIncludedForStandardAndClientIntelligence()
		{
			AssertFilterExists(OrgModuleType.Standard, "Marketing Options", true);
			AssertFilterExists(OrgModuleType.ClientIntelligence, "Marketing Options", true);
			AssertFilterExists(OrgModuleType.CompetitorIntelligence, "Marketing Options", false);
		}

		[StressTest]
		public void TestOrgCategory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Category = "BUS";
			org2.OH_Category = "GOV";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Category"];
			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).Category_List["BUS"].Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).Category_List["GOV"].Code;
			OrgHeaderCollection orgCollection1 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			Assert("Expect collection to contain org2", orgCollection1.Contains(org2));
			Assert("Expect collection not to contain org1", !orgCollection1.Contains(org1));
		}

		[StressTest]
		public void TestOrgLanguage()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Language = Core.SharedConstants.Languages.French;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgAddress address1 = org1.Addresses.AddNew();
			address1.OA_Address1 = "Who cares?";
			address1.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

			OrgAddress address2 = org2.Addresses.AddNew();
			address2.OA_Address1 = "Nobody cares.";
			address2.OA_Language = Core.SharedConstants.Languages.French;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Language"];
			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).Language_List[Core.SharedConstants.Languages.French].Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		[StressTest]
		public void TestAccountType()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYY1";
			org2.OH_Code = "XXXYY2";
			org3.OH_Code = "XXXYY3";

			org1.OH_IsGlobalAccount = true;
			org1.OH_IsNationalAccount = true;
			org1.OH_IsTempAccount = false;

			org2.OH_IsGlobalAccount = true;
			org2.OH_IsNationalAccount = false;
			org2.OH_IsTempAccount = true;

			org3.OH_IsGlobalAccount = false;
			org3.OH_IsNationalAccount = true;
			org3.OH_IsTempAccount = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Organization – Account Type"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			AssertAccountTypeFilter(filter, org1, org2, org3, OrgConstants.FilterControl.AccountType.Code.GlobalAccount, true, true, false);
			AssertAccountTypeFilter(filter, org1, org2, org3, OrgConstants.FilterControl.AccountType.Code.NonGlobalAccount, false, false, true);
			AssertAccountTypeFilter(filter, org1, org2, org3, OrgConstants.FilterControl.AccountType.Code.NationalAccount, true, false, true);
			AssertAccountTypeFilter(filter, org1, org2, org3, OrgConstants.FilterControl.AccountType.Code.NonNationalAccount, false, true, false);
			AssertAccountTypeFilter(filter, org1, org2, org3, OrgConstants.FilterControl.AccountType.Code.TemporaryAccount, false, true, true);
			AssertAccountTypeFilter(filter, org1, org2, org3, OrgConstants.FilterControl.AccountType.Code.NonTemporaryAccount, true, false, false);
		}

		void AssertAccountTypeFilter(ModuleTextFilter filter, OrgHeader org1, OrgHeader org2, OrgHeader org3, string filterCode, bool shouldContainOrg1, bool shouldContainOrg2, bool shouldContainOrg3)
		{
			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).AccountType_List[filterCode].Code;
			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert($"Expect collection{(!shouldContainOrg1 ? " not" : string.Empty)} to contain org1", shouldContainOrg1 == orgCollection.Contains(org1));
			Assert($"Expect collection{(!shouldContainOrg2 ? " not" : string.Empty)} to contain org2", shouldContainOrg2 == orgCollection.Contains(org2));
			Assert($"Expect collection{(!shouldContainOrg3 ? " not" : string.Empty)} to contain org3", shouldContainOrg3 == orgCollection.Contains(org3));
		}

		public void TestCreditNotYetApproved()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "YYYZZZ";
			org4.OH_Code = "ZZZZZZ";

			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.OB_ARCreditApproved = true;
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.OB_ARCreditApproved = false;

			org3.CompanyData.OB_IsDebtor = false;
			org3.CompanyData.OB_ARCreditApproved = true;
			org4.CompanyData.OB_IsDebtor = false;
			org4.CompanyData.OB_ARCreditApproved = false;

			Factory.Save();

			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO["Credit Not Yet Approved"];
			filter["Credit Not Yet Approved"] = true;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
			Assert("Expect collection not to contain org4", !orgCollection.Contains(org4));

			filter["Credit Not Yet Approved"] = false;
			OrgHeaderCollection orgCollection1 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			Assert("Expect collection to contain org1", orgCollection1.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection1.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection1.Contains(org3));
			Assert("Expect collection not to contain org4", !orgCollection1.Contains(org4));
		}

		void AssertOrgHeaderCollection(string department, string filterName)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			GlbStaff staff = factory.NewWithValidTestData<GlbStaff>();

			factory.Save();

			OrgStaffAssignments assign = org.StaffAssignments.AddNew();
			assign.O8_GS_NKPersonResponsible = staff.GS_Code;
			assign.O8_GC = ZGuid.Empty;
			assign.O8_Role = "SAL";
			assign.O8_Department = department;

			factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[filterName];

			filter.Property = staff.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org", orgCollection.Contains(org));
		}

		public void TestSalesImportAirRepresentativeFilter()
		{
			AssertOrgHeaderCollection("FIA", "Sales Import Air Representative");
		}

		public void TestSalesImportAirRepresentativeFilterHavingDataWhenDepartmentIsAll()
		{
			AssertOrgHeaderCollection("ALL", "Sales Import Air Representative");
		}

		public void TestSalesImportSeaRepresentativeFilter()
		{
			AssertOrgHeaderCollection("FIS", "Sales Import Sea Representative");
		}

		public void TestSalesImportSeaRepresentativeFilterHavingDataWhenDepartmentIsAll()
		{
			AssertOrgHeaderCollection("ALL", "Sales Import Sea Representative");
		}

		public void TestSalesExportAirRepresentativeFilter()
		{
			AssertOrgHeaderCollection("FEA", "Sales Export Air Representative");
		}

		public void TestSalesExportAirRepresentativeFilterHavingDataWhenDepartmentIsAll()
		{
			AssertOrgHeaderCollection("ALL", "Sales Export Air Representative");
		}

		public void TestSalesExportSeeRepresentativeFilter()
		{
			AssertOrgHeaderCollection("FES", "Sales Export Sea Representative");
		}

		public void TestSalesExportSeeRepresentativeFilterHavingDataWhenDepartmentIsAll()
		{
			AssertOrgHeaderCollection("ALL", "Sales Export Sea Representative");
		}

		public void TestSalesWarehousingRepresentativeFilter()
		{
			AssertOrgHeaderCollection("WAR", "Sales Warehousing Representative");
		}

		public void TestSalesWarehousingRepresentativeFilterHavingDataWhenDepartmentIsAll()
		{
			AssertOrgHeaderCollection("ALL", "Sales Warehousing Representative");
		}

		[StressTest]
		public void TestSalesRepAssigned()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader org1 = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = factory.NewWithValidTestData<OrgHeader>();

			GlbStaff staff1 = factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();

			OrgStaffAssignments assign1 = org1.StaffAssignments.AddNew();
			assign1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assign1.O8_GC = ZGuid.Empty;
			assign1.O8_Role = "SAL";
			factory.Save();

			OrgStaffAssignments assign2 = org2.StaffAssignments.AddNew();
			assign2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assign2.O8_Role = "SAL";
			factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Sales Rep Assigned"];

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).SalesRepAssigned_List[OrgConstants.FilterControl.SalesRepAssigned.Code.Assigned].Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).SalesRepAssigned_List[OrgConstants.FilterControl.SalesRepAssigned.Code.NotAssigned].Code;

			OrgHeaderCollection orgCollection1 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			Assert("Expect collection not to contain org1", !orgCollection1.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection1.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection1.Contains(org3));

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).SalesRepAssigned_List[OrgConstants.FilterControl.SalesRepAssigned.Code.All].Code;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection2.Contains(org3));
		}

		[StressTest]
		public void TestRatesSecurity()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			OrgHeader org1 = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = factory.NewWithValidTestData<OrgHeader>();

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("111", "Rate Security 1"));
			list.Add(new CodeDescriptionPair("222", "Rate Security 2"));

			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			factory.Save();

			org1.CompanyData.OB_RateSecurityGroup = "111";
			org2.CompanyData.OB_RateSecurityGroup = "222";

			factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Rates' Security"];

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).RatesSecurityList[OrgConstants.FilterControl.RatesSecurity.Code.All].Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));

			filter.Property = "111";

			OrgHeaderCollection orgCollection1 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			Assert("Expect collectionto contain org1", orgCollection1.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection1.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection1.Contains(org3));

			filter.Property = "222";

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to not contain org1", !orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));
			Assert("Expect collection to not contain org3", !orgCollection2.Contains(org3));
		}

		[StressTest]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestExternalValidationStatus()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			var org3 = factory.NewWithValidTestData<OrgHeader>();
			var org4 = factory.NewWithValidTestData<OrgHeader>();
			var org5 = factory.NewWithValidTestData<OrgHeader>();

			org1.Logs.AddNew(AutoEvents.ExternalValidationPassed);
			org2.Logs.AddNew(AutoEvents.ExternalValidationFailed);
			org4.Logs.AddNew(AutoEvents.ExternalValidationNotCompleted);

			factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["External Validation Status"];

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).ExternalValidationStatusList[OrgConstants.FilterControl.ExternalValidationStatus.Code.Passed].Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
			Assert("Expect collection not to contain org4", !orgCollection.Contains(org4));
			Assert("Expect collection not to contain org5", !orgCollection.Contains(org5));

			filter.Property = "";

			var orgCollection2 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			Assert("Expect collection to contain org1", orgCollection2.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection2.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection2.Contains(org3));
			Assert("Expect collection to contain org4", orgCollection2.Contains(org4));
			Assert("Expect collection to contain org5", orgCollection2.Contains(org5));

			filter.Property = "XXX";

			var orgCollection3 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection3.Load();

			Assert("Expect collection to contain org1", orgCollection3.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection3.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection3.Contains(org3));
			Assert("Expect collection to contain org4", orgCollection3.Contains(org4));
			Assert("Expect collection to contain org5", orgCollection3.Contains(org5));

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).ExternalValidationStatusList[OrgConstants.FilterControl.ExternalValidationStatus.Code.Failed].Code;

			var orgCollection4 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection4.Load();

			Assert("Expect collection to not contain org1", !orgCollection4.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection4.Contains(org2));
			Assert("Expect collection to not contain org3", !orgCollection4.Contains(org3));
			Assert("Expect collection to not contain org4", !orgCollection4.Contains(org4));
			Assert("Expect collection to not contain org5", !orgCollection4.Contains(org5));

			org1.Logs.AddNew(AutoEvents.ExternalValidationPassed);
			org1.Logs.AddNew(AutoEvents.ExternalValidationFailed);
			org1.Logs.AddNew(AutoEvents.ExternalValidationNotCompleted);
			org1.Logs.AddNew(AutoEvents.ExternalValidationPassed);

			org2.Logs.AddNew(AutoEvents.ExternalValidationFailed);
			org2.Logs.AddNew(AutoEvents.ExternalValidationFailed);
			org2.Logs.AddNew(AutoEvents.ExternalValidationPassed);

			org3.Logs.AddNew(AutoEvents.ExternalValidationPassed);
			org3.Logs.AddNew(AutoEvents.ExternalValidationPassed);
			org3.Logs.AddNew(AutoEvents.ExternalValidationPassed);
			org3.Logs.AddNew(AutoEvents.ExternalValidationFailed);

			org4.Logs.AddNew(AutoEvents.ExternalValidationNotCompleted);
			org4.Logs.AddNew(AutoEvents.ExternalValidationPassed);
			org4.Logs.AddNew(AutoEvents.ExternalValidationNotCompleted);

			factory.Save();

			var orgCollection5 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection5.Load();

			Assert("Expect collection to not contain org1", !orgCollection5.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection5.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection5.Contains(org3));
			Assert("Expect collection to not contain org4", !orgCollection5.Contains(org4));
			Assert("Expect collection to not contain org4", !orgCollection5.Contains(org5));

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).ExternalValidationStatusList[OrgConstants.FilterControl.ExternalValidationStatus.Code.Passed].Code;

			var orgCollection6 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection6.Load();

			Assert("Expect collection to contain org1", orgCollection6.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection6.Contains(org2));
			Assert("Expect collection to not contain org3", !orgCollection6.Contains(org3));
			Assert("Expect collection to not contain org4", !orgCollection6.Contains(org4));
			Assert("Expect collection to not contain org5", !orgCollection6.Contains(org5));

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).ExternalValidationStatusList[OrgConstants.FilterControl.ExternalValidationStatus.Code.NotCompleted].Code;

			var orgCollection7 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection7.Load();

			Assert("Expect collection to not contain org1", !orgCollection7.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection7.Contains(org2));
			Assert("Expect collection to not contain org3", !orgCollection7.Contains(org3));
			Assert("Expect collection to contain org4", orgCollection7.Contains(org4));
			Assert("Expect collection to not contain org5", !orgCollection7.Contains(org5));

			filter.Property = ((OrganisationFilterBusinessObject)FilterStripBizO).ExternalValidationStatusList[OrgConstants.FilterControl.ExternalValidationStatus.Code.NotRun].Code;

			var orgCollection8 = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			orgCollection8.Load();

			Assert("Expect collection to not contain org1", !orgCollection8.Contains(org1));
			Assert("Expect collection to not contain org2", !orgCollection8.Contains(org2));
			Assert("Expect collection to not contain org3", !orgCollection8.Contains(org3));
			Assert("Expect collection to not contain org4", !orgCollection8.Contains(org3));
			Assert("Expect collection to contain org5", orgCollection8.Contains(org5));
		}

		[StressTest]
		public void TestAccountingTransaction()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			//Organisation 1 has one open transaction
			OrgHeader organisation1 = factory.NewWithValidTestData<OrgHeader>();
			AccTransactionHeader transaction1 = factory.NewWithValidTestData<AccTransactionHeader>();
			transaction1.AH_OH = organisation1.PK;
			transaction1.AH_Ledger = "AR";
			transaction1.AH_TransactionType = "INV";

			//Organisation 2 has one open and one closed transactions
			OrgHeader organisation2 = factory.NewWithValidTestData<OrgHeader>();
			AccTransactionHeader transaction2 = factory.NewWithValidTestData<AccTransactionHeader>();
			transaction2.AH_OH = organisation2.PK;
			transaction2.AH_Ledger = "AP";
			transaction2.AH_TransactionType = "CRD";

			AccTransactionHeader transaction3 = factory.NewWithValidTestData<AccTransactionHeader>();
			transaction3.AH_OH = organisation2.PK;
			transaction3.AH_Ledger = "AP";
			transaction3.AH_TransactionType = "CRD";
			transaction3.AH_FullyPaidDate = new ZDateTime(2008, 3, 1);

			//Organisation 3 has all closed transactions
			OrgHeader organisation3 = factory.NewWithValidTestData<OrgHeader>();
			AccTransactionHeader transaction4 = factory.NewWithValidTestData<AccTransactionHeader>();
			transaction4.AH_OH = organisation3.PK;
			transaction4.AH_Ledger = "AP";
			transaction4.AH_TransactionType = "CRD";
			transaction4.AH_FullyPaidDate = new ZDateTime(2008, 3, 1);

			AccTransactionHeader transaction5 = factory.NewWithValidTestData<AccTransactionHeader>();
			transaction5.AH_OH = organisation3.PK;
			transaction5.AH_Ledger = "AR";
			transaction5.AH_TransactionType = "ADJ";
			transaction5.AH_FullyPaidDate = new ZDateTime(2008, 4, 12);

			//Organisations without transactions
			OrgHeader organisation4 = factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation5 = factory.NewWithValidTestData<OrgHeader>();

			//Organisation 6 has one open transaction
			OrgHeader organisation6 = factory.NewWithValidTestData<OrgHeader>();
			AccTransactionHeader transaction6 = factory.NewWithValidTestData<AccTransactionHeader>();
			transaction6.AH_OH = organisation6.PK;
			transaction6.AH_Ledger = "AR";
			transaction6.AH_TransactionType = "JNL";

			//Organisation 7 has one open transaction
			OrgHeader organisation7 = factory.NewWithValidTestData<OrgHeader>();
			AccTransactionHeader transaction7 = factory.NewWithValidTestData<AccTransactionHeader>();
			transaction7.AH_OH = organisation7.PK;
			transaction7.AH_Ledger = "AR";

			factory.Save();
			string flagName = "With Outstanding Transactions in any Company";
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStripBizO["Accounting Transactions - Debtor"];
			filter.DefaultProperties[flagName] = true;
			filter.IsActive = true;
			OrgHeaderCollection organisations = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			organisations.Load();

			AssertEquals("Should be three organisations in collection", 3, organisations.Count);
			Assert("Organisation 1 with one open transaction should be selected", organisations.Contains(organisation1));
			Assert("Organisation 2 with one open and one closed transactions should not be selected because open transaction is AP", !organisations.Contains(organisation2));
			Assert("Organisation 3 with all closed transaction should NOT be selected", !organisations.Contains(organisation3));
			Assert("Organisation 4 with no transactions should NOT be selected", !organisations.Contains(organisation4));
			Assert("Organisation 5 with no transactions should NOT be selected", !organisations.Contains(organisation5));
			Assert("Organisation 6 with one open transaction should be selected", organisations.Contains(organisation6));
			Assert("Organisation 7 with one open transaction should be selected", organisations.Contains(organisation7));

			filter.DefaultProperties[flagName] = false;
			organisations = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			organisations.Load();

			// Filter is active with false value
			Assert("Organisation 1 should not be in collection", !organisations.Contains(organisation1));
			Assert("Organisation 2 should be in collection", organisations.Contains(organisation2));
			Assert("Organisation 3 should be in collection", organisations.Contains(organisation3));
			Assert("Organisation 4 should be in collection", organisations.Contains(organisation4));
			Assert("Organisation 5 should be in collection", organisations.Contains(organisation5));
			Assert("Organisation 6 should not be in collection", !organisations.Contains(organisation6));
			Assert("Organisation 7 should not be in collection", !organisations.Contains(organisation7));

			filter.IsActive = false;

			filter = (ModuleFlagsFilter)FilterStripBizO["Accounting Transactions - Creditor"];
			filter.DefaultProperties[flagName] = true;
			filter.IsActive = true;
			organisations = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			organisations.Load();

			AssertEquals("Should be one organisations in collection", 1, organisations.Count);
			Assert("Organisation 1 with one open transaction should not selected", !organisations.Contains(organisation1));
			Assert("Organisation 2 with one open and one closed transactions should be selected", organisations.Contains(organisation2));
			Assert("Organisation 3 with all closed transaction should NOT be selected", !organisations.Contains(organisation3));
			Assert("Organisation 4 with no transactions should NOT be selected", !organisations.Contains(organisation4));
			Assert("Organisation 5 with no transactions should NOT be selected", !organisations.Contains(organisation5));
			Assert("Organisation 6 with one open transaction should not be selected as it is AP", !organisations.Contains(organisation6));
			Assert("Organisation 7 with one open transaction should not be selected as it is AP", !organisations.Contains(organisation7));

			filter.DefaultProperties[flagName] = false;
			organisations = new OrgHeaderCollection(factory, FilterStripBizO.Filter);
			organisations.Load();

			// Filter is active with false value
			Assert("Organisation 1 should be in collection", organisations.Contains(organisation1));
			Assert("Organisation 2 should not be in collection", !organisations.Contains(organisation2));
			Assert("Organisation 3 should be in collection", organisations.Contains(organisation3));
			Assert("Organisation 4 should be in collection", organisations.Contains(organisation4));
			Assert("Organisation 5 should be in collection", organisations.Contains(organisation5));
			Assert("Organisation 6 should be in collection", organisations.Contains(organisation6));
			Assert("Organisation 7 should be in collection", organisations.Contains(organisation7));
		}

		[StressTest]
		public void TestIsForeignOperator()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			var foreignOperator = (BusinessObject)Factory.New<ICusBRForeignOperator>();
			foreignOperator[CusBRForeignOperatorSchema.BFR_OH_ForeignOperator] = org1.PK;
			foreignOperator[CusBRForeignOperatorSchema.BFR_OH_Owner] = org2.PK;

			Factory.Save();

			var dataRegistryMock = new Mock<IBRCustomsDataRegistry>();
			using (ObjectFactory.Substitute(dataRegistryMock.Object))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Brazil))
				{
					dataRegistryMock.Setup(m => m.EnableForeignOperator).Returns(false);
					AssertNull((ModuleTextFilter)FilterStripBizO["Is Foreign Operator"]);

					FilterStripBizO.ResetModuleFilters();

					dataRegistryMock.Setup(m => m.EnableForeignOperator).Returns(true);
					var filter = (ModuleTextFilter)FilterStripBizO["Is Foreign Operator"];
					AssertNotNull(filter);

					filter.IsActive = true;
					filter.Property = OrgConstants.FilterControl.IsForeignOperator.Code.No;

					var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
					orgCollection.Load();

					Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
					Assert("Expect collection to contain org2", orgCollection.Contains(org2));

					filter.Property = OrgConstants.FilterControl.IsForeignOperator.Code.Yes;

					orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
					orgCollection.Load();

					Assert("Expect collection to contain org1", orgCollection.Contains(org1));
					Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

					filter.Property = OrgConstants.FilterControl.IsForeignOperator.Code.All;

					orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
					orgCollection.Load();

					Assert("Expect collection to contain org1", orgCollection.Contains(org1));
					Assert("Expect collection to contain org2", orgCollection.Contains(org2));
				}

				FilterStripBizO.ResetModuleFilters();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
				{
					dataRegistryMock.Setup(m => m.EnableForeignOperator).Returns(true);
					AssertNull((ModuleTextFilter)FilterStripBizO["Is Foreign Operator"]);
				}
			}
		}

		#region TestDistributionCentre

		public void TestDistributionCentre()
		{
			var distributionCentreForCurrentCompany = Factory.NewWithValidTestData<OrgHeader>();
			var nonDistributionCentre = Factory.NewWithValidTestData<OrgHeader>();
			SetOrgHeaderForDistributionCentre(distributionCentreForCurrentCompany, "O1", true);
			SetOrgHeaderForDistributionCentre(nonDistributionCentre, "O2", false);
			Factory.Save();

			var filter = (OrgSecondaryTypeModuleFilter)FilterStripBizO["Secondary Type"];
			filter.Property = OrganisationSecondaryTypes.DistributionCentre;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertCollectionContains("Expect collection to contain DistributionCenterForCurrentCompany", distributionCentreForCurrentCompany, orgCollection);
			AssertCollectionNotContains("Expect collection not to contain NonDistributionCenter", nonDistributionCentre, orgCollection);
		}

		void SetOrgHeaderForDistributionCentre(OrgHeader orgHeader, string orgCode, bool isDistributionCentre)
		{
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_IsMiscFreightServices = true;
			orgHeader.OH_IsDistributionCentre = isDistributionCentre;
		}

		#endregion

		#endregion

		public void TestNoneOptions_IsExcludedForCompetitorIntelligence()
		{
			AssertFilterExists(OrgModuleType.Standard, "Secondary Type", true);
			AssertFilterExists(OrgModuleType.ClientIntelligence, "Secondary Type", true);
			AssertFilterExists(OrgModuleType.CompanyCampaignContact, "Secondary Type", true);
			AssertFilterExists(OrgModuleType.CompetitorIntelligence, "Secondary Type", false);
		}

		#region Date filters

		public void TestNoExceptionThrow_SALCSDateLastUnactionedFilter()
		{
			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateLastUnactioned];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 1, 2);
			filter.Property2 = new ZDateTime(2020, 1, 3);
			filter.IsActive = true;

			var query = FilterStripBizO.Filter;

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;

			query = query.AddToFilter(FilterStripBizO.Filter);

			var orgCollection = new OrgHeaderCollection(Factory, query);
			AssertNoExceptionThrown(orgCollection.Load);
		}

		public void TestGetWhereClauseFromDateComparison()
		{
			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateLastUnactioned];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 1, 2);
			filter.Property2 = new ZDateTime(2020, 1, 3);
			filter.IsActive = true;

			var query = filter.Query;
			AssertEquals(2, query.Params.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "@dateFrom", "@dateTo" }, query.Params.Select(u => u.ParameterName));
			AssertContains("BETWEEN", query.LiteralTextSqlFormatted);

			filter.Property1 = ZDateTime.Empty;
			query = filter.Query;
			AssertEquals(1, query.Params.Length);
			AssertEquals("@dateTo", query.Params[0].ParameterName);
			AssertNotContains("BETWEEN", query.LiteralTextSqlFormatted);

			filter.Property1 = new ZDateTime(2020, 1, 2);
			filter.Property2 = ZDateTime.Empty;
			query = filter.Query;
			AssertEquals(1, query.Params.Length);
			AssertEquals("@dateFrom", query.Params[0].ParameterName);
			AssertNotContains("BETWEEN", query.LiteralTextSqlFormatted);
		}

		public void TestSTDARLastChecked()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;

			org1.CompanyData.OB_ARQualityAssuredCheckedDate = new ZDateTime(2005, 1, 2);
			org2.CompanyData.OB_ARQualityAssuredCheckedDate = new ZDateTime(2005, 1, 4);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.STDARLastChecked];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 3);
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSTDAPLastChecked()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsCreditor = true;
			org2.OH_IsCreditor = true;

			org1.CompanyData.OB_APQualityAssuredCheckedDate = new ZDateTime(2005, 1, 2);
			org2.CompanyData.OB_APQualityAssuredCheckedDate = new ZDateTime(2005, 1, 4);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.STDAPLastChecked];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 3);
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSTDCRShipExpected()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			OrgHeader supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierBuyerLink orgSupplierBuyerLink1 = supplier1.SupplierLinks.AddNew();
			orgSupplierBuyerLink1.OL_OH_Supplier = org1.PK;
			orgSupplierBuyerLink1.UpdateShipmentDate = false;

			OrgSupplierBuyerLink orgSupplierBuyerLink2 = supplier2.SupplierLinks.AddNew();
			orgSupplierBuyerLink2.OL_OH_Supplier = org2.PK;
			orgSupplierBuyerLink2.UpdateShipmentDate = false;

			orgSupplierBuyerLink1.OL_InitialShipmentExpected = new ZDateTime(2005, 1, 2);
			orgSupplierBuyerLink2.OL_InitialShipmentExpected = new ZDateTime(2005, 1, 4);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.STDCRShipExpected];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 3);
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSTDCEShipExpected()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			OrgHeader buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader buyer2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierBuyerLink orgSupplierBuyerLink1 = buyer1.BuyerLinks.AddNew();
			orgSupplierBuyerLink1.OL_OH_Buyer = org1.PK;

			OrgSupplierBuyerLink orgSupplierBuyerLink2 = buyer2.BuyerLinks.AddNew();
			orgSupplierBuyerLink2.OL_OH_Buyer = org2.PK;

			orgSupplierBuyerLink1.OL_InitialShipmentExpected = new ZDateTime(2005, 1, 2);
			orgSupplierBuyerLink2.OL_InitialShipmentExpected = new ZDateTime(2005, 1, 4);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.STDCEShipExpected];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 3);
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSTDCreditReviewDate()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;

			org1.CompanyData.OB_ARAccountAndCreditReviewDue = new ZDateTime(2005, 1, 2);
			org2.CompanyData.OB_ARAccountAndCreditReviewDue = new ZDateTime(2005, 1, 4);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.STDCreditReviewDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 3);
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSALCSDateLastCall()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMLastCallDate = new ZDateTime(2005, 1, 2);
			org2.MiscServ.OM_CMLastCallDate = new ZDateTime(2005, 1, 4);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateLastCall];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1).ToLocalBranchTime();
			filter.Property2 = new ZDateTime(2005, 1, 3).ToLocalBranchTime();
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		[TestDate(2014, 1, 1)]
		public void TestSALCSDateNextCall()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = Factory.NewWithValidTestData<OrgSalesCall>();
			var call2 = Factory.NewWithValidTestData<OrgSalesCall>();
			var call3 = Factory.NewWithValidTestData<OrgSalesCall>();
			org1.SalesCalls.Add(call1);
			org2.SalesCalls.Add(call2);
			org2.SalesCalls.Add(call3);

			call1.OQ_CallDate = ZDateTime.Empty;
			call2.OQ_CallDate = ZDateTime.Empty;
			call3.OQ_CallDate = ZDateTime.Empty;

			call1.OQ_NextCall = new ZDateTime(2014, 1, 2);
			call2.OQ_NextCall = new ZDateTime(2014, 1, 2);
			call3.OQ_NextCall = new ZDateTime(2014, 1, 4);

			call2.OQ_Status = "CAN";

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateNextCall];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2014, 1, 1);
			filter.Property2 = new ZDateTime(2014, 1, 3);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		[TestDate(2014, 1, 5)]
		public void TestSALCSDateLastUnactioned()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = Factory.NewWithValidTestData<OrgSalesCall>();
			var call2 = Factory.NewWithValidTestData<OrgSalesCall>();
			var call3 = Factory.NewWithValidTestData<OrgSalesCall>();
			var call4 = Factory.NewWithValidTestData<OrgSalesCall>();
			org1.SalesCalls.Add(call1);
			org2.SalesCalls.Add(call2);
			org2.SalesCalls.Add(call3);
			org2.SalesCalls.Add(call4);

			call1.OQ_CallDate = ZDateTime.Empty;
			call1.OQ_NextCall = new ZDateTime(2014, 1, 4);
			call2.OQ_CallDate = ZDateTime.Empty;
			call2.OQ_NextCall = new ZDateTime(2014, 1, 6);
			call3.OQ_CallDate = new ZDateTime(2014, 1, 4);
			call3.OQ_NextCall = new ZDateTime(2014, 1, 4);
			call3.OQ_CallDate = ZDateTime.Empty;
			call3.OQ_NextCall = new ZDateTime(2014, 1, 8);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateLastUnactioned];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2014, 1, 1);
			filter.Property2 = new ZDateTime(2014, 1, 7);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			call1.OQ_Status = "CAN";
			Factory.Save();
			orgCollection.Load();

			Assert("Expect collection to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		[TestDate(2014, 1, 6)]
		public void TestSALCSDateLastUnactioned2()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var call1a = org1.SalesCalls.AddNew();
			var call1b = org1.SalesCalls.AddNew();
			var call1c = org1.SalesCalls.AddNew();
			var call2 = org2.SalesCalls.AddNew();

			call1a.OQ_CallDate = ZDateTime.Empty;
			call1a.OQ_NextCall = new ZDateTime(2014, 1, 4);
			call1b.OQ_CallDate = ZDateTime.Empty;
			call1b.OQ_NextCall = new ZDateTime(2014, 1, 2);
			call1c.OQ_CallDate = new ZDateTime(2014, 1, 5);
			call1c.OQ_NextCall = new ZDateTime(2014, 1, 5);
			call2.OQ_CallDate = new ZDateTime(2014, 1, 4);
			call2.OQ_NextCall = new ZDateTime(2014, 1, 4);

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateLastUnactioned];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2014, 1, 1);
			filter.Property2 = new ZDateTime(2014, 1, 5);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		[TestDate(2014, 1, 5)]
		public void TestSALCSDateLastUnactioned_NoClosed()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = Factory.NewWithValidTestData<OrgSalesCall>();
			var call2 = Factory.NewWithValidTestData<OrgSalesCall>();
			var call3 = Factory.NewWithValidTestData<OrgSalesCall>();
			var call4 = Factory.NewWithValidTestData<OrgSalesCall>();
			org1.SalesCalls.Add(call1);
			org2.SalesCalls.Add(call2);
			org2.SalesCalls.Add(call3);
			org2.SalesCalls.Add(call4);

			call1.OQ_CallDate = ZDateTime.Empty;
			call1.OQ_NextCall = new ZDateTime(2014, 1, 4);
			call2.OQ_CallDate = ZDateTime.Empty;
			call2.OQ_NextCall = new ZDateTime(2014, 1, 6);
			call3.OQ_CallDate = new ZDateTime(2014, 1, 4);
			call3.OQ_NextCall = new ZDateTime(2014, 1, 4);
			call3.OQ_CallDate = ZDateTime.Empty;
			call3.OQ_NextCall = new ZDateTime(2014, 1, 8);

			Factory.Save();
			var collection = new CommunicationStatusCollection();
			collection.Add(Constants.Sales.Status.Cancelled, (NoResString)"Cancelled", false, true);
			collection.Add(Constants.Sales.Status.Completed, (NoResString)"Completed", false, true);
			OrganisationsDataRegistry.Instance.CommunicationStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSDateLastUnactioned];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2014, 1, 1);
			filter.Property2 = new ZDateTime(2014, 1, 7);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			AssertNoExceptionThrown(orgCollection.Load);

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSALCSEstClose()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMEstimatedDateToClose = new ZDateTime(2005, 1, 2);
			org2.MiscServ.OM_CMEstimatedDateToClose = new ZDateTime(2005, 1, 4);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCSEstClose];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 3);
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSALCRClientComm()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMClientCommenced = new ZDateTime(2005, 1, 2);
			org2.MiscServ.OM_CMClientCommenced = new ZDateTime(2005, 1, 4);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.SALCRClientComm];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 3);
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestKnownShipperExpiryDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();

				org1.OH_Code = "XXXYYY";
				org2.OH_Code = "XXXXXX";
				org3.OH_Code = "XXXZZZ";

				org1.CountryData.OV_EXApprovalExpiryDate = new ZDate(2013, 9, 1);
				org2.CountryData.OV_EXApprovalExpiryDate = new ZDate(2013, 6, 1);

				Factory.Save();

				var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Code.KnownShipperExpiryDate];
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = new ZDateTime(2013, 8, 15);
				filter.Property2 = new ZDateTime(2013, 9, 3);
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
				orgCollection.Load();

				Assert("Expect collection to contain org1", orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
				Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
			}
		}

		public void TestPowerOfAttorneyValidToDate_HaveResult()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var document = org.RequiredDocuments.AddNew();
			document.EQ_DocType = Constants.RefDocTypes.PowerOfAttorney;
			document.EQ_ValidToDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.PowerOfAttorneyValidToDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(1971, 9, 17);
			filter.Property2 = new ZDateTime(1971, 9, 18);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org", orgCollection.Contains(org));
		}

		public void TestPowerOfAttorneyValidToDate_NoResult_TypeIsIncorrect()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var document = org.RequiredDocuments.AddNew();
			document.EQ_DocType = Constants.RefDocTypes.AgentsInstruction;
			document.EQ_ValidToDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.PowerOfAttorneyValidToDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(1971, 9, 17);
			filter.Property2 = new ZDateTime(1971, 9, 18);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to not contain org", !orgCollection.Contains(org));
		}

		public void TestPowerOfAttorneyValidToDate_NoResult_TimeIsIncorrect()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var document = org.RequiredDocuments.AddNew();
			document.EQ_DocType = Constants.RefDocTypes.AgentsInstruction;
			document.EQ_ValidToDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.PowerOfAttorneyValidToDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(1971, 10, 17);
			filter.Property2 = new ZDateTime(1971, 10, 18);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to not contain org", !orgCollection.Contains(org));
		}

		public void TestLastScreenDateBasic()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var opss1 = Factory.New<StmEntityScreeningLog>();
			opss1.PJ_SystemCreateUser = EnvProxy.Instance.CurrentUser.Initials;
			var datetime1 = new DateTime(2020, 4, 3);
			opss1.PJ_SystemCreateTimeUtc = new ZDateTime(datetime1.ToUniversalTime());
			opss1.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
			opss1.PJ_ParentTableCode = "OH";
			opss1.PJ_ParentID = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var opss2 = Factory.New<StmEntityScreeningLog>();
			opss2.PJ_SystemCreateUser = EnvProxy.Instance.CurrentUser.Initials;
			var datetime2 = new DateTime(2020, 3, 3);
			opss2.PJ_SystemCreateTimeUtc = new ZDateTime(datetime2.ToUniversalTime());
			opss2.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
			opss2.PJ_ParentTableCode = "OH";
			opss2.PJ_ParentID = org2.PK;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.LastScreenDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 4, 1);
			filter.Property2 = new ZDateTime(2020, 4, 10);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to NOT contain org2", !orgCollection.Contains(org2));
		}

		public void TestLastScreenDateUnsupportedType()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var opss1 = Factory.New<StmEntityScreeningLog>();
			opss1.PJ_SystemCreateUser = EnvProxy.Instance.CurrentUser.Initials;
			var datetime1 = new DateTime(2020, 4, 4);
			opss1.PJ_SystemCreateTimeUtc = new ZDateTime(datetime1.ToUniversalTime());
			opss1.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.JobCleared;
			opss1.PJ_ParentTableCode = "OH";
			opss1.PJ_ParentID = org1.PK;

			var opss2 = Factory.New<StmEntityScreeningLog>();
			opss2.PJ_SystemCreateUser = EnvProxy.Instance.CurrentUser.Initials;
			var datetime2 = new DateTime(2020, 4, 3);
			opss2.PJ_SystemCreateTimeUtc = new ZDateTime(datetime2.ToUniversalTime());
			opss2.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
			opss2.PJ_ParentTableCode = "OH";
			opss2.PJ_ParentID = org1.PK;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.LastScreenDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 4, 1);
			filter.Property2 = new ZDateTime(2020, 4, 10);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
		}

		public void TestLastScreenDateLatestOutOfRange()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var opss1 = Factory.New<StmEntityScreeningLog>();
			opss1.PJ_SystemCreateUser = EnvProxy.Instance.CurrentUser.Initials;
			var datetime1 = new DateTime(2020, 4, 12);
			opss1.PJ_SystemCreateTimeUtc = new ZDateTime(datetime1.ToUniversalTime());
			opss1.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
			opss1.PJ_ParentTableCode = "OH";
			opss1.PJ_ParentID = org1.PK;

			var opss2 = Factory.New<StmEntityScreeningLog>();
			opss2.PJ_SystemCreateUser = EnvProxy.Instance.CurrentUser.Initials;
			var datetime2 = new DateTime(2020, 4, 3);
			opss2.PJ_SystemCreateTimeUtc = new ZDateTime(datetime2.ToUniversalTime());
			opss2.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.ScreenedClear;
			opss2.PJ_ParentTableCode = "OH";
			opss2.PJ_ParentID = org1.PK;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.LastScreenDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 4, 1);
			filter.Property2 = new ZDateTime(2020, 4, 10);
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to NOT contain org1", !orgCollection.Contains(org1));
		}

		#endregion

		#region Location filters

		public void TestClosestPort_SearchRestrictions()
		{
			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort];
			Assert("Zones should be allowed if there is no Country search restriction", ((LocationCollection)filter.List).AllowZones);
			Assert("Empty default filter value if there is no Country search restriction", filter.DefaultProperty.IsEmpty);
			AssertEquals("Filter should have default visibility if there is no Country search restriction", FilterVisibility.Visible, filter.Visibility);
			AssertNull("PropertyValidation should be null if search is unrestricted", filter.PropertyValidation);

			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = false;

			FilterStripBusinessObject newFilterStripBizO = GetNewFilterStripBusinessObject();
			filter = (ModuleNkFilter)newFilterStripBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort];
			Assert("Zones should be disallowed if there is Country search restriction", !((LocationCollection)filter.List).AllowZones);
			AssertEquals("If there is UNLOCO search restriction the filter should be defaulted to CurrentCompany.CountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, filter.DefaultProperty);
			AssertEquals("Filter should be always visible if there is Country search restriction", FilterVisibility.AlwaysVisible, filter.Visibility);
			AssertNotNull("PropertyValidation should not be null if search is restricted", filter.PropertyValidation);
			AssertNoErrors(filter.PropertyInfo);

			filter.Property = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "ADL";
			AssertNoErrors(filter.PropertyInfo);

			filter.Property = "AA";
			string expectedError = $@"Your current security rights only allow you to view organizations based in your current login country/region ({GlbCompany.CurrentCompany.GC_RN_NKCountryCode}).
If you think this is incorrect, please contact your system administrator.";
			AssertHasError(filter.PropertyInfo, expectedError);

			filter.Property = "AABOS";
			AssertHasError(filter.PropertyInfo, expectedError);

			filter.Property = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "MEL";
			AssertNoErrors(filter.PropertyInfo);

			filter.Property = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertNoErrors(filter.PropertyInfo);
		}

		public void TestOrgPort()
		{
			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = true;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_RL_NKClosestPort = "AUSYD";
			org2.OH_RL_NKClosestPort = "UAIEV";

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort];
			filter.Property = "AUSYD";
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCountry()
		{
			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = true;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "Test1";

			var address1 = org1.Addresses.AddNew();
			address1.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address1.OA_RN_NKCountryCode = "AU";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "Test2";
			org2.OH_IsGlobalAccount = true;

			var address2 = org2.Addresses.AddNew();
			address2.OA_Language = "EN-US";
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address2.OA_RN_NKCountryCode = "US";

			var address3 = org2.Addresses.AddNew();
			address3.OA_Language = "EN";
			address3.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address3.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address3.OA_RN_NKCountryCode = "AU";
			Factory.Save();

			var filter1 = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.OrgAddress.Country];
			filter1.Property = "AU";
			filter1.IsActive = true;

			var filter2 = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDetails.Code];
			filter2.Property = "Test";
			filter2.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter2.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1, org2 }, orgCollection);

			filter1.Property = "US";
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org2 }, orgCollection);

			filter1.Property = "CN";
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals(0, orgCollection.Count);
		}

		[StressTest]
		public void TestMainUnLOCOCOWorksForCountryCode()
		{
			Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed = true;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_RL_NKClosestPort = "AUSYD";
			org2.OH_RL_NKClosestPort = "UAIEV";

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.OrgPort];
			filter.Property = "AU";
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestOrgSALTradeLanes()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uaiev = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "UAIEV");

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			OrgSales orgSale1 = org1.SalesCollection.AddNew();
			orgSale1.OW_OH_Supplier = org1.PK;
			orgSale1.OW_DestinationID = ausyd.PK;

			OrgSales orgSale2 = org2.SalesCollection.AddNew();
			orgSale2.OW_OH_Supplier = org2.PK;
			orgSale2.OW_DestinationID = uaiev.PK;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.SALTradeLanes];
			filter.Property = "AUSYD";
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestOrgCMPTradeLanes()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uaiev = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "UAIEV");

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsCompetitor = true;
			org2.OH_IsCompetitor = true;

			OrgSales orgSale1 = Factory.NewWithValidTestData<OrgSales>();
			orgSale1.OW_DestinationID = ausyd.PK;
			OrgTradeDetail orgTradeDetail1 = orgSale1.TradeDetails.AddNew();
			orgTradeDetail1.ProspectDetail.PAP_OH_Competitor = org1.PK;

			OrgSales orgSale2 = Factory.NewWithValidTestData<OrgSales>();
			orgSale2.OW_DestinationID = uaiev.PK;
			OrgTradeDetail orgTradeDetail2 = orgSale2.TradeDetails.AddNew();
			orgTradeDetail2.ProspectDetail.PAP_OH_Competitor = org2.PK;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.CMPTradeLanes];
			filter.Property = "AUSYD";
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { org1 }, orgCollection);
		}

		public void TestSalesMainExImCommodities()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XXXYYY";
			org1.OH_IsSalesLead = true;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "XXXXXX";
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_RH_NKCMMainImportCmdty = "ALUM";
			org1.MiscServ.OM_RH_NKCMMainExportCmdty = "AABT";
			org2.MiscServ.OM_RH_NKCMMainImportCmdty = "AABT";
			org2.MiscServ.OM_RH_NKCMMainExportCmdty = "ALUM";

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALMainImpCommodity];
			ModuleNkFilter filter2 = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALMainExpCommodity];

			filter.Property = "ALUM";
			filter2.Property = "ALUM";

			filter.IsActive = true;
			filter2.IsActive = false;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.IsActive = false;
			filter2.IsActive = true;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));

			filter.Property = "AABT";
			filter2.Property = "AABT";

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.IsActive = true;
			filter2.IsActive = false;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
		}

		public void TestOrgForwarderAppPort()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsForwarder = true;
			org2.OH_IsForwarder = true;

			OrgAppointedAgentPorts airAppointedAgentPorts1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			airAppointedAgentPorts1.O5_OH = org1.PK;
			airAppointedAgentPorts1.O5_PortOrCountry = "AUSYD";
			org1.AppointedAgentPorts.Add(airAppointedAgentPorts1);

			OrgAppointedAgentPorts airAppointedAgentPorts2 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			airAppointedAgentPorts2.O5_OH = org2.PK;
			airAppointedAgentPorts2.O5_PortOrCountry = "UAIEV";
			org2.AppointedAgentPorts.Add(airAppointedAgentPorts2);

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.ForwarderAppPort];
			filter.Property = "AUSYD";
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestOrgCarrierAppPort()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsShippingProvider = true;
			org2.OH_IsShippingProvider = true;

			OrgAppointedAgentPorts airAppointedAgentPorts1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			airAppointedAgentPorts1.O5_OH = org1.PK;
			airAppointedAgentPorts1.O5_PortOrCountry = "AUSYD";
			org1.AppointedAgentPorts.Add(airAppointedAgentPorts1);

			OrgAppointedAgentPorts airAppointedAgentPorts2 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			airAppointedAgentPorts2.O5_OH = org2.PK;
			airAppointedAgentPorts2.O5_PortOrCountry = "UAIEV";
			org2.AppointedAgentPorts.Add(airAppointedAgentPorts2);

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.CarrierAppPort];
			filter.Property = "AUSYD";
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		#endregion

		#region Relationship Guids Filters

		#region ARFilters

		public void TestARAcctGroup()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;

			OrgDebtorGroup orgDebtorGroup1 = Factory.NewWithValidTestData<OrgDebtorGroup>();
			OrgDebtorGroup orgDebtorGroup2 = Factory.NewWithValidTestData<OrgDebtorGroup>();

			org1.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup1.PK;
			org2.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup2.PK;
			org1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			org2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.ARAcctGroup];
			filter.Property = orgDebtorGroup1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestARCurrency()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;

			RefCurrency currency1 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "UAH"));
			RefCurrency currency2 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));

			org1.CompanyData.OB_RX_NKARDDefltCurrency = currency1.RX_Code;
			org2.CompanyData.OB_RX_NKARDDefltCurrency = currency2.RX_Code;
			org1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			org2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.ARCurrency];
			filter.Property = currency1.RX_Code;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		#endregion

		#region APFilters

		public void TestAPAcctGroup()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsCreditor = true;
			org2.OH_IsCreditor = true;

			OrgCreditorGroup orgCreditorGroup1 = Factory.NewWithValidTestData<OrgCreditorGroup>();
			OrgCreditorGroup orgCreditorGroup2 = Factory.NewWithValidTestData<OrgCreditorGroup>();

			org1.CompanyData.OB_OG_APCreditorGroup = orgCreditorGroup1.PK;
			org2.CompanyData.OB_OG_APCreditorGroup = orgCreditorGroup2.PK;
			org1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			org2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.APAcctGroup];
			filter.Property = orgCreditorGroup1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestAPBankAccount()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsCreditor = true;
			org2.OH_IsCreditor = true;

			AccBankAccount accBankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount accBankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();

			org1.CompanyData.OB_AB_APDefaultBankAccount = accBankAccount1.PK;
			org2.CompanyData.OB_AB_APDefaultBankAccount = accBankAccount2.PK;
			org1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			org2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.APBankAccount];
			filter.Property = accBankAccount1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestAPChargeCode()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsCreditor = true;
			org2.OH_IsCreditor = true;

			AccChargeCode accChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode accChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();

			org1.CompanyData.OB_AC_APDefaultChargeCode = accChargeCode1.PK;
			org2.CompanyData.OB_AC_APDefaultChargeCode = accChargeCode2.PK;
			org1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			org2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.APChargeCode];
			filter.Property = accChargeCode1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		#endregion

		#region CNRFilters

		public void TestCNRCountry()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			RefCountry refCountry1 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			RefCountry refCountry2 = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "UA"));

			org1.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = refCountry1.RN_Code;
			org2.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = refCountry2.RN_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNRCountry];
			filter.Property = refCountry1.RN_Code;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCNRCurrency()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			RefCurrency refCurrency1 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			RefCurrency refCurrency2 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "UAH"));

			org1.MiscServ.OM_RX_NKEXDefCurrency = refCurrency1.RX_Code;
			org2.MiscServ.OM_RX_NKEXDefCurrency = refCurrency2.RX_Code;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNRCurrency];
			filter.Property = refCurrency1.RX_Code;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCNRSeaCartageCordinator()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "TES";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "TES";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNRSeaCartageCordinator];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCNRAirCartageCordinator()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "TEA";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "TEA";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNRAirCartageCordinator];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCNRSeaCustomerServiceRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "FES";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "FES";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNRSeaCustomerServiceRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCNRAirCustomerServiceRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "FEA";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "FEA";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNRAirCustomerServiceRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		#endregion

		#region GlobalCreditGroupFilter

		public void TestGlobalCreditGroup()
		{
			var globalOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS"));
			var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AALSHI"));
			var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "4BELEV"));
			org1.MiscServ.OM_OH_ARGlobalCreditGroup = globalOrg.PK;

			Factory.Save();
			var filter = (ModuleGuidFilter)FilterStripBizO["Global Credit Group"];
			filter.Property = globalOrg.PK;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain the first org", orgCollection.Contains(org1));
			Assert("Expect collection not cointain the second org", !orgCollection.Contains(org2));
		}

		#endregion

		#region CNEFilters

		public void TestCNESeaCustomerServiceRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org1.OH_FullName = "1234567890";
			org1.OH_FullName = "1234567890";

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "FIS";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "FIS";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNESeaCustomerServiceRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNESeaCustomerServiceRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));

			filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNESeaCustomerServiceRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			ModuleTextFilter filterName = (ModuleTextFilter)FilterStripBizO["Name"];
			filterName.Property = "1234567890";
			filterName.IsActive = true;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCNEAirCustomerServiceRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "FIA";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "FIA";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNEAirCustomerServiceRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCNESeaCartageCordinator()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "TIS";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "TIS";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNESeaCartageCordinator];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestCNEAirCartageCordinator()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "TIA";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "TIA";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.CartageCoordinator;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.CNEAirCartageCordinator];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		#endregion

		#region FWDFilters

		public void TestFWDCurrency()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsForwarder = true;
			org2.OH_IsForwarder = true;

			RefCurrency currency1 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "UAH"));
			RefCurrency currency2 = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));

			org1.MiscServ.OM_RX_NKFWDefCurrency = currency1.RX_Code;
			org2.MiscServ.OM_RX_NKFWDefCurrency = currency2.RX_Code;
			org1.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			org2.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.FWDCurrency];
			filter.Property = currency1.RX_Code;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		#endregion

		#region SALFilters

		public void TestSALOverallRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "XXXXXZ";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;
			org3.OH_IsSalesLead = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment1.O8_OH = org1.PK;
			orgStaffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment1.O8_Department = "ALL";
			orgStaffAssignment1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2.O8_OH = org2.PK;
			orgStaffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2.O8_Department = "ALL";
			orgStaffAssignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			OrgStaffAssignments orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_OH = org3.PK;
			orgStaffAssignment3.O8_GC = ZGuid.Empty;
			orgStaffAssignment3.O8_Department = "ALL";
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALOverallRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));

			org1.OH_IsSalesLead = false;
			org3.OH_IsSalesLead = false;
			Factory.Save();

			filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALOverallRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;
			OrgHeaderCollection orgWithSalesLeadFalseCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgWithSalesLeadFalseCollection.Load();
			Assert("Expect collection to contain org1.", orgWithSalesLeadFalseCollection.Contains(org1));
			Assert("Expect collection to contain org3.", orgWithSalesLeadFalseCollection.Contains(org3));
		}

		public void TestSALImportAirRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "FIA";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "FIA";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALImportAirRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSALImportSeaRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "FIS";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "FIS";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALImportSeaRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSALExportAirRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "FEA";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "FEA";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALExportAirRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSALExportSeaRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "FES";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "FES";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALExportSeaRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSALWarehousingRep()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = org1.PK;
			orgStaffAssignent1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent1.O8_Department = "WAR";
			orgStaffAssignent1.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignent2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent2.O8_OH = org2.PK;
			orgStaffAssignent2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignent2.O8_Department = "WAR";
			orgStaffAssignent2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignent2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALWarehousingRep];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestSALOverallAccountManager()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";
			org3.OH_Code = "XXXXXZ";
			org4.OH_Code = "XXXXXA";

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;
			org3.OH_IsSalesLead = true;
			org4.OH_IsSalesLead = true;

			GlbStaff glbStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff glbStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			OrgStaffAssignments orgStaffAssignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment1.O8_OH = org1.PK;
			orgStaffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment1.O8_Department = "ALL";
			orgStaffAssignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment1.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2.O8_OH = org2.PK;
			orgStaffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2.O8_Department = "ALL";
			orgStaffAssignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment2.O8_GS_NKPersonResponsible = glbStaff2.GS_Code;

			OrgStaffAssignments orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_OH = org3.PK;
			orgStaffAssignment3.O8_GC = ZGuid.Empty;
			orgStaffAssignment3.O8_Department = "ALL";
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			OrgStaffAssignments orgStaffAssignment4 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment4.O8_OH = org4.PK;
			orgStaffAssignment4.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment4.O8_Department = "WAR";
			orgStaffAssignment4.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment4.O8_GS_NKPersonResponsible = glbStaff1.GS_Code;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALOverallAccountManager];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals(2, orgCollection.Count);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));
			Assert("Expect collection not to contain org4.", !orgCollection.Contains(org4));

			filter.Property = glbStaff2.PK;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertEquals(0, orgCollection.Count);

			org1.OH_IsSalesLead = false;
			org3.OH_IsSalesLead = false;
			Factory.Save();

			filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALOverallAccountManager];
			filter.Property = glbStaff1.PK;
			filter.IsActive = true;
			OrgHeaderCollection orgCollectionIsSalesLeadFalse = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollectionIsSalesLeadFalse.Load();
			AssertEquals(2, orgCollectionIsSalesLeadFalse.Count);
			Assert("Expect collection to contain org1.", orgCollectionIsSalesLeadFalse.Contains(org1));
			Assert("Expect collection to contain org3.", orgCollectionIsSalesLeadFalse.Contains(org3));
		}

		#endregion

		#region Relationship Manager Filters

		public void TestRMStaffRole()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			var organization3 = Factory.NewWithValidTestData<OrgHeader>();
			var organization4 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "XXXYYY";
			organization2.OH_Code = "XXXXXX";
			organization3.OH_Code = "XXXXXZ";
			organization4.OH_Code = "XXXXXA";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var orgStaffAssignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment1.O8_OH = organization1.PK;
			orgStaffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment1.O8_Department = "ALL";
			orgStaffAssignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2.O8_OH = organization2.PK;
			orgStaffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2.O8_Department = "ALL";
			orgStaffAssignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			var orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_OH = organization3.PK;
			orgStaffAssignment3.O8_GC = ZGuid.Empty;
			orgStaffAssignment3.O8_Department = "ALL";
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment4 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment4.O8_OH = organization4.PK;
			orgStaffAssignment4.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment4.O8_Department = "WAR";
			orgStaffAssignment4.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment4.O8_GS_NKPersonResponsible = staff1.GS_Code;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffRole];

			filter.Property = StaffAssignmentRoles.Codes.AccountManager;
			filter.IsActive = true;
			var filterQuery = FilterStripBizO.Filter;
			var organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals(3, organizations.Count);
			Assert("Expect collection to contain org1.", organizations.Contains(organization1));
			Assert("Expect collection not to contain org2.", !organizations.Contains(organization2));
			Assert("Expect collection to contain org3.", organizations.Contains(organization3));
			Assert("Expect collection to contain org4.", organizations.Contains(organization4));

			filter.Property = StaffAssignmentRoles.Codes.SalesRep;
			filterQuery = FilterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();
			AssertEquals(1, organizations.Count);
			Assert("Expect collection not to contain org1.", !organizations.Contains(organization1));
			Assert("Expect collection to contain org2.", organizations.Contains(organization2));
			Assert("Expect collection not to contain org3.", !organizations.Contains(organization3));
			Assert("Expect collection not to contain org4.", !organizations.Contains(organization4));

			var filter1 = (ModuleTextFilter)FilterStripBizO.CreateDuplicateFor(OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffRole);
			filter1.Property = StaffAssignmentRoles.Codes.AccountManager;
			filter1.IsActive = true;
			filter.OrCategory = FilterOrCategory.Red;
			filter1.OrCategory = FilterOrCategory.Red;
			filterQuery = FilterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();
			AssertEquals(4, organizations.Count);
		}

		public void TestRMStaff()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			var organization3 = Factory.NewWithValidTestData<OrgHeader>();
			var organization4 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "XXXYYY";
			organization2.OH_Code = "XXXXXX";
			organization3.OH_Code = "XXXXXZ";
			organization4.OH_Code = "XXXXXA";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var orgStaffAssignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment1.O8_OH = organization1.PK;
			orgStaffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment1.O8_Department = "ALL";
			orgStaffAssignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2.O8_OH = organization2.PK;
			orgStaffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2.O8_Department = "ALL";
			orgStaffAssignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			var orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_OH = organization3.PK;
			orgStaffAssignment3.O8_GC = ZGuid.Empty;
			orgStaffAssignment3.O8_Department = "ALL";
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment4 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment4.O8_OH = organization4.PK;
			orgStaffAssignment4.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment4.O8_Department = "WAR";
			orgStaffAssignment4.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment4.O8_GS_NKPersonResponsible = staff1.GS_Code;
			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.RMStaff];

			filter.Property = staff1.PK;
			filter.IsActive = true;
			var filterQuery = FilterStripBizO.Filter;
			var organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals(3, organizations.Count);
			Assert("Expect collection to contain org1.", organizations.Contains(organization1));
			Assert("Expect collection not to contain org2.", !organizations.Contains(organization2));
			Assert("Expect collection to contain org3.", organizations.Contains(organization3));
			Assert("Expect collection to contain org4.", organizations.Contains(organization4));

			filter.Property = staff2.PK;
			filterQuery = FilterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals(1, organizations.Count);
			Assert("Expect collection not to contain org1.", !organizations.Contains(organization1));
			Assert("Expect collection to contain org2.", organizations.Contains(organization2));
			Assert("Expect collection not to contain org3.", !organizations.Contains(organization3));
			Assert("Expect collection not to contain org4.", !organizations.Contains(organization4));

			var filter1 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(OrgConstants.FilterControl.GuidRelationships.Description.RMStaff);
			filter1.Property = staff1.PK;
			filter1.IsActive = true;
			filter.OrCategory = FilterOrCategory.Red;
			filter1.OrCategory = FilterOrCategory.Red;
			filterQuery = FilterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();
			AssertEquals(4, organizations.Count);
		}

		public void TestRMStaffDepartment()
		{
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			var organization3 = Factory.NewWithValidTestData<OrgHeader>();
			var organization4 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "XXXYYY";
			organization2.OH_Code = "XXXXXX";
			organization3.OH_Code = "XXXXXZ";
			organization4.OH_Code = "XXXXXA";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var orgStaffAssignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment1.O8_OH = organization1.PK;
			orgStaffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment1.O8_Department = "ALL";
			orgStaffAssignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2.O8_OH = organization2.PK;
			orgStaffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2.O8_Department = "ALL";
			orgStaffAssignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			var orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_OH = organization3.PK;
			orgStaffAssignment3.O8_GC = ZGuid.Empty;
			orgStaffAssignment3.O8_Department = "ALL";
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment4 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment4.O8_OH = organization4.PK;
			orgStaffAssignment4.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment4.O8_Department = "WAR";
			orgStaffAssignment4.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment4.O8_GS_NKPersonResponsible = staff1.GS_Code;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffDepartment];

			filter.Property = "ALL";
			filter.IsActive = true;
			var filterQuery = FilterStripBizO.Filter;
			var organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals(3, organizations.Count);
			Assert("Expect collection to contain org1.", organizations.Contains(organization1));
			Assert("Expect collection to contain org2.", organizations.Contains(organization2));
			Assert("Expect collection to contain org3.", organizations.Contains(organization3));
			Assert("Expect collection not to contain org4.", !organizations.Contains(organization4));

			filter.Property = "WAR";
			filterQuery = FilterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals(1, organizations.Count);
			Assert("Expect collection not to contain org1.", !organizations.Contains(organization1));
			Assert("Expect collection not to contain org2.", !organizations.Contains(organization2));
			Assert("Expect collection not to contain org3.", !organizations.Contains(organization3));
			Assert("Expect collection to contain org4.", organizations.Contains(organization4));

			var filter1 = (ModuleTextFilter)FilterStripBizO.CreateDuplicateFor(OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffDepartment);
			filter1.Property = "ALL";
			filter1.IsActive = true;
			filter.OrCategory = FilterOrCategory.Red;
			filter1.OrCategory = FilterOrCategory.Red;
			filterQuery = FilterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();
			AssertEquals(4, organizations.Count);
		}

		public void TestRMStaffCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			var organization3 = Factory.NewWithValidTestData<OrgHeader>();
			var organization4 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "XXXYYY";
			organization2.OH_Code = "XXXXXX";
			organization3.OH_Code = "XXXXXZ";
			organization4.OH_Code = "XXXXXA";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var orgStaffAssignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment1.O8_OH = organization1.PK;
			orgStaffAssignment1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment1.O8_Department = "ALL";
			orgStaffAssignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2.O8_OH = organization2.PK;
			orgStaffAssignment2.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2.O8_Department = "ALL";
			orgStaffAssignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			orgStaffAssignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			var orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_OH = organization3.PK;
			orgStaffAssignment3.O8_GC = company.PK;
			orgStaffAssignment3.O8_Department = "ALL";
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment4 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment4.O8_OH = organization4.PK;
			orgStaffAssignment4.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment4.O8_Department = "WAR";
			orgStaffAssignment4.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment4.O8_GS_NKPersonResponsible = staff1.GS_Code;
			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.RMStaffCompany];

			filter.Property = GlbCompany.CurrentCompany.PK;
			filter.IsActive = true;
			var filterQuery = FilterStripBizO.Filter;
			var organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals(3, organizations.Count);
			Assert("Expect collection to contain org1.", organizations.Contains(organization1));
			Assert("Expect collection to contain org2.", organizations.Contains(organization2));
			Assert("Expect collection not to contain org3.", !organizations.Contains(organization3));
			Assert("Expect collection to contain org4.", organizations.Contains(organization4));

			var filter1 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(OrgConstants.FilterControl.GuidRelationships.Description.RMStaffCompany);
			filter1.Property = company.PK;
			filter1.IsActive = true;
			filter.OrCategory = FilterOrCategory.Red;
			filter1.OrCategory = FilterOrCategory.Red;
			filterQuery = FilterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();
			AssertEquals(4, organizations.Count);
		}

		public void TestRMStaffCompanyFilter_ReadOnlyWhenViewOtherSecurityCheckpointIsDisallowed()
		{
			Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = true;
			var filter1 = (ModuleGuidFilter)GetNewFilterStripBusinessObject()[OrgConstants.FilterControl.GuidRelationships.Description.RMStaffCompany];
			Assert("The Staff Assignments - Company filter should not be readonly when security checkpoint allows.", !filter1.ReadOnly);

			Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed = false;
			var filter2 = (ModuleGuidFilter)GetNewFilterStripBusinessObject()[OrgConstants.FilterControl.GuidRelationships.Description.RMStaffCompany];
			Assert("The Staff Assignments - Company filter should be readonly when security checkpoint disallows.", filter2.ReadOnly);
			AssertEquals("The Staff Assignments - Company filter should be filled with the current company when security checkpoint disallows.", GlbCompany.CurrentCompany.PK, filter2.Property);
		}

		public void TestStaffFilters_SubGroup()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			var organization3 = Factory.NewWithValidTestData<OrgHeader>();
			var organization4 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "XXXYYY";
			organization2.OH_Code = "XXXXXX";
			organization3.OH_Code = "XXXXXZ";
			organization4.OH_Code = "XXXXXA";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var orgStaffAssignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment1.O8_OH = organization1.PK;
			orgStaffAssignment1.O8_GC = company1.PK;
			orgStaffAssignment1.O8_Department = "ALL";
			orgStaffAssignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment1.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment2_1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2_1.O8_OH = organization2.PK;
			orgStaffAssignment2_1.O8_GC = GlbCompany.CurrentCompany.PK;
			orgStaffAssignment2_1.O8_Department = "WAR";
			orgStaffAssignment2_1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment2_1.O8_GS_NKPersonResponsible = staff2.GS_Code;
			var orgStaffAssignment2_2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment2_2.O8_OH = organization2.PK;
			orgStaffAssignment2_2.O8_GC = company1.PK;
			orgStaffAssignment2_2.O8_Department = "ALL";
			orgStaffAssignment2_2.O8_Role = StaffAssignmentRoles.Codes.CustomerServiceRep;
			orgStaffAssignment2_2.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment3 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment3.O8_OH = organization3.PK;
			orgStaffAssignment3.O8_GC = company2.PK;
			orgStaffAssignment3.O8_Department = "ALL";
			orgStaffAssignment3.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment3.O8_GS_NKPersonResponsible = staff1.GS_Code;
			var orgStaffAssignment4 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignment4.O8_OH = organization4.PK;
			orgStaffAssignment4.O8_GC = company1.PK;
			orgStaffAssignment4.O8_Department = "WAR";
			orgStaffAssignment4.O8_Role = StaffAssignmentRoles.Codes.AccountManager;
			orgStaffAssignment4.O8_GS_NKPersonResponsible = staff1.GS_Code;

			Factory.Save();

			var filterStripBizO = GetNewFilterStripBusinessObject();

			var filterRole = (ModuleTextFilter)filterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Code.RMStaffRole];
			filterRole.Property = StaffAssignmentRoles.Codes.AccountManager;
			filterRole.IsActive = true;

			var filterStaff = (ModuleGuidFilter)filterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.RMStaff];
			filterStaff.Property = staff1.PK;
			filterStaff.IsActive = true;

			var filterQuery = filterStripBizO.Filter;
			var organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals("Organisation Count - Role and Staff", 3, organizations.Count);
			AssertContainsExactElementsInAnyOrder("Organisation values - Role and Staff", new[] { organization1, organization3, organization4 }, organizations);

			var filterCompany = (ModuleGuidFilter)filterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.RMStaffCompany];
			filterCompany.Property = company1.PK;
			filterCompany.IsActive = true;

			filterQuery = filterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals("Organisation Count - Role, Staff and Company", 2, organizations.Count);
			AssertContainsExactElementsInAnyOrder("Organisation values - Role, Staff and Company", new[] { organization1, organization4 }, organizations);

			var filterDepartment = (ModuleTextFilter)filterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffDepartment];
			filterDepartment.Property = "ALL";
			filterDepartment.IsActive = true;

			filterQuery = filterStripBizO.Filter;
			organizations = new OrgHeaderCollection(Factory, filterQuery);
			organizations.Load();

			AssertEquals("Organisation Count - Role, Staff, Company and Department", 1, organizations.Count);
			AssertContainsExactElementsInAnyOrder("Organisation values - Role, Staff, Company and Department", new[] { organization1 }, organizations);
		}

		public void TestStaffAssignmentsFilter()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var org1Assignment1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			org1Assignment1.O8_OH = org1.PK;
			org1Assignment1.O8_GC = company.PK;
			org1Assignment1.O8_Role = StaffAssignmentRoles.Codes.AccountManager;

			var org1Assignment2 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			org1Assignment2.O8_OH = org1.PK;
			org1Assignment2.O8_GC = company.PK;
			org1Assignment2.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			var org2Assignment = Factory.NewWithValidTestData<OrgStaffAssignments>();
			org2Assignment.O8_OH = org2.PK;
			org2Assignment.O8_GC = GlbCompany.CurrentCompany.PK;
			org2Assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			var org3Assignment = Factory.NewWithValidTestData<OrgStaffAssignments>();
			org3Assignment.O8_OH = org3.PK;
			org3Assignment.O8_GC = company.PK;
			org3Assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			Factory.Save();

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidForeignCollectionFilter)filterStripBizO["Staff Assignments"];
			filter.IsActive = true;

			filter.SelectedFilters.AddTextFilterStrip("Role", StaffAssignmentRoles.Codes.SalesRep);
			filter.SelectedFilters.AddGuidFilterStrip("Company", company.PK);

			AssertEquals("Precondition: ComparisonOperator", ModuleTextFilter.ComparisonConstants.AnyMatch, filter.ComparisonOperator);
			var orgCollection = new OrgHeaderCollection(Factory, filterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1, org3 }, orgCollection);
		}

		public void TestStaffAssignmentFilters_HiddenWhenViewSecurityCheckpointIsDisallowed()
		{
			var staffAssignmentFilters = new string[]
			{
				OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffRole,
				OrgConstants.FilterControl.GuidRelationships.Description.RMStaff,
				OrgConstants.FilterControl.DropEditRelationships.Description.RMStaffDepartment,
				OrgConstants.FilterControl.GuidRelationships.Description.RMStaffCompany,
				"Staff Assignments"
			};

			Env.Security.OrgDetailsViewCompanysStaffAssignments.IsAllowed = true;
			foreach (var filter in staffAssignmentFilters)
			{
				AssertFilterExists(OrgModuleType.Standard, filter, expectedExists: true);
			}

			Env.Security.OrgDetailsViewCompanysStaffAssignments.IsAllowed = false;
			foreach (var permission in staffAssignmentFilters)
			{
				AssertFilterExists(OrgModuleType.Standard, permission, expectedExists: false);
			}
		}

		#endregion

		#endregion

		#region Relationship Flags Filters

		public void TestCustomsCodeType()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			OrgCusCode orgCusCode1 = Factory.NewWithValidTestData<OrgCusCode>();
			OrgCusCode orgCusCode2 = Factory.NewWithValidTestData<OrgCusCode>();

			orgCusCode1.OK_OH = org1.PK;
			orgCusCode1.OK_CodeType = OrgCusCode.CodeTypes.UniversalNettingCode;

			orgCusCode2.OK_OH = org2.PK;
			orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.UniversalOfficeCode;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CustomsCodeType];
			filter.Property = OrgCusCode.CodeTypes.UniversalNettingCode;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		public void TestGlobalRateTarriff()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			OrgRateTariffLevel orgTariffLevel1 = Factory.NewWithValidTestData<OrgRateTariffLevel>();
			OrgRateTariffLevel orgTariffLevel2 = Factory.NewWithValidTestData<OrgRateTariffLevel>();
			OrgRateTariffLevel orgTariffLevel3 = Factory.NewWithValidTestData<OrgRateTariffLevel>();

			ZByte result1 = (ZByte)1;
			ZByte result2 = (ZByte)2;

			orgTariffLevel1.P7_OH = org1.PK;
			orgTariffLevel1.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;
			orgTariffLevel1.P7_TariffLevel = result1;
			orgTariffLevel1.P7_GC = GlbCompany.CurrentCompany.PK;
			orgTariffLevel1.P7_Direction = "EXP";

			orgTariffLevel2.P7_OH = org2.PK;
			orgTariffLevel2.P7_TariffType = OrgRateTariffLevel.DefaultTariffType;
			orgTariffLevel2.P7_TariffLevel = result2;
			orgTariffLevel2.P7_GC = GlbCompany.CurrentCompany.PK;
			orgTariffLevel2.P7_Direction = "IMP";

			orgTariffLevel3.P7_OH = org3.PK;
			orgTariffLevel3.P7_TariffType = "TRN";
			orgTariffLevel3.P7_TariffLevel = result1;
			orgTariffLevel3.P7_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.GlobalRateTarriff];
			filter.Property = "1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));
		}

		public void TestARAcctRelationship()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;

			org1.CompanyData.OB_ARCategory = "XXX";
			org2.CompanyData.OB_ARCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.ARAcctRelationship];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestARConsolidation()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;

			org1.CompanyData.OB_ARConsolidatedAccountingCategory = "XXX";
			org2.CompanyData.OB_ARConsolidatedAccountingCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.ARConsolidation];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestARStdInvoiceTerms()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org6 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org7 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg6 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg7 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;
			org3.OH_IsDebtor = false;
			org4.OH_IsDebtor = true;
			org5.OH_IsDebtor = true;
			org6.OH_IsDebtor = false;
			org7.OH_IsDebtor = true;

			settlementOrg4.OH_IsDebtor = true;
			settlementOrg5.OH_IsDebtor = true;
			settlementOrg6.OH_IsDebtor = true;
			settlementOrg7.OH_IsDebtor = false;

			org1.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			org2.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			org3.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			org4.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org5.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org6.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org7.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;

			org4.ARSettlementGroupPK = settlementOrg4.PK;
			org5.ARSettlementGroupPK = settlementOrg5.PK;
			org6.ARSettlementGroupPK = settlementOrg6.PK;
			org7.ARSettlementGroupPK = settlementOrg7.PK;

			settlementOrg4.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg5.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			settlementOrg6.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg7.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			Factory.Save();

			ModuleTextFilter filter1 = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.ARStdInvoiceTerms];
			filter1.Property = Constants.InvoiceTerms.FromInvoiceDate;
			filter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter1.OrCategory = FilterOrCategory.Blue;
			filter1.IsActive = true;

			ModuleTextFilter filter2 = (ModuleTextFilter)FilterStripBizO.CreateDuplicateFor(OrgConstants.FilterControl.DropEditRelationships.Description.ARStdInvoiceTerms);
			filter2.Property = Constants.InvoiceTerms.FromCustomsClearanceDate;
			filter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter2.OrCategory = FilterOrCategory.Blue;
			filter2.IsActive = true;

			ModuleTextFilter filter3 = (ModuleTextFilter)FilterStripBizO.CreateDuplicateFor(OrgConstants.FilterControl.DropEditRelationships.Description.ARStdInvoiceTerms);
			filter3.Property = Constants.InvoiceTerms.FromInvoiceDate;
			filter3.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter3.OrCategory = FilterOrCategory.Red;
			filter3.IsActive = true;

			ModuleTextFilter filter4 = (ModuleTextFilter)FilterStripBizO.CreateDuplicateFor(OrgConstants.FilterControl.DropEditRelationships.Description.ARStdInvoiceTerms);
			filter4.Property = Constants.InvoiceTerms.FromMonthEnd;
			filter4.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter4.OrCategory = FilterOrCategory.Red;
			filter4.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org4.", orgCollection.Contains(org4));
			Assert("Expect collection not to contain org5.", !orgCollection.Contains(org5));
			Assert("Expect collection not to contain org6.", !orgCollection.Contains(org6));
			Assert("Expect collection to contain org7.", orgCollection.Contains(org7));
		}

		public void TestARDisbInvoiceTerms()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org6 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org7 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg6 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg7 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;
			org3.OH_IsDebtor = false;
			org4.OH_IsDebtor = true;
			org5.OH_IsDebtor = true;
			org6.OH_IsDebtor = false;
			org7.OH_IsDebtor = true;

			settlementOrg4.OH_IsDebtor = true;
			settlementOrg5.OH_IsDebtor = true;
			settlementOrg6.OH_IsDebtor = true;
			settlementOrg7.OH_IsDebtor = false;

			org1.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			org2.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			org3.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			org4.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org5.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org6.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org7.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;

			org4.ARSettlementGroupPK = settlementOrg4.PK;
			org5.ARSettlementGroupPK = settlementOrg5.PK;
			org6.ARSettlementGroupPK = settlementOrg6.PK;
			org7.ARSettlementGroupPK = settlementOrg7.PK;

			settlementOrg4.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg5.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			settlementOrg6.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg7.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.ARDisbInvoiceTerms];
			filter.Property = Constants.InvoiceTerms.FromInvoiceDate;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org4.", orgCollection.Contains(org4));
			Assert("Expect collection not to contain org5.", !orgCollection.Contains(org5));
			Assert("Expect collection not to contain org6.", !orgCollection.Contains(org6));
			Assert("Expect collection to contain org7.", orgCollection.Contains(org7));
		}

		public void TestARDisbInvoiceTermsWithFallback()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;
			org3.OH_IsDebtor = true;
			org4.OH_IsDebtor = false;
			org5.OH_IsDebtor = true;

			org1.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			org1.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			org2.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			org2.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;

			org3.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			org4.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			org4.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			org5.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			org5.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.ARDisbInvoiceTerms];
			filter.Property = Constants.InvoiceTerms.FromInvoiceDate;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));
			Assert("Expect collection not to contain org4.", !orgCollection.Contains(org4));
			Assert("Expect collection to contain org5.", orgCollection.Contains(org5));
		}

		public void TestARDisbInvoiceTermsWithFallbackForDefaultTerms()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org6 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg6 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org2.OH_IsDebtor = true;
			org3.OH_IsDebtor = true;
			org4.OH_IsDebtor = true;
			org5.OH_IsDebtor = true;
			org6.OH_IsDebtor = true;

			settlementOrg1.OH_IsDebtor = true;
			settlementOrg2.OH_IsDebtor = true;
			settlementOrg3.OH_IsDebtor = true;
			settlementOrg4.OH_IsDebtor = false;
			settlementOrg5.OH_IsDebtor = true;
			settlementOrg6.OH_IsDebtor = true;

			org1.ARSettlementGroupPK = settlementOrg1.PK;
			org2.ARSettlementGroupPK = settlementOrg2.PK;
			org3.ARSettlementGroupPK = settlementOrg3.PK;
			org4.ARSettlementGroupPK = settlementOrg4.PK;
			org5.ARSettlementGroupPK = settlementOrg5.PK;
			org6.ARSettlementGroupPK = settlementOrg6.PK;

			org1.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			settlementOrg1.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg1.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;

			org2.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			settlementOrg2.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg2.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;

			org3.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			settlementOrg3.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			org4.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			settlementOrg4.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg4.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			org5.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			settlementOrg5.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			settlementOrg5.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			org6.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			settlementOrg6.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Constants.InvoiceTerms.CashOnDelivery;
			settlementOrg6.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.ARDisbInvoiceTerms];
			filter.Property = Constants.InvoiceTerms.FromInvoiceDate;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));
			Assert("Expect collection to contain org4.", orgCollection.Contains(org4));
			Assert("Expect collection to contain org5.", orgCollection.Contains(org5));
			Assert("Expect collection not to contain org6.", !orgCollection.Contains(org6));
		}

		public void TestAPAcctRelationship()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsCreditor = true;
			org2.OH_IsCreditor = true;

			org1.CompanyData.OB_APCategory = "XXX";
			org2.CompanyData.OB_APCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.APAcctRelationship];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestAPConsolidation()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsCreditor = true;
			org2.OH_IsCreditor = true;

			org1.CompanyData.OB_ARConsolidatedAccountingCategory = "XXX";
			org2.CompanyData.OB_ARConsolidatedAccountingCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.APConsolidation];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestAPPaymentTerms()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org6 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org7 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg6 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader settlementOrg7 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsCreditor = true;
			org2.OH_IsCreditor = true;
			org3.OH_IsCreditor = false;
			org4.OH_IsCreditor = true;
			org5.OH_IsCreditor = true;
			org6.OH_IsCreditor = false;
			org7.OH_IsCreditor = true;

			settlementOrg4.OH_IsCreditor = true;
			settlementOrg5.OH_IsCreditor = true;
			settlementOrg6.OH_IsCreditor = true;
			settlementOrg7.OH_IsCreditor = false;

			org1.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromInvoiceDate;
			org2.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.CashOnDelivery;
			org3.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromInvoiceDate;

			org4.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org5.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org6.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			org7.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;

			org4.APSettlementGroupPK = settlementOrg4.PK;
			org5.APSettlementGroupPK = settlementOrg5.PK;
			org6.APSettlementGroupPK = settlementOrg6.PK;
			org7.APSettlementGroupPK = settlementOrg7.PK;

			settlementOrg4.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg5.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.CashOnDelivery;
			settlementOrg6.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromInvoiceDate;
			settlementOrg7.CompanyData.OB_APPaymentTerms = Constants.InvoiceTerms.FromInvoiceDate;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.APPaymentTerms];
			filter.Property = Constants.InvoiceTerms.FromInvoiceDate;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org4.", orgCollection.Contains(org4));
			Assert("Expect collection not to contain org5.", !orgCollection.Contains(org5));
			Assert("Expect collection not to contain org6.", !orgCollection.Contains(org6));
			Assert("Expect collection to contain org7.", orgCollection.Contains(org7));
		}

		public void TestCNRExportCategory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			org1.MiscServ.OM_EXExporterCategory = "XXX";
			org2.MiscServ.OM_EXExporterCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CNRExportCategory];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestCNRINCOTerm()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			org1.MiscServ.OM_EXDefaultIncoTerm = "XXX";
			org2.MiscServ.OM_EXDefaultIncoTerm = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CNRINCOTerm];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestCNEImportCategory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			org1.MiscServ.OM_IMImporterCategory = "XXX";
			org2.MiscServ.OM_IMImporterCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CNEImportCategory];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestCNEMergeCusLines()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			org1.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "XXX";
			org2.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CNEMergeCusLines];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestCNESendAirDocs()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			org1.MiscServ.OM_IMSendImportDocsTo = "XXX";
			org2.MiscServ.OM_IMSendImportDocsTo = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CNESendAirDocs];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestCNESendSeaDocs()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			org1.MiscServ.OM_IMSendSeaImportDocsTo = "XXX";
			org2.MiscServ.OM_IMSendSeaImportDocsTo = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CNESendSeaDocs];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestMCRCarrierCategory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsShippingProvider = true;
			org2.OH_IsShippingProvider = true;

			org1.MiscServ.OM_CRCarrierCategory = "XXX";
			org2.MiscServ.OM_CRCarrierCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.MCRCarrierCategory];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestFWDAgentCategory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsForwarder = true;
			org2.OH_IsForwarder = true;

			org1.MiscServ.OM_FWAgentCategory = "XXX";
			org2.MiscServ.OM_FWAgentCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.FWDAgentCategory];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSRVUsagePreference()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsMiscFreightServices = true;
			org2.OH_IsMiscFreightServices = true;

			org1.MiscServ.OM_SVServicesCategory = "XXX";
			org2.MiscServ.OM_SVServicesCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SRVUsagePreference];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestADRType()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress1.OA_Address1 = "don't know";
			orgAddress2.OA_Address1 = "don't remember";

			OrgAddressCapability orgAddressCapability1 = Factory.NewWithValidTestData<OrgAddressCapability>();
			OrgAddressCapability orgAddressCapability2 = Factory.NewWithValidTestData<OrgAddressCapability>();
			orgAddressCapability1.PZ_OA = orgAddress1.PK;
			orgAddressCapability2.PZ_OA = orgAddress2.PK;
			orgAddressCapability1.PZ_AddressType = "XXX";
			orgAddressCapability2.PZ_AddressType = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.ADRType];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestADRLanguage()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress1.OA_Address1 = "don't know";
			orgAddress2.OA_Address1 = "don't remember";
			orgAddress1.OA_Language = Core.SharedConstants.Languages.French;
			orgAddress2.OA_Language = Core.SharedConstants.Languages.EnglishAmerican;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.ADRLanguage];
			filter.Property = Core.SharedConstants.Languages.French;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestFCLEquipment()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress1.OA_Address1 = "don't know";
			orgAddress2.OA_Address1 = "don't remember";
			orgAddress1.OA_FCLEquipmentNeeded = "XXX";
			orgAddress2.OA_FCLEquipmentNeeded = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.FCLEquipment];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestLCLEquipment()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress1.OA_Address1 = "don't know";
			orgAddress2.OA_Address1 = "don't remember";
			orgAddress1.OA_LCLEquipmentNeeded = "XXX";
			orgAddress2.OA_LCLEquipmentNeeded = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.LCLEquipment];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestAirEquipment()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress orgAddress1 = org1.Addresses.AddNew();
			OrgAddress orgAddress2 = org2.Addresses.AddNew();
			orgAddress1.OA_Address1 = "don't know";
			orgAddress2.OA_Address1 = "don't remember";
			orgAddress1.OA_AIREquipmentNeeded = "XXX";
			orgAddress2.OA_AIREquipmentNeeded = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.AirEquipment];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestFeesAndCharges()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			var rateFeeChargeLevel1 = Factory.NewWithValidTestData<OrgRateFeeChargeLevel>();
			var rateFeeChargeLevel2 = Factory.NewWithValidTestData<OrgRateFeeChargeLevel>();
			var rateFeeChargeLevel3 = Factory.NewWithValidTestData<OrgRateFeeChargeLevel>();
			var rateFeeChargeLevel4 = Factory.NewWithValidTestData<OrgRateFeeChargeLevel>();

			rateFeeChargeLevel1.ORF_OH = org1.PK;
			rateFeeChargeLevel1.ORF_ServiceType = "IWY";
			rateFeeChargeLevel1.ORF_Level = "STD";

			rateFeeChargeLevel2.ORF_OH = org2.PK;
			rateFeeChargeLevel2.ORF_ServiceType = "IWY";
			rateFeeChargeLevel2.ORF_Level = "";

			rateFeeChargeLevel3.ORF_OH = org3.PK;
			rateFeeChargeLevel3.ORF_ServiceType = "DWY";
			rateFeeChargeLevel3.ORF_Level = "STD";

			rateFeeChargeLevel4.ORF_OH = org4.PK;
			rateFeeChargeLevel4.ORF_ServiceType = "DWY";
			rateFeeChargeLevel4.ORF_Level = "";

			Factory.Save();

			AssertExpectedOrgHeaderResult("IWY", ZString.Empty, new List<OrgHeader>() { org1, org2 });
			AssertExpectedOrgHeaderResult("IWY", "STD", new List<OrgHeader>() { org1 });
			AssertExpectedOrgHeaderResult("DWY", ZString.Empty, new List<OrgHeader>() { org3, org4 });
			AssertExpectedOrgHeaderResult("DWY", "STD", new List<OrgHeader>() { org3 });
		}

		void AssertExpectedOrgHeaderResult(ZString type, ZString level, IList<OrgHeader> expectedHeaders)
		{
			var filter = (FeesAndChargesFilter)FilterStripBizO["Fees And Charges"];
			filter.ServiceType = type;
			filter.ServiceLevel = level;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals(expectedHeaders.Count, orgCollection.Count);
			AssertContainsExactElementsInAnyOrder(expectedHeaders, orgCollection.ToList());
		}

		public void TestSALCategory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMSalesCategory = "XXX";
			org2.MiscServ.OM_CMSalesCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALCategory];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALClientSize()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMClientSize = "XXX";
			org2.MiscServ.OM_CMClientSize = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALClientSize];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALIndustryVertical()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMIndustryVertical = "XXX";
			org2.MiscServ.OM_CMIndustryVertical = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALIndustryVertical];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALPeriodOfActivity()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMPeriodOfActivity = "XXX";
			org2.MiscServ.OM_CMPeriodOfActivity = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALPeriodOfActivity];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALTradeLaneIndustryVertical()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;
			org3.OH_IsSalesLead = true;
			org4.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMIndustryVertical = "XXX";
			org2.MiscServ.OM_CMIndustryVertical = "YYY";
			org3.MiscServ.OM_CMIndustryVertical = "";
			org4.MiscServ.OM_CMIndustryVertical = "XXX";

			var sales1 = org1.SalesCollection.AddNew();
			var tradeDetails1 = sales1.TradeDetails.AddNew();
			tradeDetails1.ProspectDetail.PAP_IndustryVertical = "YYY";

			var sales2 = org2.SalesCollection.AddNew();
			var tradeDetail2 = sales2.TradeDetails.AddNew();
			tradeDetail2.ProspectDetail.PAP_IndustryVertical = "";

			var sales3 = org3.SalesCollection.AddNew();
			var tradeDetail3 = sales3.TradeDetails.AddNew();
			tradeDetail3.ProspectDetail.PAP_IndustryVertical = "XXX";

			var salesWithoutPrimary = Factory.NewWithValidTestData<OrgSales>();
			salesWithoutPrimary.OW_OH_Buyer = org4.PK;
			var tradeDetailWithoutPrimary = salesWithoutPrimary.TradeDetails.AddNew();
			tradeDetailWithoutPrimary.ProspectDetail.PAP_IndustryVertical = "";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeLaneIndustryVertical];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org3, org4 }, orgCollection);

			filter.Property = "YYY";
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1, org2 }, orgCollection);
		}

		public void TestSALTradeLanePeriodOfActivity()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;
			org3.OH_IsSalesLead = true;
			org4.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMPeriodOfActivity = "XXX";
			org2.MiscServ.OM_CMPeriodOfActivity = "YYY";
			org3.MiscServ.OM_CMPeriodOfActivity = "";
			org4.MiscServ.OM_CMPeriodOfActivity = "XXX";

			var sales1 = org1.SalesCollection.AddNew();
			var tradeDetails1 = sales1.TradeDetails.AddNew();
			tradeDetails1.ProspectDetail.PAP_PeriodOfActivity = "YYY";

			var sales2 = org2.SalesCollection.AddNew();
			var tradeDetail2 = sales2.TradeDetails.AddNew();
			tradeDetail2.ProspectDetail.PAP_PeriodOfActivity = "";

			var sales3 = org3.SalesCollection.AddNew();
			var tradeDetail3 = sales3.TradeDetails.AddNew();
			tradeDetail3.ProspectDetail.PAP_PeriodOfActivity = "XXX";

			var salesWithoutPrimary = Factory.NewWithValidTestData<OrgSales>();
			salesWithoutPrimary.OW_OH_Buyer = org4.PK;
			var tradeDetailWithoutPrimary = salesWithoutPrimary.TradeDetails.AddNew();
			tradeDetailWithoutPrimary.ProspectDetail.PAP_PeriodOfActivity = "";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeLanePeriodOfActivity];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org3, org4 }, orgCollection);

			filter.Property = "YYY";
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { org1, org2 }, orgCollection);
		}

		public void TestSALTerritory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMSalesTerritory = "XXX";
			org2.MiscServ.OM_CMSalesTerritory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTerritory];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALOutlook()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMGrowthOutlook = "XXX";
			org2.MiscServ.OM_CMGrowthOutlook = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALOutlook];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALCompActivity()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMCompetitorActivity = "XXX";
			org2.MiscServ.OM_CMCompetitorActivity = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALCompActivity];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALAirCosts()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts = "XXX";
			org2.MiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALAirCosts];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALLCLCosts()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMOverallEffectOfClientOnLCLCosts = "XXX";
			org2.MiscServ.OM_CMOverallEffectOfClientOnLCLCosts = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALLCLCosts];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALTEUCosts()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMOverallEffectOfClientOnTEUCosts = "XXX";
			org2.MiscServ.OM_CMOverallEffectOfClientOnTEUCosts = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTEUCosts];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALWarehouseCosts()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts = "XXX";
			org2.MiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALWarehouseCosts];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALOtherCosts()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMOverallEffectOfClientOnOtherCosts = "XXX";
			org2.MiscServ.OM_CMOverallEffectOfClientOnOtherCosts = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALOtherCosts];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALClientRelationship()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMOverallClientRelation = (ZByte)1;
			org2.MiscServ.OM_CMOverallClientRelation = (ZByte)2;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALClientRelationship];
			filter.Property = "1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALDesireToRemain()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMClientsDesireToRemain = (ZByte)1;
			org2.MiscServ.OM_CMClientsDesireToRemain = (ZByte)2;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALDesireToRemain];
			filter.Property = "1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALEaseToPoach()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMEaseClientCanBePoached = (ZByte)1;
			org2.MiscServ.OM_CMEaseClientCanBePoached = (ZByte)2;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALEaseToPoach];
			filter.Property = "1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSALElectronicIntegration()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			org1.MiscServ.OM_CMAmountOfElectronicIntegration = (ZByte)1;
			org2.MiscServ.OM_CMAmountOfElectronicIntegration = (ZByte)2;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALElectronicIntegration];
			filter.Property = "1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestCMPServiceType()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsCompetitor = true;
			org2.OH_IsCompetitor = true;

			org1.MiscServ.OM_CITypeOfService = "XXX";
			org2.MiscServ.OM_CITypeOfService = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CMPServiceType];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestCMPSellStyle()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsCompetitor = true;
			org2.OH_IsCompetitor = true;

			org1.MiscServ.OM_CISellingStyle = "XXX";
			org2.MiscServ.OM_CISellingStyle = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CMPSellStyle];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestCMPCategory()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsCompetitor = true;
			org2.OH_IsCompetitor = true;

			org1.MiscServ.OM_CICompetitorCategory = "XXX";
			org2.MiscServ.OM_CICompetitorCategory = "YYY";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.CMPCategory];
			filter.Property = "XXX";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestSecurityGroup()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsCompetitor = true;
			org1.OH_FullName = "TestOrgHeader1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsCompetitor = true;
			org2.OH_FullName = "TestOrgHeader2";

			var glbGroup = Factory.NewWithValidTestData<GlbGroup>();
			org1.MiscServ.OM_GG_OrgSecurityGroup = glbGroup.PK;
			Factory.Save();

			var fullNameFilter = (ModuleTextFilter)FilterStripBizO["Name"];
			fullNameFilter.Property = "TestOrgHeader";
			fullNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			fullNameFilter.IsActive = true;
			var orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals(2, orgCollection.Count);
			AssertCollectionContains(org1, orgCollection);
			AssertCollectionContains(org2, orgCollection);

			var securityGroupfilter = (ModuleGuidFilter)FilterStripBizO["Security Group"];
			securityGroupfilter.Property = glbGroup.PK;
			securityGroupfilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			securityGroupfilter.IsActive = true;

			AssertEquals("Security Group", securityGroupfilter.Description);
			AssertEquals("Relationship Flags", securityGroupfilter.Category.Description);

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			AssertEquals(1, orgCollection.Count);
			AssertCollectionContains(org1, orgCollection);
		}

		#endregion

		#region Organisation Type Related Filters

		public void TestOrgTypeConsigneeRelatedConsignor()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignee = true;

			OrgHeader supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierBuyerLink orgSupplierBuyerLink1 = org1.SupplierLinks.AddNew();
			orgSupplierBuyerLink1.OL_OH_Buyer = org1.PK;
			orgSupplierBuyerLink1.OL_OH_Supplier = supplier1.PK;

			OrgSupplierBuyerLink orgSupplierBuyerLink2 = org2.SupplierLinks.AddNew();
			orgSupplierBuyerLink2.OL_OH_Buyer = org2.PK;
			orgSupplierBuyerLink2.OL_OH_Supplier = supplier2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Consignee - Related Consignor"];
			filter.Property = supplier1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestOrgTypeConsignorRelatedConsignee()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsConsignor = true;
			org2.OH_IsConsignor = true;

			OrgHeader buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader buyer2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierBuyerLink orgSupplierBuyerLink1 = org1.BuyerLinks.AddNew();
			orgSupplierBuyerLink1.OL_OH_Buyer = buyer1.PK;
			orgSupplierBuyerLink1.OL_OH_Supplier = org1.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Consignor - Related Consignee"];
			filter.Property = buyer1.PK;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestForwarderAgentStatus()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsForwarder = true;
			org2.OH_IsForwarder = true;

			OrgAppointedAgentPorts port1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port1.O5_OH = org1.PK;
			port1.O5_AirAgentStatus = AgentStatusList.Codes.Appointed;
			port1.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			port1.O5_RoadAgentStatus = AgentStatusList.Codes.Published;
			port1.O5_RailAgentStatus = AgentStatusList.Codes.Appointed;
			org1.AppointedAgentPorts.Add(port1);

			OrgAppointedAgentPorts port2 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port2.O5_OH = org2.PK;
			port2.O5_AirAgentStatus = AgentStatusList.Codes.Handles;
			port2.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
			port2.O5_RoadAgentStatus = AgentStatusList.Codes.Appointed;
			port2.O5_RailAgentStatus = AgentStatusList.Codes.Appointed;
			org2.AppointedAgentPorts.Add(port2);

			Factory.Save();

			OrgForwarderModuleFilter filter = (OrgForwarderModuleFilter)FilterStripBizO["Forwarder - Agent Status"];
			filter.Property = Core.Constants.TransportModes.Air;
			filter.Status = AgentStatusList.Codes.Appointed;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));

			filter.Property = Core.Constants.TransportModes.Sea;
			filter.Status = AgentStatusList.Codes.Handles;

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));

			filter.Property = Core.Constants.TransportModes.Road;
			filter.Status = AgentStatusList.Codes.Published;

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));

			filter.Property = Core.Constants.TransportModes.Rail;
			filter.Status = AgentStatusList.Codes.Appointed;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2.", orgCollection.Contains(org2));

			filter.Property = Core.Constants.TransportModes.Rail;
			filter.Status = AgentStatusList.Codes.Handles;
			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
		}

		public void TestReceivablesInvoiceNumber()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			AccTransactionHeader accTransactionHeader1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			accTransactionHeader1.AH_OH = org1.PK;
			accTransactionHeader1.AH_GB = GlbCompany.CurrentCompany.Branches[0].PK;
			accTransactionHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			accTransactionHeader1.AH_TransactionType = TransactionTypes.Invoice;
			accTransactionHeader1.AH_TransactionNum = "11111";
			accTransactionHeader1.AH_ReceiptBatchNo = "22222";
			accTransactionHeader1.AH_TransactionReference = "33333";
			accTransactionHeader1.AH_ConsolidatedInvoiceRef = "44444";

			AccTransactionHeader accTransactionHeader2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			accTransactionHeader2.AH_OH = org2.PK;
			accTransactionHeader2.AH_GB = GlbCompany.CurrentCompany.Branches[0].PK;
			accTransactionHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			accTransactionHeader2.AH_TransactionType = TransactionTypes.Invoice;
			accTransactionHeader2.AH_TransactionNum = "54321";
			accTransactionHeader2.AH_ReceiptBatchNo = "12345";
			accTransactionHeader2.AH_TransactionReference = "54321";
			accTransactionHeader2.AH_ConsolidatedInvoiceRef = "12345";

			Factory.Save();

			OrgReceivablesModuleFilter filter = (OrgReceivablesModuleFilter)FilterStripBizO["Receivables - Invoice Number"];
			filter.Property = "11111";
			filter.InvoiceType = Enterprise.MasterFiles.Module.OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.InvoiceNumber;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));

			filter.Property = "22222";
			filter.InvoiceType = Enterprise.MasterFiles.Module.OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.BatchInvoiceNumber;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));

			filter.Property = "33333";
			filter.InvoiceType = Enterprise.MasterFiles.Module.OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.GovtTaxNumber;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));

			filter.Property = "44444";
			filter.InvoiceType = Enterprise.MasterFiles.Module.OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.JobInvoiceNumber;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));

			filter.Property = "1";
			filter.InvoiceType = Enterprise.MasterFiles.Module.OrgReceivablesModuleFilter.InvoiceTypeConstants.Codes.All;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2.", orgCollection.Contains(org2));
		}

		public void TestSalesMainCompetitorOnForwarding()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org2);
			var customsCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Forwarding, forwardingCompetitor1, forwardingCompetitor2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Forwarding, forwardingCompetitor3, org2);
		}

		public void TestSalesMainCompetitorOnCustoms()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var customsCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org2);
			var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Customs, customsCompetitor1, customsCompetitor2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Customs, customsCompetitor3, org2);
		}

		public void TestSalesMainCompetitorOnWarehouse()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var warehouseCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org1);
			var warehouseCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org1);
			var warehouseCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org2);
			var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Warehouse, warehouseCompetitor1, warehouseCompetitor2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.Warehouse, warehouseCompetitor3, org2);
		}

		public void TestSalesMainCompetitorOnLandTransport()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var landTransportCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org2);
			var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.LandTransport, landTransportCompetitor1, landTransportCompetitor2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.LandTransport, landTransportCompetitor3, org2);
		}

		public void TestSalesMainCompetitorOnCustomCompetitorType()
		{
			var testCompetitorCode = "TST";
			var testCollection = (OverrideImmuneCodeDescriptionBoolCollection)OrganisationsDataRegistry.Instance.CompetitorType.Value;
			testCollection.AddSystemDefined(testCompetitorCode, (NoResString)"TST REGISTRY", false);

			using (OrganisationsDataRegistry.Instance.CompetitorType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();

				var customCompetitor1 = SetCompetitorToOrg(testCompetitorCode, org1);
				var customCompetitor2 = SetCompetitorToOrg(testCompetitorCode, org1);
				var customCompetitor3 = SetCompetitorToOrg(testCompetitorCode, org2);
				var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

				Factory.Save();

				AssertSalesMainCompetitorOnFilter(testCompetitorCode, customCompetitor1, customCompetitor2, 2, org1);
				AssertSalesMainCompetitorOnFilter(testCompetitorCode, customCompetitor3, org2);
			}
		}

		public void TestSalesMainCompetitorGCCompanyIsNullOrCurrentCompany()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var companyNew = Factory.NewWithValidTestData<GlbCompany>();

			var landTransportCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1, GlbCompany.CurrentCompany.PK);
			var landTransportCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org2, GlbCompany.CurrentCompany.PK);
			var landTransportCompetitor4 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org2, companyNew.PK);
			var forwardingCompetitor = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);

			Factory.Save();

			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.LandTransport, landTransportCompetitor1, landTransportCompetitor2, 2, org1);
			AssertSalesMainCompetitorOnFilter(CompetitorTypeList.Codes.LandTransport, landTransportCompetitor3, org2);
		}

		OrgHeader SetCompetitorToOrg(string competitorType, OrgHeader org, ZGuid? gcCompany = null)
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgCompetitor>();
			competitor.OCP_Type = competitorType;
			competitor.OCP_OH_Competitor = org.PK;
			competitor.OCP_OH_Parent = parentOrg.PK;
			if (gcCompany != null)
			{
				competitor.OCP_GC_Company = (ZGuid)gcCompany;
			}
			return parentOrg;
		}

		void AssertSalesMainCompetitorOnFilter(ZString competitorType, OrgHeader expected, OrgHeader targetOrg)
		{
			AssertSalesMainCompetitorOnFilter(competitorType, expected, null, 1, targetOrg);
		}

		void AssertSalesMainCompetitorOnFilter(ZString competitorType, OrgHeader expected1, OrgHeader expected2, int expectedCount, OrgHeader targetOrg)
		{
			var filter = (OrgSalesMainCompetitorModuleFilter)FilterStripBizO["Sales Main Competitor On"];
			filter.IsActive = true;
			filter.CompetitorType = competitorType;
			filter.Competitor = targetOrg.PK;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Collection contains the expected number of orgs", orgCollection.Count == expectedCount);
			if (expectedCount == 2)
			{
				Assert("Expect collection to contain the first org", orgCollection.Contains(expected1));
				Assert("Expect collection to contain the second org", orgCollection.Contains(expected2));
			}
			if (expectedCount == 1)
			{
				Assert("Expect collection to contain the org.", orgCollection.Contains(expected1));
			}
		}

		public void TestHasMainCompetitorOnCustoms()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var customsCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org2);
			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.Customs, allOrgs: new[] { customsCompetitor1, customsCompetitor2, customsCompetitor3, forwardingCompetitor1, forwardingCompetitor2 }, expectedOrgsWithCompetitor: new[] { customsCompetitor1, customsCompetitor2, customsCompetitor3 });
		}

		public void TestHasMainCompetitorOnForwarding()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org2);
			var customsCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org1);
			var customsCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Customs, org3);

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.Forwarding, allOrgs: new[] { forwardingCompetitor1, forwardingCompetitor2, forwardingCompetitor3, customsCompetitor1, customsCompetitor2 }, expectedOrgsWithCompetitor: new[] { forwardingCompetitor1, forwardingCompetitor2, forwardingCompetitor3 });
		}

		public void TestHasMainCompetitorOnLandTransport()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var landTransportCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1);
			var landTransportCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org2);
			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.LandTransport, allOrgs: new[] { landTransportCompetitor1, landTransportCompetitor2, landTransportCompetitor3, forwardingCompetitor1, forwardingCompetitor2 }, expectedOrgsWithCompetitor: new[] { landTransportCompetitor1, landTransportCompetitor2, landTransportCompetitor3 });
		}

		public void TestHasMainCompetitorOnWarehouse()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var warehouseCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org1);
			var warehouseCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org1);
			var warehouseCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.Warehouse, org2);
			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.Warehouse, allOrgs: new[] { warehouseCompetitor1, warehouseCompetitor2, warehouseCompetitor3, forwardingCompetitor1, forwardingCompetitor2 }, expectedOrgsWithCompetitor: new[] { warehouseCompetitor1, warehouseCompetitor2, warehouseCompetitor3 });
		}

		public void TestHasMainCompetitorGCCompanyIsNullOrCurrentCompany()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var companyNew = Factory.NewWithValidTestData<GlbCompany>();

			var landTransportCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1, GlbCompany.CurrentCompany.PK);
			var landTransportCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org1, companyNew.PK);
			var landTransportCompetitor3 = SetCompetitorToOrg(CompetitorTypeList.Codes.LandTransport, org2);
			var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1, GlbCompany.CurrentCompany.PK);
			var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

			Factory.Save();

			AssertHasMainCompetitorTypeOnFilter(CompetitorTypeList.Codes.LandTransport, allOrgs: new[] { landTransportCompetitor1, landTransportCompetitor2, landTransportCompetitor3, forwardingCompetitor1, forwardingCompetitor2 }, expectedOrgsWithCompetitor: new[] { landTransportCompetitor1, landTransportCompetitor3 });
		}

		public void TestHasMainCompetitorOnCustomizedCompetitor()
		{
			var testCompetitorCode = "TST";
			var testCollection = (OverrideImmuneCodeDescriptionBoolCollection)OrganisationsDataRegistry.Instance.CompetitorType.Value;
			testCollection.AddSystemDefined(testCompetitorCode, (NoResString)"TST REGISTRY", false);

			using (OrganisationsDataRegistry.Instance.CompetitorType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCollection))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();

				var customizedCompetitorType1 = SetCompetitorToOrg(testCompetitorCode, org1);
				var customizedCompetitorType2 = SetCompetitorToOrg(testCompetitorCode, org1);
				var customizedCompetitorType3 = SetCompetitorToOrg(testCompetitorCode, org2);
				var forwardingCompetitor1 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org1);
				var forwardingCompetitor2 = SetCompetitorToOrg(CompetitorTypeList.Codes.Forwarding, org3);

				Factory.Save();

				AssertHasMainCompetitorTypeOnFilter(testCompetitorCode, allOrgs: new[] { customizedCompetitorType1, customizedCompetitorType2, customizedCompetitorType3, forwardingCompetitor1, forwardingCompetitor2 }, expectedOrgsWithCompetitor: new[] { customizedCompetitorType1, customizedCompetitorType2, customizedCompetitorType3 });
			}
		}

		void AssertHasMainCompetitorTypeOnFilter(ZString competitorType, OrgHeader[] allOrgs, OrgHeader[] expectedOrgsWithCompetitor)
		{
			var filter = (OrgHasMainCompetitorModuleFilter)FilterStripBizO["Has Main Competitor On"];
			filter.IsActive = true;
			filter.CompetitorType = competitorType;
			filter.HasMainCompetitor = true;

			var filterQuery = FilterStripBizO.Filter;
			filterQuery.AddToFilter(OrgHeaderSchema.PK, allOrgs.Select(org => org.PK));

			var orgCollection = new OrgHeaderCollection(Factory);
			orgCollection.Load(filterQuery);
			AssertContainsExactElementsInAnyOrder(expectedOrgsWithCompetitor, orgCollection);

			filter.HasMainCompetitor = false;
			filterQuery = FilterStripBizO.Filter;
			filterQuery.AddToFilter(OrgHeaderSchema.PK, allOrgs.Select(org => org.PK));

			orgCollection.Load(filterQuery);
			var expectedOrgsWithoutCompetitor = allOrgs.Except(expectedOrgsWithCompetitor);
			AssertContainsExactElementsInAnyOrder(expectedOrgsWithoutCompetitor, orgCollection);
		}

		public void TestRegistrationCountryAndType()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org1, "AU", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "2342093043");
			AddOrgCusCode(org1, "NZ", OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "1234125");

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org2, "AU", OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, "65468468");
			AddOrgCusCode(org2, "US", OrgCusCode.USACodeTypes.CBPAssignedNumber, "6846688");

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org3, string.Empty, OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode, "32165484");
			AddOrgCusCode(org3, "US", OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, "98798757");

			Factory.Save();

			OrgRegistrationCountryAndTypeModuleFilter filter = (OrgRegistrationCountryAndTypeModuleFilter)FilterStripBizO["Registration Country/Type"];
			filter.IsActive = true;

			filter.Property1 = "AU";
			filter.Property2 = string.Empty;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection contains only two orgs", orgCollection.Count == 2);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2.", orgCollection.Contains(org2));

			filter.Property1 = "AU";
			filter.Property2 = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection contains only one org", orgCollection.Count == 1);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
		}

		public void TestNoRegistrationCountryAndType()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org1, "AU", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "LSK992342093043");
			AddOrgCusCode(org1, "NZ", OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "LSK991234125");

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org2, "AU", OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, "LSK9965468468");
			AddOrgCusCode(org2, "US", OrgCusCode.USACodeTypes.CBPAssignedNumber, "LSK996846688");

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org3, string.Empty, OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode, "LSK9932165484");
			AddOrgCusCode(org3, "US", OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, "LSK9998798757");

			Factory.Save();

			ModuleTextFilter additionalFilter = (ModuleTextFilter)FilterStripBizO["Registration Number"];
			additionalFilter.IsActive = true;
			additionalFilter.Property = "LSK99";

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection contains only three orgs", orgCollection.Count == 3);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2.", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));

			OrgRegistrationCountryAndTypeModuleFilter filter = (OrgRegistrationCountryAndTypeModuleFilter)FilterStripBizO["No Registration Country/Type"];
			filter.IsActive = true;

			filter.Property1 = "AU";
			filter.Property2 = string.Empty;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection contains only one org", orgCollection.Count == 1);
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));

			filter.Property1 = "AU";
			filter.Property2 = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;

			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection contains only one org", orgCollection.Count == 2);
			Assert("Expect collection to contain org2.", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));
		}

		public void TestRegistrationNoRegistrationCountryAndType()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org1, "AU", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "LSK992342093043");
			AddOrgCusCode(org1, "NZ", OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "LSK991234125");

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org2, "AU", OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, "LSK9965468468");
			AddOrgCusCode(org2, "US", OrgCusCode.USACodeTypes.CBPAssignedNumber, "LSK996846688");

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			AddOrgCusCode(org3, string.Empty, OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode, "LSK9932165484");
			AddOrgCusCode(org3, "US", OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, "LSK9998798757");

			Factory.Save();

			OrgRegistrationCountryAndTypeModuleFilter filter = (OrgRegistrationCountryAndTypeModuleFilter)FilterStripBizO["Registration Country/Type"];
			filter.IsActive = true;
			filter.Property1 = "AU";
			filter.Property2 = string.Empty;

			OrgRegistrationCountryAndTypeModuleFilter filter2 = (OrgRegistrationCountryAndTypeModuleFilter)FilterStripBizO["No Registration Country/Type"];
			filter2.IsActive = true;
			filter2.Property1 = "US";
			filter2.Property2 = OrgCusCode.USACodeTypes.CBPAssignedNumber;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();
			Assert("Expect collection contains only one org", orgCollection.Count == 1);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
		}

		void AddOrgCusCode(OrgHeader org, ZString countryCode, ZString codeType, ZString regNo)
		{
			OrgCusCode code = org.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = countryCode;
			code.OK_CodeType = codeType;
			code.OK_CustomsRegNo = regNo;
		}

		[StressTest]
		public void TestOrganisationTypes()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org5 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org6 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org7 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org8 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org9 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org10 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org11 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org12 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsDebtor = true;
			org2.OH_IsCreditor = true;
			org3.OH_IsConsignee = true;
			org4.OH_IsConsignor = true;
			org5.OH_IsShippingProvider = true;
			org6.OH_IsForwarder = true;
			org7.OH_IsTransportClient = true;
			org8.OH_IsWarehouseClient = true;
			org9.OH_IsBroker = true;
			org10.OH_IsMiscFreightServices = true;
			org11.OH_IsCompetitor = true;
			org12.OH_IsSalesLead = true;

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignor = true;

			Factory.Save();

			OrgTypeModuleFilter filter = (OrgTypeModuleFilter)FilterStripBizO["Organisation Types"];
			filter.Property0 = true; // IsDebtor
			filter.AndJoinCondition = true;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = true; //IsCreditor

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2.", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = true; //IsConsignee

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = true; //IsConsignor

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection to contain org2.", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org4.", orgCollection.Contains(org4));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = true; //OH_IsShippingProvider

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org5.", orgCollection.Contains(org5));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = true; //OH_IsForwarder

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org6.", orgCollection.Contains(org6));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = true; //OH_IsTransportClient

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org7.", orgCollection.Contains(org7));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = true; //OH_IsWarehouseClient

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org8.", orgCollection.Contains(org8));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = true; //OH_IsBroker

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org9.", orgCollection.Contains(org9));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = false;
			filter.Property9 = true; //OH_IsMiscFreightServices

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org10.", orgCollection.Contains(org10));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = false;
			filter.Property9 = false;
			filter.Property10 = true; //OH_IsCompetitor

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection to contain org11.", orgCollection.Contains(org11));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.Property0 = false;
			filter.Property1 = false;
			filter.Property2 = false;
			filter.Property3 = false;
			filter.Property4 = false;
			filter.Property5 = false;
			filter.Property6 = false;
			filter.Property7 = false;
			filter.Property8 = false;
			filter.Property9 = false;
			filter.Property10 = false;
			filter.Property11 = true; //OH_IsSalesLead

			orgCollection.Load(FilterStripBizO.Filter);
			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection not to contain org11.", !orgCollection.Contains(org11));
			Assert("Expect collection to contain org12.", orgCollection.Contains(org12));

			filter.Property0 = true;
			filter.Property1 = true;
			filter.Property2 = true;
			filter.Property3 = true;
			filter.Property4 = true;
			filter.Property5 = true;
			filter.Property6 = true;
			filter.Property7 = true;
			filter.Property8 = true;
			filter.Property9 = true;
			filter.Property10 = true;
			filter.Property11 = true;
			filter.AndJoinCondition = true;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
			Assert("Expect collection not to contain org4.", !orgCollection.Contains(org4));
			Assert("Expect collection not to contain org5.", !orgCollection.Contains(org5));
			Assert("Expect collection not to contain org6.", !orgCollection.Contains(org6));
			Assert("Expect collection not to contain org7.", !orgCollection.Contains(org7));
			Assert("Expect collection not to contain org8.", !orgCollection.Contains(org8));
			Assert("Expect collection not to contain org9.", !orgCollection.Contains(org9));
			Assert("Expect collection not to contain org10.", !orgCollection.Contains(org10));
			Assert("Expect collection not to contain org11.", !orgCollection.Contains(org11));
			Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

			filter.AndJoinCondition = false;
			filter.OrJoinCondition = true;

			orgCollection.Load(FilterStripBizO.Filter);

			Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2.", orgCollection.Contains(org2));
			Assert("Expect collection to contain org3.", orgCollection.Contains(org3));
			Assert("Expect collection to contain org4.", orgCollection.Contains(org4));
			Assert("Expect collection to contain org5.", orgCollection.Contains(org5));
			Assert("Expect collection to contain org6.", orgCollection.Contains(org6));
			Assert("Expect collection to contain org7.", orgCollection.Contains(org7));
			Assert("Expect collection to contain org8.", orgCollection.Contains(org8));
			Assert("Expect collection to contain org9.", orgCollection.Contains(org9));
			Assert("Expect collection to contain org10.", orgCollection.Contains(org10));
			Assert("Expect collection to contain org11.", orgCollection.Contains(org11));
			Assert("Expect collection to contain org12.", orgCollection.Contains(org12));
		}

		#endregion

		#region Related Trade Lane Filters

		public void TestSALTradeMode()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_IsSalesLead = true;
			org2.OH_IsSalesLead = true;

			OrgSales orgSales1 = Factory.NewWithValidTestData<OrgSales>();
			OrgSales orgSales2 = Factory.NewWithValidTestData<OrgSales>();
			orgSales1.OW_OH_Buyer = org1.PK;
			orgSales2.OW_OH_Supplier = org2.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeMode];
			filter.Property = Core.Constants.MovementCodes.Import;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			OrgHeaderCollection orgCollection1 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection1.Load();

			AssertContainsExactElementsInAnyOrder(new[] { org1 }, orgCollection1);
			filter.Property = Core.Constants.MovementCodes.Export;

			OrgHeaderCollection orgCollection2 = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection2.Load();

			AssertContainsExactElementsInAnyOrder(new[] { org2 }, orgCollection2);
		}

		public void TestSalesTradeLaneLocation()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var uschi = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USCHI");
			var deham = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEHAM");
			var aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsSalesLead = true;
			OrgSales sales11 = org1.SalesCollection.AddNew();
			sales11.OW_OriginID = ausyd.PK;
			sales11.OW_DestinationID = uslax.PK;
			sales11.OW_OH_Buyer = org1.PK;
			OrgSales sales12 = org1.SalesCollection.AddNew();
			sales12.OW_OriginID = aumel.PK;
			sales12.OW_DestinationID = uschi.PK;
			sales12.OW_OH_Supplier = org1.PK;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsSalesLead = true;
			OrgSales sales2 = org2.SalesCollection.AddNew();
			sales2.OW_OriginID = ausyd.PK;
			sales2.OW_DestinationID = deham.PK;
			sales2.OW_OH_Buyer = org2.PK;

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsSalesLead = true;
			OrgSales sales3 = org3.SalesCollection.AddNew();
			sales3.OW_OriginID = aubne.PK;
			sales3.OW_DestinationID = uslax.PK;
			sales3.OW_OH_Supplier = org3.PK;

			Factory.Save();

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			ModuleLocationFilter locationFilter = (ModuleLocationFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.SALTradeLaneLocation];
			locationFilter.IsActive = true;

			locationFilter.Property1 = "AUSYD";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(org1, collection);
			AssertCollectionContains(org2, collection);

			locationFilter.Property1 = "AUMEL";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(org1, collection);

			locationFilter.Property1 = "";
			locationFilter.Property2 = "USLAX";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(org1, collection);
			AssertCollectionContains(org3, collection);

			locationFilter.Property1 = "AUBNE";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(org3, collection);
		}

		public void TestSalesTradeLaneCommodity()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "XXXYYY";
			org1.OH_IsSalesLead = true;

			OrgSales orgSale1 = org1.SalesCollection.AddNew();
			orgSale1.OW_OH_Supplier = org1.PK;
			OrgTradeDetail orgTradeDetail1 = orgSale1.TradeDetails.AddNew();
			orgTradeDetail1.ProspectDetail.PAP_RH_NKCommodityCode = "ALUM";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "XXXXXX";
			org2.OH_IsSalesLead = true;

			OrgSales orgSale2 = org2.SalesCollection.AddNew();
			orgSale2.OW_OH_Buyer = org2.PK;
			OrgTradeDetail orgTradeDetail2 = orgSale2.TradeDetails.AddNew();
			orgTradeDetail2.ProspectDetail.PAP_RH_NKCommodityCode = "AABT";

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALTradeLaneCommodity];
			filter.Property = "ALUM";
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));

			filter.Property = "AABT";
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
		}

		public void TestSalesTradeLaneProductFilter()
		{
			IOrgSalesProduct forwarding = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			IOrgSalesProduct transport = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DDDSYD";
			org1.OH_IsSalesLead = true;

			OrgSales orgSale1 = org1.SalesCollection.AddNew();
			orgSale1.OW_OH_Supplier = org1.PK;
			orgSale1.OW_MP_Product = forwarding.Identifier;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "CLLMEL";
			org2.OH_IsSalesLead = true;

			OrgSales orgSale2 = org2.SalesCollection.AddNew();
			orgSale2.OW_MP_Product = transport.Identifier;
			orgSale2.OW_OH_Buyer = org2.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTransportMode];
			filter.Property = SystemDefinedSalesProductList.Codes.ForwardingShipment;
			filter.IsActive = true;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should contain org2", !collection.Contains(org2));

			filter.Property = SystemDefinedSalesProductList.Codes.Transport;
			collection.Load(FilterStripBizO.Filter);

			Assert("Should contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
		}

		public void TestSalesTradeLaneStatusFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DDDSYD";
			org1.OH_IsSalesLead = true;

			OrgSales orgSale1 = org1.SalesCollection.AddNew();
			orgSale1.OW_OH_Supplier = org1.PK;
			OrgTradeDetail orgTradeDetail1 = orgSale1.TradeDetails.AddNew();
			orgTradeDetail1.PA_Status = "NON";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "CLLMEL";
			org2.OH_IsSalesLead = true;

			OrgSales orgSale2 = org2.SalesCollection.AddNew();
			orgSale2.OW_OH_Buyer = org2.PK;
			OrgTradeDetail orgTradeDetail2 = orgSale2.TradeDetails.AddNew();
			orgTradeDetail2.PA_Status = "CNF";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeStatus];
			filter.Property = "NON";
			filter.IsActive = true;

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			Assert("Should contain org1", collection.Contains(org1));
			Assert("Should contain org2", !collection.Contains(org2));

			filter.Property = "CNF";
			collection.Load(FilterStripBizO.Filter);

			Assert("Should contain org1", !collection.Contains(org1));
			Assert("Should contain org2", collection.Contains(org2));
		}

		public void TestSalesTradeLaneFilterSubGroup()
		{
			var forwarding = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var transport = Factory.LoadFromNaturalKey<IOrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Transport);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var uschi = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USCHI");
			var deham = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEHAM");
			var aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsSalesLead = true;
			OrgSales sales11 = org1.SalesCollection.AddNew();
			sales11.OW_MP_Product = forwarding.Identifier;
			sales11.OW_OriginID = ausyd.PK;
			sales11.OW_DestinationID = uslax.PK;
			sales11.OW_OH_Buyer = org1.PK;
			OrgTradeDetail trade11 = sales11.TradeDetails.AddNew();
			trade11.PA_Status = "CNF";
			OrgTradeDetail trade12 = sales11.TradeDetails.AddNew();
			OrgSales sales12 = org1.SalesCollection.AddNew();
			sales12.OW_MP_Product = forwarding.Identifier;
			sales12.OW_OriginID = aumel.PK;
			sales12.OW_DestinationID = uschi.PK;
			sales12.OW_OH_Supplier = org1.PK;
			OrgTradeDetail trade13 = sales12.TradeDetails.AddNew();
			trade13.ProspectDetail.PAP_RH_NKCommodityCode = "ALUM";
			trade13.PA_Status = "QTE";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsSalesLead = true;
			OrgSales sales2 = org2.SalesCollection.AddNew();
			sales2.OW_MP_Product = transport.Identifier;
			sales2.OW_OriginID = ausyd.PK;
			sales2.OW_DestinationID = deham.PK;
			sales2.OW_OH_Buyer = org2.PK;
			OrgTradeDetail trade21 = sales2.TradeDetails.AddNew();
			trade21.ProspectDetail.PAP_RH_NKCommodityCode = "ALUM";
			trade21.PA_Status = "QTE";
			OrgTradeDetail trade22 = sales2.TradeDetails.AddNew();
			trade22.ProspectDetail.PAP_RH_NKCommodityCode = "AABT";
			OrgTradeDetail trade23 = sales2.TradeDetails.AddNew();
			trade23.ProspectDetail.PAP_RH_NKCommodityCode = "ALUM";

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsSalesLead = true;
			OrgSales sales3 = org3.SalesCollection.AddNew();
			sales3.OW_MP_Product = forwarding.Identifier;
			sales3.OW_OriginID = aubne.PK;
			sales3.OW_DestinationID = uslax.PK;
			sales3.OW_OH_Supplier = org3.PK;
			OrgTradeDetail trade31 = sales3.TradeDetails.AddNew();
			trade31.PA_Status = "CNF";

			Factory.Save();

			OrgHeaderCollection collection = new OrgHeaderCollection(Factory);

			ModuleTextFilter salesProductfilter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTransportMode];
			ModuleTextFilter directionFilter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeMode];
			ModuleLocationFilter locationFilter = (ModuleLocationFilter)FilterStripBizO[OrgConstants.FilterControl.UNLOCOType.SALTradeLaneLocation];
			ModuleNkFilter commodityFilter = (ModuleNkFilter)FilterStripBizO[OrgConstants.FilterControl.GuidRelationships.Description.SALTradeLaneCommodity];
			ModuleTextFilter statusfilter = (ModuleTextFilter)FilterStripBizO[OrgConstants.FilterControl.DropEditRelationships.Description.SALTradeStatus];

			directionFilter.IsActive = true;
			locationFilter.IsActive = false;
			salesProductfilter.IsActive = true;
			commodityFilter.IsActive = false;
			statusfilter.IsActive = false;

			salesProductfilter.Property = SystemDefinedSalesProductList.Codes.ForwardingShipment;
			directionFilter.Property = Core.Constants.Sales.Mode.Export;
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Export, Product: SHP", 2, collection.Count);
			AssertCollectionContains(org1, collection);
			AssertCollectionContains(org3, collection);

			directionFilter.Property = Core.Constants.Sales.Mode.Import;
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Import, Product: SHP", 1, collection.Count);
			AssertCollectionContains(org1, collection);

			locationFilter.IsActive = true;
			locationFilter.Property1 = "AUSYD";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Import, Product: SHP, Origin: AUSYD", 1, collection.Count);
			AssertCollectionContains(org1, collection);

			locationFilter.Property2 = "USLAX";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Import, Product: SHP, Origin: AUSYD, Dest: USLAX", 1, collection.Count);
			AssertCollectionContains(org1, collection);

			locationFilter.IsActive = false;
			commodityFilter.IsActive = true;
			commodityFilter.Property = "ALUM";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Import, Product: SHP, Comm: ALUM", 0, collection.Count);

			salesProductfilter.Property = SystemDefinedSalesProductList.Codes.Transport;
			commodityFilter.Property = "AABT";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Import, Product: TRN, Comm: AABT", 1, collection.Count);
			AssertCollectionContains(org2, collection);

			directionFilter.IsActive = true;
			locationFilter.IsActive = true;
			salesProductfilter.IsActive = false;
			commodityFilter.IsActive = false;

			directionFilter.Property = Core.Constants.Sales.Mode.Import;
			locationFilter.Property1 = "AUMEL";
			locationFilter.Property2 = "USCHI";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Import, Origin: AUMEL, Dest: USCHI", 0, collection.Count);

			salesProductfilter.IsActive = true;
			commodityFilter.IsActive = true;
			directionFilter.Property = Core.Constants.Sales.Mode.Export;
			salesProductfilter.Property = SystemDefinedSalesProductList.Codes.ForwardingShipment;
			commodityFilter.Property = "ALUM";

			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Export, Product: SHP, Origin: AUMEL, Dest: USCHI, Comm: ALUM", 1, collection.Count);
			AssertCollectionContains(org1, collection);

			statusfilter.IsActive = true;
			statusfilter.Property = "QTE";

			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Export, Product: SHP, Origin: AUMEL, Dest: USCHI, Comm: ALUM, Status: QTE", 1, collection.Count);
			AssertCollectionContains(org1, collection);

			statusfilter.Property = "NON";

			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Trade Mode: Export, Product: SHP, Origin: AUMEL, Dest: USCHI, Comm: ALUM, Status: NON", 0, collection.Count);

			directionFilter.IsActive = false;
			locationFilter.IsActive = true;
			salesProductfilter.IsActive = false;
			commodityFilter.IsActive = true;
			statusfilter.IsActive = false;
			locationFilter.Property1 = "AUSYD";
			locationFilter.Property2 = "";
			commodityFilter.Property = "ALUM";
			collection.Load(FilterStripBizO.Filter);
			AssertEquals("Origin: AUSYD, Comm: ALUM", 1, collection.Count);
			AssertCollectionContains(org2, collection);
		}

		#endregion

		#region Workflow Custom Filters

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var customFieldName = "State";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = customFieldName;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;

			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();

			var collection = new OrganisationFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[customFieldName]);
			AssertNotNull(collection[customFieldName + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		public void TestAddWorkflowCustomFieldsFilters()
		{
			ModuleFilterCollection filterCollection = new OrganisationFilterBusinessObject().ModuleFilters;

			AssertNull(filterCollection["C11"]);
			AssertNull(filterCollection["C12"]);
			AssertNull(filterCollection["C21"]);
			AssertNull(filterCollection["C22"]);
			AssertNull(filterCollection["Workflow Flags"]);
			AssertNull(filterCollection["C31"]);

			PrepareTemplates();

			filterCollection = new OrganisationFilterBusinessObject().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), filterCollection["C11"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["C12"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["C21"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C22"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			AssertNull(filterCollection["C31"]);
		}

		void PrepareTemplates()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.Organisation.Code;

			GenCustomColumnDefinition def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = JobInvoicingConsumerTypes.Organisation.Code;

			GenCustomColumnDefinition def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "C21";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;

			GenCustomColumnDefinition def22 = template2.GenCustomColumnDefinitions.AddNew();
			def22.XC_Name = "C22";
			def22.XC_Type = AddOnColumnDataType.Codes.Boolean;

			GenCustomColumnDefinition defDuplicate = template2.GenCustomColumnDefinitions.AddNew();
			defDuplicate.XC_Name = "C11";
			defDuplicate.XC_Type = AddOnColumnDataType.Codes.String;

			ProcessTaskTemplate template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "YYY";

			GenCustomColumnDefinition def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "C31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		#endregion

		#region Additional Defaults for Search

		public void TestAdditionalDefaultsSetupForSearch()
		{
			OrganisationsFindBoxCollection orgList = new OrganisationsFindBoxCollection(Factory);
			orgList.DefaultsForNewChild.Add(new OrgFieldDefault() { IsConditional = true, FieldName = OrgHeader.Schema.OH_FullName, Value = new ZString("org name") });
			orgList.DefaultsForNewChild.Add(new OrgFieldDefault() { IsConditional = true, FieldName = OrgAddress.Schema.OA_Address1, Value = new ZString("org address 1") });
			orgList.DefaultsForNewChild.Add(new OrgFieldDefault() { IsConditional = true, FieldName = OrgAddress.Schema.OA_PostCode, Value = new ZString("post code") });
			orgList.DefaultsForNewChild.Add(OrgHeader.Schema.OH_RL_NKClosestPort, "AUSYD");

			AssertEquals("doesn't contain filter for name", true, !orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName("Name")));
			AssertEquals("doesn't contain filter for addresss", true, !orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.Address)));
			AssertEquals("doesn't contain filter for postcode", true, !orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.PostCode)));

			FilterStripBizO.SetAdditionalFilterDefaults(OrgHeader.UnmatchedOrganisationCode, orgList);

			AssertEquals("contain filter for name", true, orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName("Name")));
			AssertEquals("contain filter for addresss", true, orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.Address)));
			AssertEquals("contain filter for postcode", true, orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.PostCode)));

			FilterStripBizO.SetAdditionalFilterDefaults("abc", orgList);
			AssertEquals("doesn't contain filter for name", true, !orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName("Name")));
			AssertEquals("doesn't contain filter for addresss", true, !orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.Address)));
			AssertEquals("doesn't contain filter for postcode", true, !orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.PostCode)));
		}

		OrganisationsFindBoxCollection OrgList { get; set; }

		public void TestAdditionalDefaultsSetupForSearch_Unmatched()
		{
			OrgList = new OrganisationsFindBoxCollection(Factory);
			var listPropertyDescriptor = KPropertyDescriptorCollection.FromType(GetType(), true).Find(nameof(OrgList), false);
			if (listPropertyDescriptor != null)
			{
				var shipment = Factory.New<ICommonShipment>() as IBusiness;

				var stmNote = Factory.NewWithValidTestData<StmNote>();
				stmNote.ST_ParentID = shipment.Identifier;
				stmNote.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Code;
				stmNote.ST_Table = shipment.TableName;
				stmNote.ST_NoteText = @"<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Consignor</OrganisationType><OrganisationSubType>Consignor</OrganisationSubType><OwnerCode /><EDICode>CONSALVADALV</EDICode>
<OrganisationName>CONSIGNOR</OrganisationName><AddressLine1>TIAN</AddressLine1><AddressLine2 /><City>NANJING</City><PostCode>21000</PostCode><StateOrProvince>32</StateOrProvince><Country>CN</Country><DocAddressType /></UnmatchOrgRecord></UnmatchOrgRecords>";
				Factory.Save();

				OrgList = new OrganisationsFindBoxCollection(Factory);
				OrgList.OrganisationType = OrganisationTypes.Consignor;
				OrgList.OrganisationSubType = ZString.Empty;
				OrgList.DocAddressType = ZString.Empty;

				((IBusinessObjectCollection)OrgList).Parent = shipment;
				((IBusinessObjectCollection)OrgList).ListPropertyDescriptor = listPropertyDescriptor;

				AssertEquals("precondition: ShouldSetValuesFromConditionalDefaults = false", false, ((IOrganisationDefaultProvider)OrgList).ShouldSetValuesFromConditionalDefaults);
				FilterStripBizO.SetAdditionalFilterDefaults(OrgHeader.UnmatchedOrganisationCode, OrgList);

				AssertEquals("contain filter for Name", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName("Name")));
				AssertEquals("contain filter for Address", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.Address)));
				AssertEquals("contain filter for PostCode", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.PostCode)));
				AssertEquals("contain filter for State", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.State)));
				AssertEquals("contain filter for City", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.City)));
			}
			else
			{
				Assert($"{GetType()} does not require this test.", true);
			}
		}

		public void TestAdditionalDefaultsSetupForSearch_Unmatched_ForPluginParent()
		{
			OrgList = new OrganisationsFindBoxCollection(Factory);
			var listPropertyDescriptor = KPropertyDescriptorCollection.FromType(GetType(), true).Find(nameof(OrgList), false);
			if (listPropertyDescriptor != null)
			{
				var pluginParent = new JobDocAddressCollectionForPlugin(new JobDocAddressCollection(Factory));
				pluginParent.HostParentBizo = Factory.New<ICommonShipment>() as IBusiness;

				var stmNote = Factory.NewWithValidTestData<StmNote>();
				stmNote.ST_ParentID = (pluginParent.HostParentBizo as BusinessObject).PK;
				stmNote.ST_Description = PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Code;
				stmNote.ST_Table = pluginParent.HostParentBizo.TableName;
				stmNote.ST_NoteText = @"<UnmatchOrgRecords><UnmatchOrgRecord><OrganisationType>Consignor</OrganisationType><OrganisationSubType>Consignor</OrganisationSubType><OwnerCode /><EDICode>CONSALVADALV</EDICode>
<OrganisationName>CONSIGNOR</OrganisationName><AddressLine1>TIAN</AddressLine1><AddressLine2 /><City>NANJING</City><PostCode>21000</PostCode><StateOrProvince>32</StateOrProvince><Country>CN</Country><DocAddressType /></UnmatchOrgRecord></UnmatchOrgRecords>";
				Factory.Save();

				OrgList = new OrganisationsFindBoxCollection(Factory);
				OrgList.OrganisationType = OrganisationTypes.Consignor;
				OrgList.OrganisationSubType = ZString.Empty;
				OrgList.DocAddressType = ZString.Empty;

				((IBusinessObjectCollection)OrgList).Parent = pluginParent;
				((IBusinessObjectCollection)OrgList).ListPropertyDescriptor = listPropertyDescriptor;

				AssertEquals("precondition: ShouldSetValuesFromConditionalDefaults = false", false, ((IOrganisationDefaultProvider)OrgList).ShouldSetValuesFromConditionalDefaults);
				FilterStripBizO.SetAdditionalFilterDefaults(OrgHeader.UnmatchedOrganisationCode, OrgList);

				AssertEquals("contain filter for Name", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName("Name")));
				AssertEquals("contain filter for Address", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.Address)));
				AssertEquals("contain filter for PostCode", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.PostCode)));
				AssertEquals("contain filter for State", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.State)));
				AssertEquals("contain filter for City", true, OrgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName(OrgConstants.FilterControl.OrgAddress.City)));
			}
			else
			{
				Assert($"{GetType()} does not require this test.", true);
			}
		}

		public void TestDefaultFiltersSetupForSearchShouldNotBeTranslatable()
		{
			GlbStaff currentUser = Factory.NewWithValidTestData<GlbStaff>();
			currentUser.GS_Code = "EON";
			currentUser.GS_LoginName = "french";
			currentUser.GS_GB_HomeBranch = Env.CurrentBranchPK;
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;

			Factory.Save();

			ExceptionReporterTestListener.Instance.Clear();

			OrganisationsFindBoxCollection orgList = new OrganisationsFindBoxCollection(Factory);
			orgList.DefaultsForNewChild.Add(new OrgFieldDefault() { IsConditional = true, FieldName = OrgHeader.Schema.OH_FullName, Value = new ZString("org name") });
			orgList.DefaultsForNewChild.Add(new OrgFieldDefault() { IsConditional = true, FieldName = OrgAddress.Schema.OA_Address1, Value = new ZString("org address 1") });
			orgList.DefaultsForNewChild.Add(new OrgFieldDefault() { IsConditional = true, FieldName = OrgAddress.Schema.OA_PostCode, Value = new ZString("post code") });
			orgList.DefaultsForNewChild.Add(OrgHeader.Schema.OH_RL_NKClosestPort, "AUSYD");

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				FilterStripBizO.SetAdditionalFilterDefaults(OrgHeader.UnmatchedOrganisationCode, orgList);
				AssertEquals("contain filter for name", true, orgList.FilterBusinessObjectDefaults.ContainsDefaultFor(GetFilterName("Name")));

				FilterStripBizO.SetExternalDefaults(orgList);
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		ZString GetFilterName(ZString filterCode)
		{
			return string.Concat(filterCode, FilterBusinessObjectDefault.FilterPropertyDelimiter, "Property");
		}

		#endregion

		#region ProductivityWise Mode

		public void TestExactListOfFilters_WhenProductivityWiseModeIsEnabled_ShouldExcludeLogisticsFilters()
		{
			var expectedProductivityWiseFilters = new[]
			{
				"Dates: A/R Account / Credit Review Due Date",
				"Dates: Created",
				"Dates: Date of Last QA Check For A/P",
				"Dates: Date of Last QA Check For A/R",
				"Dates: Date of Last Sales Call",
				"Dates: Date of Next Sales Call",
				"Dates: Date Sales Client Relationship Commenced",
				"Dates: Estimated Date to Close Business",
				"Dates: Last Un-Actioned Communication",
				"Dates: Power Of Attorney Valid To Date",
				"Locations: Main UNLOCO",
				"Locations: Located Within",
				"Locations: Country/Region",
				"Numbers and References: Achievable Business",
				"Numbers and References: Client Number",
				"Numbers and References: Code Mapping (Foreign Code)",
				"Numbers and References: Code Mapping (Local Code)",
				"Numbers and References: Related Staff",
				"Numbers and References: Known/Approved ID Number",
				"Organization Type: Organisation Types",
				"Organization Type: Receivables - Invoice Number",
				"Other: Accounting Transactions - Creditor",
				"Other: Accounting Transactions - Debtor",
				"Other: Global Credit Group",
				"Other: Min Transactions in Date Range",
				"Other: No Transaction",
				"Other: No Transaction in Current Company",
				"Other: Shipping Line",
				"Registration Numbers: No Registration Country/Type",
				"Registration Numbers: Registration Country/Type",
				"Relationship Flags: Address Language",
				"Relationship Flags: Address Type",
				"Relationship Flags: Has Main Competitor On",
				"Relationship Flags: Marketing Options",
				"Relationship Flags: Payables Accounts Relationship",
				"Relationship Flags: Payables Consolidation Category",
				"Relationship Flags: Payables Payment Terms",
				"Relationship Flags: Receivables Accounts Relationship",
				"Relationship Flags: Receivables Consolidation Category",
				"Relationship Flags: Receivables Disbursement Invoice Terms",
				"Relationship Flags: Receivables Standard Invoice Terms",
				"Relationship Flags: SAL Client Period of Activity",
				"Relationship Flags: SAL Client Vertical Market",
				"Relationship Flags: Sales Amount of Electronic Integration",
				"Relationship Flags: Sales Client Category",
				"Relationship Flags: Sales Client Desire To Remain With Company",
				"Relationship Flags: Sales Client Effect on Air Costs",
				"Relationship Flags: Sales Client Effect on LCL Costs",
				"Relationship Flags: Sales Client Effect on Other Costs",
				"Relationship Flags: Sales Client Effect on TEU Costs",
				"Relationship Flags: Sales Client Effect on Warehousing Costs",
				"Relationship Flags: Sales Client Growth Outlook",
				"Relationship Flags: Sales Client Size",
				"Relationship Flags: Sales Client Territory",
				"Relationship Flags: Sales Ease Client Can Be Poached",
				"Relationship Flags: Sales Main Competitor Activity",
				"Relationship Flags: Sales Main Competitor On",
				"Relationship Flags: Sales Overall Client Relation",
				"Relationship Flags: Service Provider Usage Preference",
				"Relationship Flags: Security Group",
				"Relationship Org./Staff: Payables Charge Code",
				"Relationship Org./Staff: Payables Creditor Group",
				"Relationship Org./Staff: Payables Default Bank Account",
				"Relationship Org./Staff: Receivables Currency",
				"Relationship Org./Staff: Receivables Debtor Group",
				"Relationship Org./Staff: Related Parties",
				"Relationship Org./Staff: Sales Export Air Representative",
				"Relationship Org./Staff: Sales Export Sea Representative",
				"Relationship Org./Staff: Sales Import Air Representative",
				"Relationship Org./Staff: Sales Import Sea Representative",
				"Relationship Org./Staff: Sales Main Export Commodity",
				"Relationship Org./Staff: Sales Main Import Commodity",
				"Relationship Org./Staff: Sales Overall Account Manager",
				"Relationship Org./Staff: Sales Overall Representative",
				"Relationship Org./Staff: Sales Rep Assigned",
				"Relationship Org./Staff: Sales Warehousing Representative",
				"Sales Relation Activity: Has Sales Relation",
				"Sales Relation Activity: Recent Activity Date",
				"Staff Assignments: RM Staff Company",
				"Staff Assignments: RM Staff Department",
				"Staff Assignments: RM Staff Initials",
				"Staff Assignments: RM Staff Role",
				"Staff Assignments: Staff Assignments",
				"Status and Flags: Organization – Account Type",
				"Status and Flags: Category",
				"Status and Flags: Credit Not Yet Approved",
				"Status and Flags: External Validation Status",
				"Status and Flags: Known/Approved Status",
				"Status and Flags: Language",
				"Status and Flags: Secondary Type",
				"Status and Flags: Shipping Line Integrations Enabled",
				"Text Search: Additional Company Names",
				"Text Search: Address",
				"Text Search: Address 1",
				"Text Search: Address 2",
				"Text Search: Additional Address Info",
				"Text Search: Branch",
				"Text Search: Branch Management Code",
				"Text Search: City",
				"Text Search: Code",
				"Text Search: Contact Email",
				"Text Search: Contact Fax",
				"Text Search: Contact Mobile",
				"Text Search: Contact Name",
				"Text Search: Contact Work Phone",
				"Text Search: Email",
				"Text Search: Fax",
				"Text Search: Mobile",
				"Text Search: Name",
				"Text Search: Phone",
				"Text Search: Post Code",
				"Text Search: Registration Number",
				"Text Search: Related Company Name",
				"Text Search: State",
				"Text Search: SystemDefinedOrg",
				"Text Search: Web",
				"Text Search: Shipping Line Name",
				"Text Search: Shipping Line SCAC",
				"Text Search: Shipping Line C1C",
				"Text Search: Shipping Line SCAC or C1C",
				"Text Search: Exclusive Gateway Service",
				"Text Search: External Debtor Code",
				"Text Search: External Creditor Code",
				"Workflow Milestones: Last Completed Milestone",
				"Workflow Milestones: Milestone Completed",
				"Workflow Milestones: Milestone Date",
				"Workflow Milestones: Next Milestone",
				"Workflow Tasks: Any Open Task Assigned To",
				"Workflow Tasks: Exceptions",
				"Workflow Tasks: Milestones",
				"Workflow Tasks: Next Task Assigned To",
				"Workflow Tasks: Tasks",
				"Workflow Tasks: Triggers",
				"Credit Scores: D&B Rating",
				"Credit Scores: Late Payment Risk",
				"Credit Scores: Failure Risk",
				"Other: Custom SQL Filter",
				"Document Tracking: Date Received",
				"Document Tracking: Document Type",
				"Document Tracking: Valid To Date",
				"Audit Information: Creating User",
				"Audit Information: Creating Branch",
				"Audit Information: Creating Department",
				"Audit Information: Created Time",
				"Audit Information: Last Edit User",
				"Audit Information: Last Edit Time",
				"Audit Information: Created On Web/Internal",
			};

			var nonProductivityWiseFilters = new[]
			{
				"Dates: Date of First Shipment From Buyer",
				"Dates: Date of First Shipment From Supplier",
				"Dates: Known Shipper Expiry Date",
				"Dates: Last Screening Date",
				"Locations: Carrier Appointed Port",
				"Locations: Competitors Lanes",
				"Locations: Forwarder Appointed Port",
				"Locations: Sales Trade Lanes",
				"Numbers and References: Delivery Route Sequence",
				"Organization Type: Consignee - Related Consignor",
				"Organization Type: Consignor - Related Consignee",
				"Organization Type: Forwarder - Agent Status",
				"Organization Type: Carrier - Carrier Category",
				"Related Trade Lanes: SAL Trade Lane Industry Vertical",
				"Related Trade Lanes: SAL Trade Lane Period of Activity",
				"Related Trade Lanes: Sales Monthly Trade Mode",
				"Related Trade Lanes: Sales Trade Lane Commodity",
				"Related Trade Lanes: Sales Trade Lane Origin / Destination",
				"Related Trade Lanes: Sales Trade Lane Status",
				"Related Trade Lanes: Sales Trade Lane Transport Mode",
				"Relationship Flags: Company Tariff",
				"Relationship Flags: Competitor Category",
				"Relationship Flags: Competitor Selling Style",
				"Relationship Flags: Competitor Service Type",
				"Relationship Flags: Consignee Importer Category",
				"Relationship Flags: Consignee Merge Customs Lines By",
				"Relationship Flags: Consignee Send Air Documents To",
				"Relationship Flags: Consignee Send Sea Documents To",
				"Relationship Flags: Consignor Exporter Category",
				"Relationship Flags: Consignor Incoterm",
				"Relationship Flags: Customs Code Type",
				"Relationship Flags: Equipment Needed For Air Drop Mode",
				"Relationship Flags: Equipment Needed For FCL Drop Mode",
				"Relationship Flags: Equipment Needed For LCL Drop Mode",
				"Relationship Flags: Fees And Charges",
				"Relationship Flags: Forwarder Agent Category",
				"Relationship Org./Staff: Consignee Air Port Transport Coordinator",
				"Relationship Org./Staff: Consignee Air Service Representative",
				"Relationship Org./Staff: Consignee Sea Port Transport Coordinator",
				"Relationship Org./Staff: Consignee Sea Service Representative",
				"Relationship Org./Staff: Consignor Air Port Transport Coordinator",
				"Relationship Org./Staff: Consignor Air Service Representative",
				"Relationship Org./Staff: Consignor Country of Origin",
				"Relationship Org./Staff: Consignor Default Currency",
				"Relationship Org./Staff: Consignor Sea Port Transport Coordinator",
				"Relationship Org./Staff: Consignor Sea Service Representative",
				"Relationship Org./Staff: Forwarder Default Currency",
				"Status and Flags: Rates' Security",
				"Text Search: Delivery Route",
				"Value Analysis: Organization Customs Brokerage Value Analysis",
				"Value Analysis: Organization Forwarding Value Analysis",
				"Value Analysis: Organization Liner & Agency Value Analysis",
				"Value Analysis: Organization Transport Value Analysis",
				"Value Analysis: Organization Warehouse Value Analysis",
			};

			var allFilters = expectedProductivityWiseFilters.Concat(nonProductivityWiseFilters);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			var filterBizo = new OrganisationFilterBusinessObject();
			AssertContainsExactElementsInAnyOrder("Only non-logistics-related filters should be included when ProductivityWise is enabled. If you've added a filter, please add it to the appropriate list so that logistics-related filters will not appear when logistics elements are hidden, such as in ProductivityWise mode.",
				expectedProductivityWiseFilters,
				filterBizo.ModuleFilters.Select(x => x.Category + ": " + x.Description));

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			filterBizo = new OrganisationFilterBusinessObject();
			AssertContainsExactElementsInAnyOrder("All filters, including logistics and non-logistics filters should be included when ProductivityWise is disabled. If you've added a filter, please add it to the appropriate list so that logistics-related filters will not appear when logistics elements are hidden, such as in ProductivityWise mode.",
				allFilters,
				filterBizo.ModuleFilters.Select(x => x.Category + ": " + x.Description));
		}

		#endregion

		public void TestImporterBondQueriedDate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "ORG1";
			org2.OH_Code = "ORG2";
			org3.OH_Code = "ORG3";
			org2.OH_Code = "ORG4";

			var cusCode1 = org1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode1.OK_CustomsRegNo = "11-1234567AA";
			((ILightValidationInternals)cusCode1).IsValid = true;
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode2.OK_CustomsRegNo = "22-1234567AA";
			((ILightValidationInternals)cusCode2).IsValid = true;
			var cusCode3 = org3.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			cusCode3.OK_CustomsRegNo = "33-1234567AA";
			((ILightValidationInternals)cusCode3).IsValid = true;
			EDIMessage message1 = (EDIMessage)Factory.New<Enterprise.Integration.Customs.US.IMQEDIMessage>();
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2016, 9, 21);
			message1.EM_IsActive = true;
			message1.EM_MessageType = "KI";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_LinkedObject = org1;
			EDIMessage message2 = (EDIMessage)Factory.New<Enterprise.Integration.Customs.US.IMQEDIMessage>();
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2016, 7, 21);
			message2.EM_IsActive = true;
			message2.EM_MessageType = "KI";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_LinkedObject = org2;
			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO[OrgConstants.FilterControl.OrgDateFilterList.Description.ImporterBondQueried];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2016, 9, 20).ToLocalBranchTime();
			filter.Property2 = new ZDateTime(2016, 9, 22).ToLocalBranchTime();
			filter.IsActive = true;

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
			Assert("Expect collection not to contain org4", !orgCollection.Contains(org4));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection to contain org2", orgCollection.Contains(org2));
			Assert("Expect collection not to contain org3", !orgCollection.Contains(org3));
			Assert("Expect collection not to contain org4", !orgCollection.Contains(org4));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			orgCollection = new OrgHeaderCollection(Factory, FilterStripBizO.Filter);
			orgCollection.Load();

			Assert("Expect collection not to contain org1", !orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
			Assert("Expect collection to contain org3", orgCollection.Contains(org3));
			Assert("Expect collection not to contain org4", !orgCollection.Contains(org4));
		}

		public void TestCRMSecurityFilters()
		{
			Env.Security.OrganisationCRMSecurity.DisableCRMSecurityForTesting(false);
			CRMSecurityProviderTest<OrgHeader>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.OrganisationCRMSecurity);
		}

		public void TestValueAnalysisModuleFilters()
		{
			var filterBizObj = new OrganisationFilterBusinessObject();
			var modules = new[]
			{
				ModuleIDs.ValueAnalysisCustomsBrokerageOrg,
				ModuleIDs.ValueAnalysisForwardingOrg,
				ModuleIDs.ValueAnalysisLinerAgencyOrg,
				ModuleIDs.ValueAnalysisTransportOrg,
				ModuleIDs.ValueAnalysisWarehouseOrg
			};
			foreach (var module in modules)
			{
				var filter = filterBizObj[module.Description];
				AssertNotNull(module.Description, filter);
				AssertEquals("ValueAnalysisModuleFilter", filter.GetType().Name);
				AssertEquals("Value Analysis", (string)filter.Category.Description);
			}
		}

		public void TestSystemDefinedOrganisationIsExcluded()
		{
			var unmatchedOrgPK = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
			var miscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

			var orgUnmatched = Factory.Load<OrgHeader>(unmatchedOrgPK);
			orgUnmatched.OH_FullName = "AAA";
			var orgMisc = Factory.Load<OrgHeader>(miscOrgPK);
			orgMisc.OH_FullName = "BBB";
			var orgABIGAS = Factory.NewWithValidTestData<OrgHeader>();
			orgABIGAS.OH_FullName = "CCC";

			Assert("UNMATCHED is system Defined", orgUnmatched.IsSystemDefinedOrganisation);
			Assert("MISC is system Defined", orgMisc.IsSystemDefinedOrganisation);
			Assert("ABIGAS is NOT system Defined", !orgABIGAS.IsSystemDefinedOrganisation);

			var filter = (ModuleTextFilter)FilterStripBizO["Name"];
			filter.Property = "AAA";
			filter.IsActive = true;
			var collection = new OrgHeaderCollection(Factory);

			collection.Load(filter.Query);
			var nonSystemExcludedCount = collection.Count;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("System Defined Unmatched Organisation should not load with Organization Filter BizO", 1, nonSystemExcludedCount - collection.Count);

			filter.Property = "BBB";
			collection.Load(filter.Query);
			nonSystemExcludedCount = collection.Count;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("System Defined Misc Organisation should not load with Organization Filter BizO", 1, nonSystemExcludedCount - collection.Count);

			filter.Property = "CCC";
			collection.Load(filter.Query);
			nonSystemExcludedCount = collection.Count;
			collection.Load(FilterStripBizO.Filter);

			AssertEquals("Non-System Defined Organisation should load in both queries", 0, nonSystemExcludedCount - collection.Count);
		}

		public void TestAlwaysAddActiveFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XYTEST1";
			org2.OH_Code = "XYTEST2";
			org3.OH_Code = "ABTEST3";
			org4.OH_Code = "XYTEST3";

			org1.OH_IsActive = true;
			org2.OH_IsActive = false;
			org3.OH_IsActive = true;
			org4.OH_IsActive = true;

			var newFilterStripBizO = GetNewFilterStripBusinessObject();
			newFilterStripBizO.SetActiveStatusFilter(OrgHeaderSchema.OH_IsActive, true);

			var filter = (ModuleTextFilter)newFilterStripBizO[OrgConstants.FilterControl.OrgDetails.Code];
			filter.Property = "XYTEST";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			var collection = new OrgHeaderCollection(Factory);
			collection.Load(newFilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { "XYTEST1", "XYTEST3" }, collection.Cast<OrgHeader>().Select(org => org.OH_Code));
		}

		#region Shipping Line Filters

		public void TestShippingLineFilter_Name()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CarrierName = "Dummy Shipping Line";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;
			org.OH_IsShippingLine = true;

			Factory.Save();

			AssertShippingLineTextFilter("Shipping Line Name", "New Shipping Line", false);
			AssertShippingLineTextFilter("Shipping Line Name", "Dummy Shipping Line", true);
		}

		public void TestShippingLineFilter_SCAC()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "WOWO";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;
			org.OH_IsShippingLine = true;

			Factory.Save();

			AssertShippingLineTextFilter("Shipping Line SCAC", "DUMM", false);
			AssertShippingLineTextFilter("Shipping Line SCAC", "WOWO", true);
		}

		public void TestShippingLineFilter_C1C()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "CW1C";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;
			org.OH_IsShippingLine = true;

			Factory.Save();

			AssertShippingLineTextFilter("Shipping Line C1C", "DUMM", false);
			AssertShippingLineTextFilter("Shipping Line C1C", "CW1C", true);
		}

		public void TestShippingLineFilter_SCACOrC1C()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "WOWO";
			shippingLine.RSL_CargoWiseOneCode = "CW1C";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RSL_ShippingLine = shippingLine.PK;
			org.OH_IsShippingLine = true;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertShippingLineTextFilter("Shipping Line SCAC or C1C", "DUMM", false);
				AssertShippingLineTextFilter("Shipping Line SCAC or C1C", "WOWO", true);
				AssertShippingLineTextFilter("Shipping Line SCAC or C1C", "DUMM", false);
				AssertShippingLineTextFilter("Shipping Line SCAC or C1C", "CW1C", true);
			});
		}

		public void TestShippingLineFilter_ShippingLine_IsBlank()
		{
			var usedShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			usedShippingLine.RSL_StandardCarrierAlphaCode = "SCA1";
			usedShippingLine.RSL_CargoWiseOneCode = "C111";

			var unusedShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			unusedShippingLine.RSL_StandardCarrierAlphaCode = "SCA2";
			unusedShippingLine.RSL_CargoWiseOneCode = "C122";

			var orgWithShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			orgWithShippingLine.OH_FullName = "ShippingLineTest";
			orgWithShippingLine.OH_Code = "HasLine";
			orgWithShippingLine.OH_IsShippingProvider = true;
			orgWithShippingLine.OH_RSL_ShippingLine = usedShippingLine.PK;

			var orgWithoutShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			orgWithoutShippingLine.OH_FullName = "ShippingLineTest";
			orgWithoutShippingLine.OH_Code = "NoLine";
			orgWithoutShippingLine.OH_IsShippingProvider = true;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var guidFilter = filter["Shipping Line"] as ModuleGuidFilter;
			guidFilter.IsActive = true;
			guidFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			guidFilter.Property = ZGuid.Empty;

			// This further filter is needed to reduce the results down to just the two orgs
			// in the test. Otherwise 3000+ orgs are loaded
			var query = filter.Filter.AddToFilter(OrgHeaderSchema.OH_FullName, "ShippingLineTest");

			var orgCollection = new OrgHeaderCollection(Factory, query);
			orgCollection.Load();
			Assert(orgCollection.Contains(orgWithoutShippingLine));
			Assert(!orgCollection.Contains(orgWithShippingLine));
		}

		public void TestShippingLineFilter_ShippingLine_IsNonBlank()
		{
			var usedShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			usedShippingLine.RSL_StandardCarrierAlphaCode = "SCA1";
			usedShippingLine.RSL_CargoWiseOneCode = "C111";

			var unusedShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			unusedShippingLine.RSL_StandardCarrierAlphaCode = "SCA2";
			unusedShippingLine.RSL_CargoWiseOneCode = "C122";

			var orgWithShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			orgWithShippingLine.OH_FullName = "Has Shipping Line";
			orgWithShippingLine.OH_Code = "HasLine";
			orgWithShippingLine.OH_IsShippingProvider = true;
			orgWithShippingLine.OH_RSL_ShippingLine = usedShippingLine.PK;

			var orgWithoutShippingLine = Factory.NewWithValidTestData<OrgHeader>();
			orgWithoutShippingLine.OH_FullName = "Has no Shipping Line";
			orgWithoutShippingLine.OH_Code = "NoLine";
			orgWithoutShippingLine.OH_IsShippingProvider = true;

			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var guidFilter = filter["Shipping Line"] as ModuleGuidFilter;
			guidFilter.IsActive = true;
			guidFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			guidFilter.Property = usedShippingLine.PK;

			var orgCollection = new OrgHeaderCollection(Factory, filter.Filter);
			orgCollection.Load();
			Assert(!orgCollection.Contains(orgWithoutShippingLine));
			Assert(orgCollection.Contains(orgWithShippingLine));
		}

		public void TestShippingLineFilter_IntegrationsEnabled()
		{
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_OceanCarrierMessagingAvailable = v, (u, v) => u.Property0 = v, u => u.FlagNames[0] == "Ocean Carrier Messaging");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_BookingRequestAvailable = v, (u, v) => u.Property1 = v, u => u.FlagNames[1] == "Booking Request");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_ShippingInstructionAvailable = v, (u, v) => u.Property2 = v, u => u.FlagNames[2] == "Shipping Instruction");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_VerifiedGrossContainerWeightAvailable = v, (u, v) => u.Property3 = v, u => u.FlagNames[3] == "Verified Gross Container Weight");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_ShippingOrderAvailable = v, (u, v) => u.Property4 = v, u => u.FlagNames[4] == "Shipping Order (China)");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_EManifestAvailable = v, (u, v) => u.Property5 = v, u => u.FlagNames[5] == "eManifest (China)");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_GlobalSailingScheduleAvailable = v, (u, v) => u.Property6 = v, u => u.FlagNames[6] == "Global Sailing Schedule");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_ContainerAutomationAvailable = v, (u, v) => u.Property7 = v, u => u.FlagNames[7] == "Container Automation");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_CargoSphereRatesAvailable = v, (u, v) => u.Property8 = v, u => u.FlagNames[8] == "Cargo Sphere Rates");
			AssertShippingLineIntegrationsEnabledFilterResult((u, v) => u.RSL_InvoiceAvailable = v, (u, v) => u.Property9 = v, u => u.FlagNames[9] == "Invoice");
		}

		#endregion

		#region TestCreditScores

		public void TestCreditScoresFilter_DnBRating_FinancialStrength()
		{
			var validCodes = new FinancialStrengthList();

			var orgs = new OrgHeader[validCodes.Count];
			for (int i = 0; i < validCodes.Count; i++)
			{
				orgs[i] = Factory.NewWithValidTestData<OrgHeader>();
				orgs[i].MiscServ.OM_CCCreditRating = validCodes[i].Code + CreditAppraisalList.Codes.Strong;
				orgs[i].OH_RL_NKClosestPort = "AUSYD";
			}

			Factory.Save();

			var creditReportItemCollection = new CreditReportItemCollection() { CreditReportItemTest.CreateCreditReportItemForTest("AU") };
			creditReportItemCollection[0].CountryEnabledForCompany = true;

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory());
				var filter = (OrgCreditScoresDnBRatingModuleFilter)FilterStripBizO["D&B Rating"];
				filter.IsActive = true;

				for (int i = 0; i < validCodes.Count; i++)
				{
					filter.FinancialStrength = validCodes[i].Code;
					orgCollection.Load(FilterStripBizO.Filter);
					AssertContainsExactElementsInAnyOrder($"Filter for Financial Strength {validCodes[i].Code}", new[] { orgs[i].PK }, orgCollection.Select(org => org.PK));
				}

				filter.FinancialStrength = string.Empty;
				filter.CreditAppraisal = CreditAppraisalList.Codes.Strong;
				orgCollection.Load(FilterStripBizO.Filter);

				var orgPks = orgs.Select(u => u.PK).ToArray();
				AssertContainsExactElementsInAnyOrder("Filter for Financial Strength wildcard (any)", orgPks, orgCollection.Select(org => org.PK));

				filter.CreditAppraisal = string.Empty;
				Assert("Query is empty", filter.Query.IsEmpty);
			}
		}

		public void TestCreditScoresFilter_DnBRating_CreditAppraisal()
		{
			var validCodes = new CreditAppraisalList();

			var orgs = new OrgHeader[validCodes.Count];
			for (int i = 0; i < validCodes.Count; i++)
			{
				orgs[i] = Factory.NewWithValidTestData<OrgHeader>();
				orgs[i].MiscServ.OM_CCCreditRating = FinancialStrengthList.Codes.FiveA + validCodes[i].Code;
				orgs[i].OH_RL_NKClosestPort = "AUSYD";
			}

			Factory.Save();

			var creditReportItemCollection = new CreditReportItemCollection() { CreditReportItemTest.CreateCreditReportItemForTest("AU") };
			creditReportItemCollection[0].CountryEnabledForCompany = true;

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCollection = new OrgHeaderCollection(new BusinessObjectFactory());
				var filter = (OrgCreditScoresDnBRatingModuleFilter)FilterStripBizO["D&B Rating"];
				filter.IsActive = true;

				for (int i = 0; i < validCodes.Count; i++)
				{
					filter.CreditAppraisal = validCodes[i].Code;
					orgCollection.Load(FilterStripBizO.Filter);
					AssertContainsExactElementsInAnyOrder($"Filter for Credit Appraisal {validCodes[i].Code}", new[] { orgs[i].PK }, orgCollection.Select(org => org.PK));
				}

				filter.CreditAppraisal = string.Empty;
				filter.FinancialStrength = FinancialStrengthList.Codes.FiveA;
				orgCollection.Load(FilterStripBizO.Filter);

				var orgPks = orgs.Select(u => u.PK).ToArray();
				AssertContainsExactElementsInAnyOrder("Filter for Credit Appraisal wildcard (any)", orgPks, orgCollection.Select(org => org.PK));
			}
		}

		public void TestCreditScoresFilter_LatePaymentRisk()
		{
			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.OH_RL_NKClosestPort = "AUSYD";
			organisation2.OH_RL_NKClosestPort = "AUSYD";
			organisation3.OH_RL_NKClosestPort = "AUSYD";
			organisation1.MiscServ.OM_CCLatePaymentScore = 200;
			organisation2.MiscServ.OM_CCLatePaymentScore = 500;
			organisation3.MiscServ.OM_CCLatePaymentScore = 700;

			Factory.Save();

			var creditReportItemCollection = new CreditReportItemCollection() { CreditReportItemTest.CreateCreditReportItemForTest("AU") };
			creditReportItemCollection[0].CountryEnabledForCompany = true;

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCollection = new OrgHeaderCollection(Factory);
				var filter = (ModuleNumberRangeFilter)FilterStripBizO["Late Payment Risk"];
				filter.IsActive = true;

				filter.Property1 = 300;
				filter.Property2 = 600;

				AssertEquals(ZCalcEditPropertyType.Int, filter.PropertyType);

				orgCollection.Load(FilterStripBizO.Filter);

				CombineAssertions(() =>
				{
					AssertCollectionNotContains("Should not contains organisation1", organisation1, orgCollection);
					AssertCollectionContains("Should contains organisation2", organisation2, orgCollection);
					AssertCollectionNotContains("Should not contains organisation3", organisation3, orgCollection);
				});
			}
		}

		public void TestCreditScoresFilter_FailureRisk()
		{
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			organisation1.OH_RL_NKClosestPort = "AUSYD";
			organisation2.OH_RL_NKClosestPort = "AUSYD";
			organisation3.OH_RL_NKClosestPort = "AUSYD";
			organisation1.MiscServ.OM_CCFailureRiskScore = 1100;
			organisation2.MiscServ.OM_CCFailureRiskScore = 1500;
			organisation3.MiscServ.OM_CCFailureRiskScore = 1800;

			Factory.Save();

			var creditReportItemCollection = new CreditReportItemCollection() { CreditReportItemTest.CreateCreditReportItemForTest("AU") };

			creditReportItemCollection[0].CountryEnabledForCompany = true;

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCollection = new OrgHeaderCollection(Factory);
				var filter = (ModuleNumberRangeFilter)FilterStripBizO["Failure Risk"];
				filter.IsActive = true;

				filter.Property1 = 1200;
				filter.Property2 = 1600;

				AssertEquals(ZCalcEditPropertyType.Int, filter.PropertyType);

				orgCollection.Load(FilterStripBizO.Filter);

				CombineAssertions(() =>
				{
					AssertCollectionNotContains("Should not contains organisation1", organisation1, orgCollection);
					AssertCollectionContains("Should contains organisation2", organisation2, orgCollection);
					AssertCollectionNotContains("Should not contains organisation3", organisation3, orgCollection);
				});
			}
		}

		public void TestCreditScoresFilter_Visibility()
		{
			AssertCreditScoresFilterVisibility(true, true, true);
			AssertCreditScoresFilterVisibility(true, false, false);
			AssertCreditScoresFilterVisibility(false, false, false);
			AssertCreditScoresFilterVisibility(false, false, false);
		}

		void AssertCreditScoresFilterVisibility(bool enableCreditReports, bool currentCompanyCountryAvailable, bool isVisible)
		{
			var currentCompanyCollection = new CreditReportItemCollection
			{
				new CreditReportItem
				{
					CountryCode = Env.CurrentCompany.Country.Code,
					Country = Env.CurrentCompany.Country.Description,
					CountryEnabledForCompany = currentCompanyCountryAvailable,
					CountryEnabledForOrganisation = false,
					ComprehensiveReportEnabled = false,
					FailureRiskEnabled = false,
					LatePaymentRiskEnabled = false,
					CommercialBureauEnquiryEnabled = false,
				}
			};

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCreditReports))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, currentCompanyCollection))
			{
				var filterBizO = GetNewFilterStripBusinessObject();
				ModuleTextFilter creditRatingFilter = (ModuleTextFilter)filterBizO["D&B Rating"];
				ModuleNumberRangeFilter latePaymentRiskFilter = (ModuleNumberRangeFilter)filterBizO["Late Payment Risk"];
				ModuleNumberRangeFilter failureRiskFilter = (ModuleNumberRangeFilter)filterBizO["Failure Risk"];
				if (isVisible)
				{
					CombineAssertions(() =>
					{
						AssertNotNull(creditRatingFilter);
						AssertNotNull(latePaymentRiskFilter);
						AssertNotNull(failureRiskFilter);
					});
					CombineAssertions(() =>
					{
						AssertEquals(isVisible, creditRatingFilter.Visible);
						AssertEquals(isVisible, latePaymentRiskFilter.Visible);
						AssertEquals(isVisible, failureRiskFilter.Visible);
					});
					AssertNotNull(creditRatingFilter.Category);
					AssertEquals("Credit Scores", creditRatingFilter.Category.Description);
					AssertNotNull(latePaymentRiskFilter.Category);
					AssertEquals("Credit Scores", latePaymentRiskFilter.Category.Description);
					AssertNotNull(failureRiskFilter.Category);
					AssertEquals("Credit Scores", failureRiskFilter.Category.Description);
				}
				else
				{
					CombineAssertions(() =>
					{
						AssertNull(creditRatingFilter);
						AssertNull(latePaymentRiskFilter);
						AssertNull(failureRiskFilter);
					});
				}
			}
		}

		public void TestCreditScoresFilter_OrganisationCountry()
		{
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation4 = Factory.NewWithValidTestData<OrgHeader>();

			organisation1.OH_RL_NKClosestPort = "AUSYD";
			organisation2.OH_RL_NKClosestPort = "USLAX";
			organisation3.OH_RL_NKClosestPort = "NZAKL";
			organisation4.OH_RL_NKClosestPort = "NLRTM";

			organisation1.MiscServ.OM_CCFailureRiskScore = 70;
			organisation2.MiscServ.OM_CCFailureRiskScore = 70;
			organisation3.MiscServ.OM_CCFailureRiskScore = 70;
			organisation4.MiscServ.OM_CCFailureRiskScore = 70;

			Factory.Save();

			var creditReportItemCollection = new CreditReportItemCollection() { CreditReportItemTest.CreateCreditReportItemForTest("AU"), CreditReportItemTest.CreateCreditReportItemForTest("NZ") };

			creditReportItemCollection[0].CountryEnabledForCompany = true;
			creditReportItemCollection[1].CountryEnabledForCompany = true;

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var orgCollection = new OrgHeaderCollection(Factory);
				var filter = (ModuleNumberRangeFilter)FilterStripBizO["Failure Risk"];
				filter.IsActive = true;

				filter.Property1 = 40;
				filter.Property2 = 80;

				orgCollection.Load(FilterStripBizO.Filter);

				CombineAssertions(() =>
				{
					AssertCollectionContains("Should contains organisation1 (AU)", organisation1, orgCollection);
					AssertCollectionContains("Should contains organisation3 (NZ)", organisation3, orgCollection);
					AssertCollectionNotContains("Should not contains organisation2 (US)", organisation2, orgCollection);
					AssertCollectionNotContains("Should not contains organisation4 (NL)", organisation4, orgCollection);
				});
			}
		}

		#endregion

		#region Implementation

		protected virtual FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = GetNewFilterStripBusinessObject();
				}
				return fFilterStripBizO;
			}
		}

		FilterStripBusinessObject fFilterStripBizO;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrganisationFilterBusinessObject();
		}

		void AssertFilterExists(OrgModuleType orgModuleType, string filterDescription, bool expectedExists)
		{
			var filterBizObj = new OrganisationFilterBusinessObject(orgModuleType);
			AssertEquals("Filter:[" + filterDescription + "] exists for OrgModuleType:[" + orgModuleType + "] ?", expectedExists, filterBizObj[filterDescription] != null);
		}

		void AssertShippingLineTextFilter(string filterName, string filterValueForNotFound, bool hasResult)
		{
			var filter = GetNewFilterStripBusinessObject();
			var textFilter = filter[filterName] as ModuleTextFilter;
			AssertNotNull(textFilter);

			textFilter.IsActive = true;
			textFilter.Property = filterValueForNotFound;

			var orgCollection = new OrgHeaderCollection(Factory, filter.Filter);
			orgCollection.Load();
			AssertEquals(hasResult ? 1 : 0, orgCollection.Count);
		}

		void AssertShippingLineIntegrationsEnabledFilterResult(Action<RefShippingLine, bool> setAvailableField, Action<ModuleFlagsFilter, bool> setFlagsFilterProperty, Func<ModuleFlagsFilter, bool> assertFlagName)
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CarrierName = "Dummy Shipping Line";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsShippingLine = true;
			org.OH_RSL_ShippingLine = shippingLine.PK;

			setAvailableField(shippingLine, false);
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var nameFilter = filter["Shipping Line Name"] as ModuleTextFilter;
			var flagsFilter = filter["Shipping Line Integrations Enabled"] as ModuleFlagsFilter;
			AssertNotNull(nameFilter);
			AssertNotNull(flagsFilter);
			AssertEquals(true, assertFlagName(flagsFilter));

			nameFilter.IsActive = true;
			nameFilter.Property = "Dummy Shipping Line";

			flagsFilter.IsActive = true;
			setFlagsFilterProperty(flagsFilter, false);

			var orgCollection = new OrgHeaderCollection(Factory, filter.Filter);
			orgCollection.Load();
			AssertEquals(1, orgCollection.Count);

			setFlagsFilterProperty(flagsFilter, true);
			orgCollection = new OrgHeaderCollection(Factory, filter.Filter);
			orgCollection.Load();
			AssertEquals(0, orgCollection.Count);

			setAvailableField(shippingLine, true);
			Factory.Save();

			orgCollection = new OrgHeaderCollection(Factory, filter.Filter);
			orgCollection.Load();
			AssertEquals(1, orgCollection.Count);

			org.Delete();
			shippingLine.Delete();
		}

		#endregion

		#region Index Search Filter

		public void TestResolveSearchField_WhenEnableIndexSearch()
		{
			using (GetIndexSearchRegistryMock())
			using (GetGlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var orgTypeFilter = filterStrip["Organisation Types"] as IndexSearchOrgTypeModuleFilter;
				AssertNotNull(orgTypeFilter);
				AssertEquals(FilterCategories.StatusAndFlags, orgTypeFilter.Category);
				AssertEquals(OrgModuleType.Standard, orgTypeFilter.ModuleType);
			}

			IDisposable GetIndexSearchRegistryMock()
			{
				var registryMock = new Mock<IGlowRegistry>();
				_ = registryMock.Setup(e => e.IsGlowIndexSearchAllowedForModule(It.IsAny<string>())).Returns(true);
				_ = registryMock.Setup(e => e.MaximumNumberOfModuleFiltersSearchResults).Returns(50);
				return ObjectFactory.Substitute(registryMock.Object);
			}

			IDisposable GetGlowIndexQueryEngineMock()
			{
				var mock = new Mock<IGlowIndexQueryEngine>();
				_ = mock.Setup(e => e.GetSearchFields(It.IsAny<string>())).Returns(new SearchFieldCollection(null, Array.Empty<SearchField>()));
				_ = mock.Setup(e => e.GetGlowEntityTypes()).Returns(new HashSet<string>() { "IDummyBusinessObject", "IOrgHeader" });
				return ObjectFactory.Substitute(mock.Object);
			}

			SearchFieldCollection GetSearchFieldCollection()
			{
				var field = SearchField.Create("ORGANIZATIONTYPE");
				var ret = new SearchFieldCollection(null, new SearchField[] { field });
				return ret;
			}
		}

		public void TestAlwaysAppliedAndHiddenFilterUrls()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var unMatchedOrgCode = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Code;
				var unMatchedOrgPK = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation;
				var miscOrgCode = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Code;
				var miscOrgPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;

				AssertContainsExactElementsInAnyOrder(
					"Should create default AlwaysAppliedAndHidden filter",
					new string[] { $"((CODE ne '{unMatchedOrgCode}') and (CODE ne '{miscOrgCode}'))" },
					filterStrip.GetActiveFiltersQueries().Select(m => m.ToUrlComponent()));

				filterStrip.SearchType = SearchType.Sql;
				filterStrip.LoadModuleFilters();
				AssertEquals(
					"Should create default AlwaysAppliedAndHidden filter",
					$"OH_PK <> '{unMatchedOrgPK}' \r\nAND\r\nOH_PK <> '{miscOrgPK}'\r\n",
					(filterStrip["SystemDefinedOrg"] as ModuleTextFilter).Query.LiteralTextSqlFormatted);
			}
		}

		#endregion
	}
}
