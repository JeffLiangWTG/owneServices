using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public class AsycudaUniversalEventMessagePinProcessor : ASYCUDA.Business.UniversalDataTransfer.AsycudaUniversalEventMessagePinProcessor
	{
		public AsycudaUniversalEventMessagePinProcessor(IXmlSessionTracker logger, Event universalEvent, ASYCUDA.Business.AsycudaEDIMessage message, AsycudaManifestHeader manifestHeader)
			: base(logger, universalEvent, message, manifestHeader)
		{
		}
		protected new AsycudaManifestHeader manifestHeader => (AsycudaManifestHeader)base.manifestHeader;

		protected override void ProcessCore()
		{
			new SGAsycudaUniversalEventMessageProcessor(universalEvent, manifestHeader).Process();
		}
	}
}
