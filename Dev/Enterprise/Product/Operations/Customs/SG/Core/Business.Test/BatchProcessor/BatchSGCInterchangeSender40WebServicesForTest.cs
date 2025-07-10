using Enterprise.Customs.SG.MHUB.Mhx4Soap;
using Enterprise.Customs.SG.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class BatchSGCInterchangeSender40WebServicesForTest : BatchSGCInterchangeSender40WebServices
	{
		protected override MHAccessClient GetIgorsClient()
		{
			return new SG.MHUB.Mhx4Soap.Testing.MHAccessClientForTest("djc", "xyz", new MHUBSettingsProvider(), Logger, pukeOnLoginForTest: true);
		}

		public void UploadOneFileExposed(string fileName, string remoteFile, EDIInterchange interchange)
		{
			base.UploadOneFile(fileName, remoteFile, interchange);
		}

		public string PackageAttachmentInterchangeExtend(EDIInterchange interchange, string outputDirectory, string fileNameNaked)
		{
			return base.PackageAttachmentInterchange(interchange, outputDirectory, fileNameNaked);
		}
	}
}
