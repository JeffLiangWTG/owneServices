using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public interface ITablesConfig
	{
		Dictionary<string, ColumnConfiguration[]> GetTablesConfigurations();
	}
}
