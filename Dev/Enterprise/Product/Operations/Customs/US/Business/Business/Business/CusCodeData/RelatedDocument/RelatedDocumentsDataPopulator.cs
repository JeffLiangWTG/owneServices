using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class RelatedDocumentsDataPopulator
	{
		public RelatedDocumentsDataPopulator(JobComInvoiceHeader invoice)
		{
			this.invoice = invoice;
		}

		public void PopulateRelatedDocuments()
		{
			if (invoice.JobDeclaration.LowestBills.Count == 1)
			{
				IBillDetails bill = (IBillDetails)invoice.JobDeclaration.LowestBills[0];
				if (bill != null)
				{
					invoice.RelatedDocuments.AddOrUpdate((invoice.JobDeclaration.IsAir) ? RelatedDocumentIdentifierList.Codes.AirWaybillNumber : RelatedDocumentIdentifierList.Codes.BillOfLadingNumber, bill.MasterBillNumber);
					invoice.RelatedDocuments.AddOrUpdate(RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber, bill.HouseBillNumber);
					invoice.RelatedDocuments.AddOrUpdate(RelatedDocumentIdentifierList.Codes.SubhouseBillOfLading, bill.SubHouseBillNumber);
				}
			}
		}

		readonly JobComInvoiceHeader invoice;
	}
}
