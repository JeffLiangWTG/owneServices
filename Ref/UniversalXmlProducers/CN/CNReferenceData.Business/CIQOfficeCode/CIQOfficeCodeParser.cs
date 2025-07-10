using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public static class CIQOfficeCodeParser
	{
		public static void ExportToXMLFile(string inputFileName, string outputFileName, string dataSource, string codeType, DateTime publicationTime)
		{
			var refCusCodeLists = GetRefCusCodeLists(inputFileName);
			Helper.ExportToXMLFile(outputFileName, dataSource, codeType, publicationTime, refCusCodeLists);
		}

		public static List<RefCusCodeList> GetRefCusCodeLists(string inputFileName, int startRowIndex = 2, int codeColumnIndex = 1, int descriptionColumnIndex = 2)
		{
			Argument.NotNullOrEmpty(inputFileName, nameof(inputFileName));

			var inputXlsFile = File.OpenRead(inputFileName);

			return GetRefCusCodeLists(inputXlsFile, startRowIndex, codeColumnIndex, descriptionColumnIndex);
		}

		public static List<RefCusCodeList> GetRefCusCodeLists(Stream stream, int startRowIndex = 2, int codeColumnIndex = 1, int descriptionColumnIndex = 2)
		{
			var result = new List<RefCusCodeList>();

			var xlsFile = new XlsFile(stream, false);
			xlsFile.SetSheetSelected(1, true);
			var rowCount = xlsFile.GetRowCount(xlsFile.ActiveSheet);

			for (var rowId = startRowIndex; rowId <= rowCount; rowId++)
			{
				var code = xlsFile.GetCellValue(rowId, codeColumnIndex)?.ToString().Trim();
				var description = xlsFile.GetCellValue(rowId, descriptionColumnIndex)?.ToString().Trim();
				if (!string.IsNullOrEmpty(code))
				{
					var refCusCodeList = new RefCusCodeList()
					{
						ZZD_Code = code,
						ZZD_Description = description
					};
					result.Add(refCusCodeList);
				}
			}
			return result;
		}
	}
}
