using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AluminumImportLicenseValidatorTest : PermitValidatorTest<AluminumImportLicenseValidator>
	{
		public void TestPermitNoRequired()
		{
			InvoiceLine.LicenceAndPermits.RemoveAndDeleteAll();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Now.AddDays(-2);
			tariff.UE_DateTo = ZDateTime.Now.AddDays(2);
			tariff.UE_PermitLicenseIndicator = LicencePermitTypeList.Codes._28;
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("Aluminum Import License is required when entry type is 01", InvoiceLine.JI_TariffInfo, "The Aluminum Import License number is required.");

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			InvoiceLine.Declaration.US_EnableENS = true;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("Aluminum Import License is NOT required when entry type is 06", InvoiceLine.JI_TariffInfo, "The Aluminum Import License number is required.");

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			InvoiceLine.LicenceAndPermits.AddNew("28");
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("Aluminum Import License is entered", InvoiceLine.JI_TariffInfo, "The Aluminum Import License number is required.");

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			InvoiceLine.Declaration.US_EnableENS = true;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("Aluminum Import License should NOT be entered for entry type is 06", InvoiceLine.JI_TariffInfo, "The Aluminum Import License number is not permitted for FTZ withdrawals.");
		}

		protected override string[] PermitNotRequiredEntryTypes => new string[]
		{
			EntryTypeList.Codes.Warehouse,
			EntryTypeList.Codes.ReWarehouse,
			EntryTypeList.Codes.TemporaryImportationBond,
			EntryTypeList.Codes.ConsumptionFTZ
		};

		protected override string[] ValidNumbers => new string[] { "12345", "ABCDE", "12345ABCD" };

		protected override string[] InvalidNumbers => new[] { "1234567890", "ABCDEFGHIJ", "12345 ABCDE" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._28;

		protected override string LicenseTypeDescription => "Aluminum Import License";
	}
}
