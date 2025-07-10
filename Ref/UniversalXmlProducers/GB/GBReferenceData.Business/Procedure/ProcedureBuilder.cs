using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Business;
using static System.FormattableString;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Procedure
{
	public class ProcedureBuilder : IProcedureBuilder
	{
		public ProcedureBuilder(StringBuilder errorCollector)
		{
			if (errorCollector == null)
			{
				throw new ArgumentNullException(nameof(errorCollector));
			}

			ErrorCollector = errorCollector;
		}
		StringBuilder ErrorCollector;

		public void BuildXml(DateTime publicationDate, IEnumerable<ProcedureCodeData> data, IEnumerable<CategoryProcedureMapping> categoryProcedureMapping, string outputPath)
		{
			var content = ConvertToRefModels(data, categoryProcedureMapping);
			Helper.ExportToXMLFile(XMLWriterDataSource, Path.Combine(outputPath, GetOutputFileName(publicationDate)), XmlWriterConfiguration(), publicationDate, UpdateType.Full, content);
		}

		protected static string FilePrefix => "GB_RefCusProcedure";
		protected static string XMLWriterDataSource => "CDS RefCusProcedure";
		protected static string GetOutputFileName(DateTime publicationDate) => Invariant($"{FilePrefix}_{publicationDate:HHmmssfff}.xml");

		protected static XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusProcedure>(true);

			entityConfig.IncludeColumn(x => x.ZZ6_ProcedureCode, true);
			entityConfig.IncludeColumn(x => x.ZZ6_PreviousProcedureCode, true);
			entityConfig.IncludeColumn(x => x.ZZ6_Concession, true);
			entityConfig.IncludeColumn(x => x.ZZ6_Description);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZ6_ZZZ_NKDataGrouping, true, "CDS");
			entityConfig.IncludeColumn(x => x.ZZ6_ShipmentType);
			entityConfig.IncludeColumn(x => x.ZZ6_Group);
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoWarehouse, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfWarehouse, false, "N");
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZ6_StartDate, false, DefaultValues.MinimumDateTime);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZ6_EndDate, false, DefaultValues.MaximumDateTime);
			entityConfig.IncludeColumnWithDefaultValue(x => x.RefCusProcedureAttributes, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoTemporaryImport, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoTemporaryExport, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfTemporaryImport, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfTemporaryExport, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoInwardProcessing, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_IntoOutwardProcessing, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_OutOfInwardProcessing, false, "N");
			entityConfig.IncludeColumnWithDefaultValue(x => x.ZZ6_OutofOutwardProcessing, false, "N");

			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			var attribConfig = new EntityTypeConfiguration<RefCusProcedureAttribute>(true);
			attribConfig.IncludeColumn(x => x.ZXB_Name, true);
			attribConfig.IncludeColumn(x => x.ZXB_Value, true);

			writerConfig.IncludeEntityTypeConfiguration(attribConfig);

			return writerConfig;
		}

		public const string ShipmentTypeImport = "IMP";
		public const string ShipmentTypeExport = "EXP";

		protected IEnumerable<RefCusProcedure> ConvertToRefModels(IEnumerable<ProcedureCodeData> data, IEnumerable<CategoryProcedureMapping> categoryProcedureMapping)
		{
			var results = new List<RefCusProcedure>();
			foreach (var procedureCodeData in data)
			{
				var trimmedCode = procedureCodeData.ProcedureCode.Trim().Substring(0, 4);
				var procedureCode = trimmedCode.Substring(0, 2);
				var previousCode = trimmedCode.Substring(trimmedCode.Length - 2);
				var isImport = procedureCodeData.ShipmentType == ShipmentTypeImport;
				var isExport = procedureCodeData.ShipmentType == ShipmentTypeExport;
				results.Add(new RefCusProcedure()
				{
					ZZ6_Concession = procedureCodeData.AdditionalProcedureCode,
					ZZ6_Description = procedureCodeData.Description,
					ZZ6_Group = GetCategoryGroup(trimmedCode, categoryProcedureMapping),
					ZZ6_IntoWarehouse = procedureCode == "07" || procedureCode == "71" ? "Y" : "N",
					ZZ6_OutOfWarehouse = previousCode == "71" || (isExport && previousCode == "07") ? "Y" : isImport && previousCode == "78" ? "Y" : "N",
					ZZ6_ProcedureCode = procedureCode,
					ZZ6_PreviousProcedureCode = previousCode,
					ZZ6_ShipmentType = procedureCodeData.ShipmentType,
					ZZ6_IntoTemporaryImport = isImport && procedureCode == "53" ? "Y" : "N",
					ZZ6_IntoTemporaryExport = isExport && procedureCode == "23" ? "Y" : "N",
					ZZ6_OutOfTemporaryImport = previousCode == "53" ? "Y" : "N",
					ZZ6_OutOfTemporaryExport = previousCode == "23" ? "Y" : "N",
					ZZ6_IntoInwardProcessing = procedureCode == "51" ? "Y" : "N",
					ZZ6_IntoOutwardProcessing = procedureCode == "21" || procedureCode == "22" ? "Y" : "N",
					ZZ6_OutOfInwardProcessing = previousCode == "51" ? "Y" : "N",
					ZZ6_OutofOutwardProcessing = previousCode == "21" || previousCode == "22" ? "Y" : "N",
				});
			}
			return results;
		}

		string GetCategoryGroup(string procedureCode, IEnumerable<CategoryProcedureMapping> categoryProcedureMapping)
		{
			var groups = new List<string>();

			foreach (var category in categoryProcedureMapping)
			{
				if (ScanByProcedureCode(procedureCode, category) || ScanByProcedureSubCode(procedureCode, category) || ScanCodeRange(procedureCode, category))
				{
					groups.Add(category.CategoryCode);
				}
			}
			groups = groups.Select(x => specialCategoryCodeMap.ContainsKey(x) ? specialCategoryCodeMap[x] : x).Distinct().OrderBy(x => x).ToList();

			return string.Join(",", groups);
		}

		static bool ScanByProcedureCode(string procedureCode, CategoryProcedureMapping category)
			=> category.ProcedureMapping.Split(',').Select(x => x.Trim()).Contains(procedureCode);

		static bool ScanByProcedureSubCode(string procedureCode, CategoryProcedureMapping category)
			=> ScanByProcedureCode(procedureCode.Substring(0, 2), category);

		static bool ScanCodeRange(string procedureCode, CategoryProcedureMapping category)
		{
			var rangeBounds = category.ProcedureMapping.Split(new[] { "\u2013" }, StringSplitOptions.RemoveEmptyEntries);
			if (rangeBounds.Length != 2)
			{
				rangeBounds = category.ProcedureMapping.Split('-').Select(x => x.Trim()).ToArray();
			}

			if (rangeBounds.Length == 2
				&& int.TryParse(rangeBounds[0], out var lBound) && int.TryParse(rangeBounds[1], out var uBound)
				&& int.TryParse(procedureCode, out var numProcCode))
			{
				return lBound <= numProcCode && numProcCode <= uBound;
			}
			return false;
		}

		readonly ImmutableDictionary<string, string> specialCategoryCodeMap = new Dictionary<string, string> {
			{ "BIRDS", "21B" }, { "C21i", "21I"}, { "C21i EIDR NOP", "21N" },
			{ "C21e", "21E" }, { "C21e EIDR NOP", "CEN" },
			{ "FSD", "FS" },
			{ "C1 C&F", "C1" }, { "C1 B&E", "C1" },
			{ "I1 C&F", "I1" }, { "I1 B&E", "I1" }
		}.ToImmutableDictionary();
	}
}
