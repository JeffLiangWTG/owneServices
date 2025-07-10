using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CsvHelper;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CBSAErrorCode
{
	public class DataReader
	{
		public DataReader(string filePath, string exportFilePath, DateTime publicationTime)
		{
			this.filePath = filePath;
			this.exportFilePath = exportFilePath;
			this.publicationTime = publicationTime;
			RefCusCodeLists = new List<RefCusCodeList>();
		}

		public bool ReadCSVAndExportXML()
		{
			GetRefCusCodeLists();
			if (RefCusCodeLists.Count > 0)
			{
				ExportToXml();
				return true;
			}
			return false;
		}

		void GetRefCusCodeLists()
		{
			using (var resourceStream = File.OpenRead(filePath))
			using (var streamReader = new StreamReader(resourceStream))
			using (var csv = new CsvReader(streamReader))
			{
				while (csv.Read())
				{
					var refCusCodeListLanguage = new RefCusCodeListLanguage()
					{
						ZXA_Description = ReplaceWhiteSpace(csv.GetField<string>(6)) + " " + ReplaceWhiteSpace(csv.GetField<string>(7))
					};
					var refCusCodeListAttribute = new RefCusCodeListAttribute()
					{
						ZZE_Value = ReplaceWhiteSpace(csv.GetField<string>(2))
					};
					var refCusCodeList = new RefCusCodeList()
					{
						ZZD_Code = ReplaceWhiteSpace(csv.GetField<string>(0)),
						ZZD_Description = ReplaceWhiteSpace(csv.GetField<string>(4)) + " " + ReplaceWhiteSpace(csv.GetField<string>(5)),
						RefCusCodeListAttributes = new RefCusCodeListAttribute[] { refCusCodeListAttribute },
						RefCusCodeListLanguages = new RefCusCodeListLanguage[] { refCusCodeListLanguage }
				};
					RefCusCodeLists.Add(refCusCodeList);
				}
			}
		}

		string ReplaceWhiteSpace(string text) => Regex.Replace(text, @"\s+", " ");

		void ExportToXml()
		{
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.CodeType.CAERR);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.DefaultValues.MinDateTime);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaxDateTime);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DefaultValues.CountryCodeCanada);
			refCusCodeList.IncludeColumn(x => x.RefCusCodeListLanguages, false);
			refCusCodeList.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var refCusCodeListLanguage = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			refCusCodeListLanguage.IncludeColumn(x => x.ZXA_Description, false);
			refCusCodeListLanguage.IncludeColumnWithDefaultValue(x => x.ZXA_ZX6_NKLanguage, true, Constants.DefaultValues.FRLanguage);

			var refCusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			refCusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, true);
			refCusCodeListAttribute.IncludeColumnWithDefaultValue(x => x.ZZE_ZXE_NKName, true, Constants.DefaultValues.MessageNumber);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListAttribute);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeListLanguage);

			var writer = new XmlWriter(writerConfiguration);
			writer.SetDataSource(XMLWriterDataSource);
			writer.SetPublicationTime(publicationTime);
			writer.SetUpdateType(UpdateType.Full);

			var outputFilePath = Path.Combine(exportFilePath, publicationTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + OutputFileName);

			foreach (var code in RefCusCodeLists)
			{
				if (!string.IsNullOrEmpty(code.ZZD_Code) && !code.ZZD_Code.Contains("EDI"))
				{
					writer.PopulateData(code);
				}
			}
			writer.SaveXml(outputFilePath);
		}

		

		List<RefCusCodeList> RefCusCodeLists;
		readonly string filePath;
		string exportFilePath;
		const string XMLWriterDataSource = "CA CBSA Error Codes";
		const string OutputFileName = @"_CACBSAErrorCodes.xml";
		DateTime publicationTime;
	}
}
