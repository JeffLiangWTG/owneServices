using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusAuthorisationHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPH_OA_AppliesTo()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			const string errorMessage = "Please enter an Authorization Address when Authorization Type is CW1, CW2, CWP.";

			CombineAssertions(() =>
			{
				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsValue;
				cusAuthorisationHeader.Validation.ValidateCPH_OA_AppliesTo();
				AssertNoError("Address can be empty", cusAuthorisationHeader.CPH_OA_AppliesToInfo, errorMessage);

				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				cusAuthorisationHeader.Validation.ValidateCPH_OA_AppliesTo();
				AssertHasError("Address cannot be empty", cusAuthorisationHeader.CPH_OA_AppliesToInfo, errorMessage);

				cusAuthorisationHeader.CPH_OA_AppliesTo = address.PK;
				cusAuthorisationHeader.CPH_Number = "1234";
				var cusAuthorisationHeader2 = Factory.New<CusAuthorisationHeader>();
				cusAuthorisationHeader2.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				cusAuthorisationHeader2.CPH_OA_AppliesTo = address.PK;
				AssertHasError(cusAuthorisationHeader2.CPH_OA_AppliesToInfo, "There is already a Customs Warehousing authorization 1234 for this address.");

				var address2 = Factory.NewWithValidTestData<OrgAddress>();
				cusAuthorisationHeader2.CPH_OA_AppliesTo = address2.PK;
				AssertNoError(cusAuthorisationHeader2.CPH_OA_AppliesToInfo, "There is already a Customs Warehousing authorization 1234 for this address.");
			});
		}

		public void TestCheckCPH_Type_Mandatory()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationHeader.Validation.ValidateCPH_Type();
				AssertHasErrorContaining("Empty", cusAuthorisationHeader.CPH_TypeInfo, MandatoryValidation.MustBeEntered);

				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				AssertNoErrorContaining("Entered", cusAuthorisationHeader.CPH_TypeInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCPH_Type_List()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationHeader.CPH_Type = "XYZ";
				AssertHasErrorContaining("Invalid", cusAuthorisationHeader.CPH_TypeInfo, ListValidation.InvalidCodeError);

				cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsValue;
				AssertNoErrorContaining("Valid", cusAuthorisationHeader.CPH_TypeInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckCPH_Number_Mandatory()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationHeader.Validation.ValidateCPH_Number();
				AssertHasErrorContaining("Empty", cusAuthorisationHeader.CPH_NumberInfo, MandatoryValidation.MustBeEntered);

				cusAuthorisationHeader.CPH_Number = "NUMBER123";
				AssertNoErrorContaining("Entered", cusAuthorisationHeader.CPH_NumberInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCPH_OH_PermitHolder_MandatoryForNonRUL()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Rule;
				AssertNoErrorContaining("RUL application code", cusAuthorisationHeader.CPH_OH_PermitHolderInfo, MandatoryValidation.MustBeEntered);

				cusAuthorisationHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
				cusAuthorisationHeader.Validation.ValidateCPH_OH_PermitHolder();
				AssertHasErrorContaining("Not RUL application code", cusAuthorisationHeader.CPH_OH_PermitHolderInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckCPH_Number_Unique()
		{
			const string errorMessage = "Authorization holder has another authorization with the same type and number";
			var permitHolder = Factory.New<OrgHeader>();
			SetupCusPermit(cusAuthorisationHeader, permitHolder.PK, "DUPLICATE");
			var cusAuthorisationHeader2 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(cusAuthorisationHeader2, permitHolder.PK, "DUPLICATE");

			CombineAssertions(() =>
			{
				AssertHasError("CPH_Number duplicate", cusAuthorisationHeader2.CPH_NumberInfo, errorMessage);

				cusAuthorisationHeader2.CPH_Number = "DIFFERENT";
				AssertNoError("CPH_Number different", cusAuthorisationHeader2.CPH_NumberInfo, errorMessage);
			});
		}

		public void TestCPH_StartDate()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationHeader.CPH_StartDate = ZDate.Empty;
				AssertHasErrorContaining("Empty", cusAuthorisationHeader.CPH_StartDateInfo, MandatoryValidation.MustBeEntered);

				cusAuthorisationHeader.CPH_StartDate = ZDate.Today;
				AssertNoErrorContaining("Entered", cusAuthorisationHeader.CPH_StartDateInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCPH_EndDate_Mandatory()
		{
			CombineAssertions(() =>
			{
				cusAuthorisationHeader.CPH_EndDate = ZDate.Empty;
				AssertHasErrorContaining("Empty", cusAuthorisationHeader.CPH_EndDateInfo, MandatoryValidation.MustBeEntered);

				cusAuthorisationHeader.CPH_EndDate = ZDate.Today;
				AssertNoErrorContaining("Entered", cusAuthorisationHeader.CPH_EndDateInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCPH_EndDate_DateRange()
		{
			const string dateRangeMessageError = "Start date should be earlier than end date.";
			var today = ZDate.Today;
			CombineAssertions(() =>
			{
				cusAuthorisationHeader.CPH_StartDate = today.AddDays(1);
				cusAuthorisationHeader.CPH_EndDate = today;
				AssertHasError("Start later", cusAuthorisationHeader.CPH_EndDateInfo, dateRangeMessageError);

				cusAuthorisationHeader.CPH_StartDate = today;
				cusAuthorisationHeader.Validation.ValidateCPH_EndDate();
				AssertHasError("Start End Equal", cusAuthorisationHeader.CPH_EndDateInfo, dateRangeMessageError);

				cusAuthorisationHeader.CPH_EndDate = today.AddDays(1);
				AssertNoError("End older than Start", cusAuthorisationHeader.CPH_EndDateInfo, dateRangeMessageError);
			});
		}

		public void TestCheckCPH_EndDateIsValidZDateRange()
		{
			cusAuthorisationHeader.CPH_EndDate = new ZDate(9999, 12, 31);
			AssertEquals("No Date Range Invalid", false, cusAuthorisationHeader.CPH_EndDateInfo.Notifications.Any(x => x.Message.Contains(" years from now and thus is not valid.")));
		}

		public void TestCheckCPH_NumberFormat()
		{
			var mockedCusAuthorizationHeaderProviderType = new Hashtable { { Core.Constants.CountryCodes.Latvia, new CusAuthorisationHeaderProviderForTestRegistration() } };
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", mockedCusAuthorizationHeaderProviderType))
			{
				var provider = (CusAuthorisationHeaderProviderForTest)cusAuthorisationHeader.Provider;
				provider.AuthorisationNumberInvalidFormatMessageExposed = "Override Message";
				provider.IsAuthorisationNumberValidExposed = false;

				cusAuthorisationHeader.CPH_Number = "";
				AssertNoMessageErrorContaining("Number is empty", cusAuthorisationHeader.CPH_NumberInfo, "Override Message");

				cusAuthorisationHeader.CPH_Number = "1";
				AssertHasMessageErrorContaining("Invalid number", cusAuthorisationHeader.CPH_NumberInfo, "Override Message");

				provider.IsAuthorisationNumberValidExposed = true;
				cusAuthorisationHeader.CPH_Number = "2";
				AssertNoMessageErrorContaining("Valid number", cusAuthorisationHeader.CPH_NumberInfo, "Override Message");
			}
		}

		void SetupCusPermit(CommonCusPermitHeader header, ZGuid permitHolder, ZString permitNumber)
		{
			header.CPH_OH_PermitHolder = permitHolder;
			header.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			header.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			header.CPH_StartDate = ZDate.Today.AddDays(-1);
			header.CPH_EndDate = ZDate.Today.AddDays(1);
			header.CPH_Number = permitNumber;
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationHeader = Factory.New<CusAuthorisationHeader>();
		}
		CusAuthorisationHeader cusAuthorisationHeader;
	}
}
