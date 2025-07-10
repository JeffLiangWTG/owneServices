using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class DeclarationPurposeDataConverter : IDataConverter<DeclarationPurpose, RefCusCodeList>
{
	public string SheetName => Constants.SheetNames.DeclarationPurpose;

	public List<string> GetExcelColumns()
	{
		return ExcelColumns;
	}

	public List<DeclarationPurpose> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<DeclarationPurpose>();
		foreach (var row in rows)
		{
			var declarationPurpose = new DeclarationPurpose();
			foreach (var (columnName, value) in row)
			{
				switch (columnName)
				{
					case PurposeIdColumn:
						declarationPurpose.PurposeId = value;
						break;
					case PurposeNameColumn:
						declarationPurpose.PurposeName = value;
						break;
				}
			}
			result.Add(declarationPurpose);
		}
		return result;
	}

	public List<RefCusCodeList> ConvertToRefModels(List<DeclarationPurpose> data)
	{
		var refModels = new List<RefCusCodeList>();
		foreach (var DeclarationPurpose in data)
		{
			refModels.Add(new RefCusCodeList
			{
				ZZD_Code = DeclarationPurpose.PurposeId,
				ZZD_Description = DeclarationPurpose.PurposeName,
			});
		}
		return refModels;
	}

	const string PurposeIdColumn = "Purpose_ID";
	const string PurposeNameColumn = "Purpose_Name";
	List<string> ExcelColumns = new List<string> { PurposeIdColumn, PurposeNameColumn };

}
