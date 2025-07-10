using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity))]
	sealed class NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodityTest : TestCaseWithFactory
	{
		[TestDate(2024, 05, 09)]
		[ExpectNoExceptions]
		public void TestItemChargeAmount()
		{
			var originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = ZBool.True;
				CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2024, 05, 09), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
				Factory.Save();

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
				var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.AssignCMHeaderToInvoices(header);

				var invoice1 = invoiceLine.InvoiceHeader;
				invoice1.JZ_InvoiceAmount = 10000m;
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 7000m;
				var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
				invoiceLine2.AssignCMHeaderToInvoices(header);

				invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
				invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
				invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CarriagePaidTo;
				invoiceLine.JI_EnteredUnitPrice = 1234m;
				invoiceLine.JI_InvoiceQuantity = 2m;
				ICommodity goodsShipmentGovernmentAgencyGoodsItemCommodity = new NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.InvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "Import: ItemChargeAmount");
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}
	}
}
