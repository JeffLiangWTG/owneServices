using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class PenaltyViewModel : ViewModelWithNotificationBase
	{
		public string Direction { get; set; }
		public string Type { get; set; }
		public string Name { get; set; }
		public decimal FreeTime { get; set; }
		public decimal PerUnitRate { get; set; }
		public string Currency { get; set; }
	}
}
