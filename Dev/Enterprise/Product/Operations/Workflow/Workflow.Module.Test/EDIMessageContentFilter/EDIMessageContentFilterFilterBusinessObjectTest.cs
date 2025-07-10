using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test
{
	[TestedType(typeof(EDIMessageContentFilterFilterBusinessObject))]
	class EDIMessageContentFilterFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestECF_Name()
		{
			var o1 = Factory.New<EDIMessageContentFilter>();
			var o2 = Factory.New<EDIMessageContentFilter>();
			o1.ECF_Name = "Joyce";
			o2.ECF_Name = "Gunther";

			var strip = (ModuleTextFilter)Filter[ZArchitecture.Schema.EDIMessageContentFilterSchema.Constants.ECF_Name];
			strip.IsActive = true;
			strip.Property = "Joyce";

			AssertContainsExactElementsInAnyOrder(new[] { o1 }, Factory.Load<EDIMessageContentFilter>(Filter.Filter));
		}

		public void TestECF_SystemCreateUser()
		{
			var s1 = Factory.NewWithValidTestData<GlbStaff>();
			var s2 = Factory.NewWithValidTestData<GlbStaff>();
			var obj1 = Factory.New<EDIMessageContentFilter>();
			obj1.ECF_SystemCreateUser = s1.GS_Code;
			var obj2 = Factory.New<EDIMessageContentFilter>();
			obj2.ECF_SystemCreateUser = s2.GS_Code;
			var strip = (ModuleNkFilter)Filter[ZArchitecture.Schema.EDIMessageContentFilterSchema.Constants.ECF_SystemCreateUser];
			strip.IsActive = true;
			strip.Property = s1.GS_Code;
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessageContentFilter>(Filter.Filter));
			strip.Property = s2.GS_Code;
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessageContentFilter>(Filter.Filter));
		}

		public void TestECF_SystemLastEditUser()
		{
			var s1 = Factory.NewWithValidTestData<GlbStaff>();
			var s2 = Factory.NewWithValidTestData<GlbStaff>();
			var obj1 = Factory.New<EDIMessageContentFilter>();
			obj1.ECF_SystemLastEditUser = s1.GS_Code;
			var obj2 = Factory.New<EDIMessageContentFilter>();
			obj2.ECF_SystemLastEditUser = s2.GS_Code;
			var strip = (ModuleNkFilter)Filter[ZArchitecture.Schema.EDIMessageContentFilterSchema.Constants.ECF_SystemLastEditUser];
			strip.IsActive = true;
			strip.Property = s1.GS_Code;
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessageContentFilter>(Filter.Filter));
			strip.Property = s2.GS_Code;
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessageContentFilter>(Filter.Filter));
		}

		public void TestECF_SystemLastEditTime()
		{
			var o1 = Factory.New<EDIMessageContentFilter>();
			var o2 = Factory.New<EDIMessageContentFilter>();
			o1.ECF_SystemLastEditTimeUtc = ZDateTime.Now;
			o2.ECF_SystemLastEditTimeUtc = ZDateTime.Now.AddDays(12);

			var strip = (ModuleDateFilter)Filter[ZArchitecture.Schema.EDIMessageContentFilterSchema.Constants.ECF_SystemLastEditTimeUtc];
			strip.IsActive = true;
			strip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			strip.Property1 = ZDateTime.UtcNow.AddDays(-1);
			strip.Property2 = ZDateTime.UtcNow.AddDays(1);

			AssertContainsExactElementsInAnyOrder(new[] { o1 }, Factory.Load<EDIMessageContentFilter>(Filter.Filter));
		}

		public void TestECF_SystemCreateTime()
		{
			var o1 = Factory.New<EDIMessageContentFilter>();
			var o2 = Factory.New<EDIMessageContentFilter>();
			o1.ECF_SystemCreateTimeUtc = ZDateTime.Now;
			o2.ECF_SystemCreateTimeUtc = ZDateTime.Now.AddDays(12);

			var strip = (ModuleDateFilter)Filter[ZArchitecture.Schema.EDIMessageContentFilterSchema.Constants.ECF_SystemCreateTimeUtc];
			strip.IsActive = true;
			strip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			strip.Property1 = ZDateTime.UtcNow.AddDays(-1);
			strip.Property2 = ZDateTime.UtcNow.AddDays(1);

			AssertContainsExactElementsInAnyOrder(new[] { o1 }, Factory.Load<EDIMessageContentFilter>(Filter.Filter));
		}

		#region Impl

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EDIMessageContentFilterFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new EDIMessageContentFilterFilterBusinessObject();
		}

		protected override void TearDown()
		{
			Filter = null;
			base.TearDown();
		}

		EDIMessageContentFilterFilterBusinessObject Filter { get; set; }

		#endregion
	}
}
