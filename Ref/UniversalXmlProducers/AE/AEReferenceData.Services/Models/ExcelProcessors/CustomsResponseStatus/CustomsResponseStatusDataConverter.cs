using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public class CustomsResponseStatusDataConverter : IDataConverter<CustomsResponseStatus, RefCusCodeList>
{
	public string SheetName => Constants.SheetNames.DeclarationStatus;

	public List<CustomsResponseStatus> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<CustomsResponseStatus>();
		foreach (var row in rows)
		{
			var csta = new CustomsResponseStatus();
			foreach (var cell in row)
			{
				switch (cell.columnName)
				{
					case DeclarationStatusColumn:
						csta.DeclarationStatus = cell.value;
						break;
					case DeclarationStatusNameColumn:
						csta.DeclarationStatusName = cell.value;
						break;
				}
			}
			result.Add(csta);
		}
		return result;
	}

	public List<string> GetExcelColumns() => ExcelColumns;

	public List<RefCusCodeList> ConvertToRefModels(List<CustomsResponseStatus> data)
	{
		var refModels = new List<RefCusCodeList>();
		foreach (var csta in data)
		{
			refModels.Add(new RefCusCodeList
			{
				ZZD_Code = csta.DeclarationStatus,
				ZZD_Description = csta.DeclarationStatusName,
			});
		}
		return refModels;
	}

	const string DeclarationStatusColumn = "Declaration Status";
	const string DeclarationStatusNameColumn = "Declaration Status Name";
	List<string> ExcelColumns = new List<string> { DeclarationStatusColumn, DeclarationStatusNameColumn };
}
