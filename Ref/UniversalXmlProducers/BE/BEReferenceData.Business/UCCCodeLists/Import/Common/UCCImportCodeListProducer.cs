using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public abstract class UCCImportCodeListProducer<T>
	where T : RefDataRepoModelEntityType
	{
		public abstract IUCCImportCodeListDetails CodeListDetail { get; }

		public abstract int StartingRow { get; }

		protected virtual int valueColumn => 4;
		protected virtual int descriptionENColumn => 5;
		protected virtual int descriptionFRColumn => 6;
		protected virtual int descriptionNLColumn => 7;
		protected virtual int descriptionDEColumn => 8;

		public IReadOnlyList<RefCusCodeList> ParseExcel(XlsFile xlsFile, string sheetName)
		{
			var outputCodeList = new List<RefCusCodeList>();

			var sheetIndex = -1;
			var lastRow = int.MaxValue;

			var findSheet = 1;
			while (findSheet <= xlsFile.SheetCount)
			{
				if (sheetName != null && xlsFile.GetSheetName(findSheet) == sheetName)
				{
					sheetIndex = findSheet;
					break;
				}

				findSheet++;
			}

			if (sheetIndex == -1)
			{
				return outputCodeList;
			}

			xlsFile.SetSheetSelected(sheetIndex, true);
			xlsFile.ActiveSheet = sheetIndex;

			var rowCount = xlsFile.GetRowCount(sheetIndex);

			for (var rowId = StartingRow; rowId <= rowCount && rowId <= lastRow; rowId++)
			{
				RefCusCodeList excelData;
				var value = xlsFile.GetCellValue(rowId, valueColumn)?.ToString();
				var description = string.Empty;
				var descriptionEN = XMLGeneration.ReplaceHexadecimalSymbols(xlsFile.GetCellValue(rowId, descriptionENColumn)?.ToString());
				var descriptionFR = XMLGeneration.ReplaceHexadecimalSymbols(xlsFile.GetCellValue(rowId, descriptionFRColumn)?.ToString());
				var descriptionNL = XMLGeneration.ReplaceHexadecimalSymbols(xlsFile.GetCellValue(rowId, descriptionNLColumn)?.ToString());
				var descriptionDE = XMLGeneration.ReplaceHexadecimalSymbols(xlsFile.GetCellValue(rowId, descriptionDEColumn)?.ToString());
				var columnNum = descriptionENColumn;

				while (string.IsNullOrEmpty(description) && columnNum <= descriptionDEColumn)
				{
					description = XMLGeneration.ReplaceHexadecimalSymbols(xlsFile.GetCellValue(rowId, columnNum)?.ToString());
					columnNum++;
				}

				if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(description))
				{
					continue;
				}

				excelData = new RefCusCodeList
				{
					ZZD_Code = value,
					ZZD_Description = description,
				};

				excelData.RefCusCodeListAttributes = GetAttributes();
				excelData.RefCusCodeListLanguages = GetLanguages(descriptionEN, descriptionFR, descriptionNL, descriptionDE);

				outputCodeList.Add(excelData);
			}

			return outputCodeList;
		}

		static RefCusCodeListAttribute[] GetAttributes()
		{
			return new RefCusCodeListAttribute[]
			{
				new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.AttributeNames.Level,
					ZZE_Value = Constants.AttributeValues.Header,
				},
				new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.AttributeNames.Level,
					ZZE_Value = Constants.AttributeValues.Item,
				},
				new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = Constants.AttributeNames.Reference,
					ZZE_Value = Constants.AttributeValues.Y,
				}
			};
		}

		static RefCusCodeListLanguage[] GetLanguages(string descriptionEN, string descriptionFR, string descriptionNL, string descriptionDE)
		{
			var languages = new List<RefCusCodeListLanguage>();
			var generalDescription = descriptionEN;

			if (!string.IsNullOrEmpty(generalDescription) && !string.IsNullOrEmpty(descriptionFR))
			{
				languages.Add(new RefCusCodeListLanguage
				{
					ZXA_ZX6_NKLanguage = Constants.Languages.French,
					ZXA_Description = descriptionFR
				});
			}
			else if(string.IsNullOrEmpty(generalDescription))
			{
				generalDescription = descriptionFR;
			}

			if (!string.IsNullOrEmpty(generalDescription) && !string.IsNullOrEmpty(descriptionNL))
			{
				languages.Add(new RefCusCodeListLanguage
				{
					ZXA_ZX6_NKLanguage = Constants.Languages.Dutch,
					ZXA_Description = descriptionNL
				});
			}
			else if (string.IsNullOrEmpty(generalDescription))
			{
				generalDescription = descriptionNL;
			}

			if (!string.IsNullOrEmpty(generalDescription) && !string.IsNullOrEmpty(descriptionDE))
			{
				languages.Add(new RefCusCodeListLanguage
				{
					ZXA_ZX6_NKLanguage = Constants.Languages.German,
					ZXA_Description = descriptionDE
				});
			}

			return languages.ToArray();
		}

		public void RunProcess(string outputPath, XlsFile sourceXls, string sheetName, StringBuilder errorCollector, UpdateType updateType = UpdateType.Full)
		{
			var parsedExcel = ParseExcel(sourceXls, sheetName);
			if (parsedExcel != null && parsedExcel.Count > 0)
			{
				XMLGeneration.ExportToXMLFile(
					CodeListDetail.DataSource,
					CodeListDetail.CodeType == null ? Path.Combine(outputPath, $"{typeof(T).Name}ZZ_BE.xml") :
													Path.Combine(outputPath, $"{typeof(T).Name}ZZ_BE_{CodeListDetail.CodeType}.xml"),
					CodeListDetail.XmlWriterConfiguration,
					DateTime.Now,
					parsedExcel,
					updateType);
			}
			else
			{
				errorCollector.Append("The excel file  has no code list for " + CodeListDetail.CodeType);
			}
		}
	}
}
