using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class SRDbUpdaterDependencyProvider : IUpdaterDependencyProvider<ISRDbDataSetUpdater>
	{
		public SRDbUpdaterDependencyProvider(ISRDbDataSetUpdater[] updaters)
		{
			this.updaters = updaters;
		}
		readonly ISRDbDataSetUpdater[] updaters;

		public IEnumerable<ISRDbDataSetUpdater> GetAllLeaves()
		{
			return updaters.Where(x => !updaters.Any(y => y.Prerequisites.Contains(x.UpdaterName)));
		}

		public IEnumerable<ISRDbDataSetUpdater> GetAllNodes()
		{
			return updaters;
		}

		public IEnumerable<ISRDbDataSetUpdater> GetAllRoots()
		{
			return updaters.Where(x => x.Prerequisites.Length == 0);
		}

		public IEnumerable<ISRDbDataSetUpdater> GetChildren(ISRDbDataSetUpdater updater)
		{
			return updaters.Where(x => x.Prerequisites.Contains(updater.UpdaterName));
		}

		public IEnumerable<ISRDbDataSetUpdater> GetParents(ISRDbDataSetUpdater updater)
		{
			return updaters.Where(x => (updater).Prerequisites.Contains(x.UpdaterName));
		}

		public void Initialize()
		{
		}
	}
}
