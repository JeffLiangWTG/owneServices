using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCommodityConstituentAddInfo))]
	public class NZCommodityConstituentAddInfoTest : NonPersistentBusinessObjectTestCase
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

			var commodityConstituent = invoiceLine.CommodityConstituents.AddNew();
			commodityConstituent.NZ_ConstituentName = "PCP-20";
			AssertEquals(true, invoiceLine.ShouldValidateOnSave);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var commodityConstituent = Factory.New<CommodityConstituent>();
			var addInfo = new NZCommodityConstituentAddInfo(commodityConstituent.B7_AddInfoDataInfo);
			addInfo.CommodityConstituent = commodityConstituent;
			return addInfo;
		}

		#endregion
	}
}
