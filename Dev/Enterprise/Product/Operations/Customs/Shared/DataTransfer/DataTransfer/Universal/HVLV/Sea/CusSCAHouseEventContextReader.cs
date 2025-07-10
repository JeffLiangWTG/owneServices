using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public class CusSCAHouseDataEventContextReader
	{
		public CusSCAHouseDataEventContextReader(BaseCusSCAHouse houseBill)
		{
			HouseBill = Argument.NotNull(houseBill, "houseBill");
		}

		internal void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			AddOceanBillEventContextValues(values);
			AddHouseEventContextValues(values);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual void AddOceanBillEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			var cusSCAOceanBillDataEventContextReader = new CusSCAOceanBillDataEventContextReader(HouseBill.OceanBill);
			cusSCAOceanBillDataEventContextReader.AddEventContextValues(values);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual void AddHouseEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			var helper = new EventContextValuesHelper(false, values);
			helper.AddHouseBillNumberAndPortCodes(HouseBill.CA_HouseBill, HouseBill._PortOfOrigin, HouseBill._PortOfDestination);
			helper.AddCustomsStatus(HouseBill.CA_ShipmentStatus);
		}

		protected BaseCusSCAHouse HouseBill { get; private set; }
	}
}
