using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class MalaysiaRegistrationNumberValidator : ValidationProvider
	{
		public static void ValidateOtherBusinessCode(ZPropertyInfo propertyInfo)
		{
			var validOtherBusinessCodes =
				new[]
					{
						"C", "D", "E", "F", "G", "H", "I", "J",
						"K", "L", "M", "N", "P", "R", "Z"
					};
			var validOtherBusinessCodesList = new List<string>(validOtherBusinessCodes);

			if (!validOtherBusinessCodesList.Contains(((ZString)propertyInfo.Value).SubstringSafe(0, 1)))
			{
				propertyInfo.AddErrorIfEnforced(Res.GetString("255f130d-74f1-4d99-90e1-45f84bb7ef02", @"The first character must be a valid 'other' code. Valid codes include:

  C   = REGISTRAR OF SOCIETIES
  D   = REGISTRAR OF COOPERATIVE
  E   = NATIONAL REGISTRATION DEPARTMENT
  F   = DEPARTMENT OF JUSTICE
  G   = STATUTORY
  H   = STATE ECONOMY DEVELOPMENT CORPOR
  I   = INDIVIDUAL
  J   = REGISTRAR OF PROF. BODIES
  K   = FOREIGN NATIONAL (PASSPORT)
  L   = POLICE
  M   = ARMED FORCES
  N   = DIPLOMATIC / AMBASSIES
  P   = GOVERNMENT DEPARTMENT
  R   = RULERS
  Z   = OTHERS"), OrganisationRegistry.RegistrationNumberFormatFields.MYOTH);
			}
		}

		public static void ValidatePersonalIdentificationCardNumber(ZPropertyInfo propertyInfo)
		{
			var input = (ZString)propertyInfo.Value;
			if (!Regex.IsMatch(input, "^[0-9]{12}$"))
			{
				propertyInfo.AddErrorIfEnforced(Res.GetString("EBE73322-59A7-4C40-AE69-24DF1DDA6373", "The entered Personal Identification Card Number '{0}' is invalid. It should be in format NNNNNNNNNNNN with 12 numeric digits", new string[] { input }), OrganisationRegistry.RegistrationNumberFormatFields.MYPIC);
			}
		}

		public static void ValidateTIN(ZPropertyInfo propertyInfo)
		{
			var input = (ZString)propertyInfo.Value;
			var ignoreValidationList = new List<string> { "EI00000000010", "EI00000000020", "EI00000000030" };

			if (!ignoreValidationList.Contains((ZString)propertyInfo.Value) && !Regex.IsMatch(input, "(^[CDEFJ]|CS|FA|PT|TA|TC|TN|TR|TP|LE)[0-9]{10,11}$|^IG[0-9]{9,11}$|^[0-9]{12}$"))
			{
				propertyInfo.AddErrorIfEnforced(Res.GetString("F23072F7-5422-4283-8959-20394ED39227", "The MY TIN number '{0}' is invalid. It should be C/CS/D/E/F/FA/PT/TA/TC/TN/TR/TP/J/LE + 10/11 digits, or IG + 9/10/11 digits, or it should be in format 'NNNNNNNNNNNN'.", new string[] { input }), OrganisationRegistry.RegistrationNumberFormatFields.MYTIN);
			}
		}
	}
}
