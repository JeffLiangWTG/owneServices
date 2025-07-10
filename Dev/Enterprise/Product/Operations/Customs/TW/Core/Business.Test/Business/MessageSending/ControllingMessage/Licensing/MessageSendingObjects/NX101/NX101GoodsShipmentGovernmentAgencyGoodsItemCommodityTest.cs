using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity))]
	sealed class NX101GoodsShipmentGovernmentAgencyGoodsItemCommodityTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCommodityProperties()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_InvoiceNumber = "I12";
			invoice.JZ_InvoiceDate = ZDateTime.BrettsBirthday;
			var entryInstruction = declaration.CusEntryInstruction;
			var header = entryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			invoiceLine.JI_CL = mergedLine.PK;
			invoiceLine.JI_Model = "M";
			invoiceLine.JI_DeclarationGoodsDescription = new ZString('A', 513);
			invoiceLine.JI_BrandName = "B N";
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.JI_IMPTariff = "12345678";
			invoiceLine.JI_InnerPackDescription = "Inner pack desc";
			invoiceLine.JI_PermitUnitPrice = 102M;
			invoiceLine.JI_PermitQty = 3M;
			invoiceLine.JI_TariffPrintLength = "2";
			invoiceLine.JI_Compositions = "C";

			ICommodity commodity = new NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);
			CombineAssertions("Certificate type is 15", () =>
			{
				NUnit.Framework.Assert.That(commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("M").Using(CustomComparers.TypeComparison), "ICommodity.CommercialCategorizationID");
				NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo(new ZString('A', 512)), "ICommodity.Description");
				NUnit.Framework.Assert.That(commodity.Name, NUnit.Framework.Is.EqualTo("B N").Using(CustomComparers.TypeComparison), "ICommodity.Name");
				NUnit.Framework.Assert.That(commodity.Constituent.ElementDescription, NUnit.Framework.Is.EqualTo("C").Using(CustomComparers.TypeComparison), "ICommodity.Constituent");
				NUnit.Framework.Assert.That(commodity.PrintingTariffCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "ICommodity.PrintingTariffCode");
				NUnit.Framework.Assert.That(commodity.Classifications.Any(x => x.ID == "99999999" && x.IdentificationTypeCode == "HS"), NUnit.Framework.Is.True, "ICommodity.Classifications 1");
				NUnit.Framework.Assert.That(commodity.Classifications.Any(x => x.ID == "12345678" && x.IdentificationTypeCode == "ZZZ"), NUnit.Framework.Is.True, "ICommodity.Classifications 2");
				NUnit.Framework.Assert.That(commodity.CommodityRelatedPackaging.Specification, NUnit.Framework.Is.EqualTo("Inner pack desc").Using(CustomComparers.TypeComparison), "ICommodity.CommodityRelatedPackaging");
				NUnit.Framework.Assert.That(commodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(306M).Using(CustomComparers.TypeComparison), "ICommodity.InvoiceLine.ItemChargeAmount");
				NUnit.Framework.Assert.That(commodity.InvoiceLine.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "ICommodity.InvoiceLine.CurrencyTypeCode");
			});

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			commodity = new NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);
			CombineAssertions("Certificate type is 01", () =>
			{
				NUnit.Framework.Assert.That(commodity.CommercialCategorizationID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ICommodity.CommercialCategorizationID - should be [null] or [empty]");
				NUnit.Framework.Assert.That(commodity.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty, "ICommodity.Name - should be [null] or [empty]");
				NUnit.Framework.Assert.That(commodity.Constituent, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IConstituent)), "ICommodity.Constituent - should be [null]");
				NUnit.Framework.Assert.That(commodity.InvoiceLine, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IInvoiceLine)), "ICommodity.InvoiceLine - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestInvoice()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "I12";
			invoice.JZ_InvoiceDate = ZDateTime.BrettsBirthday;
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = mergedLine.PK;
			var header = entryInstruction.ControllingMessageHeaders.AddNew();
			AssertInvoice(header, invoiceLine, CertificateTypeList.Codes.Code9);
			AssertInvoice(header, invoiceLine, CertificateTypeList.Codes.Code11);
			AssertInvoice(header, invoiceLine, CertificateTypeList.Codes.Code12);
			AssertInvoice(header, invoiceLine, CertificateTypeList.Codes.Code13);
			AssertInvoice(header, invoiceLine, CertificateTypeList.Codes.Code14);
			AssertInvoice(header, invoiceLine, CertificateTypeList.Codes.Code15);

			header.TW1_CertificateType = CertificateTypeList.Codes.Code2;
			ICommodity commodity = new NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Invoice, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IInvoice)));
		}

		[ExpectNoExceptions]
		void AssertInvoice(CusTWControllingMessageHeader header, JobComInvoiceLine line, ZString certificateType)
		{
			header.TW1_CertificateType = certificateType;
			ICommodity commodity = new NX101GoodsShipmentGovernmentAgencyGoodsItemCommodity(header, line);
			NUnit.Framework.Assert.That(commodity.Invoice.ID, NUnit.Framework.Is.EqualTo("I12").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.Invoice.IssueDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday.Date));
		}
	}
}
