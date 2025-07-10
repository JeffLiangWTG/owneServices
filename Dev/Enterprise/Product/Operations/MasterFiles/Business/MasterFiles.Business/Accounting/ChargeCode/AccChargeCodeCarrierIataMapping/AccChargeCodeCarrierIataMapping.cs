using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeCodeCarrierIataMapping : AutoAccChargeCodeCarrierIataMapping
	{
		public AccChargeCodeCarrierIataMapping(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region AirLineName

		public ZString AirLineName
		{
			get
			{
				if (!airLineName.HasValue)
				{
					airLineName = (Carrier != null && Carrier.MiscServ != null && Carrier.MiscServ.IsAirline && Carrier.MiscServ.Airline != null)
						? Carrier.MiscServ.Airline.RM_AirlineName1 : ZString.Empty;
				}

				return airLineName.Value;
			}
		}

		ZString? airLineName;

		#endregion

		#region AirLine2CharCode

		public ZString AirLine2CharCode
		{
			get
			{
				if (!airLine2CharCode.HasValue)
				{
					airLine2CharCode = (Carrier != null && Carrier.MiscServ != null && Carrier.MiscServ.IsAirline && Carrier.MiscServ.Airline != null)
						? Carrier.MiscServ.Airline.RM_TwoCharacterCode : ZString.Empty;
				}

				return airLine2CharCode.Value;
			}
		}

		ZString? airLine2CharCode;

		#endregion

		#region ACI_IATAChargeCodeMap

		[List("Lookups.ACI_IATAChargeCodeMap_List")]
		public override ZString ACI_IATAChargeCodeMap
		{
			get => base.ACI_IATAChargeCodeMap;
			set => base.ACI_IATAChargeCodeMap = value;
		}

		#endregion

		#region SupportsClone

		protected override bool SupportsCloneCore() => true;

		#endregion
	}
}
