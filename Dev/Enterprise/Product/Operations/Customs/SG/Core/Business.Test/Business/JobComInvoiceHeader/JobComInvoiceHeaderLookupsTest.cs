namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Lookups.Invoice, parent);
		}

		public void TestIncoTermList()
		{
			Assert(Lookups.JZ_IncoTerm_List is UnitPriceTermTypeCodeList);
			AssertEquals("UnitPriceTermTypeCodeList should be cached", Lookups.JZ_IncoTerm_List, Factory.New<JobComInvoiceHeader>().Lookups.JZ_IncoTerm_List);
		}

		JobComInvoiceHeaderLookups Lookups
		{
			get
			{
				return InvoiceHeader.Lookups;
			}
		}

		public override void TestMessageTypes()
		{
			JobComInvoiceHeader header = Factory.New<JobComInvoiceHeader>();
			AssertEquals(typeof(MessageTypeCodeList), header.Lookups.MessageTypes.GetType());
		}

		#region InvoiceHeader
		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Factory.New<JobComInvoiceHeader>());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
	}
}
