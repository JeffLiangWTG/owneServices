using System;
using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class TransportLegDateInfoViewModel : ViewModelWithNotificationBase
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public DateTime Date { get; set; }
	}
}
