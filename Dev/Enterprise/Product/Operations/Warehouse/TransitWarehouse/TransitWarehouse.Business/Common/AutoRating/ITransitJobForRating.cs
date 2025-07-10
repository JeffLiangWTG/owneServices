using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface ITransitJobForRating
	{
		WhsWarehouse Warehouse { get; }

		FreightMode? FreightMode { get; }

		JobHeader JobHeader { get; }

		ZDateTime ExpectedArrivalDate { get; }
		ZDateTime ExpectedDepartureDate { get; }

		string ChargeCodeGroup { get; }

		IEnumerable<TransitPackageForRatingInfo> TransitPackagesForRating { get; }
	}

	public interface ITransitConsignmentForRating : ITransitJobForRating
	{
		string ServiceLevel { get; }

		JobDocAddress ConsignorDocAddress { get; }
		JobDocAddress ConsigneeDocAddress { get; }
		JobDocAddress BookingPartyDocAddress { get; }
	}

	public interface ITransitTransportationUnitForRating : ITransitJobForRating
	{
		JobDocAddress TransportCompanyDocAddress { get; }

		KeyValuePair<PkgPackageContainer, bool>? ContainerForRating { get; }
	}
}
