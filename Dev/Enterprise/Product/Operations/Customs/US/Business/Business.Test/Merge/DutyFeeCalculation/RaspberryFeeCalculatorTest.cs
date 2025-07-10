using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RaspberryFeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2014, 10, 16)]
		public void TestFee()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0811202025";
			importTariff.UE_Unit1 = "KG";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.Today;

			var dutyRate = importTariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Raspberry;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.022m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0811202025";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(22m, invoiceLine.CusEntryLine.RaspberryAmount);

			invoiceLine.US_SupTariff = "9802004020";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(22m, invoiceLine.CusEntryLine.RaspberryAmount);

			var feeExmpt = invoiceLine.LicenceAndPermits.AddNew();
			feeExmpt.CY_Code = LicencePermitTypeList.Codes._23;
			feeExmpt.CY_Data = "RASP0012";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, invoiceLine.CusEntryLine.RaspberryAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
