using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.TariffValidation;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(PermitCodeCollection))]
	public class PermitCodeCollectionTest : CodeDataPairCollectionTest<PermitCodeCollection>
	{
		public void TestDoesNotValidateInvoiceLineOnLoad()
		{
			PermitCode permitOnLine = InvoiceLine.PermitCodes.AddNew();
			InvoiceLine.JI_Tariff = TariffValidation.Testing.TariffValidatorTest.TariffCodeRequiringPermitCodes14Digit;
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);
			InvoiceLine.PermitCodes.LoadFromString("");
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);
		}

		public void TestPermitCodeCollectionOnInvoiceLineAffectsValidationOnInvoiceLine()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Precondition: Declaration.IsImport", true, Declaration.IsImport);
			InvoiceLine.JI_Tariff = TariffValidation.Testing.TariffValidatorTest.TariffCodeRequiringPermitCodes14Digit;
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			PermitCode permitOnLine = InvoiceLine.PermitCodes.AddNew();
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);
		}

		public void TestAllowDuplicates()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			PermitCodeCollection permits = new PermitCodeCollection(Factory, dummyBO.Z0_VarCharMaxInfo);
			PermitCode permit1 = permits.AddNew();
			permit1.ZO_Code = PermitCodeList.Codes.EnvironmentalProtectionAuthority;
			permit1.ZO_Data = "123";
			AssertNoErrors("Permit1 should have no errors", permit1);
			PermitCode permit2 = permits.AddNew();
			permit2.ZO_Code = PermitCodeList.Codes.EnvironmentalProtectionAuthority;
			permit2.ZO_Data = "456";
			AssertNoErrors("Permit1 should not error on duplicate", permit1);
			AssertNoErrors("Permit2 should not error on duplicate", permit2);
		}

		#region Implementation
		protected override PermitCodeCollection GetCollectionToTest()
		{
			return new PermitCodeCollection(Factory, Classification.CC_PermitCodesInfo);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PermitCode(Factory, null);
		}

		protected override CodeDataPairCollection CollectionAgainstDeclaration
		{
			get { return Declaration.PermitCodes; }
		}

		protected override CodeDataPairCollection CollectionAgainstInvoiceLine
		{
			get { return InvoiceLine.PermitCodes; }
		}
		#endregion
	}
}
