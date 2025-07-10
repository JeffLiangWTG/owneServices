using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test.EDIMessageDeliveryContext
{
	[TestedType(typeof(EDIMessageDeliveryContextFilterBusinessObject))]
	class EDIMessageDeliveryContextFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EDIMessageDeliveryContextFilterBusinessObject();

		public void Test_ProcessTypeFilter()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<EDIMessageDeliveryContextSelector>();
			obj1.ECS_ProcessType = "DUM";
			var obj2 = Factory.New<EDIMessageDeliveryContextSelector>();
			obj2.ECS_ProcessType = "WKI";
			var strip = (ModuleTextFilter)filter[EDIMessageDeliveryContextSelectorSchema.Constants.ECS_ProcessType];
			strip.IsActive = true;
			strip.Property = "DUM";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessageDeliveryContextSelector>(filter.Filter));
			strip.Property = "WKI";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessageDeliveryContextSelector>(filter.Filter));
		}

		public void Test_CodeFilter()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<EDIMessageDeliveryContextSelector>();
			obj1.ECS_Code = "DUM";
			var obj2 = Factory.New<EDIMessageDeliveryContextSelector>();
			obj2.ECS_Code = "WKI";
			var strip = (ModuleTextFilter)filter[EDIMessageDeliveryContextSelectorSchema.Constants.ECS_Code];
			strip.IsActive = true;
			strip.Property = "DUM";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessageDeliveryContextSelector>(filter.Filter));
			strip.Property = "WKI";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessageDeliveryContextSelector>(filter.Filter));
		}

		public void Test_DescriptionFilter()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<EDIMessageDeliveryContextSelector>();
			obj1.ECS_Description = "DUM";
			var obj2 = Factory.New<EDIMessageDeliveryContextSelector>();
			obj2.ECS_Description = "WKI";
			var strip = (ModuleTextFilter)filter[EDIMessageDeliveryContextSelectorSchema.Constants.ECS_Description];
			strip.IsActive = true;
			strip.Property = "DUM";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessageDeliveryContextSelector>(filter.Filter));
			strip.Property = "WKI";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessageDeliveryContextSelector>(filter.Filter));
		}
	}
}
