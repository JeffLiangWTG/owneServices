using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTransitTimeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckServiceLevel()
		{
			var transitTime = Factory.New<RefTransitTime>();
			transitTime.RTT_RS_NKServiceLevel = ZString.Empty;
			AssertHasError(transitTime.RTT_RS_NKServiceLevelInfo, "Please enter a Service Level.");

			transitTime.RTT_RS_NKServiceLevel = "STD";
			AssertNoErrors(transitTime.RTT_RS_NKServiceLevelInfo);

			transitTime.RTT_RS_NKServiceLevel = "XZZ";
			AssertHasError(transitTime.RTT_RS_NKServiceLevelInfo, "Enter a valid Service Level.");
		}

		public void TestCheckServiceLevelAndMode_DuplicateTransitTime()
		{
			var auZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC"));
			var usZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC"));

			var transitTime1 = Factory.New<RefTransitTime>();
			transitTime1.RTT_Mode = "AIR";
			transitTime1.RTT_RS_NKServiceLevel = "STD";
			transitTime1.RTT_FZ_OriginInternationalZone = auZone.PK;
			transitTime1.RTT_FZ_DestinationInternationalZone = usZone.PK;
			transitTime1.RTT_TransitHours = 20;

			AssertNoError(transitTime1.RTT_RS_NKServiceLevelInfo, "Transit Time for this Zone combination has already been configured for this Service Level.");

			Factory.Save();

			var transitTime2 = new BusinessObjectFactory().New<RefTransitTime>();
			transitTime2.RTT_Mode = "AIR";
			transitTime2.RTT_RS_NKServiceLevel = "STD";
			transitTime2.RTT_FZ_OriginInternationalZone = auZone.PK;
			transitTime2.RTT_FZ_DestinationInternationalZone = usZone.PK;

			transitTime2.Validation.ValidateRTT_RS_NKServiceLevel();
			AssertHasError(transitTime2.RTT_RS_NKServiceLevelInfo, "Transit Time for this Zone combination has already been configured for this Service Level.");

			transitTime2.RTT_RS_NKServiceLevel = "D2D";
			AssertNoError(transitTime2.RTT_RS_NKServiceLevelInfo, "Transit Time for this Zone combination has already been configured for this Service Level.");

			transitTime2.RTT_Mode = "SEA";
			transitTime2.RTT_RS_NKServiceLevel = "STD";
			AssertNoError(transitTime2.RTT_RS_NKServiceLevelInfo, "Transit Time for this Zone combination has already been configured for this Service Level.");
		}

		public void TestCheckServiceLevel_WeekendDeliveryServiceLevel()
		{
			var auZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC"));
			var usZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC"));

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "WKE";
			serviceLevel.RS_DeliverOnSunday = true;

			Factory.Save();

			var transitTime = Factory.New<RefTransitTime>();
			transitTime.RTT_Mode = Core.Constants.TransportModes.Air;
			transitTime.RTT_RS_NKServiceLevel = "WKE";
			transitTime.RTT_FZ_OriginInternationalZone = auZone.PK;
			transitTime.RTT_FZ_DestinationInternationalZone = usZone.PK;
			transitTime.RTT_TransitHours = 20;

			Factory.Save();

			transitTime.Validation.ValidateRTT_RS_NKServiceLevel();
			AssertHasError(transitTime.RTT_RS_NKServiceLevelInfo, "Weekend Delivery Service Level cannot be used for Transit Time.");
		}

		public void TestCheckTransitDays()
		{
			CheckTransitDays(false);
			CheckTransitDays(true);
		}

		void CheckTransitDays(bool isRegistry)
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = isRegistry, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var transitTime = Factory.New<RefTransitTime>();

				AssertNoErrors(transitTime.TransitDaysInfo);

				transitTime.TransitDays = -5;
				AssertHasError(transitTime.TransitDaysInfo, "Days cannot be negative.");

				transitTime.TransitDays = 5;
				AssertNoErrors(transitTime.TransitDaysInfo);

				transitTime.TransitHours = 0;
				transitTime.TransitDays = 0;
				AssertErrorCheck(!isRegistry, transitTime.TransitDaysInfo);
			}
		}

		public void TestCheckTransitHours()
		{
			CheckTransitHours(false);
			CheckTransitHours(true);
		}

		void CheckTransitHours(bool isRegistry)
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = isRegistry, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var transitTime = Factory.New<RefTransitTime>();
				AssertNoErrors(transitTime.TransitHoursInfo);

				transitTime.TransitHours = -5;
				AssertHasError(transitTime.TransitHoursInfo, "Hours cannot be negative.");

				transitTime.TransitHours = 5;
				AssertNoErrors(transitTime.TransitHoursInfo);

				transitTime.TransitHours = 25;
				AssertHasError(transitTime.TransitHoursInfo, "Hours must be less than 24. Please use the 'Days' component for longer times.");

				transitTime.TransitHours = 23;
				AssertNoErrors(transitTime.TransitHoursInfo);

				transitTime.TransitDays = 0;
				transitTime.TransitHours = 0;
				AssertErrorCheck(!isRegistry, transitTime.TransitHoursInfo);
			}
		}

		public void TestCheckMode()
		{
			var transitTime = Factory.New<RefTransitTime>();
			transitTime.RTT_Mode = ZString.Empty;
			AssertHasError(transitTime.RTT_ModeInfo, "Please enter a Mode.");

			transitTime.RTT_Mode = "AIR";
			AssertNoErrors(transitTime.RTT_ModeInfo);

			transitTime.RTT_Mode = "XZZ";
			AssertHasError(transitTime.RTT_ModeInfo, "Enter a valid Mode.");
		}

		void AssertErrorCheck(bool isError, ZPropertyInfo zPropertyInfo)
		{
			if (isError)
			{
				AssertHasError(zPropertyInfo, "Please specify a Transit time greater than zero.");
			}
			else
			{
				AssertNoErrors(zPropertyInfo);
			}
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}
	}
}
