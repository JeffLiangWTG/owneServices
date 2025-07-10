namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExportIncoTermAndCustomsChargeFactoryTest : ExportIncoTermAndCustomsChargeFactoryAbstractTest<ExportIncoTermAndCustomsChargeFactory>
	{
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\TW\Core\Business.Test\Business\JobComInvHeaderCharge\InvoiceCharge\TestFile\ExportIncoTermAndCustomsChargeConfiguration.csv";
	}
}
