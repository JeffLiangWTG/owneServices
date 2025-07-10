using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class ExitPointDataConverter : IDataConverter<ExitPoint, RefCusCodeList>
{
	public string SheetName => Constants.SheetNames.ExitPoint;

	public List<string> GetExcelColumns()
	{
		return ExcelColumns;
	}

	public List<ExitPoint> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<ExitPoint>();
		foreach (var row in rows)
		{
			var exitPoint = new ExitPoint();
			foreach (var cell in row)
			{
				switch (cell.columnName)
				{
					case LocationCodeColumn:
						exitPoint.LocationCode = cell.value;
						break;
					case LocationDescriptionColumn:
						exitPoint.LocationDescription = cell.value;
						break;
				}
			}
			result.Add(exitPoint);
		}
		return result;
	}

	public List<RefCusCodeList> ConvertToRefModels(List<ExitPoint> data)
	{
		var refModels = new List<RefCusCodeList>();
		foreach (var exitPoint in data)
		{
			refModels.Add(new RefCusCodeList
			{
				ZZD_Code = exitPoint.LocationCode,
				ZZD_Description = exitPoint.LocationDescription,
			});
		}
		return refModels;
	}

	const string LocationCodeColumn = "Customs Location Code";
	const string LocationDescriptionColumn = "Description";
	List<string> ExcelColumns = new List<string> { LocationCodeColumn, LocationDescriptionColumn };
}
