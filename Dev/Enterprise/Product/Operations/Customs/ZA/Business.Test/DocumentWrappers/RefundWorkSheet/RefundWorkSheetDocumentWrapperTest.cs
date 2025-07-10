using System.Globalization;
using System.Linq;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class RefundWorkSheetDocumentWrapperTest : DA63DocumentWrapperTest
	{
		public void TestRefundWorkSheetRounding()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.GetCultureInfo("en-ZA")))
			{
				invoiceLine1.JI_ImportCustomsValue = 1.2345;
				invoiceLine2.JI_ImportCustomsValue = 1.5678;
				invoiceLine1.JI_ImportCustomsQty = 3.4545;
				invoiceLine2.JI_ImportCustomsQty = 3.6789;
				var refundControlSheetWrapper = new RefundWorkSheetDocumentWrapper(entryHeader);
				CombineAssertions(() =>
				{
					AssertEquals("DA63CustomsValue round down", "1", refundControlSheetWrapper.RefundWorkSheetLines[0].DA63CustomsValueString);
					AssertEquals("DA63CustomsValue round up", "2", refundControlSheetWrapper.RefundWorkSheetLines[1].DA63CustomsValueString);
					AssertEquals("DA63CustomsQuantity round down", "3.45", refundControlSheetWrapper.RefundWorkSheetLines[0].DA63CustomsQuantityString);
					AssertEquals("DA63CustomsQuantity round up", "3.68", refundControlSheetWrapper.RefundWorkSheetLines[1].DA63CustomsQuantityString);
				});
			}
		}

		public void TestRefundWorkSheetLines()
		{
			AssertEquals(3, entryHeader.MergedLines.Count);
			JobDeclaration declaration = entryLine1.Declaration;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine1.JI_Tariff = "991001";
			invoiceLine1.CusLineTariffDetails.RemoveAll();
			var tariffDetail = invoiceLine1.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = "991012";
			var tester = new RefundWorkSheetDocumentWrapper(entryHeader);
			AssertEquals(3, tester.RefundWorkSheetLines.Count);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<CusEntryLine>(), tester.RefundWorkSheetLines.OfType<RefundWorkSheetLineDetailWrapper>().Where(x => x.RefundWorkSheetRebatesUsed).Select(x => x.EntryLine));
			invoiceLine2.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine2.JI_Tariff = "85284910";
			invoiceLine2.CusLineTariffDetails.RemoveAll();
			var tariffDetail2 = invoiceLine2.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Type = "5P2";
			tariffDetail2.BZ_Tariff = "52203";
			tester = new RefundWorkSheetDocumentWrapper(entryHeader);
			AssertContainsExactElementsInAnyOrder(new CusEntryLine[] { entryLine2 }, tester.RefundWorkSheetLines.OfType<RefundWorkSheetLineDetailWrapper>().Where(x => x.RefundWorkSheetRebatesUsed).Select(x => x.EntryLine));
			invoiceLine1.CusLineTariffDetails.RemoveAll();
			invoiceLine2.CusLineTariffDetails.RemoveAll();
			tariffDetail = invoiceLine1.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = "991012";
			tariffDetail2 = invoiceLine2.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Type = "5P2";
			tariffDetail2.BZ_Tariff = "52203";
			tester = new RefundWorkSheetDocumentWrapper(entryHeader);
			var countResult = tester.RefundWorkSheetLines.OfType<RefundWorkSheetLineDetailWrapper>().Sum(x => x.EntryLine.InvoiceLines.OfType<JobComInvoiceLine>().Select(y => y.CusLineTariffDetails.Count).Sum());
			AssertEquals("CusLineTariffDetails.Count", 2, countResult);
			AssertContainsExactElementsInAnyOrder(new CusEntryLine[] { entryLine2 }, tester.RefundWorkSheetLines.OfType<RefundWorkSheetLineDetailWrapper>().Where(x => x.RefundWorkSheetRebatesUsed).Select(x => x.EntryLine));
		}
	}
}
