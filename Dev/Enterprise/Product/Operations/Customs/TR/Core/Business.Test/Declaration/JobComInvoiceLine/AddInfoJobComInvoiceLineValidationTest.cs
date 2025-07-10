using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_InwardProcessingLicenseLineNumber_Length8()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_InwardProcessingLicenseLineNumber = "12345678";
				AssertHasMessageError("Mixed Numerics", invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo, ExpectedInwardProcessingLicenseLineNumberMessage);

				invoiceLine.ZG_InwardProcessingLicenseLineNumber = "99999999";
				AssertNoMessageError("All 9's", invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo, ExpectedInwardProcessingLicenseLineNumberMessage);
			});
		}

		public void TestCheckZG_InwardProcessingLicenseLineNumber_Length14()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_InwardProcessingLicenseLineNumber = "A12.345.67.890";
				AssertHasMessageError("Invalid Dot notation", invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo, ExpectedInwardProcessingLicenseLineNumberMessage);

				invoiceLine.ZG_InwardProcessingLicenseLineNumber = "A1.2.34567.890";
				AssertNoMessageError("Valid Dot notation", invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo, ExpectedInwardProcessingLicenseLineNumberMessage);
			});
		}

		public void TestCheckZG_InwardProcessingLicenseLineNumber_InvalidLength()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_InwardProcessingLicenseLineNumber = "ABC1234";
				AssertHasMessageError("Shorter than 8", invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo, ExpectedInwardProcessingLicenseLineNumberMessage);

				invoiceLine.ZG_InwardProcessingLicenseLineNumber = "ABC.1234.123";
				AssertHasMessageError("Shorter than 14 with Dot notation", invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo, ExpectedInwardProcessingLicenseLineNumberMessage);

				invoiceLine.ZG_InwardProcessingLicenseLineNumber = ZString.Empty;
				AssertNoMessageError("Empty", invoiceLine.ZG_InwardProcessingLicenseLineNumberInfo, ExpectedInwardProcessingLicenseLineNumberMessage);
			});
		}

		public void TestCheckZG_UsedGoodsCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_UsedGoodsCodeInfo, "ZZ", UsedGoodsCodeList.Codes.K1);
		}

		public void TestCheckZG_ReturningGoodsReasonCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_ReturningGoodsReasonCodeInfo, "XX", "10");
		}

		public void TestCheckZG_EntryExitPurposeCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_EntryExitPurposeCodeInfo, "XX", EntryExitPurposeCodeList.Codes._01);
		}

		public void TestCheckZG_ExportUnionProductionYear()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_ExportUnionProductionYear = -2021;
				AssertHasNotifications("negative", invoiceLine.ZG_ExportUnionProductionYearInfo);
				invoiceLine.ZG_ExportUnionProductionYear = 2021;
				AssertNoNotifications("positive", invoiceLine.ZG_ExportUnionProductionYearInfo);
			});
		}

		public void TestCheckZG_ExportUnionPackCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_ExportUnionPackCodeInfo, "X", "147");
		}

		public void TestCheckZG_ExportUnionThreadCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_ExportUnionThreadCodeInfo, "X", "OK122");
		}

		public void TestCheckZG_ExportUnionAdditionalTariffCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_ExportUnionAdditionalTariffCodeInfo, "XXX", "010310001000");
		}

		public void TestCheckZG_CommercialPaymentCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_CommercialPaymentCodeInfo, "99", InvoicePaymentCodeList.Codes.IP01);
		}

		public void TestCheckZG_CommercialPaymentNumber_ProcedureCodeGroup()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_CommercialPaymentNumber = ZString.Empty;
				foreach (var procedureCodeRequiringPaymentNumber in new[] { "4000", "7100" })
				{
					invoiceLine.Parent.JI_FormattedProcedure = procedureCodeRequiringPaymentNumber;
					invoiceLine.Validation.ValidateZG_CommercialPaymentNumber();
					AssertHasMessageError($"Procedure Code: {procedureCodeRequiringPaymentNumber}", invoiceLine.ZG_CommercialPaymentNumberInfo, ExpectedCommercialPaymentNumberMessage);
				}
				invoiceLine.Parent.JI_FormattedProcedure = "1234";
				invoiceLine.Validation.ValidateZG_CommercialPaymentNumber();
				AssertNoMessageError("Non mandatory procedure code", invoiceLine.ZG_CommercialPaymentNumberInfo, ExpectedCommercialPaymentNumberMessage);
			});
		}

		public void TestCheckZG_CommercialPaymentNumber_MandatoryProcedureCodeList()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_CommercialPaymentNumber = ZString.Empty;
				foreach (var procedureCodeRequiringPaymentNumber in new[] { "6121", "6123", "6323", "6771", "5100", "5121", "5171", "5191", "5300", "5321", "5353", "5358", "5371", "5391", "5800" })
				{
					invoiceLine.Parent.JI_FormattedProcedure = procedureCodeRequiringPaymentNumber;
					invoiceLine.Validation.ValidateZG_CommercialPaymentNumber();
					AssertHasMessageError($"Procedure Code: {procedureCodeRequiringPaymentNumber}", invoiceLine.ZG_CommercialPaymentNumberInfo, ExpectedCommercialPaymentNumberMessage);
				}
				invoiceLine.Parent.JI_FormattedProcedure = "9876";
				invoiceLine.Validation.ValidateZG_CommercialPaymentNumber();
				AssertNoMessageError("Non mandatory procedure code", invoiceLine.ZG_CommercialPaymentNumberInfo, ExpectedCommercialPaymentNumberMessage);
			});
		}

		public void TestCheckZG_CommercialPaymentAmount()
		{
			invoiceLine.ZG_CommercialPaymentCode = "1";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.ZG_CommercialPaymentAmountInfo);
		}

		public void TestCheckZG_ReturnToOrigin()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader1 = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLinewithaddinfo1 = invoiceLine1.AddInfo;

			invoiceLinewithaddinfo1.ZG_ReturnToOrigin = ZBool.False;

			var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLinewithaddinfo2 = invoiceLine2.AddInfo;

			invoiceLinewithaddinfo2.ZG_ReturnToOrigin = ZBool.True;

			var invoiceLine3 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLinewithaddinfo3 = invoiceLine3.AddInfo;

			invoiceLinewithaddinfo3.ZG_ReturnToOrigin = ZBool.False;
			invoiceLinewithaddinfo1.ZG_ReturnToOrigin = ZBool.False;
			CombineAssertions("Case for one of them is true, another 2 are false", () =>
			{
				AssertHasWarning("ZG_ReturnToOrigin field should have message error", invoiceLinewithaddinfo1.ZG_ReturnToOriginInfo, necessaryMessageNotification);
				AssertHasWarning("ZG_ReturnToOrigin field should have message error", invoiceLinewithaddinfo3.ZG_ReturnToOriginInfo, necessaryMessageNotification);
			});

			AssertNoWarning("ZG_ReturnToOrigin field should not have message error", invoiceLinewithaddinfo2.ZG_ReturnToOriginInfo, necessaryMessageNotification);

			invoiceLinewithaddinfo2.ZG_ReturnToOrigin = ZBool.False;
			invoiceLinewithaddinfo1.ZG_ReturnToOrigin = ZBool.False;
			invoiceLinewithaddinfo3.ZG_ReturnToOrigin = ZBool.False;

			CombineAssertions("Case for true one becase false which means the other 2 should not have message error anymore", () =>
			{
				AssertNoWarning("ZG_ReturnToOrigin field should not have message error", invoiceLinewithaddinfo1.ZG_ReturnToOriginInfo, necessaryMessageNotification);
				AssertNoWarning("ZG_ReturnToOrigin field should not have message error", invoiceLinewithaddinfo3.ZG_ReturnToOriginInfo, necessaryMessageNotification);
			});

			invoiceLinewithaddinfo1.ZG_ReturnToOrigin = ZBool.True;
			invoiceLinewithaddinfo2.ZG_ReturnToOrigin = ZBool.False;
			invoiceLinewithaddinfo3.ZG_ReturnToOrigin = ZBool.False;

			CombineAssertions("Case for true one's place has been changed, such as the false ones so the place of errors as well", () =>
			{
				AssertHasWarning("ZG_ReturnToOrigin field should have message error", invoiceLinewithaddinfo2.ZG_ReturnToOriginInfo, necessaryMessageNotification);
				AssertHasWarning("ZG_ReturnToOrigin field should have message error", invoiceLinewithaddinfo3.ZG_ReturnToOriginInfo, necessaryMessageNotification);
			});

			AssertNoWarning("ZG_ReturnToOrigin field should not have message error", invoiceLinewithaddinfo1.ZG_ReturnToOriginInfo, necessaryMessageNotification);

			invoiceLinewithaddinfo1.ZG_ReturnToOrigin = ZBool.False;
			invoiceLinewithaddinfo2.ZG_ReturnToOrigin = ZBool.False;
			invoiceLinewithaddinfo3.ZG_ReturnToOrigin = ZBool.False;

			CombineAssertions("Case for all of them are false and there is no any true between them, which means no message errors expected", () =>
			{
				AssertNoWarning("ZG_ReturnToOrigin field should not have message error", invoiceLinewithaddinfo1.ZG_ReturnToOriginInfo, necessaryMessageNotification);
				AssertNoWarning("ZG_ReturnToOrigin field should not have message error", invoiceLinewithaddinfo2.ZG_ReturnToOriginInfo, necessaryMessageNotification);
				AssertNoWarning("ZG_ReturnToOrigin field should not have message error", invoiceLinewithaddinfo3.ZG_ReturnToOriginInfo, necessaryMessageNotification);
			});
		}

		public void TestCheckZG_PriceType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.ZG_PriceTypeInfo, "66", "01");
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyReturningGoodsReason, "10", "Kimyasal ve teknolojik olaylar (Radyasyon ve hava kirliliği gibi)", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyExportUnionPackageCodes, "TREUP");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyExportUnionPackageCodes, "147", "test Union Package Code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyThreadCodes, "TREUD");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyThreadCodes, "OK122", "test Union Thread Code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.ExportUnionAdditional);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "010310001000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			this.invoiceLine = invoiceLine.AddInfo;
		}
		AddInfoJobComInvoiceLine invoiceLine;

		const string necessaryMessageNotification = "Are you sure that these goods are not Returned to Origin? They will be merged as another entry line.";
		const string ExpectedCommercialPaymentNumberMessage = "Payment Code field is mandatory for procedure codes starting with 4 and 71, and for the procedure codes 6121, 6123, 6323, 6771, 5100, 5121, 5171, 5191, 5300, 5321, 5353, 5358, 5371, 5391, 5800.";
		const string ExpectedInwardProcessingLicenseLineNumberMessage = "Line number should be 11 alphanumeric characters in format of \"XX.X.XXXXX.XXX\" or 99999999";
	}
}
