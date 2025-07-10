using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceHeaderActiveCollection : EU.Business.Declaration.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration) : base(declaration)
		{
		}

		public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship) : base(groupInvoice, isDirectRelationship)
		{
		}

		protected new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public new JobComInvoiceHeader AddNew() => (JobComInvoiceHeader)base.AddNew();

		public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)base[index];

		#region SetDefaultsForNewElement

		protected override Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader newElement, BaseJobDeclaration declaration)
		{
			return new DefaultSetterForInvoiceHeader((JobComInvoiceHeader)newElement, (JobDeclaration)declaration);
		}

		#endregion
	}
}
