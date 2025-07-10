using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.ZA.Business
{
	[System.Serializable]
	class CUSRESAutoEDocDeliveryJob : AutoDocumentDeliveryJob
	{
		public CUSRESAutoEDocDeliveryJob(IDocumentSupportable businessObject, ZGuid documentCommandPK, ZGuid printerQueuePK, string subjectOverride = "") : base(businessObject, documentCommandPK, printerQueuePK, true, true)
		{
			this.subjectOverride = subjectOverride;
		}

#if NETFRAMEWORK
		public CUSRESAutoEDocDeliveryJob(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		readonly ZString subjectOverride;

		protected override DeliveryInstructions GetDeliveryInstructions(DocumentPack pack)
		{
			return new DeliveryInstructionsWrapperWithEmailSubjectOverride(pack, subjectOverride);
		}
	}

	class DeliveryInstructionsWrapperWithEmailSubjectOverride : DeliveryInstructions, IDeliveryInstructionsWithEmailSubjectOverride
	{
		public DeliveryInstructionsWrapperWithEmailSubjectOverride(DocumentPack pack, ZString emailSubjectOverride) : base(pack)
		{
			Recipients.RemoveAll();
			Destination = DeliveryInstructionDestination.DocManager;
			SendToEDocs = true;
			EmailSubjectOverride = emailSubjectOverride;
		}

		public ZString EmailSubjectOverride
		{
			get { return emailSubjectOverride; }
			set { emailSubjectOverride = value; }
		}
		ZString emailSubjectOverride;
	}
}
