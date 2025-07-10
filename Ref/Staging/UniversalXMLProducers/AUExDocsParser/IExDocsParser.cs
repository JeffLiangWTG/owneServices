using System;
using System.Collections.Generic;
using System.IO;
using ExcelParser;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser
{
	public interface IExDocsParser
	{
		IEnumerable<RowResult> Parse(string filePath);
		void ExportToXml(IEnumerable<RowResult> rowResults, string filePath, bool initialLoad = false);
	}
}
