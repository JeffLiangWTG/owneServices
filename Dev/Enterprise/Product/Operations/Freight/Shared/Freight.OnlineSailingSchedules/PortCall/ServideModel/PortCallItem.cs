using System;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public class PortCallItem
	{
		public Vessel Vessel { get; set; }
		public Port Port { get; set; }
		public Carrier Carrier { get; set; }
		public string VoyageNumberIn { get; set; }
		public DateTime? Eta { get; set; }
		public DateTime? Ata { get; set; }
		public string ArrivalNumber { get; set; }
		public string VoyageNumberOut { get; set; }
		public DateTime? Etd { get; set; }
		public DateTime? Atd { get; set; }
		public string DepartureNumber { get; set; }
	}
}
