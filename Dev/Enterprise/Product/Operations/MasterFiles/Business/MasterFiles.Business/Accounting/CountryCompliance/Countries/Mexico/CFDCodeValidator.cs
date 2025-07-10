using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Accounting;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class CFDCodeValidator
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.MXCFD;
		string[] ValidCodesStrings(ZPropertyInfo codeInfo) => codeInfo.BizObj.Factory.GetCachedValue("Mexico_UsosCFDI_PairList", () => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetMexicoEInvoicingExtension().GetUsosCFDI().GetAllCodes());
		HashSet<string> ValidPatternStrings(ZPropertyInfo codeInfo)
		{
			var usosCFDI = ValidCodesStrings(codeInfo);

			return new HashSet<string>(usosCFDI.Select(x => $"^{x}$"));
		}

		internal bool Validate(ZPropertyInfo codeInfo)
		{
			return OrgCusCodePatternValidator.Validate(codeInfo, ValidPatternStrings(codeInfo), OrganisationRegistryCodeType, InvalidCodeMessage(codeInfo));
		}

		string InvalidCodeMessage(ZPropertyInfo codeInfo) => Res.GetString(@"195cd11f-313f-450e-aaa5-bfc0ca7d3e78", @"The 'MX CFD' registration code is invalid.

Valid codes are:
{0}

Please verify that you are entering a correct registration code.", string.Join(", ", ValidCodesStrings(codeInfo)));
	}
}
