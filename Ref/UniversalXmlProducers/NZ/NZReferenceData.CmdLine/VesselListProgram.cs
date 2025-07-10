using System;
using System.IO;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.NZReferenceData.CmdLine
{
	public sealed class VesselListProgram : NZProgram
	{
		public override string ProgramName => "Vessel List";

		protected override void RunCore()
		{
			var url = ApplicationConfig.VesselListUrl;
			var localFilePath = Path.GetTempFileName();
			try
			{
				var httpClientHelper = new HttpClientHelper();
				VesselListFileDownloder.Download(httpClientHelper, url, localFilePath);
				VesselParser.ExportToXMLFile(localFilePath, ApplicationConfig.OutputDirectory, DateTime.Now);

				Logger.LogInfo($"Vessel records are generated completed!");
			}
			finally
			{
				if (File.Exists(localFilePath))
				{
					File.Delete(localFilePath);
				}
			}
		}
	}
}
