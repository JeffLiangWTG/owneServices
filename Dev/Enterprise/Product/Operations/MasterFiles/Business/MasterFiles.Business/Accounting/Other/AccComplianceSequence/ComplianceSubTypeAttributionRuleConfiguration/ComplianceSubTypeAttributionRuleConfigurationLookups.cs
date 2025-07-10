using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Registry.Business
{
	public class ComplianceSubTypeAttributionRuleConfigurationLookups : ZLookups, IComplianceSubTypeListAdditionalDataProvider
	{
		public ComplianceSubTypeAttributionRuleConfigurationLookups(ComplianceSubTypeAttributionRuleConfiguration parent)
			: base(parent)
		{
			this.Parent = parent;
			this.BizOFactory = new BusinessObjectFactory();
		}
		new readonly ComplianceSubTypeAttributionRuleConfiguration Parent;

		readonly BusinessObjectFactory BizOFactory;

		public CodeDescriptionPairList SubTypeList => Lists.GetSubTypeList(Parent.Country);

		public CodeDescriptionPairList LedgerTypeList => Lists.GetLedgerTypeList(this);

		public CodeDescriptionPairList InvoiceTypeList => Lists.GetInvoiceTypeList(this);

		public RefCountryCollection CountryList => Lists.GetCountryList(BizOFactory);

		public CodeDescriptionPairList OrganisationLocationList => Lists.GetOrganisationLocationList(Parent.Country, BizOFactory);

		#region Tax Invoice Rule List

		public CodeDescriptionPairList TaxInvoiceRuleList => Lists.GetTaxInvoiceRuleList(this);

		#endregion

		#region Original Rule List

		public CodeDescriptionPairList OriginalRuleList => Lists.OriginalRuleList;

		#endregion

		#region Tax Registration Type List

		public CodeDescriptionPairList TaxRegistrationTypeList => Lists.GetTaxRegistrationTypeList(this, Parent.Country);

		#endregion

		#region Disbursement Rule List

		public CodeDescriptionPairList DisbursementRuleList => Lists.DisbursementRuleList;

		#endregion

		#region Exporter Exemption List

		public CodeDescriptionPairList ExporterExemptionList => Lists.ExporterExemptionList;

		#endregion

		#region Self Billing Rule List

		public CodeDescriptionPairList SelfBillingRuleList => Lists.SelfBillingRuleList;

		#endregion

		#region Tax Registration Location Rule List

		public CodeDescriptionPairList TaxRegistrationLocationRuleList => Lists.GetTaxRegistrationLocationRuleList(Parent.Country, BizOFactory);

		#endregion

		#region VAT Group List

		public CodeDescriptionPairList VATGroupList => Lists.VATGroupList;

		#endregion

		#region Tax System List

		public CodeDescriptionPairList TaxSystemList => GetTaxSystemList();

		CodeDescriptionPairList GetTaxSystemList()
		{
			var taxSystemsCodeDescriptionPairList = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystems(Parent.Country);

			var result = new CodeDescriptionPairList(taxSystemsCodeDescriptionPairList);
			result.Insert(0, new CodeDescriptionPair(string.Empty, string.Empty));
			return result;
		}

		#endregion

		#region RegistrationCode List

		public CodeDescriptionPairList RegistrationCodeList => new OrgCodeLists().CustomsCodes_List(Parent.Country, BizOFactory);

		#endregion

		#region OrganisationCategory List

		public CodeDescriptionPairList OrganisationCategoryList => new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory);

		#endregion

		#region IComplianceSubTypeListDataProvider

		CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalTaxInvoiceRuleList() => new CodeDescriptionPairList();

		CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalLedgerTypeList() => new CodeDescriptionPairList();

		CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalInvoiceTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(TransactionTypes.Invoice, Res.GetString("0389748d-f2db-48b7-b6bb-43a4dda1ef66", "Invoice"));
			result.AddPair(TransactionTypes.CreditNote, Res.GetString("8c4877a2-2bbf-464f-a876-b8d464e810f1", "Credit Note"));
			result.AddPair(TransactionTypes.AdjustmentNote, Res.GetString("6111ea15-f5f7-4ac0-a218-49502e731a58", "Adjustment Note"));
			return result;
		}

		CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalTaxRegistrationTypeList()
		{
			var result = CountryComplianceFactory.GetIComplianceSubTypeAdditionalTaxRegistrationTypeListProvider(Parent.Country)?.GetTaxRegistrationTypeList();
			#region This code is obsolete plese dont add new countries in this section instead use CountryComplianceFactory
			if (result == null)
			{
				result = new CodeDescriptionPairList();
				if (Parent.Country == Core.Constants.CountryCodes.Peru)
				{
					result.AddPair(TaxRegistrationTypeCodes.Individual, TaxRegistrationTypeDescriptions.Individual);
					result.AddPair(TaxRegistrationTypeCodes.OrgRegisteredForTaxInPeru, TaxRegistrationTypeDescriptions.OrgRegisteredForTax);
				}
			}
			#endregion
			return result;
		}

		#endregion
	}
}
