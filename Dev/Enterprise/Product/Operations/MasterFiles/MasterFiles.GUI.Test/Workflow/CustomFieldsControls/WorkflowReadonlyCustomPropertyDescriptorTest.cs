using System;
using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowReadonlyCustomPropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestIsReadOnly()
		{
			Assert("Should be readonly", new WorkflowReadonlyCustomPropertyDescriptor(new CustomPropertyImplementation<DummyBusinessObject>("Abc", typeof(ZString)), "Abc").IsReadOnly);
		}

		public void TestGetValue()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();

			GenCustomAddOnValue value = Factory.New<GenCustomAddOnValue>();
			value.XV_Name = "xyz";
			value.XV_Data = "AAA";
			value.XV_Type = "STR";
			value.XV_ParentID = dummy1.PK;
			value.XV_ParentTableCode = dummy1.TablePrefix;

			Factory.Save();

			PropertyDescriptor descriptor1 = new WorkflowReadonlyCustomPropertyDescriptor(new CustomPropertyImplementation<DummyBusinessObject>("xyz", typeof(ZString)), "xyz");
			PropertyDescriptor descriptor2 = new WorkflowReadonlyCustomPropertyDescriptor(new CustomPropertyImplementation<DummyBusinessObject>("abc", typeof(ZString)), "abc");

			AssertEquals("AAA", descriptor1.GetValue(dummy1));
			AssertNull(descriptor1.GetValue(dummy2));
			AssertNull(descriptor2.GetValue(dummy1));
			AssertNull(descriptor2.GetValue(dummy2));
		}

		public void TestGetValue_UsesParentTableCode()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();

			var value = Factory.New<GenCustomAddOnValue>();
			value.XV_Name = "xyz";
			value.XV_Type = "STR";
			value.XV_ParentID = dummy1.PK;
			value.XV_ParentTableCode = "A1";
			value.XV_Data = "AAA";

			var value2 = value.Clone() as GenCustomAddOnValue;
			value2.XV_ParentTableCode = dummy1.TablePrefix;
			value2.XV_Data = "BBB";

			AssertNotEquals("pre: XV_ParentTableCode is different", value.XV_ParentTableCode, value2.XV_ParentTableCode);
			var descriptor1 = new WorkflowReadonlyCustomPropertyDescriptor(new CustomPropertyImplementation<DummyBusinessObject>("xyz", typeof(ZString)), "xyz");
			AssertEquals("BBB", descriptor1.GetValue(dummy1));
		}

		public void TestGetDecimalValueFromIntProperty()
		{
			AssertNullValueForPropertyDescriptorOfType("100.50", typeof(ZInt));
		}

		public void TestGetBoolValueFromDateTimeProperty()
		{
			AssertNullValueForPropertyDescriptorOfType("True", typeof(ZDateTime));
		}

		public void TestGetIntValueFromBoolProperty()
		{
			AssertNullValueForPropertyDescriptorOfType("123", typeof(ZBool));
		}

		void AssertNullValueForPropertyDescriptorOfType(string sourceData, Type descriptorType)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var value = Factory.New<GenCustomAddOnValue>();
			value.XV_Name = "Bananas";
			value.XV_Data = sourceData;
			value.XV_ParentID = dummy.PK;

			Factory.Save();

			var descriptor = new WorkflowReadonlyCustomPropertyDescriptor(new CustomPropertyImplementation<DummyBusinessObject>("Bananas", descriptorType), "Bananas");
			AssertEquals(string.Format("Should get null since a {0} cannot be parsed from {1}", descriptorType.Name, sourceData),
				null, descriptor.GetValue(dummy));
		}

		public void TestSetValue()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			PropertyDescriptor descriptor = new WorkflowReadonlyCustomPropertyDescriptor(new CustomPropertyImplementation<DummyBusinessObject>("xyz", typeof(ZString)), "xyz");

			AssertNull(descriptor.GetValue(dummy));
			AssertExceptionThrown(typeof(NotSupportedException), "CustomFieldReadonlyPropertyDescriptor for property 'xyz' is read only", () => descriptor.SetValue(dummy, "BBB"));
		}
	}
}
