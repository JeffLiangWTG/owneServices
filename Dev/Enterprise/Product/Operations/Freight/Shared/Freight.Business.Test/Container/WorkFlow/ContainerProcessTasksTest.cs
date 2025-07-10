using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ContainerProcessTask))]
	sealed class ContainerProcessTasksTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			return container.WorkflowItems.AddNew();
		}

		#endregion

		public void TestParentIsNull()
		{
			var taskNoParent = Factory.New<ContainerProcessTask>();
			AssertNull(taskNoParent.Parent);

			AssertExceptionThrown<NullReferenceException>("Parent returns null.", () => taskNoParent.Parent.ContainerParent?.GetType());

			AssertNoExceptionThrown("Parent is null, but there is a check on null.", () => taskNoParent.Parent?.ContainerParent?.GetType());
		}
	}
}
