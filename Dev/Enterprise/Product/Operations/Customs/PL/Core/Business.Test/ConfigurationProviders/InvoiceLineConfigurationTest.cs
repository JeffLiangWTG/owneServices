using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(InvoiceLineConfiguration))]
sealed class InvoiceLineConfigurationTest : InvoiceLineConfigurationAbstractTest<InvoiceLineConfiguration>
{
	public override void TestAdditionalInfosSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.AdditionalInfosSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.AdditionalInfosSupport(declaration));
		});
	}

	public override void TestAuthorisationsForInvoiceLineSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.AuthorisationsSupportForInvoiceLine(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.AuthorisationsSupportForInvoiceLine(declaration));
		});
	}

	public override void TestCountryOfDestinationVisibleOnExportControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.CountryOfDestinationVisibleOnExportControl(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.CountryOfDestinationVisibleOnExportControl(declaration));
		});
	}

	public override void TestCountryOfDestinationVisibleOnImportControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.CountryOfDestinationVisibleOnImportControl(declaration));
		});
	}

	public override void TestCountryOfSupplyMustBeTheSameForAllLinesOnInstruction()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.CountryOfSupplyMustBeTheSameForAllLinesOnInstruction(declaration));
		});
	}

	public override void TestDefaultCountryOfSupplyFromSupplier()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.DefaultCountryOfSupplyFromSupplier(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.DefaultCountryOfSupplyFromSupplier(declaration));
		});
	}

	public override void TestFiscalReferencesSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.FiscalReferencesSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.FiscalReferencesSupport(declaration));
		});
	}

	public override void TestInflateItemPriceByValuationMarkup()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.InflateItemPriceByValuationMarkup(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.InflateItemPriceByValuationMarkup(declaration));
		});
	}

	public override void TestInvoiceLinePaymentSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.InvoiceLinePaymentSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.InvoiceLinePaymentSupport(declaration));
		});
	}

	public override void TestMethodOfPaymentVisibleOnImportControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.MethodOfPaymentVisibleOnImportControl(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.MethodOfPaymentVisibleOnImportControl(declaration));
		});
	}

	public override void TestOrganizationsSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.OrganizationsSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.OrganizationsSupport(declaration));
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

	public override void TestSupportingDocumentsSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.SupportingDocumentsSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.SupportingDocumentsSupport(declaration));
		});
	}

	public override void TestTaxSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.TaxSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.TaxSupport(declaration));
		});
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

	public override void TestVehicleSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.VehicleSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.VehicleSupport(declaration));
		});
	}

	public override void TestAdditionalSupplyChainActorSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Disabled for export", false, configuration.AdditionalSupplyChainActorSupport(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.AdditionalSupplyChainActorSupport(declaration));
		});
	}

	public void TestGetValidationDecider()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration
			.Invoices.AddNew()
			.InvoiceLines.AddNew();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertNull("Export", configuration.GetValidationDecider(invoiceLine));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<UCC6ImportInvoiceLineValidationDecider>("Import", configuration.GetValidationDecider(invoiceLine));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
