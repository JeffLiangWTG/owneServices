using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunityDependentCollection))]
	sealed class OrgOpportunityDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			return new OrgOpportunityDependentCollection(header);
		}

		public void TestModuleIDAttribute()
		{
			ModuleIDAttribute[] attributes = (ModuleIDAttribute[])GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), true);
			AssertEquals(ModuleIDs.Opportunity, attributes[0].ModuleIdentifier);
		}

		public void TestValidationChildOpportunity()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var collection = new OrgOpportunityDependentCollection(header);
			var opportunity1 = collection.AddNew();
			var opportunity2 = collection.AddNew();

			header.SalesOpportunities.Load();
			AssertEquals("Child opportunities from this collection must have the validation suspended", 0, header.SalesOpportunities.Count(o => !o.IsValidationSuspended));

			opportunity2.SourceDetails = "TEST";
			AssertContainsExactElementsInAnyOrder("opportunity2 must have the validation restored", new[] { opportunity2 }, header.SalesOpportunities.Find(o => !o.IsValidationSuspended));
		}
	}
}
