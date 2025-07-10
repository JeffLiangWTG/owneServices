using System;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using JobComInvoiceHeader = Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESDeliveryTermsProviderTest : Customs.Business.Testing.DataProviderTestCase<AESDeliveryTermsProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null InvoiceHeader", "Value cannot be null.\r\nParameter name: invoiceHeader",
				() => new AESDeliveryTermsProvider(null));
		});
	}

	public void TestIncotermCode()
	{
		AssertEquals(JZIncoTermList.Codes.CFR, Provider.IncotermCode);
	}

	public void TestUNLocode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("GWE is unknown UNLocode", string.Empty, Provider.UNLocode);

			invoiceHeader.ZG_AgreedPlaceCode = "BEBRU";
			AssertEquals("Valid system UNLocode", "BEBRU", GetProvider().UNLocode);

			var unlocode = new RefUNLOCO.Loader(invoiceHeader.Factory)
				.Load("BEBRU");
			unlocode.RL_IsSystem = false;
			AssertEquals("Valid custom UNLocode", string.Empty, GetProvider().UNLocode);
		});
	}

	public void TestLocation()
	{
		AssertEquals("WAW", Provider.Location);
	}

	public void TestCountry()
	{
		CombineAssertions(() =>
		{
			AssertEquals("More than 2 signs", string.Empty, Provider.Country);
			invoiceHeader.ZG_AgreedPlaceCode = "PL";
			AssertEquals("2 signs", "PL", GetProvider().Country);
		});
	}

	public void TestText()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Mapped", string.Empty, Provider.Text);
			invoiceHeader.JZ_IncoTerm = JZIncoTermList.Codes.XXX;
			AssertEquals("Empty", "WAW", GetProvider().Text);
		});
	}

	protected override AESDeliveryTermsProvider GetProvider() => new AESDeliveryTermsProvider(invoiceHeader);

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_IncoTerm = JZIncoTermList.Codes.CFR;
		invoiceHeader.ZG_AgreedPlaceCode = "GWE";
		invoiceHeader.JZ_IncoTermPlace = "WAW";
	}

	JobComInvoiceHeader invoiceHeader;
}
