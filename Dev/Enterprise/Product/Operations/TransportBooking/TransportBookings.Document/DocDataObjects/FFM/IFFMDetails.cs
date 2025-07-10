using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportBookings.Document
{
	public interface IFFMDetails
	{
		BusinessObject BusinessObject { get; }

		public ZString VoyageFlightNo { get; }
		public ZDateTime FlightDate { get; }
		public IRefUNLOCO AirportOfDestinationCode { get; }
		public IRefUNLOCO PortOfLoading { get; }
		public IList<IFFMPackageDetails> Packages { get; }
		public ZString RegulatedAgentID { get; }
		public ZString RegulatedAgentCountry { get; }
		public ZString DriverDocumentID { get; }
		public ZString SecurityStatusCode { get; }
	}

	public interface IFFMPackageDetails
	{
		public ZString ContainerNumber { get; }
		public ZString WayBillNumber { get; }
		public IRefUNLOCO PortOfDestination { get; }
		public IRefUNLOCO PortOfLoading { get; }
		public ZString GoodsDescription { get; }

		public ZInt Quantity { get; }
		public ZDecimal Weight { get; }
		public ZDecimal Volume { get; }

		public ZString WeightMetric { get; }
		public ZString VolumeMetric { get; }
	}
}
