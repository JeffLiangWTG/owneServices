using System;
using System.ServiceModel;

using CargoWise.Services.Common;
using Enterprise.Freight.DistanceCalculation.Integration;

namespace Enterprise.Freight.DistanceCalculation.Service
{
	[ServiceContract(Namespace = "http://www.cargowise.com")]
	public interface IDistanceCalculationService
	{
		[OperationContract]
		DistanceCalculationResult Calculate(ServiceRequestConfigurationData ServiceConfiguration, DistanceCalculationConfiguration DistanceCalculationConfiguration, DistanceCalculationAddress OriginAddress, DistanceCalculationAddress DestinationAddress);
	}
}
