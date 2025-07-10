using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector
{
	public sealed class BookingRateSelector : IBookingRateSelector
	{
		public IBookingRate SelectRate(IReadOnlyCollection<IBookingRate> rates)
		{
			if (rates == null || rates.Count == 0)
			{
				return null;
			}

			using (var selector = CreateBookingRatesSelector(rates))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(selector) == DialogResult.OK)
				{
					return selector.SelectedBookingRate;
				}
			}

			return null;
		}

		BookingRateSelectorForm CreateBookingRatesSelector(IReadOnlyCollection<IBookingRate> rates)
		{
			var factory = new BusinessObjectFactory();

			var viewModels = rates
				.Select(rate => BookingEngineRateViewModel.New(factory, rate))
				.ToArray();

			var bookingRatesViewModel = new BookingRatesViewModel(viewModels);
			return new BookingRateSelectorForm(bookingRatesViewModel);
		}
	}
}
