using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using static System.FormattableString;
using static CargoWise.RefDbRepo.GBReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.UKOfficeCodes
{
	public class UKOfficeCodesParser
	{
		public UKOfficeCodesParser(StringBuilder errorCollector, byte[] odsData)
		{
			this.errorCollector = errorCollector ?? throw new ArgumentNullException(nameof(errorCollector));
			this.odsData = odsData ?? throw new ArgumentNullException(nameof(odsData));
		}

		public void Parse()
		{
			var officeCodes = GetUKOfficeCodes();
			var refOfficeCodes = ConvertToRefModels(officeCodes);
			if (refOfficeCodes.Any())
			{
				ExportToXMLFile(refOfficeCodes, UpdateType.Partial, DateTime.UtcNow);
			}
			else
			{
				errorCollector.Clear();
				errorCollector.AppendLine("Unable to import any UK Customs Office records. No valid data found!");
			}
		}

		protected IEnumerable<UKOfficeCode> GetUKOfficeCodes()
		{
			var dataTable = ODTFileHelper.GetTableFromCellContent(odsData, HeaderTag);
			var invalidData = new List<UKOfficeCode>();
			foreach (DataRow row in dataTable.Rows)
			{
				var officeCode = ConvertToModel(row);
				if (officeCode.IsValid)
				{
					yield return officeCode;
				}
				else
				{
					invalidData.Add(officeCode);
				}
			}

			if (invalidData.Any())
			{
				errorCollector.AppendLine("Unable to import the following UK Customs Office records:");
				invalidData.ForEach(data => { AppendInvalidDataError(data); });
			}
		}

		protected static UKOfficeCode ConvertToModel(DataRow row)
		{
			return new UKOfficeCode
			{
				Region = GetColumnStringValue(row[0]),
				City = GetColumnStringValue(row[1]),
				UsualName = GetColumnStringValue(row[2]),
				Code = GetColumnStringValue(row[3]),
				Roles = AddDefaultRoles()
			};
		}

		static IEnumerable<UKOfficeCodeRole> AddDefaultRoles()
		{
			yield return new UKOfficeCodeRole(Constants.UKOfficeCodeDefaults.RoleValues.EXT);
			yield return new UKOfficeCodeRole(Constants.UKOfficeCodeDefaults.RoleValues.EXP);
		}

		void AppendInvalidDataError(UKOfficeCode invalidOfficeCodeData)
		{
			errorCollector.AppendLine(new StringBuilder()
				.AppendJoin(" | ", $"- Code: {invalidOfficeCodeData.Code}",
				$"Description: {invalidOfficeCodeData.Description}",
				$"Usual Name: {invalidOfficeCodeData.UsualName}",
				$"City: {invalidOfficeCodeData.City}",
				$"Region: {invalidOfficeCodeData.Region}").ToString());
		}

		protected static IEnumerable<RefCusCodeList> ConvertToRefModels(IEnumerable<UKOfficeCode> data)
		{
			var results = new List<RefCusCodeList>();
			results.AddRange(data.Select(d => new RefCusCodeList
			{
				ZZD_Code = d.Code,
				ZZD_Description = TruncateDescription(d.Description),
				RefCusCodeListAttributes = AddRefModelRoleAttributes(d)
			}));

			return results;
		}

		static RefCusCodeListAttribute[] AddRefModelRoleAttributes(UKOfficeCode officeCode)
		{
			var roles = new List<RefCusCodeListAttribute>();
			roles.AddRange(officeCode.Roles.Select(r => new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = Constants.AttributeNames.Role,
				ZZE_Value = r.Value
			}));
			return roles.ToArray();
		}

		static string GetColumnStringValue(object obj)
		{
			var stringValue = obj?.ToString()?.Trim() ?? string.Empty;
			return stringValue == "—" ? string.Empty : stringValue;
		}

		static string TruncateDescription(string text) => text.Length <= Constants.UKOfficeCodeDefaults.DescriptionMaxLength ? text : string.Concat(text.AsSpan(0, Constants.UKOfficeCodeDefaults.DescriptionMaxLength - 1), "…");

		protected static string HeaderTag => UKOfficeCodeDefaults.HeaderTag;

		protected static string XMLWriterDataSource => $"{Constants.UKOfficeCodeDefaults.CodeTypeDescription}";

		protected static XmlWriterConfiguration XmlWriterConfiguration()
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);

			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.UKOfficeCodeDefaults.CodeType);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DefaultValues.GBDataGrouping);
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var cusCodeListAttributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_Value, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttributeConfig);
			return writerConfiguration;
		}

		protected void ExportToXMLFile<T>(IEnumerable<T> codeList, UpdateType updateType, DateTime publicationDate)
		{
			var writer = new XmlWriter(XmlWriterConfiguration());
			writer.SetDataSource(XMLWriterDataSource);
			writer.SetPublicationTime(publicationDate);
			writer.SetUpdateType(updateType);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(OutputFile);
		}

		protected virtual string OutputFile => Path.Combine(ConfigurationProvider.OutputDirectory, Invariant($"GB_RefCusCodeListZZ_{Constants.UKOfficeCodeDefaults.CodeType}.xml"));

		readonly StringBuilder errorCollector;
		readonly byte[] odsData;
	}
}
