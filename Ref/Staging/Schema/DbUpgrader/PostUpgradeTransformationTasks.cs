using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	class PostUpgradeTransformationTasks : TransformationTasks
	{
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get { return Enumerable.Empty<IDataTransformationTask>(); }
		}
	}
}
