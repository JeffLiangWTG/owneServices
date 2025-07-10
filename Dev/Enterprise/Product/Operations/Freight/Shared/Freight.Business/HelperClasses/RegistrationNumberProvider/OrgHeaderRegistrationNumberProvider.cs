using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public sealed class OrgHeaderRegistrationNumberProvider : IRegistrationNumberProvider
	{
		public OrgHeaderRegistrationNumberProvider(OrgHeader orgHeader)
		{
			org = orgHeader;
		}

		readonly OrgHeader org;

		ZString IRegistrationNumberProvider.GetTaxNumber(ZString countryCode, ZString typeCode)
		{
			if (org != null)
			{
				return org.CustomsCodes
					.Cast<OrgCusCode>()
					.Where(code => code.OK_CodeType == typeCode && code.OK_RN_NKCodeCountry == countryCode)
					.Select(code => code.OK_CustomsRegNo)
					.FirstOrDefault();
			}

			return ZString.Empty;
		}

		IReadOnlyCollection<(ZString taxNumber, ZString countryOfIssue)> IRegistrationNumberProvider.GetTaxNumbers(ZString typeCode)
		{
			if (org?.CustomsCodes == null || typeCode.IsEmpty)
			{
				return Array.Empty<(ZString taxNumber, ZString countryOfIssue)>();
			}

			return org.CustomsCodes
				.Cast<OrgCusCode>()
				.Where(code => code.OK_CodeType == typeCode)
				.Select(code => (code.OK_CustomsRegNo, code.OK_RN_NKCodeCountry))
				.ToArray();
		}
	}
}
