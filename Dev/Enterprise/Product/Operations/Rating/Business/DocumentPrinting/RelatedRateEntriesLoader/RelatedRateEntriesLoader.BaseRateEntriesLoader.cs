using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public partial class RelatedRateEntriesLoader
	{
		abstract class BaseRateEntriesLoader
		{
			protected BaseRateEntriesLoader(PricingPage pricingPage, BusinessObjectFactory factory)
			{
				this.pricingPage = pricingPage;
				this.factory = factory;
			}

			readonly PricingPage pricingPage;
			readonly BusinessObjectFactory factory;

			public List<RateEntry> GetRelatedRateEntries(RateEntry parentRateEntry, bool exactMatch)
			{
				var parentHeader = parentRateEntry.Parent;
				if (!NeedToLoad(parentHeader))
				{
					return new List<RateEntry>();
				}

				var rateCategory = GetRateCategory(parentRateEntry);
				if (!RatingConstants.RateCategory.IsValidEntryCategory(rateCategory))
				{
					return new List<RateEntry>();
				}

				var rateMode = ConvertModeToAnotherCategory(parentRateEntry, rateCategory);

				var filter = new ZQuery();
				filter.AddToFilter(RatingHelper.GetOriginDestinationModeExclusiveFilter(parentRateEntry.FreightMode()));
				filter.AddToFilter(BuildDateFilter(parentRateEntry));
				filter.AddToFilter(BuildLocationFilter(parentRateEntry, rateCategory));
				filter.AddToFilter(BuildContainerFilter());
				filter.AddToFilter(BuildServiceProviderFilter(parentRateEntry));
				filter.AddToFilter(parentHeader.EntryCollections[rateCategory].LazyLoadingCollection.GetLoadFilter());

				var entryHeaders = GetHeaders(parentRateEntry, parentHeader, rateCategory, rateMode).ToArray();
				filter.AddToFilter(RateEntrySchema.TI_TH, entryHeaders.Select(x => x.PK));

				var distinguishingColumns = PricingPageRateLineFactory.DistinguishingColumnsForDocumentGrouping;
				foreach (var distinguishingColumn in distinguishingColumns)
				{
					filter.AddToFilter(BuildColumnFilter(distinguishingColumn, parentRateEntry));
				}

				var entries = factory.Load<RateEntry>(filter).ToList();

				var filteredEntries = exactMatch
					? FilterRateEntries(parentRateEntry, entries)
					: entries;

				SetTariffParent(filteredEntries, parentHeader, entryHeaders);

				return filteredEntries.OrderBy(rateEntry => rateEntry.TI_LineOrder).ToList();
			}

			static SchemaColumn[] DistinguishingColumnsForDocumentGroupingExactMatch
				=> new SchemaColumn[]
				{
					RateEntrySchema.TI_PlannedDischargeLRC,
					RateEntrySchema.TI_PlannedLoadLRC,
					RateEntrySchema.TI_RateDestination,
					RateEntrySchema.TI_RateOrigin,
					RateEntrySchema.TI_MatchContainerRateClass,
					RateEntrySchema.TI_Frequency,
					RateEntrySchema.TI_FrequencyUnit,
					RateEntrySchema.TI_ContractNumber,
					RateEntrySchema.TI_PaymentTerm,
				};

			static IEnumerable<RateEntry> FilterRateEntries(RateEntry existingRateEntry, IEnumerable<RateEntry> rateEntriesToFilter)
			{
				foreach (var rateEntryToFilter in rateEntriesToFilter)
				{
					var allMatched = true;

					var distinguishingColumns = new List<SchemaColumn>(PricingPageRateLineFactory.DistinguishingColumnsForDocumentGrouping);
					distinguishingColumns.AddRange(DistinguishingColumnsForDocumentGroupingExactMatch);

					foreach (var distinguishingColumn in distinguishingColumns)
					{
						if (!existingRateEntry[distinguishingColumn].Equals(rateEntryToFilter[distinguishingColumn]))
						{
							allMatched = false;
							break;
						}
					}
					if (allMatched)
					{
						yield return rateEntryToFilter;
					}
				}
			}

			// RateMode=SEA means 2 things (based on RateCategory):
			//	* RateCategory = FCL means FCL
			//	* RateCategory = ORG/DST means FCL and LCL
			static string ConvertModeToAnotherCategory(RateEntry rateEntry, string newRateCategory)
			{
				var rateMode = rateEntry.TI_Mode;
				if (rateMode == Core.Constants.RateMode.SEA
					&& rateEntry.TI_RateCategory == RatingConstants.RateCategory.FCL
					&& (newRateCategory == RatingConstants.RateCategory.ORG || newRateCategory == RatingConstants.RateCategory.DST))
				{
					rateMode = Core.Constants.RateMode.FCL;
				}

				return rateMode;
			}

			void SetTariffParent(IEnumerable<RateEntry> rateEntries, RatingHeader parentRatingHeader, IEnumerable<RatingHeader> entryHeaders)
			{
				if (parentRatingHeader.IsTariff())
				{
					rateEntries.ForEach(r => r.Parent = parentRatingHeader);
				}
				else
				{
					var tariffLevel1Header = entryHeaders.SingleOrDefault(x => x.IsLevelOneTariff() && x.IsGlobal() == parentRatingHeader.IsGlobal());
					var higherTariffLevelHeader = entryHeaders.SingleOrDefault(x => x.IsAdditionalTariff() && x.IsGlobal() == parentRatingHeader.IsGlobal());

					if (tariffLevel1Header != null && higherTariffLevelHeader != null)
					{
						foreach (var entry in rateEntries.Where(x => x.TI_TH == tariffLevel1Header.PK))
						{
							entry.Parent = higherTariffLevelHeader;
						}
					}
				}
			}

			#region Get Related Rating Headers

			IEnumerable<RatingHeader> GetHeaders(RateEntry parentRateEntry, RatingHeader parentRatingHeader, string rateCategory, string rateMode)
			{
				var headers = new List<RatingHeader>();
				headers.Add(parentRatingHeader);

				if (NeedToLoadInheritedRates(parentRatingHeader))
				{
					var isAdditionalTariff = parentRatingHeader.IsAdditionalTariff();
					var company = parentRatingHeader.Company;
					var isPublishedGlobalEntry = company != null
						&& !isAdditionalTariff
						&& parentRateEntry.TI_TH != parentRatingHeader.PK
						&& parentRateEntry.TI_TH == parentRatingHeader.GlobalRatingHeader?.PK;

					if (isPublishedGlobalEntry)
					{
						headers.Add(parentRatingHeader.GlobalRatingHeader);
					}

					if (isAdditionalTariff)
					{
						headers.AddRange(LoadCompanyTariffs(parentRatingHeader.TH_GlobalRateLevel, company));
					}

					if (parentRatingHeader.IsQuote() || parentRatingHeader.IsClientRate())
					{
						var direction = OrgRateTariffLevel.GetOrgRateTariffLevelDirection(parentRateEntry.JobDirection);

						var companyTariffLevel = CompanyTariff.GetLevel(parentRatingHeader.Header, parentRatingHeader.Company, rateCategory, rateMode, direction, parentRateEntry.TI_RateStartDate, parentRateEntry.TI_RateEndDate);
						if (companyTariffLevel > 0)
						{
							headers.AddRange(LoadCompanyTariffs((byte)companyTariffLevel, company));
						}
					}

					if (parentRatingHeader.IsQuote() && parentRatingHeader.Header != null)
					{
						var filter = new ZQuery();
						filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.ClientRate);
						filter.AddToFilter(GetCompanyQuery(company));
						filter.AddToFilter(RatingHeaderSchema.TH_OH, parentRatingHeader.Header.PK);

						var rates = factory.Load<ClientRate>(filter);
						if (rates != null)
						{
							headers.AddRange(rates);
						}
					}
				}

				return headers;
			}

			CompanyTariff[] LoadCompanyTariffs(byte level, GlbCompany company)
			{
				var filter = new ZQuery();
				filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Tariff);
				filter.AddToFilter(GetCompanyQuery(company));

				if (level > 1)
				{
					filter.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, new[] { (byte)1, level });
				}
				else
				{
					filter.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, (byte)1);
				}

				var tariffs = factory.Load<CompanyTariff>(filter);
				return tariffs;
			}

			static ZQuery GetCompanyQuery(GlbCompany company)
			{
				var companyFilter = new ZQuery();
				companyFilter.AddToFilter(RatingHeaderSchema.TH_GC, SQLComparisonOperator.Equal, null);

				if (company != null)
				{
					companyFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GC, SQLComparisonOperator.Equal, company.PK);
				}

				return companyFilter;
			}

			#endregion Get Related Rating Headers

			protected abstract bool NeedToLoad(RatingHeader header);
			protected abstract bool NeedToLoadInheritedRates(RatingHeader header);
			protected abstract bool EmptyColumnFilterMatch(RateEntry entry);
			protected abstract ZString GetRateCategory(RateEntry entry);
			protected abstract RefContainerCollection GetContainersInSameClass(RefContainer container);

			#region Get Location Filters

			ZQuery BuildLocationFilter(RateEntry rateEntry, string rateCategory)
			{
				var doesNotUseReferenceLocations = rateCategory == RatingConstants.RateCategory.WHS || rateCategory == RatingConstants.RateCategory.CYD;
				if (doesNotUseReferenceLocations)
				{
					return new ZQuery();
				}

				var locationQuery = new ZQuery();
				locationQuery.AddToFilter(BuildOriginFilter(rateEntry));
				locationQuery.AddToFilter(BuildDestinationFilter(rateEntry));
				locationQuery.AddToFilter(BuildCrossTradeFilter(rateEntry), rateEntry.IsCrossTrade() ? JoinCondition.Or : JoinCondition.And);
				locationQuery.AddToFilter(BuildViaFilter(rateEntry));

				return locationQuery;
			}

			protected virtual ZQuery BuildOriginFilter(RateEntry rateEntry)
				=> rateEntry.Parent != null && rateEntry.Parent.IsQuote() && rateEntry.TI_OriginLRC.Length == 4
					? BuildComplexLocationFilter(RateEntrySchema.TI_OriginLRC, rateEntry, true)
					: BuildSimpleLocationFilter(RateEntrySchema.TI_OriginLRC, rateEntry);

			protected virtual ZQuery BuildDestinationFilter(RateEntry rateEntry)
				=> rateEntry.Parent != null && rateEntry.Parent.IsQuote() && rateEntry.TI_DestinationLRC.Length == 4
					? BuildComplexLocationFilter(RateEntrySchema.TI_DestinationLRC, rateEntry, true)
					: BuildSimpleLocationFilter(RateEntrySchema.TI_DestinationLRC, rateEntry);

			static ZQuery BuildCrossTradeFilter(RateEntry rateEntry)
				=> new ZQuery(RateEntrySchema.TI_IsCrossTrade, rateEntry.IsCrossTrade());

			static ZQuery BuildViaFilter(RateEntry rateEntry) =>  GetFilterWithEmptyMatch(RateEntrySchema.TI_ViaLRC, rateEntry);

			protected ZQuery BuildComplexLocationFilter(SchemaColumn column, RateEntry rateEntry, bool allowEmpty)
			{
				var location = GetLocation((ZString)rateEntry[column]);
				var possibleLocations = new HashSet<ZString>(GetPossibleLocationCodes(location, rateEntry.TI_OH_TransportProvider));
				if (possibleLocations.Any())
				{
					var rateCategory = GetRateCategory(rateEntry);
					var otherEntryLocations = GetOtherEntryLocations(rateEntry.Parent, rateCategory, column);
					foreach (var otherEntryLocation in otherEntryLocations)
					{
						if (otherEntryLocation != null)
						{
							var code = otherEntryLocation.Code;
							var canAdd = !location.Code.EqualsIgnoringCase(code)
								&& !possibleLocations.Contains(code)
								&& location.CompletelyCovers(otherEntryLocation);

							if (canAdd)
							{
								possibleLocations.Add(code);
							}
						}
					}
				}

				if (allowEmpty)
				{
					possibleLocations.Add(ZString.Empty);
				}

				return new ZQuery(column, possibleLocations);
			}

			protected ZQuery BuildSimpleLocationFilter(SchemaColumn column, RateEntry rateEntry)
			{
				var location = GetLocation((ZString)rateEntry[column]);
				var possibleLocations = new List<ZString>(GetPossibleLocationCodes(location, rateEntry.TI_OH_TransportProvider));
				possibleLocations.Add(ZString.Empty);

				return new ZQuery(column, possibleLocations);
			}

			#region Factory Cached Location Dictionaries

			ILocation GetLocation(ZString locationCode)
			{
				var cache = factory.GetCachedValue("RelatedEntriesLoader.Location", () => new Dictionary<ZString, ILocation>());
				return cache.GetOrAdd(locationCode, () => LocationHelper.GetLocationFromString(locationCode, factory));
			}

			ZString[] GetPossibleLocationCodes(ILocation location, ZGuid carrier)
			{
				var key = location == null ? "null" : location.Code + carrier;
				var cache = factory.GetCachedValue("RelatedEntriesLoader.PossibleLocationCodes", () => new Dictionary<string, ZString[]>());
				ZString[] BuildPossibleLocationCodes()
				{
					var result = new HashSet<ZString>();
					if (location != null)
					{
						result.Add(location.Code);

						foreach (var zone in GetLocationZones(location))
						{
							if (zone.FZ_OH_RelatedParty.IsEmpty || zone.FZ_OH_RelatedParty == carrier)
							{
								result.Add(zone.Code);
							}
						}

						var country = location.Country;
						if (country != null && !country.RN_Code.IsEmpty)
						{
							result.Add(country.RN_Code);
						}
					}

					return result.ToArray();
				}

				return cache.GetOrAdd(key, BuildPossibleLocationCodes);
			}

			RefZoneHeader[] GetLocationZones(ILocation location)
			{
				var cache = factory.GetCachedValue("RelatedEntriesLoader.LocationZones", () => new Dictionary<ILocation, RefZoneHeader[]>());
				return cache.GetOrAdd(location, () => location.Zones);
			}

			ILocation[] GetOtherEntryLocations(RatingHeader parentRatingHeader, ZString rateCategory, SchemaColumn column)
			{
				var key = string.Join(".", parentRatingHeader.PK, rateCategory, column.Name);
				var cache = factory.GetCachedValue("RelatedEntriesLoader.OtherEntryLocations", () => new Dictionary<string, ILocation[]>());

				ILocation[] BuildOtherEntryLocations()
				{
					var locationCodes = parentRatingHeader.EntryCollections[rateCategory]
						.LazyLoadingCollection
						.Select(entry => (ZString)entry[column])
						.Where(x => !x.IsEmpty)
						.Distinct();

					return locationCodes.Select(code => GetLocation(code)).ToArray();
				}

				return cache.GetOrAdd(key, BuildOtherEntryLocations);
			}

			#endregion Factory Cached Location Dictionaries

			#endregion Get Location Filters

			ZQuery BuildColumnFilter(SchemaColumn column, RateEntry rateEntry)
				=> EmptyColumnFilterMatch(rateEntry)
				? GetFilterWithEmptyMatch(column, rateEntry)
				: GetFilterWithEmptyMatchIfHasValue(column, rateEntry);

			protected virtual ZQuery BuildServiceProviderFilter(RateEntry rateEntry)
			{
				var serviceProviderQuery = new ZQuery(RateEntrySchema.TI_OH_Supplier, rateEntry.TI_OH_Supplier);
				serviceProviderQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OH_Supplier, null);
				return serviceProviderQuery;
			}

			static ZQuery GetFilterWithEmptyMatchIfHasValue(SchemaColumn column, RateEntry rateEntry)
			{
				var value = (IZType)rateEntry[column];
				var result = new ZQuery();
				result.DefaultJoinCondition = JoinCondition.Or;

				if (!value.IsEmpty)
				{
					result.AddToFilter(column, column.IsNullable ? null : value.Default);
					result.AddToFilter(column, value);
				}

				return result;
			}

			static ZQuery GetFilterWithEmptyMatch(SchemaColumn column, RateEntry rateEntry)
			{
				var value = (IZType)rateEntry[column];
				var result = new ZQuery(column, column.IsNullable ? null : value.Default);

				if (!value.IsEmpty)
				{
					result.AddToFilter(JoinCondition.Or, column, value);
				}

				return result;
			}

			ZQuery BuildContainerFilter()
			{
				var filter = new ZQuery(RateEntrySchema.TI_RC, SQLComparisonOperator.Equal, null);
				var containerTypes = pricingPage.ContainerSet;

				if (containerTypes.Any())
				{
					var containerPKs = new HashSet<ZGuid>();
					var sameClassContainerPKs = new HashSet<ZGuid>();
					foreach (var container in containerTypes)
					{
						containerPKs.Add(container.PK);
						foreach (var rateClassContainer in GetContainersInSameClass(container))
						{
							sameClassContainerPKs.Add(rateClassContainer.PK);
						}
					}

					filter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RC, SQLComparisonOperator.Equal, containerPKs);
					var sameClassContainers = sameClassContainerPKs.Except(containerPKs);
					if (sameClassContainers.Any())
					{
						var sameClassContainer = new ZQuery(RateEntrySchema.TI_MatchContainerRateClass, ZBool.True);
						sameClassContainer.AddToFilter(RateEntrySchema.TI_RC, sameClassContainers);
						filter.AddToFilter(sameClassContainer, JoinCondition.Or);
					}
				}

				return filter;
			}

			static ZQuery BuildDateFilter(RateEntry rateEntry)
			{
				ZDateTime startDate;
				ZDateTime endDate;

				if (rateEntry.IsQuote() && rateEntry.Parent != null)
				{
					startDate = rateEntry.Parent.TH_QuoteDate;
					endDate = rateEntry.Parent.TH_QuoteEndDate;
				}
				else
				{
					startDate = rateEntry.TI_RateStartDate;
					endDate = rateEntry.TI_RateEndDate;
				}

				var filter = new ZQuery();

				if (!endDate.IsEmpty)
				{
					var startDateSubFilter = new ZQuery();
					startDateSubFilter.DefaultJoinCondition = JoinCondition.Or;
					startDateSubFilter.AddToFilter(RateEntrySchema.TI_RateStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, endDate);
					filter.AddToFilter(startDateSubFilter, JoinCondition.And);
				}

				if (!startDate.IsEmpty)
				{
					var endDateSubFilter = new ZQuery();
					endDateSubFilter.DefaultJoinCondition = JoinCondition.Or;
					endDateSubFilter.AddToFilter(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
					endDateSubFilter.AddToFilter(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.Equal, null);
					filter.AddToFilter(endDateSubFilter, JoinCondition.And);
				}

				return filter;
			}
		}
	}
}
