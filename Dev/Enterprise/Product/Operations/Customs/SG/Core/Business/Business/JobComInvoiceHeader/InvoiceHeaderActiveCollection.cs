using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection
	{
		public InvoiceHeaderActiveCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
			: base(groupInvoice, isDirectRelationship)
		{
		}

		public new JobComInvoiceHeader AddNew()
		{
			return (JobComInvoiceHeader)base.AddNew();
		}

		public new JobComInvoiceHeader this[int index]
		{
			get { return (JobComInvoiceHeader)(base[index]); }
		}

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override void OnAdded(BaseJobComInvoiceHeader businessObject)
		{
			base.OnAdded(businessObject);
			if (!JobDeclaration.IsValidationSuspended)
			{
				JobDeclaration.Validation.ValidateJE_Calc_InvoicesCount();
			}

			JobDeclaration.JE_Calc_InvoicesCountInfo.RefreshBinding();
		}

		protected override Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader newElement, BaseJobDeclaration declaration)
		{
			return new DefaultSetterForInvoiceHeader(newElement, declaration);
		}
	}
}
