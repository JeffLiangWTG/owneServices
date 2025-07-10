using Enterprise.Customs.SG.MHUB;

namespace Enterprise.Customs.SG.Registry
{
	public sealed class MHUBSettingsProvider : IMHUBSettings
	{
		public string ClientVersion => SGCustomsDataRegistry.Instance.MhaccessVersionString.Value.EffectiveValueForToday;

		public string DigestForLibs => SGCustomsDataRegistry.Instance.DigestForLibs.Value.EffectiveValueForToday;

		public string EncryptionKey => SGCustomsDataRegistry.Instance.EncryptionKey.Value;

		public string EndpointPartialPathForDownload => SGCustomsDataRegistry.Instance.Mhx4EndpointPartialPathForDownload.Value;

		public string EndpointPartialPathForServlet => SGCustomsDataRegistry.Instance.Mhx4EndpointPartialPathForServlet.Value;

		public string EndpointPartialPathForUpload => SGCustomsDataRegistry.Instance.Mhx4EndpointPartialPathForUpload.Value;

		public string JreVersion => SGCustomsDataRegistry.Instance.JREVersion.Value.EffectiveValueForToday;

		public string RecipientID => SGCustomsDataRegistry.Instance.SendTestMessages.Value ? SGCustomsDataRegistry.Instance.RecipientIDTrial.Value : SGCustomsDataRegistry.Instance.RecipientIDLive.Value;

		public string SoapDownloadDirectory => SGCustomsDataRegistry.Instance.SoapDownloadDirectory.Value;

		public int SynchronousReadWriteTimeout => SGCustomsDataRegistry.Instance.SynchronousMHUBReadWriteTimeout.Value;

		public int SynchronousTimeout => SGCustomsDataRegistry.Instance.SynchronousMHUBTimeout.Value;

		public string TrustStore => SGCustomsDataRegistry.Instance.SendTestMessages.Value ? SGCustomsDataRegistry.Instance.MhaccessTrustStore_Trial.Value : SGCustomsDataRegistry.Instance.MhaccessTrustStore_Prod.Value;

		public bool UseDirectWebServicesInsteadOfScripting => SGCustomsDataRegistry.Instance.UseMhxDirectWebServicesInsteadOfScripting.Value;

		public string VendorID => SGCustomsDataRegistry.Instance.VendorID.Value;

		public string WebAddress => SGCustomsDataRegistry.Instance.SendTestMessages.Value ? SGCustomsDataRegistry.Instance.WebAddressTrial.Value.EffectiveValueForToday : SGCustomsDataRegistry.Instance.WebAddress.Value.EffectiveValueForToday;

		public string WebProxyAddress => SGCustomsDataRegistry.Instance.WebProxyAddress.Value;
	}
}
