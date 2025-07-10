using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunityCollection))]
	sealed class OrgOpportunityCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgOpportunityCollection(Factory);
		}

		public void TestModuleIDAttribute()
		{
			ModuleIDAttribute[] attributes = (ModuleIDAttribute[])GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true);
			AssertEquals(ModuleIDs.Opportunity, attributes[0].ModuleIdentifier);
		}
	}
}
