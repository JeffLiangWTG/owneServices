using System.Collections.Generic;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public interface IDataConverter<T, TResult>
{
	List<string> GetExcelColumns();

	List<T> ConvertExcelColumns(List<List<(string columnName, string value)>> rows);

	List<TResult> ConvertToRefModels(List<T> data);

	string SheetName { get; }
}
