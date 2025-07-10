using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(DeclarationConfiguration))]
sealed class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EntryHeaderConfiguration, EntryLineConfiguration>
{
	public void TestUseEucdmSupportingDocumentGoodsShipmentAndItem()
	{
		AssertEquals(true, configuration.UseEucdmSupportingDocumentGoodsShipmentAndItem);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;

	public override void TestUCCAdditionalInfosSupport()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Disabled for import", false, configuration.UCCAdditionalInfosSupport(declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.UCCAdditionalInfosSupport(declaration));
		});
	}

	public override void TestMiscAdditionalInfosSupport() => AssertEquals(false, configuration.MiscAdditionalInfosSupport(declaration));

	public override void TestMiscSupportingDocumentsSupport() => AssertEquals(false, configuration.MiscSupportingDocumentsSupport(declaration));

	public override void TestMiscPreviousDocumentsSupport() => AssertEquals(false, configuration.MiscPreviousDocumentsSupport(declaration));

	public override void TestMiscGuaranteesSupport() => AssertEquals(false, configuration.LockNumberOfEntryLinesForRegisteredEntry);

	public override void TestDV1DetailsSupport() => AssertEquals(true, configuration.DV1DetailsSupport(declaration));

	public override void TestUseUniversalFeeCalculation() => AssertEquals(true, configuration.UseUniversalFeeCalculation(declaration));

	public override void TestLockNumberOfEntryLinesForRegisteredEntry() => AssertEquals(false, configuration.LockNumberOfEntryLinesForRegisteredEntry);

	public override void TestIsUCC5() => AssertEquals(false, configuration.IsUCC5(Factory.New<DummyBusinessObject>()));

	public override void TestIsUCC6()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Disabled when business object is not JobDeclaration", false, configuration.IsUCC6(Factory.New<DummyBusinessObject>()));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Enabled for export", true, configuration.IsUCC6(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Enabled for import", true, configuration.IsUCC6(declaration));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Disabled for other", false, configuration.IsUCC6(declaration));
		});
	}

	public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice() => AssertEquals(false, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);

	public override void TestShouldCheckLegalByDeclarantType()
	{
		AssertEquals(false, configuration.ShouldCheckLegalByDeclarantType);
	}

	public void TestIsPopulateAuthorisationsForOfficeOfPresentationEnabledCore()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", true, configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import", false, configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration));
		});
	}

	public void TestGetUCC6CustomsOfficeValidationDecider()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<UCC6ExportCustomsOfficeValidationDecider>("Export", configuration.GetCustomsOfficeValidationDecider(declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<UCC6CustomsOfficeValidationDecider>("Import", configuration.GetCustomsOfficeValidationDecider(declaration));
		});
	}

	public void TestGetValidationDecider()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertType<UCC6ImportDeclarationValidationDecider>("Import", configuration.GetValidationDecider(declaration));
	}

	public void TestInvoiceLinePackageValidationDecider()
	{
		AssertType<InvoiceLinePackageValidationDecider>(configuration.GetInvoiceLinePackageValidationDecider());
	}
}
