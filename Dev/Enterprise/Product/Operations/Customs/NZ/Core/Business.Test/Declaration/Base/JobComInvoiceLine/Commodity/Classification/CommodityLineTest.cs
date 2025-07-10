using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CommodityLine))]
	public class CommodityLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CommodityLine>
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return CommodityLine;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommodityLine.CommodityCodes.AddNew();
			return CommodityLine;
		}

		protected override IEnumerable<CommodityLine> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			yield return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().CommodityLines.AddNew();
		}

		#endregion

		#region Implementation

		CommodityLine CommodityLine
		{
			get { return commodityLine ?? (commodityLine = InvoiceLine.CommodityLines.AddNew()); }
		}
		CommodityLine commodityLine;

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
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
