using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DefaultSetterForInvoiceHeader : Customs.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader newElement, BaseJobDeclaration declaration) : base(newElement, declaration)
		{
		}

		new JobDeclaration declaration => (JobDeclaration)base.declaration;

		protected override void DefaultForNewElementCore()
		{
			base.DefaultForNewElementCore();

			newElement.JZ_IncoTerm = declaration.GetDefaultINCOFromSupplierBuyerLink();
		}
	}
}
