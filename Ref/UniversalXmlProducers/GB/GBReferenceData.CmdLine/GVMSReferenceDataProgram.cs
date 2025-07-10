using System;
using CargoWise.RefDbRepo.GBReferenceData.Business.GvmsReferenceData;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	class GVMSReferenceDataProgram
	{
		public static void Run()
		{
			var builder = new GvmsReferenceDataBuilder();
			var referenceData = new GVMSReferenceDataClient().GetReferenceData(ConfigurationProvider.GvmsReferenceDataUrl);
			builder.BuildXml(ConfigurationProvider.OutputDirectory, referenceData, DateTime.Now);
		}
	}
}
