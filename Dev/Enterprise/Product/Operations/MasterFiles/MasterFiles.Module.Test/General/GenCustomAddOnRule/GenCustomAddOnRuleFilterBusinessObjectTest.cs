using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GenCustomAddOnRuleFilterBusinessObject))]
	sealed class GenCustomAddOnRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GenCustomAddOnRuleFilterBusinessObject();
		}

		#endregion

		public void TestCodeFilter()
		{
			var rule1 = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			var rule2 = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			rule1.XR_Code = "ABC";
			rule2.XR_Code = "ABD";

			var filter = new GenCustomAddOnRuleFilterBusinessObject();
			var strip = ((ModuleTextFilter)filter[GenCustomAddOnRuleSchema.Constants.XR_Code]);
			strip.Property = "ABC";
			strip.IsActive = true;

			var filterResults = Factory.Load<GenCustomAddOnRule>(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { rule1 }, filterResults);
		}

		public void TestDescriptionFilter()
		{
			var rule1 = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			var rule2 = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			rule1.XR_Description = "ABC";
			rule2.XR_Description = "ABD";

			var filter = new GenCustomAddOnRuleFilterBusinessObject();
			var strip = ((ModuleTextFilter)filter[GenCustomAddOnRuleSchema.Constants.XR_Description]);
			strip.Property = "ABC";
			strip.IsActive = true;

			var filterResults = Factory.Load<GenCustomAddOnRule>(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { rule1 }, filterResults);
		}
	}
}
