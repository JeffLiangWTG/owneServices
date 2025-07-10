using System;

namespace Enterprise.Freight.OnlineSailingSchedules.ServiceModel
{
	public class Leg
	{
		public Voyage Voyage { get; set; }
		public Port LoadPort { get; set; }
		public DateTime? Etd { get; set; }
		public Port DischargePort { get; set; }
		public DateTime? Eta { get; set; }
		public string LegType { get; set; } = GssConstants.SeaLegType;
		public string DepartureReference { get; set; }
		public string DepartureReferenceProvider { get; set; }
		public decimal? Co2eKgPerTeu { get; set; }
		public decimal? Co2eKgPerTonne { get; set; }
	}
}
