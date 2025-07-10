using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class AccChargeTaxOverrideMatcher
	{
		public class TaxCalculationParameters : ConfigurationMatcherHelper.ConfigurationMatcherParameters
		{
			public ZString IncoTerm;
			public OrgHeader Organisation;
			public OrgHeader Consignor;
			public OrgHeader Consignee;
			public ZString CustomsStatus;
			public ZString CommunityTransitStatus;
			public ILocation FixedPlaceOfSupply;
			public ZString TransactionContext;
			public ZString SupplyType;
		}

		public static AccChargeTaxOverride GetChargeTaxOverride(this AccChargeCode parent, TaxCalculationParameters parameters) => GetChargeTaxOverride(parent.Factory, parent.PK, parameters);

		public static AccChargeTaxOverride GetChargeTaxOverride(this AccTaxOverrideGroup parent, TaxCalculationParameters parameters) => GetChargeTaxOverride(parent.Factory, parent.PK, parameters);

		static AccChargeTaxOverride GetChargeTaxOverride(BusinessObjectFactory factory, ZGuid parentPK, TaxCalculationParameters parameters)
		{
			ZQuery mainQuery = new ZQuery(AccChargeTaxOverrideSchema.AO_ParentID, parentPK);

			AccChargeTaxOverride[] matchedTaxOverrides = GetRankerForTaxRate(parameters, factory).GetBestMatches<AccChargeTaxOverride>(factory, mainQuery, useInMemoryFiltering: false, "", 0);
			AccChargeTaxOverride resultingTaxOverride = null;
			foreach (AccChargeTaxOverride taxOverride in matchedTaxOverrides)
			{
				var taxOverrideSatisfiesExporterExemption = !taxOverride.AO_VATExemptOnExportCharges || parameters.Organisation.IsExporterExempt(taxOverride.CurrentCountryCode, parameters.CostOrSell);
				if (taxOverrideSatisfiesExporterExemption)
				{
					resultingTaxOverride = VerifyBasedOnSplitPaymentVAT(taxOverride, parameters.Organisation, parameters.Country);
					if (resultingTaxOverride != null)
					{
						break;
					}
				}
			}

			return (resultingTaxOverride?.AO_CreateTaxRecord ?? false) ? resultingTaxOverride : null;
		}

		static AccChargeTaxOverride VerifyBasedOnSplitPaymentVAT(AccChargeTaxOverride taxOverride, OrgHeader organisation, RefCountry country)
		{
			AccChargeTaxOverride result = null;
			if (organisation == null || country.Code != Core.Constants.CountryCodes.Italy ||
				!taxOverride.AO_SplitPaymentVATOrganisation || organisation.CompanyData.OB_ARVATSplitPaymentApplicable)
			{
				result = taxOverride;
			}

			return result;
		}

		static ColumnValueRanker GetRankerForTaxRate(TaxCalculationParameters parameters, BusinessObjectFactory factory)
		{
			ZString costSellValue = parameters.CostOrSell == CostSell.Cost ? AccChargeTaxOverrideLookups.Cost : AccChargeTaxOverrideLookups.Revenue;
			ZString orgCategoryValue = parameters.Organisation != null ? parameters.Organisation.OH_Category : ZString.Empty;
			ZString transactionContext = parameters.TransactionContext.IsEmpty ? (ZString)Constants.TaxOverrideTransactionContext.Codes.Standard : parameters.TransactionContext;

			var branchState = string.Empty;
			if (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value)
			{
				branchState = AccountingTaxLocations.GetBranchState(parameters.Branch);
			}

			var ranker = new ColumnValueRanker();
			if (parameters.JobType == AccountingMasterFilesConstants.JobTypes.NonJobRelated)
			{
				ranker.Add(AccChargeTaxOverrideSchema.AO_JobType, parameters.JobType);
			}
			else
			{
				ranker.Add(AccChargeTaxOverrideSchema.AO_JobType, parameters.JobType, (ZString)AccChargeTaxOverride.ALL);
			}
			ranker.Add(AccChargeTaxOverrideSchema.AO_CostSellAll, costSellValue, (ZString)AccChargeTaxOverride.ALL);
			ranker.Add(AccChargeTaxOverrideSchema.AO_IncoTerm, parameters.IncoTerm, (ZString)AccChargeTaxOverride.ALL);
			ranker.Add(AccChargeTaxOverrideSchema.AO_Direction, ConfigurationMatcherHelper.GetDirectionFallBackCodes(GetDirectionValue(parameters.Direction), parameters));
			ranker.Add(AccChargeTaxOverrideSchema.AO_TransportMode, parameters.TransportMode, (ZString)AccChargeTaxOverride.ALL, ZString.Empty);
			ranker.Add(AccChargeTaxOverrideSchema.AO_OrganisationCategory, orgCategoryValue, (ZString)AccChargeTaxOverride.ALL);
			ranker.Add(AccChargeTaxOverrideSchema.AO_TaxRegCntryOrGroup, GetTaxRegistration(parameters, factory));
			ranker.Add(AccChargeTaxOverrideSchema.AO_Origin, AccountingTaxLocations.GetLocationFallbackCodes(factory, parameters.Origin, parameters.Country.Code, branchState));
			ranker.Add(AccChargeTaxOverrideSchema.AO_Destination, AccountingTaxLocations.GetLocationFallbackCodes(factory, parameters.Destination, parameters.Country.Code, branchState));
			ranker.Add(AccChargeTaxOverrideSchema.AO_DebtorRole, GetDebtorRoleFallBackCodes(parameters));

			var customsStatus = !string.IsNullOrWhiteSpace(parameters.CommunityTransitStatus)
				? parameters.CommunityTransitStatus
				: parameters.CustomsStatus;
			ranker.Add(AccChargeTaxOverrideSchema.AO_CustomsStatus, customsStatus, ZString.Empty);

			ranker.Add(AccChargeTaxOverrideSchema.AO_HomeCountryOrZone, AccountingTaxLocations.GetHomeCountryFallbacks(factory, parameters.Country.Code, branchState, parameters.Organisation, parameters.FixedPlaceOfSupply));
			ranker.Add(AccChargeTaxOverrideSchema.AO_VATExemptOnExportCharges, ZBool.True, ZBool.False);
			ranker.Add(AccChargeTaxOverrideSchema.AO_SplitPaymentVATOrganisation, ZBool.True, ZBool.False);
			ranker.Add(AccChargeTaxOverrideSchema.AO_TransactionContext, transactionContext, (ZString)AccChargeTaxOverride.ALL);

			if (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value)
			{
				ranker.Add(AccChargeTaxOverrideSchema.AO_GB, parameters.Branch?.PK ?? ZGuid.Empty, ZGuid.Empty);
			}

			if (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				ranker.Add(AccChargeTaxOverrideSchema.AO_SupplyType, parameters.SupplyType, ZString.Empty);
			}

			return ranker;
		}

		static IZType[] GetTaxRegistration(TaxCalculationParameters parameters, BusinessObjectFactory factory)
		{
			var taxRegistrationCountry = parameters.Organisation != null && !parameters.Organisation.RawTaxRegistrationNumber.IsEmpty
				? parameters.Organisation.CountryOfTaxRegistration
				: null;

			var taxRegions = AccountingTaxLocations.GetCountryFallbackCodes(factory, taxRegistrationCountry, parameters.Country.Code, null, "");
			var sezCode = parameters.Organisation?.CustomsCodes.GetOrgCusCode((ZString)OrgCusCode.CodeTypes.SpecialEconomicZone, parameters.Country);
			if (sezCode != null)
			{
				taxRegions = taxRegions.Prepend((ZString)OrgCusCode.CodeTypes.SpecialEconomicZone).ToArray();
			}

			return taxRegions;
		}

		static IZType[] GetDebtorRoleFallBackCodes(TaxCalculationParameters parameters)
		{
			List<IZType> result = new List<IZType>();
			ZString costSellValue = parameters.CostOrSell == CostSell.Cost ? AccChargeTaxOverrideLookups.Cost : AccChargeTaxOverrideLookups.Revenue;
			var jobType = parameters.JobType;
			var debtor = parameters.Organisation;
			var consignor = parameters.Consignor;
			var consignee = parameters.Consignee;
			if (debtor != null && jobType == JobInvoicingConsumerTypes.ShipmentCode && costSellValue == AccChargeTaxOverrideLookups.Revenue)
			{
				if (debtor.CompanyData.OB_ARGoodsOwnership == OrgCompanyDataLookups.GoodsOwnership.NeverOwner)
				{
					result.Add((ZString)DebtorRoleList.Codes.NotOwnGoods);
				}
				if (debtor.OH_IsForwarder)
				{
					result.Add((ZString)DebtorRoleList.Codes.Agent);
				}
				if (debtor.OH_Code != (consignor?.OH_Code ?? ZString.Empty)
					&& debtor.OH_Code != (consignee?.OH_Code ?? ZString.Empty))
				{
					result.Add((ZString)DebtorRoleList.Codes.NotConsignorOrConsignee);
				}
			}
			result.Add(ZString.Empty);
			return result.ToArray();
		}

		public static ZString GetDirectionValue(Directions direction)
		{
			ZString directionValue = "";

			if (direction == Directions.Import)
			{
				directionValue = OrgConstants.ServiceDirection.Code.Import;
			}
			else if (direction == Directions.Export)
			{
				directionValue = OrgConstants.ServiceDirection.Code.Export;
			}
			else if (direction == Directions.Domestic)
			{
				directionValue = OrgConstants.ServiceDirection.Code.Domestic;
			}
			else if (direction == Directions.CrossTrade)
			{
				directionValue = AccChargeTaxOverride.DirectionType_Other;
			}

			return directionValue;
		}
	}
}
