using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public static class CustomsOfficeHelper
	{
		public static async Task<List<RefCusCodeList>> ParseExcelData(string filePath)
		{
			await using var inputXlsFile = File.OpenRead(filePath);
			var xlsFile = new XlsFile(inputXlsFile, false);
			xlsFile.SetSheetSelected(1, true);

			var result = Enumerable.Range(2, xlsFile.GetRowCount(1))
				.Select(CreateFromRow)
				.Where(FilterInvalidCode)
				.Where(FilterInvalidDescription)
				.DistinctBy(x => x.ZZD_Code);
			return result.ToList();

			RefCusCodeList CreateFromRow(int rowIdx)
			{
				const int columnE = 5;
				const int columnG = 7;
				var code = xlsFile.GetCellValue(rowIdx, columnE) as string;
				var description = xlsFile.GetCellValue(rowIdx, columnG) as string;
				return new RefCusCodeList { ZZD_Code = code, ZZD_Description = description };
			}

			bool FilterInvalidCode(RefCusCodeList codeList)
			{
				if (string.IsNullOrWhiteSpace(codeList.ZZD_Code))
					return false;

				var isAlphaNumeric = Regex.IsMatch(codeList.ZZD_Code, "^[a-zA-Z0-9]+$");
				return isAlphaNumeric;
			}

			bool FilterInvalidDescription(RefCusCodeList codeList)
			{
				return !string.IsNullOrWhiteSpace(codeList.ZZD_Description);
			}
		}
	}
}
