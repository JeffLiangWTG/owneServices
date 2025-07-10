using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageRatingAdaptersProvider : RatingAdaptersProvider<CommonCartage>
	{
		public CommonCartageRatingAdaptersProvider(CommonCartage parent)
			: base(parent)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		protected override List<IAutoRating> GetAdapters(CommonCartage parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return options.AutoratingProcess == CostSell.Cost
				? GetAdaptersForCost(parent, uiInteractor)
				: GetAdaptersForRevenue(parent, uiInteractor);
		}

		protected override IQuickCalculateRating GetForQuickCalculate(CommonCartage parent, IAutoRatingInteractor uiInteractor)
		{
			var allMoves = parent.BookedMovesCollection;

			var allMoveAdapters = allMoves
				.Select(move => new CartageMoveRatingAdapter(move, move.PickupFromDocAddress, move.DeliverToDocAddress, true))
				.ToList();

			var allMeasures = allMoveAdapters
				.Select(m => m.RateableMeasures)
				.Cast<RateableMeasureSet>();

			if (!allMeasures.Any())
			{
				return base.GetForQuickCalculate(parent, uiInteractor);
			}

			var finalMeasure = MergeMeasures(allMeasures);
			return new QuickCalculateRating(allMoveAdapters[0], finalMeasure);
		}

		/// <summary>
		/// Merges the given measures, and converts values to the units of the first measures.
		/// It only focuses on measures necessary for QuickCalculate. Namely:
		/// - Weight
		/// - Volume
		/// - Unit
		/// - Package
		/// - Containers
		/// <param name="allMeasures"></param>
		/// <returns></returns>
		RateableMeasureSet MergeMeasures(IEnumerable<RateableMeasureSet> allMeasures)
		{
			// The first measure will be used as the basis of the units to convert-to.
			var firstMeasure = allMeasures.First();
			var otherMeasures = allMeasures.Skip(1).ToList();
			var destinationMeasure = new RateableMeasureSet(AdapterType.PortTransport);

			var mergedWeight = MergeAndConvert(MeasureType.Weight, firstMeasure, otherMeasures);
			var mergedVolume = MergeAndConvert(MeasureType.Volume, firstMeasure, otherMeasures);
			var mergedPacks = MergePotentiallyIgnoringUnits(MeasureType.Package, firstMeasure, otherMeasures);
			var mergedUnit = MergePotentiallyIgnoringUnits(MeasureType.Unit, firstMeasure, otherMeasures);

			destinationMeasure.SetCartagePackage(null, null, null,
				mergedWeight.Total, mergedWeight.Unit,
				mergedVolume.Total, mergedVolume.Unit,
				mergedPacks.Total, mergedPacks.Unit);
			destinationMeasure.SetQuantity(MeasureType.Unit, mergedUnit.Total, mergedPacks.Unit);

			MergeContainers(destinationMeasure, allMeasures);

			// We do this so we have parts for other values that aren't the ones that we just changed
			firstMeasure.RestorePartList(MeasureType.ContainerCount, destinationMeasure.GetPartList(MeasureType.ContainerCount));
			firstMeasure.RestorePartList(MeasureType.Package, destinationMeasure.GetPartList(MeasureType.Package));
			firstMeasure.RestorePartList(MeasureType.Unit, destinationMeasure.GetPartList(MeasureType.Unit));
			firstMeasure.RestorePartList(MeasureType.Weight, destinationMeasure.GetPartList(MeasureType.Weight));
			firstMeasure.RestorePartList(MeasureType.Volume, destinationMeasure.GetPartList(MeasureType.Volume));

			return firstMeasure;
		}

		(ZDecimal Total, string Unit) MergeAndConvert(MeasureType type, RateableMeasureSet firstMeasure, List<RateableMeasureSet> otherMeasures)
		{
			var unit = firstMeasure.GetUnit(type);
			var totalValue = firstMeasure.GetActual(type);

			foreach (var otherMeasure in otherMeasures)
			{
				var otherUnit = otherMeasure.GetUnit(type);
				var otherValue = otherMeasure.GetActual(type);
				totalValue += Convert(otherValue, otherUnit, unit);
			}

			return (totalValue, unit);
		}

		(ZDecimal Total, string Unit) MergePotentiallyIgnoringUnits(MeasureType type, RateableMeasureSet firstMeasure, List<RateableMeasureSet> otherMeasures)
		{
			var unit = firstMeasure.GetUnit(type);
			var total = firstMeasure.GetActual(type);

			foreach (var otherMeasure in otherMeasures)
			{
				var otherUnit = otherMeasure.GetUnit(type);
				var otherValue = otherMeasure.GetActual(type);

				if (otherUnit != unit)
				{
					// When the units are different we remove the unit so that the count is now
					// just the number rather than the number of something.
					unit = string.Empty;
				}

				total += otherValue;
			}

			return (total, unit);
		}

		void MergeContainers(RateableMeasureSet destinationMeasure, IEnumerable<RateableMeasureSet> allMeasures)
		{
			destinationMeasure.CreateContainerList(includeCommodity: false, includeContainerNumber: true, includeCartageLegPK: false);

			//Note: this assumes there are no duplicate containers. Not sure how I can detect that
			//Each of these 'allMeasures' originated from a Move, so they should be unique.
			var allContainers = allMeasures
				.Where(m => m.HasMeasureType(MeasureType.ContainerCount))
				.SelectMany(m => m.GetPartList(MeasureType.ContainerCount))
				.Cast<IRateableContainer>();

			destinationMeasure.AddContainers(allContainers);
		}

		static decimal Convert(decimal sourceValue, string sourceUnit, string targetUnit)
		{
			if (sourceUnit == targetUnit)
			{
				return sourceValue;
			}
			else if (Constants.Weight.ContainsCode(sourceUnit) && Constants.Weight.ContainsCode(targetUnit))
			{
				return Constants.Weight.Convert(sourceValue, sourceUnit, targetUnit);
			}
			else if (Constants.Volume.ContainsCode(sourceUnit) && Constants.Volume.ContainsCode(targetUnit))
			{
				return Constants.Volume.Convert(sourceValue, sourceUnit, targetUnit);
			}
			else
			{
				throw new NotImplementedException($"Unable to convert '{sourceUnit}' to {targetUnit}");
			}
		}

		static List<IAutoRating> GetAdaptersForCost(CommonCartage parent, IAutoRatingInteractor uiInteractor)
		{
			var list = new List<IAutoRating>();

			// no services adapters
			var ratings = parent
				.CartageLegs
				.Select(leg => new CartageLegRatingAdapter(leg, forWorkSheet: false, shouldAutorateServices: false));

			list.AddRange(ratings);

			// services only adapters
			list.AddRange(parent.BookedMovesCollection
				.Distinct()
				.Select(move => new CartageMoveRatingAdapter(move, move.PickupFromDocAddress, move.DeliverToDocAddress, true)));

			if (!list.Any())
			{
				uiInteractor.Warning(LogMessages.RatingAdaptersCannotBeCreated((NoResString)"No legs or services found"));
			}

			return list;
		}

		static List<IAutoRating> GetAdaptersForRevenue(CommonCartage parent, IAutoRatingInteractor uiInteractor)
		{
			var result = new List<IAutoRating>();

			foreach (CommonBookedCtgMove move in parent.BookedMovesCollection)
			{
				RatingAdapter moveAdapter = null;

				if (move.IsLoose && (parent.JJ_ContainerMode == Constants.CartageContainerMode.Loose || parent.JJ_ContainerMode == Constants.CartageContainerMode.Mixed))
				{
					moveAdapter = new CartageMoveRatingAdapter(move, move.PickupFromDocAddress, move.WaitPointDocAddress, false);
				}
				else if (parent.JJ_ContainerMode == Constants.CartageContainerMode.Containerized || parent.JJ_ContainerMode == Constants.CartageContainerMode.Mixed)
				{
					var addresses = new List<JobDocAddress>();
					if (move.PickupFromDocAddress != null)
					{
						addresses.Add(move.PickupFromDocAddress);
					}

					if (move.WaitPointDocAddress != null)
					{
						addresses.Add(move.WaitPointDocAddress);
					}

					if (move.DeliverToDocAddress != null)
					{
						addresses.Add(move.DeliverToDocAddress);
					}

					if (addresses.Count > 2)
					{
						foreach (JobDocAddress address in addresses.ToArray())
						{
							if (address.DocAddressType == DocAddressType.LocalCartageYard)
							{
								addresses.Remove(address);
							}
						}
					}

					if (addresses.Count > 0)
					{
						moveAdapter = new CartageMoveRatingAdapter(move, addresses[0], addresses[addresses.Count - 1], false);
					}
				}

				if (moveAdapter == null)
				{
					moveAdapter = new CartageMoveRatingAdapter(move, null, null, true);
				}
				result.Add(moveAdapter);

				foreach (CommonCartageLeg leg in move.CartageLegs)
				{
					if (!leg.JU_AdditionalService.IsEmpty)
					{
						result.Add(new CartageLegRatingAdapter(leg, forWorkSheet: false, shouldAutorateServices: true));
					}
				}
			}

			if (!result.Any())
			{
				uiInteractor.Warning(LogMessages.RatingAdaptersCannotBeCreated((NoResString)"No booked moves found"));
			}

			return result;
		}
	}
}
