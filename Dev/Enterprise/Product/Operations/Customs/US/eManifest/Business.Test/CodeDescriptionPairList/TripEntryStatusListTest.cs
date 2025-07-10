using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TripEntryStatusListTest : TestCase
	{
		public void TestGetStatusFromNotificationCode()
		{
			AssertEquals(TripEntryStatusList.Codes.TripArrived, TripEntryStatusList.GetStatusFromNotificationCode("SN030"));
			AssertEquals(TripEntryStatusList.Codes.HoldCrew, TripEntryStatusList.GetStatusFromNotificationCode("SN031"));
			AssertEquals(TripEntryStatusList.Codes.ReleaseCrew, TripEntryStatusList.GetStatusFromNotificationCode("SN032"));
			AssertEquals(TripEntryStatusList.Codes.HoldEquipment, TripEntryStatusList.GetStatusFromNotificationCode("SN033"));
			AssertEquals(TripEntryStatusList.Codes.ReleaseEquipment, TripEntryStatusList.GetStatusFromNotificationCode("SN034"));
			AssertEquals(TripEntryStatusList.Codes.HoldConveyance, TripEntryStatusList.GetStatusFromNotificationCode("SN035"));
			AssertEquals(TripEntryStatusList.Codes.ReleaseConveyance, TripEntryStatusList.GetStatusFromNotificationCode("SN036"));
			AssertEquals(TripEntryStatusList.Codes.HoldTrip, TripEntryStatusList.GetStatusFromNotificationCode("SN037"));
			AssertEquals(TripEntryStatusList.Codes.ReleaseTrip, TripEntryStatusList.GetStatusFromNotificationCode("SN038"));
			AssertEquals(TripEntryStatusList.Codes.CarrierHasNoValidOperatingAuthority, TripEntryStatusList.GetStatusFromNotificationCode("SN039"));
			AssertEquals(TripEntryStatusList.Codes.CarrierHasAnOssOrderOnIt, TripEntryStatusList.GetStatusFromNotificationCode("SN040"));
			AssertEquals(TripEntryStatusList.Codes.CarriersCurrentInsuranceLessThanMinimumLevel, TripEntryStatusList.GetStatusFromNotificationCode("SN041"));
			AssertEquals(TripEntryStatusList.Codes.DriverDoesNotHaveValidcurrentCdl, TripEntryStatusList.GetStatusFromNotificationCode("SN042"));
			AssertEquals(TripEntryStatusList.Codes.DriverDoesNotHaveProperCdlEndorsementForHazmat, TripEntryStatusList.GetStatusFromNotificationCode("SN043"));
			AssertEquals(TripEntryStatusList.Codes.VehicleDoesNotHaveCurrentCvsaInspection, TripEntryStatusList.GetStatusFromNotificationCode("SN044"));
			AssertEquals(TripEntryStatusList.Codes.SafetyScoreLessThanAVariableValueForCarrier, TripEntryStatusList.GetStatusFromNotificationCode("SN045"));
			AssertEquals(TripEntryStatusList.Codes.CarrierDataIsNotAvailableAtThisTime, TripEntryStatusList.GetStatusFromNotificationCode("SN046"));
			AssertEquals(TripEntryStatusList.Codes.DriverDataIsNotAvailableAtThisTime, TripEntryStatusList.GetStatusFromNotificationCode("SN047"));
			AssertEquals(TripEntryStatusList.Codes.ConveyanceDataIsNotAvailableAtThisTime, TripEntryStatusList.GetStatusFromNotificationCode("SN048"));
			AssertEquals(TripEntryStatusList.Codes.TrailerDataIsNotAvailableAtThisTime, TripEntryStatusList.GetStatusFromNotificationCode("SN049"));
			AssertEquals(TripEntryStatusList.Codes.CarrierDataNotFoundInFmcsaSystems, TripEntryStatusList.GetStatusFromNotificationCode("SN500"));
			AssertEquals(TripEntryStatusList.Codes.ConveyanceInformationAddedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN501"));
			AssertEquals(TripEntryStatusList.Codes.ConveyanceInformationUpdatedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN502"));
			AssertEquals(TripEntryStatusList.Codes.ConveyanceInformationRemovedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN503"));
			AssertEquals(TripEntryStatusList.Codes.EquipmentInformationAddedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN504"));
			AssertEquals(TripEntryStatusList.Codes.EquipmentInformationUpdatedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN505"));
			AssertEquals(TripEntryStatusList.Codes.EquipmentInformationRemovedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN506"));
			AssertEquals(TripEntryStatusList.Codes.CrewInformationAddedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN507"));
			AssertEquals(TripEntryStatusList.Codes.CrewInformationUpdatedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN508"));
			AssertEquals(TripEntryStatusList.Codes.CrewInformationRemovedByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN509"));
			AssertEquals(TripEntryStatusList.Codes.SealAddedToEquipmentByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN510"));
			AssertEquals(TripEntryStatusList.Codes.SealRemovedFromEquipmentByCustoms, TripEntryStatusList.GetStatusFromNotificationCode("SN511"));
			AssertNull(TripEntryStatusList.GetStatusFromNotificationCode("BLA"));
		}
	}
}
