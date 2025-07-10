using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Rating.Business
{
	public class RatingCriteria : AutoRatingProxy
	{
		protected RatingCriteria(BusinessObjectFactory factory, bool isProductionCriteria = true)
			: this(null, factory, isProductionCriteria)
		{
		}

		public RatingCriteria(IAutoRating consumer, BusinessObjectFactory factory, bool isProductionCriteria = true)
			: base(consumer)
		{
			this.factory = factory;
			CheckRateAndJobType(consumer, isProductionCriteria);

			ID = Guid.NewGuid();
		}

		public readonly Guid ID;

		public RatingCriteria CreateCopy(bool useOriginalProxy = false)
		{
			var autorating = GetRatingAdapter(useOriginalProxy);
			var criteria = autorating != null
				? new RatingCriteria(autorating, Factory)
				: new RatingCriteria(this, Factory);

			return criteria;
		}

		public virtual IAutoRating GetRatingAdapter(bool useOriginalProxy = false)
		{
			IAutoRating autorating = AutoRating;

			if (useOriginalProxy)
			{
				return autorating;
			}

			while (autorating is AutoRatingProxy proxy)
			{
				autorating = proxy.AutoRating;
			}

			return autorating;
		}

		void CheckRateAndJobType(IAutoRating consumer, bool isProductionCriteria)
		{
			if (isProductionCriteria && consumer?.ConsumerType != null && !RatingConstants.JobTypesByRateType[consumer.RateTypeToUse].Contains(consumer.ConsumerType))
			{
				ErrorReporter.ReportOnce(nameof(RatingCriteria) + nameof(CheckRateAndJobType), Invariant($"Incorrect RateType '{consumer.RateTypeToUse}' - ConsumerType '{consumer.ConsumerType?.Code}' combination on {consumer.AdapterType}"));
			}
		}

		#region Locations

		internal OrgRateTariffLevel.Directions Direction => OrgRateTariffLevel.GetOrgRateTariffLevelDirection(JobDirection);

		#region SuppressResourceStringsCheckRegion

		public int GetFreightLeg(IRateEntry entry)
		{
			return Factory.GetCachedValue(GetFreightLegCacheKey(entry), () =>
			{
				if (entry.IsFreightEntry())
				{
					var via = GetVia(entry.IsCostRate() ? CostSell.Cost : CostSell.Revenue);
					if (via == null || (entry.Origin() == null || entry.Origin().CompletelyCovers(Origin) || entry.Origin().CompletelyCovers(RateOrigin)
						&& (entry.Destination() == null || entry.Destination().CompletelyCovers(via) || entry.Destination().CompletelyCovers(Destination) || entry.Destination().CompletelyCovers(RateDestination))))
					{
						return 1;
					}

					if ((entry.Origin() == null || entry.Origin().CompletelyCovers(via)) && (entry.Destination() == null || entry.Destination().CompletelyCovers(Destination)))
					{
						return 2;
					}

					return 1;
				}

				return 0;
			});
		}

		string GetFreightLegCacheKey(IRateEntry entry)
		{
			if (entry == null)
			{
				return ZString.Empty;
			}

			return string.Format(
				CultureInfo.InvariantCulture,
				"{0}|{1}|{2}|{3}|{4}|{5}",
				entry.TI_RateCategory,
				entry.TI_OriginLRC,
				OriginCode,
				entry.TI_DestinationLRC,
				DestinationCode,
				GetViaCode(entry.IsCostRate() ? CostSell.Cost : CostSell.Revenue)
			);
		}

		#endregion

		bool InSameCountry(ILocation legLocation, ILocation criteriaPort)
		{
			return IsCoveredByLeg(legLocation, criteriaPort);
		}

		bool AllInSameCountry(ILocation legOrigin, ILocation legDestination, ILocation criteriaPort)
		{
			var coveredByOrigin = IsCoveredByLeg(legOrigin, criteriaPort);
			var coveredByDestination = IsCoveredByLeg(legDestination, criteriaPort);

			return coveredByOrigin && coveredByDestination;
		}

		bool IsCoveredByLeg(ILocation leg, ILocation criteriaPort)
		{
			return leg == null || (leg.Country != null ? leg.Country.CompletelyCovers(criteriaPort) : leg.CompletelyCovers(criteriaPort));
		}

		#region Location Codes

		public ZString OriginCode
		{
			get { return Origin?.Code ?? ZString.Empty; }
		}

		public ZString DestinationCode
		{
			get { return Destination?.Code ?? ZString.Empty; }
		}

		public ZString GetViaCode(CostSell costSell) => GetVia(costSell)?.Code ?? ZString.Empty;

		public ZString GetFirstLoadCode(CostSell costSell) => GetFirstLoad(costSell)?.Code ?? ZString.Empty;

		public ZString GetLastDischargeCode(CostSell costSell) => GetLastDischarge(costSell)?.Code ?? ZString.Empty;

		public ZString GetFirstRouteSetLoadCode(CostSell costSell) => GetFirstRouteSetLoad(costSell)?.Code ?? ZString.Empty;

		public ZString GetLastRouteSetDischargeCode(CostSell costSell) => GetLastRouteSetDischarge(costSell)?.Code ?? ZString.Empty;

		public ZString PlannedLoadCode(CostSell costSell)
		{
			return PlannedLoad(costSell)?.Code ?? ZString.Empty;
		}

		public ZString PlannedDischargeCode(CostSell costSell)
		{
			return PlannedDischarge(costSell)?.Code ?? ZString.Empty;
		}

		public ZString RateOriginCode
		{
			get { return RateOrigin?.Code ?? ZString.Empty; }
		}

		public ZString RateDestinationCode
		{
			get { return RateDestination?.Code ?? ZString.Empty; }
		}

		public ZString DefaultFilterValueForOriginCode
		{
			get { return DefaultFilterValueForOrigin?.Code ?? ZString.Empty; }
		}

		public ZString DefaultFilterValueForDestinationCode
		{
			get { return DefaultFilterValueForDestination?.Code ?? ZString.Empty; }
		}

		#endregion

		#region Transport Leg

		public TransportLeg GetLeg(IRateEntry entry)
		{
			return new TransportLeg(this, entry);
		}

		public class TransportLeg
		{
			public TransportLeg(RatingCriteria criteria, IRateEntry entry)
			{
				Origin = criteria.Origin;
				Destination = criteria.Destination;

				var via = criteria.GetVia(entry.IsCostRate() ? CostSell.Cost : CostSell.Revenue);
				if (via == null)
				{
					return;
				}

				var entryOrigin = entry.Origin();
				var entryDestination = entry.Destination();

				if ((entryOrigin == null || entryOrigin.CompletelyCovers(via)) && (entryDestination == null || entryDestination.CompletelyCovers(criteria.Destination)))
				{
					Origin = via;
				}
				else if ((entryOrigin == null || entryOrigin.CompletelyCovers(criteria.Origin)) && (entryDestination == null || entryDestination.CompletelyCovers(via)))
				{
					Destination = via;
				}
			}

			public ILocation Origin { get; }
			public ILocation Destination { get; }

			public override string ToString()
			{
				return ZString.Format("{0}-{1}", Origin?.Code, Destination?.Code);
			}
		}

		#endregion

		#endregion

		#region Freight Info

		#region IncoTermObj and PaymentTerm

		public string GetDirectionPaymentTermString(CostSell costOrSell, ZString chargeGroup)
		{
			var term = GetPaymentTermString(costOrSell, chargeGroup);
			return (JobDirection + " " + term).Trim();
		}

		public string GetPaymentTermString(CostSell costOrSell, ZString chargeGroup)
			=> PaymentTerm.GetPrepaidCollect(costOrSell, chargeGroup);

		public ChargedParty ChargesPaidBy(ILocation legOrigin, ILocation legDestination, string chargeCodeGroup, CostSell costOrSell)
		{
			if (PaymentTerm.IsEmpty())
			{
				return ChargedParty.Unknown;
			}

			var defaultingParam = Job?.GetParamForCrossTradeDebtorDefaulting();

			if (JobDirection == Directions.CrossTrade && defaultingParam != null && JobHeader.IsCrossTradeDebtorDefaultingFunctionalityEnabled()
				&& (defaultingParam.JobType.Code == JobInvoicingConsumerTypes.ShipmentCode || defaultingParam.JobType.Code == JobInvoicingConsumerTypes.QuotedBookingCode))
			{
				var crossTradeParty = PaymentTerm.GetChargedPartyForCrossTradeJob(chargeCodeGroup, costOrSell, defaultingParam);
				var supporter = (JobInvoicingSupporter)InvoicingSupporter;

				switch (crossTradeParty)
				{
					case ChargedPartyForCrossTradeJob.Unknown:
						return ChargedParty.None;
					case ChargedPartyForCrossTradeJob.Agent:
						return ChargedParty.Agent;
					case ChargedPartyForCrossTradeJob.LocalClient:
						return ChargedParty.LocalClient;
					case ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToLocalClient:
						var ccBill = supporter.GetRelatedPartyFromControllingCustomer(defaultingParam.JobType, false);
						return ccBill != null ? ChargedParty.ControllingCustomerFallingBackToLocalClient : ChargedParty.LocalClient;
					case ChargedPartyForCrossTradeJob.ControllingCustomerFallingBackToAgent:
						ccBill = supporter.GetRelatedPartyFromControllingCustomer(defaultingParam.JobType, true);
						return ccBill != null ? ChargedParty.ControllingCustomerFallingBackToAgent : ChargedParty.Agent;
					default:
						return ChargedParty.None;
				}
			}

			var exportImport = PaymentTermInfos.GetExportImport(costOrSell, JobDirection, LocalClient, OverseasAgent, Consignor, Consignee);
			var chargedParty = PaymentTerm.GetChargedParty(chargeCodeGroup, JobDirection, exportImport, costOrSell);

			if (chargedParty.HasFlag(ChargedParty.Agent))
			{
				if (exportImport == Directions.Unknown || JobDirection != Directions.Domestic && IsForeignLegService(legOrigin, legDestination, chargeCodeGroup, exportImport, costOrSell))
				{
					return ChargedParty.None;
				}

				var isOverseasAgentApplicable = ConsumerType == null || ConsumerType.OverseasAgentApplicable;
				if (!isOverseasAgentApplicable)
				{
					chargedParty &= ~ChargedParty.Agent;
				}
			}

			return chargedParty;
		}

		bool IsForeignLegService(ILocation legOrigin, ILocation legDestination, string chargeCodeGroup, Directions exportImport, CostSell costOrSell)
		{
			bool result;

			var isImport = exportImport == Directions.Import;

			var localPort = isImport ? Destination : Origin;
			var via = GetVia(costOrSell);

			switch (chargeCodeGroup)
			{
				case ChargeCodeGroupList.Codes.Freight:
				case ChargeCodeGroupList.Codes.Insurance:
					result = via == null || !AllInSameCountry(legOrigin, legDestination, localPort) || !InSameCountry(via, localPort);
					break;

				case ChargeCodeGroupList.Codes.Origin:
				case ChargeCodeGroupList.Codes.OriginBrokerage:
				case ChargeCodeGroupList.Codes.OriginBrokerageOnly:
				case ChargeCodeGroupList.Codes.Loading:
					result = via == null ? isImport : (legOrigin == null || !InSameCountry(legOrigin, localPort));
					break;

				case ChargeCodeGroupList.Codes.Unloading:
				case ChargeCodeGroupList.Codes.Destination:
				case ChargeCodeGroupList.Codes.CustomsDuty:
				case ChargeCodeGroupList.Codes.Brokerage:
				case ChargeCodeGroupList.Codes.BrokerageOnly:
					result = via == null ? !isImport : (legDestination == null || !InSameCountry(legDestination, localPort));
					break;

				default:
					result = true;
					break;
			}

			return result;
		}

		#endregion

		public bool IsSeaFreight
		{
			get { return (FreightMode & FreightMode.SEA) != 0; }
		}

		public bool IsBCNFreight
		{
			get { return (FreightMode & FreightMode.BCN) != 0; }
		}

		public bool IsSCNFreight
		{
			get { return (FreightMode & FreightMode.SCN) != 0; }
		}

		public bool IsAirFreight
		{
			get { return (FreightMode & FreightMode.AIR) != 0; }
		}

		public bool IsRailFreight
		{
			get { return (FreightMode & FreightMode.RAI) != 0; }
		}

		public bool IsRoadFreight
		{
			get { return (FreightMode & FreightMode.ROA) != 0; }
		}

		public bool IsContainerised
		{
			get { return (FreightMode & FreightMode.Containerised) != 0; }
		}

		public bool IsFullLoad
		{
			get { return (FreightMode & FreightMode.FullLoad) != 0; }
		}

		public Money GetApplicableValue(ILogger autoratingLog, IRateLineItem applyToItem)
		{
			if (applyToItem != null)
			{
				switch (applyToItem.TM_Text.ToUpper())
				{
					case CalculatorConstants.Text.ApplyTo.Value.ValueOfGoods:
						return GetValue(applyToItem.ParentRateLine.Currency, MoneyType.ValueType.GoodsValue, CalculatorConstants.Text.ApplyTo.Value.Descriptions.ValueOfGoods);

					case CalculatorConstants.Text.ApplyTo.Value.InsuranceValue:
						return GetValue(applyToItem.ParentRateLine.Currency, MoneyType.ValueType.InsuranceValue, CalculatorConstants.Text.ApplyTo.Value.Descriptions.InsuranceValue);

					case CalculatorConstants.Text.ApplyTo.Value.CustomsValue:
						return CustomsValue(autoratingLog, applyToItem.ParentRateLine);

					case CalculatorConstants.Text.ApplyTo.Value.InvoiceValue:
						return InvoiceValue(applyToItem.ParentRateLine);

					case CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount:
						var localCurrency = applyToItem.ParentRateLine.ParentRateEntry?.Company()?.LocalCurrency ?? GlbCompany.CurrentCompany.LocalCurrency;
						return GetValue(localCurrency, MoneyType.ValueType.SingleTransactionBondAmount, CalculatorConstants.Text.ApplyTo.Value.Descriptions.BondAmount);

					default:
						throw new AutoRater.InvalidApplyToException(applyToItem);
				}
			}

			return Money.Invalid;
		}

		public Money GetValue(RefCurrency currency, MoneyType.ValueType valueType, ZString source)
		{
			return MonetaryValues.GetMoney(valueType, currency, CurrencyConverter as CurrencyConverter, source);
		}

		public void SetOverrideMeasures(RateableMeasureSet measures)
		{
			base.RateableMeasures = measures;
			jobMeasures = null;
		}

		public new RateableMeasureSet RateableMeasures
		{
			get => (RateableMeasureSet)base.RateableMeasures;
			set => base.RateableMeasures = value;
		}

		/// <summary>
		/// High level access to measures.
		/// Will be null if the underlying measures are null, but this will only happen in unit tests (and shouldn't really even then).
		/// </summary>
		public MeasureAnalyser JobMeasures
		{
			get
			{
				if (jobMeasures == null)
				{
					var measures = RateableMeasures;
					if (measures != null)
					{
						jobMeasures = new MeasureAnalyser(measures);
					}
				}
				return jobMeasures;
			}
		}
		MeasureAnalyser jobMeasures;

		public TimeInfo Time(AccChargeCode chargeCode)
		{
			TimeInfo timeInfo = null;

			if (chargeCode != null && !chargeCode.AC_ChargeSubGroup.IsEmpty)
			{
				timeInfo = JobServices.Time(chargeCode);
			}

			if (timeInfo == null)
			{
				timeInfo = JobMeasures.GetTime();
			}

			return timeInfo ?? TimeInfo.Empty;
		}

		#endregion

		#region Organisations

		public OrgHeader Consignor
		{
			get { return DebtorOrgs[RatingDebtorOrgTypes.CNR]; }
			set { DebtorOrgs[RatingDebtorOrgTypes.CNR] = value; }
		}

		public OrgHeader Consignee
		{
			get { return DebtorOrgs[RatingDebtorOrgTypes.CNE]; }
			set { DebtorOrgs[RatingDebtorOrgTypes.CNE] = value; }
		}

		public OrgHeader LocalClient
		{
			get { return DebtorOrgs[RatingDebtorOrgTypes.LC]; }
			set { DebtorOrgs[RatingDebtorOrgTypes.LC] = value; }
		}

		public OrgHeader OverseasAgent
		{
			get { return DebtorOrgs[RatingDebtorOrgTypes.AG]; }
			set { DebtorOrgs[RatingDebtorOrgTypes.AG] = value; }
		}

		public override ZString DeliveryCartageEquipment
		{
			get
			{
				if (base.DeliveryCartageEquipment.IsEmpty)
				{
					return DefaultCartageEquipment;
				}
				else
				{
					return base.DeliveryCartageEquipment;
				}
			}
		}

		public override ZString PickupCartageEquipment
		{
			get
			{
				if (base.PickupCartageEquipment.IsEmpty)
				{
					return DefaultCartageEquipment;
				}
				else
				{
					return base.PickupCartageEquipment;
				}
			}
		}

		ZString DefaultCartageEquipment
		{
			get
			{
				if (RateTypeToUse == RateType.Warehouse)
				{
					return ZString.Empty;
				}
				else if (IsAirFreight || FreightMode == FreightMode.LCL)
				{
					return Constants.LCLAIREquipmentNeeded.Premise;
				}
				else
				{
					return Constants.FCLEquipmentNeeded.WaitForUnpack;
				}
			}
		}

		List<OrgWithSource> TransportProvidersAndContractorsWithSource => Creditors.AllOrgsWithSource.Concat(JobServices.GetContractorsWithSource()).Where(x => x != null).Distinct().ToList();

		public HashSet<ZGuid> GetTransportProvidersAndContractorPKs(AccChargeCode chargeCode)
		{
			Argument.NotNull(chargeCode, "chargeCode");

			if (!Cache.TransportProvidersAndContractorPKsCache.TryGetValue(chargeCode.PK, out var result))
			{
				result = new HashSet<ZGuid>();
				result.UnionWith(Creditors[chargeCode.AC_ChargeGroup].Select(x => x.Org).Select(x => x.PK));
				result.UnionWith(JobServices.GetContractors(chargeCode).Select(x => x.PK));
				Cache.TransportProvidersAndContractorPKsCache.Add(chargeCode.PK, result);
			}

			return result;
		}

		public List<DebtorOrg> GetDebtors()
		{
			var debtors = DebtorOrgs.ToList();

			if (LocalClient == ImportBroker || LocalClient == ExportBroker)
			{
				debtors.Remove(debtors.FirstOrDefault(d => d.RatingDebtorOrgTypes == RatingDebtorOrgTypes.LC));

				if (LocalClient != null)
				{
					var localClientBroker = new DebtorOrg(LocalClient, RatingDebtorOrgTypes.LCBK);

					if (!debtors.Contains(localClientBroker))
					{
						debtors.Add(localClientBroker);
					}
				}
			}

			return debtors;
		}

		/// <summary>
		/// Get organizations being charged with client rates or company tariffs.
		/// The list adds LocalClient or OverseasAgent if specific registry items are set.
		///
		/// However, Products added an edge case from incident CS01359187 with group rate:
		/// when related party is MNG, registry `charge agent always` itself should not apply any filter to remove charges.
		/// In this case, we use all debtors instead, which may also include OverseasAgent.
		/// </summary>
		public List<OrgHeader> GetChargedOrgs(IRateLine rateLine, bool forCompanyTariff)
		{
			var chargedOrgs = new List<OrgHeader>();

			var chargeCode = rateLine.ChargeCode;
			var chargedParty = IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode, ConsumerType == null || ConsumerType.OverseasAgentApplicable);
			if (chargedParty == ChargedParty.LocalClient)
			{
				chargedOrgs.Add(LocalClient);
			}
			else if (forCompanyTariff && chargedParty == ChargedParty.Agent)
			{
				chargedOrgs.Add(OverseasAgent);
			}
			else
			{
				chargedOrgs.AddRange(GetDebtors().Select(x => x.OrgHeader));
			}

			var result = chargedOrgs
				.Where(x => x != null)
				.Distinct(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer);

			var lineOrg = rateLine.ParentRateEntry.ParentRatingHeader.Header;
			if (lineOrg != null)
			{
				var subsidiaryLineOrgs = lineOrg.RelatedManagementSubsidiaries();
				result = result.Where(x => x.PK == lineOrg.PK || subsidiaryLineOrgs.Any(o => o.PK == x.PK));
			}

			return result.ToList();
		}

		/// <summary>
		/// Returns the union of Carrier and PossibleCarriers, without any duplicates and with no nulls.
		/// </summary>
		public IEnumerable<OrgHeader> AllDistinctCarriers
		{
			get
			{
				var result = new HashSet<OrgHeader>();
				if (Carrier != null)
				{
					result.Add(Carrier);
				}

				var possibles = PossibleCarriers;
				if (possibles != null)
				{
					foreach (var carrier in possibles)
					{
						if (carrier != null)
						{
							result.Add(carrier);
						}
					}
				}
				return result;
			}
		}

		public override OrgHeader Carrier
		{
			get => base.Carrier;
			set
			{
				base.Carrier = value;
				allDistinctCarriersAndConsortiumOrgProxies = null;
			}
		}

		public override IEnumerable<OrgHeader> PossibleCarriers
		{
			get => base.PossibleCarriers;
			set
			{
				base.PossibleCarriers = value;
				allDistinctCarriersAndConsortiumOrgProxies = null;
			}
		}

		/// <summary>
		/// Org header PKs for all carriers plus their consortium org proxies <see cref="GetConsortiumOrgProxyForOrg(ZGuid)"/>
		/// Cached for performance.
		/// </summary>
		public IReadOnlyCollection<ZGuid> AllDistinctCarriersAndConsortiumOrgProxies
		{
			get
			{
				if (allDistinctCarriersAndConsortiumOrgProxies == null)
				{
					allDistinctCarriersAndConsortiumOrgProxies = new HashSet<ZGuid>();
					foreach (var carrier in AllDistinctCarriers)
					{
						allDistinctCarriersAndConsortiumOrgProxies.Add(carrier.PK);
						foreach (var pk in GetConsortiumOrgProxyForOrg(carrier.PK))
						{
							allDistinctCarriersAndConsortiumOrgProxies.Add(pk);
						}
					}
				}
				return allDistinctCarriersAndConsortiumOrgProxies;
			}
		}

		HashSet<ZGuid> allDistinctCarriersAndConsortiumOrgProxies;

		public OrgHeader[] ZoneOwnerOrganizations()
		{
			var ratingZoneOwnerOrganizations = new[]
				{
					LocalClient,
					Carrier,
					OverseasAgent,
					Consignor,
					Consignee,
				}
				.Concat(Creditors.AllOrgs)
				.Concat(PossibleCarriers ?? Enumerable.Empty<OrgHeader>())
				.Distinct(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer)
				.OrderBy(x => x != null ? x.OH_Code : ZString.Empty)
				.ToArray();

			return ratingZoneOwnerOrganizations;
		}

		#endregion

		#region ISpotRate

		public override SpotRateInfo SellSpotRateInfo
		{
			get { return base.SellSpotRateInfo ?? new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.ClientRate); }
		}

		public override SpotRateInfo CostSpotRateInfo
		{
			get { return base.CostSpotRateInfo ?? new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost); }
		}

		#endregion

		#region Customs

		public Money CustomsValue(ILogger autoratingLog, IRateLine rateLine)
		{
			ZDecimal result = 0m;

			if (!Entries.IsNullOrEmpty())
			{
				foreach (var entry in Entries)
				{
					result += entry.CustomsValue;
				}
			}
			else
			{
				autoratingLog.Warning(ResString.GetMultilingualString("6d4081b4-8fec-499c-bf60-e24cead7c5a0", "Customs Value is only available upon action of MERGE (Generate Entries) is performed. Please perform the merging and try again."));
			}

			if (Convert(ref result, Company.LocalCurrency, rateLine, ExchangeRateType.Sell))
			{
				return new Money(result, rateLine.Currency, CalculatorConstants.Text.ApplyTo.Value.Descriptions.CustomsValue);
			}

			return Money.Invalid;
		}

		public Money InvoiceValue(IRateLine rateLine)
		{
			ZDecimal result = 0m;

			if (Invoices != null)
			{
				foreach (var info in Invoices)
				{
					var amount = info.InvoiceValue.Amount;
					if (Convert(ref amount, info.InvoiceValue.Currency, rateLine, ExchangeRateType.Sell))
					{
						result += amount;
					}
					else
					{
						return Money.Invalid;
					}
				}
			}

			return new Money(result, rateLine.Currency, CalculatorConstants.Text.ApplyTo.Value.Descriptions.InvoiceValue);
		}

		#endregion

		#region Time and Locations Filter

		public OrgWithSource[] GetCostsSearchOrgs()
		{
			return TransportProvidersAndContractorsWithSource.Concat(new[] { OrgWithSource.New(null, null) }).ToArray();
		}

		public IEnumerable<ZGuid> GetConsortiumOrgProxyForOrg(ZGuid orgPK)
		{
			if (!orgPK.IsValid)
			{
				return Enumerable.Empty<ZGuid>();
			}

			var key = ZString.Format("RatingCriteria|ConsortiumOrgProxyForOrg-{0}", orgPK); // cache key
			return Factory.GetCachedValue(key, () =>
			{
				var query = new ZDBOnlyQuery(typeof(RefCarrierConsortium));
				query.AddToFilter(RefCarrierConsortiumSchema.RG_OH, SQLComparisonOperator.NotEqual, null);

				var subQuery = new ZDBOnlySubQuery(typeof(RefOrgConsortiumPivot), RefOrgConsortiumPivotSchema.RO_RG);
				subQuery.AddToFilter(RefOrgConsortiumPivotSchema.RO_OH, orgPK);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return Factory.Load<RefCarrierConsortium>(query).Select(x => x.RG_OH);
			});
		}

		public OrgHeader GetOrgHeader(ZGuid orgPK)
		{
			var key = ZString.Format("RatingCriteria|OrgHeader-{0}", orgPK); // cache key
			return Factory.GetCachedValue(key, () => Factory.Load<OrgHeader>(orgPK));
		}

		public ZGuid GetRelatedForwarderGroupParty(ZGuid orgPK)
		{
			var key = ZString.Format("RatingCriteria|RelatedForwarderGroupParty-{0}", orgPK); // cache key
			return Factory.GetCachedValue(key, () =>
			{
				var relatedParty = GetOrgHeader(orgPK)?.GetRelatedParty(RelatedPartyTypeList.Codes.ForwarderGroup, RelatedPartyDirectionList.Codes.Forwarder);
				return relatedParty?.PK ?? Guid.Empty;
			});
		}

		/// <summary>
		///		Get containers from criteria measures
		/// </summary>
		public RefContainer[] GetContainers()
		{
			var containers = new HashSet<RefContainer>();

			var containerPKs = GetContainerPKs();
			var refContainerQuery = new ZQuery(RefContainerSchema.PK, containerPKs);
			var refContainers = Factory.Load<RefContainer>(refContainerQuery);
			foreach (var refContainer in refContainers.WhereNotNull())
			{
				containers.Add(refContainer);
			}

			return containers.ToArray();
		}

		public IEnumerable<ZGuid> GetContainerPKs()
		{
			return IsContainerised
				? JobMeasures.GetContainerTypePKs()
				: Enumerable.Empty<ZGuid>();
		}

		public RefContainerCollection GetContainersInSameClass(ZGuid containerPK)
		{
			var query = new ZQuery(RefContainerSchema.PK, containerPK);
			if (IsLooseRateSearchForCarrierConnect)
			{
				query.ReLoadExistingRows = true;
			}
			var refContainer = Factory.Load<RefContainer>(query).FirstOrDefault();
			if (refContainer != null)
			{
				return refContainer.ContainersInSameFreightRateClass;
			}

			return null;
		}

		/// <summary>
		/// Get the effective date for the given line or the specific charge group.
		/// With order: from rating contract, cost provider, registry date configuration, finally fallback to default standard.
		/// </summary>
		/// <param name="line">Can be null to use specificChargeGroup. If not null, the charge group is gotten from the line.</param>
		/// <param name="specificChargeGroup">Use this charge group instead when the line is null.</param>
		/// <param name="shouldAcceptInvalidDateWhenFallbackIsEnabled">Whether returning invalid date when date type allows fallback, or not</param>
		/// <param name="isCosting">Is it autorating for cost</param>
		/// <param name="overridenContractNumber">When presenting, from rate selector, we use it instead of job's contract number</param>
		///	<param name="overriddenCostProvider">When presenting, from rate selector, we use it instead of job's cost provider</param>
		/// <returns>
		/// The effective date type and the according job date.
		/// </returns>
		public (IAutoRateDate rateDateType, ZDate date) GetEffectiveDate(
			IRateLine line = null,
			string specificChargeGroup = ChargeCodeGroupList.Codes.Freight,
			bool shouldAcceptInvalidDateWhenFallbackIsEnabled = true,
			bool isCosting = true,
			IRatingContract overridenContractNumber = null,
			OrgHeader overriddenCostProvider = null)
		{
			// if the job date is overridden, use it
			var jobDateTypeOverride = isCosting ? JobDateTypes.Codes.CostingAutoratingDateOverride : JobDateTypes.Codes.RevenueAutoratingDateOverride;
			var autoRateDateOverride = JobDatesProvider.GetJobDateByType(jobDateTypeOverride).Date;
			if (!autoRateDateOverride.IsEmpty)
			{
				return (new AutoRateDate { DateType = jobDateTypeOverride, IsFallbackDisabled = false }, autoRateDateOverride);
			}

			IAutoRateDate rateDateType = null;
			var date = ZDate.Empty;

			// store charge group in a variable to avoid multiple null checks
			var chargeGroup = line?.ChargeCode.AC_ChargeGroup ?? specificChargeGroup;
			var rankedJobDateTypes = GetRankedDateTypes(line, chargeGroup, isCosting, overriddenContractNumber: overridenContractNumber, overriddenCostProvider: overriddenCostProvider);
			foreach (var autoRateDate in rankedJobDateTypes)
			{
				rateDateType = autoRateDate;
				var type = rateDateType.DateType;
				date = type.IsEmpty ? ZDate.Today : JobDatesProvider.GetJobDateByType(type).Date;
				var isFallbackDisabled = rateDateType.IsFallbackDisabled;

				// TODO: investigate if invalid dates are actually different from the empty ones.
				// The goal is to remove shouldAcceptInvalidDateWhenFallbackIsEnabled if possible. Its usage looks like a workaround.
				// For now, if the date is valid (which is not empty and is not minDate) or fallback is disabled, we return it early. Otherwise, just accept the last date from the loop.
				if (date.IsValid || shouldAcceptInvalidDateWhenFallbackIsEnabled || isFallbackDisabled)
				{
					break;
				}
			}

			return (rateDateType, date);
		}

		/// <summary>
		/// Get the effective date with fallback to standard registry for the given line or the specific charge group.
		/// With order: from rating contract, cost provider, registry date configuration, finally fallback to default standard.
		/// </summary>
		/// <param name="line">Can be null to use specificChargeGroup. If not null, the charge group is gotten from the line.</param>
		/// <param name="specificChargeGroup">Use this charge group instead when the line is null.</param>
		/// <param name="isCosting">Is it autorating for cost</param>
		/// <returns>
		/// The effective date type, according job date, final date type code, standard fall back description.
		/// </returns>
		public (IAutoRateDate rateDateType, ZDate date, ZString dateType, string standardFilteringDesc) GetEffectiveDateWithFallback(
			IRateLine line = null,
			string specificChargeGroup = ChargeCodeGroupList.Codes.Freight,
			bool isCosting = true)
		{
			var (rateDateType, jobDate) = GetEffectiveDate(line, specificChargeGroup: specificChargeGroup, shouldAcceptInvalidDateWhenFallbackIsEnabled: false, isCosting: isCosting);
			var dateType = rateDateType.DateType;

			var standardFilteringDesc = string.Empty;
			if (rateDateType.IsFallbackDisabled && !jobDate.IsValid)
			{
				return (rateDateType, jobDate, dateType, standardFilteringDesc);
			}
			else if (jobDate.IsEmpty)
			{
				// Looks like we repeat it again what have been done in GetEffectiveDate but the date returned from a higher ranked date type can be empty.
				// It only fallbacks to the standard configuration when the registry setting is NOT STD.
				var registryConfig = RatingCache.AutoRateDateByChargeGroupConfiguration;
				if (registryConfig != null && registryConfig.FilterType != Core.Constants.RatingDateFilterTypes.Codes.Standard)
				{
					standardFilteringDesc = (NoResString)ZString.Format("{0} was empty and so Standard Job filtering was applied. ", JobDateTypes.JobDateTypeList.GetDescriptionFromCode(dateType));

					var chargeGroup = line?.ChargeCode?.AC_ChargeGroup ?? specificChargeGroup;
					dateType = JobDateTypeRetriever.GetStandardJobDateTypeByChargeGroup(chargeGroup).DateType;
					jobDate = JobDatesProvider.GetJobDateByType(dateType).Date;
				}
			}

			return (rateDateType, jobDate, dateType, standardFilteringDesc);
		}

		IEnumerable<IAutoRateDate> GetRankedDateTypes(IRateLine line, string chargeGroup, bool isCosting, IRatingContract overriddenContractNumber = null, OrgHeader overriddenCostProvider = null)
		{
			return GetDateTypesForContract(chargeGroup, isCosting, overriddenContractNumber: overriddenContractNumber) ??
				GetDateTypesForCostProvider(line, chargeGroup, overriddenCostProvider: overriddenCostProvider) ??
				GetDateTypesFromRegistry(chargeGroup) ??
				new[] { JobDateTypeRetriever.GetStandardJobDateTypeByChargeGroup(chargeGroup) };
		}

