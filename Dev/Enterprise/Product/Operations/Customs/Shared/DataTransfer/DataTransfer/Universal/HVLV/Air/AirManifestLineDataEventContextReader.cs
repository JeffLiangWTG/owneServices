using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class AirManifestLineDataEventContextReader
	{
		public AirManifestLineDataEventContextReader(CusHAWB hawb)
		{
			this.hawb = Argument.NotNull(hawb, "hawb");
		}

		internal void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			AddMAWBEventContextValues(values);
			AddHAWBEventContextValues(values);
		}

		protected virtual void AddMAWBEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			AirManifestDataEventContextReader.AddEventContextValues(values, hawb.MAWB);
		}

		protected virtual void AddHAWBEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			var helper = new EventContextValuesHelper(true, values);
			helper.AddHouseBillNumberAndPortCodes(hawb.CS_HAWB, hawb.Origin, hawb.Destination);
			helper.AddCustomsStatus(hawb.CS_CustomsStatus);
		}

		protected readonly CusHAWB hawb;
	}
}
