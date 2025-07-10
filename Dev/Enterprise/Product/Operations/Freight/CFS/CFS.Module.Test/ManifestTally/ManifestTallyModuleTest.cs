using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(ManifestTallyModule))]
	sealed class ManifestTallyModuleTest : ZModuleBasherTest
	{
		public void TestBusinessContexts()
		{
			using (ManifestTallyModule module = new ManifestTallyModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("CFSTallySheet business context should be returned", BusinessContext.CFSTallySheet, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ManifestTally;
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var container = factory.NewWithValidTestData<TallyContainer>();
			var consol = factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_IsCFS = ZBool.True;
			container.JC_JK = consol.PK;

			return container;
		}
	}
}
