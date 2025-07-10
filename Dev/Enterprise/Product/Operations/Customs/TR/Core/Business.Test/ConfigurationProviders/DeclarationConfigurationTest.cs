using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(DeclarationConfiguration))]
	class DeclarationConfigurationTest : EU.Business.Testing.DeclarationConfigurationAbstractTest<DeclarationConfiguration, InstructionConfiguration, InvoiceHeaderConfiguration, InvoiceLineConfiguration, EntryHeaderConfiguration, EntryLineConfiguration>
	{
		public override void TestUCCAdditionalInfosSupport()
		{
			AssertEquals("Disabled", false, configuration.UCCAdditionalInfosSupport(declaration));
		}

		public override void TestUseUniversalFeeCalculation()
		{
			AssertEquals(true, configuration.UseUniversalFeeCalculation(declaration));
		}

		public override void TestMiscAdditionalInfosSupport()
		{
			AssertEquals(true, configuration.MiscAdditionalInfosSupport(declaration));
		}

		public override void TestMiscSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscSupportingDocumentsSupport(declaration));
		}

		public override void TestMiscPreviousDocumentsSupport()
		{
			AssertEquals(true, configuration.MiscPreviousDocumentsSupport(declaration));
		}

		public override void TestMiscGuaranteesSupport()
		{
			AssertEquals(false, configuration.MiscGuaranteesSupport(declaration));
		}

		public override void TestDV1DetailsSupport()
		{
			AssertEquals(true, configuration.DV1DetailsSupport(declaration));
		}

		public override void TestLockNumberOfEntryLinesForRegisteredEntry()
		{
			AssertEquals(false, configuration.LockNumberOfEntryLinesForRegisteredEntry);
		}

		public override void TestIsUCC5()
		{
			AssertEquals(false, configuration.IsUCC5(Factory.New<DummyBusinessObject>()));
		}

		public override void TestIsUCC6()
		{
			AssertEquals(false, configuration.IsUCC6(Factory.New<DummyBusinessObject>()));
		}

		public override void TestIsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice()
		{
			AssertEquals(false, configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice);
		}

		public override void TestShouldCheckLegalByDeclarantType()
		{
			AssertEquals(false, configuration.ShouldCheckLegalByDeclarantType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
