namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public static class Constants
	{
		public static class Header
		{
			public static class AddInfo
			{
				public const string FTZMove = "FTZMove";
				public const string UI_NKCarrierSCAC = "UI_NKCarrierSCAC";
				public const string SchDArrival = "SchDArrival";
				public const string US_NKLocationOfGoods = "US_NKLocationOfGoods";
				public const string InBondMode = "InBondMode";
				public const string SchDLoading = "SchDLoading";
			}

			public static class UniversalCopyIgnoreElement
			{
				public const string AMSMoveHeaders = "AMSMoveHeaders";
				public const string CusAddInfos = "CusAddInfos";
				public const string CusInBondManifestBills = "CusInBondManifestBills";
				public const string CusInBondOceanBills = "CusInBondOceanBills";
				public const string CusInBondRegularBills = "CusInBondRegularBills";
				public const string PTTMoveHeaders = "PTTMoveHeaders";
				public const string CusInBondEDIMessages = "CusInBondEDIMessages";
				public const string ReferenceNumbers = "ReferenceNumbers";
				public const string CusInBondBill = "CusInBondBill";
				public const string CusInBondMoveDetails = "CusInBondMoveDetails";
				public const string CusInBondContainers = "CusInBondContainers";
				public const string CusInbondBillAddRefs = "CusInbondBillAddRefs";
				public const string CusCodeDatas = "CusCodeDatas";
				public const string CusInBondCargoDescs = "CusInBondCargoDescs";
				public const string CusInBondVehicleCtrls = "CusInBondVehicleCtrls";
				public const string UNDGDataItems = "UNDGDataItems";
				public const string InBondMoveDetail = "InBondMoveDetail";
				public const string Commodity = "Commodity";
				public const string UNDGSubstancePivots = "UNDGSubstancePivots";
			}
		}

		public static class Bill
		{
			public static class AddInfo
			{
				public const string ManifestUnit = "ManifestUnit";
				public const string Weight = "Weight";
				public const string WeightUnit = "WeightUnit";
				public const string Volume = "Volume";
				public const string VolumeUnit = "VolumeUnit";
				public const string PortOfLadingScheduleK = "PortOfLadingScheduleK";
				public const string PlaceOfReceiptScheduleD = "PlaceOfReceiptScheduleD";
				public const string IssuerCode = "IssuerCode";
			}
		}

		public static class AdditionalReference
		{
			public const string Type = "ADR";
			public const string TypeDescription = "Additional Reference";
		}

		public static class MovementHeader
		{
			public static class NumberTypes
			{
				public const string InBondNumberDescription = "In-Bond Transit Number";
			}

			public static class AdditionalReferences
			{
				public const string GONumber = "GON";
				public const string GONumberDescription = "G.O. Number";
				public const string PedimentoNumber = "PDN";
				public const string PedimentoNumberDescription = "Pedimento Number";
			}
		}

		public static class MovementDetail
		{
			public static class NumberTypes
			{
				public const string PreviousInBondNumber = "PIT";
				public const string PreviousInBondNumberDescription = "Previous In-Bond Transit Number";
			}
		}

		public static class SecondaryNotifyParty
		{
			public const string Type = "SNP";
			public const string TypeDescription = "Secondary Notify Party";
		}

		public static class AddInfo
		{
			public const string True = "Y";
			public const string False = "N";
		}
	}
}
