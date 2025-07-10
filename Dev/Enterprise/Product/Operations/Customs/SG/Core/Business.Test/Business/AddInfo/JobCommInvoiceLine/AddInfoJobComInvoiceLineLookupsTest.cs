namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobComInvoiceLineLookupsTest : SGAddInfoLookupsTest
	{
		public void TestCertItemUQList()
		{
			AssertNotNull(AddInfoJobComInvoiceLineLookups.ProductCodeUQList);
		}

		public void TestEndUseCodes1()
		{
			Assert(AddInfoJobComInvoiceLineLookups.EndUseCodes1 is CA_SC1CodeList);
		}

		public void TestEndUseCodes2()
		{
			Assert(AddInfoJobComInvoiceLineLookups.EndUseCodes2 is CA_SC2CodeList);
		}

		public void TestEndUseCodes3()
		{
			Assert(AddInfoJobComInvoiceLineLookups.EndUseCodes3 is CA_SC3CodeList);
		}

		public void TestCorrectStrategicGoodsProductCodeListIsReturned()
		{
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat0;
			AssertEquals("Expected list - Dual Use Category 0", typeof(DualUseCat0), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat1;
			AssertEquals("Expected list - Dual Use Category 1", typeof(DualUseCat1), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat2;
			AssertEquals("Expected list - Dual Use Category 2", typeof(DualUseCat2), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat3;
			AssertEquals("Expected list - Dual Use Category 3", typeof(DualUseCat3), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat4;
			AssertEquals("Expected list - Dual Use Category 4", typeof(DualUseCat4), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat5;
			AssertEquals("Expected list - Dual Use Category 5", typeof(DualUseCat5), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat6;
			AssertEquals("Expected list - Dual Use Category 6", typeof(DualUseCat6), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat7;
			AssertEquals("Expected list - Dual Use Category 7", typeof(DualUseCat7), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat8;
			AssertEquals("Expected list - Dual Use Category 8", typeof(DualUseCat8), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.Cat9;
			AssertEquals("Expected list - Dual Use Category 9", typeof(DualUseCat9), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
			InvoiceLine.SG_StrategicGoodsCategory = StrategicGoodsCategoryList.Codes.ML;
			AssertEquals("Expected list - Munitions List", typeof(StrategicGoodsMunitionsList), AddInfoJobComInvoiceLineLookups.StrategicGoodsProductCode.GetType());
		}

		#region Implementation
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
			}
		}

		JobComInvoiceLine invoiceLine;
		AddInfoJobComInvoiceLine AddInfoJobComInvoiceLine
		{
			get
			{
				return addInfoJobComInvoiceLine ?? (addInfoJobComInvoiceLine = new AddInfoJobComInvoiceLine(InvoiceLine.JI_AddInfoInfo));
			}
		}

		AddInfoJobComInvoiceLine addInfoJobComInvoiceLine;
		AddInfoJobComInvoiceLineLookups AddInfoJobComInvoiceLineLookups
		{
			get
			{
				return AddInfoJobComInvoiceLine.Lookups;
			}
		}
		#endregion
	}
}
