using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PhoneNumberFormatAndValidationTest : TestCaseWithFactory
	{
		public void TestUseOptionalPrefixFormatting()
		{
			BizO.Port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NLAAL"));
			BizO.PhoneNumberValidator.PhNumber = "+31205000390";
			BizO.PhoneNumberValidator.ConvertToGeneralFormat();

			// Optional Prefix Formatting is not mandatory now. According to CS00078553
			// I.e. if user sets it - we keep it, but don't put it ourselves if it's absent
			AssertEquals("Incorrect formatting", BizO.PhoneNumberValidator.PhNumber, "+31 205000390");

			BizO.PhoneNumberValidator.PhNumber = "+31(0)205000390";
			BizO.PhoneNumberValidator.ConvertToGeneralFormat();
			AssertEquals("Incorrect formatting", BizO.PhoneNumberValidator.PhNumber, "+31 (0)205000390");
		}

		public void TestDoNotFormatLocalNumber()
		{
			RefUNLOCO london = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));
			BizO.PhoneNumberValidator.ConvertToInternationalFormattedPhoneNumber("1376 565100", london);
			AssertEquals("Incorrect formatting", "+44 1376 565100", BizO.PhoneNumberValidator.PhNumber);

			BizO.PhoneNumberValidator.ConvertToInternationalFormattedPhoneNumber("+44 1376 565100", london);
			AssertEquals("Incorrect formatting", "+44 1376 565100", BizO.PhoneNumberValidator.PhNumber);
		}

		#region Perform Number Validation

		public void TestPerformNumberValidation()
		{
			DummyBusinessObjectForPhoneValidation bizOPortValidationForced = new DummyBusinessObjectForPhoneValidation(true);
			DummyBusinessObjectForPhoneValidation bizOPortValidationNOTForced = new DummyBusinessObjectForPhoneValidation(false);

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			Validator.TestBranch = SydneyBranch;

			bizOPortValidationForced.Port = null;
			bizOPortValidationNOTForced.Port = null;

			bizOPortValidationForced.PhoneNum = ZString.Empty;
			AssertNoErrors("Phone number is empty, should not have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = ZString.Empty;
			AssertNoErrors("Phone number is empty, should not have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			bizOPortValidationForced.PhoneNum = "+61 (2) 9025-1199";
			AssertHasErrors("Formatting on, Phone number is valid, Port is empty, port validation is forced, should have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = "+61 (2) 9025-1199";
			AssertNoErrors("Formatting on, Phone number is valid, Port is empty, port validation is NOT forced, should NOT have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			bizOPortValidationForced.PhoneNum = "sdfsdf";
			AssertHasErrors("Formatting on, Phone number is NOT valid, Port is empty, port validation is forced, should have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = "sdfsdf";
			AssertHasErrors("Formatting on, Phone number is NOT valid, Port is empty, port validation is NOT forced, should have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			bizOPortValidationForced.Port = Sydney;
			bizOPortValidationNOTForced.Port = Sydney;

			bizOPortValidationForced.PhoneNum = "+61 (2) 9025-1199";
			AssertNoErrors("Formatting on, Phone number is valid, Port is valid, port validation is forced, should have NOT have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = "+61 (2) 9025-1199";
			AssertNoErrors("Formatting on, Phone number is valid, Port is valid, port validation is NOT forced, should NOT have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			bizOPortValidationForced.PhoneNum = "ddd";
			AssertHasErrors("Formatting on, Phone number is NOT valid, Port is valid, port validation is forced, should have NOT have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = "ddd";
			AssertHasErrors("Formatting on, Phone number is NOT valid, Port is valid, port validation is NOT forced, should NOT have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			Env.Registry.SetOrgUsePhoneNumberFormatting(false);

			bizOPortValidationForced.Port = null;
			bizOPortValidationNOTForced.Port = null;

			bizOPortValidationForced.PhoneNum = ZString.Empty;
			AssertNoErrors("Phone number is empty, should not have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = ZString.Empty;
			AssertNoErrors("Phone number is empty, should not have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			bizOPortValidationForced.PhoneNum = "+61 (2) 9025-1199";
			AssertNoErrors("Formatting off, Phone number is valid, Port is empty, port validation is forced, should NOT have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = "+61 (2) 9025-1199";
			AssertNoErrors("Formatting off, Phone number is valid, Port is empty, port validation is NOT forced, should NOT have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			bizOPortValidationForced.PhoneNum = "sdfsdf";
			AssertHasErrors("Formatting off, Phone number is NOT valid, Port is empty, port validation is forced, should have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = "sdfsdf";
			AssertHasErrors("Formatting off, Phone number is NOT valid, Port is empty, port validation is NOT forced, should have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			bizOPortValidationForced.Port = Sydney;
			bizOPortValidationNOTForced.Port = Sydney;

			bizOPortValidationForced.PhoneNum = "+61 (2) 9025-1199";
			AssertNoErrors("Formatting off, Phone number is valid, Port is valid, port validation is forced, should have NOT have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = "+61 (2) 9025-1199";
			AssertNoErrors("Formatting off, Phone number is valid, Port is valid, port validation is NOT forced, should NOT have errors", bizOPortValidationNOTForced.PhoneNumInfo);

			bizOPortValidationForced.PhoneNum = "ddd";
			AssertHasErrors("Formatting off, Phone number is NOT valid, Port is valid, port validation is forced, should have NOT have errors", bizOPortValidationForced.PhoneNumInfo);

			bizOPortValidationNOTForced.PhoneNum = "ddd";
			AssertHasErrors("Formatting off, Phone number is NOT valid, Port is valid, port validation is NOT forced, should NOT have errors", bizOPortValidationNOTForced.PhoneNumInfo);
		}

		public void TestContainsOnlyValidCharacters()
		{
			Assert("Contains only valid phone number characters", Validator.ContainsOnlyValidCharacters("1"));
			Assert("Contains only valid phone number characters", Validator.ContainsOnlyValidCharacters("+1 43543"));
			Assert("Contains only valid phone number characters", Validator.ContainsOnlyValidCharacters("142343243244234"));
			Assert("Contains characters that are NOT valid phone number characters", !Validator.ContainsOnlyValidCharacters("32432L"));
			Assert("Contains two +'s, NOT a valid phone number", !Validator.ContainsOnlyValidCharacters("++4343"));
			Assert("Plus symbol only valid as a prefix", !Validator.ContainsOnlyValidCharacters("43+43"));
			Assert("Plus symbol only valid as a prefix", !Validator.ContainsOnlyValidCharacters("4343+"));
		}

		[ExpectNoExceptions]
		public void TestValidateFaxNumberForUnlocoWithNoStates()
		{
			RefUNLOCO newUNLOCO = Factory.New<RefUNLOCO>();
			newUNLOCO.RL_Code = "CHIPS";
			newUNLOCO.RL_RN_NKCountryCode = "AU";
			newUNLOCO.RL_IsActive = true;

			AssertNull("NewUNLOCO has no states", newUNLOCO.CountryStates);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = newUNLOCO.RL_Code;
			org.MainAddress.OA_Fax = "1234567";
		}

		public void TestCheckValidPhoneNumber()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);

			BizO.Port = null;
			Validator.TestBranch = SydneyBranch;
			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("321232323", Sydney);
			Assert("Phone number should have errors", BizO.PhoneNumInfo.HasErrors());

			BizO.Port = Sydney;

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber(ZString.Empty, Sydney);
			Assert("Phone number shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61290251199", Sydney);
			AssertEquals("Expecting Phone number to format to \"+61 (2) 9025-1199\"", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61(2)90251199", Sydney);
			AssertEquals("Expecting Phone number to format to \"+61 (2) 9025-1199\"", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61290251199", Sydney);
			AssertEquals("Expecting phone number to format to \"+61 (2) 9025-1199\"", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+ 61 2 9025 1199", Sydney);
			AssertEquals("Expecting Phone number to format to \"+61 (2) 9025-1199\"", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+ 61 2 9025-1199", Sydney);
			AssertEquals("Expecting Phone number to format to \"+61 (2) 9025-1199\"", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61-2-9025-1199 ", Sydney);
			AssertEquals("Expecting Phone number to format to \"+61 (2) 9025-1199\"", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("02-9025-1100 ", Sydney);
			AssertEquals("Expecting Phone number to format to \"+61 (2) 9025-1100\"", new ZString("+61 (2) 9025-1100"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("0290251100", Sydney);
			AssertEquals("Expecting Phone number to format to \"+61 (2) 9025-1100\"", new ZString("+61 (2) 9025-1100"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("90251100", Sydney);
			AssertEquals("Expecting Phone number to format to \"+61 (2) 9025-1100\"", new ZString("+61 (2) 9025-1100"), BizO.PhoneNum);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("@#$", Sydney);
			Assert("Phone number  should have errors.", BizO.PhoneNumInfo.HasErrors());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61290251199", Sydney);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61(2)90251199", Sydney);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+(61)290251199", Sydney);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+ 61 2 9025 1199", Sydney);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+ 61 2 9025-1199", Sydney);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+65290251199", Sydney);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  should have warnings.", BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61-2-9025-1199 ", Sydney);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  shouldn't have warnings.", !BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+616453988", Sydney);
			Assert("Phone number  shouldn't have errors.", !BizO.PhoneNumInfo.HasErrors());
			Assert("Phone number  should have warnings.", BizO.PhoneNumInfo.HasWarnings());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61 (2)y 9025 1199", Sydney);
			Assert("Phone number  should have errors.", BizO.PhoneNumInfo.HasErrors());

			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("+61 (2)aaaa aaaa", Sydney);
			Assert("Phone number  should have errors.", BizO.PhoneNumInfo.HasErrors());

			BizO.Port = NewYork;
			BizO.PhoneNum = Validator.ConvertToInternationalFormattedPhoneNumber("924 1659", NewYork);
			Assert("Phone number  should have errors.", BizO.PhoneNumInfo.HasErrors());
			AssertEquals("Phone number  error", "An area code needs to be entered for this number.", BizO.PhoneNumInfo.GetErrors().GetFirstMessage());
		}

		#endregion

		#region ConvertToInternationalFormattedPhoneNumber

		public void TestPhoneNumberWithNoFormattingAndAreaCodeEntered()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			RefUNLOCO unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("DEAAC"));

			AssertEquals("German phone number with brackets", "+49 (4216) 5552155", Validator.ConvertToInternationalFormattedPhoneNumber("+49 (4216) 5552155", unloco));
			AssertEquals("German phone number with brackets", "+49 4216 5552155", Validator.ConvertToInternationalFormattedPhoneNumber("+49 4216 5552155", unloco));
		}

		[ExpectNoExceptions]
		public void TestConvertToInternationalFormattedPhoneNumberFromUNLOCOWithNoStates()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			RefUNLOCO uNLOCOWithNoStates = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("UAIEV"));
			AssertEquals("UNLOCO for Kiev should not have states", 0, uNLOCOWithNoStates.Country.States.Count);

			Validator.TestBranch = SydneyBranch;
			BizO.Port = uNLOCOWithNoStates;

			ZString rawPhoneNumber = ZString.Empty;
			ZString result = ZString.Empty;

			rawPhoneNumber = "380442922219";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, uNLOCOWithNoStates);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as Ukrainian number.", new ZString("+380 (44) 292-2219"), BizO.PhoneNum);
		}

		public void TestConvertToInternationFormattedPhoneNumberByCountry()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			Validator.TestBranch = SydneyBranch;

			CombineAssertions(delegate
			{
				DoTestConvert("Mexico", "Baja California", "Tijuana", "+52 (664) 765-4321", "526647654321", "6647654321", "016647654321", "7654321");
				DoTestConvert("Mexico", "Jalisco", "Guadalajara", "+52 (33) 765-4321", "52337654321", "337654321", "01337654321", "7654321");
				// DoTestConvert("Mexico", null, null, "+52 (33) 1-765-4321", "0133 17654321"); cant' handle cell phones correctly

				DoTestConvert("China", "Shandong", "Qingdao", "+86 (532) 6532-9107", "8653265329107", "53265329107", "65329107");
				DoTestConvert("China", "Shanghai", "Shanghai", "+86 (21) 6532-9107", "862165329107", "2165329107", "65329107");

				DoTestConvert("Albania", null, null, "+355 4 23 79 063", "04 23 79 063");
				DoTestConvert("Albania", null, null, "+355 68 202 2388", "068 202 2388");

				// Can't handle Argentina with current architeture, a pattern based approach would be much better
				//DoTestConvert("Argentina", null, null, "+54 9 (11) 4321-4321", "011 15 4321-4321"); // Buenos Aires cell phone
				//DoTestConvert("Argentina", "Buenos Aires", "Buenos Aires", "+54 (11) 4371-2004", "(011) 4371-2004", "01143712004");
				//DoTestConvert("Argentina", "San Juan", "San Juan", "+54 (264) 422-3623", "(0246) 422-3623", "2464223623");
			});
		}

		void DoTestConvert(string country, string state, string portName, ZString expected, params ZString[] inputs)
		{
			DoTestConvert(country, state, portName, false, false, expected, inputs);
		}

		void DoTestConvert(string countryName, string state, string portName, bool expectErrors, bool expectWarnings, ZString expected, params ZString[] inputs)
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = RefCountry.LoadFromCountryName(Factory, countryName).Code;
			if (!string.IsNullOrEmpty(state))
			{
				foreach (RefCountryStates refstate in unloco.Country.States)
				{
					if (refstate.RW_Description.EqualsIgnoringCase(state))
					{
						unloco.RL_RW = refstate.PK;
						break;
					}
				}
			}
			if (!string.IsNullOrEmpty(portName))
			{
				unloco.RL_PortName = portName;
			}
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			ZString[] allInputs = new ZString[inputs.Length + 1];
			Array.Copy(inputs, allInputs, inputs.Length);
			allInputs[inputs.Length] = expected;
			foreach (ZString input in allInputs)
			{
				ZString result = Validator.ConvertToInternationalFormattedPhoneNumber(input, unloco);
				AssertEquals(string.Format("Converstion of {0} phone number '{1}'", countryName, input), expected, result);
				bizO.Z0_NVarChar = result;
				Validator.CheckValidPhoneNumber(bizO.Z0_NVarCharInfo, unloco);
				AssertEquals(string.Format("Validiation errors for {0} phone number '{1}'\r\n{2}", countryName, result, bizO.Z0_NVarCharInfo.GetErrors().ToMessageListString()), expectErrors, bizO.Z0_NVarCharInfo.HasErrors());
				AssertEquals(string.Format("Validiation warnings for {0} phone number '{1}'\r\n{2}", countryName, result, bizO.Z0_NVarCharInfo.GetWarnings().ToMessageListString()), expectWarnings, bizO.Z0_NVarCharInfo.HasWarnings());
			}
		}

		#region SydneyUNLOCO

		public void TestConvertToInternationalFormattedPhoneNumberFromSydneyBranch()
		{
			Validator.TestBranch = SydneyBranch;

			ZString rawPhoneNumber = ZString.Empty;
			ZString result = ZString.Empty;

			rawPhoneNumber = "0290251199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as Sydney number.", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "001115555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "+15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as Australian number.", new ZString("+61 (*) 155-5555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "222222";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("No area code entered, should format as Australian number.", new ZString("+61 (*) 22-2222"), BizO.PhoneNum);

			rawPhoneNumber = "111";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Not enough digits, phone number should not format.", new ZString("111"), BizO.PhoneNum);

			rawPhoneNumber = "1800 234 234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 1-800-234-234"), BizO.PhoneNum);

			rawPhoneNumber = "0402295109";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 (402) 295-109"), BizO.PhoneNum);

			rawPhoneNumber = "08 9025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 (8) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "3 9025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 (3) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "9025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "131313";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 131-313"), BizO.PhoneNum);

			rawPhoneNumber = "61131313";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 131-313"), BizO.PhoneNum);

			rawPhoneNumber = "+611800123456";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+61 1-800-123-456\"", new ZString("+61 1-800-123-456"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+12121234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1-212-123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 212 1234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123 4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 2 1 2 1 23 4 5 6 7";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+852 2851 7653";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, HongKong);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+852 2851-7653\"", new ZString("+852 2851-7653"), BizO.PhoneNum);

			rawPhoneNumber = "+64 9 262 4340";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (9) 262-4340\"", new ZString("+64 (9) 262-4340"), BizO.PhoneNum);

			rawPhoneNumber = "+64 21 947 854";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (21) 947-854\"", new ZString("+64 (21) 947-854"), BizO.PhoneNum);

			rawPhoneNumber = "001112122341234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 234-1234\"", new ZString("+1 (212) 234-1234"), BizO.PhoneNum);

			rawPhoneNumber = "(09) 3061870";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (9) 306-1870\"", new ZString("+64 (9) 306-1870"), BizO.PhoneNum);

			rawPhoneNumber = "(02) 80012213";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+61 (2) 8001-2213\"", new ZString("+61 (2) 8001-2213"), BizO.PhoneNum);

			rawPhoneNumber = "(044) 2922219";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Kiev);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+380 (044) 292-2219\"", new ZString("+380 (044) 292-2219"), BizO.PhoneNum);
		}

		#endregion

		#region NewYork UNLOCO

		public void TestConvertToInternationalFormattedPhoneNumberFromNewYorkBranch()
		{
			Validator.TestBranch = NewYorkBranch;

			ZString rawPhoneNumber = ZString.Empty;
			ZString result = ZString.Empty;

			rawPhoneNumber = "5555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "+61290251199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as Aus number.", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "01161290251199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as Aus number.", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "61290251199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (***) 612-9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "111";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Not enough digits, phone number should not format.", new ZString("111"), BizO.PhoneNum);

			rawPhoneNumber = "222222";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("No area code entered, should format as US number.", new ZString("+1 (***) 22-2222"), BizO.PhoneNum);

			rawPhoneNumber = "61 1800 234 234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 1-800-234-234"), BizO.PhoneNum);

			rawPhoneNumber = "61 2 9025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "213 025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, LosAngeles);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+1 (213) 025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, LosAngeles);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+1 (***) 025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "925 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+1 (***) 925-1199"), BizO.PhoneNum);

			rawPhoneNumber = "212 925 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+1 (212) 925-1199"), BizO.PhoneNum);

			rawPhoneNumber = "+611800123456";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+61 1-800-123-456\"", new ZString("+61 1-800-123-456"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+12121234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1-212-123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 212 1234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123 4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 2 1 2 1 23 4 5 6 7";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+852 2851 7653";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, HongKong);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+852 2851-7653\"", new ZString("+852 2851-7653"), BizO.PhoneNum);

			rawPhoneNumber = "011 852 2851 7653";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, HongKong);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+852 2851-7653\"", new ZString("+852 2851-7653"), BizO.PhoneNum);

			rawPhoneNumber = "+64 9 262 4340";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (9) 262-4340\"", new ZString("+64 (9) 262-4340"), BizO.PhoneNum);

			rawPhoneNumber = "01164 21 947 854";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (21) 947-854\"", new ZString("+64 (21) 947-854"), BizO.PhoneNum);

			rawPhoneNumber = "01112122341234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 234-1234\"", new ZString("+1 (212) 234-1234"), BizO.PhoneNum);
		}

		#endregion

		#region HongKong UNLOCO

		public void TestConvertToInternationalFormattedPhoneNumberFromHongKongBranch()
		{
			Validator.TestBranch = HongKongBranch;

			ZString rawPhoneNumber = ZString.Empty;
			ZString result = ZString.Empty;

			rawPhoneNumber = "+15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as HK number.", new ZString("+852 155-5555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "00115555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "111";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Not enough digits, phone number should not format.", new ZString("111"), BizO.PhoneNum);

			rawPhoneNumber = "61 1800 234 234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 1-800-234-234"), BizO.PhoneNum);

			rawPhoneNumber = "61 2 9025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "2325 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, HongKong);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+852 2325-1199"), BizO.PhoneNum);

			rawPhoneNumber = "212 925 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+1 (212) 925-1199"), BizO.PhoneNum);

			rawPhoneNumber = "+611800123456";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+61 1-800-123-456\"", new ZString("+61 1-800-123-456"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+12121234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1-212-123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 212 1234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123 4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 2 1 2 1 23 4 5 6 7";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+852 2851 7653";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, HongKong);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+852 2851-7653\"", new ZString("+852 2851-7653"), BizO.PhoneNum);

			rawPhoneNumber = "001 1 (212) 123 4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+64 9 262 4340";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (9) 262-4340\"", new ZString("+64 (9) 262-4340"), BizO.PhoneNum);

			rawPhoneNumber = "00164 21 947 854";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (21) 947-854\"", new ZString("+64 (21) 947-854"), BizO.PhoneNum);

			rawPhoneNumber = "00112122341234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 234-1234\"", new ZString("+1 (212) 234-1234"), BizO.PhoneNum);
		}

		#endregion

		#region NZ UNLOCO

		public void TestConvertToInternationalFormattedPhoneNumberFromNZBranch()
		{
			Validator.TestBranch = NZBranch;

			ZString rawPhoneNumber = ZString.Empty;
			ZString result = ZString.Empty;

			rawPhoneNumber = "+15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as NZ number.", new ZString("+64 (*) 155-5555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "0015555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "111";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Not enough digits, phone number should not format.", new ZString("111"), BizO.PhoneNum);

			rawPhoneNumber = "222222";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("No area code entered, should format as NZ number.", new ZString("+64 (*) 22-2222"), BizO.PhoneNum);

			rawPhoneNumber = "61 1800 234 234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 1-800-234-234"), BizO.PhoneNum);

			rawPhoneNumber = "61 2 9025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 (2) 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "262 4341";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+64 (9) 262-4341"), BizO.PhoneNum);

			rawPhoneNumber = "212 925 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+1 (212) 925-1199"), BizO.PhoneNum);

			rawPhoneNumber = "+611800123456";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+61 1-800-123-456\"", new ZString("+61 1-800-123-456"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+12121234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1-212-123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 212 1234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123 4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 2 1 2 1 23 4 5 6 7";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+852 2851 7653";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, HongKong);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+852 2851-7653\"", new ZString("+852 2851-7653"), BizO.PhoneNum);

			rawPhoneNumber = "00 1 (212) 123 4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+64 9 262 4340";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (9) 262-4340\"", new ZString("+64 (9) 262-4340"), BizO.PhoneNum);

			rawPhoneNumber = "0064 21 947 854";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (21) 947-854\"", new ZString("+64 (21) 947-854"), BizO.PhoneNum);

			rawPhoneNumber = "0012122341234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 234-1234\"", new ZString("+1 (212) 234-1234"), BizO.PhoneNum);
		}

		#endregion

		#region Singapore UNLOCO

		public void TestConvertToInternationalFormattedPhoneNumberFromSingaporeBranch()
		{
			Validator.TestBranch = SingaporeBranch;

			ZString rawPhoneNumber = ZString.Empty;
			ZString result = ZString.Empty;

			rawPhoneNumber = "+15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "15555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as SG number.", new ZString("+65 155-5555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "00115555555555";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as US number.", new ZString("+1 (555) 555-5555"), BizO.PhoneNum);

			rawPhoneNumber = "111";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, null);
			BizO.PhoneNum = result;
			AssertEquals("Not enough digits, phone number should not format.", new ZString("111"), BizO.PhoneNum);

			rawPhoneNumber = "61 1800 234 234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+61 1-800-234-234"), BizO.PhoneNum);

			rawPhoneNumber = "65 9025 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Singapore);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+65 9025-1199"), BizO.PhoneNum);

			rawPhoneNumber = "2262 4341";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Singapore);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+65 2262-4341"), BizO.PhoneNum);

			rawPhoneNumber = "212 925 1199";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting phone number to format as expected.", new ZString("+1 (212) 925-1199"), BizO.PhoneNum);

			rawPhoneNumber = "+611800123456";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Sydney);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+61 1-800-123-456\"", new ZString("+61 1-800-123-456"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+12121234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1-212-123-4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 212 1234567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 (212) 123 4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+1 2 1 2 1 23 4 5 6 7";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+852 2851 7653";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, HongKong);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+852 2851-7653\"", new ZString("+852 2851-7653"), BizO.PhoneNum);

			rawPhoneNumber = "001 1 (212) 123 4567";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 123-4567\"", new ZString("+1 (212) 123-4567"), BizO.PhoneNum);

			rawPhoneNumber = "+64 9 262 4340";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (9) 262-4340\"", new ZString("+64 (9) 262-4340"), BizO.PhoneNum);

			rawPhoneNumber = "00164 21 947 854";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, Auckland);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+64 (21) 947-854\"", new ZString("+64 (21) 947-854"), BizO.PhoneNum);

			rawPhoneNumber = "00112122341234";
			result = Validator.ConvertToInternationalFormattedPhoneNumber(rawPhoneNumber, NewYork);
			BizO.PhoneNum = result;
			AssertEquals("Expecting fax number to format to \"+1 (212) 234-1234\"", new ZString("+1 (212) 234-1234"), BizO.PhoneNum);
		}

		#endregion

		#endregion

		#region Notification level

		public void TestNotificationLevel()
		{
			TestPhoneNumberValidator validator = new TestPhoneNumberValidator(CargoWise.EntityFramework.NotificationType.Warning);
			AssertEquals(CargoWise.EntityFramework.NotificationType.Warning, validator.NotificationLevel);
		}

		#endregion

		public void TestDoesntBlowupWhenGettingTargetCountryDataRowForSerbia()
		{
			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "RSBEG"));
			Validator.TargetUNLOCO = unloco;

			AssertNoExceptionThrown("Shouldn't blow up when there is no phone info for the country.", () => { Validator.ConvertToInternationalFormattedPhoneNumber("+ 38117115195", unloco); });
		}

		public void TestOA_PhoneForPitcairnWhichIsNotAvailableInPhoneInfoClass()
		{
			var company = Factory.NewWithValidTestData<OrgHeader>();
			var address = company.Addresses.AddNew();

			company.OH_RL_NKClosestPort = null;

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);

			address.OA_RL_NKRelatedPortCode = "PNPCN";
			AssertNoExceptionThrown("Should be no exception", () => address.OA_Phone = "21 666666");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			PreviousPhoneNumberFormattingRegistrySetting = Env.Registry.OrgUsePhoneNumberFormatting;

			Sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUSYD"));
			NewYork = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("USNYC"));
			HongKong = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("HKHKG"));
			Auckland = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("NZAKL"));
			LosAngeles = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("USLAX"));
			Singapore = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("SGSIN"));
			Kiev = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("UAIEV"));

			BizO = new DummyBusinessObjectForPhoneValidation(true);
			Validator = new TestPhoneNumberValidator();

			Company = Factory.New<GlbCompany>();

			SydneyBranch = Factory.New<GlbBranch>();
			SydneyBranch.GB_GC = Company.PK;
			SydneyBranch.GB_RL_NKHomePort = "AUSYD";
			SydneyBranch.GB_Phone = "+61 (2) 9025-1100";

			NewYorkBranch = Factory.New<GlbBranch>();
			NewYorkBranch.GB_GC = Company.PK;
			NewYorkBranch.GB_RL_NKHomePort = "USNYC";
			NewYorkBranch.GB_Fax = "+1 (212) 654-2131";

			HongKongBranch = Factory.New<GlbBranch>();
			HongKongBranch.GB_GC = Company.PK;
			HongKongBranch.GB_RL_NKHomePort = "HKHKG";
			HongKongBranch.GB_Phone = "+852 2851-7653";

			NZBranch = Factory.New<GlbBranch>();
			NZBranch.GB_GC = Company.PK;
			NZBranch.GB_RL_NKHomePort = "NZAKL";
			NZBranch.GB_Phone = "+64 (9) 262-4340";

			SingaporeBranch = Factory.New<GlbBranch>();
			SingaporeBranch.GB_GC = Company.PK;
			SingaporeBranch.GB_RL_NKHomePort = "SGSIN";
			SingaporeBranch.GB_Phone = "+65 6247-9420";
		}

		bool PreviousPhoneNumberFormattingRegistrySetting;
		RefUNLOCO Sydney;
		RefUNLOCO Kiev;
		RefUNLOCO NewYork;
		RefUNLOCO HongKong;
		RefUNLOCO Auckland;
		RefUNLOCO LosAngeles;
		RefUNLOCO Singapore;

		DummyBusinessObjectForPhoneValidation BizO;
		GlbCompany Company;
		GlbBranch SydneyBranch;
		GlbBranch NewYorkBranch;
		GlbBranch HongKongBranch;
		GlbBranch NZBranch;
		GlbBranch SingaporeBranch;

		protected override void TearDown()
		{
			base.TearDown();
			Env.Registry.SetOrgUsePhoneNumberFormatting(PreviousPhoneNumberFormattingRegistrySetting);
		}

		TestPhoneNumberValidator Validator;

		public class TestPhoneNumberValidator : PhoneNumberFormatAndValidation
		{
			public TestPhoneNumberValidator()
				: base()
			{
				TestBranch = Factory.New<GlbBranch>();
			}

			public TestPhoneNumberValidator(INotificationType notificationLevel)
				: base(notificationLevel)
			{
			}

			public new INotificationType NotificationLevel
			{
				get { return base.NotificationLevel; }
			}

			protected override GlbBranch CurrentBranch
			{
				get { return TestBranch; }
			}

			public GlbBranch TestBranch;
		}

		#region DummyBusinessObject

		public class DummyBusinessObjectForPhoneValidation : NonPersistentBusinessObject, IObsoleteValidation
		{
			public DummyBusinessObjectForPhoneValidation(bool portValidationForced)
				: base()
			{
				this.PortValidationForced = portValidationForced;
			}

			readonly bool PortValidationForced;

			#region PhoneNum

			ZString fPhoneNum;
			public ZString PhoneNum
			{
				get { return fPhoneNum; }
				set
				{
					if (fPhoneNum != value)
					{
						fPhoneNum = value;
						ValidatePhoneNum();
					}
				}
			}

			public ZPropertyInfo PhoneNumInfo
			{
				get { return GetZPropertyInfo(nameof(PhoneNum)); }
			}

			public virtual void ValidatePhoneNum()
			{
				PhoneNumInfo.ClearAllNotifications();
				PhoneNumberValidator.PerformNumberValidation(PhoneNumInfo, Port, PortValidationForced);
			}

			#endregion

			protected PhoneNumberFormatAndValidationForTest fPhoneNumberValidator;
			public PhoneNumberFormatAndValidationForTest PhoneNumberValidator
			{
				get
				{
					if (fPhoneNumberValidator == null)
					{
						fPhoneNumberValidator = new PhoneNumberFormatAndValidationForTest();
					}
					return fPhoneNumberValidator;
				}
			}

			RefUNLOCO fPort;
			public RefUNLOCO Port
			{
				get { return fPort; }
				set { fPort = value; }
			}
		}

		public class PhoneNumberFormatAndValidationForTest : PhoneNumberFormatAndValidation
		{
			public new String PhNumber
			{
				get { return base.PhNumber; }
				set { base.PhNumber = value; }
			}

			public new void ConvertToGeneralFormat()
			{
				base.ConvertToGeneralFormat();
			}
		}

		#endregion

		#endregion
	}
}
