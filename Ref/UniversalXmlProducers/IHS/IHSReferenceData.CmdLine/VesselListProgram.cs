using System;
using CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel;
using CargoWise.RefDbRepo.IHSReferenceData.Services;
using CargoWise.RefDbRepo.IHSReferenceData.Services.Vessel;

namespace CargoWise.RefDbRepo.IHSReferenceData.CmdLine
{
	class VesselListProgram
	{
		public static void Run(string[] args)
		{
			var ftpUserName = args.Length != 0 ? args[0] : ApplicationConfig.VesselListFTPUser;
			var ftpPassword = args.Length > 1 ? args[1] : ApplicationConfig.VesselListFTPPassword;
			var ftpHost = ApplicationConfig.VesselListFTPHost;
			string targetFileName = ApplicationConfig.VesselListFileName;
			string flagCodesFileName = ApplicationConfig.FlagCodesFileName;

			var vesselsCsv = VesselListFileDownloader.DownloadAndExtractVesselFile(ftpHost, ftpUserName, ftpPassword, targetFileName, flagCodesFileName, out var publicationDateTime, out string flagCodeCsv);
			VesselParser.ExportToXMLFile(vesselsCsv, flagCodeCsv, ApplicationConfig.OutputPath, publicationDateTime);

			Console.WriteLine($"Vessel records are completed!");
		}
	}
}
