using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFeeCollection))]
class CusEntryLineFeeCollectionTest : EU.Business.Declaration.Testing.CusEntryLineFeeCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		return new CusEntryLineFeeCollection(entryLine, Factory);
	}

	public void TestCollectionSort()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();

		AddFee(TaxTypeList.Codes.ExciseTax, entryLine);
		AddFee(TaxTypeList.Codes.ArrearsInterestVAT, entryLine);
		AddFee(TaxTypeList.Codes.GuaranteedCharges, entryLine);
		AddFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty, entryLine);
		AddFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, entryLine);
		AddFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty, entryLine);
		AddFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge, entryLine);
		AddFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, entryLine);
		AddFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat, entryLine);
		AddFee(TaxTypeList.Codes.AdditionalDutiesSecurity, entryLine);
		AddFee(TaxTypeList.Codes.Additional1P1TaxDuties, entryLine);
		AddFee(TaxTypeList.Codes.ExportTaxes, entryLine);
		AddFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, entryLine);
		AddFee(TaxTypeList.Codes.ECImportTaxes, entryLine);
		AddFee(TaxTypeList.Codes.CompensatoryInterest, entryLine);
		AddFee(TaxTypeList.Codes.ArrearsInterest, entryLine);
		AddFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, entryLine);
		AddFee(TaxTypeList.Codes.AgriculturalProductsExportTaxes, entryLine);
		AddFee(TaxTypeList.Codes.ImportTaxBefore113, entryLine);
		AddFee(TaxTypeList.Codes.OtherCountriesDuties, entryLine);

		entryLine.Fees.Load();

		CombineAssertions(() =>
		{
			var allCodes = entryLine.Fees.AllLineFees.Select(x => x.CF_ChargeType);
			var resultString = string.Join(",", allCodes);
			AssertEquals("Ascending", "A00,A20,A30,A35,A40,A45,1A1,B00,B10,B20,1P1,1S1,1T1,C00,C10,D00,D10,E00,1R1,1Z1", resultString);
			entryLine.Fees.Sort(nameof(CusEntryLineFee.CF_ChargeType), ListSortDirection.Descending);
			allCodes = entryLine.Fees.AllLineFees.Select(x => x.CF_ChargeType);
			resultString = string.Join(",", allCodes);
			AssertEquals("Descending", "1Z1,1R1,E00,D10,D00,C10,C00,1T1,1S1,1P1,B20,B10,B00,1A1,A45,A40,A35,A30,A20,A00", resultString);
		});
	}

	void AddFee(string chargeType, CusEntryLine entryLine)
	{
		var fee = Factory.New<CusEntryLineFee>();
		fee.CF_ChargeType = chargeType;
		fee.CF_CL = entryLine.PK;
	}
}
