using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestRefUNLOCOValidation : BusinessObjectValidationTestCase
	{
		#region Port Code Consists Of Only Letters

		public void TestPortCodeConsistsOfOnlyLettersAndNumbers()
		{
			RefUNLOCO loco = Factory.New<RefUNLOCO>();
			Assert("Port Code has no errors", !loco.RL_CodeInfo.HasErrors());

			loco.RL_Code = "";
			Assert("Port Code must be entered", loco.RL_CodeInfo.HasErrors());

			loco.RL_Code = "AUXGY";
			Assert("Port Code has no errors", !loco.RL_CodeInfo.HasErrors());

			loco.RL_Code = "AUC12";
			Assert("Port Code has no errors", !loco.RL_CodeInfo.HasErrors());

			loco.RL_Code = "AU12.";
			Assert("Port Code must be only letters and numbers.", loco.RL_CodeInfo.HasError("A UNLOCO must consist of only letters and numbers."));
		}

		#endregion

		#region IATA Code Consists Of Only Letters

		public void TestIATACodeConsistsOfOnlyLetters()
		{
			RefUNLOCO loco = Factory.New<RefUNLOCO>();
			loco.RL_HasAirport = true;
			Assert("Port Code has no errors", !loco.RL_IATAInfo.HasErrors());

			loco.RL_IATA = "AUC";
			Assert("Port Code has no errors", !loco.RL_IATAInfo.HasErrors());

			loco.RL_IATA = "AU2";
			Assert("IATA Code must be only letters", loco.RL_IATAInfo.HasError("The IATA Code must consist of only letters."));
		}

		#endregion

		#region IATA Region Code Consists Of Only Three Letters

		public void TestIATARegionCodeConsistsOfOnlyThreeLetters()
		{
			var loco = Factory.NewWithValidTestData<RefUNLOCO>();

			loco.RL_IATARegionCode = string.Empty;
			Assert("IATA Region Code should allow empty string", !loco.RL_IATARegionCodeInfo.HasErrors());

			loco.RL_IATARegionCode = "XYZ";
			Assert("IATA Region Code Code has no errors", !loco.RL_IATARegionCodeInfo.HasErrors());

			loco.RL_IATARegionCode = "123";
			Assert("IATA Code must be letters only", loco.RL_IATARegionCodeInfo.HasErrors());

			loco.RL_IATARegionCode = "AB";
			Assert("IATA Code must be exactly three letters", loco.RL_IATARegionCodeInfo.HasErrors());
		}

		#endregion

		#region UNLOCO (Code)

		public void TestValidateRL_Code()
		{
			UNLOCO.RL_Code = "UKTTT";
			Assert("Invalid country code in UNLOCO, 5 chars, error expected", UNLOCO.RL_CodeInfo.HasErrors());

			UNLOCO.RL_Code = "AUXGY";
			Assert("Valid country code in UNLOCO, 5 chars, no error", !UNLOCO.RL_CodeInfo.HasErrors());

			UNLOCO.RL_Code = "AUTT";
			Assert("Valid country code, < 5 chars, error expected", UNLOCO.RL_CodeInfo.HasErrors());

			UNLOCO.RL_Code = "";
			Assert("No country code provided, error expected", UNLOCO.RL_CodeInfo.HasErrors());
		}

		public void TestRL_CodeIsNotDuplicated()
		{
			RefUNLOCO loco1 = Factory.New<RefUNLOCO>();
			loco1.RL_Code = "USAZ5";

			AssertEquals(false, loco1.RL_CodeInfo.HasErrors());

			RefUNLOCO loco2 = Factory.New<RefUNLOCO>();
			loco2.RL_Code = "USAZ5";

			Assert("There is already an UNLOCO record with same code.", loco2.RL_CodeInfo.HasErrors());
		}

		#endregion

		#region Country

		public void TestValidateRL_RN()
		{
			UNLOCO.RL_Code = "AUTTT";
			RefCountry uS = Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, "US") as RefCountry;
			UNLOCO.RL_RN_NKCountryCode = uS.Code;
			Assert("Non matching UNLOCO and Country codes, error expected", UNLOCO.RL_RN_NKCountryCodeInfo.HasErrors());

			UNLOCO.RL_Code = "GBTTT";
			Assert("Matching UNLOCO and Country codes, no error", !UNLOCO.RL_RN_NKCountryCodeInfo.HasErrors());

			UNLOCO.RL_RN_NKCountryCode = ZString.Empty;
			Assert("Country code is empty, error expected", UNLOCO.RL_RN_NKCountryCodeInfo.HasErrors());
		}

		#endregion

		#region Port Name

		public void TestValidateRL_PortName()
		{
			UNLOCO.RL_PortName = "";
			Assert("Port Name empty, error expected", UNLOCO.RL_PortNameInfo.HasErrors());

			UNLOCO.RL_PortName = "Test Port";
			Assert("Port Name not empty, no error expected", !UNLOCO.RL_PortNameInfo.HasErrors());
		}

		#endregion

		#region Name With Diacriticals

		public void TestValidateRL_NameWithDiacriticals()
		{
			UNLOCO.RL_NameWithDiacriticals = "";
			Assert("Port Name empty, error expected", UNLOCO.RL_NameWithDiacriticalsInfo.HasErrors());

			UNLOCO.RL_NameWithDiacriticals = "Test Port";
			Assert("Port Name not empty, no error expected", !UNLOCO.RL_NameWithDiacriticalsInfo.HasErrors());
		}

		#endregion

		#region RL_IsUpdatable

		public void TestCheckRL_IsUpdatable()
		{
			Assert("Precondition", !UNLOCO.IsInDatabase);

			UNLOCO.RL_IsUpdatable = true;
			Assert(UNLOCO.RL_IsUpdatableInfo.HasWarning("All user's changes will be lost when data is updated from system reference source."));

			UNLOCO.RL_IsUpdatable = false;
			Assert(!UNLOCO.RL_IsUpdatableInfo.HasWarnings());

			UNLOCO.RL_IsUpdatable = true;
			Factory.Save();
			UNLOCO.Validation.ValidateRL_IsUpdatable();
			Assert(!UNLOCO.RL_IsUpdatableInfo.HasWarnings());

			UNLOCO.RL_IsUpdatable = false;
			UNLOCO.RL_IsUpdatable = true;
			Assert(UNLOCO.RL_IsUpdatableInfo.HasWarning("All user's changes will be lost when data is updated from system reference source."));
		}

		#endregion

		#region Implementation

		RefUNLOCO UNLOCO;

		protected override void SetUp()
		{
			base.SetUp();
			UNLOCO = Factory.New<RefUNLOCO>();
		}

		#endregion
	}
}
