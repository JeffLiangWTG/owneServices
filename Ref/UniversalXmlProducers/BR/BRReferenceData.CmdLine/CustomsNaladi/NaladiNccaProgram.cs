using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class NaladiNccaProgram : BaseCheckUpdateProgram
	{
		protected override string DataSourceFriendlyName => "Naladi Ncca file";

		protected override string LogFileSuffix => Constants.NaladiNccaLogName;

		protected override byte[] DowloadContent()
		{
			using (var client = GetHttpClient())
			{
				var bFile = NaladiNccaDownloader.Download(client);
				return bFile;
			}
		}

		protected override void ExportToXMLFile(byte[] bFile)
		{
			throw new System.NotImplementedException();
		}
	}
}
