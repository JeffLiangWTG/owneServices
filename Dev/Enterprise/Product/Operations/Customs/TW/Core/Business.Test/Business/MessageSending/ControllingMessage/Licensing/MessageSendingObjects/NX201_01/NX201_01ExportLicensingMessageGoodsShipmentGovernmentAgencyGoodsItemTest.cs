using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem))]
	sealed class NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(((IOrigin)goodsShipmentGovernmentAgencyGoodsItem).CountryCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestCommodity()
		{
			NUnit.Framework.Assert.That(((IGovernmentAgencyGoodsItem)goodsShipmentGovernmentAgencyGoodsItem).Commodity, NUnit.Framework.Is.TypeOf<NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			goodsShipmentGovernmentAgencyGoodsItem = new NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(1, header, invoiceLine);
		}

		NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem goodsShipmentGovernmentAgencyGoodsItem;
	}
}
