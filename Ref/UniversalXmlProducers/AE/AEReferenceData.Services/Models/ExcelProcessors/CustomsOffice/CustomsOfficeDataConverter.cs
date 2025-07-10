using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class CustomsOfficeDataConverter : IDataConverter<CustomsOffice, RefCusCodeList>
{
	public string SheetName => Constants.SheetNames.CustomsLocation;

	public List<string> GetExcelColumns()
	{
		return ExcelColumns;
	}

	public List<CustomsOffice> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<CustomsOffice>();
		foreach (var row in rows)
		{
			var customsOffices = new CustomsOffice();
			foreach (var (columnName, value) in row)
			{
				switch (columnName)
				{
					case LocationCodeColumn:
						customsOffices.LocationCode = value;
						break;
					case LocationNameColumn:
						customsOffices.LocationName = value;
						break;
					case LocationArabicNameColumn:
						customsOffices.LocationArabicName = value;
						break;
					case GCCCodeColumn:
						customsOffices.GCCCode = value;
						break;
					case IsActiveColumn:
						customsOffices.IsActive = value;
						break;
				}
			}
			result.Add(customsOffices);
		}
		return result;
	}

	public List<RefCusCodeList> ConvertToRefModels(List<CustomsOffice> data)
	{
		var refModels = new List<RefCusCodeList>();
		foreach (var customsOffice in data)
		{
			refModels.Add(GetRefCusCodeList(customsOffice));
		}
		return refModels;
	}

	static RefCusCodeList GetRefCusCodeList(CustomsOffice customsOffice)
	{
		var refCusCodeList = new RefCusCodeList();

		refCusCodeList.ZZD_Code = customsOffice.LocationCode;
		refCusCodeList.ZZD_Description = customsOffice.LocationName;
		refCusCodeList.ZZD_EndDate = GetZZD_EndDate(customsOffice);
		if (!customsOffice.LocationArabicName.IsNullOrEmpty())
		{
			refCusCodeList.RefCusCodeListLanguages = GetCusCodeListLanguage(customsOffice);
		}
		if (!customsOffice.GCCCode.IsNullOrEmpty())
		{
			refCusCodeList.RefCusCodeListAttributes = GetCusCodeListAttribute(customsOffice);
		}

		return refCusCodeList;
	}

	static DateTime GetZZD_EndDate(CustomsOffice customsOffice)
	{
		if (customsOffice.IsActive.Equals("Yes", StringComparison.Ordinal))
		{
			return Constants.DefaultValues.MaxDateTime;
		}
		else
		{
			return Constants.DefaultValues.MinDateTime;
		}
	}

	 static RefCusCodeListLanguage[] GetCusCodeListLanguage(CustomsOffice customsOffice)
	{
		return new List<RefCusCodeListLanguage>
		{
			new() {
				ZXA_Description = customsOffice.LocationArabicName,
			}
		}.ToArray();
	}

	static RefCusCodeListAttribute[] GetCusCodeListAttribute(CustomsOffice customsOffice)
	{
		return new List<RefCusCodeListAttribute>
		{
			new() {
				ZZE_Value = customsOffice.GCCCode,
			}
		}.ToArray();
	}

	const string LocationCodeColumn = "Cus. Location Code";
	const string LocationNameColumn = "Location Name";
	const string LocationArabicNameColumn = "Location Arabic Name";
	const string GCCCodeColumn = "GCC Code";
	const string IsActiveColumn = "Is Active";
	readonly List<string> ExcelColumns = new() { LocationCodeColumn, LocationNameColumn, LocationArabicNameColumn, GCCCodeColumn, IsActiveColumn };
}
