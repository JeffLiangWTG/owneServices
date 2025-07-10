using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Tools.Common
{
	public class FKRelationshipProvider
	{
		public FKRelationshipProvider(Dictionary<string, string[]> dataSetsDictionary)
		{
			Argument.NotNull(dataSetsDictionary, nameof(dataSetsDictionary));
			this.dataSetsDictionary = dataSetsDictionary;
		}

		public Dictionary<string, List<FKRelationship>> GetFilteredTableAndFKs(Dictionary<string, List<FKRelationship>> tableAndFKsDic)
		{
			var filteredTableAndFKs = new Dictionary<string, List<FKRelationship>>();
			foreach (var table in tableAndFKsDic.Keys)
			{
				filteredTableAndFKs[table] = new List<FKRelationship>();
				var fkList = tableAndFKsDic[table];
				foreach (var fk in fkList)
				{
					if (TableAndReferencedTableInSameDataSet(fk))
					{
						filteredTableAndFKs[table].Add(fk);
					}
				}
			}
			return filteredTableAndFKs;
		}

		bool TableAndReferencedTableInSameDataSet(FKRelationship fkRelation)
		{
			var table = fkRelation.Table;
			var referencedTable = fkRelation.ReferencedTable;
			var isSameDataSet = dataSetsDictionary.Values.Any(x => x.Contains(referencedTable) && x.Contains(table));
			return isSameDataSet;
		}

		readonly Dictionary<string, string[]> dataSetsDictionary;
	}
}
