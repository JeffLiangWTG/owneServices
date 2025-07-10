using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class SimpleOverriddenRateLinesRemover
	{
		public SimpleOverriddenRateLinesRemover(RatingCriteria criteria)
		{
			this.Criteria = criteria;
			if (criteria == null)
			{
				fastLineProviderWhenCriteriaIsNull = new FastLineProvider(null);
			}
		}

		protected readonly RatingCriteria Criteria;
		readonly FastLineProvider fastLineProviderWhenCriteriaIsNull;

		#region Comparer List

		internal virtual LinkedList<BaseRateLineComparer> GetComparers()
		{
			var result = new LinkedList<BaseRateLineComparer>();

			// Note: It appears that these converters all relate to the parent
			// RateEntry of each RateLine.

			result.AddLast(new RateTypeComparer(Criteria));
			result.AddLast(new GlobalLocalComparer());
			result.AddLast(new GatewayAgentTypeComparer());
			result.AddLast(new GatewayAgentOrderComparer(Criteria));
			result.AddLast(new OriginDestinationComparer(Criteria));
			result.AddLast(new ModeComparer());

			result.AddLast(new ContainerTypeColumnComparer());
			if (Criteria?.RateTypeToUse != RateType.ContainerYardTransportationUnit)
			{
				result.AddLast(new ControllingCustomerComparer(Criteria));
			}
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_AircraftType));
			if (Criteria?.RateTypeToUse != RateType.ContainerYardTransportationUnit)
			{
				result.AddLast(new ColumnComparer(RateEntrySchema.TI_OH_ControllingCustomer));
			}
			result.AddLast(new GuidColumnComparer(RateEntrySchema.TI_OH_Consignor, (NoResString)"Consignor", Criteria?.PickupAddress?.Organisation?.PK ?? ZGuid.Empty, Criteria?.Consignor?.PK ?? ZGuid.Empty)); // log message, subject to change, more for support people as of now
			result.AddLast(new GuidColumnComparer(RateEntrySchema.TI_OH_Consignee, (NoResString)"Consignee", Criteria?.DeliveryAddress?.Organisation?.PK ?? ZGuid.Empty, Criteria?.Consignee?.PK ?? ZGuid.Empty)); // log message, subject to change, more for support people as of now

			result.AddLast(new ColumnComparer(RateEntrySchema.TI_OA_CartagePickupAddressOverride));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_OA_CartageDeliveryAddressOverride));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_R9_FromSuburb));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_R9_ToSuburb));

			result.AddLast(new PostcodeLengthComparer(RateEntrySchema.TI_CartagePickupAddressPostCode));
			result.AddLast(new PostcodeLengthComparer(RateEntrySchema.TI_CartageDeliveryAddressPostCode));

			result.AddLast(new TransportZoneComparer(isCheckingOrigin: true));
			result.AddLast(new TransportZoneComparer(isCheckingOrigin: false));

			result.AddLast(new InternationalZoneComparer(true, Criteria));
			result.AddLast(new InternationalZoneComparer(false, Criteria));

			result.AddLast(new ColumnComparer(RateEntrySchema.TI_ParentID));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_ParentTableCode));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_FMCTariffID));

			var freightPriorities = Env.Registry.Rating.FreightSearchPriorities.Split(',');
			foreach (var columnName in freightPriorities)
			{
				if (string.Equals(columnName, RateEntrySchema.TI_RS_NKServiceLevel_NI.Name, StringComparison.OrdinalIgnoreCase))
				{
					result.AddLast(new ColumnComparer(RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel));
					result.AddLast(new ColumnComparer(RateEntrySchema.TI_RS_NKGatewayServiceLevel));
					result.AddLast(new ColumnComparer(RateEntrySchema.TI_RS_NKServiceLevel_NI));
					result.AddLast(new ColumnComparer(RateEntrySchema.TI_PL_NKCarrierServiceLevel));
				}
				else if (string.Equals(columnName, RateEntrySchema.TI_RH_NKCommodityCode.Name, StringComparison.OrdinalIgnoreCase))
				{
					result.AddLast(new ColumnComparer(RateEntrySchema.TI_RH_NKCommodityCode));
				}
				else if (string.Equals(columnName, RateEntrySchema.TI_ViaLRC.Name, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(columnName, "TI_RL_NKTranshipmentPort", StringComparison.OrdinalIgnoreCase))  // old column name has not been transformed
				{
					result.AddLast(new ColumnComparer(RateEntrySchema.TI_ViaLRC));
				}
				else if (string.Equals(columnName, RateEntrySchema.TI_OH_TransportProvider.Name, StringComparison.OrdinalIgnoreCase))
				{
					result.AddLast(new TransportProviderComparer());
				}
				else if (string.Equals(columnName, RateEntrySchema.TI_HBLDeliveryMode.Name, StringComparison.OrdinalIgnoreCase))
				{
					result.AddLast(new HBLDeliveryModeComparer(Criteria));
				}
			}

			result.AddLast(new ColumnComparer(RateEntrySchema.TI_OH_Supplier));

			result.AddLast(new PickupTransportComparer(Criteria));
			result.AddLast(new DeliveryTransportComparer(Criteria));

			result.AddLast(new RateOriginRateDestinationComparer(Criteria));
			result.AddLast(new PlannedLoadPlannedDischargeComparer(Criteria));
			result.AddLast(new GatewayPlannedLoadPlannedDischargeComparer(Criteria));
			result.AddLast(new PaymentTermOverrideComparer());

			result.AddLast(new MatchingLocationComparer(RateEntrySchema.TI_FirstLoadLRC, Criteria));
			result.AddLast(new MatchingLocationComparer(RateEntrySchema.TI_LastDischargeLRC, Criteria));
			result.AddLast(new MatchingLocationComparer(RateEntrySchema.TI_FirstRouteSetLoadPortLRC, Criteria));
			result.AddLast(new MatchingLocationComparer(RateEntrySchema.TI_LastRouteSetDischargePortLRC, Criteria));

			result.AddLast(new CrossTradeComparer());

			result.AddLast(new ColumnComparer(RateEntrySchema.TI_IsNonOperatedReefer));

			result.AddLast(new ColumnComparer(RateEntrySchema.TI_RCC_ComponentCode));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_ContainerUnitSection));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_RRC_RepairCode));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_RMC_Material));

			result.AddLast(new ColumnComparer(RateEntrySchema.TI_EstimateType));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_REG_EquipmentGrade));
			result.AddLast(new ColumnComparer(RateEntrySchema.TI_MNRGroup));

			return result;
		}

		internal virtual bool IsComparable(FastLine line1, FastLine line2)
		{
			return chargeCodeComparer.Equals(line1.ChargeCode, line2.ChargeCode);
		}
		readonly ChargeCodeComparer chargeCodeComparer = new ChargeCodeComparer();

		#endregion

		#region Implementation

		FastLine GetOrCreateFastLine(IRateLine rateLine)
			=> Criteria != null ? Criteria.Cache.GetOrCreateFastLine(rateLine) : fastLineProviderWhenCriteriaIsNull.GetOrCreate(rateLine);

		/// <summary>
		/// Compares all the RateLines in the given list and modifies the list
		/// to exclude those which are less-specific than others.
		///
		/// Note this method is slow if there are too many items.
		/// </summary>
		public void Remove<T>(List<T> rateLines, RateLinesRepository lineRepository = null) where T : IRateLine
		{
			if (rateLines.Count <= 1)
			{
				return;
			}

			var rateLinesByChargeCodeHash =
				rateLines
				.GroupBy(x => chargeCodeComparer.GetHashCode(x.ChargeCode))
				.ToHashSet();
			rateLines.Clear();

			foreach (var similarRateLineGroups in rateLinesByChargeCodeHash)
			{
				var similarFastLines =
					similarRateLineGroups
					.Select(x => GetOrCreateFastLine(x))
					.ToList();

				RemoveCore(similarFastLines, lineRepository);
				rateLines.AddRange(similarFastLines.Select(x => x.Line).Cast<T>());
			}
		}

		/// <summary>
		/// Compares all the RateLines in the given list and modifies the list
		/// to exclude those which are less-specific than others.
		///
		/// Note this method is slow if there are too many items.
		/// </summary>
		public void Remove(List<FastLine> rateLines, RateLinesRepository lineRepository = null)
		{
			if (rateLines.Count <= 1)
			{
				return;
			}

			var rateLinesByChargeCodeHash =
				rateLines
				.GroupBy(x => chargeCodeComparer.GetHashCode(x.ChargeCode))
				.ToHashSet();
			rateLines.Clear();

			foreach (var similarFastLineGroups in rateLinesByChargeCodeHash)
			{
				var similarFastLines = similarFastLineGroups.ToList();

				RemoveCore(similarFastLines, lineRepository);
				rateLines.AddRange(similarFastLines);
			}
		}

		/// <summary>
		/// Compares all the FastLines in the given list and modifies the list
		/// to exclude those which are less-specific than others in the list.
		/// It is not the responsibility of this function or class to remove
		/// identical RateLines.
		///
		/// If RateLinesRepository is specified, it will remove the discarded ones
		/// from there as well.
		/// </summary>
		void RemoveCore(List<FastLine> rateLines, RateLinesRepository rateLinesRepository = null)
		{
			var delayedExceptions = new Queue<DelayedException>();

			// These two loops start from the end of the list and work back to the front
			// while looking at combinations of twos from the end of the array (for the second index)
			// down to the position of the first index.
			for (var rateLineIndex1 = rateLines.Count - 2; rateLineIndex1 >= 0; rateLineIndex1--)
			{
				for (var rateLineIndex2 = rateLines.Count - 1; rateLineIndex2 > rateLineIndex1; rateLineIndex2--)
				{
					var rateLine1 = rateLines[rateLineIndex1];
					var rateLine2 = rateLines[rateLineIndex2];
					// Despite this function being called with a hash for the chargecode
					// this IsComparable below is still needed as it also checks the AC_GC field
					// in a special way to allow for local and global charge codes
					if (IsComparable(rateLine1, rateLine2))
					{
						try
						{
							BaseRateLineComparer overrideCauseComparer;
							var compareResult = Compare(rateLine1, rateLine2, out overrideCauseComparer);
							if (compareResult > 0)
							{
								// in this case rateline1 > rateLine2; so discarding rateLine2 off the end of the array is ok
								Remove(overriddenLineIndex: rateLineIndex2, overriddenBy: rateLine1, rateLines, rateLinesRepository, overrideCauseComparer);
							}
							else if (compareResult < 0)
							{
								// in this case rateline1 < rateLine2; so we need break the loop so we can
								// skip over the unwanted one as it's now potentially removed from the `rateLines`
								Remove(overriddenLineIndex: rateLineIndex1, overriddenBy: rateLine2, rateLines, rateLinesRepository, overrideCauseComparer);
								break;
							}
							// The case of compareResult == 0 is not meant to happen in reality
							// The Remove method, in this case, does NOT remove it silently.
						}
						catch (AutoRaterException ex)
						{
							delayedExceptions.Enqueue(new DelayedException(rateLine1, rateLine2, ex));
						}
					}
				}
			}

			foreach (var delayedEx in delayedExceptions)
			{
				if (rateLines.Contains(delayedEx.line1) || rateLines.Contains(delayedEx.line2))
				{
					throw delayedEx.ex;
				}
			}
		}

		void Remove(int overriddenLineIndex, FastLine overriddenBy, List<FastLine> list, RateLinesRepository rateLinesRepository, BaseRateLineComparer overrideCauseComparer)
		{
			var overriddenLine = list[overriddenLineIndex];

			if (rateLinesRepository != null)
			{
				rateLinesRepository.Remove(overriddenLine, overrideCauseComparer.GetReason(overriddenLine, overriddenBy));
			}
			else
			{
				list.RemoveAt(overriddenLineIndex);
			}
		}

