using System;

namespace Enterprise.Freight.DistanceCalculation.Integration
{
	[Serializable]
	public class DistanceCalculationResult
	{
		public DistanceCalculationResult()
		{
			this.StatusMessage = "";
		}
		
		public double Distance { get; set; }
		public string DistanceUnit { get; set; }
		public double TravelTime { get; set; }
		public string StatusMessage { get; set; }
	}
}
