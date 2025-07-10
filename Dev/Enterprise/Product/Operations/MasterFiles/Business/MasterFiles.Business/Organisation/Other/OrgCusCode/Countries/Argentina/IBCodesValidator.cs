using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	class IBCodesValidator
	{
		readonly ZString OrganisationRegistryCodeType;

		public IBCodesValidator(ZString organisationRegistryCodeType)
		{
			OrganisationRegistryCodeType = organisationRegistryCodeType;
		}

		HashSet<string> ValidPatternStrings => new HashSet<string> { @"^[0-9]{10}$", @"^[0-9]{8}-[0-9]{2}$", @"^[0-9]{11}$", @"^[0-9]{3}-[0-9]{6}-[0-9]{1}$", @"^[0-9]{2}-[0-9]{8}-[0-9]{1}$" };

		HashSet<int> ValidLengths => new HashSet<int> { 10, 11, 12, 13 };

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodeLengthValidator.Validate(codeInfo, ValidLengths, OrganisationRegistryCodeType, InvalidLengthMessage)
				&& OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings, OrganisationRegistryCodeType, InvalidPatternMessage);
		}

		static string InvalidLengthMessage
		{
			get { return Res.GetString("5ABF52C2-6A7D-4789-9B74-76B58602BBA6", @"The IB registration code length is invalid.
IB codes must be 10 to 13 digits long."); }
		}

		static string InvalidPatternMessage
		{
			get
			{
				return Res.GetString(@"70842FE5-CDD7-42E6-9284-956EB05BB272", @"The IB registration code pattern is invalid.

Valid patterns are:
	nnnnnnnnnn
	nnnnnnnnnnn
	nnnnnnnn-nn
	nnn-nnnnnn-n
	nn-nnnnnnnn-n

with 'n' a digit from 0 to 9.
Please verify that you are entering a correct number.");
			}
		}
	}
}
