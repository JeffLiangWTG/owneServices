using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USLinkedEntryAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_LE_EntryNumber()
		{
			var protest = new Protest.Protest(Factory.New<JobDeclaration>());
			var linkedEntry = protest.LinkedEntries.AddNew();
			linkedEntry.Declaration.US_ConsolACE = true;
			linkedEntry.AddInfoValidation.ValidateUS_LE_EntryNumber();
			AssertHasMessageError(linkedEntry.US_LE_EntryNumberInfo, USLinkedEntryAddInfoValidation.EntryNoCannotBeEmpty);
			linkedEntry.US_LE_EntryNumber = "11";
			AssertNoMessageError(linkedEntry.US_LE_EntryNumberInfo, USLinkedEntryAddInfoValidation.EntryNoCannotBeEmpty);
			linkedEntry = protest.LinkedEntries.AddNew();
			linkedEntry.US_LE_EntryNumber = "12";
			AssertNoMessageError(linkedEntry.US_LE_EntryNumberInfo, USLinkedEntryAddInfoValidation.DuplicateEntry);
			linkedEntry = protest.LinkedEntries.AddNew();
			linkedEntry.US_LE_EntryNumber = "12";
			AssertHasMessageError(linkedEntry.US_LE_EntryNumberInfo, USLinkedEntryAddInfoValidation.DuplicateEntry);
			AssertHasMessageError(linkedEntry.US_LE_EntryNumberInfo, EntryNumberValidator.EntryNumberFormat);
			linkedEntry.US_LE_EntryNumber = "XXX12345678";
			AssertNoMessageError(linkedEntry.US_LE_EntryNumberInfo, EntryNumberValidator.EntryNumberFormat);
		}

		public void TestCheckUS_LE_PortCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var protest = new Protest.Protest(Factory.New<JobDeclaration>());
			protest.US_P_FilingDDPP = "8888";
			var linkedEntry = protest.LinkedEntries.AddNew();
			linkedEntry.US_LE_PortCode = "7809";
			AssertHasWarning(linkedEntry.US_LE_PortCodeInfo, USLinkedEntryAddInfoValidation.PortDoNotMatchFilingDDPP);
			linkedEntry.US_LE_PortCode = "8889";
			AssertNoWarning(linkedEntry.US_LE_PortCodeInfo, USLinkedEntryAddInfoValidation.PortDoNotMatchFilingDDPP);
			linkedEntry.US_LE_PortCode = "~";
			AssertHasMessageErrorContaining(linkedEntry.US_LE_PortCodeInfo, ListValidation.InvalidCodeMessageError);
			linkedEntry.US_LE_PortCode = "3901";
			AssertNoMessageErrorContaining(linkedEntry.US_LE_PortCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_LE_Withdraw()
		{
			var protest = new Protest.Protest(Factory.New<JobDeclaration>());
			protest.TariffActCitation = Enterprise.Customs.US.Business.Protest.TariffActCitationList.Codes.D_Section181;
			Assert(protest.Is181115Intervention);
			var linkedEntry = protest.LinkedEntries.AddNew();
			linkedEntry.US_LE_Withdraw = true;
			AssertHasMessageError(linkedEntry.US_LE_WithdrawInfo, USLinkedEntryAddInfoValidation.WithdrawingEntriesIsNotSupportedFor181115Intervention);
			linkedEntry.US_LE_Withdraw = false;
			AssertNoMessageError(linkedEntry.US_LE_WithdrawInfo, USLinkedEntryAddInfoValidation.WithdrawingEntriesIsNotSupportedFor181115Intervention);
			protest.TariffActCitation = Enterprise.Customs.US.Business.Protest.TariffActCitationList.Codes.C_Section520d;
			linkedEntry.US_LE_Withdraw = true;
			AssertNoMessageError(linkedEntry.US_LE_WithdrawInfo, USLinkedEntryAddInfoValidation.WithdrawingEntriesIsNotSupportedFor181115Intervention);
		}
	}
}
