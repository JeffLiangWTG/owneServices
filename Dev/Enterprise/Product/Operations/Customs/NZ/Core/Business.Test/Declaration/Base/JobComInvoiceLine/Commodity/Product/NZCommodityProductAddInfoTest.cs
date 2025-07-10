using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using NUnit.Framework;

	[TestedType(typeof(NZCommodityProductAddInfo))]
	public class NZCommodityProductAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestSettingCommodityDataMarksInvoiceLineForValidation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";

			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4201.00.00.01B";
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "PK";
			invoiceLine.JI_LinePrice = 1000m;

			Factory.Save();

			var commodityProduct = invoiceLine.CommodityProducts.AddNew();
			commodityProduct.NZ_ProductIDType = IdentityTypeList.Codes.BN;
			AssertEquals(true, invoiceLine.ShouldValidateOnSave);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var commodityProduct = Factory.New<CommodityProduct>();
			var addInfo = new NZCommodityProductAddInfo(commodityProduct.B7_AddInfoDataInfo);
			addInfo.CommodityProduct = commodityProduct;
			return addInfo;
		}

		#endregion
	}
}
