using System.Collections.Generic;

namespace Enterprise.Customs.US.eManifest.Business
{
	partial class TripEntryStatusList
	{
		public static string GetStatusFromNotificationCode(string code)
		{
			var dictionary =
				new Dictionary<string, string>
					{
						{ "SN030", Codes.TripArrived },
						{ "SN031", Codes.HoldCrew },
						{ "SN032", Codes.ReleaseCrew },
						{ "SN033", Codes.HoldEquipment },
						{ "SN034", Codes.ReleaseEquipment },
						{ "SN035", Codes.HoldConveyance },
						{ "SN036", Codes.ReleaseConveyance },
						{ "SN037", Codes.HoldTrip },
						{ "SN038", Codes.ReleaseTrip },
						{ "SN039", Codes.CarrierHasNoValidOperatingAuthority },
						{ "SN040", Codes.CarrierHasAnOssOrderOnIt },
						{ "SN041", Codes.CarriersCurrentInsuranceLessThanMinimumLevel },
						{ "SN042", Codes.DriverDoesNotHaveValidcurrentCdl },
						{ "SN043", Codes.DriverDoesNotHaveProperCdlEndorsementForHazmat },
						{ "SN044", Codes.VehicleDoesNotHaveCurrentCvsaInspection },
						{ "SN045", Codes.SafetyScoreLessThanAVariableValueForCarrier },
						{ "SN046", Codes.CarrierDataIsNotAvailableAtThisTime },
						{ "SN047", Codes.DriverDataIsNotAvailableAtThisTime },
						{ "SN048", Codes.ConveyanceDataIsNotAvailableAtThisTime },
						{ "SN049", Codes.TrailerDataIsNotAvailableAtThisTime },
						{ "SN500", Codes.CarrierDataNotFoundInFmcsaSystems },
						{ "SN501", Codes.ConveyanceInformationAddedByCustoms },
						{ "SN502", Codes.ConveyanceInformationUpdatedByCustoms },
						{ "SN503", Codes.ConveyanceInformationRemovedByCustoms },
						{ "SN504", Codes.EquipmentInformationAddedByCustoms },
						{ "SN505", Codes.EquipmentInformationUpdatedByCustoms },
						{ "SN506", Codes.EquipmentInformationRemovedByCustoms },
						{ "SN507", Codes.CrewInformationAddedByCustoms },
						{ "SN508", Codes.CrewInformationUpdatedByCustoms },
						{ "SN509", Codes.CrewInformationRemovedByCustoms },
						{ "SN510", Codes.SealAddedToEquipmentByCustoms },
						{ "SN511", Codes.SealRemovedFromEquipmentByCustoms },
					};
			string result;
			dictionary.TryGetValue(code, out result);
			return result;
		}
	}
}
