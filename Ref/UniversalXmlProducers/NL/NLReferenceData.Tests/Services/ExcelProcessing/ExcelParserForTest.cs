using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	public class ExcelParserForTest : IExcelParser<CommonExcelTestData>
	{
		public IList<CommonExcelTestData> ReadXlsFile(IList<string> files)
		{
			var parser = new CommonExcelParser<CommonExcelTestData>((xls, key, rowId) =>
			{
				return new CommonExcelTestData
				{
					Code = key,
					Description = xls.GetStr(rowId, 2),
					Kind = xls.GetStr(rowId, 3),
					Type = xls.GetStr(rowId, 4),
					List = xls.GetStr(rowId, 5)
				};
			});

			return parser.ReadXlsFile(files);
		}
	}
}
