using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(InvoiceHeaderConfiguration))]
sealed class InvoiceHeaderConfigurationTest : EU.Business.Testing.InvoiceHeaderConfigurationAbstractTest<InvoiceHeaderConfiguration>
{
	public override void TestAdditionalInfosSupport()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Enabled when business object is not IImportExport", true, configuration.AdditionalInfosSupport(Factory.New<DummyBusinessObject>()));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.AdditionalInfosSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.AdditionalInfosSupport(declaration));
		});
	}

	public override void TestSupportingDocumentsSupport()
	{
		CombineAssertions(() =>
		{
			Assert("Enabled when business object is not IImportExport", configuration.SupportingDocumentsSupport(Factory.New<DummyBusinessObject>()));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			Assert("Enabled for export", configuration.SupportingDocumentsSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			Assert("Enabled for import", configuration.SupportingDocumentsSupport(declaration));
		});
	}

	public override void TestPreviousDocumentsSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.PreviousDocumentsSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.PreviousDocumentsSupport(declaration));
		});
	}

	public override void TestTaxSupport()
	{
		AssertEquals(true, configuration.TaxSupport(declaration));
	}

	public override void TestValueIndicatorsSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.ValueIndicatorsSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.ValueIndicatorsSupport(declaration));
		});
	}

	public override void TestAgreedPlaceCodeSupport()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Disabled when business object is not IImportExport", false, configuration.AgreedPlaceCodeSupport(Factory.New<DummyBusinessObject>()));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.AgreedPlaceCodeSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.AgreedPlaceCodeSupport(declaration));
		});
	}

	public void TestGetValidationDecider()
	{
		var invoiceHeader = declaration.Invoices.AddNew();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertNull("Export", configuration.GetValidationDecider(invoiceHeader));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<UCC6ImportInvoiceHeaderValidationDecider>("Import", configuration.GetValidationDecider(invoiceHeader));
		});
	}

	public override void TestInvoicePaymentSupport()
	{
		AssertEquals(false, configuration.InvoicePaymentSupport(declaration));
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
