using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CommodityProduct))]
	public class CommodityProductTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CommodityProduct>
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return CommodityProduct;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommodityProduct.CommodityProducts.AddNew();
			return commodityProduct;
		}

		protected override IEnumerable<CommodityProduct> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			yield return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().CommodityProducts.AddNew();
		}

		#endregion

		#region Implementation

		CommodityProduct CommodityProduct
		{
			get { return commodityProduct ?? (commodityProduct = InvoiceLine.CommodityProducts.AddNew()); }
		}
		CommodityProduct commodityProduct;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoiceHeader;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
