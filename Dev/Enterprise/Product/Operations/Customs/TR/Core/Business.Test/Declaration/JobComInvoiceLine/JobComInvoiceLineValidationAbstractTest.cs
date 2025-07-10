using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobComInvoiceLineValidation))]
	abstract class JobComInvoiceLineValidationAbstractTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			invoice = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			validation = GetValidation();
		}
		protected JobDeclaration jobDeclaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected JobComInvoiceLineValidation validation;

		protected abstract string MessageType { get; }

		protected abstract JobComInvoiceLineValidation GetValidation();

		protected void SetIsPreviousEntryAvailableToTrue()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure("TR", "", "22", "11", "", "des", invoiceLine.IsImport ? "IMP" : "EXP");
			procedure.ZZ6_OutOfInwardProcessing = "Y";
			invoiceLine.JI_Procedure = "2211";
		}

		public void TestCheckJI_ValuationCode()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "TRNOB");
			referenceDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness,
				"01",
				"Eşyanın İade Edilmesi - Geri Gelen Eşya",
				ZDateTime.Today.AddDays(-1),
				ZDateTime.Today.AddDays(1));

			referenceDataHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness,
				"02",
				"İade Edilen Eşyanın Değiştirilmesi",
				ZDateTime.Today.AddDays(-1),
				ZDateTime.Today.AddDays(1));

			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_ValuationCode = "01";
				var xx = invoiceLine.JI_ValuationCodeInfo;
				var yy = ListValidation.InvalidCodeMessageError.ToString();

				AssertNoMessageErrorContaining("When ValuationCode entered and valid", invoiceLine.JI_ValuationCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
				AssertNoMessageErrorContaining("When ValuationCode entered and valid", invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_ValuationCode = "";
				AssertHasMessageErrorContaining("When ValuationCode Empty", invoiceLine.JI_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_ValuationCode = "XX";
				AssertHasMessageErrorContaining("When ValuationCode is invalid", invoiceLine.JI_ValuationCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		public void TestCheckJI_Description()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "88881998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Test Description");
			Factory.Save();

			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
				AssertNoNotifications(invoiceLine.JI_DescriptionInfo);

				invoiceLine.JI_Tariff = ZString.Empty;
				AssertNoNotifications(invoiceLine.JI_DescriptionInfo);
			});

			invoiceLine.JI_PartNo = "TestTEST";
			invoiceLine.JI_Description = ZString.Empty;

			CombineAssertions(() =>
			{
				AssertNull("Part should now be null", invoiceLine.Part);
				AssertHasWarning(invoiceLine.JI_DescriptionInfo, BaseJobComInvoiceLineValidation.MandatoryForAutoCreateProduct);
			});
		}
	}
}
