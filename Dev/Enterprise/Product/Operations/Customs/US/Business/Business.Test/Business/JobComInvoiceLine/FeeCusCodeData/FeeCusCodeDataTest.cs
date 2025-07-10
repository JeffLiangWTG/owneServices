using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FeeCusCodeData))]
	public class FeeCusCodeDataTest : Customs.Business.Testing.CusCodeDataTest<FeeCusCodeData>
	{
		public void TestIFee()
		{
			FeeCusCodeData feeCode = Factory.New<FeeCusCodeData>();

			IFee fee = feeCode;

			fee.Code = "AAA";
			AssertEquals("AAA", fee.Code);

			fee.Amount = 432098m;
			AssertEquals(432098m, fee.Amount);

			feeCode.CY_IsOverridden = true;
			AssertEquals(true, fee.IsOverridden);

			fee.Delete();
			AssertEquals(true, feeCode.IsDeleted);
		}

		public void TestValidation()
		{
			FeeCusCodeData feeCode = Factory.New<FeeCusCodeData>();
			AssertEquals("Validation", typeof(FeeCusCodeDataValidation), feeCode.Validation.GetType());
		}

		public void TestLookups()
		{
			FeeCusCodeData feeCode = Factory.New<FeeCusCodeData>();
			AssertEquals("Validation", typeof(FeeCusCodeDataLookups), feeCode.Lookups.GetType());
		}

		public void TestDefault()
		{
			FeeCusCodeData cusCodeData = Factory.New<FeeCusCodeData>();
			AssertEquals(CusCodeDataTypeList.Codes.Fee, cusCodeData.CY_Type);
		}

		public void TestCY_SelectedRateTypeInfoReadOnly()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate.UD_TaxFeeSpecificRate = 0.89817800m;
			dutyRate.UD_TaxFeeAdvalorem = 0.87176100m;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			FeeCusCodeData fee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Wines);
			fee.CY_IsOverridden = true;
			AssertNull("TariffDutyRate", fee.TariffDutyRate);
			AssertEquals("fee.CY_SelectedRateTypeInfo.ReadOnly", true, fee.CY_SelectedRateTypeInfo.ReadOnly);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("TariffDutyRate.IsSpecificSpecificTaxFee", true, fee.TariffDutyRate.IsSpecificSpecificTaxFee);
			AssertEquals("fee.CY_SelectedRateTypeInfo.ReadOnly", false, fee.CY_SelectedRateTypeInfo.ReadOnly);
			AssertEquals("fee.CY_SelectedRateInfo.ReadOnly", true, fee.CY_SelectedRateInfo.ReadOnly);
			fee.CY_SelectedRateType = RateTypeList.Codes.Primary;
			AssertEquals("fee.CY_SelectedRate", "0.89817800", fee.CY_SelectedRate);
			fee.CY_SelectedRateType = RateTypeList.Codes.Secondary;
			AssertEquals("fee.CY_SelectedRate", "0.87176100", fee.CY_SelectedRate);
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			AssertEquals("fee.CY_SelectedRateTypeInfo.ReadOnly", true, fee.CY_SelectedRateTypeInfo.ReadOnly);

			invoiceLine.US_TaxApply = ZString.Empty;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificPyrotechnics;
			AssertEquals("TariffDutyRate.IsSpecificSpecificTaxFee", false, fee.TariffDutyRate.IsSpecificSpecificTaxFee);
			AssertEquals("fee.CY_SelectedRateTypeInfo.ReadOnly", true, fee.CY_SelectedRateTypeInfo.ReadOnly);
		}

		public void TestProperties()
		{
			FeeCusCodeData cusCodeData = Factory.New<FeeCusCodeData>();
			AssertEquals("", cusCodeData.CY_Data);
			AssertEquals(ZDecimal.Zero, cusCodeData.CY_FeeAmount);
			AssertEquals("", cusCodeData.CY_SelectedRateType);

			cusCodeData.CY_Data = "P122.23232";
			AssertEquals("P122.23232", cusCodeData.CY_Data);
			AssertEquals(122.23232m, cusCodeData.CY_FeeAmount);
			AssertEquals("P", cusCodeData.CY_SelectedRateType);

			cusCodeData.CY_FeeAmount = 43.3453473m;
			AssertEquals("P43.34535", cusCodeData.CY_Data);
			AssertEquals(43.34535m, cusCodeData.CY_FeeAmount);
			AssertEquals("P", cusCodeData.CY_SelectedRateType);

			cusCodeData.CY_SelectedRateType = "S";
			AssertEquals("S43.34535", cusCodeData.CY_Data);
			AssertEquals(43.34535m, cusCodeData.CY_FeeAmount);
			AssertEquals("S", cusCodeData.CY_SelectedRateType);
		}

		public void TestParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var cusCodeData = Factory.New<FeeCusCodeData>();
			cusCodeData.CY_ParentID = invoiceLine.PK;
			cusCodeData.CY_ParentTableCode = "JI";
			AssertEquals(invoiceLine, cusCodeData.Parent);
		}

		public void TestUpdateCY_SelectedRateTypeBasedOnDutyRateType()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeComputationCode = "";
			dutyRate.UD_TaxFeeFlag = "1";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var fee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Wines);
			fee.CY_SelectedRateType = "P";
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.Tobacco;
			AssertEquals("", fee.CY_SelectedRateType);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			fee = invoiceLine.FeeCusCodes.AddNew();
			fee.CY_SelectedRateType = "P";
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals("", fee.CY_SelectedRateType);
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			fee.CY_SelectedRateType = "P";
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.Tobacco;
			AssertEquals("", fee.CY_SelectedRateType);
			fee.CY_SelectedRateType = "P";
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.Wines;
			AssertEquals("P", fee.CY_SelectedRateType);
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.Tobacco;
			AssertEquals("", fee.CY_SelectedRateType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			return invoiceLine.FeeCusCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<FeeCusCodeData>();
		}

		protected override IEnumerable<FeeCusCodeData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<FeeCusCodeData>();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.FeeCusCodes.Add(result);

			yield return result;
		}
	}
}
