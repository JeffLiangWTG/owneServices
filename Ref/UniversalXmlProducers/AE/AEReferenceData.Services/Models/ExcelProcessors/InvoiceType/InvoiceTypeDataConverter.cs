using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class InvoiceTypeDataConverter : IDataConverter<InvoiceType, RefCusCodeList>
{
	public string SheetName => Constants.SheetNames.InvoiceType;

	public List<string> GetExcelColumns()
	{
		return ExcelColumns;
	}

	public List<InvoiceType> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<InvoiceType>();
		foreach (var row in rows)
		{
			var invoiceType = new InvoiceType();
			foreach (var (columnName, value) in row)
			{
				switch (columnName)
				{
					case CodeColumn:
						invoiceType.Code = value;
						break;
					case TypeColumn:
						invoiceType.Type = value;
						break;
				}
			}
			result.Add(invoiceType);
		}
		return result;
	}

	public List<RefCusCodeList> ConvertToRefModels(List<InvoiceType> data)
	{
		var refModels = new List<RefCusCodeList>();
		foreach (var invoiceType in data)
		{
			refModels.Add(new RefCusCodeList
			{
				ZZD_Code = invoiceType.Code,
				ZZD_Description = invoiceType.Type,
			});
		}
		return refModels;
	}

	const string CodeColumn = "Code";
	const string TypeColumn = "Invoice type";
	List<string> ExcelColumns = new List<string> { CodeColumn, TypeColumn };
}
