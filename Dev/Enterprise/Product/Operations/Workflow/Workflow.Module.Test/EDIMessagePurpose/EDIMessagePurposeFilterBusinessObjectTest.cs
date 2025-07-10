using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Module.Test
{
	[TestedType(typeof(EDIMessagePurposeFilterBusinessObject))]
	class EDIMessagePurposeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEMP_Code()
		{
			var obj1 = Factory.New<EDIMessagePurpose>();
			obj1.EMP_Code = "GUH";
			var obj2 = Factory.New<EDIMessagePurpose>();
			obj2.EMP_Code = "BIP";
			var strip = (ModuleTextFilter)Filter[EDIMessagePurposeSchema.Constants.EMP_Code];
			strip.IsActive = true;
			strip.Property = "GUH";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
			strip.Property = "BIP";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
		}

		public void TestEMP_Description()
		{
			var obj1 = Factory.New<EDIMessagePurpose>();
			obj1.EMP_Description = "GUH";
			var obj2 = Factory.New<EDIMessagePurpose>();
			obj2.EMP_Description = "BIP";
			var strip = (ModuleTextFilter)Filter[EDIMessagePurposeSchema.Constants.EMP_Description];
			strip.IsActive = true;
			strip.Property = "GUH";
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
			strip.Property = "BIP";
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
		}

		public void TestEMP_SystemCreateUser()
		{
			var s1 = Factory.NewWithValidTestData<GlbStaff>();
			var s2 = Factory.NewWithValidTestData<GlbStaff>();
			var obj1 = Factory.New<EDIMessagePurpose>();
			obj1.EMP_SystemCreateUser = s1.GS_Code;
			var obj2 = Factory.New<EDIMessagePurpose>();
			obj2.EMP_SystemCreateUser = s2.GS_Code;
			var strip = (ModuleNkFilter)Filter[EDIMessagePurposeSchema.Constants.EMP_SystemCreateUser];
			strip.IsActive = true;
			strip.Property = s1.GS_Code;
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
			strip.Property = s2.GS_Code;
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
		}

		public void TestEMP_SystemLastEditUser()
		{
			var s1 = Factory.NewWithValidTestData<GlbStaff>();
			var s2 = Factory.NewWithValidTestData<GlbStaff>();
			var obj1 = Factory.New<EDIMessagePurpose>();
			obj1.EMP_SystemLastEditUser = s1.GS_Code;
			var obj2 = Factory.New<EDIMessagePurpose>();
			obj2.EMP_SystemLastEditUser = s2.GS_Code;
			var strip = (ModuleNkFilter)Filter[EDIMessagePurposeSchema.Constants.EMP_SystemLastEditUser];
			strip.IsActive = true;
			strip.Property = s1.GS_Code;
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
			strip.Property = s2.GS_Code;
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
		}

		public void TestEMP_SystemCreateTimeUtc()
		{
			foreach (var p in Factory.Load<EDIMessagePurpose>(new ZQuery()))
			{
				p.Delete();
			}

			var obj1 = Factory.New<EDIMessagePurpose>();
			obj1.EMP_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var obj2 = Factory.New<EDIMessagePurpose>();
			obj2.EMP_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(3);
			var strip = (ModuleDateFilter)Filter[EDIMessagePurposeSchema.Constants.EMP_SystemCreateTimeUtc];
			strip.IsActive = true;
			strip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			strip.Property1 = ZDateTime.UtcNow.AddDays(-1);
			strip.Property2 = ZDateTime.UtcNow.AddDays(1);
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
			strip.Property1 = ZDateTime.UtcNow.AddDays(2);
			strip.Property2 = ZDateTime.UtcNow.AddDays(4);
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
		}

		public void TestEMP_SystemLastEditTimeUtc()
		{
			foreach (var p in Factory.Load<EDIMessagePurpose>(new ZQuery()))
			{
				p.Delete();
			}

			var obj1 = Factory.New<EDIMessagePurpose>();
			obj1.EMP_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			var obj2 = Factory.New<EDIMessagePurpose>();
			obj2.EMP_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(3);
			var strip = (ModuleDateFilter)Filter[EDIMessagePurposeSchema.Constants.EMP_SystemLastEditTimeUtc];
			strip.IsActive = true;
			strip.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			strip.Property1 = ZDateTime.UtcNow.AddDays(-1);
			strip.Property2 = ZDateTime.UtcNow.AddDays(1);
			AssertContainsExactElementsInAnyOrder(new[] { obj1 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
			strip.Property1 = ZDateTime.UtcNow.AddDays(2);
			strip.Property2 = ZDateTime.UtcNow.AddDays(4);
			AssertContainsExactElementsInAnyOrder(new[] { obj2 }, Factory.Load<EDIMessagePurpose>(Filter.Filter));
		}

		#region Impl

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EDIMessagePurposeFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new EDIMessagePurposeFilterBusinessObject();
		}

		protected override void TearDown()
		{
			Filter = null;
			base.TearDown();
		}

		EDIMessagePurposeFilterBusinessObject Filter { get; set; }

		#endregion
	}
}
