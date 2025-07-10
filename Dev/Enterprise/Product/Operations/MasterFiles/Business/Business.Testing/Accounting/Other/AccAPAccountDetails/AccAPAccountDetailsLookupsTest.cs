using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAPAccountDetailsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestA1_EPaymentReferenceType_List()
		{
			var lookups = new AccAPAccountDetailsLookups(Factory.New<AccAPAccountDetails>());
			var list = lookups.A1_EPaymentReferenceType_List;

			AssertEquals("list.Count", 3, list.Count);
			AssertEquals(true, list.ContainsCode(EPaymentReferenceTypes.FreeText));
			AssertEquals(true, list.ContainsCode(EPaymentReferenceTypes.InvoiceNumbers));
			AssertEquals(true, list.ContainsCode(EPaymentReferenceTypes.PaymentReferenceNum));
		}

		public void TestA1_PaymentMethod_List()
		{
			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.Cheque);
			AccAPAccountDetailsLookups lookups = new AccAPAccountDetailsLookups(Factory.New<AccAPAccountDetails>());
			CodeDescriptionPairList list = lookups.A1_PaymentMethod_List;
			AssertSame("List was not created twice", list, lookups.A1_PaymentMethod_List);
			AssertEquals("Lookup edit type should be APPaymentMethod", OLookUpEditType.APPaymentMethod, list.LookupEditType);
			AssertEquals("Lookup Count should be greater than 0", true, list.Count > 0);
			AssertEquals("List should contain Default as first element", "Default (Check)", list[0].Description);
		}

		public void TestAccountCurrencies()
		{
			AccAPAccountDetailsLookups lookups = new AccAPAccountDetailsLookups(Factory.New<AccAPAccountDetails>());
			RefCurrencyCollection collection = lookups.AccountCurrencies;
			AssertSame("AccountCurrencies was not lazy loaded", collection, lookups.AccountCurrencies);
		}
	}
}
