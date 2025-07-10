using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Tools.Common
{
	public interface IDataSetHelper
	{
		Dictionary<string, List<FKRelationship>> GetAllTableAndFKs();
		Dictionary<string, List<string>> GetAllTableAndColumns();
	}
}
