using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using EventContextTypes = Enterprise.UniversalDataBuss.DataObjects.Universal.Event.ContextTypes;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public sealed class AirManifestLineDataEventContextReader : DataTransfer.Universal.AirManifest.AirManifestLineDataEventContextReader
	{
		public AirManifestLineDataEventContextReader(CusHAWB hawb)
			: base(hawb)
		{
		}

		protected override void AddMAWBEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			var mawb = hawb.MAWB;
			var eventContextValuesHelper = new EventContextValuesHelper(isAir: true, values);
			eventContextValuesHelper.AddMasterBillNumberAndPortCodes(mawb.CM_MAWB, mawb.LoadPort, mawb.DischargePort);
		}

		protected override void AddHAWBEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			base.AddHAWBEventContextValues(values);
			values.AddIfNotEmpty(EventContextTypes.MasterHouseBill, hawb.CS_MasterHouseBill);
		}
	}
}
