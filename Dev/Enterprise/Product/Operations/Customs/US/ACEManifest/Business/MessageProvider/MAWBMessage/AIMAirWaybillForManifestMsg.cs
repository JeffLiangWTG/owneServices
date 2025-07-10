using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMAirWaybillForManifestMsg : IAIMAirWaybill
	{
		public AIMAirWaybillForManifestMsg(AsycudaManifestHeader header, AdditionalMessageInformation additionalMessageInformation)
		{
			this.header = Argument.NotNull(header, "AsycudaManifestHeader");
			this.additionalMessageInformation = Argument.NotNull(additionalMessageInformation, "additionalMessageInformation");
		}

		readonly AsycudaManifestHeader header;
		readonly AdditionalMessageInformation additionalMessageInformation;

		public ZString AirWaybillPrefix => GetFormattedMAWB().Left(3);
		public ZString AWBSerialNumber => GetFormattedMAWB().SubstringSafe(3, 8);
		public ZBool IsMasterAirWaybill => additionalMessageInformation.AM_IsConsolidation;
		public ZString HAWBNumber => ZString.Empty;
		public ZString PackageTrackingIdentifier => ZString.Empty;
		public ZString PartArrivalReference => ZString.Empty;

		ZString GetFormattedMAWB()
		{
			return header.AMA_MasterBill.KeepAlphanumericCharacters();
		}
	}
}
