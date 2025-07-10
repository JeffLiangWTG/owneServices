using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public class EmptyTransformationTasks : TransformationTasks
	{
		protected override IEnumerable<IDataTransformationTask> Tasks => Enumerable.Empty<IDataTransformationTask>();
	}
}
