namespace CargoWise.RefDbRepo.PLReferenceData.Business.CusProcedure
{
	public static class Constants
	{
		public const string NotApplicableXmlValue = "N";
		public const string ApplicableXmlValue = "Y";

		public static class PuescDictionaryId
		{
			public const string AllowedRequestedProcedureAndPreviousProcedureCombinations = "011";
		}

		public static class ProcedureCodes
		{
			public const string TemporaryExportUnderTheOutwardProcessingProcedure = "21";
			public const string TemporaryExportOtherThanThatReferredToUnderCode21 = "22";
			public const string TemporaryExportForReturnInTheUnalteredState = "23";
			public const string InwardProcessingProcedureSuspensionSystem = "51";
			public const string PlacingOfGoodsUnderTemporaryAdmission = "53";
			public const string PlacingOfGoodsUnderTheCustomsWarehousingProcedure = "71";
			public const string PlacingUnderTheCustomsWarehousingProcedureOrInAFreeZoneWithAdvancePaymentOfExportRefundsOfProducts = "76";
			public const string EntryOfGoodsForAFreeZoneSubjectToTypeIIControls = "78";
			public const string PlacingOfUnionGoodsUnderAWarehousingProcedureOtherThanACustomsWarehousingProcedureWhereTaxIsSuspended = "96";
		}
	}
}
