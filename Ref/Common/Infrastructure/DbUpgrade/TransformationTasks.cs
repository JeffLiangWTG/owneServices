using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public abstract class TransformationTasks
	{
		public void Run(int fromVersion, int toVersion, IDbTransaction trans)
		{
			Argument.Argument.NotNull(trans, nameof(trans));
			var tasks = Tasks.Where(x => x.Version > fromVersion && x.Version <= toVersion).OrderBy(x => x.Version).ToArray();
			foreach (var task in tasks)
			{
				task.Run(trans);
			}
		}

		protected abstract IEnumerable<IDataTransformationTask> Tasks { get; }
	}
}
