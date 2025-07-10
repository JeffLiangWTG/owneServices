using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ClientIntelligenceModule))]
	sealed class ClientIntelligenceModuleTest : ZModuleBasherTest
	{
		public void TestBusinessContexts()
		{
			using (ClientIntelligenceModule module = new ClientIntelligenceModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Organisation business context should be returned", BusinessContext.Organisation, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestSupportsWorkflow()
		{
			using (ClientIntelligenceModule module = new ClientIntelligenceModule())
			{
				AssertEquals("SupportsWorkflow should be true", true, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ClientIntelligence;
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			// The test db will be full of OrgHeaders, so an additional identifier is needed to filter by in order to avoid test problems.
			var org = (OrgHeader)factory.NewWithValidTestData(businessObjectType);
			org.OH_FullName = filterStripHelperTestOrgName;

			return org;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);

			var filter = (ModuleTextFilter)filterBusinessObject["Name"];
			filter.IsActive = true;
			filter.Property = filterStripHelperTestOrgName;
		}

		const string filterStripHelperTestOrgName = "MODULE BASHER TEST";
	}
}
