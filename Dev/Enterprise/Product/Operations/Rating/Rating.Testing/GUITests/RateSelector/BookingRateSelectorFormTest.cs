using System;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Testing.GUITests.RateSelector
{
	[TestedType(typeof(BookingRateSelectorForm))]
	public class BookingRateSelectorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BookingRateSelectorForm(new BookingRatesViewModel(Array.Empty<BookingEngineRateViewModel>()));
		}
	}
}
