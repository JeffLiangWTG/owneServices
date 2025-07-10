using System;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Carriers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Business.Carrier
{
	public class CarrierProcessor
	{
		public CarrierProcessor(
			ILogger logger,
			ILoader<CarrierData> carrierLoader,
			ILoader<VesselData> vesselLoader)
		{
			this.logger = logger;
			this.carrierLoader = carrierLoader;
			this.vesselLoader = vesselLoader;
		}

		readonly ILogger logger;
		readonly ILoader<CarrierData> carrierLoader;
		readonly ILoader<VesselData> vesselLoader;

		public void Run(string outputFolder)
		{
			var creationDate = DateTime.UtcNow;

			logger.LogInfo("Loading vessels");
			var vesselData = vesselLoader.LoadData();

			logger.LogInfo("Loading carriers");
			var carrierData = carrierLoader.LoadData();

			logger.LogInfo("Creating vessel xml file");
			var vesselBuilder = new VesselBuilder(vesselData, logger);
			vesselBuilder.CreateXmlFile(outputFolder, creationDate);

			logger.LogInfo("Creating carrier xml file");

			var carrierVesselData = new CarrierVesselData
			{
				PublicationDate = carrierData.PublicationDate,
				Data = carrierData.Data,
				Vessels = vesselData.Data
			};

			var carrierBuilder = new CarrierBuilder(carrierVesselData, logger);

			carrierBuilder.CreateXmlFile(outputFolder, creationDate);
			
		}
	}
}
