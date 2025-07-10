using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public sealed class CAGCodeIssuer
	{
		CAGCodeIssuer()
		{
		}

		public static CodeDescriptionPairList UpdateListWithCAGCodeIfNeeded(CodeDescriptionPairList list, RefCountry country)
		{
			if (country == null || GetIsCountryCAGCodeIssuer(country))
			{
				list.AddPair(OrgCusCode.CodeTypes.CommercialAndGovernmentEntity, Res.GetString("(OrgCusCode.CodeTypes.CommercialAndGovernmentEntity", "Commercial and Government Entity Code"));
			}

			return list;
		}

		public static void ValidateCAGCode(OrgCusCode orgCusCode)
		{
			if (!orgCusCode.OK_CodeTypeInfo.HasErrors() && !orgCusCode.OK_RN_NKCodeCountryInfo.HasErrors())
			{
				if (!GetIsCAGCodeValid(orgCusCode))
				{
					orgCusCode.OK_CustomsRegNoInfo.AddError(Res.GetString("BD054206-A70B-42B1-8987-8D76E06B20A5", "Commercial And Government Entity Code is not valid."));
				}
			}
		}

		public static bool GetIsCAGCodeValid(OrgCusCode orgCusCode)
		{
			var isCAGCodeValid = false;

			if (CAGValidationRulesByCountry.ContainsKey(orgCusCode.OK_RN_NKCodeCountry) && orgCusCode.OK_CustomsRegNo.Length == 5)
			{
				var validationRule = CAGValidationRulesByCountry[orgCusCode.OK_RN_NKCodeCountry];
				var isCodeContainsAllowedFirstChar = validationRule.Item1.Contains(orgCusCode.OK_CustomsRegNo.Substring(0, 1));
				var isCodeContainsAllowedLastChar = validationRule.Item2.Contains(orgCusCode.OK_CustomsRegNo.Substring(orgCusCode.OK_CustomsRegNo.Length - 1));

				isCAGCodeValid = isCodeContainsAllowedFirstChar && isCodeContainsAllowedLastChar;
			}

			return isCAGCodeValid;
		}

		static bool GetIsCountryCAGCodeIssuer(RefCountry country)
		{
			return CAGValidationRulesByCountry.ContainsKey(country.RN_Code);
		}

		[SuppressMessage("Microsoft.Design", "CA1006")]
		static Dictionary<string, Tuple<List<string>, List<string>>> CAGValidationRulesByCountry
		{
			get
			{
				return new Dictionary<string, Tuple<List<string>, List<string>>>
				{
					// implemnted from https://en.wikipedia.org/wiki/Commercial_and_Government_Entity_code
					[string.Empty] = new Tuple<List<string>, List<string>>(new List<string>() { "I", "S", "X" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Germany] = new Tuple<List<string>, List<string>>(new List<string>() { "C", "D" }, new List<string>() { "#" }),
					[Constants.CountryCodes.UnitedKingdom] = new Tuple<List<string>, List<string>>(new List<string>() { "U", "K" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Canada] = new Tuple<List<string>, List<string>>(new List<string>() { "#", "L" }, new List<string>() { "#" }),
					[Constants.CountryCodes.France] = new Tuple<List<string>, List<string>>(new List<string>() { "F", "M" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Austria] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "N" }),
					[Constants.CountryCodes.Brazil] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "K" }),
					[Constants.CountryCodes.Bulgaria] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "U" }),
					[Constants.CountryCodes.CzechRepublic] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "G" }),
					[Constants.CountryCodes.Egypt] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "D" }),
					[Constants.CountryCodes.Estonia] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "J" }),
					[Constants.CountryCodes.Fiji] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "S" }),
					[Constants.CountryCodes.Hungary] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "V" }),
					[Constants.CountryCodes.India] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "Y" }),
					[Constants.CountryCodes.Indonesia] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "Z" }),
					[Constants.CountryCodes.Israel] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "A" }),
					[Constants.CountryCodes.KoreaSouth] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "F" }),
					[Constants.CountryCodes.Lithuania] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "R" }),
					[Constants.CountryCodes.Philippines] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "P" }),
					[Constants.CountryCodes.Poland] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "H" }),
					[Constants.CountryCodes.Romania] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "L" }),
					[Constants.CountryCodes.SaudiArabia] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "E" }),
					[Constants.CountryCodes.Slovakia] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "M" }),
					[Constants.CountryCodes.Slovenia] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "Q" }),
					[Constants.CountryCodes.Spain] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "B" }),
					[Constants.CountryCodes.Thailand] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "C" }),
					[Constants.CountryCodes.Tonga] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "T" }),
					[Constants.CountryCodes.UnitedArabEmirates] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "W" }),
					[Constants.CountryCodes.UnitedStates] = new Tuple<List<string>, List<string>>(new List<string>() { "#" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Afghanistan] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "Q" }),
					[Constants.CountryCodes.Albania] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "H" }),
					[Constants.CountryCodes.BosniaAndHerzegovina] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "U" }),
					[Constants.CountryCodes.Brunei] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "V" }),
					[Constants.CountryCodes.Chile] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "A" }),
					[Constants.CountryCodes.Colombia] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "Z" }),
					[Constants.CountryCodes.Croatia] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "B" }),
					[Constants.CountryCodes.Finland] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "G" }),
					[Constants.CountryCodes.Georgia] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "R" }),
					[Constants.CountryCodes.Italy] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Jordan] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "X" }),
					[Constants.CountryCodes.Kuwait] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "K" }),
					[Constants.CountryCodes.Latvia] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "D" }),
					[Constants.CountryCodes.Macedonia] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "C" }),
					[Constants.CountryCodes.Montenegro] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "W" }),
					[Constants.CountryCodes.Morocco] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "M" }),
					[Constants.CountryCodes.Oman] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "E" }),
					[Constants.CountryCodes.Pakistan] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "T" }),
					[Constants.CountryCodes.PapuaNewGuinea] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "P" }),
					[Constants.CountryCodes.Peru] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "Y" }),
					[Constants.CountryCodes.Russia] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "F" }),
					[Constants.CountryCodes.Serbia] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "S" }),
					[Constants.CountryCodes.Sweden] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "N" }),
					[Constants.CountryCodes.Ukraine] = new Tuple<List<string>, List<string>>(new List<string>() { "A" }, new List<string>() { "J" }),
					[Constants.CountryCodes.Belgium] = new Tuple<List<string>, List<string>>(new List<string>() { "B" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Luxembourg] = new Tuple<List<string>, List<string>>(new List<string>() { "B" }, new List<string>() { "#" }),
					[Constants.CountryCodes.NewZealand] = new Tuple<List<string>, List<string>>(new List<string>() { "E" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Greece] = new Tuple<List<string>, List<string>>(new List<string>() { "G" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Netherlands] = new Tuple<List<string>, List<string>>(new List<string>() { "H" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Japan] = new Tuple<List<string>, List<string>>(new List<string>() { "J" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Norway] = new Tuple<List<string>, List<string>>(new List<string>() { "N" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Portugal] = new Tuple<List<string>, List<string>>(new List<string>() { "P" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Singapore] = new Tuple<List<string>, List<string>>(new List<string>() { "Q" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Denmark] = new Tuple<List<string>, List<string>>(new List<string>() { "R" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Iceland] = new Tuple<List<string>, List<string>>(new List<string>() { "S" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Turkey] = new Tuple<List<string>, List<string>>(new List<string>() { "T" }, new List<string>() { "#" }),
					[Constants.CountryCodes.SouthAfrica] = new Tuple<List<string>, List<string>>(new List<string>() { "V" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Argentina] = new Tuple<List<string>, List<string>>(new List<string>() { "W" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Malaysia] = new Tuple<List<string>, List<string>>(new List<string>() { "Y" }, new List<string>() { "#" }),
					[Constants.CountryCodes.Australia] = new Tuple<List<string>, List<string>>(new List<string>() { "Z" }, new List<string>() { "#" }),
				};
			}
		}
	}
}
