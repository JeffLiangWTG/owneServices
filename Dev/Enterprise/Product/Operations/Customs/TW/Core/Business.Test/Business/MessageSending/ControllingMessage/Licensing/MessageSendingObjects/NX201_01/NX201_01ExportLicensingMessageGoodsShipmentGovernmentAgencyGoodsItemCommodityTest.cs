using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity))]
	sealed class NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodityTest : TestCaseWithFactory
	{
		[TestDate(2024, 06, 05)]
		[ExpectNoExceptions]
		public void TestUnitPriceAmount()
		{
			var originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = ZBool.True;
				CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2024, 06, 05), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
				Factory.Save();

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
				var goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine = goodsShipmentGovernmentAgencyGoodsItemCommodity.InvoiceLine;
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.UnitPriceAmount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "UnitPriceAmount");
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			var permit1 = invoiceLine.PermitCusSupportingCollection.AddNew();
			permit1.CSI_Code = "111";
			permit1.CSI_LineNo = 1;
			var permit2 = invoiceLine.PermitCusSupportingCollection.AddNew();
			permit2.CSI_Code = "222";
			permit2.CSI_LineNo = 2;
			var additionalDocument = goodsShipmentGovernmentAgencyGoodsItemCommodity.AdditionalDocuments;
			NUnit.Framework.Assert.That(additionalDocument.Count(), NUnit.Framework.Is.EqualTo(0));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			goodsShipmentGovernmentAgencyGoodsItemCommodity = new NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		JobComInvoiceLine invoiceLine;
		ICommodity goodsShipmentGovernmentAgencyGoodsItemCommodity;
	}
}
