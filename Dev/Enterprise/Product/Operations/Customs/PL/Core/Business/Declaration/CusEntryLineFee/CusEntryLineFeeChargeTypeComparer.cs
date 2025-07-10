using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CargoWise.Common;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryLineFeeChargeTypeComparer : IComparer
{
	public CusEntryLineFeeChargeTypeComparer(ListSortDirection direction)
	{
		isAscending = direction == ListSortDirection.Ascending;
	}

	readonly bool isAscending;

	readonly ReadOnlyDictionary<string, int> orderValueDictionary = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>
	{
		{ EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 1 },
		{ EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge, 2 },
		{ EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, 3 },
		{ EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty, 4 },
		{ EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, 5 },
		{ EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty, 6 },
		{ TaxTypeList.Codes.ExciseTax, 7 },
		{ EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 8 },
		{ EU.Business.UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat, 9 },
		{ TaxTypeList.Codes.ArrearsInterestVAT, 10 },
		{ TaxTypeList.Codes.AdditionalDutiesSecurity, 11 },
		{ TaxTypeList.Codes.Additional1P1TaxDuties, 12 },
		{ TaxTypeList.Codes.GuaranteedCharges, 13 },
		{ TaxTypeList.Codes.ExportTaxes, 14 },
		{ TaxTypeList.Codes.AgriculturalProductsExportTaxes, 15 },
		{ TaxTypeList.Codes.ArrearsInterest, 16 },
		{ TaxTypeList.Codes.CompensatoryInterest, 17 },
		{ TaxTypeList.Codes.OtherCountriesDuties, 18 },
		{ TaxTypeList.Codes.ECImportTaxes, 19 },
		{ TaxTypeList.Codes.ImportTaxBefore113, 20 },
	});

	public int Compare(object x, object y)
	{
		return x is CusEntryLineFee xFee && y is CusEntryLineFee yFee
			? Compare(xFee.CF_ChargeType, yFee.CF_ChargeType)
			: 0;
	}

	int Compare(string xChargeType, string yChargeType)
	{
		var xValue = orderValueDictionary.GetValueSafe(xChargeType);
		var yValue = orderValueDictionary.GetValueSafe(yChargeType);

		return xValue + yValue > 0
			? isAscending ? xValue.CompareTo(yValue) : yValue.CompareTo(xValue)
			: isAscending ? string.CompareOrdinal(xChargeType, yChargeType) : string.CompareOrdinal(yChargeType, xChargeType);
	}
}
