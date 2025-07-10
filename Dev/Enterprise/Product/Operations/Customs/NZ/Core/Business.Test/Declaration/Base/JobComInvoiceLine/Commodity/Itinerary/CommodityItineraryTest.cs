using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CommodityItinerary))]
	public class CommodityItineraryTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CommodityItinerary>
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return CommodityItinerary;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommodityItinerary.CommodityItineraries.AddNew();
			return CommodityItinerary;
		}

		protected override IEnumerable<CommodityItinerary> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			yield return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().CommodityItineraries.AddNew();
		}

		#endregion

		#region Implementation

		CommodityItinerary CommodityItinerary
		{
			get { return commodityItinerary ?? (commodityItinerary = InvoiceLine.CommodityItineraries.AddNew()); }
		}
		CommodityItinerary commodityItinerary;

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
