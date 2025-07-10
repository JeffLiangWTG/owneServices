using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.QuotedBookings.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Test
{
	public class ScheduleChooserControlExtensionsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSetControlVisibilityThrowsNoExceptionsOnEmptyBooking()
		{
			var mockIScheduleChooserControl = new Mock<IScheduleChooserControl>();

			mockIScheduleChooserControl.Object.SetControlVisibility(null);
			mockIScheduleChooserControl.Object.SetControlVisibility(QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory));
		}

		public void TestMAWBVisibility()
		{
			var control = new DummyScheduleChooserControl();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			booking.JS_IsDirectBooking = ZBool.True;
			booking.JS_TransportMode = Constants.TransportModes.Sea;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(control.ShowMAWBSeaNumberTextBox);
			Assert(!control.ShowHBLNumberTextBox);

			booking.JS_TransportMode = Constants.TransportModes.Air;
			control.SetControlVisibility(quotedBooking);
			Assert(control.ShowIsNeutral);
			Assert(control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(!control.ShowHBLNumberTextBox);

			booking.JS_TransportMode = Constants.TransportModes.Rail;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(control.ShowHBLNumberTextBox);

			booking.JS_TransportMode = Constants.TransportModes.Courier;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(control.ShowHBLNumberTextBox);

			booking.JS_TransportMode = Constants.TransportModes.Road;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(control.ShowHBLNumberTextBox);

			booking.JS_IsDirectBooking = ZBool.False;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(control.ShowHBLNumberTextBox);

			booking.JS_TransportMode = ZString.Empty;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(control.ShowHBLNumberTextBox);

			booking.JS_TransportMode = Constants.TransportModes.Rail;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(control.ShowHBLNumberTextBox);

			booking.JS_TransportMode = Constants.TransportModes.Courier;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(control.ShowHBLNumberTextBox);

			booking.JS_TransportMode = Constants.TransportModes.Road;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowIsNeutral);
			Assert(!control.ShowMAWBFields);
			Assert(!control.ShowMAWBSeaNumberTextBox);
			Assert(control.ShowHBLNumberTextBox);
		}

		public void TestCarrierDetails_And_TotalWeightAndVolumeVisibility_And_ImportOnlineSchedulesButton()
		{
			var control = new DummyScheduleChooserControl();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			booking.JS_TransportMode = Constants.TransportModes.Sea;
			control.TransportContainerMode = Constants.ContainerModes.FCL;
			control.SetControlVisibility(quotedBooking);
			Assert(control.ShowCarrierDetails);
			Assert(control.ShowCreditorDetails);
			Assert(!control.ShowScheduleTotalWeightAndVolume);
			Assert(control.ShowImportOnlineSchedulesButton);

			control.TransportContainerMode = Constants.ContainerModes.LCL;
			control.SetControlVisibility(quotedBooking);
			Assert(control.ShowCarrierDetails);
			Assert(control.ShowCreditorDetails);
			Assert(!control.ShowScheduleTotalWeightAndVolume);
			Assert(control.ShowImportOnlineSchedulesButton);

			control.TransportContainerMode = Constants.ContainerModes.Liquid;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowCarrierDetails);
			Assert(!control.ShowCreditorDetails);
			Assert(!control.ShowScheduleTotalWeightAndVolume);
			Assert(control.ShowImportOnlineSchedulesButton);

			control.TransportContainerMode = Constants.ContainerModes.OnBoardCourier;
			control.SetControlVisibility(quotedBooking);
			Assert(!control.ShowCarrierDetails);
			Assert(!control.ShowCreditorDetails);
			Assert(control.ShowScheduleTotalWeightAndVolume);
			Assert(control.ShowImportOnlineSchedulesButton);
		}

		public void TestImportOnlineSchedulesButton_OnlyVisibleForSeaFreight()
		{
			var control = new DummyScheduleChooserControl();
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);

			var transportContainerModes = new string[] {
				Core.Constants.RateMode.AIR,
				Core.Constants.RateMode.LSE,
				Core.Constants.RateMode.ULD
			};

			booking.JS_TransportMode = Constants.TransportModes.Air;
			booking.JS_PackingMode = Core.Constants.RateMode.FCL;
			CombineAssertions(delegate
			{
				foreach (string transportContainerMode in transportContainerModes)
				{
					control.TransportContainerMode = transportContainerMode;
					control.SetControlVisibility(quotedBooking);
					Assert("Precondition: JS_PackingMode is not yet updated and still has 'previous' value of FCL", booking.JS_PackingMode == "FCL");
					Assert(string.Format("No air freight should have Import Online Schedules button, including {0} container mode", transportContainerMode),
						!control.ShowImportOnlineSchedulesButton);
				}
			});

			transportContainerModes = new string[] {
				Core.Constants.RateMode.SEA,
				Core.Constants.RateMode.FCL,
				Core.Constants.RateMode.LCL,
			};

			booking.JS_TransportMode = Core.Constants.RateMode.SEA;
			booking.JS_PackingMode = Core.Constants.RateMode.LSE;
			CombineAssertions(delegate
			{
				foreach (string transportContainerMode in transportContainerModes)
				{
					control.TransportContainerMode = transportContainerMode;
					control.SetControlVisibility(quotedBooking);
					Assert("Precondition: JS_PackingMode is not yet updated and still has 'previous' value of LSE", booking.JS_PackingMode == "LSE");
					Assert(string.Format("All sea freight should have import online schedules button, including {0} container mode", transportContainerMode),
					control.ShowImportOnlineSchedulesButton);
				}
			});
		}

		class DummyScheduleChooserControl : IScheduleChooserControl
		{
			public bool ShowDirect { get; set; }
			public bool ShowIsNeutral { get; set; }
			public bool ShowServiceLevelFields { get; set; }
			public bool ShowMAWBFields { get; set; }
			public bool ShowMAWBSeaNumberTextBox { get; set; }
			public bool ShowHBLNumberTextBox { get; set; }
			public bool ShowCarrierDetails { get; set; }
			public bool ShowCreditorDetails { get; set; }
			public bool ShowScheduleTotalWeightAndVolume { get; set; }
			public bool ShowVesselBoundTextBox { get; set; }
			public bool ShowFCLCutOffReaoOnlyDateEdit { get; set; }
			public bool ShowImportOnlineSchedulesButton { get; set; }
			public ZString TransportContainerMode { get; set; }
		}
	}
}
