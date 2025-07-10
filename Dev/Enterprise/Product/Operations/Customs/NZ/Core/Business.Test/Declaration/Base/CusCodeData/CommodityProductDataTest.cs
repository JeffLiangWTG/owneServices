using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;
	using NUnit.Framework;

	[TestedType(typeof(CommodityProductData))]
	public class CommodityProductDataTest : Customs.Business.Testing.CusCodeDataTest<CommodityProductData>
	{
		public void TestHumanReadableNameForCY_Data()
		{
			var uscAffirmCode = Factory.New<CommodityProductData>();
			uscAffirmCode.CY_Code = "AAA";
			AssertEquals("TSW (AAA)", uscAffirmCode.CY_DataInfo.HumanReadableName);

			uscAffirmCode.CY_Code = "";
			AssertEquals("CommodityProduct", uscAffirmCode.CY_DataInfo.HumanReadableName);

			uscAffirmCode.CY_Code = "BBB";
			AssertEquals("TSW (BBB)", uscAffirmCode.CY_DataInfo.HumanReadableName);

			var copyCode = Factory.New<CommodityProductData>();
			copyCode.CopyPersistentValuesFrom(uscAffirmCode);
			AssertEquals("TSW (BBB)", copyCode.CY_DataInfo.HumanReadableName);

			copyCode = (CommodityProductData)uscAffirmCode.Clone();
			AssertEquals("TSW (BBB)", copyCode.CY_DataInfo.HumanReadableName);
		}

		public void TestCY_Data()
		{
			var commodityProduct = Factory.New<CommodityProductData>();
			commodityProduct.CY_Data = "dsfsdf";
			AssertEquals("DSFSDF", commodityProduct.CY_Data);
		}

		public void TestSetDefaultValues()
		{
			var commodityProduct = Factory.New<CommodityProductData>();
			AssertEquals("NZTSWCommodityProductData Type", "TCP", commodityProduct.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var commodityProduct = invoiceLine.CommodityProducts.AddNew();
			return commodityProduct.CommodityProducts.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CommodityProducts.AddNew();
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
					fInvoiceLine.CommodityProducts.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		CommodityProductDataCollection CommodityProducts
		{
			get
			{
				if (fCommodityProducts == null)
				{
					fCommodityProducts = new CommodityProductDataCollection(InvoiceLine.CommodityProducts.AddNew());
				}
				return fCommodityProducts;
			}
		}
		CommodityProductDataCollection fCommodityProducts;
	}
}
