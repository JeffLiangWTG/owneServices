using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AirManifest;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class ETailAirManifestDataObjectReaderHelper : AirManifestDataObjectReaderHelper
	{
		public ETailAirManifestDataObjectReaderHelper(UniversalObjectFactory factory, ZString targetCountryCode, ZGuid shipmentPK, string dataProviderForCodeMapping = null)
			: base(factory, targetCountryCode, dataProviderForCodeMapping)
		{
			this.shipmentPK = shipmentPK;
		}

		readonly ZGuid shipmentPK;

		protected override bool ShouldMarkBillAsUnprocessed(CusHAWB hawb)
		{
			return hawb.ShouldMarkBillAsUnprocessed_HVLV(shipmentPK);
		}
	}
}
