using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business
{
	public sealed class IncotermsRanker
	{
		public IncotermsRanker() { }

		public IncotermsRanker Rank(FastLine fastLine) => Rank(fastLine.Criteria, fastLine.Line, fastLine.IsCostRate(), fastLine.IsCollect());

#if DEBUG
		internal IncotermsRanker Rank(RatingCriteria criteria, IRateLine rateLine) => Rank(criteria, rateLine, rateLine.IsCostRate(), null);
		internal
#endif
		IncotermsRanker Rank(RatingCriteria criteria, IRateLine rateLine, bool isCostRate, bool? isCollect)
		{
			OrgImportance = 0;
			OrgTypesSellRatePriorityRegistryFiltered = Enumerable.Empty<RatingDebtorOrgTypes>();
			OrgTypes = Enumerable.Empty<RatingDebtorOrgTypes>();
			ratesPrioritiesRegistryItem = null;

			if (criteria != null
				&& rateLine != null
				&& !isCostRate
				&& !RateLineHelper.IsRateCategoryUnknownForJobServiceSpotEntry(rateLine)//Rate Category is unknown in few cases for Spot Rates
				&& !IsGatewayBillingApplicable(criteria))
			{
				var orgTypes = criteria.GetDebtors()
					.Where(debtor => IsApplicableToOrg(rateLine, debtor.OrgHeader, criteria))
					.Select(debtor => debtor.RatingDebtorOrgTypes)
					.ToList();
				OrgTypes = orgTypes;

				if (criteria.RateTypeToUse == RateType.Forwarding ||
					criteria.RateTypeToUse == RateType.Customs ||
					criteria.RateTypeToUse == RateType.CFS)
				{
					if (orgTypes.Count > 0)
					{
						var orgTypesFiltered = new List<RatingDebtorOrgTypes>(orgTypes);
						OrgTypesSellRatePriorityRegistryFiltered = orgTypesFiltered;

						isCollect = isCollect ?? criteria.Cache.IsCollect(false, rateLine.ChargeCode.AC_ChargeGroup);
						if (isCollect.HasValue)
						{
							ratesPrioritiesRegistryItem = GetRatesPrioritiesRegistryItem(criteria, isCollect.Value);
							var priorities = ratesPrioritiesRegistryItem?.Value;
							var minImportance = NotApplicable;
							for (var i = orgTypes.Count - 1; i >= 0; i--)
							{
								var importance = GetOrgImportance(priorities, rateLine, orgTypes[i], criteria);
								if (minImportance > importance)
								{
									minImportance = importance;
								}
								if (importance == NotApplicable)
								{
									orgTypesFiltered.RemoveAt(i);
								}
							}

							OrgImportance = minImportance;
						}
						else
						{
							// If payment term is not established then charges are not filtered out. Currently I can see such behaviour is expected in Order for LandedCosting
							// However we'll need to review this
							OrgImportance = 0;
						}
					}
					else
					{
						OrgImportance = NotApplicable;
					}
				}
				else
				{
					OrgTypesSellRatePriorityRegistryFiltered = orgTypes.Where(x => x == RatingDebtorOrgTypes.LC || x == RatingDebtorOrgTypes.LCBK).ToList();
					OrgImportance = OrgTypesSellRatePriorityRegistryFiltered.Any()
						? 0
						: NotApplicable;
				}
			}

			// Handles the Company Tariff Level override. Typically found in shipment
			// and if set, it will apply above any organisation company tariff level ranking.
			if (rateLine?.ParentRateEntry?.IsCompanyTariff() ?? false)
			{
				var rateLineLevel = rateLine.ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel;
				if (rateLineLevel == criteria.TariffLevel)
				{
					OrgImportance = 0;
				}
			}

			return this;
		}

		/// <summary>
		/// A number which represents which org priority index in the registry got
		/// matched. Or IncotermsRanker.NotApplicable if none was found. This number
		/// is used later for ordering FastLines
		/// </summary>
		public int OrgImportance { get; private set; }

		public IEnumerable<RatingDebtorOrgTypes> OrgTypes { get; private set; } = Enumerable.Empty<RatingDebtorOrgTypes>();

		public IEnumerable<RatingDebtorOrgTypes> OrgTypesSellRatePriorityRegistryFiltered { get; private set; } = Enumerable.Empty<RatingDebtorOrgTypes>();

		public string GetRatesPrioritiesRegistryItemCaption() => ratesPrioritiesRegistryItem?.Caption;
		RatesPrioritiesRegistryItem ratesPrioritiesRegistryItem;

		static bool IsGatewayBillingApplicable(RatingCriteria criteria)
		{
			return (criteria.GatewayBillingSupporter?.IsGatewayBillingEnabled() ?? false)
				&& RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value;
		}

		static RatesPrioritiesRegistryItem GetRatesPrioritiesRegistryItem(RatingCriteria criteria, bool isCollect)
		{
			RatesPrioritiesRegistryItem ratesPrioritiesRegistryItem = null;

			var isGateway = criteria.GatewayBillingSupporter?.IsGatewayBillingEnabled() ?? false;
			if (isGateway)
			{
				ratesPrioritiesRegistryItem = isCollect ? RatingDataRegistry.Instance.GatewayCollectPriorities : RatingDataRegistry.Instance.GatewayPrepaidPriorities;
			}
			else
			{
				switch (criteria.JobDirection)
				{
					case Directions.Import:
						ratesPrioritiesRegistryItem = isCollect ? RatingDataRegistry.Instance.ImportCollectPriorities : RatingDataRegistry.Instance.ImportPrepaidPriorities;
						break;

					case Directions.Export:
						ratesPrioritiesRegistryItem = isCollect ? RatingDataRegistry.Instance.ExportCollectPriorities : RatingDataRegistry.Instance.ExportPrepaidPriorities;
						break;

					case Directions.Domestic:
						ratesPrioritiesRegistryItem = isCollect ? RatingDataRegistry.Instance.DomesticCollectPriorities : RatingDataRegistry.Instance.DomesticPrepaidPriorities;
						break;

					case Directions.CrossTrade:
						ratesPrioritiesRegistryItem = isCollect ? RatingDataRegistry.Instance.CrossTradeCollectPriorities : RatingDataRegistry.Instance.CrossTradePrepaidPriorities;
						break;
				}
			}

			return ratesPrioritiesRegistryItem;
		}

		static int GetOrgImportance(RatesPrioritiesCollection priorities, IRateLine rateLine, RatingDebtorOrgTypes orgType, RatingCriteria criteria)
		{
			if (priorities == null)
			{
				return NotApplicable;
			}

			if (criteria.RateTypeToUse == RateType.Customs)
			{
				if (!RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.Value)
				{
					return NotApplicable;
				}
			}

			var rateLineCanUsePriority = !rateLine.TL_FeeChargeType.IsEmpty || !rateLine.ParentRateEntry.IsCompanyTariff();

			// The priorities could contain a mix of ALL and CUS priorities.
			// here, we retrieve either only the ALL priorities or only the CUS priorities.
			// along with the index (also called Importance) of it on the original collection.
			var requiredRatingJobType = (criteria.RateTypeToUse == RateType.Customs ? RatingJobTypes.CUS : RatingJobTypes.ALL);
			var prioritiesMatchingJobType =
				priorities
					.Cast<RatesPriorities>()
					.Select((priority, importance) => (priority, importance))
					.Where(pair => pair.priority.RatingJobType == requiredRatingJobType);

			var prioritiesMatchingOrgType =
				prioritiesMatchingJobType
					.Where(pair => pair.priority.RatingOrganizationType == orgType)
					.Where(pair => rateLineCanUsePriority || pair.priority.UseCompanyTariff);

			if (prioritiesMatchingOrgType.Any())
			{
				return prioritiesMatchingOrgType.First().importance;
			}

			return NotApplicable;
		}

		public const int NotApplicable = short.MaxValue;

		#region Hacks for testing

		internal Func<IRateLine, OrgHeader, RatingCriteria, bool> IsApplicableToOrg
		{
			get;
#if DEBUG
			set;
#endif
		}
			= RateLineExtensions.IsApplicableToOrg;

		#endregion
	}
}
