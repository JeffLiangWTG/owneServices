using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskTriggerConditionsViewModel))]
	class ProcessTaskTriggerConditionsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var trigger = job.WorkflowItems.Triggers.AddNew();

			return new ProcessTaskTriggerConditionsViewModel(trigger);
		}
	}
}