#if DEBUG
		internal int CompareForTest(IRateLine line1, IRateLine line2, out BaseRateLineComparer overrideCauseComparer)
		{
			return Compare(GetOrCreateFastLine(line1), GetOrCreateFastLine(line2), out overrideCauseComparer);
		}
#endif
		/// <summary>
		/// Returns a positive or negative number. Does NOT return 0
		/// </summary>
		internal virtual int Compare(FastLine line1, FastLine line2, out BaseRateLineComparer overrideCauseComparer)
		{
			var i = Comparers.Count;
			foreach (var comparer in Comparers)
			{
				var compareResult = comparer.Compare(line1, line2);
				if (compareResult != 0)
				{
					overrideCauseComparer = comparer;
					return Math.Sign(compareResult) * (100 + i);
				}
				i--;
			}

			overrideCauseComparer = null;
			return 0;
		}

		LinkedList<BaseRateLineComparer> Comparers
		{
			get { return comparers ?? (comparers = GetComparers()); }
		}

		LinkedList<BaseRateLineComparer> comparers;

		class DelayedException
		{
			public DelayedException(FastLine line1, FastLine line2, Exception ex)
			{
				this.line1 = line1;
				this.line2 = line2;
				this.ex = ex;
			}

			public readonly FastLine line1;
			public readonly FastLine line2;
			public readonly Exception ex;
		}

		#endregion
	}
}


