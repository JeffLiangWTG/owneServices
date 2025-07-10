using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Freight.Integration;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Testing.GUITests.RateSelector
{
	sealed class BookingRateSelectorTest : TestCase
	{
		#region TestSelectRate

		public void TestReturnsSelectedRate()
		{
			var transportLeg1 = new Mock<IBookingTransportLeg>(MockBehavior.Loose);

			var rate1 = new Mock<IBookingRate>(MockBehavior.Loose);
			rate1.SetupGet(r => r.Amount).Returns(2000);
			rate1.SetupGet(r => r.Currency).Returns(Core.Constants.CurrencyCodes.EuropeanUnion);
			rate1.SetupGet(r => r.CarrierPrefix).Returns("074");
			rate1.SetupGet(r => r.Remarks).Returns("rate1");
			rate1.SetupGet(r => r.TransportLegs).Returns(new[] { transportLeg1.Object });

			var transportLeg2 = new Mock<IBookingTransportLeg>(MockBehavior.Loose);

			var rate2 = new Mock<IBookingRate>(MockBehavior.Loose);
			rate2.SetupGet(r => r.Amount).Returns(1250);
			rate2.SetupGet(r => r.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			rate2.SetupGet(r => r.CarrierPrefix).Returns("074");
			rate2.SetupGet(r => r.Remarks).Returns("rate2");
			rate2.SetupGet(r => r.TransportLegs).Returns(new[] { transportLeg2.Object });

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			ZFormModaliser.SetDelegateToCallOnFormShown(frm =>
			{
				if (frm is BookingRateSelectorForm selectorForm)
				{
					selectorForm.ViewModel.SelectedRate = selectorForm.ViewModel.Rates.First(r => r.Remarks.Equals("rate2"));
					BookButtonPerformClick(selectorForm);
				}
				else
				{
					Fail("Expected BookingRateSelector to show");
				}
			});

			var selector = new BookingRateSelector();
			var res = selector.SelectRate(new[]
			{
				rate1.Object,
				rate2.Object
			});

			AssertEquals("Rate was selected", rate2.Object, res);
		}

		public void TestReturnsNullWhenUserCancelled()
		{
			var transportLeg = new Mock<IBookingTransportLeg>(MockBehavior.Loose);

			var rate = new Mock<IBookingRate>(MockBehavior.Loose);
			rate.SetupGet(r => r.Amount).Returns(2000);
			rate.SetupGet(r => r.Currency).Returns(Core.Constants.CurrencyCodes.EuropeanUnion);
			rate.SetupGet(r => r.CarrierPrefix).Returns("074");
			rate.SetupGet(r => r.TransportLegs).Returns(new[] { transportLeg.Object });

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

			ZFormModaliser.SetDelegateToCallOnFormShown(frm =>
			{
				if (frm is BookingRateSelectorForm selectorForm)
				{
					selectorForm.ViewModel.SelectedRate = selectorForm.ViewModel.Rates[0];
				}
				else
				{
					Fail("Expected BookingRateSelector to show");
				}
			});

			var selector = new BookingRateSelector();
			var res = selector.SelectRate(new[] { rate.Object });

			AssertNull("No rate was selected as user cancelled", res);
		}

		#endregion

		#region TestCallWithEmptyOrNullRates

		public void TestCallWithEmptyOrNullRates()
		{
			var selector = new BookingRateSelector();

			AssertNull("No rate was selected", selector.SelectRate(null));
			AssertNull("No rate was selected", selector.SelectRate(Array.Empty<IBookingRate>()));
		}

		#endregion

		#region TestAirBookingRateSelectorCanBeAccessedThroughObjectFactory

		public void TestAirBookingRateSelectorCanBeAccessedThroughObjectFactory()
		{
			var selector = ObjectFactory.Get<IBookingRateSelector>();
			Assert("AirBookingRateSelector is accessible via ObjectFactory", selector is BookingRateSelector);
		}

		#endregion

		void BookButtonPerformClick(BookingRateSelectorForm selectorForm)
		{
			var toolStrip = (ZToolStrip)selectorForm.Controls.Find("zToolStrip", true).Single();
			var btnBook = toolStrip.Items.Find("btnBook", false).Single();
			btnBook.PerformClick();
		}
	}
}
