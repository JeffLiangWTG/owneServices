using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.MasterFiles.Module
{
	public class RefPremisesGateCodeFilterBusinessObject : FilterStripBusinessObject
	{
		public RefPremisesGateCodeFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddFlagsFilters(result);
			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Premises Code", RefPremisesGateCodeSchema.R5_PremisesGateCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPremisesGateCodeFilter|PremisesCode", "Premises Code");

			filters.AddTextFilter("Data Provider", RefPremisesGateCodeSchema.R5_DataProvider, TypesOfDataProviders).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPremisesGateCodeFilter|DataProvider", "Data Provider");
		}

		#endregion

		#region Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter wharfFilter = filters.AddTextFilter("Is Wharf", GetIsWharfFilter, IsWharfList);
			wharfFilter.Category = FilterCategories.StatusAndFlags;
			wharfFilter.DefaultProperty = IsWharfCodes.IsWharf;
			wharfFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPremisesGateCodeFilter|IsWharf", "Is Wharf");

			ModuleTextFilter containerYardFilter = filters.AddTextFilter("Is Container Yard", GetIsContainerYardFilter, IsContainerYardList);
			containerYardFilter.Category = FilterCategories.StatusAndFlags;
			containerYardFilter.DefaultProperty = IsContainerYardCodes.IsContainerYard;
			containerYardFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPremisesGateCodeFilter|IsContainerYard", "Is Container Yard");
		}

		ZQuery GetIsWharfFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == IsWharfCodes.IsWharf)
			{
				query.AddToFilter(RefPremisesGateCodeSchema.R5_IsWharf, ZBool.True);
			}
			else if (value == IsWharfCodes.IsNotWharf)
			{
				query.AddToFilter(RefPremisesGateCodeSchema.R5_IsWharf, ZBool.False);
			}

			return query;
		}

		ZQuery GetIsContainerYardFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == IsContainerYardCodes.IsContainerYard)
			{
				query.AddToFilter(RefPremisesGateCodeSchema.R5_IsContainerYard, ZBool.True);
			}
			else if (value == IsContainerYardCodes.IsNotContainerYard)
			{
				query.AddToFilter(RefPremisesGateCodeSchema.R5_IsContainerYard, ZBool.False);
			}

			return query;
		}

		#endregion

		#endregion

		#region Lookups

		PremiseGateCodeDataProviderList fDataProviderTypes;

		PremiseGateCodeDataProviderList TypesOfDataProviders
		{
			get
			{
				if (fDataProviderTypes == null)
				{
					fDataProviderTypes = new PremiseGateCodeDataProviderList();
				}
				return fDataProviderTypes;
			}
		}

		#region Flag Filters Lookups

		CodeDescriptionPairList IsWharfList
		{
			get
			{
				if (R5IsWharfList == null)
				{
					R5IsWharfList = new CodeDescriptionPairList();
					R5IsWharfList.AddPair(IsWharfCodes.All, Enterprise.MasterFiles.Module.Res.GetString("MasterFiles|RefPremisesGateCodeFilter|All", "All"));
					R5IsWharfList.AddPair(IsWharfCodes.IsWharf, Enterprise.MasterFiles.Module.Res.GetString("MasterFiles|RefPremisesGateCodeFilter|IsWharf", "Is Wharf"));
					R5IsWharfList.AddPair(IsWharfCodes.IsNotWharf, Enterprise.MasterFiles.Module.Res.GetString("MasterFiles|RefPremisesGateCodeFilter|IsNotWharf", "Is Not Wharf"));
				}
				return R5IsWharfList;
			}
		}

		CodeDescriptionPairList R5IsWharfList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		static class IsWharfCodes
		{
			public const string All = "All";
			public const string IsWharf = "Is Wharf";
			public const string IsNotWharf = "Is Not Wharf";
		}

		CodeDescriptionPairList IsContainerYardList
		{
			get
			{
				if (R5IsContainerYardList == null)
				{
					R5IsContainerYardList = new CodeDescriptionPairList();
					R5IsContainerYardList.AddPair(IsContainerYardCodes.All, Enterprise.MasterFiles.Module.Res.GetString("MasterFiles|RefPremisesGateCodeFilter|All", "All"));
					R5IsContainerYardList.AddPair(IsContainerYardCodes.IsContainerYard, Enterprise.MasterFiles.Module.Res.GetString("MasterFiles|RefPremisesGateCodeFilter|IsContainerYard", "Is Container Yard"));
					R5IsContainerYardList.AddPair(IsContainerYardCodes.IsNotContainerYard, Enterprise.MasterFiles.Module.Res.GetString("MasterFiles|RefPremisesGateCodeFilter|IsNotContainerYard", "Is Not Container Yard"));
				}
				return R5IsContainerYardList;
			}
		}

		CodeDescriptionPairList R5IsContainerYardList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		static class IsContainerYardCodes
		{
			public const string All = "All";
			public const string IsContainerYard = "Is Container Yard";
			public const string IsNotContainerYard = "Is Not Container Yard";
		}

		#endregion

		#endregion
	}
}
