using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class KoreaSouthRegistrationNumberValidator : ValidationProvider
	{
		public static void ValidateVATNumber(ZPropertyInfo propertyInfo)
		{
			var input = (ZString)propertyInfo.Value;
			if (!Regex.IsMatch(input, "^[0-9]{10}$"))
			{
				propertyInfo.AddErrorIfEnforced(Res.GetString("31328DEB-3A08-422A-842A-3E1846C2C88B", "The KR VAT Registration number '{0}' is invalid.\r\nIt should be in format 'NNNNNNNNNN'.", new string[] { input }), OrganisationRegistry.RegistrationNumberFormatFields.KRVAT);
			}
		}

		public static void ValidateOfficeID(ZPropertyInfo propertyInfo)
		{
			var input = (ZString)propertyInfo.Value;
			if (!Regex.IsMatch(input, "^[0-9]{4}$"))
			{
				propertyInfo.AddErrorIfEnforced(Res.GetString("BD6841DF-0C33-4B0D-BC19-5990965A6C7E", "The KR 08 - Office Registration number '{0}' is invalid.\r\nIt should be in format 'NNNN'.", new string[] { input }), OrganisationRegistry.RegistrationNumberFormatFields.KR08);
			}
		}

		public static void ValidateKoreanRegNoForResident(ZPropertyInfo propertyInfo)
		{
			var input = (ZString)propertyInfo.Value;
			if (!Regex.IsMatch(input, "^[0-9]{13}$"))
			{
				propertyInfo.AddErrorIfEnforced(Res.GetString("050DC7F8-2AC1-4D58-AC42-7EBABB8F5C17", "The KR 01 - Citizen Registration number '{0}' is invalid.\r\nIt should be in format 'NNNNNNNNNNNNN'.", new string[] { input }), OrganisationRegistry.RegistrationNumberFormatFields.KR01);
			}
		}

		public static void ValidateKBT(ZPropertyInfo propertyInfo)
		{
			var input = (ZString)propertyInfo.Value;
			if (input.Length > 100)
			{
				propertyInfo.AddErrorIfEnforced(Res.GetString("36DD4E55-3ACF-47B2-B2B5-4C12DD7BA664", "The KR KBT Registration number '{0}' is invalid.\r\nIt must not exceed 100 characters.", new string[] { input }), OrganisationRegistry.RegistrationNumberFormatFields.KRKBT);
			}
		}

		public static void ValidateKBC(ZPropertyInfo propertyInfo)
		{
			var input = (ZString)propertyInfo.Value;
			if (input.Length > 100)
			{
				propertyInfo.AddErrorIfEnforced(Res.GetString("D6C37702-64D4-4362-90F9-D6A0F6B26EDC", "The KR KBC Registration number '{0}' is invalid.\r\nIt must not exceed 100 characters.", new string[] { input }), OrganisationRegistry.RegistrationNumberFormatFields.KRKBC);
			}
		}

		public static void ValidateAEO(ZPropertyInfo propertyInfo)
		{
			if (!Regex.IsMatch((ZString)propertyInfo.Value, "^[0-9a-z]{7}$", RegexOptions.IgnoreCase))
			{
				propertyInfo.AddMessageError(Res.GetString("e3f6b259-9f07-469f-b455-c6f67e6283cf", "KR AEO number should consist of 7 alphanumeric characters."));
			}
		}
	}
}
