using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(CommodityItineraryDataCollection))]
	public class CommodityItineraryDataCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<CommodityItineraryData>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		#region Implementation

		protected override Customs.Business.CusCodeDataCollection<CommodityItineraryData> GetCusCodeDataCollection()
		{
			return CommodityItineraries;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommodityItineraryData result = Factory.New<CommodityItineraryData>();
			result.CY_ParentID = InvoiceLine.CommodityItineraries[0].PK;
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
					fInvoiceLine.CommodityItineraries.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		CommodityItinerary CommodityItinerary
		{
			get { return InvoiceLine.CommodityItineraries[0]; }
		}

		CommodityItineraryDataCollection CommodityItineraries
		{
			get { return CommodityItinerary.CommodityItineraries; }
		}

		#endregion
	}
}
