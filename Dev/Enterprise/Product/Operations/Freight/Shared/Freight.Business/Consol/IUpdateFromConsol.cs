using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IUpdateFromConsol
	{
		ZString MAWBNumber { set; }
		ZString FlightNumber { set; }
		ZDateTime ArrivalDate { set; }
		ZDateTime DepartureDate { set; }
		ZString LoadPort { set; }
		ZString DischargePort { set; }
		bool IsAir { get; set; }
		void UpdateLoadPort(Transport transport);
		void UpdateDischargePort(Transport transport);
	}
}
