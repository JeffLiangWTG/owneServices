using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SteelPermitValidatorTest : PermitValidatorTest<SteelPermitValidator>
	{
		public void TestPermitNoRequired()
		{
			InvoiceLine.LicenceAndPermits.RemoveAndDeleteAll();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Now.AddDays(-2);
			tariff.UE_DateTo = ZDateTime.Now.AddDays(2);
			tariff.UE_PermitLicenseIndicator = LicencePermitTypeList.Codes._01;
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("Steel Import License is required when entry type is 01", InvoiceLine.JI_TariffInfo, "The Steel Import License number is required.");

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			InvoiceLine.Declaration.US_EnableENS = true;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("Steel Import License is NOT required when entry type is 06", InvoiceLine.JI_TariffInfo, "The Steel Import License number is required.");

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			InvoiceLine.LicenceAndPermits.AddNew("01");
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageError("Steel Import License is entered", InvoiceLine.JI_TariffInfo, "The Steel Import License number is required.");

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			InvoiceLine.Declaration.US_EnableENS = true;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError("Steel Import License should NOT be entered for entry type is 06", InvoiceLine.JI_TariffInfo, "The Steel Import License number is not permitted for FTZ withdrawals.");
		}

		protected override string[] PermitNotRequiredEntryTypes => new string[]
		{
			EntryTypeList.Codes.Warehouse,
			EntryTypeList.Codes.ReWarehouse,
			EntryTypeList.Codes.InformalFreeDutiable,
			EntryTypeList.Codes.InformalQuotaVisa,
			EntryTypeList.Codes.TemporaryImportationBond,
			EntryTypeList.Codes.ConsumptionFTZ
		};

		protected override string[] ValidNumbers => new string[] { "12345", "ABCDE", "12345ABCD" };

		protected override string[] InvalidNumbers => new[] { "1234567890", "ABCDEFGHIJ", "12345 ABCDE" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._01;

		protected override string LicenseTypeDescription => "Steel Import License";
	}
}
