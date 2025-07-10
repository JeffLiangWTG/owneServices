using System.Linq;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExportExWorksIncoTermAndCustomsChargeFactoryTest : ExportIncoTermAndCustomsChargeFactoryAbstractTest<ExportExWorksIncoTermAndCustomsChargeFactory>
	{
		public override void TestGetAllCharges()
		{
			AssertEquals("There should be 8 charges", 8, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public void TestChargesIsDutiable()
		{
			var charges = incoTermAndChargeFactory.GetAllCharges();
			AssertEquals("ONS", false, charges.FirstOrDefault(x => x.Code == "ONS").IsDutiable);
			AssertEquals("OFT", false, charges.FirstOrDefault(x => x.Code == "OFT").IsDutiable);
			AssertEquals("ADD", false, charges.FirstOrDefault(x => x.Code == "ADD").IsDutiable);
			AssertEquals("DED", true, charges.FirstOrDefault(x => x.Code == "DED").IsDutiable);
			AssertEquals("FIF", true, charges.FirstOrDefault(x => x.Code == "FIF").IsDutiable);
			AssertEquals("PAC", true, charges.FirstOrDefault(x => x.Code == "PAC").IsDutiable);
			AssertEquals("LCH", false, charges.FirstOrDefault(x => x.Code == "LCH").IsDutiable);
			AssertEquals("EXW", true, charges.FirstOrDefault(x => x.Code == "EXW").IsDutiable);
		}

		#region Implementation
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\TW\Core\Business.Test\Business\JobComInvHeaderCharge\InvoiceCharge\TestFile\ExportEXWIncoTermAndCustomsChargeConfiguration.csv";

		protected override string GetCountryContext()
		{
			return base.GetCountryContext() + Common.Shared.SharedJobMessageTypeList.Codes.Export + Core.Constants.IncoTerms.ExWorks;
		}
		#endregion
	}
}
