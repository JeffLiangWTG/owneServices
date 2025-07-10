using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	static class DataTypeDefinitions
	{
		public static readonly DataFormat<ZDateTime> Date = new DateDataFormat();
		public static readonly DataFormat<ZDateTime> MessageTimestamp = new TimestampDataFormat();
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly DataFormat<ZInt> MessageVersion = new NumericDataFormat(1, 2);
		public static readonly DataFormat<ZDateTime> Time = new TimeDataFormat();
		public static readonly DataFormat<ZDateTime> DateTime = new TimestampDataFormat();
		public static readonly DataFormat<ZString> ShipmentIds_ShipmentId = new TextDataFormat(0, 20);
		public static readonly DataFormat<ZString> ShipmentDetail_ProductServiceCode = new TextDataFormat(0, 20);
		public static readonly DataFormat<ZString> ShipmentDetail_HandlingCode = new UpperCaseAlphaDataFormat(0, 10);
		public static readonly DataFormat<ZInt> Quantities_NumberOfPieces = new NumericDataFormat(0, 6);
		public static readonly DataFormat<ZDecimal> Quantities_Weight = new DecimalDataFormat(8, 1);
		public static readonly DataFormat<ZDecimal> Quantities_Volume = new DecimalDataFormat(9, 3);
		public static readonly DataFormat<ZString> Locations_OrgDestCode = new UpperCaseAlphaNumericDataFormat(3, 5);
		public static readonly DataFormat<ZString> Locations_LocationId = new TextDataFormat(0, 20);
		public static readonly DataFormat<ZString> Parties_CarrierId = new UpperCaseAlphaNumericDataFormat(0, 20);
		public static readonly DataFormat<ZString> Parties_ForwarderId = new UpperCaseAlphaDataFormat(3, 3);
		public static readonly DataFormat<ZString> Parties_InterestedPartyId = new TextDataFormat(0, 30);
		public static readonly DataFormat<ZString> Parties_InterestedPartyServiceIndicator = new FreeFormTextDataFormat(0, 10);
		public static readonly DataFormat<ZString> Parties_InterestedPartyReference = new FreeFormTextDataFormat(0, 20);
		public static readonly DataFormat<ZString> OtherInfo_Remarks = new FreeFormTextDataFormat(0, 80);
		public static readonly ListConverter<InterestedPartyTypes, PartnersInterestedPartiesInterestedPartyTypeList> InterestedPartyTypeConverter = new InterestedPartyTypeConverter();
		public static readonly ListConverter<ShipmentReferenceTypes, ShipmentIdentifiersReferenceTypeList> ShipmentReferenceTypeConverter = new ShipmentReferenceTypeConverter();
		public static readonly ListConverter<VolumeUnits, QuantitiesVolumeUnitList> VolumeUnitConverter = new VolumeUnitConverter();
		public static readonly ListConverter<VoyageModes, MovementVoyageModeList> VoyageModeConverter = new VoyageModeConverter();
		public static readonly ListConverter<WeightUnits, QuantitiesWeightUnitList> WeightUnitConverter = new WeightUnitConverter();
		public static readonly ListConverter<ZString, MilestonesMilestoneStatusCodeList> MilestoneStatusCodeConverter = new MilestoneStatusCodeConverter();
	}
}
