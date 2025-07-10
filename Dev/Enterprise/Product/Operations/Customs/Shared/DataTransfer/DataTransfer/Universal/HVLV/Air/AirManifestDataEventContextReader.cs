using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest
{
	public class AirManifestDataEventContextReader
	{
		public AirManifestDataEventContextReader(CusMAWB mawb)
		{
			this.mawb = Argument.NotNull(mawb, "mawb");
		}

		internal protected static void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values, CusMAWB mawb)
		{
			var helper = new EventContextValuesHelper(true, values);
			helper.AddMasterBillNumberAndPortCodes(mawb.CM_MAWB, mawb.LoadPort, mawb.DischargePort);
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.MasterHouseBill, mawb.CM_MasterHouseBill);
		}

		internal void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			AddEventContextValuesCore(values);
		}

		protected readonly CusMAWB mawb;

		protected virtual void AddEventContextValuesCore(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			AddEventContextValues(values, mawb);
		}
	}
}
