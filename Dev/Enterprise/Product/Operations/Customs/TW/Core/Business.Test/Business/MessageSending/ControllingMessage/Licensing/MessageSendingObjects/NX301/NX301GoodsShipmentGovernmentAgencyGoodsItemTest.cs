using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX301GoodsShipmentGovernmentAgencyGoodsItem))]
	sealed class NX301GoodsShipmentGovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPackaging()
		{
			var goodsItem = GoodsShipmentGovernmentAgencyGoodsItem;
			CombineAssertions(() =>
			{
				InvoiceLine.JI_PackagingQTY = 123M;
				InvoiceLine.JI_PackagingUQ = "PKG";
				Header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
				var packaging = goodsItem.Packaging;
				NUnit.Framework.Assert.That(packaging.QuantityQuantity, NUnit.Framework.Is.EqualTo(123M).Using(CustomComparers.TypeComparison), $"QuantityQuantity");
				NUnit.Framework.Assert.That(packaging.TypeCode, NUnit.Framework.Is.EqualTo("PKG").Using(CustomComparers.TypeComparison), $"TypeCode");

				Header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
				InvoiceLine.JI_PackagingQTY = ZDecimal.Zero;
				NUnit.Framework.Assert.That(goodsItem.Packaging, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPackaging)), "QTY == 0, not output Packaging - should be [null]");
			});
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ??= Factory.NewWithValidTestData<JobDeclaration>();

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = (JobComInvoiceLine)Declaration.Invoices.AddNew().InvoiceLines.AddNew();
					invoiceLine.AssignCMHeaderToInvoices(Header);
				}
				return invoiceLine;
			}
		}

		CusTWControllingMessageHeader header;
		CusTWControllingMessageHeader Header => header ??= Declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();

		IGovernmentAgencyGoodsItem goodsShipmentGovernmentAgencyGoodsItem;
		IGovernmentAgencyGoodsItem GoodsShipmentGovernmentAgencyGoodsItem => goodsShipmentGovernmentAgencyGoodsItem ??= new NX301GoodsShipmentGovernmentAgencyGoodsItem(1, Header, InvoiceLine);
	}
}
