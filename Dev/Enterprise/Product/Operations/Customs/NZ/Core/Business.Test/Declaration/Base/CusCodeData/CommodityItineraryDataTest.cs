using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;
	using NUnit.Framework;

	[TestedType(typeof(CommodityItineraryData))]
	public class CommodityItineraryDataTest : Customs.Business.Testing.CusCodeDataTest<CommodityItineraryData>
	{
		public void TestHumanReadableNameForCY_Data()
		{
			var uscAffirmCode = Factory.New<CommodityItineraryData>();
			uscAffirmCode.CY_Code = "AAA";
			AssertEquals("TSW (AAA)", uscAffirmCode.CY_DataInfo.HumanReadableName);

			uscAffirmCode.CY_Code = "";
			AssertEquals("CommodityItinerary", uscAffirmCode.CY_DataInfo.HumanReadableName);

			uscAffirmCode.CY_Code = "BBB";
			AssertEquals("TSW (BBB)", uscAffirmCode.CY_DataInfo.HumanReadableName);

			var copyCode = Factory.New<CommodityItineraryData>();
			copyCode.CopyPersistentValuesFrom(uscAffirmCode);
			AssertEquals("TSW (BBB)", copyCode.CY_DataInfo.HumanReadableName);

			copyCode = (CommodityItineraryData)uscAffirmCode.Clone();
			AssertEquals("TSW (BBB)", copyCode.CY_DataInfo.HumanReadableName);
		}

		public void TestCY_Data()
		{
			var commodityItinerary = Factory.New<CommodityItineraryData>();
			commodityItinerary.CY_Data = "dsfsdf";
			AssertEquals("DSFSDF", commodityItinerary.CY_Data);
		}

		public void TestSetDefaultValues()
		{
			var commodityItinerary = Factory.New<CommodityItineraryData>();
			AssertEquals("NZTSWCommodityItineraryData Type", "TCI", commodityItinerary.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().CommodityItineraries.AddNew().CommodityItineraries.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CommodityItineraries.AddNew();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
					fInvoiceLine = invoice.JobComInvoiceLines.AddNew();
					fInvoiceLine.CommodityItineraries.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		CommodityItineraryDataCollection CommodityItineraries
		{
			get
			{
				if (fCommodityItineraries == null)
				{
					fCommodityItineraries = new CommodityItineraryDataCollection(InvoiceLine.CommodityItineraries.AddNew());
				}
				return fCommodityItineraries;
			}
		}
		CommodityItineraryDataCollection fCommodityItineraries;
	}
}
