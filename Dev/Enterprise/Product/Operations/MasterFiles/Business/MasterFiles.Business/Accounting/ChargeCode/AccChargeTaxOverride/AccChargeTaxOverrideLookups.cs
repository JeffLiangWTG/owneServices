using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeTaxOverrideLookups : AutoAccChargeTaxOverrideLookups
	{
		public AccChargeTaxOverrideLookups(AutoAccChargeTaxOverride parent) : base(parent) { }

		#region Job Types

		public CodeDescriptionPairList JobTypes
		{
			get
			{
				var result = Parent.JobTypeDirectionAndTransportListProvider.JobTypeList;
				if (!result.ContainsCode(AccountingMasterFilesConstants.JobTypes.NonJobRelated))
				{
					var njrPair = new CodeDescriptionPair(AccountingMasterFilesConstants.JobTypes.NonJobRelated, Res.GetString("D7E9FDC3-0492-431A-A4DA-C175932F0DB7", "Non-Job"));
					result.InsertInSortOrder(njrPair);
				}

				return result;
			}
		}

		#endregion

		#region OrganisationCategoryList

		public CodeDescriptionPairList OrganisationCategoryList
		{
			get
			{
				if (fOrganisationCategoryList == null)
				{
					fOrganisationCategoryList = new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory);
					fOrganisationCategoryList.Insert(0, new CodeDescriptionPair(AccChargeTaxOverride.ALL, CategoryAdditionalDescriptions.All));
				}
				return fOrganisationCategoryList;
			}
		}

		CodeDescriptionPairList fOrganisationCategoryList;

		public static class CategoryAdditionalDescriptions
		{
			public static string All
			{
				get { return Res.GetString("7da76348-cd34-48dd-80c3-024c8e0ba8a6", "All Categories"); }
			}
		}

		#endregion

		#region DirectionList

		public CodeDescriptionPairList DirectionList => Parent.JobTypeDirectionAndTransportListProvider.DirectionList;

		#endregion

		#region Mode List

		public CodeDescriptionPairList TransportModeList => Parent.JobTypeDirectionAndTransportListProvider.TransportModeList;

		#endregion

		#region INCOTERMS

		public CodeDescriptionPairList Incoterms
		{
			get
			{
				CodeDescriptionPairList fIncoterms = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
				fIncoterms.AddPair("ALL", Res.GetString("6d6965e4-b57f-16ad-4c55-5041eb65036b", "All Incoterms"));

				return fIncoterms;
			}
		}

		#endregion

		#region Cost / Sell List

		public const string Cost = "COS";
		public const string Revenue = "REV";
		public const string ALL = "ALL";

		public CodeDescriptionPairList CostSellList
		{
			get
			{
				CodeDescriptionPairList fCostSellList = new CodeDescriptionPairList();
				fCostSellList.AddPair(ALL, Res.GetString("14cabda2-7cd6-4546-9d4d-548f6197f1b0", "Cost and Revenue"));
				fCostSellList.AddPair(Cost, Res.GetString("01afd92d-abb4-46f1-a7cc-0ea2badee89d", "Cost"));
				fCostSellList.AddPair(Revenue, Res.GetString("52a8a112-7ebb-4993-bdee-80b492a70970", "Revenue"));

				return fCostSellList;
			}
		}

		#endregion

		public CodeDescriptionPairList Locations
		{
			get
			{
				return AccountingTaxLocations.GetLocations(Factory);
			}
		}

		public CodeDescriptionPairList TaxRegistrationLocations
		{
			get
			{
				return AccountingTaxLocations.GetTaxRegistrationLocations(Factory);
			}
		}

		ZString ParentCountryCode
		{
			get { return Parent != null ? Parent.CurrentCountryCode : ZString.Empty; }
		}

		public CodeDescriptionPairList CustomsStatusList
		{
			get
			{
				if (customsStatusList == null)
				{
					var result = new CodeDescriptionPairList();

					var customList = ObjectFactory.Get<Enterprise.Integration.Customs.ICusEntryNumberTypes>()
						.CountrySpecificCustomsEntryNumberTypeList(Factory, ParentCountryCode, false) as CodeDescriptionPairList;
					result.AddPairsIfNotExist(customList.ToArray());

					var freightList = ObjectFactory.New<Enterprise.Integration.Freight.ICommunityTransitStatusCodes>(Factory)
						.GetList() as CodeDescriptionPairList;
					result.AddPairsIfNotExist(freightList.ToArray());

					customsStatusList = result;
				}
				return customsStatusList;
			}
		}

		CodeDescriptionPairList customsStatusList;

		public CodeDescriptionPairList SupplyTypes
		{
			get
			{
				if (supplyTypes == null)
				{
					var registryItem = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList;
					supplyTypes = registryItem.Value.GetActiveCodeDescriptionPairList();
				}
				return supplyTypes;
			}
		}

		CodeDescriptionPairList supplyTypes;

		public override AccTaxRateCollection TaxRates
		{
			get
			{
				if (Parent.IsTaxFrameworkRelated)
				{
					var taxConfiguration = Parent.TaxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.FirstOrDefault(x => x.AXP_AT_TaxID.IsEmpty)?.TaxConfiguration;
					return new NonVATAccTaxRateCollection(Factory, ParentCountryCode, taxConfiguration);
				}
				else
				{
					return new VATAccTaxRateCollection(Factory, ParentCountryCode);
				}
			}
		}

		public override AccInvMsgCollection DefaultVATClasses
		{
			get { return new AccInvMsgCollection(Factory, ParentCountryCode); }
		}

		new AccChargeTaxOverride Parent
		{
			get { return (AccChargeTaxOverride)base.Parent; }
		}

		#region Defaulting Rule List

		public CodeDescriptionPairList DefaultingRuleList => fDefaultingRuleList ?? (fDefaultingRuleList = new CodeDescriptionPairList(OLookUpEditType.TaxOverrideDefaultingRule));
		CodeDescriptionPairList fDefaultingRuleList;

		#endregion

		#region Transaction Context List

		public CodeDescriptionPairList TransactionContextList => fTransactionContextList ?? (fTransactionContextList = new CodeDescriptionPairList(OLookUpEditType.TaxOverrideTransactionContext));
		CodeDescriptionPairList fTransactionContextList;

		#endregion

		#region DebtorRoleList

		public CodeDescriptionPairList DebtorRoleList => debtorRoleList ?? (debtorRoleList = new DebtorRoleList());
		CodeDescriptionPairList debtorRoleList;

		#endregion
	}
}
