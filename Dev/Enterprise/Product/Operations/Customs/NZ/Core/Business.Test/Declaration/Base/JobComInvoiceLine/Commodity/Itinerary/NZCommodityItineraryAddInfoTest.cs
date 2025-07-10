using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCommodityItineraryAddInfo))]
	public class NZCommodityItineraryAddInfoTest : NonPersistentBusinessObjectTestCase
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

			var commodityItinerary = invoiceLine.CommodityItineraries.AddNew();
			commodityItinerary.NZ_RoutingCountry = "JP";
			AssertEquals(true, invoiceLine.ShouldValidateOnSave);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var commodityItinerary = Factory.New<CommodityItinerary>();
			var addInfo = new NZCommodityItineraryAddInfo(commodityItinerary.B7_AddInfoDataInfo);
			addInfo.CommodityItinerary = commodityItinerary;
			return addInfo;
		}

		#endregion
	}
}
