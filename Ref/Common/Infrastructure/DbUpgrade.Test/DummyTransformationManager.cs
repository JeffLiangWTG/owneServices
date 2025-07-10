using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.DbUpgrade.Test
{
	class DummyTransformationManager : TransformationTasks
	{
		public DummyTransformationManager(IDataTransformationTask[] tasks)
		{
			this.tasks = tasks;
		}
		readonly IDataTransformationTask[] tasks;

		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get { return tasks; }
		}
	}
}
