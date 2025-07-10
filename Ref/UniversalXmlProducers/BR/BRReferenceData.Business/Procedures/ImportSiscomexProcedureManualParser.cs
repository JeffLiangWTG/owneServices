using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class ImportSiscomexProcedureManualParser : BaseParser
	{
		public ImportSiscomexProcedureManualParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream streamDeclarationType, string outputFileName, DateTime publicationTime)
		{
			var refCusProcedureLists = GetRefCusProcedureList(streamDeclarationType);
			var writer = Helper.GetRefCusProcedureWriterConfiguration(Constants.ShipmentTypes.ImportSiscomex, "1", string.Empty, true, true);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writer, refCusProcedureLists);
		}

		static IEnumerable<RefCusProcedure> GetRefCusProcedureList(Stream streamDeclarationType)
		{
			Contract.Assume(streamDeclarationType != null);

			var xls = new XlsFile(streamDeclarationType, true);
			Contract.Assume(xls != null);

			var result = GetRefCusProcedures(xls);

			return result;
		}

		static IEnumerable<RefCusProcedure> GetRefCusProcedures(XlsFile xls)
		{
			var rowCount = xls.GetRowCount(xls.ActiveSheet);
			var result = new List<RefCusProcedure>();
			for (var rowId = 1; rowId <= rowCount; rowId++)
			{
				var refCusProcedure = new RefCusProcedure
				{
					ZZ6_ProcedureCode = xls.GetCellValue(rowId, 1)?.ToString(),
					ZZ6_Category = xls.GetCellValue(rowId, 2)?.ToString(),
					ZZ6_Description = xls.GetCellValue(rowId, 4)?.ToString().ToUpper(CultureInfo.CurrentCulture)
				};
				result.Add(refCusProcedure);
			}
			return result;
		}
	}
}
