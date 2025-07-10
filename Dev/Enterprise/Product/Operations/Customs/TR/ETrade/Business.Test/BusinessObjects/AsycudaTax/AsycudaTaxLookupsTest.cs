using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxCodeList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaTax = bill.AsycudaTaxes.AddNew();
			var taxCodeList = asycudaTax.Lookups.ChargeTypeList;
			var list = new CodeDescriptionPairList();
			list.AddPair(TaxCodeList.Codes.CustomsDuty, TaxCodeList.Descriptions.CustomsDuty);
			list.AddPair(TaxCodeList.Codes.VATValueAddTax, TaxCodeList.Descriptions.VATValueAddTax);
			list.AddPair(TaxCodeList.Codes.TRTBandrol, TaxCodeList.Descriptions.TRTBandrol);
			list.AddPair(TaxCodeList.Codes.StampTax, TaxCodeList.Descriptions.StampTax);
			AssertEquals(taxCodeList, list);
		}

		public void TestMethodOfPaymentList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaTax = bill.AsycudaTaxes.AddNew();
			var methodofpaymentList = asycudaTax.Lookups.MethodOfPaymentList;
			var list = new CodeDescriptionPairList();
			list.AddPair(MethodOfPaymentList.Codes.Cash, MethodOfPaymentList.Descriptions.Cash);
			list.AddPair(MethodOfPaymentList.Codes.Defered, MethodOfPaymentList.Descriptions.Defered);
			list.AddPair(MethodOfPaymentList.Codes.Guarantee, MethodOfPaymentList.Descriptions.Guarantee);
			AssertEquals(methodofpaymentList, list);
		}
	}
}
