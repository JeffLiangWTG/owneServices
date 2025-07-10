using System;
using CargoWise.EntityFramework;
using CargoWise.Workflow;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskDeletableRelationWrapper<T> : IDeletableItem where T : IBusiness
	{
		public ProcessTaskDeletableRelationWrapper(T related)
		{
			if (related == null)
			{
				throw new ArgumentNullException(nameof(related));
			}

			this.related = related;
		}

		public void Delete() => related.Delete();

		readonly T related;
	}
}
