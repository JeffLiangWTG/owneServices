using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class CusProcedureDataConverter : IDataConverter<CusProcedure, RefCusProcedure>
{
	public string SheetName => Constants.SheetNames.DeclarationType;

	public List<string> GetExcelColumns()
	{
		return ExcelColumns;
	}

	public List<CusProcedure> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<CusProcedure>();
		foreach (var row in rows)
		{
			var cusProcedure = new CusProcedure();
			foreach (var cell in row)
			{
				var value = cell.value;
				switch (cell.columnName)
				{
					case ShortNameColumn:
						var prefix = value.Substring(0, 2).ToUpperInvariant();
						cusProcedure.ShipmentType = GetShipmentType(prefix);
						cusProcedure.CalculateDuty = GetCalculateDuty(prefix);
						cusProcedure.IsTransit = GetTransit(prefix);
						cusProcedure.IntoTemporaryImport = GetIntoTemporaryImport(prefix);
						cusProcedure.Category = prefix;
						cusProcedure.ProcedureCode = value;
						break;
					case DeclarationTypeColumn:
						cusProcedure.DeclarationDescription = value;
						break;
					case DeclarationTypeCodeColumn:
						cusProcedure.DeclarationTypeCode = value;
						break;
				}
			}
			result.Add(cusProcedure);
		}
		return result;
	}

	static string GetShipmentType(string prefix)
	{
		return prefix switch
		{
			"IM" => "IMP",
			"EX" => "EXP",
			"TS" => "TRS",
			"TR" => "TRF",
			"TA" => "TMP",
			"CT" => "CTF",
			_ => prefix
		};
	}

	static bool GetCalculateDuty(string prefix)
	{
		return prefix switch
		{
			"IM" => true,
			_ => false
		};
	}

	static string GetTransit(string prefix)
	{
		return prefix switch
		{
			"TR" => Constants.DefaultValues.ApplicableXmlValue,
			_ => Constants.DefaultValues.NotApplicableXmlValue
		};
	}

	static string GetIntoTemporaryImport(string prefix)
	{
		return prefix switch
		{
			"TA" => Constants.DefaultValues.ApplicableXmlValue,
			_ => Constants.DefaultValues.NotApplicableXmlValue
		};
	}

	public List<RefCusProcedure> ConvertToRefModels(List<CusProcedure> data)
	{
		var refModels = new List<RefCusProcedure>();
		foreach (var cusProcedure in data)
		{
			refModels.Add(new RefCusProcedure
			{
				ZZ6_ProcedureCode = cusProcedure.ProcedureCode,
				ZZ6_Description = cusProcedure.DeclarationDescription,
				ZZ6_ShipmentType = cusProcedure.ShipmentType,
				ZZ6_CalculateDuty = cusProcedure.CalculateDuty,
				ZZ6_IsTransit = cusProcedure.IsTransit,
				ZZ6_IntoTemporaryImport = cusProcedure.IntoTemporaryImport,
				ZZ6_Category = cusProcedure.Category,
				RefCusProcedureAttributes = GetRefCusProcedureAttributes(cusProcedure),
			});
		}
		return refModels;
	}

	static RefCusProcedureAttribute[] GetRefCusProcedureAttributes(CusProcedure cusProcedure)
	{
		return new List<RefCusProcedureAttribute>
		{
			new()
			{
				ZXB_Value = cusProcedure.DeclarationTypeCode
			}
		}.ToArray();
	}

	const string ShortNameColumn = "Short Name";
	const string DeclarationTypeColumn = "Declaration Type";
	const string DeclarationTypeCodeColumn = "Declaration Type Code";
	readonly List<string> ExcelColumns = new() { ShortNameColumn, DeclarationTypeColumn, DeclarationTypeCodeColumn };
}
