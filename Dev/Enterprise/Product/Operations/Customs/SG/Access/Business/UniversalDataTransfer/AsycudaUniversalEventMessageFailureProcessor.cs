using Enterprise.UniversalDataBuss.Integration;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class AsycudaUniversalEventMessageFailureProcessor : ASYCUDA.Business.UniversalDataTransfer.AsycudaUniversalEventMessageFailureProcessor
	{
		public AsycudaUniversalEventMessageFailureProcessor(IXmlSessionTracker logger, Event universalEvent, ASYCUDA.Business.AsycudaEDIMessage message, AsycudaManifestHeader manifestHeader)
			: base(logger, universalEvent, message, manifestHeader)
		{
		}

		protected new AsycudaManifestHeader manifestHeader => (AsycudaManifestHeader)base.manifestHeader;

		protected override void ProcessCore()
		{
			new SGAsycudaUniversalEventMessageFailureProcessor(universalEvent, manifestHeader).Process();
		}
	}
}
