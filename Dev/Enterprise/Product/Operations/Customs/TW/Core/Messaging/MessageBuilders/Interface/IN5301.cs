using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IN5301Declaration
	{
		ZString ID { get; }

		ZDecimal TotalGrossMassMeasure { get; }

		ZInt TotalPackageQuantity { get; }

		ZString TypeCode { get; }

		IEnumerable<IAdditionalInformation> AdditionalInformations { get; }

		IPartyDetails Agent { get; }

		ITransportMeans BorderTransportMeans { get; }

		IPartyDetails Carrier { get; }

		IConsignment Consignment { get; }

		IPartyDetails Deconsolidator { get; }

		ZString LoadingLocation { get; }

		ZString RepresentativePersonName { get; }

		IPartyDetails Applicant { get; }

		ZString UnloadingLocation { get; }
	}
}
