using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	abstract class JobComInvoiceLineValidationForInvoiceQuantityTest : BusinessObjectValidationTestCase
	{
		public void TestEmptyPropertyHasWarningWhenAppropriate()
		{
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			decWithBond.JE_MessageType = ZString.Empty;
			InfoToTest.Value = BadValue;
			((IBusinessObjectInternals)line).Validate(InfoToTest);
			AssertHasWarning(InfoToTest, Warning);
			AssertNoError(InfoToTest, Error);

			line.Declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			((IBusinessObjectInternals)line).Validate(InfoToTest);
			AssertHasError(InfoToTest, Error);
			Assert("Doesn't have warning", !InfoToTest.HasWarning(Warning));
			line.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			((IBusinessObjectInternals)line).Validate(InfoToTest);
			Assert("Doesn't have warning", !InfoToTest.HasWarning(Warning));
			AssertNoError(InfoToTest, Error);

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			decWithBond.SupportsBondedWarehousingCoreExposed = false;
			((IBusinessObjectInternals)line).Validate(InfoToTest);
			Assert("Doesn't have warning", !InfoToTest.HasWarning(Warning));
			AssertNoError(InfoToTest, Error);

			decWithBond.SupportsBondedWarehousingCoreExposed = true;
			InfoToTest.Value = CorrectValue;
			((IBusinessObjectInternals)line).Validate(InfoToTest);
			Assert("Doesn't have warning", !InfoToTest.HasWarning(Warning));
			AssertNoError(InfoToTest, Error);
		}

		protected abstract string Warning { get; }

		protected abstract string Error { get; }

		protected abstract ZPropertyInfo InfoToTest { get; }

		protected abstract IZType CorrectValue { get; }

		protected abstract IZType BadValue { get; }

		protected override void SetUp()
		{
			base.SetUp();
			decWithBond = Factory.New<DummyDeclarationWithIntegrationSupport>();
			line = decWithBond.CreateBondedLine();
		}
		DummyDeclarationWithIntegrationSupport decWithBond;
		protected BaseJobComInvoiceLine line;
	}

	class EmptyJI_InvoiceQuantityWhenWarehousingTest : JobComInvoiceLineValidationForInvoiceQuantityTest
	{
		protected override string Warning => "Please enter an invoice quantity if you would like this line to be recorded in the bonded warehousing system.";

		protected override string Error => "Please enter an invoice quantity for the bonded warehousing system.";

		protected override ZPropertyInfo InfoToTest => line.JI_InvoiceQuantityInfo;

		protected override IZType BadValue => ZDecimal.Zero;

		protected override IZType CorrectValue => new ZDecimal(1m);
	}

	class EmptyJI_InvoiceUnitOfQuantityWhenWarehousingTest : JobComInvoiceLineValidationForInvoiceQuantityTest
	{
		protected override string Warning => "Please enter an invoice unit of quantity if you would like this line to be recorded in the bonded warehousing system.";

		protected override string Error => "Please enter an invoice unit of quantity for the bonded warehousing system.";

		protected override ZPropertyInfo InfoToTest => line.JI_InvoiceUQInfo;

		protected override IZType BadValue => ZString.Empty;

		protected override IZType CorrectValue => new ZString("KG");
	}
}
