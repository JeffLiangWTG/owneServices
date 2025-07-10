using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IndiaGSTCodeValidatorTest : TestCaseWithFactory
	{
		string CodeType => OrgCusCode.CodeTypes.GSTCode;
		string CountryCode => Core.Constants.CountryCodes.India;
		IDictionary<string, (ZString ValidGST, ZString InvalidGST, ZString InvalidLengthGST)> GSTPrefixValidInvalidMap =>
			new Dictionary<string, (ZString, ZString, ZString)>()
		{
			{ "AP", ("37AAACI1681G2ZN",
						"31AAACI1681G2ZN", "37AAACI1681G2Z") },
			{ "AN", ("35AAACI1681G1ZS",
						"34AAACI1681G1ZS", "35AAACI1681G1Z") },
			{ "AR", ("12AAACI1681G1Z0",
						"34AAACI1681G1ZS", "12AAACI1681G1Z") },
			{ "AS", ("18AAACI1681G1ZO",
						"34AAACI1681G1ZS", "18AAACI1681G1Z") },
			{ "BR", ("10AAACI1681G1Z4",
						"34AAACI1681G1ZS", "10AAACI1681G") },
			{ "CH", ("04AAACI1681G1ZX",
						"34AAACI1681G1ZS", "04AAACI1681G") },
			{ "CT", ("22AAACI1681G1ZZ",
						"34AAACI1681G1ZS", "22AAACI1681") },
			{ "DD", ("25AAACI1681G1ZT",
						"34AAACI1681G1ZS", "25AAACI1681G") },
			{ "DN", ("26AAACI1681G1ZR",
						"34AAACI1681G1ZS", "26AAACI1681G") },
			{ "DL", ("07AAACI1681G1ZR",
						"34AAACI1681G1ZS", "07AAACI1681") },
			{ "GA", ("30AAACI1681G1Z2",
						"34AAACI1681G1ZS", "30AAACI1681G") },
			{ "GJ", ("24AAACI1681G1ZV",
						"34AAACI1681G1ZS", "24AAACI1681G") },
			{ "HR", ("06AAACI1681G1ZT",
						"34AAACI1681G1ZS", "06AAACI1681") },
			{ "HP", ("02AAACI1681G3ZZ",
						"34AAACI1681G1ZS", "02AAACI1681") },
			{ "JH", ("20AAACI1681G3Z1",
						"34AAACI1681G1ZS", "20AAACI168") },
			{ "JK", ("01AAACI1681G2Z2",
						"34AAACI1681G1ZS", "01AAACI168") },
			{ "KA", ("29AAACI1681G1ZL",
						"34AAACI1681G1ZS", "29AAACI1681G") },
			{ "KL", ("32AAACI1681G1ZY",
						"34AAACI1681G1ZS","32AAACI1681G") },
			{ "LD", ("31AAACI1681G1ZY",
						"34AAACI1681G1ZS", "31AAACI1681G") },
			{ "MH", ("27AAACI1681G1ZY",
						"34AAACI1681G1ZS", "27AAACI1681G") },
			{ "MP", ("23AAACI1681G1ZY",
						"34AAACI1681G1ZS", "23AAACI1681G") },
			{ "ML", ("17AAACI1681G1ZY",
						"34AAACI1681G1ZS", "17AAACI1681G") },
			{ "MN", ("14AAACI1681G1ZY",
						"34AAACI1681G1ZS", "14AAACI1681G") },
			{ "MZ", ("15AAACI1681G1ZY",
						"34AAACI1681G1ZS", "15AAACI1681G") },
			{ "NL", ("13AAACI1681G1ZY",
						"34AAACI1681G1ZS", "13AAACI1681G") },
			{ "OR", ("21AAACI1681G1ZY",
						"34AAACI1681G1ZS", "21AAACI1681G") },
			{ "PY", ("34AAACI1681G1ZY",
						"31AAACI1681G1ZS", "34AAACI1681G") },
			{ "PB", ("03AAACI1681G1ZY",
						"31AAACI1681G1ZS", "03AAACI1681G") },
			{ "RJ", ("08AAACI1681G1ZY",
						"31AAACI1681G1ZS", "08AAACI1681G") },
			{ "SK", ("11AAACI1681G1ZY",
						"31AAACI1681G1ZS", "11AAACI1681G") },
			{ "TG", ("36AAACI1681G1ZY",
						"31AAACI1681G1ZS", "36AAACI1681G") },
			{ "TN", ("33AAACI1681G1ZY",
						"31AAACI1681G1ZS", "33AAACI1681G") },
			{ "TR", ("16AAACI1681G1ZY",
						"31AAACI1681G1ZS", "16AAACI1681G") },
			{ "UP", ("09AAACI1681G1ZY",
						"31AAACI1681G1ZS", "09AAACI1681G") },
			{ "UT", ("05AAACI1681G1ZY",
						"31AAACI1681G1ZS", "05AAACI1681G") },
			{ "WB", ("19AAACI1681G1ZY",
						"31AAACI1681G1ZS", "19AAACI1681G") },
			{ "DH", ("26AAACI1681G1ZY",
						"31AAACI1681G1ZS", "26AAACI1681G") }
		};

		public void TestGSTCodeAustrailia()
		{
			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode.OK_CodeType = CodeType;

			OrgHeader header = orgCusCode.Organisation;
			header.MainAddress.OA_RN_NKCountryCode = CountryCode;

			var tuple = GSTPrefixValidInvalidMap["AP"];
			header.MainAddress.State = "AP";
			orgCusCode.OK_CustomsRegNo = tuple.InvalidLengthGST;

			AssertHasError("OrgCusCode Should be having an Error Message because GST Code must be 15 digits long in India.",
					orgCusCode.OK_CustomsRegNoInfo,
					new ZString($"{CodeType} Numbers in India must be 15 characters long."));

			header.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			header.MainAddress.State = "NSW";
			orgCusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoErrors("OrgCusCode shouldn't have an error message because GST Prefix" +
				" and length constraint need not apply for organisation address out of India!",
				orgCusCode.OK_CustomsRegNoInfo);
		}

		public void TestGSTPrefixLookupCoversAllIndiaStates()
		{
			var validator = new IndiaGSTCodeValidator();

			var stateList = new OrgCodeLists().State_List(Factory, CountryCode).ToArray();
			foreach (var state in stateList)
			{
				Assert($"State code '{state.Code}' must be in the IndiaGSTCodeValidator.GSTPrefixLookup", validator.GSTPrefixLookup_ForTestOnly.ContainsKey(state.Code));
			}
		}

		public void TestGSTCodeIndiaStateWiseAndLength()
		{
			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode.OK_CodeType = CodeType;

			var header = orgCusCode.Organisation;
			header.MainAddress.OA_RN_NKCountryCode = CountryCode;

			var stateList = new OrgCodeLists().State_List(Factory, CountryCode).ToArray();
			foreach (var state in stateList)
			{
				Assert($"State '{state.Code}' must be present in GSTPrefixValidInvalidMap", GSTPrefixValidInvalidMap.ContainsKey(state.Code));
			}

			foreach (var stateInfo in GSTPrefixValidInvalidMap)
			{
				var tuple = GSTPrefixValidInvalidMap[stateInfo.Key];
				header.MainAddress.State = stateInfo.Key;
				AssertValidation(
					orgCusCode: orgCusCode,
					invalidValue: tuple.InvalidLengthGST,
					validValue: tuple.ValidGST,
					expectedMessage: new ZString($"{CodeType} Numbers in India must be 15 characters long.")
				);
				AssertValidation(
					orgCusCode: orgCusCode,
					invalidValue: tuple.InvalidGST,
					validValue: tuple.ValidGST,
					expectedMessage: new ZString($"This {CodeType} Number must begin with {tuple.ValidGST.Substring(0, 2)}")
				);
			}
			header.MainAddress.State = "KK";
			orgCusCode.Validation.ValidateAll();
			AssertHasError("OrgCusCode Should be having an Error Message.", orgCusCode.OK_CustomsRegNoInfo, "The state code KK is not a valid state for India");
			header.MainAddress.State = "KL";
			orgCusCode.Validation.ValidateAll();
			orgCusCode.OK_CustomsRegNo = "32AAACI1681G1ZY";
			AssertNoErrors("OrgCusCode shouldn't have an error message!", orgCusCode.OK_CustomsRegNoInfo);
		}

		void AssertValidation(
			OrgCusCode orgCusCode,
			ZString invalidValue,
			ZString validValue,
			ZString expectedMessage
		)
		{
			orgCusCode.OK_CustomsRegNo = invalidValue;
			AssertHasError("OrgCusCode Should be having an Error Message.", orgCusCode.OK_CustomsRegNoInfo, expectedMessage);

			orgCusCode.OK_CustomsRegNo = validValue;
			AssertNoErrors("OrgCusCode shouldn't have an error message!", orgCusCode.OK_CustomsRegNoInfo);
		}
	}
}
