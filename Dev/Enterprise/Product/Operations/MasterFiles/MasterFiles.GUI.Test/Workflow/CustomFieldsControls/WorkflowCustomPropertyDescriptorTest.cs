using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowCustomPropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestGetCustomPropertyContainer()
		{
			WorkflowCustomPropertyDescriptorForTest descriptor = new WorkflowCustomPropertyDescriptorForTest("Abc", typeof(ZString));

			AssertNull(descriptor.GetCustomPropertyContainerExposed(null));
			AssertNull(descriptor.GetCustomPropertyContainerExposed(Factory.New<DummyBusinessObject>()));

			WorkflowCustomFieldsGridInitializerTest.DummyWithCustomBizo dummy = Factory.New<WorkflowCustomFieldsGridInitializerTest.DummyWithCustomBizo>();
			AssertNull(descriptor.GetCustomPropertyContainerExposed(dummy));

			CustomBusinessObjectExtensions.GetCustomBusinessObject(dummy, true); // Initialize custom business object
			AssertNotNull(descriptor.GetCustomPropertyContainerExposed(dummy));

			CustomPropertyContainer customPropertyContainer = new CustomPropertyContainer();
			AssertSame("Should fallback to base", customPropertyContainer, descriptor.GetCustomPropertyContainerExposed(customPropertyContainer));

			WorkflowCustomPropertyDescriptorForTest descriptor1 = new WorkflowCustomPropertyDescriptorForTest("Abc", typeof(ZString), true);
			WorkflowCustomFieldsGridInitializerTest.DummyWithCustomBizo dummy1 = Factory.New<WorkflowCustomFieldsGridInitializerTest.DummyWithCustomBizo>();
			AssertNotNull("Should automaticall initialize custom bizo and get custom properties", descriptor1.GetCustomPropertyContainerExposed(dummy1));
		}

		public void TestGetZPropertyInfo()
		{
			WorkflowCustomFieldsGridInitializerTest.DummyWithCustomBizo dummy = Factory.New<WorkflowCustomFieldsGridInitializerTest.DummyWithCustomBizo>();
			dummy.SubType1 = "T1";

			WorkflowCustomPropertyDescriptorForTest descriptor = new WorkflowCustomPropertyDescriptorForTest("__C11__prop__ZString", typeof(ZString));

			AssertNull(descriptor.GetZPropertyInfo(null));
			AssertNull(descriptor.GetZPropertyInfo(Factory.New<DummyBusinessObject>()));
			AssertNull(descriptor.GetZPropertyInfo(dummy));

			CustomBusinessObjectExtensions.GetCustomBusinessObject(dummy, true); // Initialize custom business object
			AssertNotNull(descriptor.GetZPropertyInfo(dummy));
			AssertEquals("__C11__prop__ZString", descriptor.GetZPropertyInfo(dummy).Name);

			WorkflowCustomPropertyDescriptorForTest descriptor1 = new WorkflowCustomPropertyDescriptorForTest("__C11__prop__ZString", typeof(ZString), true);
			WorkflowCustomFieldsGridInitializerTest.DummyWithCustomBizo dummy1 = Factory.New<WorkflowCustomFieldsGridInitializerTest.DummyWithCustomBizo>();
			dummy1.SubType1 = "T1";
			AssertNotNull("Should automaticall initialize custom bizo and get custom properties", descriptor1.GetZPropertyInfo(dummy1));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			PrepareTemplates();
		}

		void PrepareTemplates()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template1.P0_SubType1 = "T1";

			GenCustomColumnDefinition def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();
		}

		class WorkflowCustomPropertyDescriptorForTest : WorkflowCustomPropertyDescriptor
		{
			public WorkflowCustomPropertyDescriptorForTest(string name, Type propertyType, bool initializeCustomBizo = false) : base(new CustomPropertyImplementation<DummyBusinessObject>(name, propertyType), initializeCustomBizo) { }

			public ICustomPropertyContainer GetCustomPropertyContainerExposed(object component)
			{
				return GetCustomPropertyContainer(component);
			}
		}

		#endregion
	}
}
