namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CottonShirtingFabricLicenseNumberValidatorTest : PermitValidatorTest<CottonShirtingFabricLicenseNumberValidator>
	{
		protected override string[] ValidNumbers => new string[] { "12345", "ABCD", "12345ABCD" };

		protected override string[] InvalidNumbers => new string[] { "1234567890", "ABCDEFGHIJK", "12345ABCDE" };

		protected override string[] PermitRequiredEntryTypes => new string[] { "", EntryTypeList.Codes.TemporaryImportationBond };

		protected override string[] PermitNotRequiredEntryTypes => new string[] { EntryTypeList.Codes.Warehouse, EntryTypeList.Codes.ReWarehouse };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._12;

		protected override string LicenseTypeDescription => "Cotton Shirting Fabric License";
	}
}
