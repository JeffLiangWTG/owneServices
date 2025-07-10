using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	static class DtbConsignmentConfirmationTotalsCache
	{
		#region Constants

		const string ConfirmationPK = "ConfirmationPK";
		const string PackType = "PackType";
		const string PackTypeCount = "PackTypeCount";
		const string BookedPackType = "BookedPackType";
		const string BookedPackTypeCount = "BookedPackTypeCount";
		const string BookingPK = "BookingPK";
		public const string TotalPackages = "TotalPackages";
		public const string TotalWeight = "TotalWeight";
		public const string TotalVolume = "TotalVolume";

		#endregion

		#region class DtbConsignmentConfirmationsCache

		class DtbConsignmentActionsCache : DtbConsignmentCommonCache
		{
			protected override string GetBookedPickupsSQL
			{
				get
				{
					return @"
					SELECT
						LTA_PK as ConfirmationPK,
						SUM(BookedPackTypeCount) as BookedPackTypeCount,
						BookedPackType,
						BookingPK
					FROM
						dbo.DtbConsignmentAction
						JOIN dbo.DtbConsignmentAddress ON LTS_PK = LTA_LTS_ConsignmentAddress
						JOIN dbo.DtbConsignment ON LTC_PK = LTS_LTC_Consignment
						JOIN dbo.CusEntryNum ON CE_ParentID = LTC_PK AND CE_ParentTable = 'DtbConsignment' AND CE_EntryType = 'BJB'
						JOIN dbo.DtbBooking ON CE_EntryNum = KM_JobID
						JOIN
						(
							SELECT
								SUM(KD_Quantity) as BookedPackTypeCount,
								KP_F3_NKPackType as BookedPackType,
								KM_PK as BookingPK
							FROM
								dbo.PkgPackage
								JOIN dbo.DtbBookingInstructionPkgDivot ON KD_KP_Package = KP_PK
								JOIN dbo.DtbBookingInstruction ON KD_KN_BookingInstruction = KN_PK
								JOIN dbo.DtbBooking ON KN_KM_BookingMovement = KM_PK
								JOIN dbo.DtbBookingConsolidation ON KM_KB_Booking = KB_PK AND KB_JobType = @BookingJobType
							WHERE
								KN_InstructionType = @PickupInstructionType
							GROUP BY
								KM_PK,
								KP_F3_NKPackType
						) as BookedPackages ON BookingPK = KM_PK

					WHERE
						LTA_PK IN
						({0})
						AND LTS_InstructionType = @PickupInstructionType

					GROUP BY
						LTA_PK,
						BookedPackType,
						BookingPK

					ORDER BY
						BookedPackType";
				}
			}

			protected override ZSqlParameter[] GetBookedPickupsParameters
			{
				get
				{
					return new[]
					{
						ZSqlParameter.New("@BookingJobType", TransportConsolidationJobTypes.Codes.Booking, DtbBookingConsolidationSchema.KB_JobType),
						ZSqlParameter.New("@PickupInstructionType", InstructionTypes.Codes.PickUp, DtbConsignmentAddressSchema.LTS_InstructionType)
					};
				}
			}

			protected override string GetPackageTotalsSQL
			{
				get
				{
					return @"
					SELECT
						ConfirmationPK,
						SUM(TotalWeight) OVER (PARTITION BY ConfirmationPK) as TotalWeight,
						SUM(TotalVolume) OVER (PARTITION BY ConfirmationPK) as TotalVolume,
						SUM(PackTypeCount) OVER (PARTITION BY ConfirmationPK) as TotalPackages,
						PackType,
						PackTypeCount

					FROM
					(
						SELECT
							LTA_PK as ConfirmationPK,
							SUM(ConvertWeight.Value) as TotalWeight,
							SUM(ConvertVolume.Value) as TotalVolume,
							SUM(KP_PackageQty) as PackTypeCount,
							KP_F3_NKPackType as PackType

						FROM
							dbo.PkgPackage
							JOIN dbo.PkgPackageJob on KJ_PK = KP_KJ_ParentPackageJob
							JOIN dbo.DtbConsignment on KJ_ParentID = LTC_PK
							JOIN dbo.DtbConsignmentAddress on LTS_LTC_Consignment = LTC_PK
							JOIN dbo.DtbConsignmentAction on LTA_LTS_ConsignmentAddress = LTS_PK
							CROSS APPLY ConvertWeight(KP_Weight, KP_WeightUQ, @WeightUnit)
							CROSS APPLY ConvertVolume(KP_Volume, KP_VolumeUQ, @VolumeUnit)
						WHERE
							KP_KP_ParentPackage IS NULL
							AND (LTA_PK IN ({0}))

						GROUP BY
							LTA_PK,
							KP_F3_NKPackType
					) as Packages

					ORDER BY
						PackType";
				}
			}
		}

		class DtbBookingConsignmentConfirmationsCache : DtbConsignmentCommonCache
		{
			protected override string GetBookedPickupsSQL
			{
				get
				{
					return @"
					SELECT
						KK_PK as ConfirmationPK,
						SUM(BookedPackTypeCount) as BookedPackTypeCount,
						BookedPackType,
						BookingPK
					FROM
						dbo.DtbBookingConfirmation
						JOIN dbo.DtbBookingInstruction ON KK_KN_BookingInstruction = KN_PK
						JOIN dbo.DtbBooking ON KN_KM_BookingMovement = KM_PK
						JOIN dbo.DtbBookingConsolidation ON KM_KB_Booking = KB_PK AND KB_JobType = @ConsignmentJobType
						JOIN
						(
							SELECT
								SUM(KD_Quantity) as BookedPackTypeCount,
								KP_F3_NKPackType as BookedPackType,
								KM_PK as BookingPK
							FROM
								dbo.PkgPackage
								JOIN dbo.DtbBookingInstructionPkgDivot ON KD_KP_Package = KP_PK
								JOIN dbo.DtbBookingInstruction ON KD_KN_BookingInstruction = KN_PK
								JOIN dbo.DtbBooking ON KN_KM_BookingMovement = KM_PK
								JOIN dbo.DtbBookingConsolidation ON KM_KB_Booking = KB_PK AND KB_JobType = @BookingJobType
							WHERE
								KN_InstructionType = @PickupInstructionType
							GROUP BY
								KM_PK,
								KP_F3_NKPackType
						) as BookedPackages ON BookingPK = KB_ParentID

					WHERE
						KK_PK IN
						({0})
						AND KN_InstructionType = @PickupInstructionType

					GROUP BY
						KK_PK,
						BookedPackType,
						BookingPK

					ORDER BY
						BookedPackType";
				}
			}

			protected override ZSqlParameter[] GetBookedPickupsParameters
			{
				get
				{
					return new[]
					{
						ZSqlParameter.New("@BookingJobType", TransportConsolidationJobTypes.Codes.Booking, DtbBookingConsolidationSchema.KB_JobType),
						ZSqlParameter.New("@ConsignmentJobType", TransportConsolidationJobTypes.Codes.Consignment, DtbBookingConsolidationSchema.KB_JobType), // needed only for performance gain
						ZSqlParameter.New("@PickupInstructionType", InstructionTypes.Codes.PickUp, DtbBookingInstructionSchema.KN_InstructionType)
					};
				}
			}

			protected override string GetPackageTotalsSQL
			{
				get
				{
					return @"
					SELECT
						ConfirmationPK,
						SUM(TotalWeight) OVER (PARTITION BY ConfirmationPK) as TotalWeight,
						SUM(TotalVolume) OVER (PARTITION BY ConfirmationPK) as TotalVolume,
						SUM(PackTypeCount) OVER (PARTITION BY ConfirmationPK) as TotalPackages,
						PackType,
						PackTypeCount

					FROM
					(
						SELECT
							KK_PK as ConfirmationPK,
							SUM(ConvertWeight.Value) as TotalWeight,
							SUM(ConvertVolume.Value) as TotalVolume,
							SUM(DtbBookingInstructionPkgDivot.KD_Quantity) as PackTypeCount,
							KP_F3_NKPackType as PackType

						FROM
							dbo.PkgPackage
							JOIN dbo.DtbBookingInstructionPkgDivot ON KD_KP_Package = KP_PK
							JOIN dbo.DtbBookingInstruction ON KD_KN_BookingInstruction = KN_PK
							JOIN dbo.DtbBookingConfirmation ON KK_KN_BookingInstruction = KN_PK
							CROSS APPLY ConvertWeight(KP_Weight, KP_WeightUQ, @WeightUnit)
							CROSS APPLY ConvertVolume(KP_Volume, KP_VolumeUQ, @VolumeUnit)
						WHERE
							KP_KP_ParentPackage IS NULL
							AND KK_PK IN
							({0})

						GROUP BY
							KK_PK,
							KP_F3_NKPackType
					) as Packages

					ORDER BY
						PackType";
				}
			}
		}

		abstract class DtbConsignmentCommonCache
		{
			internal DtbConsignmentCommonCache()
			{
			}

			#region Initialise / Collections

			public void Initialise(string key, IEnumerable<IConsignmentAction> confirmations)
			{
				using (SuspendTotalsCalculation())
				{
					AllConfirmations = confirmations;

					foreach (var confirmation in AllConfirmations)
					{
						confirmation.KeyForCache = key;
						ConfirmationPKsBuilder.Append(ZSqlParameter.New("@ConfirmationPK", confirmation.PK, DtbBookingConfirmationSchema.PK).ParameterValueTextSql);
					}
				}
			}

			ZStringBuilder ConfirmationPKsBuilder
			{
				get { return confirmationPKsBuilder ?? (confirmationPKsBuilder = new ZStringBuilder()); }
			}

			ZStringBuilder confirmationPKsBuilder;

			IEnumerable<IConsignmentAction> AllConfirmations
			{
				get { return allConfirmations; }
				set
				{
					if (allConfirmations != null)
					{
						throw new InvalidOperationException("Should not call Initialise twice.");
					}

					allConfirmations = value;
				}
			}

			IEnumerable<IConsignmentAction> allConfirmations;

			#endregion

			#region GetPackageTotalsCached

			public IDictionary<ZGuid, Func<PackageTotals>> GetPackageTotalsCached(BusinessObjectFactory factory)
			{
				IDictionary<ZGuid, Func<PackageTotals>> result = null;

				if (AllConfirmations != null && !IsTotalsCalculationSuspended)
				{
					using (SuspendTotalsCalculation())
					{
						if (!HasLoadedPackageTotals)
						{
							CalculatePackageTotals(factory);
						}
					}

					result = PackageTotals;
				}

				return result;
			}

			void CalculatePackageTotals(BusinessObjectFactory factory)
			{
				if (!HasLoadedPackageTotals)
				{
					HasLoadedPackageTotals = true;

					if (!ConfirmationPKsBuilder.IsEmpty)
					{
						var confirmationPKs = ConfirmationPKsBuilder.ToStringWithDelimiterBetweenAppends(", ");
						var packageTotals = GetPackageTotals(factory, confirmationPKs);
						var bookedPickups = GetBookedPickups(factory, confirmationPKs);

						PackageTotals = new Dictionary<ZGuid, Func<PackageTotals>>(packageTotals.Count);

						var tempPackTypeQuantities = new Dictionary<ZGuid, List<PackTypeCount>>(packageTotals.Count);

						foreach (DynamicBusinessObject packageTotalBizO in packageTotals)
						{
							List<PackTypeCount> result;

							var confirmationPK = (ZGuid)packageTotalBizO[ConfirmationPK];
							if (!tempPackTypeQuantities.TryGetValue(confirmationPK, out result))
							{
								// set the parent booking's pickup pack types
								GroupedPackTypeCounts bookedPackTypes;
								if (bookedPickups.ContainsKey(confirmationPK))
								{
									bookedPackTypes = bookedPickups[confirmationPK]();

									// The foreach we are in will of course never iterate over consignments that do not have packages (eg. user may have removed them).
									// We still want to store/show the booked pickups for these, so remove each booked pickup total that is successfully added.
									// The remaining booked pickups are for confirmations that had no packages. We will use this 'trimmed' list later to add their booked pickups to the cache.
									bookedPickups.Remove(confirmationPK);
								}
								else
								{
									bookedPackTypes = GroupedPackTypeCounts.Empty;
								}

								tempPackTypeQuantities[confirmationPK] = result = new List<PackTypeCount>();
								AddPackageTotalsToCache(confirmationPK, packageTotalBizO, result, bookedPackTypes);
							}

							// build up the list of package types and their amount.
							result.Add(new PackTypeCount((ZString)packageTotalBizO[PackType], (ZInt)packageTotalBizO[PackTypeCount]));
						}

						AddBookedPickupsToConsignmentsWithNoPackages(factory, bookedPickups);
					}

					ClearCache();
				}
			}

			protected abstract string GetPackageTotalsSQL { get; }

			DynamicBusinessObjectCollection GetPackageTotals(BusinessObjectFactory factory, string confirmationPKs)
			{
				var volumeUnit = DtbTransportTotalsHelper.TotalVolumeUnit;
				var weightUnit = DtbTransportTotalsHelper.TotalWeightUnit;
				var sql = string.Format(Culture.Invariant, GetPackageTotalsSQL, confirmationPKs);

				var dynamicBizOCollection = new DynamicBusinessObjectCollection(factory);
				dynamicBizOCollection.Load(sql, new[]
				{
					ZSqlParameter.New("@WeightUnit", weightUnit, PkgPackageSchema.KP_WeightUQ),
					ZSqlParameter.New("@VolumeUnit", volumeUnit, PkgPackageSchema.KP_VolumeUQ)
				});

				return dynamicBizOCollection;
			}

			protected abstract string GetBookedPickupsSQL { get; }
			protected abstract ZSqlParameter[] GetBookedPickupsParameters { get; }

			Dictionary<ZGuid, Func<GroupedPackTypeCounts>> GetBookedPickups(BusinessObjectFactory factory, string confirmationPKs)
			{
				var bookedPickupsSql = string.Format(Culture.Invariant, GetBookedPickupsSQL, confirmationPKs);

				var bookedPickups = new DynamicBusinessObjectCollection(factory);
				bookedPickups.Load(bookedPickupsSql, GetBookedPickupsParameters);

				var bookedPickupGrouped = new Dictionary<ZGuid, Func<GroupedPackTypeCounts>>();
				var bookedPickupPackTypes = new Dictionary<ZGuid, List<PackTypeCount>>();

				foreach (DynamicBusinessObject bookedPickup in bookedPickups)
				{
					List<PackTypeCount> result;

					var confirmationPK = (ZGuid)bookedPickup[ConfirmationPK];
					if (!bookedPickupPackTypes.TryGetValue(confirmationPK, out result))
					{
						var bookingPK = (ZGuid)bookedPickup[BookingPK];
						bookedPickupPackTypes[confirmationPK] = result = new List<PackTypeCount>();
						bookedPickupGrouped[confirmationPK] = () => new GroupedPackTypeCounts(bookingPK, result);
					}

					result.Add(new PackTypeCount((ZString)bookedPickup[BookedPackType], (ZInt)bookedPickup[BookedPackTypeCount]));
				}

				return bookedPickupGrouped;
			}

			void AddBookedPickupsToConsignmentsWithNoPackages(BusinessObjectFactory factory, IEnumerable<KeyValuePair<ZGuid, Func<GroupedPackTypeCounts>>> bookedPickups)
			{
				if (bookedPickups.Any())
				{
					var collection = new DynamicBusinessObjectCollection(factory);
					collection.Load("SELECT 0.0 as TotalWeight, 0.0 as TotalVolume, 0 as TotalPackages");
					var defaultBizO = collection[0];

					foreach (var bookedPickup in bookedPickups)
					{
						AddPackageTotalsToCache(bookedPickup.Key, defaultBizO, Enumerable.Empty<PackTypeCount>(), bookedPickup.Value());
					}
				}
			}

			void AddPackageTotalsToCache(ZGuid key, DynamicBusinessObject packTotalsBizO, IEnumerable<PackTypeCount> packTypes, GroupedPackTypeCounts bookedPackTypes)
			{
				// Delay the adding of PackageTotals through a delegate. The Delegate stores local
				// scope and the PackageTotals will be created with the list fully populated.
				// This is also more performance beneficial.
				//
				// Note this functionality relies on delegates to work. Removing the use of delegates
				// breaks the code because the PackageTotals object is then created with an empty list.
				PackageTotals[key] = () => new PackageTotals(packTotalsBizO, packTypes, bookedPackTypes);
			}

			bool HasLoadedPackageTotals
			{
				get { return hasLoadedPackageTotals; }
				set
				{
					if (hasLoadedPackageTotals)
					{
						throw new InvalidOperationException("Should never load Package Totals twice.");
					}

					hasLoadedPackageTotals = value;
				}
			}

			IDictionary<ZGuid, Func<PackageTotals>> PackageTotals;
			bool hasLoadedPackageTotals;

			#endregion

			#region Clear

			void ClearCache()
			{
				confirmationPKsBuilder = null;
				allConfirmations = new List<IConsignmentAction>(0); // No need to store elements anymore
			}

			#endregion

			#region Semaphore / Suspension

			IDisposable SuspendTotalsCalculation()
			{
				return new SemaphoreManager(TotalsCalculationSemaphore);
			}

			bool IsTotalsCalculationSuspended
			{
				get { return TotalsCalculationSemaphore.IsSuspended; }
			}

			Semaphore TotalsCalculationSemaphore
			{
				get { return totalsCalculationSemaphore ?? (totalsCalculationSemaphore = new Semaphore()); }
			}

			Semaphore totalsCalculationSemaphore;

			#endregion
		}

		#endregion

		#region Initialise / Collections

		public static void InitialiseActionsTotalsCache(this BusinessObjectFactory factory, string key, IEnumerable<DtbConsignmentAction> actions)
		{
			factory.GetConsignmentCache(key).Initialise(key, actions);
		}

		public static void InitialiseConfirmationsTotalsCache(this BusinessObjectFactory factory, string key, IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			factory.GetBookingConsignmentCache(key).Initialise(key, confirmations);
		}

		#endregion

		#region GetCachedConfirmationsTotals

		public static IDictionary<ZGuid, Func<PackageTotals>> GetCachedConfirmationsTotals(this BusinessObjectFactory factory, string key)
		{
			return factory.GetBookingConsignmentCache(key).GetPackageTotalsCached(factory);
		}

		#endregion

		#region GetCachedActionsTotals

		public static IDictionary<ZGuid, Func<PackageTotals>> GetCachedActionsTotals(this BusinessObjectFactory factory, string key)
		{
			return factory.GetConsignmentCache(key).GetPackageTotalsCached(factory);
		}

		#endregion

		#region GetCache

		static DtbConsignmentCommonCache GetConsignmentCache(this BusinessObjectFactory factory, string key)
		{
			return factory.GetCachedValue("DtbConsignmentActionsCache|" + key, () => new DtbConsignmentActionsCache());
		}

		static DtbConsignmentCommonCache GetBookingConsignmentCache(this BusinessObjectFactory factory, string key)
		{
			return factory.GetCachedValue("DtbBookingConsignmentConfirmationsCache|" + key, () => new DtbBookingConsignmentConfirmationsCache());
		}

		#endregion
	}
}
