using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(InvoiceLineConfiguration))]
	class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
	{
		public override void TestInflateItemPriceByValuationMarkup()
		{
			AssertEquals(false, configuration.InflateItemPriceByValuationMarkup(declaration));
		}

		public override void TestMethodOfPaymentVisibleOnImportControl()
		{
			AssertEquals(false, configuration.MethodOfPaymentVisibleOnImportControl(declaration));
		}

		public override void TestAdditionalInfosSupport()
		{
			AssertEquals(true, configuration.AdditionalInfosSupport(declaration));
		}

		public override void TestSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.SupportingDocumentsSupport(declaration));
		}

		public override void TestPreviousDocumentsSupport()
		{
			AssertEquals(true, configuration.PreviousDocumentsSupport(declaration));
		}

		public override void TestTaxSupport()
		{
			AssertEquals(false, configuration.TaxSupport(declaration));
		}

		public override void TestCountryOfDestinationVisibleOnImportControl()
		{
			AssertEquals(false, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
		}

		public override void TestCountryOfDestinationVisibleOnExportControl()
		{
			AssertEquals(true, configuration.CountryOfDestinationVisibleOnExportControl(declaration));
		}

		public override void TestFiscalReferencesSupport()
		{
			AssertEquals(false, configuration.FiscalReferencesSupport(declaration));
		}

		public override void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction()
		{
			AssertEquals(false, configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration));
		}

		public override void TestDefaultCountryOfSupplyFromSupplier()
		{
			AssertEquals(false, configuration.DefaultCountryOfSupplyFromSupplier(declaration));
		}

		public override void TestVehicleSupport()
		{
			AssertEquals(true, configuration.VehicleSupport(declaration));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		public override void TestAuthorisationsForInvoiceLineSupport()
		{
			AssertEquals(false, configuration.AuthorisationsSupportForInvoiceLine(declaration));
		}

		public override void TestOrganizationsSupport()
		{
			AssertEquals(false, configuration.OrganizationsSupport(declaration));
		}

		public override void TestInvoiceLinePaymentSupport()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("IMP", true, configuration.InvoiceLinePaymentSupport(declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("EXP", true, configuration.InvoiceLinePaymentSupport(declaration));
			});
		}

		public override void TestValueIndicatorsSupport()
		{
			AssertEquals(false, configuration.ValueIndicatorsSupport(declaration));
		}

		public override void TestAdditionalSupplyChainActorSupport()
		{
			AssertEquals(false, configuration.AdditionalSupplyChainActorSupport(declaration));
		}

		JobDeclaration declaration;
	}
}
