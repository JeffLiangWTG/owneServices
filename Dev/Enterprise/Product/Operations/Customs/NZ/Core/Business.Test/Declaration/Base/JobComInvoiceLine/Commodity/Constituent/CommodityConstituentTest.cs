using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CommodityConstituent))]
	public class CommodityConstituentTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CommodityConstituent>
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return CommodityConstituent;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommodityConstituent.CommodityCodes.AddNew();
			return CommodityConstituent;
		}

		protected override IEnumerable<CommodityConstituent> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			yield return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().CommodityConstituents.AddNew();
		}

		#endregion

		#region Implementation

		CommodityConstituent CommodityConstituent
		{
			get { return commodityConstituent ?? (commodityConstituent = InvoiceLine.CommodityConstituents.AddNew()); }
		}
		CommodityConstituent commodityConstituent;

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
