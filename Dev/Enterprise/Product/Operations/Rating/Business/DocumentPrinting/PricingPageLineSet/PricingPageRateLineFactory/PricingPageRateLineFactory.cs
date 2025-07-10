using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Get grouped rateLines wrapped in PricingPageLineSet from entries and their related entries
	/// </summary>
	public partial class PricingPageRateLineFactory
	{
		public PricingPageRateLineFactory(EntryTypes entryType)
		{
			this.EntryType = entryType;
		}

		public EntryTypes EntryType { get; }

		public PricingPageRateLineListList LoadLineSets(PricingPage pricingPage, IEnumerable<RateEntry> rateEntries, bool exactMatch = false, bool checkEmptyContainer = false)
		{
			var lookup = LoadLineSets(new DefaultPricingPageRateLineListGroupStrategy(), pricingPage, rateEntries, exactMatch, checkEmptyContainer);
			if (lookup.TryGetValue(DefaultPricingPageRateLineListGroupStrategy.Key, out var lineSets))
			{
				return new PricingPageRateLineListList(lineSets, clones);
			}

			return new PricingPageRateLineListList();
		}

		public IDictionary<T, List<PricingPageRateLineList>> LoadLineSets<T>(IPricingPageRateLineListGroupStrategy<T> strategy, PricingPage pricingPage, IEnumerable<RateEntry> rateEntries, bool exactMatch = true, bool checkEmptyContainer = false)
			where T : IEquatable<T>
		{
			var result = new SortedDictionary<T, List<PricingPageRateLineList>>();

			foreach (var rateEntry in rateEntries)
			{
				var relatedRateEntries = GetRelatedEntries(pricingPage, rateEntry, EntryType, exactMatch);
				var rateLineListDict = GetGroupedRateLineList(strategy, relatedRateEntries);

				strategy.CrossPollinate(rateLineListDict);

				foreach (var rateLineListPair in rateLineListDict)
				{
					List<PricingPageRateLineList> pricingPageLineSetList;

					if (!result.TryGetValue(rateLineListPair.Key, out pricingPageLineSetList))
					{
						pricingPageLineSetList = new List<PricingPageRateLineList>();
						result.Add(rateLineListPair.Key, pricingPageLineSetList);
					}

					var princingPageLineSets = GetPricingPageLineSets
					(
						pricingPage,
						rateEntry,
						containerFilter: (refContainer) => strategy.IncludeContainer(rateLineListPair.Key, refContainer),
						rateLines: rateLineListPair.Value,
						checkEmptyContainer
					);
					pricingPageLineSetList.AddRange(princingPageLineSets);
				}
			}

			strategy.Purge(result);

			foreach (var pair in result.ToArray())
			{
				PricingPageRateLineList.RemoveDuplicateRateLineInSets(pair.Value);
				result[pair.Key] = Sort(pricingPage, pair.Value);
			}

			return result;
		}

		public IEnumerable<RateEntry> GetRelatedEntries(PricingPage pricingPage, IEnumerable<RateEntry> rateEntries, bool exactMatch)
		{
			foreach (var parentEntry in rateEntries)
			{
				foreach (var childEntry in GetRelatedEntries(pricingPage, parentEntry, EntryType, exactMatch))
				{
					yield return childEntry;
				}
			}
		}

		void AddRateLines(List<RateLine> resultRateLineList, RateEntry rateEntry)
		{
			var rateLines = rateEntry.RateLines;
			if (rateEntry.IsQuote())
			{
				rateLines.Load();
			}

			foreach (RateLine rateLine in rateLines)
			{
				var rateLineToUse = GetThisOrCTCClonedLine(rateLine);
				if (rateLineToUse != null && rateLineToUse.ChargeCode != null)
				{
					if (rateLineToUse.Equals(rateLine))
					{
						rateLineToUse.SendErrorReporterIfParentIsNull("PricingPageLineSetFactory.AddRateLines_EmptyParent");
					}

					if (rateLineToUse.RateCalculatorType == CalculatorType.CompanyTariffBased
						|| rateLineToUse.RateCalculatorType == CalculatorType.CostBased)
					{
						throw new DeveloperNotificationException(ZString.Format("Tariff {{{0}}}-{{{1}}}-{{{2}}} was received from {{{3}}}-{{{4}}}-{{{5}}} should not reach this place and should be replaced by base rate lines by this moment.",
							rateLineToUse.ChargeCode.AC_Code, rateLineToUse.TL_RateCalculator, rateLineToUse.Parent.Parent.TH_RateType,
							rateLine.ChargeCode.AC_Code, rateLine.TL_RateCalculator, rateLine.Parent.Parent.TH_RateType));
					}

					resultRateLineList.Add(rateLineToUse);
				}
			}
		}

		IDictionary<T, List<RateLine>> GetGroupedRateLineList<T>(IPricingPageRateLineListGroupStrategy<T> strategy, IEnumerable<RateEntry> rateEntries) where T : IEquatable<T>
		{
			var result = new SortedDictionary<T, List<RateLine>>();

			// To make deterministic result
			var orderedRateEntries = rateEntries.OrderBy(x => x.TI_LineOrder);

			foreach (var orderedRateEntry in orderedRateEntries)
			{
				List<RateLine> resultRateLineList;
				var key = strategy.GroupKey(orderedRateEntry);

				if (!result.TryGetValue(key, out resultRateLineList))
				{
					resultRateLineList = new List<RateLine>();
					result.Add(key, resultRateLineList);
				}

				AddRateLines(resultRateLineList, orderedRateEntry);
			}

			return result;
		}

		IEnumerable<PricingPageRateLineList> GetPricingPageLineSets(PricingPage pricingPage, RateEntry rateEntry, Predicate<RefContainer> containerFilter, List<RateLine> rateLines, bool checkEmptyContainer)
		{
			var rateLinesByContainerAndEquipment = SplitAndRemoveLinesByContainerAndEquipment(pricingPage, rateEntry, containerFilter, rateLines, checkEmptyContainer);
			var result = new Dictionary<RateLineLookupKey, PricingPageRateLineList>();

			foreach (var rateLine in rateLines)
			{
				if (rateLinesByContainerAndEquipment.All(pair => !pair.Value.Contains(rateLine)))
				{
					continue;
				}

				if (!IsVisibleOnPricingPage(rateLine, rateEntry, pricingPage.ViewAgentRates))
				{
					continue;
				}

				AddToDictionary(result, new RateLineLookupKey(rateEntry, rateLine), rateLine);
			}

			foreach (var pricingPageLineSet in result.Values)
			{
				foreach (var rateLine in pricingPageLineSet)
				{
					var pairs = rateLinesByContainerAndEquipment.Where(pair => pair.Value.Contains(rateLine));
					IEnumerable<ZString> containerCodes = pairs.Select(pair => pair.Key.ContainerCode).Distinct().OrderBy(code => code);

					foreach (var containerCode in containerCodes)
					{
						pricingPageLineSet.AddContainerType(rateLine, containerCode);
					}
				}
				pricingPageLineSet.ParentRateEntry = rateEntry;
			}

			return result.Values;
		}

		static IEnumerable<RateEntry> GetRelatedEntries(PricingPage pricingPage, RateEntry rateEntry, EntryTypes entryType, bool exactMatch)
		{
			var results = Enumerable.Empty<RateEntry>();
			switch (entryType)
			{
				case EntryTypes.Freight:
					results = pricingPage.GetRelatedRateEntries(rateEntry, exactMatch).FreightRateEntries;
					break;

				case EntryTypes.Origin:
					results = pricingPage.GetRelatedRateEntries(rateEntry, exactMatch).OriginRateEntries;
					break;

				case EntryTypes.Destination:
					results = pricingPage.GetRelatedRateEntries(rateEntry, exactMatch).DestinationRateEntries;
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(entryType), "Supported entry types are: Freight, Origin and Destination."); // Exception developer only message
			}

			return results.Where(entry => !entry.IsDeleted);
		}

		Dictionary<ContainerEquipmentKey, List<RateLine>> SplitAndRemoveLinesByContainerAndEquipment(PricingPage pricingPage, RateEntry parentRateEntry, Predicate<RefContainer> containerFilter, IEnumerable<RateLine> rateLines, bool checkEmptyContainer)
		{
			var result = new Dictionary<ContainerEquipmentKey, List<RateLine>>();
			var quotationContainerTypes = pricingPage == null ? new HashSet<RefContainer>() : pricingPage.ContainerSet;
			var hasContainers = quotationContainerTypes.Count > 0;

			foreach (var line in rateLines)
			{
				if (hasContainers)
				{
					foreach (var container in quotationContainerTypes)
					{
						if (containerFilter != null && !containerFilter(container))
						{
							continue;
						}

						var parent = line.Parent;

						if (parent != null && parent.Container == null &&
							(parent.TI_RateCategory == RatingConstants.RateCategory.WHS
							|| parent.TI_RateCategory == RatingConstants.RateCategory.TRN
							|| parent.TI_RateCategory == RatingConstants.RateCategory.TBC
							|| parent.TI_RateCategory == RatingConstants.RateCategory.PAC
							|| parent.TI_RateCategory == RatingConstants.RateCategory.UNP
							|| parent.TI_RateCategory == RatingConstants.RateCategory.CST))
						{
							//When rate entry is Warehouse or CFS or Port Transport
							AddToDictionary(result, new ContainerEquipmentKey(line), line);
						}
						else if (checkEmptyContainer && parent != null && (parent.IsFCL() || parent.IsULD() || parent.IsULDFreight()) && parent.Container == null)
						{
							AddToDictionary
							(
								result,
								new ContainerEquipmentKey(Res.GetString("B5313FD9-96D8-4721-B6AD-813313F8E8AC", "Empty"), line),
								line
							);
						}
						else if (parent == null
							|| parent.Container == null
							|| parent.TI_RC == container.PK
							|| (parent.TI_MatchContainerRateClass && container.GetContainersInSameClassForEntryType(EntryType).Contains(pricingPage.Factory.Load<RefContainer>(parent.TI_RC))))
						{
							AddToDictionary
							(
								result,
								new ContainerEquipmentKey(container.RC_Code, line),
								line
							);
						}
					}
				}
				else
				{
					AddToDictionary(result, new ContainerEquipmentKey(line), line);
				}
			}

			foreach (var pair in result)
			{
				new Remover(parentRateEntry, pair.Key.ContainerCode).Remove(pair.Value);
			}

			return result;
		}

		static List<PricingPageRateLineList> Sort(PricingPage pricingPage, List<PricingPageRateLineList> lineSetList)
		{
			var comparer = BasePricingPageRateLineListComparer.FromPage(pricingPage);

			if (comparer != null)
			{
				lineSetList = lineSetList.OrderBy(set => set, comparer).ToList();
			}

			return lineSetList;
		}

		static void AddToDictionary<TKey, TList, TValue>(IDictionary<TKey, TList> dictionary, TKey key, TValue line) where TList : IList<TValue>, new()
		{
			TList lines;

			if (!dictionary.TryGetValue(key, out lines))
			{
				dictionary[key] = lines = new TList();
			}

			lines.Add(line);
		}

		internal static bool IsVisibleOnPricingPage(RateLine rateLine, RateEntry parentRateEntry, bool overrideViewAgentRates)
		{
			var result = parentRateEntry != null;
			result = result && CanPrintChargeCode(rateLine, overrideViewAgentRates);
			result = result && IsChargePaidByClient(rateLine, parentRateEntry);

			return result;
		}

		internal static SchemaColumn[] DistinguishingColumnsForDocumentGrouping
			=> new SchemaColumn[]
			{
				RateEntrySchema.TI_OH_TransportProvider,
				RateEntrySchema.TI_RS_NKServiceLevel_NI,
				RateEntrySchema.TI_PL_NKCarrierServiceLevel,
				RateEntrySchema.TI_RH_NKCommodityCode,
				RateEntrySchema.TI_ParentID,
				RateEntrySchema.TI_TransitTime,
				RateEntrySchema.TI_OH_Consignee,
				RateEntrySchema.TI_OH_Consignor,
				RateEntrySchema.TI_OH_ControllingCustomer,
				RateEntrySchema.TI_OA_CartagePickupAddressOverride,
				RateEntrySchema.TI_OA_CartageDeliveryAddressOverride,
				RateEntrySchema.TI_CartageDeliveryAddressPostCode,
				RateEntrySchema.TI_CartagePickupAddressPostCode,
				RateEntrySchema.TI_TZ_OriginZone,
				RateEntrySchema.TI_TZ_DestinationZone,
				RateEntrySchema.TI_R9_FromSuburb,
				RateEntrySchema.TI_R9_ToSuburb,
			};

		static bool CanPrintChargeCode(RateLine rateLine, bool overrideViewAgentRates)
		{
			var chargeCode = rateLine?.ChargeCode;
			if (chargeCode != null && chargeCode.AC_ShowOnQuotation)
			{
				if (!chargeCode.AC_SuppressOnQuoteIfZero)
				{
					return true;
				}

				return rateLine.HasValueForDocumentPrinting(overrideViewAgentRates);
			}

			return false;
		}

		static bool IsChargePaidByClient(RateLine rateLine, RateEntry parentRateEntry)
		{
			// ORG/DST charge will be grouped with FRT charge when some of their properties are the same
			// In that case, use the Incoterm on FRT charge to check visibility of ORG/DST charge
			var frtCategories = rateLine.Parent.Criteria.GetRateCategories(RateCategoryGroup.Freight);
			var isFreightCharge = frtCategories.Contains(rateLine.Parent.TI_RateCategory.ToString());

			ZString incoterm = isFreightCharge ? rateLine.Parent.TI_QuotePageIncoTerm : parentRateEntry.TI_QuotePageIncoTerm;

			if (IncoTermRegistry.TryGetValue(incoterm, out IncoTerm incoTermObj))
			{
				if (parentRateEntry.IsImport() || parentRateEntry.IsExport())
				{
					var party = ChargedParty.None;
					var clientCountry = ((ILocation)parentRateEntry.Parent.Header?.ClosestPort)?.Country;
					if (clientCountry != null)
					{
						if (clientCountry.CompletelyCovers(parentRateEntry.Origin()))
						{
							party = parentRateEntry.IsImport() ? ChargedParty.Agent : ChargedParty.LocalClient;
						}
						else if (clientCountry.CompletelyCovers(parentRateEntry.Destination()))
						{
							party = parentRateEntry.IsImport() ? ChargedParty.LocalClient : ChargedParty.Agent;
						}
					}

					if (party != ChargedParty.None)
					{
						return incoTermObj.GetLocalClientOrAgent(parentRateEntry.JobDirection, rateLine.ChargeCode.AC_ChargeGroup) == party;
					}
				}
			}

			return true;
		}

		RateLine GetThisOrCTCClonedLine(RateLine rateLine)
		{
			if (rateLine != null)
			{
				if (!(rateLine.Calculator is CompanyTariffOrCostBasedCalculator))
				{
					return rateLine;
				}

				if (!clones.TryGetValue(rateLine.PK, out RateLine result))
				{
					var readonlyFactoryForClones = rateLine.Factory.GetCachedReadOnlyFactory();
					result = new CompanyTariffOrCostLineCloneHelper(rateLine, readonlyFactoryForClones).GetClone();
					if (result != null)
					{
						clones.Add(rateLine.PK, result);
					}
				}

				return result;
			}

			return null;
		}

		readonly Dictionary<ZGuid, RateLine> clones = new Dictionary<ZGuid, RateLine>();

		#region Nested Classes

		#endregion
	}
}
