using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ProcessTaskSystemLastEditTimeTest : TestCaseWithFactory
	{
		class AfterSavingProcessor : IAfterOnSavingBOProcessingService
		{
			readonly ProcessTask instance;

			public AfterSavingProcessor(ProcessTask instance)
			{
				this.instance = instance;
			}
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				instance.P9_Description = "XYZ";
			}
		}

		[TestDateIncremental(hours: 1)]
		public void TestLastEditTimeUpdatedOnObjectsModifiedInAfterOnSaving()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var task1 = dummy.WorkflowItems.Tasks.AddNew();
			var task2 = dummy.WorkflowItems.Tasks.AddNew();
			var task3 = dummy.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			task1.P9_Description = "ABC";
			task3.P9_Description = "TUV";

			Factory.ServiceContainer.AddAfterOnSavingService(new AfterSavingProcessor(task2));

			Factory.Save();

			AssertEquals(task1.P9_SystemLastEditTimeUtc, task3.P9_SystemLastEditTimeUtc);
			AssertEquals(task1.P9_SystemLastEditTimeUtc, task2.P9_SystemLastEditTimeUtc);
		}

		[TestDateIncremental(hours: 1)]
		public void TestLastEditTimeUTCUpdated_LightValidation()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			var lastEditTimeUtc = task.P9_SystemLastEditTimeUtc;
			(task as ILightValidationInternals).IsValid = true;
			Factory.Save();
			var taskInDB = new BusinessObjectFactory() { RefreshEnabled = false }.Load<ProcessTask>(task.PK);

			AssertLessThan("P9_SystemLastEditTimeUtc should be updated.", lastEditTimeUtc, taskInDB.P9_SystemLastEditTimeUtc);
		}
	}
}
