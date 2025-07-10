using CargoWise.Types;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test
{
	[TestedType(typeof(ProcessFieldChangeRuleFilterBusinessObject))]
	class ProcessFieldChangeRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestPFR_ProcessType()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<ProcessFieldChangeRule>();
			obj1.PFR_ProcessType = "DUM";
			var obj2 = Factory.New<ProcessFieldChangeRule>();
			obj2.PFR_ProcessType = "WKI";
			var strip = (ModuleTextFilter)filter[ProcessFieldChangeRuleSchema.Constants.PFR_ProcessType];
			strip.IsActive = true;
			strip.Property = "DUM";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
			strip.Property = "WKI";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
		}

		public void TestPFR_GroupName()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<ProcessFieldChangeRule>();
			obj1.PFR_GroupName = "DUM";
			var obj2 = Factory.New<ProcessFieldChangeRule>();
			obj2.PFR_GroupName = "WKI";
			var strip = (ModuleTextFilter)filter[ProcessFieldChangeRuleSchema.Constants.PFR_GroupName];
			strip.IsActive = true;
			strip.Property = "DUM";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
			strip.Property = "WKI";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
		}

		public void TestPFR_Description()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<ProcessFieldChangeRule>();
			obj1.PFR_Description = "DUM";
			var obj2 = Factory.New<ProcessFieldChangeRule>();
			obj2.PFR_Description = "WKI";
			var strip = (ModuleTextFilter)filter[ProcessFieldChangeRuleSchema.Constants.PFR_Description];
			strip.IsActive = true;
			strip.Property = "DUM";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
			strip.Property = "WKI";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
		}

		public void TestPFR_SE_NKEvent()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<ProcessFieldChangeRule>();
			obj1.PFR_SE_NKEvent = "Z00";
			var obj2 = Factory.New<ProcessFieldChangeRule>();
			obj2.PFR_SE_NKEvent = "Z01";
			var strip = (ModuleTextFilter)filter[ProcessFieldChangeRuleSchema.Constants.PFR_SE_NKEvent];
			strip.IsActive = true;
			strip.Property = "Z00";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
			strip.Property = "Z01";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
		}

		public void TestPFR_Reference()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<ProcessFieldChangeRule>();
			obj1.PFR_Reference = "ABC";
			var obj2 = Factory.New<ProcessFieldChangeRule>();
			obj2.PFR_Reference = "DEF";
			var strip = (ModuleTextFilter)filter[ProcessFieldChangeRuleSchema.Constants.PFR_Reference];
			strip.IsActive = true;
			strip.Property = "ABC";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
			strip.Property = "DEF";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
		}

		public void TestFieldsFilter()
		{
			var filter = GetNewFilterStripBusinessObject();

			var obj1 = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			obj1.PFR_GroupName = "1";
			var field1 = obj1.Fields.AddNew();
			var field2 = obj1.Fields.AddNew();
			field1.PFL_FieldName = "Z0_Field1";
			field2.PFL_FieldName = "Z0_Field2";

			var obj2 = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			obj2.PFR_GroupName = "2";
			var field3 = obj2.Fields.AddNew();
			var field4 = obj2.Fields.AddNew();
			field3.PFL_FieldName = "Z0_Field3";
			field4.PFL_FieldName = "Z0_Field1";

			var obj3 = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			obj3.PFR_GroupName = "3";
			var field5 = obj3.Fields.AddNew();
			var field6 = obj3.Fields.AddNew();
			field5.PFL_FieldName = "Z0_Field5";
			field6.PFL_FieldName = "Z0_Field6";

			Factory.Save();

			var strip = (ModuleTextFilter)filter["FieldName"];
			strip.IsActive = true;
			strip.Property = "Z0_Field1";
			var rule = Factory.Load<ProcessFieldChangeRule>(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { obj1, obj2 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
			strip.Property = "Z0_Field6";
			AssertContainsExactElementsInAnyOrder(new[] { obj3 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
		}

		public void TestPFR_SystemCreateTimeUtc()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<ProcessFieldChangeRule>();
			obj1.PFR_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var obj2 = Factory.New<ProcessFieldChangeRule>();
			obj2.PFR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(3);
			var strip = (ModuleDateFilter)filter[ProcessFieldChangeRuleSchema.Constants.PFR_SystemCreateTimeUtc];
			strip.IsActive = true;
			strip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			strip.Property1 = ZDateTime.UtcNow.AddDays(-1);
			strip.Property2 = ZDateTime.UtcNow.AddDays(1);
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
			strip.Property1 = ZDateTime.UtcNow.AddDays(2);
			strip.Property2 = ZDateTime.UtcNow.AddDays(4);
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
		}

		public void TestPFR_SystemLastEditTimeUtc()
		{
			var filter = GetNewFilterStripBusinessObject();
			var obj1 = Factory.New<ProcessFieldChangeRule>();
			obj1.PFR_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			var obj2 = Factory.New<ProcessFieldChangeRule>();
			obj2.PFR_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(3);
			var strip = (ModuleDateFilter)filter[ProcessFieldChangeRuleSchema.Constants.PFR_SystemLastEditTimeUtc];
			strip.IsActive = true;
			strip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			strip.Property1 = ZDateTime.UtcNow.AddDays(-1);
			strip.Property2 = ZDateTime.UtcNow.AddDays(1);
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
			strip.Property1 = ZDateTime.UtcNow.AddDays(2);
			strip.Property2 = ZDateTime.UtcNow.AddDays(4);
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessFieldChangeRule>(filter.Filter));
		}

		#region Impl

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ProcessFieldChangeRuleFilterBusinessObject();

		#endregion
	}
}
