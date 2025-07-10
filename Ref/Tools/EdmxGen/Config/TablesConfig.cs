using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public class TablesConfig : ITablesConfig
	{
		public Dictionary<string, ColumnConfiguration[]> GetTablesConfigurations()
		{
			var result = new Dictionary<string, ColumnConfiguration[]>();

			var tables = YamlHelper.GetYamlMapping(Constants.YamlConfig.TablesConfig);

			foreach (var item in tables.Children)
			{
				var tblName = item.Key.ToString();
				var tblColumnsConfigurations = new List<ColumnConfiguration>();
				var columnsMapping = (YamlMappingNode)item.Value;

				foreach (var cln in columnsMapping.Children)
				{
					if (cln.Value is YamlMappingNode)
					{
						var valueMappingNode = cln.Value.ToYamlMappingNode();

						var isNullable = Convert.ToBoolean(valueMappingNode["IsNullable"]?.ToString(), System.Globalization.CultureInfo.InvariantCulture);
						var references = valueMappingNode["References"]?.ToYamlSequenceNode();

						var logicalRelationships = new List<LogicalRelationship>();
						foreach (var reference in references.Children)
						{
							logicalRelationships.Add(
								new LogicalRelationship
								{
									Table = reference.ToString(),
									ReferencedTable = tblName,
									ReferencedColumn = cln.Key.ToString(),
									IsNullable = isNullable
								});
						}

						tblColumnsConfigurations.Add(new ColumnConfiguration
						{
							Column = cln.Key.ToString(),
							LogicalRelationships = logicalRelationships
						});
					}
					else
					{
						tblColumnsConfigurations.Add(new ColumnConfiguration { Column = cln.Key.ToString(), Configuration = cln.Value.ToString() });
					}
				}
				result.Add(tblName, tblColumnsConfigurations.ToArray());
			}

			return result;
		}
	}

	public class ColumnConfiguration
	{
		public string Column { get; set; }
		public string Configuration { get; set; }
		public IEnumerable<ILogicalRelationship> LogicalRelationships { get; set; }

		public Tuple<string, string> GetConfigurationValue()
		{
			if (string.IsNullOrEmpty(Configuration))
			{
				return null;
			}

			var splitConfigValue = Configuration.Split('=');
			if (splitConfigValue.Length <= 1)
			{
				throw new ArgumentException("Invalid configuration value");
			}

			return Tuple.Create(splitConfigValue[0], splitConfigValue[1]);
		}
	}
}
