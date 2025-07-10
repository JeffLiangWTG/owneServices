using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CAGCodeIssuerTest : BusinessObjectValidationTestCase
	{
		public void TestCAGCodeIssuer_AssertUpdateListWithCAGCodeIfNeeded_WhenCountryCAGCodeIssuer()
		{
			var countryCodesCAGCodeNeeded = new List<string>()
			{
				Constants.CountryCodes.Afghanistan,
				Constants.CountryCodes.Albania,
				Constants.CountryCodes.Argentina,
				Constants.CountryCodes.Australia,
				Constants.CountryCodes.Austria,
				Constants.CountryCodes.Belgium,
				Constants.CountryCodes.BosniaAndHerzegovina,
				Constants.CountryCodes.Brazil,
				Constants.CountryCodes.Brunei,
				Constants.CountryCodes.Bulgaria,
				Constants.CountryCodes.Canada,
				Constants.CountryCodes.Chile,
				Constants.CountryCodes.Colombia,
				Constants.CountryCodes.Croatia,
				Constants.CountryCodes.CzechRepublic,
				Constants.CountryCodes.Denmark,
				Constants.CountryCodes.Egypt,
				Constants.CountryCodes.Estonia,
				Constants.CountryCodes.Fiji,
				Constants.CountryCodes.Finland,
				Constants.CountryCodes.France,
				Constants.CountryCodes.Georgia,
				Constants.CountryCodes.Germany,
				Constants.CountryCodes.Greece,
				Constants.CountryCodes.Hungary,
				Constants.CountryCodes.Iceland,
				Constants.CountryCodes.India,
				Constants.CountryCodes.Indonesia,
				Constants.CountryCodes.Israel,
				Constants.CountryCodes.Italy,
				Constants.CountryCodes.Japan,
				Constants.CountryCodes.Jordan,
				Constants.CountryCodes.KoreaSouth,
				Constants.CountryCodes.Kuwait,
				Constants.CountryCodes.Latvia,
				Constants.CountryCodes.Lithuania,
				Constants.CountryCodes.Luxembourg,
				Constants.CountryCodes.Macedonia,
				Constants.CountryCodes.Malaysia,
				Constants.CountryCodes.Montenegro,
				Constants.CountryCodes.Morocco,
				Constants.CountryCodes.Netherlands,
				Constants.CountryCodes.NewZealand,
				Constants.CountryCodes.Norway,
				Constants.CountryCodes.Oman,
				Constants.CountryCodes.Pakistan,
				Constants.CountryCodes.PapuaNewGuinea,
				Constants.CountryCodes.Peru,
				Constants.CountryCodes.Philippines,
				Constants.CountryCodes.Poland,
				Constants.CountryCodes.Portugal,
				Constants.CountryCodes.Romania,
				Constants.CountryCodes.Russia,
				Constants.CountryCodes.SaudiArabia,
				Constants.CountryCodes.Serbia,
				Constants.CountryCodes.Singapore,
				Constants.CountryCodes.Slovakia,
				Constants.CountryCodes.Slovenia,
				Constants.CountryCodes.SouthAfrica,
				Constants.CountryCodes.Spain,
				Constants.CountryCodes.Sweden,
				Constants.CountryCodes.Thailand,
				Constants.CountryCodes.Tonga,
				Constants.CountryCodes.Turkey,
				Constants.CountryCodes.Ukraine,
				Constants.CountryCodes.UnitedArabEmirates,
				Constants.CountryCodes.UnitedKingdom,
				Constants.CountryCodes.UnitedStates,
			};
			var refCountry = Factory.New<RefCountry>();
			CombineAssertions(() =>
			{
				foreach (var countryCode in countryCodesCAGCodeNeeded)
				{
					refCountry.RN_Code = countryCode;
					var list = new OrgCodeLists().CustomsCodes_List(refCountry);

					CAGCodeIssuer.UpdateListWithCAGCodeIfNeeded(list, refCountry);

					AssertEquals($@"CustomsCodes_List of country {countryCode} should contain CAG Code", true, list.ContainsCode(OrgCusCode.CodeTypes.CommercialAndGovernmentEntity));
				}
			});
		}

		public void TestCAGCodeIssuer_AssertNotUpdateListWithCAGCodeIfNotNeeded_WhenCountryNotCAGCodeIssuer()
		{
			var someCountryCodesCAGCodeNotNeeded = new List<string>()
			{
				Constants.CountryCodes.AlandIslands,
				Constants.CountryCodes.Uzbekistan,
				Constants.CountryCodes.Ethiopia,
				Constants.CountryCodes.FrenchSouthernTerritories,
				Constants.CountryCodes.Gabon,
				Constants.CountryCodes.Nicaragua,
				Constants.CountryCodes.Zimbabwe
			};
			var refCountry = Factory.New<RefCountry>();
			CombineAssertions(() =>
			{
				foreach (var countryCode in someCountryCodesCAGCodeNotNeeded)
				{
					refCountry.RN_Code = countryCode;
					var list = new OrgCodeLists().CustomsCodes_List(refCountry);
					CAGCodeIssuer.UpdateListWithCAGCodeIfNeeded(list, refCountry);
					AssertEquals($@"CustomsCodes_List of country {countryCode} should not contain CAG Code", false, list.ContainsCode(OrgCusCode.CodeTypes.CommercialAndGovernmentEntity));
				}
			});
		}

		public void TestCAGCodeIssuer_AssertUpdateListWithCAGCodeIfNeeded_WhenCountryIsNull()
		{
			var list = new OrgCodeLists().CustomsCodes_List(ZString.Empty);
			CAGCodeIssuer.UpdateListWithCAGCodeIfNeeded(list, null);
			AssertEquals($"CusCode list of null country should contain CAG", true, list.ContainsCode(OrgCusCode.CodeTypes.CommercialAndGovernmentEntity));
		}

		public void TestCAGCodeIssuer_AssertCAGCodeIsNotValid_WhenCusCodesCAGWithIncorrectFirstChar()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithIncorrectFirstChar = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCQ", countryCode: Constants.CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCH", countryCode: Constants.CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCN", countryCode: Constants.CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCU", countryCode: Constants.CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCK", countryCode: Constants.CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCV", countryCode: Constants.CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCU", countryCode: Constants.CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "OABC#", countryCode: Constants.CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCA", countryCode: Constants.CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCZ", countryCode: Constants.CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCB", countryCode: Constants.CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCG", countryCode: Constants.CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD", countryCode: Constants.CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCJ", countryCode: Constants.CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCS", countryCode: Constants.CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCG", countryCode: Constants.CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCR", countryCode: Constants.CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCV", countryCode: Constants.CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCY", countryCode: Constants.CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCZ", countryCode: Constants.CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCA", countryCode: Constants.CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCX", countryCode: Constants.CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCF", countryCode: Constants.CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCK", countryCode: Constants.CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD", countryCode: Constants.CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCR", countryCode: Constants.CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCC", countryCode: Constants.CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCW", countryCode: Constants.CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCM", countryCode: Constants.CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCE", countryCode: Constants.CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCT", countryCode: Constants.CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCP", countryCode: Constants.CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCY", countryCode: Constants.CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCP", countryCode: Constants.CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCH", countryCode: Constants.CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCL", countryCode: Constants.CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCF", countryCode: Constants.CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCE", countryCode: Constants.CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCS", countryCode: Constants.CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCM", countryCode: Constants.CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCQ", countryCode: Constants.CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCB", countryCode: Constants.CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCN", countryCode: Constants.CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCC", countryCode: Constants.CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCT", countryCode: Constants.CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCJ", countryCode: Constants.CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCW", countryCode: Constants.CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: Constants.CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABC#", countryCode: "")
			};

			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithIncorrectFirstChar)
				{
					var isCodeValid = CAGCodeIssuer.GetIsCAGCodeValid(cusCode);
					AssertEquals($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be invalid when CAG has incorrect first char", false, isCodeValid);
				}
			});
		}

		public void TestCAGCodeIssuer_AssertCAGCodeIsNotValid_WhenCusCodesCAGHasIncorrectLastChar()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithIncorrectLastChar = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "WABCI", countryCode: Constants.CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "ZABCI", countryCode: Constants.CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "BABCI", countryCode: Constants.CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "RABCI", countryCode: Constants.CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "FABCI", countryCode: Constants.CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "CABCI", countryCode: Constants.CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "GABCI", countryCode: Constants.CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "SABCI", countryCode: Constants.CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "JABCI", countryCode: Constants.CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "BABCI", countryCode: Constants.CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "YABCI", countryCode: Constants.CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "HABCI", countryCode: Constants.CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "EABCI", countryCode: Constants.CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "NABCI", countryCode: Constants.CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "PABCI", countryCode: Constants.CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "QABCI", countryCode: Constants.CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "VABCI", countryCode: Constants.CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "TABCI", countryCode: Constants.CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCI", countryCode: Constants.CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "UABCI", countryCode: Constants.CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: Constants.CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCI", countryCode: "")
			};
			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithIncorrectLastChar)
				{
					var isCodeValid = CAGCodeIssuer.GetIsCAGCodeValid(cusCode);
					AssertEquals($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be invalid when CAG has incorrect last char", false, isCodeValid);
				}
			});
		}

		public void TestCAGCodeType_AssertCAGCodeIsNotValid_WhenCustomsRegNoHasLessThan5Chars()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithLessThan5Chars = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "AABQ", countryCode: Constants.CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABH", countryCode: Constants.CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "WAB#", countryCode: Constants.CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "ZAB#", countryCode: Constants.CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABN", countryCode: Constants.CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "BAB#", countryCode: Constants.CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABU", countryCode: Constants.CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABK", countryCode: Constants.CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABV", countryCode: Constants.CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABU", countryCode: Constants.CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "#AB#", countryCode: Constants.CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABA", countryCode: Constants.CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABZ", countryCode: Constants.CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABB", countryCode: Constants.CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABG", countryCode: Constants.CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "RAB#", countryCode: Constants.CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABD", countryCode: Constants.CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABJ", countryCode: Constants.CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABS", countryCode: Constants.CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABG", countryCode: Constants.CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "FAB#", countryCode: Constants.CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABR", countryCode: Constants.CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "CAB#", countryCode: Constants.CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "GAB#", countryCode: Constants.CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABV", countryCode: Constants.CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "SAB#", countryCode: Constants.CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABY", countryCode: Constants.CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABZ", countryCode: Constants.CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABA", countryCode: Constants.CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "AAB#", countryCode: Constants.CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "JAB#", countryCode: Constants.CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABX", countryCode: Constants.CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABF", countryCode: Constants.CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABK", countryCode: Constants.CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABD", countryCode: Constants.CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABR", countryCode: Constants.CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "BAB#", countryCode: Constants.CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABC", countryCode: Constants.CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "YAB#", countryCode: Constants.CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABW", countryCode: Constants.CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABM", countryCode: Constants.CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "HAB#", countryCode: Constants.CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "EAB#", countryCode: Constants.CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "NAB#", countryCode: Constants.CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABE", countryCode: Constants.CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABT", countryCode: Constants.CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABP", countryCode: Constants.CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABY", countryCode: Constants.CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABP", countryCode: Constants.CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABH", countryCode: Constants.CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "PAB#", countryCode: Constants.CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABL", countryCode: Constants.CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABF", countryCode: Constants.CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABE", countryCode: Constants.CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABS", countryCode: Constants.CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "QAB#", countryCode: Constants.CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABM", countryCode: Constants.CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABQ", countryCode: Constants.CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "VAB#", countryCode: Constants.CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABB", countryCode: Constants.CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABN", countryCode: Constants.CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABC", countryCode: Constants.CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABT", countryCode: Constants.CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "TAB#", countryCode: Constants.CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABJ", countryCode: Constants.CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABW", countryCode: Constants.CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "UAB#", countryCode: Constants.CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "#AB#", countryCode: Constants.CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "IAB#", countryCode: ""),
			};
			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithLessThan5Chars)
				{
					var isCodeValid = CAGCodeIssuer.GetIsCAGCodeValid(cusCode);
					AssertEquals($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be invalid when CAG has less than 5 chars", false, isCodeValid);
				}
			});
		}

		public void TestCAGCodeIssuer_AssertCAGCodeIsNotValid_WhenCustomsRegNoHasMoreThan5Chars()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithIncorrectFirstChar = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDQ",countryCode: Constants.CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDH",countryCode: Constants.CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDN",countryCode: Constants.CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDU",countryCode: Constants.CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDK",countryCode: Constants.CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDV",countryCode: Constants.CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDU",countryCode: Constants.CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDA",countryCode: Constants.CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDZ",countryCode: Constants.CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDB",countryCode: Constants.CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDG",countryCode: Constants.CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDD",countryCode: Constants.CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDJ",countryCode: Constants.CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDS",countryCode: Constants.CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDG",countryCode: Constants.CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDR",countryCode: Constants.CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDV",countryCode: Constants.CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDY",countryCode: Constants.CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDZ",countryCode: Constants.CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDA",countryCode: Constants.CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDX",countryCode: Constants.CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDF",countryCode: Constants.CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDK",countryCode: Constants.CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDD",countryCode: Constants.CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDR",countryCode: Constants.CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDC",countryCode: Constants.CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDW",countryCode: Constants.CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDM",countryCode: Constants.CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDE",countryCode: Constants.CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDT",countryCode: Constants.CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDP",countryCode: Constants.CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDY",countryCode: Constants.CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDP",countryCode: Constants.CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDH",countryCode: Constants.CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDL",countryCode: Constants.CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDF",countryCode: Constants.CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDE",countryCode: Constants.CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDS",countryCode: Constants.CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDM",countryCode: Constants.CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDQ",countryCode: Constants.CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDB",countryCode: Constants.CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDN",countryCode: Constants.CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDC",countryCode: Constants.CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDT",countryCode: Constants.CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDJ",countryCode: Constants.CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCDW",countryCode: Constants.CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "LABCD#",countryCode: Constants.CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "IABCD#", countryCode: "")
			};
			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithIncorrectFirstChar)
				{
					var isCodeValid = CAGCodeIssuer.GetIsCAGCodeValid(cusCode);
					AssertEquals($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be invalid when CAG has more than 5 chars", false, isCodeValid);
				}
			});
		}

		public void TestCAGCodeType_AssertCAGCodeIsValid_WhenCustomsRegNoIsCorrectlyFormattedForIssuingCountry()
		{
			var codeTypeCAG = OrgCusCode.CodeTypes.CommercialAndGovernmentEntity;
			var cusCodesCAGWithIncorrectFirstChar = new List<OrgCusCode>()
			{
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCQ", countryCode: Constants.CountryCodes.Afghanistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCH", countryCode: Constants.CountryCodes.Albania),
				AddNewCusCode(codeType: codeTypeCAG, code: "WABC#", countryCode: Constants.CountryCodes.Argentina),
				AddNewCusCode(codeType: codeTypeCAG, code: "ZABC#", countryCode: Constants.CountryCodes.Australia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCN", countryCode: Constants.CountryCodes.Austria),
				AddNewCusCode(codeType: codeTypeCAG, code: "BABC#", countryCode: Constants.CountryCodes.Belgium),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCU", countryCode: Constants.CountryCodes.BosniaAndHerzegovina),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCK", countryCode: Constants.CountryCodes.Brazil),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCV", countryCode: Constants.CountryCodes.Brunei),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCU", countryCode: Constants.CountryCodes.Bulgaria),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABC#", countryCode: Constants.CountryCodes.Canada),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCA", countryCode: Constants.CountryCodes.Chile),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCZ", countryCode: Constants.CountryCodes.Colombia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCB", countryCode: Constants.CountryCodes.Croatia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCG", countryCode: Constants.CountryCodes.CzechRepublic),
				AddNewCusCode(codeType: codeTypeCAG, code: "RABC#", countryCode: Constants.CountryCodes.Denmark),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCD", countryCode: Constants.CountryCodes.Egypt),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCJ", countryCode: Constants.CountryCodes.Estonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCS", countryCode: Constants.CountryCodes.Fiji),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCG", countryCode: Constants.CountryCodes.Finland),
				AddNewCusCode(codeType: codeTypeCAG, code: "FABC#", countryCode: Constants.CountryCodes.France),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCR", countryCode: Constants.CountryCodes.Georgia),
				AddNewCusCode(codeType: codeTypeCAG, code: "CABC#", countryCode: Constants.CountryCodes.Germany),
				AddNewCusCode(codeType: codeTypeCAG, code: "GABC#", countryCode: Constants.CountryCodes.Greece),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCV", countryCode: Constants.CountryCodes.Hungary),
				AddNewCusCode(codeType: codeTypeCAG, code: "SABC#", countryCode: Constants.CountryCodes.Iceland),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCY", countryCode: Constants.CountryCodes.India),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCZ", countryCode: Constants.CountryCodes.Indonesia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCA", countryCode: Constants.CountryCodes.Israel),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABC#", countryCode: Constants.CountryCodes.Italy),
				AddNewCusCode(codeType: codeTypeCAG, code: "JABC#", countryCode: Constants.CountryCodes.Japan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCX", countryCode: Constants.CountryCodes.Jordan),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCF", countryCode: Constants.CountryCodes.KoreaSouth),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCK", countryCode: Constants.CountryCodes.Kuwait),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCD", countryCode: Constants.CountryCodes.Latvia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCR", countryCode: Constants.CountryCodes.Lithuania),
				AddNewCusCode(codeType: codeTypeCAG, code: "BABC#", countryCode: Constants.CountryCodes.Luxembourg),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCC", countryCode: Constants.CountryCodes.Macedonia),
				AddNewCusCode(codeType: codeTypeCAG, code: "YABC#", countryCode: Constants.CountryCodes.Malaysia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCW", countryCode: Constants.CountryCodes.Montenegro),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCM", countryCode: Constants.CountryCodes.Morocco),
				AddNewCusCode(codeType: codeTypeCAG, code: "HABC#", countryCode: Constants.CountryCodes.Netherlands),
				AddNewCusCode(codeType: codeTypeCAG, code: "EABC#", countryCode: Constants.CountryCodes.NewZealand),
				AddNewCusCode(codeType: codeTypeCAG, code: "NABC#", countryCode: Constants.CountryCodes.Norway),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCE", countryCode: Constants.CountryCodes.Oman),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCT", countryCode: Constants.CountryCodes.Pakistan),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCP", countryCode: Constants.CountryCodes.PapuaNewGuinea),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCY", countryCode: Constants.CountryCodes.Peru),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCP", countryCode: Constants.CountryCodes.Philippines),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCH", countryCode: Constants.CountryCodes.Poland),
				AddNewCusCode(codeType: codeTypeCAG, code: "PABC#", countryCode: Constants.CountryCodes.Portugal),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCL", countryCode: Constants.CountryCodes.Romania),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCF", countryCode: Constants.CountryCodes.Russia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCE", countryCode: Constants.CountryCodes.SaudiArabia),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCS", countryCode: Constants.CountryCodes.Serbia),
				AddNewCusCode(codeType: codeTypeCAG, code: "QABC#", countryCode: Constants.CountryCodes.Singapore),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCM", countryCode: Constants.CountryCodes.Slovakia),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCQ", countryCode: Constants.CountryCodes.Slovenia),
				AddNewCusCode(codeType: codeTypeCAG, code: "VABC#", countryCode: Constants.CountryCodes.SouthAfrica),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCB", countryCode: Constants.CountryCodes.Spain),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCN", countryCode: Constants.CountryCodes.Sweden),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCC", countryCode: Constants.CountryCodes.Thailand),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCT", countryCode: Constants.CountryCodes.Tonga),
				AddNewCusCode(codeType: codeTypeCAG, code: "TABC#", countryCode: Constants.CountryCodes.Turkey),
				AddNewCusCode(codeType: codeTypeCAG, code: "AABCJ", countryCode: Constants.CountryCodes.Ukraine),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABCW", countryCode: Constants.CountryCodes.UnitedArabEmirates),
				AddNewCusCode(codeType: codeTypeCAG, code: "UABC#", countryCode: Constants.CountryCodes.UnitedKingdom),
				AddNewCusCode(codeType: codeTypeCAG, code: "#ABC#", countryCode: Constants.CountryCodes.UnitedStates),
				AddNewCusCode(codeType: codeTypeCAG, code: "IABC#",  countryCode: "")
			};
			CombineAssertions(() =>
			{
				foreach (var cusCode in cusCodesCAGWithIncorrectFirstChar)
				{
					var isCodeValid = CAGCodeIssuer.GetIsCAGCodeValid(cusCode);
					AssertEquals($"CusCode {cusCode.OK_CustomsRegNo} for country {cusCode.OK_RN_NKCodeCountry} should be valid when CAG is correctly formatted", true, isCodeValid);
				}
			});
		}

		public void TestCAGCodeType_AssertCAGCodeIsNotValid_WhenCountryIsNotCAGCodeIssuer()
		{
			var refCountry = Factory.New<RefCountry>();
			refCountry.RN_Code = Constants.CountryCodes.China;
			var list = new OrgCodeLists().CustomsCodes_List(Constants.CountryCodes.China);
			CAGCodeIssuer.UpdateListWithCAGCodeIfNeeded(list, refCountry);
			AssertEquals("Precondition: CusCode list of China should not contain CAG", false, list.ContainsCode(OrgCusCode.CodeTypes.CommercialAndGovernmentEntity));

			var cusCode = AddNewCusCode(Constants.CountryCodes.China, "IABC#", OrgCusCode.CodeTypes.CommercialAndGovernmentEntity);
			var isCodeValid = true;

			AssertNoExceptionThrown("No exception throw when country code not exist in dictionary CAGValidationRulesByCountry", () =>
			{
				isCodeValid = CAGCodeIssuer.GetIsCAGCodeValid(cusCode);
			});
			AssertEquals(false, isCodeValid);
		}

		#region implementation

		OrgHeader Org;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.NewWithValidTestData<OrgHeader>();
		}

		OrgCusCode AddNewCusCode(string countryCode, string code, string codeType)
		{
			var newCAGCode = Org.CustomsCodes.AddNew();
			newCAGCode.OK_CodeType = codeType;
			newCAGCode.OK_RN_NKCodeCountry = countryCode.IsNullOrEmpty() ? string.Empty : countryCode;
			newCAGCode.OK_CustomsRegNo = code;
			return newCAGCode;
		}

		#endregion
	}
}
