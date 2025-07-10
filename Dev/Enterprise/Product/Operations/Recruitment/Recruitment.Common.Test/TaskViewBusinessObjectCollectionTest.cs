using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Common;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing
{
	[TestedType(typeof(TaskViewBusinessObjectCollection))]
	sealed class TaskViewBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TaskViewBusinessObjectCollection>
	{
		protected override TaskViewBusinessObjectCollection GetCollectionToTest()
		{
			var tvboc = new TaskViewBusinessObjectCollection(Factory)
			{
				GetNewElementToAddToTheCollection()
			};
			return tvboc;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new ProcessTaskView(Factory.NewWithValidTestData<ProcessTask>());
	}
}
