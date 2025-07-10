using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class CusTransport : ICusTransport
	{
		public CusTransport(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		public ZString VoyageFlightNumber => header.TSS_VoyageFlight;

		public ZString TranportMode => header.AMA_TransportMode;

		OrgCusCode CarrierCusCode => header.TSS_CargoCarrier?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, header.AMA_RN_NKCountry);

		public ZString CarrierCode => CarrierCusCode?.OK_CustomsRegNo ?? ZString.Empty;

		public ZString CarrierName => CarrierCusCode?.CompanyCodeAndPremisesAddresses ?? ZString.Empty;

		public ZString CallSign => header.TSS_RadioCallSign;

		public ZDateTime ETA => header.AMA_E_ARV;

		public ZDateTime ETD => header.TSS_DateOfDeparture;
	}
}
