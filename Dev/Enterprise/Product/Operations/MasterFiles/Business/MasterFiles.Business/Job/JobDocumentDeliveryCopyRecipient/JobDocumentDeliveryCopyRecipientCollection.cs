using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDeliveryCopyRecipientCollection : CopyRecipientCollection<JobDocumentDeliveryCopyRecipient, JobDocumentDelivery>
	{
		public JobDocumentDeliveryCopyRecipientCollection(JobDocumentDelivery jobDocumentDelivery, string jobDocumentDeliveryCopyRecipientType)
			: base(jobDocumentDelivery, jobDocumentDeliveryCopyRecipientType, JobDocumentDeliveryCopyRecipientSchema.JDR_EmailAddress, JobDocumentDeliveryCopyRecipientSchema.JDR_RecipientType)
		{
		}

		protected override void SetDefaultsForNewElementCore(JobDocumentDeliveryCopyRecipient newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.JDR_JDC = Owner.PK;
		}
	}
}