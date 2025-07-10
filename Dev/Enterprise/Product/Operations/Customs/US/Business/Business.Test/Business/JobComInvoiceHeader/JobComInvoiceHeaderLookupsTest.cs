
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public override void TestMessageTypes()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(typeof(Common.US.USJobMessageTypeList), parent.Lookups.MessageTypes.GetType());
		}

		public void TestInvoice()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Lookups.Invoice, parent);
		}

		public void TestJZ_IncoTerm_ListForImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceGroupHeader group1 = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoice = group1.JobComInvoiceHeaders.AddNew();
			AssertEquals(Factory.GetCachedValue<TermsOfDeliveryList>(), invoice.Lookups.JZ_IncoTerm_List);
			AssertEquals("Import IncoTerms contains FOT", true, invoice.Lookups.JZ_IncoTerm_List.ContainsCode(TermsOfDeliveryList.Codes.FOT));
		}

		public void TestJZ_IncoTerm_ListForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			JobComInvoiceGroupHeader group1 = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoice = group1.JobComInvoiceHeaders.AddNew();
			CodeDescriptionPairList baseIncoTermList = new JobComInvoiceHeaderLookups(invoice).JZ_IncoTerm_List;
			AssertEquals("US Export Inco Terms list should use base list", baseIncoTermList.Count, invoice.Lookups.JZ_IncoTerm_List.Count);
			AssertEquals("Export IncoTerms should not contain FOT", false, invoice.Lookups.JZ_IncoTerm_List.ContainsCode(TermsOfDeliveryList.Codes.FOT));
		}
	}
}
