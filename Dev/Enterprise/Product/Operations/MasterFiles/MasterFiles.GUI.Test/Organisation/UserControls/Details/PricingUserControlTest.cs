using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class PricingUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestPricingTabPage_EnableQuotationDocumentsChargeGroupingSequencingAndRollup()
			=> TestAutoRatingAndCompanyTariffTabPage(factory: Factory, tabName: "PricingTab", enableQuotationDocumentsChargeGroupingSequencingAndRollup: true, expectedTabShown: true);

		[RequiresSTA]
		public void TestPricingTabPage_DisableQuotationDocumentsChargeGroupingSequencingAndRollup()
			=> TestAutoRatingAndCompanyTariffTabPage(factory: Factory, tabName: "PricingTab", enableQuotationDocumentsChargeGroupingSequencingAndRollup: false, expectedTabShown: false);

		public static void TestAutoRatingAndCompanyTariffTabPage(BusinessObjectFactory factory, string tabName, bool enableQuotationDocumentsChargeGroupingSequencingAndRollup, bool expectedTabShown)
		{
			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableQuotationDocumentsChargeGroupingSequencingAndRollup))
			using (var form = new OrgFormForTest(factory.New<OrgHeader>()))
			{
				form.Show();
				var detailsTabControl = form.DetailsControl.DetailsTabControl;
				var ratingTabPage = detailsTabControl.FindSingle<ZTabPage>("AutoRatingAndCompanyTariffTabPage");
				detailsTabControl.SelectedTab = ratingTabPage;
				var tabPage = ControlTestHelper.FindControls<ZTabPage>(detailsTabControl).SingleOrDefault(x => x.Name == tabName);

				if (expectedTabShown)
				{
					AssertNotNull(tabPage);
				}
				else
				{
					AssertNull(tabPage);
				}
			}
		}
	}
}
