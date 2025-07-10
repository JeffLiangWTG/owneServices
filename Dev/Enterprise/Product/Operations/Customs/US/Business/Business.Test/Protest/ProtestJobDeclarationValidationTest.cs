using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	sealed class ProtestJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestRunProtestRelatedValidationsOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var org = Factory.New<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567XX");
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var address = Factory.New<JobDocAddress>();
			address.E2_AddressType = DocAddressTypes.Codes.ProtestantAddress;
			address.OrganisationPK = org.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.DocAddresses.Add(address);

			var protest = new Protest(declaration);
			protest.US_P_ProtestedDecision = "TEST";
			protest.US_P_MerchandiseDesc = "TEST";
			protest.US_P_ProtestantType = ProtestantTypeList.Codes.ImporterConsignee;
			protest.US_P_FilingDDPP = "8888";
			protest.US_P_AddressTeam = "0PR";
			protest.US_P_PeriodBaseDate = ZDateTime.Today;
			protest.US_P_PeriodBaseDateQualifier = ProtestPeriodBaseDateQualifierList.Codes.A_Importer;

			var organisation = Factory.New<OrgHeader>();
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-123456700");
			organisation.OH_RL_NKClosestPort = "USLAX";
			protest.Protestant.OrganisationPK = organisation.PK;

			var entry = protest.LinkedEntries.AddNew();
			entry.US_LE_EntryNumber = "XXX12345675";

			protest.RunPreSaveValidation();
			AssertEquals("If this fails, it means you have added a validation in the base that needs to be overridden", "", protest.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString());
		}
	}
}
