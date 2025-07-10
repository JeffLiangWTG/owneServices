using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public static class ISailingSynchronisationTargetCollectionExtensions
	{
		public static void Synchronise<TSailingSynchronisationSource, TSailingSynchronisationTarget>(
			this ISailingSynchronisationTargetCollection<TSailingSynchronisationSource, TSailingSynchronisationTarget> targets,
			IEnumerable<TSailingSynchronisationSource> sources, bool deleteOrphanTarget = true)
			where TSailingSynchronisationSource : BusinessObject
			where TSailingSynchronisationTarget : ISailingSynchronisationTarget<TSailingSynchronisationSource>
		{
			var orphanTargets = new List<TSailingSynchronisationTarget>(targets);
			foreach (var source in sources)
			{
				var target = orphanTargets.FirstOrDefault(x => x.IsMatched(source));
				if (target == null)
				{
					target = targets.AddNew();
					target.Set(source);
				}
				else
				{
					orphanTargets.Remove(target);
				}
				target.Synchronise();
			}

			if (deleteOrphanTarget)
			{
				foreach (var target in orphanTargets)
				{
					targets.Delete(target);
				}
			}
		}
	}
}
