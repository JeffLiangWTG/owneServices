using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AgencyShipmentConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public void TestInvoiceTypeList()
		{
			var jobType = GetJobInvoicingConsumerType();
			foreach (CodeDescriptionPair invoiceType in new InvoiceTypesList())
			{
				if (invoiceType.Code.Equals(InvoiceTypesList.Codes.DestinationChargesInvoice_Batching) || invoiceType.Code.Equals(InvoiceTypesList.Codes.FreightInvoice_Batching))
				{
					AssertEquals(string.Format("{0}", invoiceType.CodeAndDescription), false, jobType.InvoiceTypeList.ContainsCode(invoiceType));
				}
				else
				{
					AssertEquals(string.Format("{0}", invoiceType.CodeAndDescription), true, jobType.InvoiceTypeList.ContainsCode(invoiceType));
				}
			}
		}
	}
}
