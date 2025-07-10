using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class CBPManifestMessageProvider : ICBPManifestMessage
	{
		public CBPManifestMessageProvider(USExportAsycudaBill bill, string action)
		{
			CBPManifestMessage = new ManifestFilingProvider(bill, action);
		}

		public IManifestFiling CBPManifestMessage { get; }
	}
}
