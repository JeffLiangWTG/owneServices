using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.AEReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.AEReferenceData.Services.AmendCancelReason;


namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class AmendCancelReasonDataConverter : IDataConverter<AmendCancelReason, RefCusCodeList>
{
	public string SheetName => Constants.SheetNames.AmendCancelReason;

	public List<string> GetExcelColumns() => ExcelColumns;

	public List<AmendCancelReason> ConvertExcelColumns(List<List<(string columnName, string value)>> rows)
	{
		var result = new List<AmendCancelReason>();
		foreach (var row in rows)
		{
			var reason = new AmendCancelReason{
				NKCodeTypeList = new List<string>()
			};
			foreach (var cell in row)
			{
				switch (cell.columnName)
				{
					case IDColumn:
						reason.ID = cell.value;
						break;
					case NameColumn:
						reason.Name = cell.value;
						break;
					case AmendColumn:
						if (IsYes(cell.value))
						{
							reason.NKCodeTypeList.Add(NKCodeType.Amend);
						}
						break;
					case CancelColumn:
						if (IsYes(cell.value))
						{
							reason.NKCodeTypeList.Add(NKCodeType.Cancel);
						}
						break;
					case TransferAmendColumn:
						if (IsYes(cell.value))
						{
							reason.NKCodeTypeList.Add(NKCodeType.TransferAmend);
						}
						break;
					case TransferCancelColumn:
						if (IsYes(cell.value))
						{
							reason.NKCodeTypeList.Add(NKCodeType.TransferCancel);
						}
						break;
				}
			}
			if (reason.NKCodeTypeList.Count > 0)
			{
				result.Add(reason);
			}
		}
		return result;

		bool IsYes(string value) => value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase);
	}

	public List<RefCusCodeList> ConvertToRefModels(List<AmendCancelReason> data)
	{
		var refModels = new List<RefCusCodeList>();
		foreach (var reason in data)
		{
			foreach (var nkCodeType in reason.NKCodeTypeList)
			{
				refModels.Add(new RefCusCodeList
				{
					ZZD_Code = reason.ID,
					ZZD_Description = reason.Name,
					ZZD_ZZK_NKCodeType = nkCodeType,
				});
			}
		}
		return refModels;
	}

	const string IDColumn = "ID";
	const string NameColumn = "NAME";
	const string AmendColumn = "Amend";
	const string CancelColumn = "Cancel";
	const string TransferAmendColumn = "TransferAmend";
	const string TransferCancelColumn = "TransferCancel";

	List<string> ExcelColumns = new List<string> { IDColumn, NameColumn, AmendColumn, CancelColumn, TransferAmendColumn, TransferCancelColumn };
}
