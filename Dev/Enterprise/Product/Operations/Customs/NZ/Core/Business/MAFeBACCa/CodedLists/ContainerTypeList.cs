using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class ContainerTypeList : CodeDescriptionEnumList<BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType>
	{
		public static class Codes
		{
			public const string AirContainer = "ULD";
			public const string DimeCoatedTank = "DCT";
			public const string Flat20ft = "FR2";
			public const string Flat40ft = "FR4";
			public const string General20ft = "GP2";
			public const string General30ft = "GP3";
			public const string General40ft = "GP4";
			public const string HalfHeight20ft = "HH2";
			public const string HalfHeight40ft = "HH4";
			public const string MixedContainerTypes = "MIX";
			public const string MoveableCaseL615m = "MCL";
			public const string OpenTop20ft = "OT2";
			public const string OpenTop40ft = "OT4";
			public const string Reefer20ft = "RF2";
			public const string Reefer40ft = "RF4";
			public const string RefrigeratedTank = "RTN";
			public const string RefrigeratedTank20ft = "RT2";
			public const string RefrigeratedTank30ft = "RT3";
			public const string RefrigeratedTank40ft = "RT4";
			public const string Tank20ft = "TN2";
			public const string Tank30ft = "TN3";
			public const string Tank40ft = "TN4";
			public const string TempControlledCtr20ft = "TC2";
			public const string TempControlledCtr40ft = "TC4";
		}

		public static class Descriptions
		{
			public const string AirContainer = "Air Container";
			public const string DimeCoatedTank = "DimeCoatedTank";
			public const string Flat20ft = "Flat20ft";
			public const string Flat40ft = "Flat40ft";
			public const string General20ft = "General20ft";
			public const string General30ft = "General30ft";
			public const string General40ft = "General40ft";
			public const string HalfHeight20ft = "HalfHeight20ft";
			public const string HalfHeight40ft = "HalfHeight40ft";
			public const string MixedContainerTypes = "Mixed Container Types";
			public const string MoveableCaseL615m = "MoveableCaseL615m";
			public const string OpenTop20ft = "OpenTop20ft";
			public const string OpenTop40ft = "OpenTop40ft";
			public const string Reefer20ft = "Reefer20ft";
			public const string Reefer40ft = "Reefer40ft";
			public const string RefrigeratedTank = "RefrigeratedTank";
			public const string RefrigeratedTank20ft = "RefrigeratedTank20ft";
			public const string RefrigeratedTank30ft = "RefrigeratedTank30ft";
			public const string RefrigeratedTank40ft = "RefrigeratedTank40ft";
			public const string Tank20ft = "Tank20ft";
			public const string Tank30ft = "Tank30ft";
			public const string Tank40ft = "Tank40ft";
			public const string TempControlledCtr20ft = "TempControlledCtr20ft";
			public const string TempControlledCtr40ft = "TempControlledCtr40ft";
		}

		public ContainerTypeList()
		{
			AddPair(Codes.AirContainer, Descriptions.AirContainer, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.AirContainer);
			AddPair(Codes.DimeCoatedTank, Descriptions.DimeCoatedTank, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.DimeCoatedTank);
			AddPair(Codes.Flat20ft, Descriptions.Flat20ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.Flat20ft);
			AddPair(Codes.Flat40ft, Descriptions.Flat40ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.Flat40ft);
			AddPair(Codes.General20ft, Descriptions.General20ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.General20ft);
			AddPair(Codes.General30ft, Descriptions.General30ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.General30ft);
			AddPair(Codes.General40ft, Descriptions.General40ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.General40ft);
			AddPair(Codes.HalfHeight20ft, Descriptions.HalfHeight20ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.HalfHeight20ft);
			AddPair(Codes.HalfHeight40ft, Descriptions.HalfHeight40ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.HalfHeight40ft);
			AddPair(Codes.MixedContainerTypes, Descriptions.MixedContainerTypes, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.MixedContainerTypes);
			AddPair(Codes.MoveableCaseL615m, Descriptions.MoveableCaseL615m, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.MoveableCaseL615m);
			AddPair(Codes.OpenTop20ft, Descriptions.OpenTop20ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.OpenTop20ft);
			AddPair(Codes.OpenTop40ft, Descriptions.OpenTop40ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.OpenTop40ft);
			AddPair(Codes.Reefer20ft, Descriptions.Reefer20ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.Reefer20ft);
			AddPair(Codes.Reefer40ft, Descriptions.Reefer40ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.Reefer40ft);
			AddPair(Codes.RefrigeratedTank, Descriptions.RefrigeratedTank, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.RefrigeratedTank);
			AddPair(Codes.RefrigeratedTank20ft, Descriptions.RefrigeratedTank20ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.RefrigeratedTank20ft);
			AddPair(Codes.RefrigeratedTank30ft, Descriptions.RefrigeratedTank30ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.RefrigeratedTank30ft);
			AddPair(Codes.RefrigeratedTank40ft, Descriptions.RefrigeratedTank40ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.RefrigeratedTank40ft);
			AddPair(Codes.Tank20ft, Descriptions.Tank20ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.Tank20ft);
			AddPair(Codes.Tank30ft, Descriptions.Tank30ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.Tank30ft);
			AddPair(Codes.Tank40ft, Descriptions.Tank40ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.Tank40ft);
			AddPair(Codes.TempControlledCtr20ft, Descriptions.TempControlledCtr20ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.TempControlledCtr20ft);
			AddPair(Codes.TempControlledCtr40ft, Descriptions.TempControlledCtr40ft, BACCApplicationTypeDetailsShipmentIdentifiersContainerContainerType.TempControlledCtr40ft);
		}
	}
}
