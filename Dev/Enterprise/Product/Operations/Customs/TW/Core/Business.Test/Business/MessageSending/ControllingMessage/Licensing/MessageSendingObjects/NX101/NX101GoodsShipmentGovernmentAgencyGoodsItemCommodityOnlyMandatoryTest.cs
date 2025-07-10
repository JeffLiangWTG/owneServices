using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101GoodsShipmentGovernmentAgencyGoodsItemCommodityOnlyMandatoryTest : TestCaseWithFactory
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
			invoiceLine.NX101PermitGoodsDescription = "101 GOODS DESC";
			invoiceLine.JI_BrandName = "B N";
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.JI_IMPTariff = "12345678";
			invoiceLine.JI_InnerPackDescription = "Inner pack desc";
			invoiceLine.JI_PermitUnitPrice = 102M;
			invoiceLine.JI_PermitQty = 3M;
			invoiceLine.JI_TariffPrintLength = "2";
			invoiceLine.JI_Compositions = "C";

			ICommodity commodity = new NX101GoodsShipmentGovernmentAgencyGoodsItemCommodityOnlyMandatory(invoiceLine, "Grouping");
			CombineAssertions("Certificate type is 15", () =>
			{
				NUnit.Framework.Assert.That(commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ICommodity.CommercialCategorizationID");
				NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "ICommodity.Description");
				NUnit.Framework.Assert.That(commodity.Name, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ICommodity.Name");
				NUnit.Framework.Assert.That(commodity.Constituent, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IConstituent)), "ICommodity.Constituent - should be [null]");
				NUnit.Framework.Assert.That(commodity.PrintingTariffCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "ICommodity.PrintingTariffCode");
				NUnit.Framework.Assert.That(commodity.Classifications.Any(x => x.ID == "99999999" && x.IdentificationTypeCode == "HS"), NUnit.Framework.Is.True, "ICommodity.Classifications 1");
				NUnit.Framework.Assert.That(commodity.Classifications.Any(x => x.ID == "12345678" && x.IdentificationTypeCode == "ZZZ"), NUnit.Framework.Is.True, "ICommodity.Classifications 2");
				NUnit.Framework.Assert.That(commodity.CommodityRelatedPackaging, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.ICommodityRelatedPackaging)), "ICommodity.CommodityRelatedPackaging - should be [null]");
				NUnit.Framework.Assert.That(commodity.InvoiceLine, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IInvoiceLine)), "ICommodity.InvoiceLine - should be [null]");
				NUnit.Framework.Assert.That(commodity.Invoice, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IInvoice)), "ICommodity.Invoice - should be [null]");
			});
		}
	}
}
