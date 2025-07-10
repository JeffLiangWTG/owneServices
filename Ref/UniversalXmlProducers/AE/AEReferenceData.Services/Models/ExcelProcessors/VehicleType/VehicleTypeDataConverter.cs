using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class VehicleTypeDataConverter : IDataConverter<VehicleType, RefCusCodeList>
{
	public List<string> GetExcelColumns()
	{
		return new List<string> { VehicleTypeCodeColumn, VehicleTypeNameColumn };
	}

	public List<VehicleType> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<VehicleType>();
		foreach (var row in rows)
		{
			var vehicleType = new VehicleType();
			foreach (var (columnName, value) in row)
			{
				switch (columnName)
				{
					case VehicleTypeCodeColumn:
						vehicleType.VehicleTypeCode = value;
						break;
					case VehicleTypeNameColumn:
						vehicleType.VehicleTypeDescription = value;
						break;
				}
			}
			result.Add(vehicleType);
		}
		return result;
	}

	public List<RefCusCodeList> ConvertToRefModels(List<VehicleType> data)
	{
		return data.Select(d => new RefCusCodeList
		{
			ZZD_Code = d.VehicleTypeCode,
			ZZD_Description = d.VehicleTypeDescription
		}).ToList();
	}

	public string SheetName => Constants.SheetNames.VehicleType;

	const string VehicleTypeCodeColumn = "Type code";
	const string VehicleTypeNameColumn = "Type Name";
}
