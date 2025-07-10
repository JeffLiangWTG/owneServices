using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public static class DocRollUpConstants
	{
		public static class CostConfirmationDocumentRollupSettingsCodes
		{
			public const string ChargeCode = "CHG";
			public const string Job = "JOB";
		}

		public static CodeDescriptionPairList CostConfirmationDocumentRollupSettingList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(CostConfirmationDocumentRollupSettingsCodes.ChargeCode, ResString.GetMultilingualString("CF3A3896-49DB-4fc8-91AC-A88BE479A0AD", "Charge Code"));
				lookUpList.AddPair(CostConfirmationDocumentRollupSettingsCodes.Job, ResString.GetMultilingualString("2B613C55-EDB7-4eec-8602-A0964126B4DF", "Job"));
				return lookUpList;
			}
		}
		public static class RollupAndSubTotalDescriptions
		{
			public static MultilingualString Origin { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|OriginCharges", "Origin Charges"); } }
			public static MultilingualString Freight { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|FreightCharges", "Freight Charges"); } }
			public static MultilingualString Insurance { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|InsuranceCharges", "Insurance Charges"); } }
			public static MultilingualString Destination { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|DestinationCharges", "Destination Charges"); } }
			public static MultilingualString OriginAndFreight { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|OriginAndFreightCharges", "Origin and Freight Charges"); } }
			public static MultilingualString FreightAndInsurance { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|FreightAndInsuranceCharges", "Freight and Insurance Charges"); } }
			public static MultilingualString OriginFreightAndInsurance { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|OriginFreightAndInsuranceCharges", "Origin, Freight and Insurance Charges"); } }
			public static MultilingualString FreightInsuranceAndDestination { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|FreightInsuranceAndDestinationCharges", "Freight, Insurance and Destination Charges"); } }
			public static MultilingualString OriginFreightInsuranceAndDestination { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|OriginFreightInsuranceAndDestinationCharges", "Origin, Freight, Insurance and Destination Charges"); } }
			public static MultilingualString AllChargesExceptCustomsDutyAndTax { get { return ResString.GetMultilingualString("Document|Invoice|RollupAndSubTotalDescriptions|AllChargesExceptCustomsDutyAndTax", "All charges except Customs Duty and Tax"); } }
		}

		public static class RollupAndSubTotalGroups
		{
			public const string Origin = "ORG+LOD";
			public const string Freight = "FRT";
			public const string Insurance = "INS";
			public const string Destination = "UNL+DST";
			public const string OriginAndFreight = "ORG+FRT";
			public const string FreightAndInsurance = "FRT+INS";
			public const string OriginFreightAndInsurance = "ORG+LOD+FRT+INS";
			public const string FreightInsuranceAndDestination = "FRT+INS+UNL+DST";
			public const string OriginFreightInsuranceAndDestination = "ORG+LOD+FRT+INS+UNL+DST";
			public const string AllChargesExceptCustomsDutyAndTax = "ALL";
		}
	}
}
