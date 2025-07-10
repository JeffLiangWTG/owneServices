using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class InvoiceLineCompleteCollection : Customs.Business.InvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)base[index]; }
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		public void ResetHadErrorInLastResponse()
		{
			foreach (JobComInvoiceLine invoiceLine in this)
			{
				invoiceLine.JI_HadErrorInLastResponse = false;
			}
		}

		#region OnCountChanged
		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (declaration != null && declaration.IsECIWriteoff)
			{
				declaration.JE_ECI_InvoiceAmountInfo.RefreshBinding();
			}
		}
		#endregion
	}
}
