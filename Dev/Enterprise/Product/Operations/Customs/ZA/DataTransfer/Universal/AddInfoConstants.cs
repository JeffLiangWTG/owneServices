namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public static class AddInfoConstants
	{
		public static class AsycudaManifestHeader
		{
			public const string ExcessIndicator = "ExcessIndicator";
			public const string FullyLoadedUnloadedDate = "FullyLoadedUnloadedDate";
			public const string GateInOutMessageType = "GateInOutMessageType";
			public const string GateInOutDate = "GateInOutDate";
			public const string ParentBill = "ParentBill";
			public const string UnpackedDate = "UnpackedDate";
		}

		public static class AsycudaBill
		{
			public const string BillIssuer = "BillIssuer";
			public const string MRN = "MRN";
			public const string CustomsCPC = "CustomsCPC";
			public const string LRN = "LRN";
			public const string BillStatus = "BillStatus";
		}

		public static class AsycudePack
		{
			public const string ContentsFoundToBe = "ContentsFoundToBe";
			public const string CargoType = "CargoType";
			public const string CargoTypeDesc = "CargoTypeDesc";
			public const string PackageConditionInd = "PackageConditionInd";
			public const string PackageConditionIndDesc = "PackageConditionIndDesc";
			public const string ExcessShortInd = "ExcessShortInd";
			public const string ExcessShortIndDesc = "ExcessShortIndDesc";
			public const string ManifestedWeightUnit = "ManifestedWeightUnit";
			public const string ManifestedVolumeUnit = "ManifestedVolumeUnit";
			public const string PackCondDesc = "PackCondDesc";
			public const string ContShouldBe = "ContShouldBe";
			public const string OutturnedWeightUnit = "OutturnedWeightUnit";
			public const string OutturnedVolumeUnit = "OutturnedVolumeUnit";
			public const string SealIntactIndicator = "SealIntactIndicator";
		}

		public static class AsycudaContainer
		{
			public const string ContUnpackTime = "ContUnpackTime";
			public const string GateInOutDate = "GateInOutDate";
		}

		public static class InvoiceLine
		{
			public const string Colour = "Colour";
			public const string EngineCapacity = "EngineCapacity";
			public const string EngineNumber = "EngineNumber";
			public const string Make = "Make";
			public const string VehicleFormat = "VehicleFormat";
			public const string VehicleType = "VehicleType";
			public const string VIN = "VIN";
			public const string YearOfManufacture = "YearOfManufacture";
		}
	}
}

