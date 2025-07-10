namespace Enterprise.Packing.Business
{
	public enum SSCCGenerationContext
	{
		ScanPacking,
		CheckIfBarcodeIsSSCC,
		GeneratingIDsOnSave,
		AutoClosingPackage,
		GeneratingIDsViaUser,
	}
}
