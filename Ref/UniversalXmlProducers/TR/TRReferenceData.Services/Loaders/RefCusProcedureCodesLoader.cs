using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public static class RefCusProcedureCodesLoader
	{
		public static IEnumerable<RefCusProcedure> LoadData(string path)
		{
			var records = new List<RefCusProcedure>();

			XlsFile xls = new XlsFile(path);
			xls.ActiveSheet = 1;

			for (int row = 2; row <= xls.RowCount; row++)
			{
				records.Add(new RefCusProcedure
				{
					ZZ6_Category = xls.GetStringFromCell(row, 1).Trim(),
					ZZ6_ProcedureCode = xls.GetStringFromCell(row, 2).Trim(),
					ZZ6_PreviousProcedureCode = xls.GetStringFromCell(row, 3).Trim(),
					ZZ6_Concession = xls.GetStringFromCell(row, 4).Trim(),
					ZZ6_Description = xls.GetStringFromCell(row, 6).Trim(),
					ZZ6_ZZZ_NKDataGrouping = xls.GetStringFromCell(row, 7).Trim(),
					ZZ6_ShipmentType = xls.GetStringFromCell(row, 8).Trim(),
					ZZ6_CalculateDuty = BooleanHelper.ParseBool(xls.GetStringFromCell(row, 9).Trim()).Value,
					ZZ6_Group =	xls.GetStringFromCell(row, 10).Trim(),
					ZZ6_LandedCost = BooleanHelper.ParseBool(xls.GetStringFromCell(row, 11).Trim()).Value,
					ZZ6_IntoWarehouse = xls.GetStringFromCell(row, 12).Trim(),
					ZZ6_OutOfWarehouse = xls.GetStringFromCell(row, 13).Trim(),
					ZZ6_StartDate = DateTimeHelper.ParseDate(xls.GetStringFromCell(row, 14).Trim()).Value,
					ZZ6_EndDate = DateTimeHelper.ParseDate(xls.GetStringFromCell(row, 15).Trim()).Value,
					ZZ6_IntoTemporaryImport = xls.GetStringFromCell(row, 16).Trim(),
					ZZ6_OutOfTemporaryImport = xls.GetStringFromCell(row, 17).Trim(),
					ZZ6_IntoTemporaryExport = xls.GetStringFromCell(row, 18).Trim(),
					ZZ6_OutOfTemporaryExport = xls.GetStringFromCell(row, 19).Trim(),
					ZZ6_IntoInwardProcessing = xls.GetStringFromCell(row, 20).Trim(),
					ZZ6_OutOfInwardProcessing = xls.GetStringFromCell(row, 21).Trim(),
					ZZ6_IntoOutwardProcessing = xls.GetStringFromCell(row, 22).Trim(),
					ZZ6_OutofOutwardProcessing = xls.GetStringFromCell(row, 23).Trim(),
					ZZ6_CalculateVAT =BooleanHelper.ParseBool(xls.GetStringFromCell(row, 24).Trim()).Value,
					ZZ6_IsGuaranteeConsumed = xls.GetStringFromCell(row, 25).Trim(),
					ZZ6_IsGuaranteeReleased = xls.GetStringFromCell(row, 26).Trim(),
					ZZ6_IsTransit = xls.GetStringFromCell(row, 27).Trim(),
					ZZ6_IntoVATWarehouse = xls.GetStringFromCell(row, 28).Trim(),
					ZZ6_OutOfVATWarehouse = xls.GetStringFromCell(row, 29).Trim(),
					RefCusProcedureLanguages = new[]
					{
						new RefCusProcedureLanguage
						{
							ZXV_Description = xls.GetStringFromCell(row, 5).Trim(),
							ZXV_ZX6_NKLanguage = Constants.CountryCodeTR
						}
					},
					RefCusProcedureAttributes = new[]
					{
						new RefCusProcedureAttribute
						{
							ZXB_Name = xls.GetStringFromCell(row, 30).Trim(),
							ZXB_Value = xls.GetStringFromCell(row, 31).Trim()
						}
					}
				});
			}
			return records;
		}
	}
}
