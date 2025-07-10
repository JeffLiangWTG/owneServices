using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI
{
	public sealed class RateChooserFilterStripBusinessObject : RateFilterStripBusinessObject
	{
		public RateChooserFilterStripBusinessObject()
		{
		}

		public RateChooserFilterStripBusinessObject(RatingCriteria criteria)
			: base(Argument.NotNull(criteria, nameof(criteria)), null)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "RateChooserFilterStripBusinessObject";
			ApplyDefaults();
		}

		#region Initializing

		public bool CanUpdateDate => OriginalCriteria?.AutoRating != null
				&& OriginalCriteria.AutoRating is AutoRatingProxy autoRatingProxy
				&& autoRatingProxy.AutoRating is IJobDataUpdater jobDataUpdater
				&& jobDataUpdater.CanUpdateDate;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddEffectiveOnFilter(filters, isReadOnly: !CanUpdateDate, FilterVisibility.AlwaysVisible, validation: MandatoryValidation.CheckEntered);
			AddLocationFilter(filters, isReadOnly: true);
			AddServiceProviderFilter(filters);

			bool addContainerTypeFilter = OriginalCriteria?.IsContainerised ?? ZArchitecture.Environment.Globals.IsTest;

			if (addContainerTypeFilter)
			{
				AddContainerTypeFilter(filters);
			}

			AddCarrierServiceLevelFilter(filters);
			AddContractNumberFilter(filters);
			AddNamedAccountFilter(filters);

			return filters;
		}

		protected override WiseRatesModuleTextFilter CreateContractNumberFilter()
		{
			if (IsCargoSphereRateSearchAllowed)
			{
				CodeDescriptionPairList GetList()
				{
					var pairs = new CodeDescriptionPairList();
					var csRefData = GetCargoSphereRefData();

					if (csRefData != null)
					{
						foreach (var contract in csRefData.Contracts.GroupBy(x => x.ContractNumber))
						{
							pairs.AddPair(contract.Key, // <- ContractNumber
								string.Join(", ", contract.SelectMany(x => x.IDs)));
						}
					}

					return pairs;
				}

				var filter = new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.CarrierContractNumber, GetList);
				filter.PropertyValidation = CheckCargoSphereReference;
				return filter;
			}

			return base.CreateContractNumberFilter();
		}

		protected override WiseRatesModuleTextFilter CreateNamedAccountFilter()
		{
			if (IsCargoSphereRateSearchAllowed)
			{
				CodeDescriptionPairList GetList()
				{
					var pairs = new CodeDescriptionPairList();
					var csRefData = GetCargoSphereRefData();

					if (csRefData != null)
					{
						foreach (var namedAccount in csRefData.NamedAccounts)
						{
							pairs.AddPair(namedAccount.Name, namedAccount.ID.ToString(DefaultCulture.Instance));
						}
					}

					return pairs;
				}

				var filter = new WiseRatesModuleTextFilter(RateEntryFilterUtility.Constants.Codes.NamedAccount, GetList);
				filter.PropertyValidation = CheckCargoSphereReference;
				return filter;
			}

			return base.CreateNamedAccountFilter();
		}

		void CheckCargoSphereReference(ZPropertyInfo info)
		{
			var warning = info.Notifications.FirstOrDefault(x => string.Equals(x.Message, ListValidation.InvalidCodeMessage.ToString()));

			if (warning != null)
			{
				info.ReplaceNotification(
					warning,
					warning.Type,
					ResString.GetMultilingualString("B0E56B57-7E81-467E-8878-524081C6D6C0", "It is not a valid code in CargoSphere system."));
			}
		}

		bool IsCargoSphereRateSearchAllowed =>
			DataRegistryRating.Instance.IsLicenseForRatesServiceValid(out _) &&
			DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(Core.Constants.TransportModes.Sea, out _) &&
			RatesServiceClient.CargoSphereRateSearchAllowed(RatesSearchRequest.Operation.Autorating);

		protected override IReadOnlyList<FilterCategory> CategorySortOrderCore => new FilterCategory[]
		{
			FilterCategories.Locations,
			FilterCategories.ModesAndTypes,
			FilterCategories.Organisations,
			FilterCategories.Dates,
			FilterCategories.NumbersAndReferences,
		};

		protected override void ApplyDefaults(IEnumerable<ZString> skippedOrCategory = null)
		{
			// This is called on construction and also from LoadLayout

			if (OriginalCriteria == null)
			{
				base.ApplyDefaults();
				return;
			}

			if (!AreModuleFiltersLoaded)
			{
				LoadModuleFilters();
			}
		}

		#endregion

		#region Carriers

		public override string OrgSourceText => (NoResString)"Rate Selection Search Criteria"; // internal use only

		public override ZQuery GetAdditionalCarrierOrgListFilter(Type typeOfBusinessObjectToQuery, SchemaColumn fieldName)
		{
			var query = new ZDBOnlyQuery(typeOfBusinessObjectToQuery);

			// valid Service Providers to search in CW1
			var cw1SubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			cw1SubQuery.AddToFilter(ServiceProviderCollection.GetServiceProviderFilter());
			query.AddSubQuery(fieldName, cw1SubQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		#region Build RatesQuery

		public override (RatesQuery ratesQuery, bool isValidForRatesService) BuildRatesQuery(ILogger logger)
		{
			RunPreSaveValidation();
			if (HasErrors)
			{
				return (null, false);
			}

			var queryBuilder = new WiseRatesQueryBuilder(logger);
			var canupdateDate = CanUpdateDate;
			var isValidForRatesService = true;
			var carriers = Carriers.Concat(OriginalCriteria.JobServices.GetContractorsWithSource()).Distinct().ToList();

			var (ratesQuery, errors) = queryBuilder.Build(OriginalCriteria, carriers,
													contractNumbersFromFilters: canupdateDate ? ContractNumbersFromFilters : null,
													carriersFromFilters: canupdateDate ? Carriers : null);

			if (!string.IsNullOrEmpty(errors))
			{
				logger.Log(LogType.Warning, errors);
				isValidForRatesService = false;
			}

			ratesQuery.EffectiveDate = EffectiveDate.ToDateTime();
			ratesQuery.EndDate = EffectiveDate.ToDateTime();

			var zoneOwners = OriginalCriteria.ZoneOwnerOrganizations();
			var origin = LocationHelper.GetCachedLocationFromString(LocationFilter.Property1, Factory);
			var destination = LocationHelper.GetCachedLocationFromString(LocationFilter.Property2, Factory);
			ratesQuery.Origin = queryBuilder.GetLocations(origin, zoneOwners);
			ratesQuery.Destination = queryBuilder.GetLocations(destination, zoneOwners);

			var (containers, containerError) = GetContainerTypes();
			if (!string.IsNullOrWhiteSpace(containerError))
			{
				logger.Log(LogType.Error, containerError);
				return (null, false);
			}

			if (!containers.IsNullOrEmpty())
			{
				ratesQuery.Container = queryBuilder.GetContainers(containers, OriginalCriteria.FreightMode);
				if (!ratesQuery.Container.Any())
				{
					logger.Log(LogType.Error, "None of Job's containers can be used for rates search");
					isValidForRatesService = false;
				}
			}

			ratesQuery.Contract = GetContracts(ContractNumbersFromFilters, logger);
			ratesQuery.NamedAccount = GetNamedAccounts(NamedAccounts, logger);
			ratesQuery.ServiceLevel = CarrierServiceLevelsFromFilters;

			queryBuilder.AddMeasureChargeableVolume(ratesQuery, OriginalCriteria);

			return (ratesQuery, isValidForRatesService);
		}

		IEnumerable<RatesQueryContract> GetContracts(IEnumerable<string> contractNumbers, ILogger logger)
		{
			var contracts = new List<RatesQueryContract>();
			var csRefData = GetCargoSphereRefData();

			foreach (var contractNumber in contractNumbers)
			{
				if (csRefData == null)
				{
					contracts.Add(new RatesQueryContract { ContractNumber = contractNumber });
					continue;
				}

				var contractIDs = csRefData.Contracts
					.Where(x => string.Equals(x.ContractNumber, contractNumber, StringComparison.OrdinalIgnoreCase))
					.SelectMany(x => x.IDs)
					.ToArray();

				if (!contractIDs.Any())
				{
					contracts.Add(new RatesQueryContract { ContractNumber = contractNumber });
					continue;
				}

				contracts.AddRange(contractIDs.Select(id =>
					new RatesQueryContract
					{
						ContractNumber = contractNumber,
						CargoSphereID = id.ToString(DefaultCulture.Instance),
					}));
			}

			return contracts;
		}

		public IEnumerable<RatesQueryNamedAccount> GetNamedAccounts(IEnumerable<string> namedAccounts, ILogger logger)
		{
			var csRefData = GetCargoSphereRefData();

			return namedAccounts
				.Select(namedAccount =>
					new RatesQueryNamedAccount
					{
						Name = namedAccount,
						CargoSphereID = csRefData?.NamedAccounts?.FirstOrDefault(x => string.Equals(x.Name, namedAccount, StringComparison.OrdinalIgnoreCase))?.ID.ToString(DefaultCulture.Instance)
					})
				.ToArray();
		}

		#endregion Build RatesQuery

		public override void SetCargoSphereFilters(IEnumerable<RefCargoSphereContract> contracts, IEnumerable<RefCargoSphereNamedAccount> namedAccounts)
		{
			var key = $"{LocationFilter.Property1}-{LocationFilter.Property2}-{OriginalCriteria.IsContainerised}-{EffectiveDate.ToDateTime():yyyy-MM-dd}";

			var item = MemoryCache.Default.GetCacheItem(key)?.Value as CargoSphereRefData;
			if (item == null)
			{
				MemoryCache.Default.Set(
					key,
					new CargoSphereRefData(contracts, namedAccounts),
					new CacheItemPolicy { AbsoluteExpiration = ZDate.Today.AddMonths(1).ToDateTime() });

				return;
			}

			foreach (var contract in contracts)
			{
				if (item.Contracts.All(c => c.ContractNumber != contract.ContractNumber))
				{
					item.Contracts.Add(contract);
				}
			}

			foreach (var namedAccount in namedAccounts)
			{
				if (item.NamedAccounts.All(na => na.ID != namedAccount.ID && na.Name != namedAccount.Name))
				{
					item.NamedAccounts.Add(namedAccount);
				}
			}
		}

		CargoSphereRefData GetCargoSphereRefData()
		{
			if (EffectiveDate == ZDateTime.Empty)
			{
				return null;
			}
			var key = $"{LocationFilter.Property1}-{LocationFilter.Property2}-{OriginalCriteria.IsContainerised}-{EffectiveDate.ToDateTime():yyyy-MM-dd}";
			return MemoryCache.Default.GetCacheItem(key)?.Value as CargoSphereRefData;
		}

		public class CargoSphereRefData(
			IEnumerable<RefCargoSphereContract> contracts,
			IEnumerable<RefCargoSphereNamedAccount> namedAccounts)
		{
			public List<RefCargoSphereContract> Contracts { get; } = contracts.ToList();
			public List<RefCargoSphereNamedAccount> NamedAccounts { get; } = namedAccounts.ToList();
		}
	}
}
