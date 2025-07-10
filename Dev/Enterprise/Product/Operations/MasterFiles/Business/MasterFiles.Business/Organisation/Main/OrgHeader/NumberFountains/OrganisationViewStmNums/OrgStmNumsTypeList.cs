using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgStmNumsTypeList : CodeDescriptionPairList
	{
		public OrgStmNumsTypeList()
		{
			AddPair(Codes.ForwardAirBillNumbers, Descriptions.ForwardAirBillNumbers);
			AddPair(Codes.FTZAdmissionControlNumber, Descriptions.FTZAdmissionControlNumber);
			AddPair(Codes.FTZAdmissionControlNumberForWarehouse, Descriptions.FTZAdmissionControlNumberForWarehouse);
			AddPair(Codes.SSCCBarCodeNumbers, Descriptions.SSCCBarCodeNumbers);
			AddPair(Codes.TransportReferenceNumbers, Descriptions.TransportReferenceNumbers);
			Sort();
		}

		public static class Codes
		{
			public const string FTZAdmissionControlNumber = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			public const string FTZAdmissionControlNumberForWarehouse = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse;
			public const string ForwardAirBillNumbers = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			public const string SSCCBarCodeNumbers = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			public const string TransportReferenceNumbers = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
		}

		public static class Descriptions
		{
			public static MultilingualString FTZAdmissionControlNumber => OrgConstants.NumberFountains.Description.FTZAdmissionControlNumber;
			public static MultilingualString FTZAdmissionControlNumberForWarehouse => OrgConstants.NumberFountains.Description.FTZAdmissionControlNumberForWarehouse;
			public static MultilingualString ForwardAirBillNumbers => OrgConstants.NumberFountains.Description.ForwardAirBillNumbers;
			public static MultilingualString SSCCBarCodeNumbers => OrgConstants.NumberFountains.Description.SSCCBarCodeNumbers;
			public static MultilingualString TransportReferenceNumbers => OrgConstants.NumberFountains.Description.TransportReferenceNumbers;
		}
	}
}
