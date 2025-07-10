using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class ECCNCodeListExcelParser : IExcelParser<ECCNCodeListData>
	{
		public IList<ECCNCodeListData> ReadXlsFile(IList<string> files)
		{
			var parser = new CommonExcelParser<ECCNCodeListData>((xls, key, rowId) =>
			{
				return new ECCNCodeListData
				{
					Code = xls.GetStr(rowId, 3),
					TariffCode = xls.GetStr(rowId, 1),
				};
			});

			return parser.ReadXlsFile(files);
		}
	}
}
