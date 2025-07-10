using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class StaffManagerCustomPropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestIsReadOnly()
		{
			var customProperty = new CustomPropertyImplementation<DummyBusinessObject>("xyz", "xyz", typeof(ZString));
			Assert("Should be readonly", new StaffManagerCustomPropertyDescriptor(customProperty).IsReadOnly);
		}

		public void TestGetValue()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			manager1.GS_FullName = "Alex Aardvark";
			manager2.GS_FullName = "Buffalo Bill";

			var manager1DRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 01, 01));
			var manager2DRMRecord = StaffManagerTestHelper.AddManager(staff, manager2, "HRM", new ZDateTime(2018, 01, 01));
			Factory.Save();

			var customProperty1 = new CustomPropertyImplementation<GlbStaff>("HRM", "HRM", typeof(ZString));
			var customProperty2 = new CustomPropertyImplementation<GlbStaff>("PRM", "PRM", typeof(ZString));

			PropertyDescriptor descriptor1 = new StaffManagerCustomPropertyDescriptor(customProperty1);
			PropertyDescriptor descriptor2 = new StaffManagerCustomPropertyDescriptor(customProperty2);

			AssertEquals("Alex Aardvark, Buffalo Bill", descriptor1.GetValue(staff));
			AssertEquals(ZString.Empty, descriptor1.GetValue(manager1));
			AssertEquals(ZString.Empty, descriptor2.GetValue(staff));
			AssertEquals(ZString.Empty, descriptor2.GetValue(manager2));
		}

		public void TestSetValue()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			var customProperty = new CustomPropertyImplementation<DummyBusinessObject>("xyz", "xyz", typeof(ZString));
			PropertyDescriptor descriptor = new StaffManagerCustomPropertyDescriptor(customProperty);

			AssertNull(descriptor.GetValue(dummy));
			AssertExceptionThrown(typeof(NotSupportedException), "StaffManagerCustomPropertyDescriptor for property 'xyz' is read only", () => descriptor.SetValue(dummy, "BBB"));
		}
	}
}
