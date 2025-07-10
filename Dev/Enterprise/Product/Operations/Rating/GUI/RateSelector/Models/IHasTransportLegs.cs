using System.Collections.ObjectModel;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	/// <summary>
	/// Serves as a common basis between <see cref="BookingInfoViewModel.TransportLegs"/> and <see cref="RateViewModel.TransportLegs"/>, both of which are bound to RoutesControl.cs
	/// </summary>
	public interface IHasTransportLegs
	{
		ObservableCollection<TransportLegViewModel> TransportLegs { get; }
	}
}
