using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class HVLVISFMessagesSendWrapper : NonPersistentBusinessObject
	{
		public HVLVISFMessagesSendWrapper(IRelatedJob relatedJob)
		{
			Argument.NotNull(relatedJob, nameof(relatedJob));
			RelatedJob = relatedJob;
		}

		public IRelatedJob RelatedJob { get; }

		public ZString JobNumber => RelatedJob?.JobNumber ?? ZString.Empty;

		public ZString JobStatus => RelatedJob?.JobStatus ?? ZString.Empty;

		public ZBool ShouldSend
		{
			get { return shouldSend; }
			set { SetNonPersistentPropertyValue(ShouldSendInfo, ref shouldSend, value); }
		}

		ZBool shouldSend;

		public ZPropertyInfo ShouldSendInfo => GetZPropertyInfo(nameof(ShouldSend));
	}
}
