using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class BookingTermViewModel : ViewModelWithNotificationBase
	{
		public string Name { get; set; }
		public string Fee { get; set; }
		public string Currency { get; set; }
	}
}
