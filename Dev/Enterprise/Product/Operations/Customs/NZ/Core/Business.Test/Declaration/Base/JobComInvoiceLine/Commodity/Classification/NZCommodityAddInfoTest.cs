using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using NUnit.Framework;

	[TestedType(typeof(NZCommodityAddInfo))]
	public class NZCommodityAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestSettingCommodityDataMarksInvoiceLineForValidation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";

			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4201.00.00.01B";
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "PK";
			invoiceLine.JI_LinePrice = 1000m;

			Factory.Save();

			var commodityLine = invoiceLine.CommodityLines.AddNew();
			commodityLine.NZ_ClassificationType = ClassificationTypeList.Codes.CV;
			AssertEquals(true, invoiceLine.ShouldValidateOnSave);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var commodityLine = Factory.New<CommodityLine>();
			var addInfo = new NZCommodityAddInfo(commodityLine.B7_AddInfoDataInfo);
			addInfo.CommodityLine = commodityLine;
			return addInfo;
		}

		#endregion
	}
}
