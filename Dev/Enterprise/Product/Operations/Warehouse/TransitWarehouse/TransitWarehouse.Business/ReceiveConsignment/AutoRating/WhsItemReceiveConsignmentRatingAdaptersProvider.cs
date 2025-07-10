using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentRatingAdaptersProvider : WhsItemConsignmentRatingAdaptersProvider<WhsItemReceiveConsignment>
	{
		public WhsItemReceiveConsignmentRatingAdaptersProvider(WhsItemReceiveConsignment parent) : base(parent)
		{
		}

		bool HasFLTCharge { get; set; }
		
		protected override List<IAutoRating> GetAdapters(WhsItemReceiveConsignment parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = new List<IAutoRating>();
			var transitPackagesForRating = ((ITransitJobForRating)parent).TransitPackagesForRating;
			var hasDangerousGoods = transitPackagesForRating.Any(p => p.HasDangerousGoods);
			var transportMode = GetTransportModeForShortTermStorage(parent);
			var freeStorageDays = GetFreeStorageDaysForShortTermStorage(parent, transportMode, hasDangerousGoods);

			var packagesWithFirstDay = new List<TransitPackageForRatingInfo>();
			var packagesWithoutFirstDay = new List<TransitPackageForRatingInfo>();

			foreach (var transitPackage in transitPackagesForRating)
			{
				var hasFirstDayInStorage = transitPackage.UnloadCompleteTime.IsValid || parent.WRC_CompleteTime.IsValid;
				if (hasFirstDayInStorage)
				{
					packagesWithFirstDay.Add(transitPackage);
				}
				else
				{
					packagesWithoutFirstDay.Add(transitPackage);
				}
			}

			if (packagesWithFirstDay.Count > 0)
			{
				result.AddRange(GetShortTermStorageResults(packagesWithFirstDay, parent, freeStorageDays));
			}

			if (packagesWithoutFirstDay.Count > 0)
			{
				var adapter = new WhsItemConsignmentRatingAdapter<WhsItemReceiveConsignment>(parent, packagesWithoutFirstDay);
				result.Add(new TransitReceiveShortTermStorageProxy(
					this,
					adapter,
					new WhsItemReceiveConsignmentJobDatesProvider(parent),
					ZDateTime.Invalid,
					ZDateTime.Invalid
				));
			}

			if (result.Count == 0)
			{
				result.Add(new WhsItemConsignmentRatingAdapter<WhsItemReceiveConsignment>(parent, transitPackagesForRating));
			}

			return result;
		}

		List<TransitReceiveShortTermStorageProxy> GetShortTermStorageResults(
			List<TransitPackageForRatingInfo> packagesWithFirstDay,
			WhsItemReceiveConsignment parent,
			int freeStorageDays
		)
		{
			var result = new List<TransitReceiveShortTermStorageProxy>();
			var latestUnloadCompleteTime = ZDateTimeOffset.Empty;
			var unloadCompleteDateForRatingOfStorageRegistryValue = WarehouseDataRegistry.Instance.UnloadCompleteDateForRatingOfStorage.Value;
			if (unloadCompleteDateForRatingOfStorageRegistryValue == UnloadCompleteDateForRatingOfStorageList.Codes.LastUnloadCompleteDate)
			{
				latestUnloadCompleteTime = packagesWithFirstDay.Where(p => p.UnloadCompleteTime.IsValid).Max(p => p.UnloadCompleteTime);
			}

			var groupedPackagesByDates = packagesWithFirstDay.GroupBy(transitPackage =>
			{
				var firstDay = parent.WRC_CompleteTime;
				if (latestUnloadCompleteTime.IsValid)
				{
					firstDay = latestUnloadCompleteTime;
				}
				else
				{
					firstDay = transitPackage.UnloadCompleteTime.IsValid ? transitPackage.UnloadCompleteTime : firstDay;
				}
				var firstDayAfterFreeDays = firstDay.AddDays(freeStorageDays);
				var lastDay = transitPackage.LoadedTime.IsValid ? transitPackage.LoadedTime : ZDateTimeOffset.Now;
				return new { FirstDay = firstDayAfterFreeDays.ToLocalZDateTime().Date, LastDay = lastDay.ToLocalZDateTime().Date };
			});
			foreach (var transitPackages in groupedPackagesByDates)
			{
				var adapter = new WhsItemConsignmentRatingAdapter<WhsItemReceiveConsignment>(parent, transitPackages);
				var firstDay = transitPackages.Key.FirstDay;
				var lastDay = transitPackages.Key.LastDay;

				if (firstDay <= lastDay)
				{
					result.Add(new TransitReceiveShortTermStorageProxy(
						this,
						adapter,
						new WhsItemReceiveConsignmentJobDatesProvider(parent),
						firstDay,
						lastDay
					));
				}
			}

			return result;
		}

		// Test in TransitWarehouseIntegrationTest
		int GetFreeStorageDaysForShortTermStorage(WhsItemReceiveConsignment parent, ZString transportMode, bool hasDangerousGoods)
		{
			var freeStorageDays = 0;
			var consignee = parent.ConsigneeDocAddress.Organisation;
			if (transportMode == TransportModes.Air)
			{
				if (hasDangerousGoods)
				{
					freeStorageDays = Env.Registry.CFSAirFreightDGLCLStorageFreeDays;
				}
				else
				{
					if (CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.Value && consignee != null)
					{
						freeStorageDays = consignee.MiscServ.OM_IMAirDepotFreeDays;
					}
					else
					{
						freeStorageDays = Env.Registry.CFSAirFreightLCLStorageFreeDays;
					}
				}
			}
			else if (transportMode == TransportModes.Sea)
			{
				if (hasDangerousGoods)
				{
					freeStorageDays = Env.Registry.CFSSeaFreightDGLCLStorageFreeDays;
				}
				else
				{
					if (CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.Value && consignee != null)
					{
						freeStorageDays = consignee.MiscServ.OM_IMSeaDepotFreeDays;
					}
					else
					{
						freeStorageDays = Env.Registry.CFSSeaFreightLCLStorageFreeDays;
					}
				}
			}

			return freeStorageDays;
		}

		ZString GetTransportModeForShortTermStorage(WhsItemReceiveConsignment parent)
		{
			var transportMode = string.Empty;
			switch (parent.WRC_TransportMode)
			{
				case TransportModes.Air:
				case TransportModes.AirSea:
					transportMode = TransportModes.Air;
					break;
				case TransportModes.Sea:
				case TransportModes.SeaAir:
					transportMode = TransportModes.Sea;
					break;
				default:
					break;
			}

			return transportMode;
		}

		class TransitReceiveShortTermStorageProxy : AutoRatingProxy
		{
			readonly WhsItemReceiveConsignmentRatingAdaptersProvider provider;

			public TransitReceiveShortTermStorageProxy(WhsItemReceiveConsignmentRatingAdaptersProvider provider, WhsItemConsignmentRatingAdapter<WhsItemReceiveConsignment> adapter, WhsItemReceiveConsignmentJobDatesProvider jobDatesProvider, ZDateTime from, ZDateTime to)
				: base(adapter)
			{
				ValuesCanBeSet = true;
				var rateableMeasures = (RateableMeasureSet)adapter.GetRateableMeasures();
				if (from.IsValid && to.IsValid)
				{
					jobDatesProvider.SetDate(JobDateTypes.Codes.ArrivalDate, from);
					jobDatesProvider.SetDate(JobDateTypes.Codes.DepartureDate, to);
					JobDatesProvider = jobDatesProvider;
					rateableMeasures.Time = new TimeInfo(JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate),
						JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
				}
				RateableMeasures = rateableMeasures;

				MergeCharges = MergeChargeOptions.CrossAdapter;
				this.provider = provider;
			}

			public override void OnAutoRated(IEnumerable<Rating.Integration.IAutoRatedCharge> charges)
			{
				base.OnAutoRated(charges);
				var rateInfoCollection = charges as AutoRateInfoCollection;
				AutoRateInfo chargeNeedDelete = null;

				foreach (AutoRateInfo charge in charges)
				{
					if (charge.Line.TL_RateCalculator == FlatCalculator.Code)
					{
						if (provider.HasFLTCharge)
						{
							chargeNeedDelete = charge;
						}
						provider.HasFLTCharge = true;
						break;
					}
				}
				rateInfoCollection.Remove(chargeNeedDelete);
			}
		}
	}
}
