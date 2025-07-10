using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public sealed class DeliveryDueDateCalculationTestHelper
	{
		public static Mock<IDeliveryDueDateCalculatorManager> SetupDeliveryDueDateCalculatorManagerMockWithoutSubstitution(ZDateTime mockDeliveryDueDate)
		{
			var deliveryDueDateCalculatorManagerMock = new Mock<IDeliveryDueDateCalculatorManager>();
			deliveryDueDateCalculatorManagerMock.Setup(m => m.Calculate(It.IsAny<ForwardingShipment>())).Returns(DeliveryDueDateCalculationResult.Success(mockDeliveryDueDate, ZString.Empty));
			return deliveryDueDateCalculatorManagerMock;
		}

		public static Mock<IDeliveryDueDateCalculatorManager> SetupDeliveryDueDateCalculatorManagerMock(ZDateTime mockDeliveryDueDate)
		{
			var deliveryDueDateCalculatorManagerMock = SetupDeliveryDueDateCalculatorManagerMockWithoutSubstitution(mockDeliveryDueDate);
			ObjectFactory.Substitute(deliveryDueDateCalculatorManagerMock.Object);
			return deliveryDueDateCalculatorManagerMock;
		}

		public static OrgTimetable AddTimetable(OrgAddress orgAddress, string type, int fromHour = 0, int fromMinute = 0, int toHour = 0, int toMinute = 0, bool isAdvanced = false, string advancedDay = "", int processingTime = 0, DateTime cutOffTime = default)
		{
			var timetable = orgAddress.Timetables.AddNew();
			timetable.OTT_Type = type;
			timetable.OTT_TimeFrom = new DateTime(2022, 1, 1, fromHour, fromMinute, 0);
			timetable.OTT_TimeTo = new DateTime(2022, 1, 1, toHour, toMinute, 0);
			timetable.OTT_ProcessingTimeInMinutes = processingTime;
			timetable.OTT_CutOffTime = cutOffTime != default ? new ZDateTime(cutOffTime) : ZDateTime.Empty;

			if (isAdvanced)
			{
				timetable.DayOfWeek = advancedDay;
			}

			return timetable;
		}

		public static OrgAddress SetupAddress(OrgHeader org, string address, string postCode, string country, string state, string city, string unloco)
		{
			var orgAddress = org.Addresses.AddNew();
			orgAddress.Address1 = address;
			orgAddress.Postcode = postCode;
			orgAddress.OA_RN_NKCountryCode = country;
			orgAddress.OA_City = city;
			orgAddress.OA_State = state;
			orgAddress.OA_RL_NKRelatedPortCode = unloco;
			return orgAddress;
		}

		public static OrgHeader SetupOrgWithAddress(BusinessObjectFactory factory, string orgCode, string address, string postCode, string country, string state, string city, string unloco)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;
			SetupAddress(org, address, postCode, country, state, city, unloco);
			return org;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static RateTransportZone SetupTransportZone(
			BusinessObjectFactory factory,
			OrgHeader orgTransitTimeZoneOwner,
			string fromPostCode,
			string toPostCode,
			string country,
			int beyondHours = 0,
			string providerType = "ALL")
		{
			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_ZoneType = providerType;
			provider.TP_OH_RelatedParty = orgTransitTimeZoneOwner.PK;
			provider.TP_IsActive = true;
			provider.TP_RN_NKCountry = country;

			var zone = provider.Zones.AddNew();
			zone.TZ_IsActive = true;

			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_FromPostCode = fromPostCode;
			zoneItem.TQ_ToPostCode = toPostCode;
			zoneItem.TQ_RN_NKCountry = country;
			zoneItem.TQ_IsBeyond = true;
			zoneItem.TQ_BeyondHours = beyondHours;

			return zone;
		}

		public static RateTransportZone SetupTransportZone(
			BusinessObjectFactory factory,
			OrgHeader orgTransitTimeZoneOwner,
			string country,
			string providerType = "ALL")
		{
			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_ZoneType = providerType;
			provider.TP_OH_RelatedParty = orgTransitTimeZoneOwner.PK;
			provider.TP_IsActive = true;
			provider.TP_RN_NKCountry = country;

			var zone = provider.Zones.AddNew();
			zone.TZ_IsActive = true;

			return zone;
		}

		public static RateTransportZone SetupTransportZoneWithName(
			BusinessObjectFactory factory,
			OrgHeader orgTransitTimeZoneOwner,
			string country,
			string providerType = "ALL",
			string zoneName = "")
		{
			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_ZoneType = providerType;
			provider.TP_OH_RelatedParty = orgTransitTimeZoneOwner.PK;
			provider.TP_IsActive = true;
			provider.TP_RN_NKCountry = country;

			var zone = provider.Zones.AddNew();
			zone.TZ_IsActive = true;
			zone.TZ_ZoneName = zoneName;

			return zone;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static RefTransitTime SetupTransitTime(
			BusinessObjectFactory factory,
			ZGuid originZonePK,
			ZGuid destinationZonePK,
			ZString serviceLevel,
			ZString mode,
			int totalTransitHours)
		{
			var transitTime = factory.New<RefTransitTime>();
			transitTime.RTT_TZ_OriginDomesticZone = originZonePK;
			transitTime.RTT_TZ_DestinationDomesticZone = destinationZonePK;
			transitTime.RTT_RS_NKServiceLevel = serviceLevel;
			transitTime.RTT_Mode = mode;
			for (int i = 1; i <= 7; i++)
			{
				var transitTimeDetail = transitTime.RefTransitTimeDetails.AddNew();
				transitTimeDetail.RTD_TransitHours = totalTransitHours;
				transitTimeDetail.RTD_DayOfWeek = (ZByte)i;
			}
			return transitTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static RefTransitTimeDetail SetupTransitTimeDetails(
			BusinessObjectFactory factory,
			RefTransitTime transitTime,
			ZDateTimeOffset endDate,
			ZDateTimeOffset effectiveDate,
			ZInt transitHours,
			ZByte dayOfWeek,
			ZDateTime arrivalTime)
		{
			var transitTimeDetail = transitTime.RefTransitTimeDetails.Cast<RefTransitTimeDetail>().FirstOrDefault(x => x.RTD_DayOfWeek == dayOfWeek);
			if (transitTimeDetail == null)
			{
				transitTimeDetail = transitTime.RefTransitTimeDetails.AddNew();
				transitTimeDetail.RTD_DayOfWeek = dayOfWeek;
			}
			transitTimeDetail.RTD_ArrivalTime = arrivalTime;
			transitTimeDetail.RTD_TransitHours = transitHours;
			transitTimeDetail.RTD_EffectiveDate = effectiveDate;
			transitTimeDetail.RTD_EndDate = endDate;
			return transitTimeDetail;
		}

		public static RefServiceLevel GetOrCreateRefServiceLevelIfNotExist(BusinessObjectFactory factory, string serviceLevel)
		{
			var result = factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, serviceLevel);
			if (result == null)
			{
				result = factory.NewWithValidTestData<RefServiceLevel>();
				result.RS_Code = serviceLevel;
			}

			return result;
		}

		public static void SetProcessingTime(OrgAddress orgAddress, ZInt processingTime, string type = null)
		{
			SetTimeTableProperty(orgAddress, tt => tt.OTT_ProcessingTimeInMinutes = processingTime, type);
		}

		public static void SetCutOffTime(OrgAddress orgAddress, ZDateTime cutOffTime, string type = null)
		{
			SetTimeTableProperty(orgAddress, tt => tt.OTT_CutOffTime = cutOffTime, type);
		}

		static void SetTimeTableProperty(OrgAddress orgAddress, Action<OrgTimetable> action, string type = null)
		{
			foreach (var timetable in orgAddress.Timetables)
			{
				if (type != null && !timetable.OTT_Type.EqualsIgnoringCase(type))
				{
					continue;
				}

				action(timetable);
			}
		}
	}
}
