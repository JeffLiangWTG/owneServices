using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class NaladiShProgram : BaseCheckUpdateProgram
	{
		protected override string DataSourceFriendlyName => "Naladi Sh file";

		protected override string LogFileSuffix => Constants.NaladiShLogName;

		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = NaladiShDownloader.Download(client);
				return bFile;
			}
		}

		protected override void ExportToXMLFile(byte[] bFile)
		{
			throw new System.NotImplementedException();
		}
	}
}
