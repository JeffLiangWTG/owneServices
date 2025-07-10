using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceLineValidation))]
	class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override JobComInvoiceLineValidation GetValidation() => new ImportJobComInvoiceLineValidation(invoiceLine);

		public void TestCheckJI_PreviousEntryNumber()
		{
			SetIsPreviousEntryAvailableToTrue();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryNumberInfo);
		}

		public void TestCheckJI_PreviousEntryLineNumber()
		{
			SetIsPreviousEntryAvailableToTrue();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_PreviousEntryLineNumberInfo);
		}

		public void TestCheckJI_ZZF_NKTaxType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var today = ZDateTime.Today;

				var testHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				var tariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
				testHelper.CreateTaxOrFee("KD1", 0.01, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "1");
				testHelper.CreateTaxOrFee("KD8", 0.08, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "8");
				Factory.Save();

				var tariff = testHelper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "0303553000", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99", taxOrFeeCode: "KD8");
				testHelper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Turkey, "KD8", startDate: ZDateTime.BrettsBirthday, endDate: ZDateTime.Today);

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "0303553000";

				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_ZZF_NKTaxTypeInfo);

				invoiceLine.JI_ZZF_NKTaxType = "XXX";
				AssertHasMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_ZZF_NKTaxType = "KD1";
				AssertHasMessageErrorContaining(invoiceLine.JI_ZZF_NKTaxTypeInfo, ListValidation.InvalidCodeMessageError);

				invoiceLine.JI_ZZF_NKTaxType = "KD8";
				AssertNoNotifications(invoiceLine.JI_ZZF_NKTaxTypeInfo);
			}
		}
	}
}
