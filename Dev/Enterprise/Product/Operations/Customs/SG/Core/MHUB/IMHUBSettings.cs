namespace Enterprise.Customs.SG.MHUB
{
	public interface IMHUBSettings
	{
		string ClientVersion { get; }
		string DigestForLibs { get; }
		string EncryptionKey { get; }
		string EndpointPartialPathForDownload { get; }
		string EndpointPartialPathForServlet { get; }
		string EndpointPartialPathForUpload { get; }
		string JreVersion { get; }
		string RecipientID { get; }
		string SoapDownloadDirectory { get; }
		int SynchronousReadWriteTimeout { get; }
		int SynchronousTimeout { get; }
		string TrustStore { get; }
		bool UseDirectWebServicesInsteadOfScripting { get; }
		string VendorID { get; }
		string WebAddress { get; }
		string WebProxyAddress { get; }
	}
}
