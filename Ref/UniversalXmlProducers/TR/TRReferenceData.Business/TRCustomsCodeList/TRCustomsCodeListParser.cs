using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
	public class TRCustomsCodeListParser
	{
		List<RefDataRepoModelEntityType> GetCodeListEntities(IEnumerable<TRCustomsRefCusCodes> data, string codeType)
		{
			var records = new List<RefDataRepoModelEntityType>();
			try
			{
				records.AddRange(GetRefCusCodeLists(data, codeType));
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GetCodeList failure, Exception: {ex.GetBaseException().Message}");
			}
			return records;
		}

		public void GenerateUniversalReferenceData(string outputPath, string refCusCodeType)
		{
			var outputFullPath = string.Empty;
			var excelFileName = string.Empty;
			var dataSource = string.Empty;
			bool specifiedCodeTypeInExcelDocument = false;
			SetProperties(outputPath, refCusCodeType, ref outputFullPath, ref excelFileName, ref dataSource, ref specifiedCodeTypeInExcelDocument);

			try
			{
				var excelConfig = TRCodeListGenerateHelper.GetRefCusCodeListExcelConfig();

				if (excelConfig.TryGetValue(refCusCodeType, out var refCusCodeListExcelConfigs))
				{
					var dataFilePath = Path.Combine(ApplicationConfig.ResPath, excelFileName);
					var data = TRCustomsCodeListLoader.LoadData(dataFilePath, refCusCodeListExcelConfigs);

					var writerConfiguration = GetXmlWriterConfiguration(refCusCodeType, specifiedCodeTypeInExcelDocument);
					Helper.ExportToXMLFile(dataSource, outputFullPath, writerConfiguration, PublicationDateTime, GetCodeListEntities(data, refCusCodeType));
				}
				else
				{
					ErrorBuilder.AppendLine($"Error, please setup excel config of code list into CargoWise.RefDbRepo.TRReferenceData.Business.TRCodeListGenerateHelper");
				}
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"GenerateTariffUniversalReferenceData failure, Exception: {ex.GetBaseException().Message}");
			}
		}

		static void SetProperties(string outputPath, string refCusCodeType, ref string outputFullPath, ref string excelFileName, ref string dataSource, ref bool specifiedCodeTypeInExcelDocument)
		{
			switch (refCusCodeType)
			{
				case Constants.RefCusCodeType.TariffAdditionalCodeListCode:
					outputFullPath = Path.Combine(outputPath, Constants.XmlFileNames.TariffAdditionalCodeList);
					excelFileName = Constants.ExcelFileNames.TariffAdditionalCodeList;
					dataSource = Constants.DataSources.TariffAdditionalCodeList;
					specifiedCodeTypeInExcelDocument = false;
					break;
				case Constants.RefCusCodeType.WarehouseCodes:
					outputFullPath = Path.Combine(outputPath, Constants.XmlFileNames.WarehouseCodes);
					excelFileName = Constants.ExcelFileNames.WarehouseCodes;
					dataSource = Constants.DataSources.WarehouseCodes;
					specifiedCodeTypeInExcelDocument = false;
					break;
				case Constants.RefCusCodeType.SupportingDocumentsCodes:
					outputFullPath = Path.Combine(outputPath, Constants.XmlFileNames.SupportingDocumentsCodes);
					excelFileName = Constants.ExcelFileNames.SupportingDocumentsCodes;
					dataSource = Constants.DataSources.SupportingDocumentsCodes;
					specifiedCodeTypeInExcelDocument = true;
					break;
				case Constants.RefCusCodeType.ExportUnionCountryCodes:
					outputFullPath = Path.Combine(outputPath, Constants.XmlFileNames.ExportUnionCountryCodes);
					excelFileName = Constants.ExcelFileNames.ExportUnionCountryCodes;
					dataSource = Constants.DataSources.ExportUnionCountryCodes;
					specifiedCodeTypeInExcelDocument = false;
					break;
				default:
					break;
			}
		}

		protected virtual DateTime PublicationDateTime => DateTime.Now;

		protected static XmlWriterConfiguration GetXmlWriterConfiguration(string codeType, bool specifiedCodeTypeInExcelDocument)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);

			codeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfig.IncludeColumn(x => x.ZZD_Description);
			codeListConfig.IncludeColumn(x => x.ZZD_StartDate);
			codeListConfig.IncludeColumn(x => x.ZZD_EndDate);

			if (specifiedCodeTypeInExcelDocument)
			{
				codeListConfig.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			}
			else
			{
				codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			}

			codeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfig);

			return writerConfiguration;
		}

		protected virtual IEnumerable<RefCusCodeList> GetRefCusCodeLists(IEnumerable<TRCustomsRefCusCodes> codeLists, string codeType)
		{
			var refCusCodeList = new List<RefCusCodeList>();
			foreach (var codeList in codeLists)
			{
				refCusCodeList.Add(new RefCusCodeList()
				{
					ZZD_ZZK_NKCodeType = codeList.CodeType,
					ZZD_Code = codeList.Code,
					ZZD_Description = codeList.Description.Replace("\r", "").Replace("\n", "").Trim(),
					ZZD_StartDate = codeList.StartDate,
					ZZD_EndDate = codeList.EndDate,
				});
			};

			return refCusCodeList;
		}

		public string ErrorMessage => ErrorBuilder.ToString();
		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
