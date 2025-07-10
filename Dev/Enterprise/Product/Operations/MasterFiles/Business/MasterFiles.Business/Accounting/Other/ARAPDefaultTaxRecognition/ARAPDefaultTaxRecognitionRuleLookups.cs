using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	public class ARAPDefaultTaxRecognitionRuleLookups : ZLookups
	{
		public ARAPDefaultTaxRecognitionRuleLookups(ARAPDefaultTaxRecognitionRule parent)
			: base(parent)
		{ }

		public CodeDescriptionPairList TaxRecognitionCodeList
		{
			get
			{
				if (fTaxRecognitionCodeList == null)
				{
					fTaxRecognitionCodeList = new AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes();
					fTaxRecognitionCodeList.Remove(AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.AccrualBasis);
					fTaxRecognitionCodeList.Remove(AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis);
				}
				return fTaxRecognitionCodeList;
			}
		}
		CodeDescriptionPairList fTaxRecognitionCodeList;

		#region Login Company Country Rule
		public CodeDescriptionPairList LoginCompanyCountryRuleCodeList
		{
			get
			{
				if (fLoginCompanyCountryRuleCodeList == null)
				{
					fLoginCompanyCountryRuleCodeList = new CodeDescriptionPairList();
					fLoginCompanyCountryRuleCodeList.AddPair(LoginCompanyCountryRuleCode.IEU, ResString.GetMultilingualString("CD9EE99A-0701-4BB8-9F9C-CD5E9A4A03E1", "European Union Country/Region"));
					fLoginCompanyCountryRuleCodeList.AddPair(LoginCompanyCountryRuleCode.OEU, ResString.GetMultilingualString("749E75AC-24D7-4B7A-B63C-C4D3931DB9AF", "Outside the European Union"));
				}
				return fLoginCompanyCountryRuleCodeList;
			}
		}
		CodeDescriptionPairList fLoginCompanyCountryRuleCodeList;

		public static class LoginCompanyCountryRuleCode
		{
			public const string IEU = "IEU";
			public const string OEU = "OEU";
		}

		#endregion

		#region Organization Country Rule

		public CodeDescriptionPairList OrganizationCountryRuleCodeList
		{
			get
			{
				if (fOrganizationCountryRuleCodeList == null)
				{
					fOrganizationCountryRuleCodeList = new CodeDescriptionPairList();
					fOrganizationCountryRuleCodeList.AddPair(OrganizationCountryRuleCode.IEU, ResString.GetMultilingualString("DFFBC8F7-0E54-4BD6-9170-65787392C133", "European Union Country/Region"));
					fOrganizationCountryRuleCodeList.AddPair(OrganizationCountryRuleCode.OEU, ResString.GetMultilingualString("DAD22437-8C5D-4D2E-9C97-6A2E9F6E5A26", "Outside the European Union"));
					fOrganizationCountryRuleCodeList.AddPair(OrganizationCountryRuleCode.SAL, ResString.GetMultilingualString("BF8C2D26-60B6-44D0-9046-7563D78E0A26", "Same country/region as login Country/Region"));
					fOrganizationCountryRuleCodeList.AddPair(OrganizationCountryRuleCode.DTL, ResString.GetMultilingualString("F2A3C86C-F1FA-4BB4-A668-4759A223A291", "Different country/region from login country/region"));
				}
				return fOrganizationCountryRuleCodeList;
			}
		}
		CodeDescriptionPairList fOrganizationCountryRuleCodeList;

		public static class OrganizationCountryRuleCode
		{
			public const string IEU = "IEU";
			public const string OEU = "OEU";
			public const string SAL = "SAL";
			public const string DTL = "DTL";
		}

		#endregion
	}
}
