using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public new void TestDefaultNumberOfDecimalsAttribute_WeightVolumeProperties()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			AssertDefaultNumberOfDecimalsAttribute_QuotedBookingOnAirTransport(quote.PK, ZGuid.Empty);
			AssertDefaultNumberOfDecimalsAttribute_QuotedBookingOnAirTransport(quote.PK, booking.PK, true);
			AssertDefaultNumberOfDecimalsAttribute_QuotedBookingOnAirTransport(ZGuid.Empty, booking.PK, true);
		}

		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight));
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.TransportMode = Core.Constants.TransportModes.Air;
			quotedBooking.WeightUnit = Core.Constants.Weight.Kilograms;
			quotedBooking.VolumeUnit = Core.Constants.Volume.CubicMetres;
			quotedBooking.Weight = 153.251m;
			quotedBooking.Volume = 2.137m;
			AssertEquals(153.251m, quotedBooking.Weight);
			AssertEquals(2.137m, quotedBooking.Volume);
			AssertEquals(356.167m, quotedBooking.Chargeable);
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			quotedBooking.TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(153.26m, quotedBooking.Weight);
			AssertEquals(2.13m, quotedBooking.Volume);
			AssertEquals("Chargeable is reset due to change in transport mode.", 2.13m, quotedBooking.Chargeable);
		}

		public void TestJS_QuoteBookingGenericOrderNumber()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "ORD1";
			booking.AttachedOrders.Add(order1);
			AssertEquals("Shipment order # should contain order number", "ORD1", quotedBooking.JS_QuoteBookingGenericOrderNumber);
			Order order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "ORD2";
			booking.AttachedOrders.Add(order2);
			AssertEquals("Shipment order # should contain both order numbers", "ORD1, ORD2", quotedBooking.JS_QuoteBookingGenericOrderNumber);
		}

		[ExpectNoExceptions]
		public void TestJS_QuoteBookingGenericOrderNumber_NoExceptionForQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			AssertEquals(string.Empty, quotedBooking.JS_QuoteBookingGenericOrderNumber);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
		}

		void AssertDefaultNumberOfDecimalsAttribute_QuotedBookingOnAirTransport(ZGuid quotePK, ZGuid bookingPK, bool withSailing = false)
		{
			quotedBooking = QuotedBooking.New(quotePK, bookingPK, Factory);
			quotedBooking.TransportMode = Core.Constants.TransportModes.Air;
			JobSailing sailingObj = null;

			if (withSailing)
			{
				var sailingHelper = new SailingsForTestClasses(Factory);
				sailingObj = sailingHelper.SydLaxSailing;
				sailingObj.Voyage.JV_VoyageFlight = "QF123";
				sailingObj.Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
				quotedBooking.Booking.JS_JX = sailingObj.PK;
			}

			foreach (var kvp in MeasurePropertiesAndUnits)
			{
				if (kvp.Value.StartsWith("ScheduleChooser+Sailing") && !withSailing)
				{
					continue;
				}

				var weightVolumeUnitInfo = BizObj.FindPropertyInfo(kvp.Value);
				var unitOfMeasureValue = (ZString)weightVolumeUnitInfo.Value;
				AssertDefaultNumberOfDecimalsAttribute(BizObj, kvp.Key.ToString(), unitOfMeasureValue);
			}
		}

		public override BusinessObject BizObj
		{
			get
			{
				return quotedBooking;
			}
		}

		QuotedBooking quotedBooking;
		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(QuotedBooking.Schema.Weight, QuotedBooking.Schema.WeightUnit);
					measurePropertiesAndUnits.Add(QuotedBooking.Schema.Volume, QuotedBooking.Schema.VolumeUnit);
					measurePropertiesAndUnits.Add(QuotedBooking.Schema.Chargeable, QuotedBooking.Schema.ChargeableUnit);
					measurePropertiesAndUnits.Add("ScheduleChooser+Sailing+TotalWeight", "ScheduleChooser+Sailing+TotalWeightUnit");
					measurePropertiesAndUnits.Add("ScheduleChooser+Sailing+TotalVolume", "ScheduleChooser+Sailing+TotalVolumeUnit");
				}

				return measurePropertiesAndUnits;
			}
		}

		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion
	}
}
