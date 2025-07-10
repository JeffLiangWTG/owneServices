using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class DepartureMessageHeader : IDepartureMessageHeader
	{
		public DepartureMessageHeader(AsycudaManifestHeader header, AdditionalMessageInformation additionalMessageInformation)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.additionalMessageInformation = Argument.NotNull(additionalMessageInformation, nameof(additionalMessageInformation));
		}

		readonly AsycudaManifestHeader header;
		readonly AdditionalMessageInformation additionalMessageInformation;

		public ZString MessageType => Constants.AIMMessageSubTypes.FDM;

		public ZString Reference => header.AMA_MasterBill;

		public IAIMDeparture Departure => departure ?? (departure = new AIMDeparture(additionalMessageInformation));
		IAIMDeparture departure;
	}
}
