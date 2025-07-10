using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test
{
	[TestedType(typeof(ProcessCompanyLinkRuleFilterBusinessObject))]
	class ProcessCompanyLinkRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestPCR_IsActive()
		{
			var obj1 = Factory.New<ProcessCompanyLinkRule>();
			obj1.PCR_IsActive = false;
			var obj2 = Factory.New<ProcessCompanyLinkRule>();
			obj2.PCR_IsActive = true;
			var strip = (ModuleFlagsFilter)Filter[ProcessCompanyLinkRuleSchema.Constants.PCR_IsActive];
			strip.IsActive = true;
			strip.Property1 = true;
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
			strip.Property0 = true;
			strip.Property1 = false;
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
		}

		public void TestPCR_Type()
		{
			var obj1 = Factory.New<ProcessCompanyLinkRule>();
			obj1.PCR_Type = "DUM";
			var obj2 = Factory.New<ProcessCompanyLinkRule>();
			obj2.PCR_Type = "WKI";
			var strip = (ModuleTextFilter)Filter[ProcessCompanyLinkRuleSchema.Constants.PCR_Type];
			strip.IsActive = true;
			strip.Property = "DUM";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
			strip.Property = "WKI";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
		}

		public void TestPCR_GC_Company()
		{
			var company1 = Factory.New<GlbCompany>();
			var company2 = Factory.New<GlbCompany>();
			var obj1 = Factory.New<ProcessCompanyLinkRule>();
			obj1.PCR_GC_Company = company1.PK;
			var obj2 = Factory.New<ProcessCompanyLinkRule>();
			obj2.PCR_GC_Company = company2.PK;
			var strip = (ModuleGuidFilter)Filter[ProcessCompanyLinkRuleSchema.Constants.PCR_GC_Company];
			strip.IsActive = true;
			strip.Property = company1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
			strip.Property = company2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
		}

		public void TestPCR_SystemCreateTimeUtc()
		{
			var obj1 = Factory.New<ProcessCompanyLinkRule>();
			obj1.PCR_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var obj2 = Factory.New<ProcessCompanyLinkRule>();
			obj2.PCR_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(3);
			var strip = (ModuleDateFilter)Filter[ProcessCompanyLinkRuleSchema.Constants.PCR_SystemCreateTimeUtc];
			strip.IsActive = true;
			strip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			strip.Property1 = ZDateTime.UtcNow.AddDays(-1);
			strip.Property2 = ZDateTime.UtcNow.AddDays(1);
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
			strip.Property1 = ZDateTime.UtcNow.AddDays(2);
			strip.Property2 = ZDateTime.UtcNow.AddDays(4);
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
		}

		public void TestPCR_SystemLastEditTimeUtc()
		{
			var obj1 = Factory.New<ProcessCompanyLinkRule>();
			obj1.PCR_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			var obj2 = Factory.New<ProcessCompanyLinkRule>();
			obj2.PCR_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(3);
			var strip = (ModuleDateFilter)Filter[ProcessCompanyLinkRuleSchema.Constants.PCR_SystemLastEditTimeUtc];
			strip.IsActive = true;
			strip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			strip.Property1 = ZDateTime.UtcNow.AddDays(-1);
			strip.Property2 = ZDateTime.UtcNow.AddDays(1);
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
			strip.Property1 = ZDateTime.UtcNow.AddDays(2);
			strip.Property2 = ZDateTime.UtcNow.AddDays(4);
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<ProcessCompanyLinkRule>(Filter.Filter));
		}

		#region Impl

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ProcessCompanyLinkRuleFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new ProcessCompanyLinkRuleFilterBusinessObject();
		}

		protected override void TearDown()
		{
			Filter = null;
			base.TearDown();
		}

		ProcessCompanyLinkRuleFilterBusinessObject Filter { get; set; }

		#endregion
	}
}
