using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public class CusSCAOceanBillDataEventContextReader
	{
		public CusSCAOceanBillDataEventContextReader(BaseCusSCAOceanBill oceanBill)
		{
			OceanBill = Argument.NotNull(oceanBill, "oceanBill");
		}

		internal void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			AddEventContextValuesCore(values);
		}

		protected virtual void AddEventContextValuesCore(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			new EventContextValuesHelper(false, values).AddMasterBillNumberAndPortCodes(
				OceanBill.CB_OceanBill,
				OceanBill.PortOfLoading,
				OceanBill.PortOfDischarge);
			var vessel = OceanBill.VesselName;
			if (vessel != null)
			{
				values.AddIfNotEmpty(Event.ContextTypes.VesselName, OceanBill.VesselName.RV_Code);
			}
			values.AddIfNotEmpty(Event.ContextTypes.LloydsNumber, OceanBill.CB_LloydsIMO);
			values.AddIfNotEmpty(Event.ContextTypes.VoyageNumber, OceanBill.CB_Voyage);
			values.AddIfNotEmpty(Event.ContextTypes.MasterHouseBill, OceanBill.CB_MasterHouseBill);
		}

		protected BaseCusSCAOceanBill OceanBill { get; private set; }
	}
}
