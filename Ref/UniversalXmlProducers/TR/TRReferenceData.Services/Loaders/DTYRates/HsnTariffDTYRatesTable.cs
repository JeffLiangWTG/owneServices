using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public class HsnTariffDTYRatesTable : NPOISearchTable
	{
		public HsnTariffDTYRatesTable(ISheet sheet, ILogger logger, HashSet<string> tableHeaders, HashSet<string> searchHeaders, Func<string, string> getCellFormat)
			: base(sheet, logger, tableHeaders, searchHeaders, getCellFormat)
		{ }

		protected override bool IsValidRowCore(IRow row) => row.Count(cell => !cell.IsEmpty()) >= TableHeaders.Count - 1;
	}
}
