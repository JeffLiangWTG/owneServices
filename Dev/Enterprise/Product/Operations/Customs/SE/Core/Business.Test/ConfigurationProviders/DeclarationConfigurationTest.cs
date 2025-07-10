using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing;

[TestedType(typeof(DeclarationConfiguration))]
sealed class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EntryHeaderConfiguration, EntryLineConfiguration>
{
	public override void TestMiscAdditionalInfosSupport()
	{
		AssertEquals(expected: true, configuration.MiscAdditionalInfosSupport(declaration));
	}

	public override void TestMiscSupportingDocumentsSupport()
	{
		AssertEquals(expected: true, configuration.MiscSupportingDocumentsSupport(declaration));
	}

	public override void TestMiscPreviousDocumentsSupport()
	{
		AssertEquals(expected: true, configuration.MiscPreviousDocumentsSupport(declaration));
	}

	public override void TestMiscGuaranteesSupport()
	{
		AssertEquals(expected: false, configuration.MiscGuaranteesSupport(declaration));
	}

	public override void TestUseUniversalFeeCalculation()
	{
		AssertEquals(expected: true, configuration.UseUniversalFeeCalculation(declaration));
	}

	public override void TestDV1DetailsSupport()
	{
		AssertEquals(expected: false, configuration.DV1DetailsSupport(declaration));
	}

	public override void TestLockNumberOfEntryLinesForRegisteredEntry()
	{
		AssertEquals(expected: false, configuration.LockNumberOfEntryLinesForRegisteredEntry);
	}

	public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
	{
		AssertEquals(expected: false, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);
	}

	public override void TestUCCAdditionalInfosSupport()
	{
		AssertEquals(expected: false, configuration.UCCAdditionalInfosSupport(declaration));
	}

	public override void TestIsUCC5()
	{
		AssertEquals(expected: false, configuration.IsUCC5(Factory.New<DummyBusinessObject>()));
	}

	public override void TestIsUCC6() => CombineAssertions(() =>
	{
		AssertEquals("UCC6 disabled when business object is not JobDeclaration", expected: false, configuration.IsUCC6(Factory.New<DummyBusinessObject>()));
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("UCC6 enabled for export", expected: true, configuration.IsUCC6(declaration));
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals("UCC6 enabled for import", expected: true, configuration.IsUCC6(declaration));
	});

	public override void TestShouldCheckLegalByDeclarantType()
	{
		AssertEquals(expected: false, configuration.ShouldCheckLegalByDeclarantType);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
