using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderConfiguration))]
	class InvoiceHeaderConfigurationTest : EU.Business.Testing.InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
	{
		public override void TestInvoicePaymentSupport()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("IMP", true, configuration.InvoicePaymentSupport(declaration));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("EXP", false, configuration.InvoicePaymentSupport(declaration));
			});
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
			AssertEquals(true, configuration.TaxSupport(declaration));
		}

		public override void TestValueIndicatorsSupport()
		{
			AssertEquals(false, configuration.ValueIndicatorsSupport(declaration));
		}

		public override void TestAgreedPlaceCodeSupport()
		{
			AssertEquals(false, configuration.AgreedPlaceCodeSupport(declaration));
		}

		public override void TestExportCostCalculationsTotalsUISupport()
		{
			AssertEquals(false, configuration.ExportCostCalculationsTotalsUISupport);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
