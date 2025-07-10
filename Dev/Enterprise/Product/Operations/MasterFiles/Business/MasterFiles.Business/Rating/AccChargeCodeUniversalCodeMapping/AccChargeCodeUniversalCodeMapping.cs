using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using WiseRates.Constants;

namespace Enterprise.MasterFiles.Business.Rating;

public class AccChargeCodeUniversalCodeMapping(BusinessObjectFactory factory, DataRow row) : AutoAccChargeCodeUniversalCodeMapping(factory, row)
{
	#region AUP_Code

	[ResourceStringData("EBEB8481-EE33-4591-B137-F78E96D6B804", Caption = "Code")]
	[List("Lookups.AUP_UniversalChargeCode_List")]
	public override ZString AUP_Code
	{
		get => base.AUP_Code;
		set => base.AUP_Code = value;
	}

	[ResourceStringData("A7AF6ACA-D9B5-4B20-872C-09CDCAB75A4C", Caption = "Description")]
	public ZString AUP_Description =>
		AUP_Type.ToString() switch
		{
			Constants.ChargeCodeMappingTypes.Carrier => GetMatchingDescriptionMappingFromApi(),
			Constants.ChargeCodeMappingTypes.Universal => GetMatchingDescriptionMappingFromApi(),
			_ => string.Empty
		} ?? string.Empty;

	string GetMatchingDescriptionMappingFromApi()
	{
		var codeMappings = AccChargeCodeUniversalCodeMappingLookups.GetAllUniversalChargeCodesWithMappingInfo(Factory);

		return AUP_Type.ToString() switch
		{
			Constants.ChargeCodeMappingTypes.Carrier =>
				AccChargeCodeUniversalCodeMappingLookups.GetCarrierChargeCodeDescription(Factory, AUP_Code, AUP_TransportMode, Carrier?.ShippingLineSCAC),
			Constants.ChargeCodeMappingTypes.Universal =>
			codeMappings.Where(mapping => mapping.Code == AUP_Code).FirstOrDefault()?.Description,
			_ => null,
		};
	}

	#endregion

	#region AUP_OH_Carrier

	[ResourceStringData("02CAA35E-5213-46F5-9943-AA2E4C381F65", Caption = "Carrier")]
	[RelatedBusinessObject("Carrier")]
	[List("Lookups.Carriers")]
	public override ZGuid AUP_OH_Carrier
	{
		get => base.AUP_OH_Carrier;
		set => base.AUP_OH_Carrier = value;
	}

	public bool AUP_OH_Carrier_ReadOnly => AUP_Type == Constants.ChargeCodeMappingTypes.Universal;

	#endregion

	#region AUP_TransportMode

	[ResourceStringData("41674F71-D554-42A0-B090-02A0895AF010", Caption = "Mode")]
	[List("Lookups.TransportModeList")]
	public override ZString AUP_TransportMode
	{
		get => base.AUP_TransportMode;
		set => base.AUP_TransportMode = value;
	}

	public bool AUP_TransportMode_ReadOnly => AUP_Type == Constants.ChargeCodeMappingTypes.Universal;

	#endregion

	#region AUP_Type

	[ResourceStringData("DED827FF-DC94-49E2-967A-17601DCC5BF7", Caption = "Type")]
	[List("Lookups.ChargeCodeMappingTypeList")]
	public override ZString AUP_Type
	{
		get => base.AUP_Type;
		set
		{
			if (value == Constants.ChargeCodeMappingTypes.Universal)
			{
				ClearCarrierAndTransportMode();
			}

			base.AUP_Type = value;
		}
	}

	#endregion

	public void UpdatePropertiesFromChargeCode(MappedChargeCodeBizo chargeCode)
	{
		if (AUP_Type != Constants.ChargeCodeMappingTypes.Carrier || chargeCode is null)
		{
			return;
		}

		SuspendValidation();
		UpdateTransportModeFromBizo(chargeCode);
		UpdateCarrierFromBizo(chargeCode);
		ResumeValidation();
	}

	void UpdateCarrierFromBizo(MappedChargeCodeBizo chargeCode)
	{
		if (string.IsNullOrEmpty(chargeCode.UCC_Carrier))
		{
			AUP_OH_Carrier = ZGuid.Empty;
			return;
		}

		if (AUP_TransportMode == Core.Constants.TransportModes.Sea)
		{
			if (AccChargeCodeUniversalCodeMappingLookups.TryGetCarrierFromScac(Factory, chargeCode.UCC_Carrier, out var seaCarrier))
			{
				AUP_OH_Carrier = seaCarrier.PK;
			}
		}
		else if (AUP_TransportMode == Core.Constants.TransportModes.Air)
		{
			if (AccChargeCodeUniversalCodeMappingLookups.TryGetCarrierFromIata(Factory, chargeCode.UCC_Carrier, out var airCarrier))
			{
				AUP_OH_Carrier = airCarrier.PK;
			}
		}
	}

	void UpdateTransportModeFromBizo(MappedChargeCodeBizo chargeCode) =>
		AUP_TransportMode = chargeCode.UCC_RateProvider.ToString() switch
		{
			WRConstants.RateProviders.CargoGuide => Core.Constants.TransportModes.Air,
			WRConstants.RateProviders.CargoSphere => Core.Constants.TransportModes.Sea,
			_ => ZString.Empty
		};

	void ClearCarrierAndTransportMode()
	{
		SuspendValidation();
		AUP_OH_Carrier = ZGuid.Empty;
		AUP_TransportMode = string.Empty;
		ResumeValidation();
	}

	#region Constants

	public static class Constants
	{
		public static class ChargeCodeMappingTypes
		{
			public const string Universal = "UCC";
			public const string Carrier = "CAR";
		}

		public static class ChargeCodeMappingTypeDescriptions
		{
			public static readonly ResourceString Universal = ResString.GetMultilingualString("867B816A-4370-4525-9335-8F545C9963CA", "Universal Charge Code");
			public static readonly ResourceString Carrier = ResString.GetMultilingualString("777B69BB-1FCA-4D87-8B32-878B4AB7FA8C", "Carrier Charge Code");
		}
	}

	#endregion
}
