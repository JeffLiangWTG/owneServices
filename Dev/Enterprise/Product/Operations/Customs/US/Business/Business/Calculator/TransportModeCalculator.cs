using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public static class TransportModeCalculator
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString CalculateUSTransportMode(ZString transportMode, ZString containerMode)
		{
			var result = ZString.Empty;

			switch (transportMode)
			{
				case TransportTypeList.Codes.Air:
					if (!containerMode.IsEmpty)
					{
						result = IsContainerised(containerMode) ? TransportModeCodes.Codes.AirContainer : TransportModeCodes.Codes.AirNonContainer;
					}
					break;
				case TransportTypeList.Codes.Auto:
					result = TransportModeCodes.Codes.Auto;
					break;
				case TransportTypeList.Codes.BorderWaterBorne:
					result = TransportModeCodes.Codes.BorderWaterBorne;
					break;
				case TransportTypeList.Codes.FixedTransportInstallations:
					result = TransportModeCodes.Codes.FixedTransportInstallations;
					break;
				case TransportTypeList.Codes.Mail:
					result = TransportModeCodes.Codes.Mail;
					break;
				case TransportTypeList.Codes.PassengerHandCarried:
					result = TransportModeCodes.Codes.PassengerHandCarried;
					break;
				case TransportTypeList.Codes.Pedestrian:
					result = TransportModeCodes.Codes.Pedestrian;
					break;
				case TransportTypeList.Codes.Rail:
					if (!containerMode.IsEmpty)
					{
						result = IsContainerised(containerMode) ? TransportModeCodes.Codes.RailContainer : TransportModeCodes.Codes.RailNonContainer;
					}
					break;
				case TransportTypeList.Codes.Road:
					result = TransportModeCodes.Codes.RoadOther;
					break;
				case TransportTypeList.Codes.Sea:
					if (!containerMode.IsEmpty)
					{
						result = IsContainerised(containerMode) ? TransportModeCodes.Codes.VesselContainer : TransportModeCodes.Codes.VesselNonContainer;
					}
					break;
				case TransportTypeList.Codes.Truck:
					if (!containerMode.IsEmpty)
					{
						result = IsContainerised(containerMode) ? TransportModeCodes.Codes.TruckContainer : TransportModeCodes.Codes.TruckNonContainer;
					}
					break;
			}

			return result;
		}

		static bool IsContainerised(ZString containerMode) => Core.Constants.ContainerModes.IsContainerised(containerMode);
	}
}
