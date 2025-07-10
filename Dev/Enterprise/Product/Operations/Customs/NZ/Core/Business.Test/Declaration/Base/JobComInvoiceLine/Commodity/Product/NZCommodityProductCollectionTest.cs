using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCommodityProductCollection))]
	public class NZCommodityProductCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CommodityProducts;
		}

		public void TestMaximumLinesForCRE()
		{
			InvoiceLine.JI_Tariff = "2101.11.00.01C";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			invoiceLine = null;

			AssertEquals("CommodityProducts.MaxCount for CRE", 1, InvoiceLine.CommodityProducts.MaxCount);
			Assert("CommodityProducts.AllowNew", InvoiceLine.CommodityProducts.AllowNew);

			var pc1 = InvoiceLine.CommodityProducts.AddNew();
			pc1.B7_AddInfoData = "PC1";
			pc1.B7_Type = CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData;
			AssertEquals("CommodityProducts.MaxCount for CRE", 1, InvoiceLine.CommodityProducts.MaxCount);
			Assert("CommodityProducts.NOTAllowNew", !InvoiceLine.CommodityProducts.AllowNew);
			AssertNoRowErrors(pc1);

			var pc2 = InvoiceLine.CommodityProducts.AddNew();
			pc2.B7_AddInfoData = "PC1";
			pc2.B7_Type = CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData;
			Factory.Save();
			AssertEquals("CommodityProducts.MaxCount for CRE", 1, InvoiceLine.CommodityProducts.MaxCount);
			Assert("CommodityProducts.NOTAllowNew", !InvoiceLine.CommodityProducts.AllowNew);
			AssertHasRowError(pc2, "You are only allowed a maximum of 1 Commodity Product for CRE.");
		}

		public void TestMaximumLinesForOthers()
		{
			InvoiceLine.JI_Tariff = "2101.11.00.01C";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			invoiceLine = null;

			AssertEquals("CommodityProducts.MaxCount for Others", -1, InvoiceLine.CommodityProducts.MaxCount);
			Assert("CommodityProducts.AllowNew", InvoiceLine.CommodityProducts.AllowNew);

			var pc1 = InvoiceLine.CommodityProducts.AddNew();
			pc1.B7_AddInfoData = "PC1";
			pc1.B7_Type = CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData;
			AssertEquals("CommodityProducts.MaxCount for CRE", -1, InvoiceLine.CommodityProducts.MaxCount);
			Assert("CommodityProducts.AllowNew", InvoiceLine.CommodityProducts.AllowNew);
			AssertNoRowErrors(pc1);

			var pc2 = InvoiceLine.CommodityProducts.AddNew();
			pc2.B7_AddInfoData = "PC1";
			pc2.B7_Type = CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData;
			Factory.Save();
			AssertEquals("CommodityProducts.MaxCount for CRE", -1, InvoiceLine.CommodityProducts.MaxCount);
			Assert("CommodityProducts.AllowNew", InvoiceLine.CommodityProducts.AllowNew);
			AssertNoRowErrors(pc2);
		}

		public void TestUpdateMaxCountValidation()
		{
			InvoiceLine.JI_Tariff = "2101.11.00.01C";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			var pc1 = InvoiceLine.CommodityProducts.AddNew();
			pc1.B7_AddInfoData = "PC1";
			pc1.B7_Type = CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData;
			var pc2 = InvoiceLine.CommodityProducts.AddNew();
			pc2.B7_AddInfoData = "PC1";
			pc2.B7_Type = CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CommodityProducts.MaxCount for CRE", 1, InvoiceLine.CommodityProducts.MaxCount);
				Assert("CommodityProducts.NOTAllowNew", !InvoiceLine.CommodityProducts.AllowNew);
				AssertNoRowErrors(pc1);
				AssertHasRowError(pc2, "You are only allowed a maximum of 1 Commodity Product for CRE.");

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("CommodityProducts.MaxCount for Import", -1, InvoiceLine.CommodityProducts.MaxCount);
				Assert("CommodityProducts.AllowNew", InvoiceLine.CommodityProducts.AllowNew);
				AssertNoRowErrors(pc1);
				AssertNoRowErrors(pc2);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				AssertEquals("CommodityProducts.MaxCount for Non-Writoff", -1, InvoiceLine.CommodityProducts.MaxCount);
				Assert("CommodityProducts.AllowNew", InvoiceLine.CommodityProducts.AllowNew);
				AssertNoRowErrors(pc1);
				AssertNoRowErrors(pc2);

				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				AssertEquals("CommodityProducts.MaxCount for CUSMOD", -1, InvoiceLine.CommodityProducts.MaxCount);
				Assert("CommodityProducts.AllowNew", InvoiceLine.CommodityProducts.AllowNew);
				AssertNoRowErrors(pc1);
				AssertNoRowErrors(pc2);

				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("CommodityProducts.MaxCount for CRE", 1, InvoiceLine.CommodityProducts.MaxCount);
				Assert("CommodityProducts.NOTAllowNew", !InvoiceLine.CommodityProducts.AllowNew);
				AssertNoRowErrors(pc1);
				AssertHasRowError(pc2, "You are only allowed a maximum of 1 Commodity Product for CRE.");
			});
		}

		NZCommodityProductCollection CommodityProducts
		{
			get { return commodityLines ?? (commodityLines = InvoiceLine.CommodityProducts); }
		}
		NZCommodityProductCollection commodityLines;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (invoiceHeader == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					invoiceHeader = declaration.Invoices.AddNew();
				}

				return invoiceHeader;
			}
		}
		JobComInvoiceHeader invoiceHeader;

		#endregion

		protected JobDeclaration declaration;
	}
}
