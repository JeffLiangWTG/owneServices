using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	class PostUpgradeTransformationTasks : TransformationTasks
	{
		protected override IEnumerable<IDataTransformationTask> Tasks
		{
			get { yield return new DataSetTimstampTransformation(); }
		}
	}
}
