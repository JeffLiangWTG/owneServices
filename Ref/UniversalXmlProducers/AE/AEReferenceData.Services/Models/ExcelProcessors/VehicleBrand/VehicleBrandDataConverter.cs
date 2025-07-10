using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class VehicleBrandDataConverter : IDataConverter<VehicleBrand, RefCusCodeList>
{
	public List<string> GetExcelColumns()
	{
		return new List<string> { VehicleBrandCodeColumn, VehicleBrandNameColumn };
	}

	public List<VehicleBrand> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<VehicleBrand>();
		foreach (var row in rows)
		{
			var vehicleBrand = new VehicleBrand();
			foreach (var (columnName, value) in row)
			{
				switch (columnName)
				{
					case VehicleBrandCodeColumn:
						vehicleBrand.VehicleBrandCode = value;
						break;
					case VehicleBrandNameColumn:
						vehicleBrand.VehicleBrandDescription = value;
						break;
				}
			}
			result.Add(vehicleBrand);
		}
		return result;
	}

	public List<RefCusCodeList> ConvertToRefModels(List<VehicleBrand> data)
	{
		return data.Select(d => new RefCusCodeList
		{
			ZZD_Code = d.VehicleBrandCode,
			ZZD_Description = d.VehicleBrandDescription
		}).ToList();
	}

	public string SheetName => Constants.SheetNames.VehicleBrand;

	const string VehicleBrandCodeColumn = "Code";
	const string VehicleBrandNameColumn = "Vehicle Brand";
}
