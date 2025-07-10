using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ChargeCodePrintSequenceUserControlTest : TestCaseWithFactory
	{
		public void TestPricingTabPage_EnableQuotationDocumentsChargeGroupingSequencingAndRollup()
			=> PricingUserControlTest.TestAutoRatingAndCompanyTariffTabPage(factory: Factory, tabName: "SequenceTab", enableQuotationDocumentsChargeGroupingSequencingAndRollup: true, expectedTabShown: true);

		public void TestPricingTabPage_DisableQuotationDocumentsChargeGroupingSequencingAndRollup()
			=> PricingUserControlTest.TestAutoRatingAndCompanyTariffTabPage(factory: Factory, tabName: "SequenceTab", enableQuotationDocumentsChargeGroupingSequencingAndRollup: false, expectedTabShown: false);
	}
}
