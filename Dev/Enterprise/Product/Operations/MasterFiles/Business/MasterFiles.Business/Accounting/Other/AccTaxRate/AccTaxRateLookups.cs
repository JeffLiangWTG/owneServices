//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxRateLookups
//
//    This class should be used for overriding collections in AutoAccTaxRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxRateLookups : AutoAccTaxRateLookups
	{
		public AccTaxRateLookups(AutoAccTaxRate parent) : base(parent) { }

		public override AccInvMsgCollection DefaultVatClasses
		{
			get { return new AccInvMsgCollection(Factory, Parent.AT_RN_NKCountry); }
		}

		public CodeDescriptionPairList Types
		{
			get { return AccountingMasterFilesConstants.TaxRateTypes; }
		}

		public CodeDescriptionPairList ExtraTypes
		{
			get
			{
				CodeDescriptionPairList fExtraTypes = new CodeDescriptionPairList();
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, Res.GetString("c68dabd4-741e-4d9a-b3f1-a3608f566b8d", "India Primary & Secondary Education Tax"));
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.QuebecQST, Res.GetString("4cd623b0-fae1-415c-b386-87a3aa3fbf64", "Quebec QST"));
				if (GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.Ghana)
				{
					fExtraTypes.AddRange(GhanaComplianceInfo.ExtraTypes.ToList());
				}
				else
				{
					var countryComplianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.Country.Code) as ICountryComplianceInfo;
					if (countryComplianceInfo != null && countryComplianceInfo.HasExtraTaxInfo().HasValue && countryComplianceInfo.HasExtraTaxInfo().Value)
					{
						fExtraTypes.AddPair(AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, countryComplianceInfo.GetExtraTaxDescription(AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase));
					}
					else
					{
						fExtraTypes.AddPair(AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, Res.GetString("fa9d9cd3-519c-4047-ac0f-68ee635aa219", "Quebec QST (excluding GST in QST Base)"));
					}
				}
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.ChinaInputVATClaimed, Res.GetString("ceb04f33-861d-40d6-b0b3-e060b0450675", "China Input VAT Claimed"));
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.VATRetention, Res.GetString("31ac62fc-055d-4d9a-b8bf-070dd169b158", "VAT Retention"));
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.VATRetentionFraction, Res.GetString("31ac62fc-055d-4d9a-b8bf-070dd169b158", "VAT Retention"));
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.VATRemittedByCustomer, Res.GetString("3b795174-a2ec-4561-b1c3-e19b7a25603c", "VAT Remitted by Customer"));
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax, Res.GetString("A6F24B6C-939D-4329-95E9-6484FA9C1F4B", "China Input VAT Offset Against Output"));
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.ServiceTax, Res.GetString("a584a32f-b8fb-40ba-bfb6-90f0aae7af0d", "Service Tax"));
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.RegionalTax, Res.GetString("e4da0445-563a-4feb-af56-ff328f71dfc2", "Regional Tax"));
				fExtraTypes.AddPair(AccTaxRate.ExtraTypes.StateGST, Res.GetString("c7708d5b-3aaa-4018-af4a-f9106f78be57", "State GST"));
				return fExtraTypes;
			}
		}

		new AutoAccTaxRate Parent
		{
			get { return (AutoAccTaxRate)base.Parent; }
		}
	}
}
