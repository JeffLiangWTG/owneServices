using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	internal class DataSetStructureProviderFixture
	{
		[Test]
		public void GetDataSetsFromTTFile()
		{
			var dataSets = DataSetStructureProvider.StructuredDataSets.ToArray();
			Assert.That(dataSets, Has.Length.GreaterThan(0));
			Assert.That(dataSets.Any(x => x.Contains("RefCusTariff")), Is.True);

			var tariffDataSet = dataSets.FirstOrDefault(x => x.Contains("RefCusTariff"));
			Assert.That(tariffDataSet, Is.Not.Null);

			Assert.That(tariffDataSet.Any(x => x.Contains("RefCusRate")));
			Assert.That(tariffDataSet.Any(x => x.Contains("RefCusCondition")));
			Assert.That(tariffDataSet.Any(x => x.Contains("RefCusApplicability")));
		}

		void AssertDataSetOrdering(string dataSetName, string[] dataSetStructure, SqlConnection conn)
		{
			if (dataSetStructure.Length <= 2)
			{
				Assert.IsTrue(true);
				return;
			}
			var result = true;
			for (var x = 1; x < dataSetStructure.Length; x++)
			{
				var tbl = dataSetStructure[x];
				var anotherTbls = dataSetStructure.Skip(x + 1);
				result = AssertTableDependency(tbl, anotherTbls, ref dataSetStructure, conn);
				if (!result)
				{
					AssertDataSetOrdering(dataSetName, dataSetStructure, conn);
					break;
				}
			}
			dataSetStructure = dataSetStructure.Select(x => $"\"{x}\"").ToArray();
			if (!result)
			{
				var message = $@"The expected structure on DataSet {dataSetName} is:
			new [] {{
				{string.Join(",\n				", dataSetStructure)}
			}}";
				Assert.Fail(message);
			}
		}

		bool AssertTableDependency(string tbl, IEnumerable<string> anotherTbls, ref string[] dataSetStructure, SqlConnection conn)
		{
			var tablesIn = string.Join(",", anotherTbls.Select(y => $"OBJECT_ID('{y}')"));
			if (string.IsNullOrEmpty(tbl) ||
				string.IsNullOrEmpty(tablesIn))
			{
				return true;
			}

			var sql = $@"select OBJECT_NAME(fk.referenced_object_id) from
sys.foreign_keys fk
where fk.parent_object_id = OBJECT_ID('{tbl}')
and fk.referenced_object_id IN ({tablesIn})";

			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = sql;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var tableToReorder = reader.GetString(0);
						var newStructure = new List<string>();
						for (var x = 0; x < dataSetStructure.Length; x++)
						{
							if (dataSetStructure[x] == tableToReorder)
							{
								continue;
							}
							else if (dataSetStructure[x] == tbl)
							{
								newStructure.AddRange(new[] { tableToReorder, tbl });
							}
							else
							{
								newStructure.Add(dataSetStructure[x]);
							}
						}
						dataSetStructure = newStructure.ToArray();
						return false;
					}
				}
			}
			return true;
		}
	}
}
