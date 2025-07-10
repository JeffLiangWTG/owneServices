using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgTypeModuleFilter))]
	sealed class OrgTypeModuleFilterTest : ModuleFilterTestCase<OrgTypeModuleFilter>
	{
		#region OrgTypeModuleFilterForTest

		OrgTypeModuleFilterForTest TestOrgTypeModuleFilter
		{
			get
			{
				if (testOrgTypeModuleFilter == null)
				{
					testOrgTypeModuleFilter = new OrgTypeModuleFilterForTest("Organisation Type");
				}

				return testOrgTypeModuleFilter;
			}
		}

		OrgTypeModuleFilterForTest testOrgTypeModuleFilter;

		#endregion

		#region ShouldCheckMaximumFlags

		public void TestShouldCheckMaximumFlags()
		{
			Assert("ShouldCheckMaximumFlags should be false", !TestOrgTypeModuleFilter.ShouldCheckMaximumFlags_Exposed);
		}

		#endregion

		#region ShouldCheckFlagAmountEqualsDelegateAmount

		public void TestShouldCheckFlagAmountEqualsDelegateAmount()
		{
			Assert("ShouldCheckFlagAmountEqualsDelegateAmount should be false", !TestOrgTypeModuleFilter.ShouldCheckFlagAmountEqualsDelegateAmount_Exposed);
		}

		#endregion

		#region Organisation Type Filter

		[StressTest]
		public void TestOrganisationTypes()
		{
			using (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				var org3 = Factory.NewWithValidTestData<OrgHeader>();
				var org4 = Factory.NewWithValidTestData<OrgHeader>();
				var org5 = Factory.NewWithValidTestData<OrgHeader>();
				var org6 = Factory.NewWithValidTestData<OrgHeader>();
				var org7 = Factory.NewWithValidTestData<OrgHeader>();
				var org8 = Factory.NewWithValidTestData<OrgHeader>();
				var org9 = Factory.NewWithValidTestData<OrgHeader>();
				var org10 = Factory.NewWithValidTestData<OrgHeader>();
				var org11 = Factory.NewWithValidTestData<OrgHeader>();
				var org12 = Factory.NewWithValidTestData<OrgHeader>();
				var org13 = Factory.NewWithValidTestData<OrgHeader>();
				var org14 = Factory.NewWithValidTestData<OrgHeader>();

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
				org13.OH_IsControllingAgent = true;
				org14.OH_IsControllingCustomer = true;

				org1.OH_IsConsignee = true;
				org2.OH_IsConsignor = true;

				Factory.Save();

				OrgTypeModuleFilterForTest filter = TestOrgTypeModuleFilter;
				filter.Property0 = true; // IsDebtor
				filter.AndJoinCondition = true;
				filter.IsActive = true;

				var orgCollection = new OrgHeaderCollection(Factory, filter.GetQuery_Exposed());
				orgCollection.Load();

				Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
				Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

				filter.Property0 = false;
				filter.Property1 = true; //IsCreditor

				orgCollection.Load(filter.GetQuery_Exposed());
				Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
				Assert("Expect collection to contain org2.", orgCollection.Contains(org2));
				Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));

				filter.Property0 = false;
				filter.Property1 = false;
				filter.Property2 = true; //IsConsignee

				orgCollection.Load(filter.GetQuery_Exposed());
				Assert("Expect collection to contain org1.", orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
				Assert("Expect collection to contain org3.", orgCollection.Contains(org3));

				filter.Property0 = false;
				filter.Property1 = false;
				filter.Property2 = false;
				filter.Property3 = true; //IsConsignor

				orgCollection.Load(filter.GetQuery_Exposed());
				Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
				Assert("Expect collection to contain org2.", orgCollection.Contains(org2));
				Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
				Assert("Expect collection to contain org4.", orgCollection.Contains(org4));

				filter.Property0 = false;
				filter.Property1 = false;
				filter.Property2 = false;
				filter.Property3 = false;
				filter.Property4 = true; //OH_IsShippingProvider

				orgCollection.Load(filter.GetQuery_Exposed());
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

				orgCollection.Load(filter.GetQuery_Exposed());
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

				orgCollection.Load(filter.GetQuery_Exposed());
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

				orgCollection.Load(filter.GetQuery_Exposed());
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

				orgCollection.Load(filter.GetQuery_Exposed());
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

				orgCollection.Load(filter.GetQuery_Exposed());
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

				orgCollection.Load(filter.GetQuery_Exposed());
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

				orgCollection.Load(filter.GetQuery_Exposed());
				Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
				Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
				Assert("Expect collection not to contain org11.", !orgCollection.Contains(org11));
				Assert("Expect collection to contain org12.", orgCollection.Contains(org12));

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
				filter.Property11 = false;
				filter.Property12 = true; //OH_IsControllingAgent

				orgCollection.Load(filter.GetQuery_Exposed());
				Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
				Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
				Assert("Expect collection not to contain org12.", !orgCollection.Contains(org12));
				Assert("Expect collection to contain org13.", orgCollection.Contains(org13));

				using (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					orgCollection.Load(filter.GetQuery_Exposed());
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingAgent is ignored as EnableControllingAgentFunctionalityAndValidations is OFF)", orgCollection.Contains(org1));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingAgent is ignored as EnableControllingAgentFunctionalityAndValidations is OFF)", orgCollection.Contains(org2));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingAgent is ignored as EnableControllingAgentFunctionalityAndValidations is OFF)", orgCollection.Contains(org3));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingAgent is ignored as EnableControllingAgentFunctionalityAndValidations is OFF)", orgCollection.Contains(org12));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingAgent is ignored as EnableControllingAgentFunctionalityAndValidations is OFF)", orgCollection.Contains(org13));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingAgent is ignored as EnableControllingAgentFunctionalityAndValidations is OFF)", orgCollection.Contains(org13));
				}

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
				filter.Property11 = false;
				filter.Property12 = false;
				filter.Property13 = true; //OH_IsControllingCustomer

				orgCollection.Load(filter.GetQuery_Exposed());
				Assert("Expect collection not to contain org1.", !orgCollection.Contains(org1));
				Assert("Expect collection not to contain org2.", !orgCollection.Contains(org2));
				Assert("Expect collection not to contain org3.", !orgCollection.Contains(org3));
				Assert("Expect collection not to contain org13.", !orgCollection.Contains(org13));
				Assert("Expect collection to contain org14.", orgCollection.Contains(org14));

				using (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					orgCollection.Load(filter.GetQuery_Exposed());
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingCustomer is ignored as EnableControllingCustomerFunctionalityAndValidations is OFF)", orgCollection.Contains(org1));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingCustomer is ignored as EnableControllingCustomerFunctionalityAndValidations is OFF)", orgCollection.Contains(org2));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingCustomer is ignored as EnableControllingCustomerFunctionalityAndValidations is OFF)", orgCollection.Contains(org3));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingCustomer is ignored as EnableControllingCustomerFunctionalityAndValidations is OFF)", orgCollection.Contains(org13));
					Assert("No filter selected, all orgs should be loaded (OH_IsControllingCustomer is ignored as EnableControllingCustomerFunctionalityAndValidations is OFF)", orgCollection.Contains(org14));
				}

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
				filter.Property12 = true;
				filter.Property13 = true;
				filter.AndJoinCondition = true;

				orgCollection.Load(filter.GetQuery_Exposed());

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
				Assert("Expect collection not to contain org13.", !orgCollection.Contains(org13));
				Assert("Expect collection not to contain org14.", !orgCollection.Contains(org14));

				filter.AndJoinCondition = false;
				filter.OrJoinCondition = true;

				orgCollection.Load(filter.GetQuery_Exposed());

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
				Assert("Expect collection to contain org13.", orgCollection.Contains(org13));
				Assert("Expect collection to contain org14.", orgCollection.Contains(org14));
			}
		}

		#endregion

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.StatusAndFlags; }
		}

		protected override OrgTypeModuleFilter GetNewModuleFilter()
		{
			return new OrgTypeModuleFilter("moo");
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, TestOrgTypeModuleFilter.IsExpensiveQuery);
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(OrgTypeModuleFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "Item" }).ToArray(); // This will cause the test to skip the indexer, which is problematic and sets values for properties that are tested independently anyway.
		}

		class OrgTypeModuleFilterForTest : OrgTypeModuleFilter
		{
			public OrgTypeModuleFilterForTest(ZString description)
				: base(description)
			{
			}

			public ZQuery GetQuery_Exposed()
			{
				return base.GetQuery();
			}

			public bool ShouldCheckMaximumFlags_Exposed
			{
				get { return base.ShouldCheckMaximumFlags; }
			}

			public bool ShouldCheckFlagAmountEqualsDelegateAmount_Exposed
			{
				get { return base.ShouldCheckFlagAmountEqualsDelegateAmount; }
			}
		}
	}
}
