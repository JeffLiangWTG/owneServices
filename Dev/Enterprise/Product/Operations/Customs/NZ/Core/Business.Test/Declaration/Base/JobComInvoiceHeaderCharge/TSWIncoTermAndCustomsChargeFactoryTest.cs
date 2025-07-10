namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using System.Linq;

	public class TSWIncoTermAndCustomsChargeFactoryTest : IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllCharges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			var chargeFactory = ((Common.IApportionInvoiceHolder)declaration).IncoTermAndChargeFactory;
			var chargeCodes = chargeFactory.GetAllCharges();
			AssertEquals("Should be 11 base charges", 11, chargeCodes.Length);
			AssertEquals(false, chargeCodes.Any(x => x.Code == TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge.Code));
		}

		public void TestAllChargesWhenTSWActive()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var chargeFactory = ((Common.IApportionInvoiceHolder)declaration).IncoTermAndChargeFactory;
			var chargeCodes = chargeFactory.GetAllCharges();
			AssertEquals("Should now be 11 base charges & 1 NZ specific charge", 12, chargeCodes.Length);
			AssertEquals(true, chargeCodes.Any(x => x.Code == TSWIncoTermAndCustomsChargeFactory.RoyaltiesCharge.Code));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\Declaration\Base\JobComInvoiceHeaderCharge\TestFile\TSWIncoTermAndCustomsChargeConfiguration.csv";

		protected override string GetCountryContext() => JobDeclaration.NZTSW;

		protected override Type GetCustomsChargeCodeProviderActualType() => typeof(TSWIncoTermAndCustomsChargeFactory);
	}
}
