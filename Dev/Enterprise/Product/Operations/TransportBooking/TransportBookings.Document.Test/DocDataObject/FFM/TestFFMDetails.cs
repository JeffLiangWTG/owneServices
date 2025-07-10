using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportBookings.Document.Testing
{
	sealed class TestFFMDetails : IFFMDetails
	{
		public TestFFMDetails(BusinessObject dummy = null)
		{
			businessObject = dummy;
		}

		public ZString VoyageFlightNo { get; set; }
		public ZDateTime FlightDate { get; set; }
		public IRefUNLOCO AirportOfDestinationCode { get; set; }
		public IRefUNLOCO PortOfLoading { get; set; }
		public IList<IFFMPackageDetails> Packages { get; set; }
		public ZString RegulatedAgentID { get; set; }
		public ZString RegulatedAgentCountry { get; set; }
		public ZString SecurityStatusCode { get; set; }
		public ZString DriverDocumentID { get; set; }

		readonly BusinessObject businessObject;
		public BusinessObject BusinessObject => businessObject;
	}

	sealed class TestPackageDetails : IFFMPackageDetails
	{
		public ZString ContainerNumber { get; set; }
		public ZString WayBillNumber { get; set; }
		public IRefUNLOCO PortOfDestination { get; set; }
		public IRefUNLOCO PortOfLoading { get; set; }
		public ZString GoodsDescription { get; set; }
		public ZInt Quantity { get; set; }
		public ZDecimal Weight { get; set; }
		public ZDecimal Volume { get; set; }
		public ZString WeightMetric { get; set; }
		public ZString VolumeMetric { get; set; }
	}

	sealed class TestRefUNLOCO : IRefUNLOCO
	{
		public ZGuid PK { get; set; }
		public ZString RL_Code { get; set; }
		public ZString RL_PortName { get; set; }
		public ZString RL_IATA { get; set; }
		public ZString RL_IATARegionCode { get; set; }
		public ZString RL_RN_NKCountryCode { get; set; }
		public ZGuid RL_RW { get; set; }
		public ZDecimal StandardZoneUTCOffset { get; set; }
	}
}