#if DEBUG

		public IEnumerable<IAutoRateDate> GetRankedDateTypes_ExposedForTest(IRateLine line, string chargeGroup, bool isCosting)
			=> GetRankedDateTypes(line, chargeGroup, isCosting);

#endif

		IEnumerable<IAutoRateDate> GetDateTypesForContract(string chargeGroup, bool isCosting, IRatingContract overriddenContractNumber = null)
		{
			// Get contract from Consol, carrier contract numbers can contain string.empty too if we have CON with empty value in consol's Numbers
			var nonEmptyContractNumber = CarrierContractNumbers.FirstOrDefault(x => !x.IsEmpty);

			if (!isCosting || (string.IsNullOrEmpty(nonEmptyContractNumber) && overriddenContractNumber == null))
			{
				return null;
			}

			IAutoRateDateByChargeGroupConfiguration ratingContract = null;
			var contractConfiguration = GetContractNumberConfiguration(CostSell.Cost);
			if (contractConfiguration.ShouldUseCarrierContractDateFilter)
			{
				if (overriddenContractNumber == null)
				{
					var query = new ZQuery(RatingContractSchema.RCT_ContractNumber, nonEmptyContractNumber);
					query.AddToFilter(RatingContractSchema.RCT_ContractType, Constants.RatingContractTypes.Provider);
					query.AddToFilter(RatingContractSchema.RCT_IsActive, true);
					// RatingContract is actually an IAutoRateDateByChargeGroupConfiguration, but we shouldn't cast it directly for safety.
					ratingContract = Factory.LoadTop1<IRatingContract>(query) as IAutoRateDateByChargeGroupConfiguration;
				}
				else
				{
					ratingContract = overriddenContractNumber as IAutoRateDateByChargeGroupConfiguration;
				}
			}

			var rankedJobDateTypes = GetRankedJobDateTypesWithCustomConfig(ratingContract, chargeGroup);
			return !rankedJobDateTypes.IsNullOrEmpty() ? rankedJobDateTypes : null;
		}

		IEnumerable<IAutoRateDate> GetDateTypesForCostProvider(IRateLine line, string chargeGroup, OrgHeader overriddenCostProvider = null)
		{
			var ratingHeader = line?.ParentRateEntry.ParentRatingHeader;
			var costProvider = overriddenCostProvider ?? ((ratingHeader.IsCosting() || ratingHeader.IsWiseCostRate()) ? ratingHeader?.Header : null);

			var rankedJobDateTypes = GetRankedJobDateTypesWithCustomConfig(costProvider, chargeGroup);
			return !rankedJobDateTypes.IsNullOrEmpty() ? rankedJobDateTypes : null;
		}

		IEnumerable<IAutoRateDate> GetDateTypesFromRegistry(string chargeGroup)
		{
			var dateConfig = RatingCache.AutoRateDateByChargeGroupConfiguration
				?? throw new InvalidOperationException();
			var rankedJobDateTypes = GetRankedJobDateTypesWithCustomConfig(dateConfig, chargeGroup);
			return !rankedJobDateTypes.IsNullOrEmpty() ? rankedJobDateTypes : null;
		}

		/// <summary>
		/// Get job date type from a custom job date type configuration by a charge code group.
		/// </summary>
		/// <param name="customRateDateConfig">Of type IAutoRateDateByChargeGroupConfiguration: either Rate contract, organization, or registry configuration</param>
		/// <param name="chargeGroup"></param>
		/// <returns>
		/// Either a null, an empty enumerable, or an ordered list of job date types. Caller must check null or empty.
		/// We are only interested in the items DateType and IsFallbackDisabled.
		/// </returns>
		IEnumerable<IAutoRateDate> GetRankedJobDateTypesWithCustomConfig(IAutoRateDateByChargeGroupConfiguration customRateDateConfig, string chargeGroup)
		{
			if (customRateDateConfig == null)
			{
				return null;
			}

			switch (customRateDateConfig.FilterType)
			{
				case Constants.RatingDateFilterTypes.Codes.Default:
					return null;

				case Constants.RatingDateFilterTypes.Codes.Standard:
					return new[] { JobDateTypeRetriever.GetStandardJobDateTypeByChargeGroup(chargeGroup) };

				case Constants.RatingDateFilterTypes.Codes.Arrival:
					return new IAutoRateDate[] { new AutoRateDate { DateType = JobDateTypes.Codes.ArrivalDate, IsFallbackDisabled = false } };

				case Constants.RatingDateFilterTypes.Codes.Departure:
					var isFallbackDisabled = chargeGroup == ChargeCodeGroupList.Codes.YardTransportationUnitGateIn;
					return new IAutoRateDate[] { new AutoRateDate { DateType = JobDateTypes.Codes.DepartureDate, IsFallbackDisabled = isFallbackDisabled } };

				case Constants.RatingDateFilterTypes.Codes.Custom:
					var jobDateTypes = customRateDateConfig.GetAutoRateDates(chargeGroup);
					var matchedTypes = jobDateTypes?.Where(IsMatched);
					var sortedTypes = matchedTypes?.OrderByDescending(x => x, new AutoRateDateComparer(Factory));
					return sortedTypes;

				default:
					throw new InvalidOperationException("filterType");
			}
		}

		bool IsMatched(IAutoRateDate autoRateDate)
		{
			var result = autoRateDate.DirectionCode == Core.Constants.FreightShipmentDirection.Code.All
				|| string.IsNullOrEmpty(autoRateDate.DirectionCode)
				|| ImportExportHelper.GetDirectionCode(JobDirection) == autoRateDate.DirectionCode
				|| (ImportExportHelper.GetDirectionCode(JobDirection) == OrgDocumentLookups.FilterDirectionConstants.Codes.CrossTrade && autoRateDate.DirectionCode == OrgDocumentLookups.FilterDirectionConstants.Codes.Other);

			result = result && (autoRateDate.Mode == JobConfigurationSelectorLookups.ModeAdditionalCodes.All
				|| string.IsNullOrEmpty(autoRateDate.Mode)
				|| FreightMode.ToTransportMode() == autoRateDate.Mode);

			result = result && (autoRateDate.JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All
				|| string.IsNullOrEmpty(autoRateDate.JobType)
				|| (ConsumerType != null && ConsumerType.Code == autoRateDate.JobType));  //direction and mode can be null or empty for some job types.

			result = result && (autoRateDate.RateType == JobRateTypes.Codes.All
				|| (autoRateDate.RateType == JobRateTypes.Codes.Cost && _Rating.Cost)
				|| (autoRateDate.RateType == JobRateTypes.Codes.Revenue && _Rating.Sell));

			result = result && (string.IsNullOrEmpty(autoRateDate.ContainerMode)
				|| autoRateDate.ContainerMode == Constants.ContainerModes.All
				|| ContainerMode == autoRateDate.ContainerMode);

			if (!result)
			{
				return false;
			}

			if (string.IsNullOrEmpty(autoRateDate.Location))
			{
				return true;
			}

			var autoRatingLocation = LocationHelper.GetLocationFromString(autoRateDate.Location, factory);
			switch (autoRateDate.DirectionCode)
			{
				case Constants.FreightShipmentDirection.Code.Export:
					return autoRatingLocation.CompletelyCovers(Destination);

				case Constants.FreightShipmentDirection.Code.Import:
					return autoRatingLocation.CompletelyCovers(Origin);

				case Constants.FreightShipmentDirection.Code.Domestic:
					return autoRatingLocation.CompletelyCovers(Origin) && autoRatingLocation.CompletelyCovers(Destination);

				case Constants.FreightShipmentDirection.Code.Other:
					return autoRatingLocation.CompletelyCovers(Origin) || autoRatingLocation.CompletelyCovers(Destination);
				default:
					return false;
			}
		}

		public ZDate LatestPossibleDate
		{
			get { return new ZDateTime(Math.Max(JobDatesProvider.LatestPossibleDate.ToZDateTime().Ticks, ZDate.Today.AddDays(Env.Registry.Rating.ExpiredRateNotificationPeriod).ToZDateTime().Ticks)).Date; }
		}

		public ZDate EarliestPossibleDate
		{
			get { return new ZDateTime(Math.Min(JobDatesProvider.EarliestPossibleDate.ToZDateTime().Ticks, ZDate.Today.AddDays(-Env.Registry.Rating.ExpiredRateNotificationPeriod).ToZDateTime().Ticks)).Date; }
		}

		#endregion

		#region GetExistingCharges

		public override IAutoRatingChargeInfo[] GetExistingCharges(bool fromAllCompanies = false)
		{
			return base.GetExistingCharges(fromAllCompanies) ?? Array.Empty<IAutoRatingChargeInfo>();
		}

		#endregion

		#region Implementation

		public override Creditors Creditors
		{
			get
			{
				try
				{
					return GetCreditorsCore();
				}
				catch (OrgWithSourceNotFoundException ex)
				{
					throw new AutoRaterException(
						ResString.GetMultilingualString(
							"6bc8acd1-a5a1-4d70-b012-8906f9583e26",
							"There was an invalid reference detected. Invalid reference in {0}", ex.HumanReadablePropertyName));
				}
			}
			set
			{
				cache = null;
				base.Creditors = (transportProviders = value);
			}
		}

		protected virtual Creditors GetCreditorsCore()
		{
			return (transportProviders = base.Creditors ?? transportProviders ?? new Creditors());
		}

		protected Creditors transportProviders;

		public override DebtorOrgCollection DebtorOrgs
		{
			get { return (debtorOrgs = base.DebtorOrgs ?? debtorOrgs ?? new DebtorOrgCollection()); }
			set
			{
				cache = null;
				base.DebtorOrgs = (debtorOrgs = value);
			}
		}
		protected DebtorOrgCollection debtorOrgs;

		public override JobServicesCollection JobServices
		{
			get { return (jobServices = base.JobServices ?? jobServices ?? new JobServicesCollection()); }
			set { base.JobServices = (jobServices = value); }
		}
		JobServicesCollection jobServices;

		public IEnumerable<ZString> DestinationServiceLocations
		{
			get
			{
				if (destinationServiceLocations == null)
				{
					destinationServiceLocations = JobServices.Where(x => x.IsEnabled && x.ChargeCodeGroup == ChargeCodeGroupList.Codes.Destination)
						.Select(x => x.LocationCode)
						.Distinct()
						.ToList();
				}
				return destinationServiceLocations;
			}
		}
		List<ZString> destinationServiceLocations;

		public IEnumerable<ZString> OriginServiceLocations
		{
			get
			{
				if (originServiceLocations == null)
				{
					originServiceLocations = JobServices.Where(x => x.IsEnabled && x.ChargeCodeGroup == ChargeCodeGroupList.Codes.Origin)
						.Select(x => x.LocationCode)
						.Distinct()
						.ToList();
				}
				return originServiceLocations;
			}
		}
		List<ZString> originServiceLocations;

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public bool Convert(ref ZDecimal amount, ICurrency amountCurrency, IRateLine rateLine, ExchangeRateType exchangeRateType)
		{
			var currency = rateLine?.Currency;
			if (amountCurrency == null || currency == null)
			{
				return false;
			}
			else if (amountCurrency.Code != currency.RX_Code)
			{
				if (CurrencyConverter != null)
				{
					if (!amount.IsEmpty)
					{
						var originalCurrencyConverterRateType = CurrencyConverter.RateType;
						try
						{
							CurrencyConverter.RateType = exchangeRateType;
							var costOrSell = exchangeRateType == ExchangeRateType.Sell ? CostSell.Revenue : CostSell.Cost;
							var debtorPK = GetDebtorPK(rateLine.ChargeCode);
							amount = CurrencyConverter.ConvertExact(new Money(amount, amountCurrency), currency, debtorPK, costOrSell).Amount;
						}
						finally
						{
							CurrencyConverter.RateType = originalCurrencyConverterRateType;
						}

						if (amount.IsEmpty)
						{
							throw new AutoRater.NoExchangeRateException(Company, amountCurrency, currency);
						}
					}
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		public virtual ZGuid GetDebtorPK(AccChargeCode chargeCode) => Job?.GetDebtorPK(chargeCode, JobNumber) ?? ZGuid.Empty;

		public ZGuid GetGSTID(AccChargeCode chargeCode, CostSell costOrSell) => Job?.GetGSTID(chargeCode, JobNumber, costOrSell, out _) ?? ZGuid.Empty;

		public ZGuid GetGSTID(JobCharge charge, CostSell costOrSell) => Job?.GetGSTID(charge, costOrSell, out _) ?? ZGuid.Empty;

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}
		readonly BusinessObjectFactory factory;

		public JobHeader Job => InvoicingSupporter?.Job;

		#endregion

		#region ContractNumbers

		public string GetSingleClientContractNumber(IEnumerable<string> contractNumbersFromFilteredRates, IDialogService dialogService)
		{
			if (IsMultipleClientContractNumberSupported || contractNumbersFromFilteredRates.IsNullOrEmpty())
			{
				return null;
			}

			if (selectedSingleClientContractNumber != null)
			{
				return selectedSingleClientContractNumber;
			}

			contractNumbersFromFilteredRates = contractNumbersFromFilteredRates
				.Distinct()
				.ToArray();
			var nonBlankContractNumbersFromFilteredRates = contractNumbersFromFilteredRates
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToArray();

			if (nonBlankContractNumbersFromFilteredRates.Length > 1)
			{
				if (dialogService == null)
				{
					return null;
				}

				var selectedNumber = dialogService.SelectSingleClientContractNumber(contractNumbersFromFilteredRates.OrderBy(x => x))
					?? throw new AutoRater.RatingCancelledException(Res.GetString("21A40D0A-E011-4802-B512-86098223C30B", "User didn't select a single Client Contract Number."));

				return (selectedSingleClientContractNumber = selectedNumber);
			}

			return nonBlankContractNumbersFromFilteredRates.FirstOrDefault() ?? contractNumbersFromFilteredRates.FirstOrDefault();
		}

		string selectedSingleClientContractNumber;

		#endregion

		public string OperationalJobRef => InvoicingSupporter?.OperationalJobRef ?? string.Empty;

		public List<OrgHeader> AgentWithRelatedParty
		{
			get { return GetOrgWithRelatedParties(this.OverseasAgent, false); }
		}

		public List<OrgHeader> BillToWithRelatedParty
		{
			get { return GetOrgWithRelatedParties(LocalClient, true); }
		}

		List<OrgHeader> GetOrgWithRelatedParties(OrgHeader org, bool isThisSide)
		{
			var result = new List<OrgHeader>();

			if (org != null)
			{
				result.Add(org);
			}

			switch (JobDirection)
			{
				case Directions.Import:
					result.Add(isThisSide ? Consignee : Consignor);
					break;
				case Directions.Export:
					result.Add(isThisSide ? Consignor : Consignee);
					break;

				default:
					result.Add(Consignee);
					result.Add(Consignor);
					break;
			}

			return result.Where(x => x != null).Distinct(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer).ToList();
		}

		public IEnumerable<OrgHeader> GetSubsidiaryDebtors(OrgHeader orgHeader)
		{
			return orgHeader.RelatedManagementSubsidiaries().Intersect(GetDebtors().Select(x => x.OrgHeader), BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer);
		}

		#region AutoRatedFor

		public override Collection<IBusiness> AutoRatedFor
		{
			get
			{
				var result = new Collection<IBusiness>();
				var collection = base.AutoRatedFor;

				if (collection != null)
				{
					foreach (var element in collection.Where(x => x != null).Distinct())
					{
						result.Add(element);
					}
				}

				if (result.Count == 0)
				{
					ErrorReporter.ReportOnce("The collection should not be null or empty!");
				}

				return result;
			}
		}

		#endregion

		#region IsWarehouseStorage

		public bool IsWarehouseStorage
		{
			get { return ConsumerType == JobInvoicingConsumerTypes.WarehouseStorage; }
		}

		#endregion

		#region IsWarehouseHandling

		public bool IsWarehouseHandling
		{
			get { return ConsumerType == JobInvoicingConsumerTypes.WarehouseInwards || ConsumerType == JobInvoicingConsumerTypes.WarehouseOutwards; }
		}

		#endregion

		#region IsTransitTransportationUnit

		public bool IsTransitTransportationUnit => ConsumerType == JobInvoicingConsumerTypes.TransitReceiveTransportationUnit || ConsumerType == JobInvoicingConsumerTypes.TransitDispatchTransportationUnit;

		#endregion

		public bool IsTransportBooking => ConsumerType == JobInvoicingConsumerTypes.TransportBooking;

		#region Cache

		internal RatingCriteriaCache Cache
		{
			get { return cache ?? (cache = new RatingCriteriaCache(this)); }
		}

		protected RatingCriteriaCache cache;

		#endregion

		public bool FMCTariffIDMatchEnabled { get; set; } = true;

		public PaymentBasis CreatePaymentBasis(RateInfo rateInfo, Quantity chargeable, ZString? chargeableUnitDescription = null, ZString? chargeableDescription = null)
		{
			return rateInfo.IsEmpty && chargeable.IsEmpty ? default(PaymentBasis) : new PaymentBasis(chargeable, rateInfo, AdapterType, OperationalJobCode, chargeableUnitDescription, chargeableDescription);
		}

		public string[] GetRateCategories(RateCategoryGroup rateCategoryGroup)
		{
			return rateCategories.GetOrAdd(rateCategoryGroup, () => RatingConstants.RateCategory.GetRateCategories(RateTypeToUse, rateCategoryGroup));
		}

		readonly Dictionary<RateCategoryGroup, string[]> rateCategories = new Dictionary<RateCategoryGroup, string[]>();

		internal GatewayConfiguration GatewayConfiguration => gatewayConfiguration ?? (gatewayConfiguration = new GatewayConfiguration(this));
		GatewayConfiguration gatewayConfiguration;

		public bool IsManualCostSelectMode { get; set; }
		public bool IsManualCostSelectSearchingCW1Rates { get; set; }

		/// <summary>
		/// Whether job-specific filters shouldn't be applied (which aren't relevant for rate searches in CarrierConnect).
		/// </summary>
		public bool IsLooseRateSearchForCarrierConnect { get; set; }

		public ZGuid SelectedServiceProviderFromRateSelector { get; set; } = ZGuid.Empty;

		public List<ZString> CarrierServiceLevelOverride { get; set; }

		public override void UpdateTransports(IEnumerable<ITransport> transports)
		{
			base.UpdateTransports(transports);

			ResetJobDatesProvider();
			IAutoRating autorating = AutoRating;
			while (autorating is AutoRatingProxy proxy)
			{
				proxy.ResetJobDatesProvider();
				autorating = proxy.AutoRating;
			}
		}

		public IDisposable TemporarilyAllowCriteriaChanging()
		{
			var originalValue = ValuesCanBeSet;
			ValuesCanBeSet = true;
			var action = new DisposableAction(() =>
			{
				ValuesCanBeSet = originalValue;
			});

			return action;
		}
	}
}

