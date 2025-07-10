using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class DefaultSetterForInvoiceHeader : Customs.Business.DefaultSetterForInvoiceHeader
	{
		public DefaultSetterForInvoiceHeader(JobComInvoiceHeader child, JobDeclaration declaration)
			: base(child, declaration)
		{
		}

		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;

		protected override void SetDefaultsForAdditionalInvoiceCore(BaseJobComInvoiceHeader previousInvoice)
		{
			base.SetDefaultsForAdditionalInvoiceCore(previousInvoice);
			newElement.JZ_RelatedIndicator = previousInvoice.JZ_RelatedIndicator;
		}
	}
}
