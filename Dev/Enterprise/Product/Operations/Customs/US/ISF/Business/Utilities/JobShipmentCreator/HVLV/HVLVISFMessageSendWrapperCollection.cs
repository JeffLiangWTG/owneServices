using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class HVLVISFMessageSendWrapperCollection : NonPersistentBusinessObjectCollection<HVLVISFMessagesSendWrapper>
	{
		public HVLVISFMessageSendWrapperCollection(RelatedJobCollection relatedJobs)
			: base()
		{
			foreach (IRelatedJob relatedJob in relatedJobs)
			{
				Add(new HVLVISFMessagesSendWrapper(relatedJob));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new System.NotSupportedException("HVLVISFMessageSendWrapperCollection is not allowed to add new elements");

		protected override bool AllowNewCore => false;
	}
}
