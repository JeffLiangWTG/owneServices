using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using ExcelParser;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser
{
	public static class Utils
	{
		public static XmlWriterConfiguration GetWriterConfiguration(string codeType = null)
		{
			var codelistConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codelistConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codelistConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			if (string.IsNullOrEmpty(codeType))
			{
				codelistConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			}
			else
			{
				codelistConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			}
			codelistConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "AU");
			codelistConfiguration.IncludeColumn(x => x.ZZD_StartDate, false);
			codelistConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, new DateTime(2079, 06, 06, 23, 59, 00));
			codelistConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(codelistConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);

			return writerConfiguration;
		}

		public static void ExportToXMLFile(string dataSource, Func<IEnumerable<RowResult>, bool, IEnumerable<RefCusCodeList>> generateRefCusCodelists, IEnumerable<RowResult> rowResults, string outputFile, XmlWriterConfiguration xmlWriterConfig, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));
			Argument.NotNullOrEmpty(outputFile, nameof(outputFile));
			Argument.NotNullOrEmpty(dataSource, nameof(dataSource));
			Argument.NotNull(generateRefCusCodelists, nameof(generateRefCusCodelists));
			Argument.NotNull(xmlWriterConfig, nameof(xmlWriterConfig));

			var writer = new Common.UniversalXmlWriter.XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(GetStartDateForRecords());
			writer.SetUpdateType(UpdateType.Full);

			var dirName = Path.GetDirectoryName(outputFile);
			Directory.CreateDirectory(dirName);

			var codelists = generateRefCusCodelists(rowResults, initialLoad);
			foreach (var codelist in codelists)
			{
				writer.PopulateData(codelist);
			}
			writer.SaveXml(outputFile);
		}

		static DateTime GetStartDateForRecords(bool initialLoad = false)
		{
			if (initialLoad)
			{
				return new DateTime(1900, 1, 1, 0, 0, 0);
			}
			if (ApplicationConfig.PublicationTime == null
				||
				!DateTime.TryParseExact(ApplicationConfig.PublicationTime, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDateTime))
			{
				startDateTime = new DateTime(1900, 1, 1, 0, 0, 0);
			}
			return startDateTime;
		}

		public static IEnumerable<RefCusCodeList> GenerateDataSetElementsForE21(IEnumerable<RowResult> rowResults, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			var result = new List<RefCusCodeList>();
			var startDateTime = GetStartDateForRecords(initialLoad);

			foreach (RowResult rowResult in rowResults)
			{
				var codelist = new RefCusCodeList()
				{
					ZZD_ZZK_NKCodeType = E21_CodeTypePrefix + rowResult["ZZD_ZZK_NKCodeType"],
					ZZD_Code = rowResult["ZZD_Code"],
					ZZD_Description = rowResult["ZZD_Description"],
					ZZD_ZZZ_NKDataGrouping = "AU",
					ZZD_StartDate = startDateTime
				};

				var scientificName = rowResult["ZZE_Value"];

				if (!string.IsNullOrEmpty(scientificName))
				{
					scientificName = scientificName.Length > 100 ? scientificName.Substring(0, 100) : scientificName;
					codelist.RefCusCodeListAttributes = new[]
					{
						new RefCusCodeListAttribute()
						{
							ZZE_ZXE_NKName = "ScientificName",
							ZZE_Value = scientificName
						}
					};
				}
				result.Add(codelist);
			}
			return result;
		}

		public static IEnumerable<RefCusCodeList> GenerateDataSetElementsForE01(IEnumerable<RowResult> rowResults, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			var result = new List<RefCusCodeList>();
			var startDateTime = GetStartDateForRecords(initialLoad);

			foreach (RowResult rowResult in rowResults)
			{
				var codelist = new RefCusCodeList()
				{
					ZZD_Code = rowResult["ZZD_Code"],
					ZZD_Description = rowResult["ZZD_Description"],
					ZZD_ZZZ_NKDataGrouping = "AU",
					ZZD_StartDate = startDateTime
				};

				var codelistAttributes = new List<RefCusCodeListAttribute>();

				codelistAttributes.Add(new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = "IsQuarantineRegion"
				});

				var commodities = rowResult["Commodities"];
				foreach (var commodity in commodities.Split(','))
				{
					codelistAttributes.Add(new RefCusCodeListAttribute()
					{
						ZZE_ZXE_NKName = CommodityMap[commodity]
					});
				}
				codelist.RefCusCodeListAttributes = codelistAttributes.ToArray();
				result.Add(codelist);
			}
			return result;
		}

		public static IEnumerable<RefCusCodeList> GenerateDataSetElementsForE29(IEnumerable<RowResult> rowResults, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			var result = new List<RefCusCodeList>();
			var startDateTime = GetStartDateForRecords(initialLoad);

			foreach (RowResult rowResult in rowResults)
			{
				var codelist = new RefCusCodeList()
				{
					ZZD_Code = rowResult["ZZD_Code"],
					ZZD_Description = rowResult["ZZD_Description"],
					ZZD_ZZZ_NKDataGrouping = "AU",
					ZZD_StartDate = startDateTime
				};

				codelist.RefCusCodeListAttributes = new[] {
					new RefCusCodeListAttribute()
					{
						ZZE_ZXE_NKName = "IsQuarantineOffice"
					},
					new RefCusCodeListAttribute()
					{
						ZZE_ZXE_NKName = "State",
						ZZE_Value = rowResult["State"]
					}
				};
				result.Add(codelist);
			}
			return result;
		}

		public static IEnumerable<RefCusCodeList> GenerateDataSetElementsForE25(IEnumerable<RowResult> rowResults, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			var result = new List<RefCusCodeList>();
			var startDateTime = GetStartDateForRecords(initialLoad);

			foreach (RowResult rowResult in rowResults)
			{
				var codelist = new RefCusCodeList()
				{
					ZZD_Code = rowResult["ZZD_Code"],
					ZZD_Description = rowResult["ZZD_Description"],
					ZZD_ZZZ_NKDataGrouping = "AU",
					ZZD_StartDate = startDateTime
				};

				var codelistAttributes = new List<RefCusCodeListAttribute>();
				var commodities = rowResult["Commodities"];
				foreach (var commodity in commodities.Split(','))
				{
					codelistAttributes.Add(new RefCusCodeListAttribute() { ZZE_ZXE_NKName = CommodityMap[commodity] });
				}
				codelist.RefCusCodeListAttributes = codelistAttributes.ToArray();
				result.Add(codelist);
			}
			return result;
		}

		public static IEnumerable<RefCusCodeList> GenerateDataSetElementsForE38(IEnumerable<RowResult> rowResults, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			var result = new List<RefCusCodeList>();
			var startDateTime = GetStartDateForRecords(initialLoad);

			foreach (RowResult rowResult in rowResults)
			{
				var codelist = new RefCusCodeList()
				{
					ZZD_Code = rowResult["ZZD_Code"],
					ZZD_Description = rowResult["ZZD_Code"],
					ZZD_ZZZ_NKDataGrouping = "AU",
					ZZD_StartDate = startDateTime
				};
				result.Add(codelist);
			}
			return result;
		}

		public static IEnumerable<RefCusCodeList> GenerateDataSetElementsForE39(IEnumerable<RowResult> rowResults, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			var result = new List<RefCusCodeList>();
			var startDateTime = GetStartDateForRecords(initialLoad);

			foreach (RowResult rowResult in rowResults)
			{
				var codelist = new RefCusCodeList()
				{
					ZZD_Code = rowResult["ZZD_Code"],
					ZZD_Description = rowResult["ZZD_Description"],
					ZZD_ZZZ_NKDataGrouping = "AU",
					ZZD_StartDate = startDateTime
				};
				result.Add(codelist);
			}
			return result;
		}

		public static IEnumerable<RefCusCodeList> GenerateDataSetElementsForE07(IEnumerable<RowResult> rowResults, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			var result = new List<RefCusCodeList>();
			var startDateTime = GetStartDateForRecords(initialLoad);

			foreach (RowResult rowResult in rowResults)
			{
				var codelist = new RefCusCodeList()
				{
					ZZD_ZZK_NKCodeType = E07_CodeTypePrefix + rowResult["ZZD_ZZK_NKCodeType"],
					ZZD_Code = rowResult["ZZD_Code"],
					ZZD_Description = rowResult["ZZD_Description"],
					ZZD_ZZZ_NKDataGrouping = "AU",
					ZZD_StartDate = startDateTime
				};

				codelist.RefCusCodeListAttributes = new[] {
					new RefCusCodeListAttribute()
					{
						ZZE_ZXE_NKName = "BoneInIndicator",
						ZZE_Value = rowResult["BoneInIndicator"]
					},
					new RefCusCodeListAttribute()
					{
						ZZE_ZXE_NKName = "IsBeefVeal",
						ZZE_Value = rowResult["BeefvealIndicator"]
					},
					new RefCusCodeListAttribute()
					{
						ZZE_ZXE_NKName = "IsChemicalLean",
						ZZE_Value = rowResult["ChemicalLeanIndicator"]
					}
				};
				result.Add(codelist);
			}
			return result;
		}

		static Dictionary<string, string> CommodityMap = new Dictionary<string, string>()
		{
			{ "D", "IsDairy" },
			{ "E", "IsEgg" },
			{ "F", "IsFish" },
			{ "G", "IsGrain" },
			{ "H", "IsHorticulture" },
			{ "I", "IsInedibleMeat" },
			{ "M", "IsMeat" },
			{ "S", "IsSkins" },
			{ "W", "IsWool" },
		};

		#region Sql Generator
		public static string GenerateSqlStringForE21(IEnumerable<RowResult> rowResults)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			string s = "";
			foreach (RowResult rowResult in rowResults)
			{
				s += $@"
DECLARE @ZZD_PK UNIQUEIDENTIFIER
SET @ZZD_PK = newid()
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) Values (@ZZD_PK, '{E21_CodeTypePrefix + rowResult["ZZD_ZZK_NKCodeType"]}', '{rowResult["ZZD_Code"]}', '{rowResult["ZZD_Description"]}', '1900-01-01T12:00:00', '2079-06-06T23:59:00', 'AU')";
				var scientificName = rowResult["ZZE_Value"];

				if (!string.IsNullOrEmpty(scientificName))
				{
					scientificName = scientificName.Length > 100 ? scientificName.Substring(0, 100) : scientificName;
					s += $@"INSERT INTO RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) Values (newid(), @ZZD_PK, 'ScientificName', '{scientificName}')
";
				}
			}
			return s;
		}

		public static string GenerateSqlStringForE01(IEnumerable<RowResult> rowResults)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			string s = "";
			foreach (RowResult rowResult in rowResults)
			{
				s += $@"
DECLARE @ZZD_PK UNIQUEIDENTIFIER
SET @ZZD_PK = newid()
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) Values (@ZZD_PK, 'AQISP', '{rowResult["ZZD_Code"]}', '{rowResult["ZZD_Description"]}', '1900-01-01T12:00:00', '2079-06-06T23:59:00', 'AU')
INSERT INTO RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName) Values (newid(), @ZZD_PK, 'IsQuarantineRegion')";
				var commodities = rowResult["Commodities"];
				foreach (var commodity in commodities.Split(','))
				{
					s += $@"
INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName) Values(newid(), @ZZD_PK, '{CommodityMap[commodity]}')";
				}
				s += "\r\n";
			}
			return s;
		}

		public static string GenerateSqlStringForE29(IEnumerable<RowResult> rowResults)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			string s = "";
			foreach (RowResult rowResult in rowResults)
			{
				s += $@"
DECLARE @ZZD_PK UNIQUEIDENTIFIER
SET @ZZD_PK = newid()
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) Values (@ZZD_PK, 'AQISP', '{rowResult["ZZD_Code"]}', '{rowResult["ZZD_Description"]}', '1900-01-01T12:00:00', '2079-06-06T23:59:00', 'AU')
INSERT INTO RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName) Values (newid(), @ZZD_PK, 'IsQuarantineOffice')
INSERT INTO RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) Values (newid(), @ZZD_PK, 'State', '{rowResult["State"]}')
";
			}
			return s;
		}

		public static string GenerateSqlStringForE25(IEnumerable<RowResult> rowResults)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			string s = "";
			foreach (RowResult rowResult in rowResults)
			{
				s += $@"
DECLARE @ZZD_PK UNIQUEIDENTIFIER
SET @ZZD_PK = newid()
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) Values (@ZZD_PK, 'SUPP', '{rowResult["ZZD_Code"]}', '{rowResult["ZZD_Description"]}', '1900-01-01T12:00:00', '2079-06-06T23:59:00', 'AU')";
				var commodities = rowResult["Commodities"];
				foreach (var commodity in commodities.Split(','))
				{
					s += $@"
INSERT INTO RefCusCodeListAttribute(ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName) Values(newid(), @ZZD_PK, '{CommodityMap[commodity]}')
";
				}
			}
			return s;
		}

		public static string GenerateSqlStringForE38(IEnumerable<RowResult> rowResults)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			string s = "";
			foreach (RowResult rowResult in rowResults)
			{
				s += $@"
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) Values (newid(), 'DOMP', '{rowResult["ZZD_Code"]}', '{rowResult["ZZD_Code"]}', '1900-01-01T12:00:00', '2079-06-06T23:59:00', 'AU')
";
			}
			return s;
		}

		public static string GenerateSqlStringForE39(IEnumerable<RowResult> rowResults)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			string s = "";
			foreach (RowResult rowResult in rowResults)
			{
				s += $@"
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) Values (newid(), 'ACERT', '{rowResult["ZZD_Code"]}', '{rowResult["ZZD_Description"]}', '1900-01-01T12:00:00', '2079-06-06T23:59:00', 'AU')
";
			}
			return s;
		}

		public static string GenerateSqlStringForE07(IEnumerable<RowResult> rowResults)
		{
			Argument.NotNull(rowResults, nameof(rowResults));

			string s = "";
			foreach (RowResult rowResult in rowResults)
			{
				s += $@"
DECLARE @ZZD_PK UNIQUEIDENTIFIER
SET @ZZD_PK = newid()
INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping) Values (@ZZD_PK, '{E07_CodeTypePrefix + rowResult["ZZD_ZZK_NKCodeType"]}', '{rowResult["ZZD_Code"]}', '{rowResult["ZZD_Description"]}', '1900-01-01T12:00:00', '2079-06-06T23:59:00', 'AU')
INSERT INTO RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) Values (newid(), @ZZD_PK, 'BoneInIndicator', '{rowResult["BoneInIndicator"]}')
INSERT INTO RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) Values (newid(), @ZZD_PK, 'IsBeefVeal', '{rowResult["BeefvealIndicator"]}')
INSERT INTO RefCusCodeListAttribute (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value) Values (newid(), @ZZD_PK, 'IsChemicalLean', '{rowResult["ChemicalLeanIndicator"]}')
";
			}
			return s;
		}
		#endregion

		public static string GetFileName(string fileCode)
		{
			if (!fileNameDictionary.ContainsKey(fileCode))
			{
				var fileName = ApplicationConfig.FileName(fileCode);
				fileNameDictionary.Add(fileCode, Path.Combine(binPath, fileName));
			}
			var filePath = fileNameDictionary[fileCode];
			return filePath;
		}

		const string E07_CodeTypePrefix = "CUTC";
		const string E21_CodeTypePrefix = "PROD";
		static string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		static Dictionary<string, string> fileNameDictionary = new Dictionary<string, string>();
	}
}
