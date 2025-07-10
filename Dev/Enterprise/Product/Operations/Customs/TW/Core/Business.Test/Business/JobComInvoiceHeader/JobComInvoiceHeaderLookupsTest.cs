using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			var parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Lookups.Invoice, parent);
		}

		public void TestIncoTermsCodeDescriptionPairList()
		{
			var parent = Factory.New<JobComInvoiceHeader>();
			var list = parent.Lookups.JZ_IncoTerm_List;
			var sourceList = Factory.GetCachedValue<IncoTermsCodeDescriptionPairList>();
			CombineAssertions(() =>
			{
				AssertEquals(sourceList.Count, list.Count);
				AssertSame(sourceList, list);
				AssertEquals("CIF, CFR, FOB, C&I, FAS, EXW, FCA, CPT, CIP, DAT, DAP, DDP, DPU", list.CodesAsString);
			});
		}

		public void TestNoOfPacksPackType_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Commercial Pack Units");
			helper.CreateCusCodeList("TW", "TWCIU", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var packTypeList = invoiceHeader.Lookups.NoOfPacksPackType_List;
			CombineAssertions(() =>
			{
				AssertSame(declaration.Lookups.PackingUnitTypesList, packTypeList);
				AssertEquals(1, packTypeList.Count);
				AssertEquals("AMP", packTypeList[0].Code);
				AssertEquals("Ampere", packTypeList[0].Description);
			});

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceUQList = invoiceLine.Lookups.InvoiceUQList;
			CombineAssertions(() =>
			{
				AssertEquals(1, invoiceUQList.Count);
				AssertEquals("AMP", invoiceUQList[0].Code);
				AssertEquals("Ampere", invoiceUQList[0].Description);
			});
		}

		public override void TestMessageTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = Factory.New<JobComInvoiceHeader>();
			var list = invoice.Lookups.MessageTypes;
			CombineAssertions(() =>
			{
				AssertType<TWJobMessageTypeList>(list);
				AssertEquals(declaration.Lookups.MessageTypeList.CodesAsString, list.CodesAsString);
			});
		}

		public override void TestMessageTypesContainsASN()
		{
			Assert("Only support EXP and IMP", true);
		}

		public void TestRelationshipIndicatorList()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var lookups = new JobComInvoiceHeaderLookups(invoiceHeader);
			CombineAssertions(() =>
			{
				var relatedIndicatorList = lookups.RelatedIndicatorList;
				AssertEquals("N,Y,E,A", string.Join(",", relatedIndicatorList.Cast<ICodeDescription>().Select(x => x.Code)));
				AssertSame("Cached", Factory.GetCachedValue<RelationshipIndicatorList>(), relatedIndicatorList);
			});
		}
	}
}
