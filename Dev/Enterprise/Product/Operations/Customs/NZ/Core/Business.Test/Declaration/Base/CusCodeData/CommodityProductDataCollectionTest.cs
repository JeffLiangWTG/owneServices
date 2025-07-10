using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CommodityProductDataCollection))]
	public class CommodityProductDataCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<CommodityProductData>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		#region Implementation

		protected override Customs.Business.CusCodeDataCollection<CommodityProductData> GetCusCodeDataCollection()
		{
			return CommodityProducts;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommodityProductData result = Factory.New<CommodityProductData>();
			result.CY_ParentID = InvoiceLine.CommodityProducts[0].PK;
			result.CY_ParentTableCode = "B7";
			return result;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
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

		CommodityProduct CommodityProduct
		{
			get { return InvoiceLine.CommodityProducts[0]; }
		}

		CommodityProductDataCollection CommodityProducts
		{
			get { return CommodityProduct.CommodityProducts; }
		}

		#endregion
	}
}
