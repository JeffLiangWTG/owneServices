using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRegistrationNumberValidationTest : TestCaseWithFactory
	{
		OrgRegistrationNumber regNo;

		OrgRegistrationNumber RegNo
		{
			get { return regNo ?? (regNo = new OrgRegistrationNumber(OrgRegistrationNumberTest.GetTestOrganization(Factory, ZString.Empty))); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
		}

		public void TestValidateNumber_DuplicateNonUniqueNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			{
				var org = OrgRegistrationNumberTest.GetTestOrganization(Factory, ZString.Empty);
				var vatCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "987654321", Constants.CountryCodes.Turkey);
				var vtcCode = org.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VTC, "Yes.", Constants.CountryCodes.Turkey);
				Factory.Save();

				var nonUniqueCodeTypesForCountry = OrgCusCodeCountryFactory.GetIOrgCusCodeNonUniqueProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.GetNonUniqueCodes();
				AssertCollectionNotContains("Precondition: VAT should not be in the non-unique codes list.", (string)vatCode.OK_CodeType, nonUniqueCodeTypesForCountry);
				AssertCollectionContains("Precondition: VTC should be in the non-unique codes list for Turkey Compliance.", (string)vtcCode.OK_CodeType, nonUniqueCodeTypesForCountry);

				var dupeVat = new OrgRegistrationNumber(Factory.New<OrgHeader>());
				dupeVat.NumberTypeForDisplay = vatCode.OK_CodeType;
				dupeVat.Number = vatCode.OK_CustomsRegNo;
				AssertHasWarning("Precondition: VAT is supposed to be unique and should be checked for conflicts.", dupeVat.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: QWERTY (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

				var dupeVtc = new OrgRegistrationNumber(Factory.New<OrgHeader>());
				dupeVtc.NumberTypeForDisplay = vtcCode.OK_CodeType;
				dupeVtc.Number = vtcCode.OK_CustomsRegNo;
				AssertNoWarnings("VTC is not expected to be unique and it should not be checked for conflicts.", dupeVtc.NumberInfo);
			}
		}

		public void TestValidateNumber_DuplicateNumber()
		{
			RegNo.Organization.ClosestPort.RL_RN_NKCountryCode = Constants.CountryCodes.Australia;
			RegNo.NumberTypeForDisplay = OrgCusCode.CodeTypes.DriverLicenceID;
			RegNo.Number = "1";

			Factory.Save();

			var anotherRegNo = new OrgRegistrationNumber(Factory.New<OrgHeader>());
			anotherRegNo.Organization.OH_Code = "ANOTHER";
			anotherRegNo.NumberTypeForDisplay = OrgCusCode.CodeTypes.DriverLicenceID;

			anotherRegNo.Number = "1";
			AssertHasWarning(anotherRegNo.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: QWERTY (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

			anotherRegNo.Number = "2";
			AssertNoNotifications(anotherRegNo.NumberInfo);

			anotherRegNo.NumberTypeForDisplay = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			anotherRegNo.Number = "21 003 980 130";

			Factory.Save();

			RegNo.NumberTypeForDisplay = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			RegNo.Number = "21 003 980 130";
			AssertHasWarning(RegNo.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: ANOTHER (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

			anotherRegNo.CusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Singapore;
			Factory.Save();
			RegNo.Validation.ValidateNumber();
			AssertNoNotifications(RegNo.NumberInfo);

			anotherRegNo.NumberTypeForDisplay = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			anotherRegNo.Number = "21-003 980.30)";

			Factory.Save();

			RegNo.NumberTypeForDisplay = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			RegNo.Number = "21(003 980/30";
			AssertHasWarning(RegNo.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: ANOTHER (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

			RegNo.Number = "21&003 980/30";
			AssertNoWarning(RegNo.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: ANOTHER (Maximum of 10 organizations shown). Please check that the organizations are not the same.");
		}

		public void TestValidateNumber_DuplicateNumber_TwoAndMore()
		{
			var anotherRegNo1 = new OrgRegistrationNumber(Factory.New<OrgHeader>());
			anotherRegNo1.Organization.OH_Code = "ANOTHER1";
			anotherRegNo1.NumberTypeForDisplay = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			anotherRegNo1.Number = "21 003 980 130";
			AssertNoNotifications(anotherRegNo1.NumberInfo);

			Factory.Save();

			var anotherRegNo2 = new OrgRegistrationNumber(Factory.New<OrgHeader>());
			anotherRegNo2.Organization.OH_Code = "ANOTHER2";
			anotherRegNo2.NumberTypeForDisplay = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			anotherRegNo2.Number = "21 003 980 130";
			AssertHasWarning(anotherRegNo2.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: ANOTHER1 (Maximum of 10 organizations shown). Please check that the organizations are not the same.");

			Factory.Save();

			var anotherRegNo3 = new OrgRegistrationNumber(Factory.New<OrgHeader>());
			anotherRegNo3.Organization.OH_Code = "ANOTHER3";
			anotherRegNo3.NumberTypeForDisplay = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			anotherRegNo3.Number = "21 003 980 130";
			AssertHasWarning(anotherRegNo3.NumberInfo, "This Registration Number is already in use by at least one organization. The organizations are: ANOTHER1, ANOTHER2 (Maximum of 10 organizations shown). Please check that the organizations are not the same.");
		}

		public void TestValidateNumber_CopiesNotificationsFromCusCode()
		{
			RegNo.NumberTypeForDisplay = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			RegNo.Number = "23";
			AssertHasError(RegNo.NumberInfo, "The entered ABN is not valid.\r\nAn ABN must be 11 or 14 digits with a valid check-digit.");

			RegNo.Number = "21 003 980 130";
			AssertNoErrors(RegNo.NumberInfo);
		}

		public void TestValidateNumber_PhoneOrBusinessNumberRequired()
		{
			RegNo.NumberTypeForDisplay = OrgCusCode.CodeTypes.MedicareID;

			Env.Registry.SetOrgConsigneeRequiredFields(new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, false));
			RegNo.Number = "";
			AssertNoErrors(RegNo.NumberInfo);

			Env.Registry.SetOrgConsigneeRequiredFields(new OrgRequiredFields(false, false, false, false, false, true, false, false, false, false, false));
			RegNo.Validation.ValidateNumber();
			AssertNoErrors(RegNo.NumberInfo);

			RegNo.Organization.ClosestPort.RL_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			RegNo.Validation.ValidateNumber();
			AssertNoErrors(RegNo.NumberInfo);

			RegNo.Organization.ClosestPort.RL_RN_NKCountryCode = Constants.CountryCodes.Australia;
			RegNo.Validation.ValidateNumber();
			AssertHasError(RegNo.NumberInfo, "Please enter either a Phone Number or a Registration Number for this organization.");

			RegNo.Number = "123";
			AssertNoErrors(RegNo.NumberInfo);

			RegNo.Organization.MainAddress.OA_Phone = "789";
			RegNo.Number = "";
			AssertNoErrors(RegNo.NumberInfo);
		}

		public void TestValidateNumber_RequiredFieldForOrg()
		{
			RegNo.NumberTypeForDisplay = OrgCusCode.CodeTypes.MedicareID;

			Env.Registry.SetOrgConsigneeRequiredFields(new OrgRequiredFields(false, false, false, false, false, false, false, false, false, false, false));
			RegNo.Number = "";
			AssertNoErrors(RegNo.NumberInfo);

			Env.Registry.SetOrgConsigneeRequiredFields(new OrgRequiredFields(false, false, false, false, true, false, false, false, false, false, false));
			RegNo.Validation.ValidateNumber();
			AssertNoErrors(RegNo.NumberInfo);

			RegNo.Organization.ClosestPort.RL_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			RegNo.Validation.ValidateNumber();
			AssertNoErrors(RegNo.NumberInfo);

			RegNo.Organization.ClosestPort.RL_RN_NKCountryCode = Constants.CountryCodes.Australia;
			RegNo.Validation.ValidateNumber();
			AssertHasError(RegNo.NumberInfo, "Please enter a Registration Number.");

			RegNo.Number = "123";
			AssertNoErrors(RegNo.NumberInfo);
		}

		public void TestValidateNumberTypeForDisplay()
		{
			RegNo.NumberTypeForDisplay = "";
			AssertHasError(RegNo.NumberTypeForDisplayInfo, "Please enter a Registration Number Type.");

			RegNo.NumberTypeForDisplay = "!@#";
			AssertHasError(RegNo.NumberTypeForDisplayInfo, "Enter a valid Registration Number Type.");

			RegNo.NumberTypeForDisplay = RegNo.Lookups.NumberTypes[0].Code;
			AssertNoErrors(RegNo.NumberTypeForDisplayInfo);
		}

		public void TestValidateNumberTypeForDisplay_NoGrantedSecurityOfSSN()
		{
			var originalOrgDetailsViewPersonalInformation = Env.Security.OrgDetailsViewPersonalInformation.IsAllowed;
			try
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
				{
					var org = OrgRegistrationNumberTest.GetTestOrganization(Factory, ZString.Empty);
					var ssnCusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "111-11-1111", Constants.CountryCodes.UnitedStates);

					var ssnRegNo = new OrgRegistrationNumber(org);
					ssnRegNo.NumberTypeForDisplay = ssnCusCode.OK_CodeType;
					AssertHasError(ssnRegNo.NumberTypeForDisplayInfo, @"You do not have the appropriate security rights to select this Code.
If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: Maintain -> Master Data -> Organization -> View -> View Personal Information. ");
				}
			}
			finally
			{
				Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = originalOrgDetailsViewPersonalInformation;
			}
		}
	}
}
