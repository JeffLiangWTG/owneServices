using System;
using System.ServiceModel;

using CargoWise.Services.Common;
using Enterprise.Freight.DistanceCalculation.Integration;

namespace Enterprise.Freight.DistanceCalculation.Service
{
	[System.Xml.Serialization.XmlSerializerAssemblyAttribute("Enterprise.Freight.DistanceCalculation.XmlSerializers")]
	public class DistanceCalculationService : IDistanceCalculationService
	{
		public DistanceCalculationResult Calculate(ServiceRequestConfigurationData ServiceConfiguration, DistanceCalculationConfiguration DistanceCalculationConfiguration, DistanceCalculationAddress OriginAddress, DistanceCalculationAddress DestinationAddress)
		{
			DistanceCalculationResult result = new DistanceCalculationResult();
			if (OriginAddress.IsEmpty || DestinationAddress.IsEmpty)
			{
				result.StatusMessage = "Error - Origin or Destination address is empty.";
				return result;
			}

			if (string.IsNullOrEmpty(DistanceCalculationConfiguration.ProviderCode))
			{
				DistanceCalculationConfiguration.ProviderCode = DistanceCalculationConstants.Providers.Google;
			}

			if (DistanceCalculationConfiguration.ProviderCode == DistanceCalculationConstants.Providers.PCMiler)
			{
				PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();
				result = service.Process(DistanceCalculationConfiguration, OriginAddress, DestinationAddress);
				RequestAuditLogger.CreateRequestAudit(ServiceConfiguration, TransactionTypes.DistanceCalculation.Code, TransactionTypes.DistanceCalculation.SubTypes.PCMiler);
			}
			else if (DistanceCalculationConfiguration.ProviderCode == DistanceCalculationConstants.Providers.Google ||
					DistanceCalculationConfiguration.ProviderCode == DistanceCalculationConstants.Providers.CargoWise)
			{
				GoogleDistanceCalculationService service = new GoogleDistanceCalculationService();
				result = service.Process(OriginAddress, DestinationAddress);
				RequestAuditLogger.CreateRequestAudit(ServiceConfiguration, TransactionTypes.DistanceCalculation.Code, TransactionTypes.DistanceCalculation.SubTypes.GoogleProvider);
			}
			else
			{
				result.StatusMessage = "Error - unknown ProviderCode - " + DistanceCalculationConfiguration.ProviderCode;
			}

			return result;
		}
	}
}
