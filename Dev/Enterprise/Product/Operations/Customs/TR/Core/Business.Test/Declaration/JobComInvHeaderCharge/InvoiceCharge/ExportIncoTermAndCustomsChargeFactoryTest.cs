using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class ExportIncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllCharges()
		{
			AssertEquals("There should be 19 charges", 19, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public void TestChargeParentTypes()
		{
			var charges = incoTermAndChargeFactory.GetAllCharges();
			foreach (var charge in charges)
			{
				AssertEquals(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} charge code should be available for Group Inovice, Invoice, and Invoice Line", charge.Code), ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, charge.ParentTypes);
			}
		}

		JobDeclaration testDec;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		}

		public override void TestGetCharge()
		{
			AssertCharge(TRIncotermChargeCodeList.Codes.LBC, ChargeProvider.LocalBankCharge);
			AssertCharge(TRIncotermChargeCodeList.Codes.LSC, ChargeProvider.LocalStorageCharge);
			AssertCharge(TRIncotermChargeCodeList.Codes.LDC, ChargeProvider.LocalDischargeCharge);
			AssertCharge(TRIncotermChargeCodeList.Codes.LPC, ChargeProvider.LocalPortCharge);
			AssertCharge(TRIncotermChargeCodeList.Codes.LocalCultureCharge, ChargeProvider.LocalCultureCharge);
			AssertCharge(TRIncotermChargeCodeList.Codes.LocalResourceUtilizationSupportFundCharge, ChargeProvider.LocalResourceUtilizationSupportFund);
			AssertCharge(TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge, ChargeProvider.LocalEnvironmentCharge);
			AssertCharge(TRIncotermChargeCodeList.Codes.LOT, ChargeProvider.LocalOther);
			AssertCharge(TRIncotermChargeCodeList.Codes.LocalTotalCharges, ChargeProvider.LocalTotalCharge);
			AssertCharge(TRIncotermChargeCodeList.Codes.OFT, ChargeProvider.InternationalFreight);
			AssertCharge(TRIncotermChargeCodeList.Codes.ONS, ChargeProvider.InternationalInsurance);
			AssertCharge(TRIncotermChargeCodeList.Codes.TotalForeignCharges, ChargeProvider.TotalForeignCharges);
		}

		void AssertCharge(string chargeType, CustomsChargeCode expectedChargeCode)
		{
			var actualChargeCode = incoTermAndChargeFactory.GetCharge(chargeType);
			AssertEquals(expectedChargeCode.GetType(), actualChargeCode.GetType());
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Turkey;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\TR\Core\Business.Test\Declaration\TestFiles\TRExportIncoTermAndCustomsChargeConfiguration.csv";
	}
}


