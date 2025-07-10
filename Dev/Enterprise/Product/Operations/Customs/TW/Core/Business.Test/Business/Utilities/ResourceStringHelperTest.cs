using System;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ResourceStringHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetTotalInternationalFreightAmountInInvoiceCurrencyResString()
		{
			AssertCaption(d => ResourceStringHelper.GetTotalInternationalFreightAmountInInvoiceCurrencyResString(d), "Freight (18)", "Freight (17)", "All freight fees paid or payable for transporting the shipment to its destination.", "All freight fees paid or payable for transporting the shipment to its destination.");
		}

		[ExpectNoExceptions]
		public void TestGetTotalInternationalInsuranceAmountInInvoiceCurrencyCaption()
		{
			AssertCaption(d => ResourceStringHelper.GetTotalInternationalInsuranceAmountInInvoiceCurrencyCaption(d), "Insurance (19)", "Insurance (18)", "The cost of insurance of goods.", "The cost of insurance of goods.");
		}

		[ExpectNoExceptions]
		public void TestGetTotalAdditionsInInvoiceCurrencyCaption()
		{
			AssertCaption(d => ResourceStringHelper.GetTotalAdditionsInInvoiceCurrencyCaption(d), "Additions (20)", "Additions (19)", "The charge not included in the invoice which should be added according to the customs valuation rules.", "The charge not included in the invoice which should be added according to the customs valuation rules.");
		}

		[ExpectNoExceptions]
		public void TestGetTotalDeductionsInInvoiceCurrencyCaption()
		{
			AssertCaption(d => ResourceStringHelper.GetTotalDeductionsInInvoiceCurrencyCaption(d), "Deductions (21)", "Deductions (20)", "The charge included in the invoice which should be deducted according to the customs valuation rules.", "The charge included in the invoice which should be deducted according to the customs valuation rules.");
		}

		[ExpectNoExceptions]
		public void TestGetTotalCustomsValueInInvoiceCurrencyCaption()
		{
			AssertCaption(d => ResourceStringHelper.GetTotalCustomsValueInInvoiceCurrencyCaption(d), "CIF (22)", "FOB (21)", "The total CIF value of this entry.", "The total FOB value of this entry.");
		}

		[ExpectNoExceptions]
		public void TestGetTotalCustomsValueInLocalCurrencyCaption()
		{
			AssertCaption(d => ResourceStringHelper.GetTotalCustomsValueInLocalCurrencyCaption(d), "CIF (TWD) (22)", "FOB (TWD) (21)", "The total CIF value (TWD) of this entry.", "The total FOB value (TWD) of this entry.");
		}

		[ExpectNoExceptions]
		void AssertCaption(Func<bool, ResourceStringData> getCaptionMethod, string expectCaptionTrue, string expectCaptionFalse, string expectFullDescriptionTrue, string expectFullDescriptionFalse)
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(getCaptionMethod(true).Caption, NUnit.Framework.Is.EqualTo(expectCaptionTrue), "caption when parameter is true");
				NUnit.Framework.Assert.That(getCaptionMethod(false).Caption, NUnit.Framework.Is.EqualTo(expectCaptionFalse), "caption when parameter is false");
				NUnit.Framework.Assert.That(getCaptionMethod(true).FullDescription, NUnit.Framework.Is.EqualTo(expectFullDescriptionTrue), "FullDescription when parameter is true");
				NUnit.Framework.Assert.That(getCaptionMethod(false).FullDescription, NUnit.Framework.Is.EqualTo(expectFullDescriptionFalse), "FullDescription when parameter is false");
			});
		}
	}
}
