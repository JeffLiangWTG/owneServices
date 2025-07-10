using System.Collections.Generic;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business
{
	public interface IExcelParser
	{
		List<T> Parse<T>(string filePath, ExcelParserConfiguration config = null, BackupMapper backupMapper = null) where T : new();
	}

	public delegate bool BackupMapper(string excelHeader, string propertyName);
}
