using CargoWise.RefDbRepo.ZAReferenceData.Business.Carrier;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration;

namespace CargoWise.RefDbRepo.ZAReferenceData.CmdLine
{
	public class CarrierRunner : Runner
	{
		public CarrierRunner(ILogger logger) : base(logger)
		{
		}

		protected override void RunCore()
		{
			var baseUrl = ConfigurationProvider.CarrierBaseUrl;

			var vesselLoader = new VesselLoader(baseUrl);
			var carrierLoader = new CarrierLoader(baseUrl);
			var processor = new CarrierProcessor(Logger, carrierLoader, vesselLoader);

			processor.Run(ConfigurationProvider.OutputFolder);
		}
	}
}
