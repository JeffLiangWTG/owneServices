using System.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusEntryLineFeeSortComparerTest : TestCaseWithFactory
{
	public void TestCompare_WrongType()
	{
		var comparer = GetComparer();
		var result = comparer.Compare(Factory.New<CusEntryLineFee>(), Factory.New<CusEntryLine>());
		AssertEquals(0, result);
	}

	public void TestCompare_FromList()
	{
		var comparer = GetComparer();
		var customsDutyOnIndustrialProductsFee = Factory.New<CusEntryLineFee>();
		var additionalDutyCountervailingSafeguardChargeVariableChargeFee = Factory.New<CusEntryLineFee>();
		var definitiveAntiDumpingDutyFee = Factory.New<CusEntryLineFee>();
		var provisionalAntiDumpingDutyFee = Factory.New<CusEntryLineFee>();
		var definitiveCountervailingDutyFee = Factory.New<CusEntryLineFee>();
		var provisionalCountervailingDutyFee = Factory.New<CusEntryLineFee>();
		var exciseTaxFee = Factory.New<CusEntryLineFee>();
		var vatFee = Factory.New<CusEntryLineFee>();
		var compensatoryInterestVatFee = Factory.New<CusEntryLineFee>();
		var arrearsInterestVATFee = Factory.New<CusEntryLineFee>();
		var additionalDutiesSecurityFee = Factory.New<CusEntryLineFee>();
		var additional1P1TaxDutiesFee = Factory.New<CusEntryLineFee>();
		var guaranteedChargesFee = Factory.New<CusEntryLineFee>();
		var exportTaxesFee = Factory.New<CusEntryLineFee>();
		var agriculturalProductsExportTaxesFee = Factory.New<CusEntryLineFee>();
		var arrearsInterestFee = Factory.New<CusEntryLineFee>();
		var compensatoryInterestFee = Factory.New<CusEntryLineFee>();
		var otherCountriesDutiesFee = Factory.New<CusEntryLineFee>();
		var importTaxesECFee = Factory.New<CusEntryLineFee>();
		var importTaxBefore113Fee = Factory.New<CusEntryLineFee>();
		customsDutyOnIndustrialProductsFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		additionalDutyCountervailingSafeguardChargeVariableChargeFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;
		definitiveAntiDumpingDutyFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
		provisionalAntiDumpingDutyFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty;
		definitiveCountervailingDutyFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty;
		provisionalCountervailingDutyFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty;
		exciseTaxFee.CF_ChargeType = TaxTypeList.Codes.ExciseTax;
		vatFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		compensatoryInterestVatFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat;
		arrearsInterestVATFee.CF_ChargeType = TaxTypeList.Codes.ArrearsInterestVAT;
		additionalDutiesSecurityFee.CF_ChargeType = TaxTypeList.Codes.AdditionalDutiesSecurity;
		additional1P1TaxDutiesFee.CF_ChargeType = TaxTypeList.Codes.Additional1P1TaxDuties;
		guaranteedChargesFee.CF_ChargeType = TaxTypeList.Codes.GuaranteedCharges;
		exportTaxesFee.CF_ChargeType = TaxTypeList.Codes.ExportTaxes;
		agriculturalProductsExportTaxesFee.CF_ChargeType = TaxTypeList.Codes.AgriculturalProductsExportTaxes;
		arrearsInterestFee.CF_ChargeType = TaxTypeList.Codes.ArrearsInterest;
		compensatoryInterestFee.CF_ChargeType = TaxTypeList.Codes.CompensatoryInterest;
		otherCountriesDutiesFee.CF_ChargeType = TaxTypeList.Codes.OtherCountriesDuties;
		importTaxesECFee.CF_ChargeType = TaxTypeList.Codes.ECImportTaxes;
		importTaxBefore113Fee.CF_ChargeType = TaxTypeList.Codes.ImportTaxBefore113;

		CombineAssertions(() =>
		{
			var result = comparer.Compare(customsDutyOnIndustrialProductsFee, additionalDutyCountervailingSafeguardChargeVariableChargeFee);
			AssertEquals("Ascending: A00 VS A20", -1, result);
			result = comparer.Compare(additionalDutyCountervailingSafeguardChargeVariableChargeFee, definitiveAntiDumpingDutyFee);
			AssertEquals("Ascending: A20 VS A30", -1, result);
			result = comparer.Compare(definitiveAntiDumpingDutyFee, provisionalAntiDumpingDutyFee);
			AssertEquals("Ascending: A30 VS A35", -1, result);
			result = comparer.Compare(provisionalAntiDumpingDutyFee, definitiveCountervailingDutyFee);
			AssertEquals("Ascending: A35 VS A40", -1, result);
			result = comparer.Compare(definitiveCountervailingDutyFee, provisionalCountervailingDutyFee);
			AssertEquals("Ascending: A40 VS A45", -1, result);
			result = comparer.Compare(provisionalCountervailingDutyFee, exciseTaxFee);
			AssertEquals("Ascending: A45 VS 1A1", -1, result);
			result = comparer.Compare(exciseTaxFee, vatFee);
			AssertEquals("Ascending: 1A1 VS B00", -1, result);
			result = comparer.Compare(vatFee, compensatoryInterestVatFee);
			AssertEquals("Ascending: B00 VS B10", -1, result);
			result = comparer.Compare(compensatoryInterestVatFee, arrearsInterestVATFee);
			AssertEquals("Ascending: B10 VS B20", -1, result);
			result = comparer.Compare(arrearsInterestVATFee, additionalDutiesSecurityFee);
			AssertEquals("Ascending: B20 VS 1P1", -1, result);
			result = comparer.Compare(additionalDutiesSecurityFee, additional1P1TaxDutiesFee);
			AssertEquals("Ascending: 1P1 VS 1S1", -1, result);
			result = comparer.Compare(additional1P1TaxDutiesFee, guaranteedChargesFee);
			AssertEquals("Ascending: 1S1 VS 1T1", -1, result);
			result = comparer.Compare(guaranteedChargesFee, exportTaxesFee);
			AssertEquals("Ascending: 1T1 VS C00", -1, result);
			result = comparer.Compare(exportTaxesFee, agriculturalProductsExportTaxesFee);
			AssertEquals("Ascending: C00 VS C10", -1, result);
			result = comparer.Compare(agriculturalProductsExportTaxesFee, arrearsInterestFee);
			AssertEquals("Ascending: C10 VS D00", -1, result);
			result = comparer.Compare(arrearsInterestFee, compensatoryInterestFee);
			AssertEquals("Ascending: D00 VS D10", -1, result);
			result = comparer.Compare(compensatoryInterestFee, otherCountriesDutiesFee);
			AssertEquals("Ascending: D10 VS E00", -1, result);
			result = comparer.Compare(otherCountriesDutiesFee, importTaxesECFee);
			AssertEquals("Ascending: E00 VS 1R1", -1, result);
			result = comparer.Compare(importTaxesECFee, importTaxBefore113Fee);
			AssertEquals("Ascending: 1R1 VS 1Z1", -1, result);

			comparer = GetComparer(ListSortDirection.Descending);

			result = comparer.Compare(customsDutyOnIndustrialProductsFee, additionalDutyCountervailingSafeguardChargeVariableChargeFee);
			AssertEquals("Descending: A00 VS A20", 1, result);
			result = comparer.Compare(additionalDutyCountervailingSafeguardChargeVariableChargeFee, definitiveAntiDumpingDutyFee);
			AssertEquals("Descending: A20 VS A30", 1, result);
			result = comparer.Compare(definitiveAntiDumpingDutyFee, provisionalAntiDumpingDutyFee);
			AssertEquals("Descending: A30 VS A35", 1, result);
			result = comparer.Compare(provisionalAntiDumpingDutyFee, definitiveCountervailingDutyFee);
			AssertEquals("Descending: A35 VS A40", 1, result);
			result = comparer.Compare(definitiveCountervailingDutyFee, provisionalCountervailingDutyFee);
			AssertEquals("Descending: A40 VS A45", 1, result);
			result = comparer.Compare(provisionalCountervailingDutyFee, exciseTaxFee);
			AssertEquals("Descending: A45 VS 1A1", 1, result);
			result = comparer.Compare(exciseTaxFee, vatFee);
			AssertEquals("Descending: 1A1 VS B00", 1, result);
			result = comparer.Compare(vatFee, compensatoryInterestVatFee);
			AssertEquals("Descending: B00 VS B10", 1, result);
			result = comparer.Compare(compensatoryInterestVatFee, arrearsInterestVATFee);
			AssertEquals("Descending: B10 VS B20", 1, result);
			result = comparer.Compare(arrearsInterestVATFee, additionalDutiesSecurityFee);
			AssertEquals("Descending: B20 VS 1P1", 1, result);
			result = comparer.Compare(additionalDutiesSecurityFee, additional1P1TaxDutiesFee);
			AssertEquals("Descending: 1P1 VS 1S1", 1, result);
			result = comparer.Compare(additional1P1TaxDutiesFee, guaranteedChargesFee);
			AssertEquals("Descending: 1S1 VS 1T1", 1, result);
			result = comparer.Compare(guaranteedChargesFee, exportTaxesFee);
			AssertEquals("Descending: 1T1 VS C00", 1, result);
			result = comparer.Compare(exportTaxesFee, agriculturalProductsExportTaxesFee);
			AssertEquals("Descending: C00 VS C10", 1, result);
			result = comparer.Compare(agriculturalProductsExportTaxesFee, arrearsInterestFee);
			AssertEquals("Descending: C10 VS D00", 1, result);
			result = comparer.Compare(arrearsInterestFee, compensatoryInterestFee);
			AssertEquals("Descending: D00 VS D10", 1, result);
			result = comparer.Compare(compensatoryInterestFee, otherCountriesDutiesFee);
			AssertEquals("Descending: D10 VS E00", 1, result);
			result = comparer.Compare(otherCountriesDutiesFee, importTaxesECFee);
			AssertEquals("Descending: E00 VS 1R1", 1, result);
			result = comparer.Compare(importTaxesECFee, importTaxBefore113Fee);
			AssertEquals("Descending: 1R1 VS 1Z1", 1, result);
		});
	}

	public void TestCompare_OneOutsideList()
	{
		var comparer = GetComparer();
		var fee1 = Factory.New<CusEntryLineFee>();
		var fee2 = Factory.New<CusEntryLineFee>();
		fee1.CF_ChargeType = "A";
		fee2.CF_ChargeType = TaxTypeList.Codes.ArrearsInterest;
		CombineAssertions(() =>
		{
			var result = comparer.Compare(fee1, fee2);
			AssertEquals("First parameter outside list", -1, result);
			result = comparer.Compare(fee2, fee1);
			AssertEquals("Second parameter outside list", 1, result);
		});
	}

	public void TestCompare_BothOutsideList()
	{
		var comparer = GetComparer();
		var fee1 = Factory.New<CusEntryLineFee>();
		var fee2 = Factory.New<CusEntryLineFee>();
		fee1.CF_ChargeType = "AAA";
		fee2.CF_ChargeType = "AAB";
		CombineAssertions(() =>
		{
			var result = comparer.Compare(fee1, fee2);
			AssertEquals("Ascending", -1, result);
			comparer = GetComparer(ListSortDirection.Descending);
			result = comparer.Compare(fee1, fee2);
			AssertEquals("Descending", 1, result);
		});
	}

	CusEntryLineFeeChargeTypeComparer GetComparer(ListSortDirection direction = ListSortDirection.Ascending)
	{
		return new CusEntryLineFeeChargeTypeComparer(direction);
	}
}
