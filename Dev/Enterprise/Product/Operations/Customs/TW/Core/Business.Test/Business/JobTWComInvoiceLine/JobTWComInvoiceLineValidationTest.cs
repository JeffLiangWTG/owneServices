using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobTWComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTWL_AircraftPartsCategory()
		{
			tWComInvoiceLine.TWL_AircraftPartsCode = "1";
			tWComInvoiceLine.Validation.ValidateTWL_AircraftPartsCategory();
			AssertHasMessageErrorContaining(tWComInvoiceLine.TWL_AircraftPartsCategoryInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftPartsCodeInfo.ClearValue();
			tWComInvoiceLine.Validation.ValidateTWL_AircraftPartsCategory();
			AssertNoMessageErrorContaining(tWComInvoiceLine.TWL_AircraftPartsCategoryInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftIPC = "AB";
			tWComInvoiceLine.Validation.ValidateTWL_AircraftPartsCategory();
			AssertHasMessageErrorContaining(tWComInvoiceLine.TWL_AircraftPartsCategoryInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftIPCInfo.ClearValue();
			tWComInvoiceLine.Validation.ValidateTWL_AircraftPartsCategory();
			AssertNoMessageErrorContaining(tWComInvoiceLine.TWL_AircraftPartsCategoryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTWL_AircraftPartsCode()
		{
			tWComInvoiceLine.TWL_AircraftPartsCategory = "1";
			tWComInvoiceLine.Validation.ValidateTWL_AircraftPartsCode();
			AssertHasMessageErrorContaining(tWComInvoiceLine.TWL_AircraftPartsCodeInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftPartsCategoryInfo.ClearValue();
			tWComInvoiceLine.Validation.ValidateTWL_AircraftPartsCode();
			AssertNoMessageErrorContaining(tWComInvoiceLine.TWL_AircraftPartsCodeInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftIPC = "AB";
			tWComInvoiceLine.Validation.ValidateTWL_AircraftPartsCode();
			AssertHasMessageErrorContaining(tWComInvoiceLine.TWL_AircraftPartsCodeInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftIPCInfo.ClearValue();
			tWComInvoiceLine.Validation.ValidateTWL_AircraftPartsCode();
			AssertNoMessageErrorContaining(tWComInvoiceLine.TWL_AircraftPartsCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTWL_AircraftIPC()
		{
			tWComInvoiceLine.TWL_AircraftPartsCategory = "1";
			tWComInvoiceLine.Validation.ValidateTWL_AircraftIPC();
			AssertHasMessageErrorContaining(tWComInvoiceLine.TWL_AircraftIPCInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftPartsCategoryInfo.ClearValue();
			tWComInvoiceLine.Validation.ValidateTWL_AircraftIPC();
			AssertNoMessageErrorContaining(tWComInvoiceLine.TWL_AircraftIPCInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftPartsCode = "1";
			tWComInvoiceLine.Validation.ValidateTWL_AircraftIPC();
			AssertHasMessageErrorContaining(tWComInvoiceLine.TWL_AircraftIPCInfo, MandatoryValidation.YouHaveNotEntered);

			tWComInvoiceLine.TWL_AircraftPartsCodeInfo.ClearValue();
			tWComInvoiceLine.Validation.ValidateTWL_AircraftIPC();
			AssertNoMessageErrorContaining(tWComInvoiceLine.TWL_AircraftIPCInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTWL_DocumentaryQty()
		{
			var targetInfo = tWComInvoiceLine.TWL_DocumentaryQtyInfo;
			tWComInvoiceLine.TWL_DocumentaryQty = 0;
			AssertHasWarningContaining(targetInfo, "must be greater than 0");

			tWComInvoiceLine.TWL_DocumentaryQty = 1;
			AssertNoWarningContaining(targetInfo, "must be greater than 0");
		}

		public void TestCheckTWL_DocumentaryUQ()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Codes.TWCustomsPackUnits, "Taiwan Customs Pack Units");
			helper.CreateNewOrGetExistingCusCodeList("TW", Codes.TWCustomsPackUnits, "FAH", "Degree Fahrenheit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var targetInfo = tWComInvoiceLine.TWL_DocumentaryUQInfo;
			tWComInvoiceLine.TWL_DocumentaryUQ = "";
			AssertHasWarning(targetInfo, "You have not entered a Documentary Quantity Unit.");
			AssertNoWarningContaining(targetInfo, ListValidation.InvalidCodeMessage);

			tWComInvoiceLine.TWL_DocumentaryUQ = "PKK";
			AssertNoWarning(targetInfo, "You have not entered a Documentary Quantity Unit.");
			AssertHasWarningContaining(targetInfo, ListValidation.InvalidCodeMessage);

			tWComInvoiceLine.TWL_DocumentaryUQ = "FAH";
			AssertNoWarning(targetInfo, "You have not entered a Documentary Quantity Unit.");
			AssertNoWarningContaining(targetInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckTWL_DocumentaryUnitPrice()
		{
			var targetInfo = tWComInvoiceLine.TWL_DocumentaryUnitPriceInfo;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 0;
			AssertHasWarningContaining(targetInfo, "must be greater than 0");

			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 1;
			AssertNoWarningContaining(targetInfo, "must be greater than 0");

			var equalityWarning = "Documentary Unit Price * Documentary Quantity must equal to Line Price.";
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;

			invoiceLine.JI_LinePrice = 99.99;
			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.3m;
			AssertHasWarning("3 * 33.3 = 99.99", targetInfo, equalityWarning);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.33m;
			AssertNoWarnings("3 * 33.33 = 99.99", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.333m;
			AssertHasWarning("3 * 33.333 = 99.99", targetInfo, equalityWarning);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.3333m;
			AssertHasWarning("3 * 33.3333 = 99.99", targetInfo, equalityWarning);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.33333m;
			AssertHasWarning("3 * 33.33333 = 99.99", targetInfo, equalityWarning);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.333333m;
			AssertHasWarning("3 * 33.333333 = 99.99", targetInfo, equalityWarning);

			invoiceLine.JI_LinePrice = 22989;
			tWComInvoiceLine.TWL_DocumentaryQty = 218m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 105.4545m;
			AssertNoWarnings("218 * 105.4545 = 22989", targetInfo);

			invoiceLine.JI_LinePrice = 100;
			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.3m;
			AssertNoWarnings("3 * 33.3 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.33m;
			AssertNoWarnings("3 * 33.33 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.333m;
			AssertNoWarnings("3 * 33.333 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.3333m;
			AssertNoWarnings("3 * 33.3333 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.33333m;
			AssertNoWarnings("3 * 33.33333 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.333333m;
			AssertNoWarnings("3 * 33.333333 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 33.3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 3m;
			AssertNoWarnings("33.3 * 3 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 33.33m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 3m;
			AssertNoWarnings("33.33 * 3 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 33.333m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 3m;
			AssertNoWarnings("33.333 * 3 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 33.3333m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 3m;
			AssertNoWarnings("33.3333 * 3 = 100", targetInfo);

			tWComInvoiceLine.TWL_DocumentaryQty = 33m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 3m;
			AssertHasWarning("33 * 3 = 100", targetInfo, equalityWarning);

			invoiceLine.JI_LinePrice = 99.99;
			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.34m;
			AssertHasWarning("3 * 33.4 = 99.99", targetInfo, equalityWarning);

			tWComInvoiceLine.TWL_DocumentaryQty = 3m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 34m;
			AssertHasWarning("3 * 34 = 99.99", targetInfo, equalityWarning);

			tWComInvoiceLine.TWL_DocumentaryQty = 4m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 33.33m;
			AssertHasWarning("4 * 33.33 = 99.99", targetInfo, equalityWarning);

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			invoiceLine.JI_LinePrice = 22989;
			tWComInvoiceLine.TWL_DocumentaryQty = 218m;
			tWComInvoiceLine.TWL_DocumentaryUnitPrice = 105.4545m;
			AssertHasWarning("218 * 105.4545 = 22989 USD", targetInfo, equalityWarning);
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			tWComInvoiceLine = invoiceLine.AddInfoChild;
		}

		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		JobTWComInvoiceLine tWComInvoiceLine;
	}
}
