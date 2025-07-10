using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTasksDependentCollectionViewFilterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypes()
		{
			CategorisedWorkflowTaskTypesCollection registryValue = new CategorisedWorkflowTaskTypesCollection();

			registryValue.AddNew().Code = "STA";
			registryValue.AddNew().Code = "DUM";

			registryValue[0].TaskTypes.AddNew().Code = "ABC";
			registryValue[1].TaskTypes.AddNew().Code = "XYZ";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue);

			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				var mock = new Mock<ProcessTaskCollection>(new object[] { Factory.New<DummyWithWorkflow>() });
				mock.CallBase = true;
				ProcessTaskCollection collection = mock.Object;
				ProcessTaskCollectionViewFilter filter = new ProcessTaskCollectionViewFilter(collection);

				AssertEquals("ContainsCode(\"ABC\")", false, filter.Lookups.Types.ContainsCode("ABC"));
				AssertEquals("ContainsCode(\"XYZ\")", true, filter.Lookups.Types.ContainsCode("XYZ"));
			}
		}

		#region Implementation

		CategorisedWorkflowTaskTypesCollection originalTaskTypes;

		protected override void SetUp()
		{
			base.SetUp();
			originalTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected override void TearDown()
		{
			base.TearDown();
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalTaskTypes);
		}

		#endregion
	}
}
