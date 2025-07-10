using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class CONITCodeValidator
	{
		string CodeType => ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;

		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CONIT;

		HashSet<string> ValidPatternStrings => new HashSet<string>
		{
			"^[0-9]{1,3}\\.[0-9]{3}\\.[0-9]{3}-[0-9]{1}$",
			"^[0-9]{1}\\.[0-9]{3}\\.[0-9]{3}\\.[0-9]{3}-[0-9]{1}$",
			"^[0-9]{7,10}-[0-9]{1}$", "^[0-9]{8,11}$"
		}; // Valid pattern of NIT for Colombia

		HashSet<int> ValidLengths => new HashSet<int> { 8, 9, 10, 11, 12, 13, 15 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage)
				&& OrgCusCodeValidator.Validate(codeInfo, code => IsValidCheckDigit(code), OrganisationRegistryCodeType, InvalidCheckDigitMessage);
		}

		protected bool IsValidCheckDigit(ZString code)
		{
			ZString originalCode = code.Replace("-", string.Empty).Replace(".", string.Empty);
			ZString number;

			number = originalCode.Substring(0, originalCode.Length - 1).PadLeft(10, '0').Substring(0, 10);

			var factors = new int[] { 43, 41, 37, 29, 23, 19, 17, 13, 7, 3 };
			var accumulated = 0;

			for (int i = 0; i < factors.Length; i++)
			{
				accumulated += int.Parse(number[i].ToString(), CultureInfo.InvariantCulture) * factors[i];
			}

			int total = accumulated - (11 * (accumulated / 11));

			if (total != 0 && total != 1)
			{
				total = 11 - total;
			}

			return int.Parse(originalCode[originalCode.Length - 1].ToString(), CultureInfo.InvariantCulture) == total;
		}

		string InvalidLengthMessage => Res.GetString("2A91BB7C-5ABF-40BC-A2D1-867F33FD3D52", @"The {0} registration code length is invalid.

NIT codes must be 8, 9, 10, 11, 12, 13 or 15 digits long", CodeType);

		string InvalidPatternMessage => Res.GetString("331DFBF8-BD23-4962-97E1-DD4BC7DF2C8E", @"The {0} registration code pattern is invalid.

Valid patterns are:
n.nnn.nnn-n OR nn.nnn.nnn-n OR nnn.nnn.nnn-n OR n.nnn.nnn.nnn-n OR
nnnnnnn-n OR nnnnnnnn-n OR nnnnnnnnn-n OR nnnnnnnnnn-n OR
nnnnnnnn OR nnnnnnnnn OR nnnnnnnnnn OR nnnnnnnnnnn

where 'n' is a digit from 0 to 9.", CodeType);

		string InvalidCheckDigitMessage => Res.GetString("B58304E2-0DBC-4776-8F8C-F99C1E41D62A", @"The check digit in the {0} registration code is incorrect.", CodeType);
	}
}
