using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class SharedDataSetUpdater : Customs.Shared.ISharedDataSetUpdater
	{
		public SharedDataSetUpdater(string updaterName, IEnumerable<string> dataSetNames)
		{
			Argument.NotNullOrEmpty(updaterName, nameof(updaterName));
			Argument.NotNull(dataSetNames, nameof(dataSetNames));

			Name = updaterName;
			DataSetNames = dataSetNames;
		}

		public string Name { get; }
		public IEnumerable<string> DataSetNames { get; }
	}
}
