using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public static class NctsCompanyValidationHelper
	{
		public const int CompanyNameMaxLength = 35;
		public const int CompanyAddressMaxLength = 35;

		public static string[] CheckAddressLength(JobDocAddress jobDocAddress)
		{
			var warningMessage = System.Array.Empty<string>();
			if (jobDocAddress == null)
			{
				return warningMessage;
			}

			var orgHeader = jobDocAddress.Address?.Header;
			var orgAddress = jobDocAddress.Address;
			if (orgHeader != null && orgAddress != null)
			{
				warningMessage = CheckOrganizationLength(orgHeader, orgAddress);
			}

			return warningMessage;
		}

		public static string[] CheckOrganizationLength(OrgHeader orgHeader, OrgAddress orgAddress)
		{
			var warningMessage = new List<string>();
			ZInt length;
			length = orgHeader.OH_FullName.Length;
			if (length > CompanyNameMaxLength)
			{
				warningMessage.Add(Res.GetString("7301023A-BEA0-411D-BEA1-DE45023DAC3F", "Company Name is {0} characters. Only the first {1} characters will be sent in the message", length, CompanyNameMaxLength));
			}

			length = string.Join(" ", orgAddress.Address1.TrimEnd(), orgAddress.Address2.TrimEnd()).Length;
			if (length > CompanyAddressMaxLength)
			{
				warningMessage.Add(Res.GetString("38D78C2B-8DAB-4370-9050-F66E681C869D", "Company Address is {0} characters. Only the first {1} characters will be sent in the message", length, CompanyAddressMaxLength));
			}

			return warningMessage.ToArray();
		}
	}
}

