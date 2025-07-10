using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	sealed class JobComInvoiceLineTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var tax = declaration.Invoices.AddNew().InvoiceLines.AddNew().Taxes.AddNew();
			var lookups = tax.Lookups;
			var collection = (CodeDescriptionPairList)lookups.TypeList;

			AssertEquals(1, collection.Count);
			AssertEquals("40", collection.CodesAsString);
		}

		public void TestRateOverrideReasonList()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			AssertEquals("ADD, OVR", invoiceLineTax.Lookups.RateOverrideList.CodesAsString);
		}

		public void TestMethodOfPaymentList()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(MethodOfPaymentList.Codes.B, MethodOfPaymentList.Descriptions.B),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.C, MethodOfPaymentList.Descriptions.C),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.D, MethodOfPaymentList.Descriptions.D),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.E, MethodOfPaymentList.Descriptions.E),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.G, MethodOfPaymentList.Descriptions.G),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.J, MethodOfPaymentList.Descriptions.J),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.L, MethodOfPaymentList.Descriptions.L),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.M, MethodOfPaymentList.Descriptions.M),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.N, MethodOfPaymentList.Descriptions.N),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.P, MethodOfPaymentList.Descriptions.P),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.R, MethodOfPaymentList.Descriptions.R),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.Y, MethodOfPaymentList.Descriptions.Y),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.Z, MethodOfPaymentList.Descriptions.Z),
			}, invoiceLineTax.Lookups.MOPList);
		}

		public void TestMethodOfCalculationList()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(MethodOfCalculationList.Codes.CW1, MethodOfCalculationList.Descriptions.CW1),
				new CodeDescriptionPair(MethodOfCalculationList.Codes.Gumruk, MethodOfCalculationList.Descriptions.Gumruk),
			}, invoiceLineTax.Lookups.MethodOfCalculationList);
		}
	}
}
