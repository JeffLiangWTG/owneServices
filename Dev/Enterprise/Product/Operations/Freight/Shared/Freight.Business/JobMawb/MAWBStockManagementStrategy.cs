using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class MAWBStockManagementStrategy
	{
		readonly BusinessObjectFactory factory;
		readonly Dictionary<string, MAWBBranchStrategy> dictStrategies;
		readonly Dictionary<string, OrgAirlineMAWBStockManagementCollection> dictStockManagementCollections;

		public MAWBStockManagementStrategy(BusinessObjectFactory factory)
		{
			this.factory = factory;
			dictStrategies = new Dictionary<string, MAWBBranchStrategy>();
			dictStockManagementCollections = new Dictionary<string, OrgAirlineMAWBStockManagementCollection>();
		}

		public bool CanAddMawb(string airlinePrefix, GlbCompany company, GlbBranch branch)
		{
			var stockManagements = GetOrgAirlineMAWBStockManagements(airlinePrefix);

			// Global level
			if (company == null && branch == null)
			{
				return stockManagements?.Any(x => x.OHM_AllowUseGlobalStock) == true;
			}

			// Company level
			if (company != null && branch == null)
			{
				return stockManagements?.Any(x => x.OHM_GC_Company == company.PK && x.OHM_GB_Branch == ZGuid.Empty && x.OHM_AllowUseCompanyStock) == true;
			}

			// Branch level
			// This should fetch allow use branch stock from strategy
			if (company != null && branch != null)
			{
				return GetBranchStrategy(branch, airlinePrefix).UseBranchStock;
			}

			return true;
		}

		public bool CanAllocateMawbToCurrentBranch(JobMawb mawb, out string stockLevel)
		{
			stockLevel = string.Empty;
			// Do not check carrier mawb
			if (mawb == null || mawb.JM_IsPaper)
			{
				return true;
			}

			var stockStrategy = GetBranchStrategy(GlbBranch.CurrentBranch, mawb.JM_Airline3DigitPrefix);
			var isMawbBranchLevel = mawb.JM_GC_Company == GlbCompany.CurrentCompany.PK && mawb.JM_GB == GlbBranch.CurrentBranch.PK;
			var isMawbCompanyLevel = mawb.JM_GC_Company == GlbCompany.CurrentCompany.PK && mawb.JM_GB.IsEmpty;
			var isMawbGlobalLevel = mawb.JM_GC_Company.IsEmpty && mawb.JM_GB.IsEmpty;
			var isMawbOtherBranchLevel = mawb.JM_GC_Company == GlbCompany.CurrentCompany.PK && mawb.JM_GB.IsValid && mawb.JM_GB != GlbBranch.CurrentBranch.PK;
			var isMawbOtherCompanyLevel = mawb.JM_GC_Company.IsValid && mawb.JM_GC_Company != GlbCompany.CurrentCompany.PK;

			if (isMawbBranchLevel && !stockStrategy.UseBranchStock)
			{
				stockLevel = Res.GetString("88548b08-4084-47eb-bf42-698febdf1bda", "branch");
				return false;
			}

			if (isMawbCompanyLevel && !stockStrategy.UseCompanyStock)
			{
				stockLevel = Res.GetString("f0d35468-9c40-4f7e-9129-8bc8779ab31f", "company");
				return false;
			}

			if (isMawbGlobalLevel && !stockStrategy.UseGlobalStock)
			{
				stockLevel = Res.GetString("c8792c9f-c288-4297-abae-c60f11d96847", "global");
				return false;
			}

			if (isMawbOtherBranchLevel && !stockStrategy.UseOtherBranchStock)
			{
				stockLevel = Res.GetString("fafae7c5-6873-4c73-a1d9-18d4db11d144", "other branch");
				return false;
			}

			if (isMawbOtherCompanyLevel)
			{
				stockLevel = Res.GetString("75b118a0-c01d-4240-ada5-60c8528590fe", "other company");
				return false;
			}

			return true;
		}

		public MAWBBranchStrategy GetBranchStrategy(GlbBranch branch, string airlinePrefix)
		{
			// Get value from dictionary so it does not hit DB every time
			var key = ComposeDictKey(branch, airlinePrefix);

			// In many unit tests we change setting on the fly so can't use cache for unit tests
#if !DEBUG
			if (dictStrategies.TryGetValue(key, out var value))
			{
				return value;
			}
#endif
			// Setting priority order: branch > company > global > default
			// Get airline mawb stock management
			var stockManagements = GetOrgAirlineMAWBStockManagements(airlinePrefix);
			if (stockManagements.IsNullOrEmpty())
			{
				// If the airlinePrefix does not have org or does not have mawb stock management just return default setting
				return SaveToDictionary(key, GetDefaultSetting());
			}

			// Branch setting
			var stockManagement = stockManagements.FirstOrDefault(x => x.OHM_GB_Branch == branch.PK);
			if (stockManagement != null)
			{
				return SaveToDictionary(key, MapManagementToStrategy(stockManagement));
			}

			// Company setting
			stockManagement = stockManagements.FirstOrDefault(x => x.OHM_GC_Company == branch.Company.PK && x.OHM_GB_Branch == ZGuid.Empty);
			if (stockManagement != null)
			{
				return SaveToDictionary(key, MapManagementToStrategy(stockManagement));
			}

			// Global setting
			stockManagement = stockManagements.FirstOrDefault(x => x.OHM_GC_Company == ZGuid.Empty && x.OHM_GB_Branch == ZGuid.Empty);
			if (stockManagement != null)
			{
				return SaveToDictionary(key, MapManagementToStrategy(stockManagement));
			}

			return GetDefaultSetting();
		}

		public OrgHeader FindCarrierBy3CharAirlineCode(string airlinePrefix)
		{
			var zDBOnlySubQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
			zDBOnlySubQuery.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airlinePrefix);

			var zDBOnlyQuery = new ZDBOnlyQuery(typeof(OrgMiscServ));
			zDBOnlyQuery.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, zDBOnlySubQuery, JoinCondition.And);

			var carrierOrgs = factory.Load<OrgMiscServ>(zDBOnlyQuery);
			if (carrierOrgs?.Length == 0)
			{
				return null;
			}

			// We do have situation that multiple carries share the same airline prefix
			// In this case the carrier in same country with current branch should take priority
			if (carrierOrgs.Length > 1)
			{
				var branchCountryCode = !string.IsNullOrEmpty(GlbBranch.CurrentBranch.GB_RN_NKCountryCode)
					? GlbBranch.CurrentBranch.GB_RN_NKCountryCode
					: GlbBranch.CurrentBranch.Country?.RN_Code;
				var orgInSameCountry = carrierOrgs.FirstOrDefault(
					x => !string.IsNullOrEmpty(branchCountryCode) && Equals(x.OM_RN_NKEXDefaultCntryOfOrigin, branchCountryCode));
				if (orgInSameCountry != null)
				{
					return orgInSameCountry.Header;
				}
			}

			// If there is no match return the last modified one
			return carrierOrgs.OrderBy(x => x.OM_SystemLastEditTimeUtc).Last().Header;
		}

		static string ComposeDictKey(GlbBranch branch, string airlinePrefix)
			=> $"{airlinePrefix}:{branch.PK}".ToLower();

		OrgAirlineMAWBStockManagementCollection GetOrgAirlineMAWBStockManagements(string airlinePrefix)
		{
			if (!dictStockManagementCollections.ContainsKey(airlinePrefix))
			{
				dictStockManagementCollections[airlinePrefix] = FindCarrierBy3CharAirlineCode(airlinePrefix)?.OrgAirlineMAWBStockManagementCollection;
			}

			return dictStockManagementCollections[airlinePrefix];
		}

		MAWBBranchStrategy GetDefaultSetting()
		{
			return new MAWBBranchStrategy
			{
				StockThreshold = FreightDataRegistry.Instance.MAWBDefaultLowStockLevel.Value,
				UseBranchStock = true,
				UseCompanyStock = false,
				UseGlobalStock = false,
				UseOtherBranchStock = FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.Value
			};
		}

		MAWBBranchStrategy MapManagementToStrategy(OrgAirlineMAWBStockManagement management)
		{
			return new MAWBBranchStrategy
			{
				StockThreshold = management.OHM_MAWBStockThreshold,
				UseBranchStock = management.OHM_AllowUseBranchStock,
				UseCompanyStock = management.OHM_AllowUseCompanyStock,
				UseGlobalStock = management.OHM_AllowUseGlobalStock,
				UseOtherBranchStock = management.OHM_AllowUseOtherBranchStock.ToString() switch
				{
					"Y" => true,
					"N" => false,
					_ => FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.Value,
				}
			};
		}

		MAWBBranchStrategy SaveToDictionary(string key, MAWBBranchStrategy value)
		{
			dictStrategies[key] = value;
			return value;
		}
	}

	public struct MAWBBranchStrategy
	{
		public int StockThreshold { get; set; }
		public bool UseBranchStock { get; set; }
		public bool UseCompanyStock { get; set; }
		public bool UseGlobalStock { get; set; }
		public bool UseOtherBranchStock { get; set; }
	}
}
