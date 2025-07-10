using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NetTopologySuite.Geometries;
using CargoWise.RefDbRepo.Tools.Common;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.DataSetServiceGenerator
{
	public class ServiceGeneratorHelper
	{
		public ServiceGeneratorHelper(IDataSetHelper dataSetHelper)
		{
			schemaAssembly = typeof(RefDataGrouping).Assembly;
			contractAssembly = typeof(Models.RefDataGrouping).Assembly;
			tableAndColumnsDictionary = new Lazy<Dictionary<string, List<string>>>(() => dataSetHelper.GetAllTableAndColumns());
			tableAndFKsDictionary = new Lazy<Dictionary<string, List<FKRelationship>>>(() => dataSetHelper.GetAllTableAndFKs());
		}

		public string CreateServiceForDateSets(string[] dataSets)
		{
			Argument.NotNull(dataSets, nameof(dataSets));
			var dataSetName = dataSets[0];
			if (dataSets.Length == 1)
			{
				return CreateOneTableDataSetService(dataSets);
			}

			var contents = new StringBuilder();
			var pkAndTableCode = GetPKAndTableCode(tableAndColumnsDictionary.Value[dataSetName]);
			contents.AppendLine(CultureInfo.InvariantCulture, $@"
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
{usingString}

namespace CargoWise.RefDbRepo.NewService
{{
	public partial class {dataSetName}Service : ReferenceDataServiceBase<Models.{dataSetName}>
	{{
		protected override string TableCode => ""{pkAndTableCode.Item2}"";");

			var tableAndRelatedTypes = GetRelatedTypes(dataSets);
			contents.AppendLine(GetStaticConstructor(dataSets, tableAndRelatedTypes));

			contents.AppendLine(CultureInfo.InvariantCulture, $@"
		public {dataSetName}Service(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
		}}

		static MapperConfiguration config;
		static MapperConfiguration Config
		{{
			get {{ return config; }}
			set
			{{
				if (config != null) throw new InvalidProgramException(""Mapper configuration should not be set second time"");
				else config = value;
			}}
		}}");

			var dataCoreContents = GetDataCore(dataSets, pkAndTableCode, tableAndRelatedTypes);
			if (string.IsNullOrEmpty(dataCoreContents))
			{
				return null;
			}
			else
			{
				contents.AppendLine(dataCoreContents);
			}

			contents.AppendLine(CultureInfo.InvariantCulture, $@"
		public override IEnumerable<Models.{dataSetName}> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}}
	}}
}}");

			return contents.ToString();
		}

		string CreateOneTableDataSetService(string[] dataSets)
		{
			Argument.NotNull(dataSets, nameof(dataSets));
			var dataSetName = dataSets[0];
			var content = $@"
{usingString}

namespace CargoWise.RefDbRepo.NewService
{{
	public partial class {dataSetName}Service : OneTableUpdateService<{dataSetName}, Models.{dataSetName}>
	{{
		public {dataSetName}Service(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
		}}
	}}
}}
";
			return content;
		}

		protected virtual string GetStaticConstructor(string[] dataSets, Dictionary<string, List<PropertyInfo>> tableAndRelatedTypes)
		{
			Argument.NotNull(dataSets, nameof(dataSets));
			Argument.NotNull(tableAndColumnsDictionary, nameof(tableAndColumnsDictionary));
			var dataSetName = dataSets[0];
			var contents = ($@"
		static {dataSetName}Service()
		{{
			Config = new MapperConfiguration(cfg =>
			{{
");

			var containByteArray = false;
			var strBuilder = new StringBuilder();
			for (int i = 0; i < dataSets.Length; i++)
			{
				var properties = schemaAssembly.GetType($"{"CargoWise.RefDbRepo.Service.Schema_0_9_New"}.{dataSets[i]}").GetProperties();
				if (properties.Any(p => p.PropertyType == typeof(byte[])))
				{
					containByteArray = true;
					break;
				}
			}

			var typeList = new List<string>();
			foreach (var table in tableAndRelatedTypes.Keys)
			{
				typeList.Add(table);
				typeList.AddRange(tableAndRelatedTypes[table].Select(p => p.PropertyType.Name.Replace("[]", string.Empty).Trim()));
			}
			typeList = typeList.Distinct().OrderBy(x => x).ToList();
			foreach (var type in typeList)
			{
				var geographyColumn = string.Empty;
				var properties = schemaAssembly.GetType($"{"CargoWise.RefDbRepo.Service.Schema_0_9_New"}.{type}").GetProperties();
				var property = properties.FirstOrDefault(p => p.PropertyType.Name == nameof(Geometry));
				if (property != null)
				{
					geographyColumn = property.Name;
				}
				if (type == "StmNote")
				{
					strBuilder.AppendLine(@"				cfg.CreateMap<StmNote, Models.StmNote>().ForMember(dest => dest.ST_Table, opt => opt.MapFrom(src => nameof(RefAirline)));");
				}
				else
				{
					if (string.IsNullOrEmpty(geographyColumn))
					{
						strBuilder.AppendLine(CultureInfo.InvariantCulture, $@"				cfg.CreateMap<{type}, Models.{type}>();");
					}
					else
					{
						strBuilder.AppendLine(CultureInfo.InvariantCulture, $@"				cfg.CreateMap<{type}, Models.{type}>().ForMember(x => x.{geographyColumn}, o => o.MapFrom(s => s.{geographyColumn}.AsText()));");
					}
				}
			}

			strBuilder.Append(@"			});
		}");

			if (containByteArray)
			{
				contents = contents + @"				cfg.AllowNullCollections = true;" + Environment.NewLine;
			}
			return contents + strBuilder.ToString();
		}

		string GetDataCore(string[] dataSets, Tuple<string, string> pkAndTableCode, Dictionary<string, List<PropertyInfo>> tableAndRelatedTypes)
		{
			Argument.NotNull(dataSets, nameof(dataSets));
			Argument.NotNull(pkAndTableCode, nameof(pkAndTableCode));
			Argument.NotNull(tableAndRelatedTypes, nameof(tableAndRelatedTypes));

			var dataSetName = dataSets[0];
			var dataSetVariableName = GetLocalVariableName(dataSetName);
			var contents = new StringBuilder();

			contents.Append(CultureInfo.InvariantCulture, $@"
		IEnumerable<Models.{dataSetName}> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{{
			var mapper = Config.CreateMapper();

			var dataSets = from {dataSetVariableName} in RefDbRepo.Get<{dataSetName}>()
						   join version in GetVersionControls(datasetId).Filter(lowerTimestamp, upperTimestamp, checkpoint) on {dataSetVariableName}.{pkAndTableCode.Item1} equals version.RVC_ParentPK
");
			var variableNameDic = new Dictionary<string, string>
			{
				{ dataSetName, dataSetVariableName }, { "RefDbVersionControl", "version.RVC_Deleted" }
			};
			var success = GetJoinStatements(dataSetName, pkAndTableCode.Item1, contents, tableAndRelatedTypes, variableNameDic);
			if (!success)
			{
				return null;
			}
			var keys = string.Join(", ", variableNameDic.Values);
			contents.Append(CultureInfo.InvariantCulture, $@"
						   select new {{ {keys} }};

			// GroupBy keeps the order of the elements in source that produced the first key.
			// https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.groupby?view=net-8.0
			var dataSetGroups = dataSets.OrderBy(x => x.{dataSetVariableName}.{pkAndTableCode.Item1}).GroupBy(x => x.{dataSetVariableName}.{pkAndTableCode.Item1});

			int idx = 0;
			foreach (var dataSetG in dataSetGroups)
			{{
				var dataSet = dataSetG.FirstOrDefault().{dataSetVariableName};
				var result = mapper.Map<Models.{dataSetName}>(dataSet);
				result.Deleted = dataSetG.FirstOrDefault().RVC_Deleted;
				if (!result.Deleted)
				{{
");

			contents.Append(SetupProperties(dataSetName, variableNameDic, tableAndRelatedTypes, true));
			var dg_Code_EF7 = string.Empty;
			if (dataSetName == "UNDGSubstance")
			{
				dg_Code_EF7 = "\r\n				result.DG_Code = result.DG_UNNO + result.DG_Variant;";
			}
			contents.Append(CultureInfo.InvariantCulture, $@"				}}{dg_Code_EF7}
				result.WriteCheckpoint(dataSetG.Key, ref idx, chunkSize);
				yield return result;
			}}
		}}");
			return contents.ToString();
		}

		protected virtual bool GetJoinStatements(string dataSetName, string dataSetPK, StringBuilder contents, Dictionary<string,List<PropertyInfo>> tableAndRelatedTypes, Dictionary<string, string> variableNameDic)
		{
			Argument.NotNullOrEmpty(dataSetName, nameof(dataSetName));
			Argument.NotNullOrEmpty(dataSetPK, nameof(dataSetPK));
			Argument.NotNull(contents, nameof(contents));
			Argument.NotNull(tableAndRelatedTypes, nameof(tableAndRelatedTypes));
			Argument.NotNull(variableNameDic, nameof(variableNameDic));

			var dataSetVariableName = GetLocalVariableName(dataSetName);

			foreach (var tableName in tableAndRelatedTypes.Keys)
			{
				var relatedPropertyList = tableAndRelatedTypes[tableName].OrderBy(x => x.Name);
				foreach (var relatedProperty in relatedPropertyList)
				{
					var isRelatedTableDataSet = false;
					var relatedTable = relatedProperty.PropertyType.Name;
					FKRelationship fkRelationship = null;
					var relatedTableName = relatedTable;
					var name = GetLocalVariableName(relatedTable.Replace("[]", string.Empty).Trim());
					if (!relatedTable.Contains("[]"))
					{
						isRelatedTableDataSet = true;
						fkRelationship = tableAndFKsDictionary.Value[tableName].FirstOrDefault(x => x.ReferencedTable == relatedTableName);
					}
					else
					{
						relatedTableName = relatedTable.Replace("[]", string.Empty).Trim();
						if (tableAndFKsDictionary.Value.ContainsKey(relatedTableName))
						{
							fkRelationship = tableAndFKsDictionary.Value[relatedTableName].FirstOrDefault(x => x.ReferencedTable == tableName);
						}
						if (fkRelationship == null)
						{
							if (DataSetHelper.ContainParentPKAndCodeColumns(tableAndColumnsDictionary.Value[relatedTableName], out var parentPKAndCodeColumns))
							{
								var parentPKColumn = parentPKAndCodeColumns.Item1;
								var property = schemaAssembly.GetType($"{"CargoWise.RefDbRepo.Service.Schema_0_9_New"}.{relatedTableName}").GetProperties().First(x => x.Name == parentPKColumn);
								var nullable = property.PropertyType == typeof(Guid?);
								parentPKColumn += nullable ? ".Value" : "";
								contents.AppendLine(CultureInfo.InvariantCulture, $@"
						   join {name} in RefDbRepo.Get<{relatedTableName}>()
						   on new {{ c = {dataSetVariableName}.{dataSetPK}, l = TableCode }} equals new {{ c = {name}.{parentPKColumn}, l = {name}.{parentPKAndCodeColumns.Item2} }} into {name}G
						   from {name} in {name}G.DefaultIfEmpty()");
								variableNameDic[relatedTableName] = name;
								continue;
							}
						}
					}

					if (fkRelationship != null)
					{
						var referencedName = GetLocalVariableName(fkRelationship.ReferencedTable);
						var referencedColumn = fkRelationship.ReferencedColumn;
						var column = fkRelationship.Column;
						if (referencedName == name)
						{
							referencedName = GetLocalVariableName(fkRelationship.Table);
							referencedColumn = fkRelationship.Column;
							column = fkRelationship.ReferencedColumn;
						}
						var notDeletedCondition = isRelatedTableDataSet ? ".WhereNotDeleted(RefDbRepo)" : string.Empty;
						var referencedCondition = $"{referencedName}.{referencedColumn}";
						if (tableName != dataSetName && !isRelatedTableDataSet)
						{
							referencedCondition = $"({referencedName} != null ? {referencedName}.{referencedColumn} : Guid.Empty)";
						}

						contents.AppendLine(CultureInfo.InvariantCulture, $@"
						   join {name} in RefDbRepo.Get<{relatedTableName}>(){notDeletedCondition} on {referencedCondition} equals {name}.{column} into {name}G
						   from {name} in {name}G.DefaultIfEmpty()");
						variableNameDic[relatedTableName] = name;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Failed to create {dataSetName} service because {tableName} does not have foreign key.");
						Console.ResetColor();
						return false;
					}
				}
			}
			return true;
		}

		protected virtual string SetupProperties(string dataSetName, Dictionary<string, string> variableNameDic, Dictionary<string, List<PropertyInfo>> tableAndRelatedTypes, bool isEF7 = false)
		{
			var contents = new StringBuilder();
			var relatedProperties = tableAndRelatedTypes[dataSetName].OrderBy(x => x.Name);
			foreach (var property in relatedProperties)
			{
				var type = property.PropertyType.Name;
				var propertyName = property.Name;
				if (!type.Contains("[]"))
				{
					var variableName = variableNameDic[type];
					contents.AppendLine(CultureInfo.InvariantCulture, $@"						result.{propertyName} = mapper.Map<Models.{type}>({(isEF7 ? $"dataSetG.FirstOrDefault()" : $"dataSet[0]")}.{variableName});");
				}
				else
				{
					var typeName = type.Replace("[]", string.Empty).Trim();
					var variableName = variableNameDic[typeName];
					var pk = GetPKAndTableCode(tableAndColumnsDictionary.Value[typeName]).Item1;
					if (tableAndRelatedTypes.ContainsKey(typeName))
					{
						var childProperty = tableAndRelatedTypes[typeName].First();
						var childPropertyName = childProperty.Name;
						var childTypeName = childProperty.PropertyType.Name;
						var childTableName = childTypeName.Replace("[]", string.Empty).Trim();
						var childVariable = variableNameDic[childTableName];
						contents.Append(CultureInfo.InvariantCulture, $@"					result.{propertyName} = {(isEF7 ? $"dataSetG" : $"dataSet")}.Select(x => x.{variableName}).NotNull().DistinctByKey(x => x.{pk}).Select(d =>
					{{
						var a = mapper.Map<Models.{typeName}>(d);");

						if (!childTypeName.Contains("[]"))
						{
							var fkRelation = tableAndFKsDictionary.Value[typeName].First(x => x.ReferencedTable == childTableName);
							contents.AppendLine(CultureInfo.InvariantCulture, $@"
						a.{childPropertyName} = mapper.Map<Models.{childTypeName}>({(isEF7 ? $"dataSetG" : $"dataSet")}.Select(y => y.{childVariable}).FirstOrDefault(z => z != null && z.{fkRelation.ReferencedColumn} == d.{fkRelation.Column}));
						return a.{childPropertyName} != null ? a : null;
					}}).NotNull().ToArray();");
						}
						else
						{
							var fkRelation = tableAndFKsDictionary.Value[childTableName].First(x => x.ReferencedTable == typeName);
							var childPK = GetPKAndTableCode(tableAndColumnsDictionary.Value[childTableName]).Item1;
							contents.AppendLine(CultureInfo.InvariantCulture, $@"
						a.{childPropertyName} = {(isEF7 ? $"dataSetG" : $"dataSet")}.Select(y => y.{childVariable}).Where(x => x != null && x.{fkRelation.Column} == d.{fkRelation.ReferencedColumn}).DistinctByKey(x => x.{childPK}).Select(z => mapper.Map<Models.{childTableName}>(z)).ToArray();
						return a;
					}}).ToArray();");
						}
					}
					else
					{
						contents.AppendLine(CultureInfo.InvariantCulture, $@"						result.{propertyName} = {(isEF7 ? $"dataSetG" : $"dataSet")}.Select(x => x.{variableName}).NotNull().DistinctByKey(x => x.{pk}).Select(x => mapper.Map<Models.{typeName}>(x)).ToArray();");
					}
				}
			}
			return contents.ToString();
		}

		protected virtual Dictionary<string, List<PropertyInfo>> GetRelatedTypes(string[] dataSets)
		{
			var typeDictionary = new Dictionary<string, List<PropertyInfo>>();
			foreach (var tableName in dataSets)
			{
				var type = contractAssembly.GetType($"CargoWise.RefDbRepo.Common.Contract_0_9.{tableName}");
				var propertyInfos = type.GetProperties().Where(p => p.PropertyType.FullName.StartsWith("CargoWise.RefDbRepo.Common.Contract_0_9", StringComparison.OrdinalIgnoreCase));
				if (!propertyInfos.Any())
				{
					continue;
				}
				var propertyList = new List<PropertyInfo>();
				foreach (var property in propertyInfos)
				{
					var propertyType = property.PropertyType;
					if ((propertyType.BaseType == typeof(RefDataSet) && propertyType.Name != dataSets[0]) || (propertyType.BaseType == typeof(Array) && dataSets.Contains(propertyType.Name.Replace("[]", string.Empty).Trim())))
					{
						if (!propertyList.Select(x => x.PropertyType.Name).Contains(propertyType.Name))
						{
							propertyList.Add(property);
						}
					}
				}
				if (propertyList.Count > 0)
				{
					typeDictionary[tableName] = propertyList;
				}
			}
			return typeDictionary;
		}

		static Tuple<string, string> GetPKAndTableCode(List<string> columnList)
		{
			return DataSetHelper.GetPKAndTableCode(columnList);
		}

		static string GetLocalVariableName(string tableName)
		{
			var name = tableName;
			if (tableName.StartsWith("RefCus", StringComparison.OrdinalIgnoreCase))
			{
				name = tableName.Substring(6);
			}
			else if (tableName.StartsWith("Ref", StringComparison.OrdinalIgnoreCase))
			{
				name = tableName.Substring(3);
			}
			return char.ToLower(name[0], CultureInfo.InvariantCulture) + name.Substring(1);
		}

		readonly Assembly schemaAssembly;
		readonly Assembly contractAssembly;
		readonly Lazy<Dictionary<string, List<string>>> tableAndColumnsDictionary;
		readonly Lazy<Dictionary<string, List<FKRelationship>>> tableAndFKsDictionary;

		readonly string usingString = @"using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;";
	}
}
