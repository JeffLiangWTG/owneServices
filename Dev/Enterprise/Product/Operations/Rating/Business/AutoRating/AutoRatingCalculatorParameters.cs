using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public abstract class AutoRatingCalculatorParameters
	{
		protected AutoRatingCalculatorParameters(RatingCriteria criteria, FreightAutoRater autoRater)
		{
			AutoRater = Argument.NotNull(autoRater, nameof(autoRater));
			Criteria = Argument.NotNull(criteria, nameof(criteria));
			RatingContext = autoRater.RatingContext;
			Logger = RatingContext.Logger;
		}

		public ILogger Logger { get; }
		public IRatingContext RatingContext { get; }
		public RatingCriteria Criteria { get; }
		public FreightAutoRater AutoRater { get; }
		internal BusinessObjectFactory Factory => AutoRater.Factory;

		/// <summary>
		/// A list of charges just autorated.
		/// </summary>
		public abstract AutoRateInfoCollection Results { get; }
#if DEBUG
		public abstract void SetResults_ForTest(AutoRateInfoCollection results);
#endif

		public abstract List<FastLine> LinesToCalculate { get; }

		internal abstract ServiceAutoRater ServiceRater { get; }
		internal abstract CalculationOrderResolver CalculationOrderResolver { get; }

		protected abstract IAmountByLine AmountByLine { get; }

		/// <summary>
		/// For the parts that matched the line get all the unique product PKs.
		/// Used by WarehouseLocationTypeCalculator for building descriptions.
		/// </summary>
		public List<ZGuid> GetProductPKs(IRateLine line)
		{
			var parts = AmountByLine.GetPartsForLine(line);
			return parts.Select(x => x.Part.ProductPk)
				.Where(x => x.HasValue)
				.Distinct()
				.Select(x => (ZGuid)x.Value)
				.ToList();
		}

		public string GetWarehouseDescription(IRateLine line)
		{
			switch (line.TL_UnitFactor.ToString())
			{
				case UnitFactorList.Codes.ProductLine:
					return GetWarehouseProductLineDescription(line);
				case UnitFactorList.Codes.PackageLine:
					return GetWarehousePackageLineDescription(line);
				default:
					throw new InvalidOperationException($"Unit Factor '{line.TL_UnitFactor}'is not valid");
			}
		}

		string GetWarehouseProductLineDescription(IRateLine line)
		{
			var partWithDimensionsList = AmountByLine.GetPartsForLine(line);
			var stringBuilder = new StringBuilder();

			var partDict = new Dictionary<Guid, OrgSupplierPart>();
			foreach (var partWithDimensions in partWithDimensionsList)
			{
				var rateablePart = partWithDimensions.Part;
				if (rateablePart != null && rateablePart.ProductPk.HasValue)
				{
					var packUQ = rateablePart.PackageType;
					var partPK = rateablePart.ProductPk.Value;
					if (!partDict.TryGetValue(partPK, out var orgSupplierPart))
					{
						orgSupplierPart = Factory.Load<OrgSupplierPart>(partPK);
						partDict[partPK] = orgSupplierPart;
					}

					if (orgSupplierPart != null)
					{
						stringBuilder.Append($"{orgSupplierPart.OP_PartNum}({packUQ}),"); // No text
					}
				}
			}

			return stringBuilder.Length != 0
				? Res.GetString("D80C4259-80FB-4B54-B758-C1BD3FA01EA6", "for warehouse line/s: {0}", stringBuilder.ToString().Trim(','))
				: string.Empty;
		}

		string GetWarehousePackageLineDescription(IRateLine line)
		{
			var partWithDimensionsList = AmountByLine.GetPartsForLine(line);
			var stringBuilder = new StringBuilder();

			foreach (var partWithDimensions in partWithDimensionsList)
			{
				var rateablePart = partWithDimensions.Part;
				if (rateablePart != null)
				{
					stringBuilder.Append($"{rateablePart.PackageCount}x {rateablePart.PackageType},"); // No text
				}
			}

			return stringBuilder.Length != 0
				? Res.GetString("D7DDD171-85D4-4E9E-8D02-346BABB28467", "for warehouse package/s: {0}", stringBuilder.ToString().Trim(','))
				: string.Empty;
		}

		/// <summary>
		/// Get the containers from the parts for the line and MeasureType.
		/// </summary>
		/// <returns>not null</returns>
		public IEnumerable<IRateableContainer> GetContainers(MeasureType measureType, IRateLine line)
		{
			var parts = GetParts(measureType, line);
			return parts?.OfType<IRateableContainer>().ToList() ?? Enumerable.Empty<IRateableContainer>();
		}

		public IEnumerable<IRateablePart> GetParts(MeasureType measureType, IRateLine line)
		{
			var unitIsPackage = measureType == MeasureType.Unit || measureType == MeasureType.StorageUnit || measureType == MeasureType.WarehousePackage;
			return GetPackageFilteredPartsForMeasure(measureType, line, unitIsPackage ? line.TL_WeightVolume : null).Parts;
		}

		/// <summary>
		/// Package unit matching or converting logic.
		/// The parts for the line can be filtered on the given pkgUnit (introduced in WI00044783 TITAN - Rate by Pack Type)
		/// or the unit converted via product conversion factors (introduced probably 10 years or more ago for warehouse rating).
		/// However, for at least one of the cases (GetContainers) it would be more consistent
		/// to do the filtering before adding the line/part match to the AmountByLine table.
		/// There is no point adding the match in the first place, but then filtering it out later.
		/// </summary>
		struct PackageUnitInfo
		{
			public PackageUnitInfo(IRateLine line)
				: this(line, line.TL_WeightVolume)
			{ }

			public PackageUnitInfo(IRateLine line, string pkgUnit)
			{
				ShouldConvertViaProduct = false;
				ShouldFilter = false;
				PackageUnit = pkgUnit;
				if (!string.IsNullOrEmpty(pkgUnit))
				{
					var measureType = RatingCache.GetMeasureTypeFromUnit(line.TL_WeightVolume);
					if (measureType == MeasureType.Unit)
					{
						if (line.RateType() == RateType.Warehouse)
						{
							ShouldConvertViaProduct = true;
						}
						else
						{
							ShouldFilter = pkgUnit != QuantityUnit.PK;
						}
					}
				}
			}

			public string PackageUnitFilter => ShouldFilter ? PackageUnit : null;
			public string PackageUnitConvert => ShouldConvertViaProduct ? PackageUnit : null;

			bool ShouldConvertViaProduct { get; }
			bool ShouldFilter { get; }
			string PackageUnit { get; }
		}

		static decimal SumAmounts(MeasureType measureType, IEnumerable<IRateablePart> partList, MeasureAmountType amountType)
		{
			decimal result = 0;
			if (partList != null && partList.Any())
			{
				var measureInfo = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				var amountFunc = GetAmountFunc(measureInfo, amountType);
				foreach (var part in partList)
				{
					result += amountFunc(part);
				}
			}

			return result;
		}

		/// <summary>
		/// Some special logic for calculating a weight break amount when the line TL_UnitFactor is PacksWeight.
		/// Returns zero if the line is not a warehouse rate or if the pkgUnit is blank.
		///
		/// Note: PacksWeight option is only available if:
		/// - a registry is enabled
		/// - it's a client rate with warehouse type
		/// - its the combined calculator
		/// <see cref="RateLinesLookups.GetUnitFactors"/>
		///
		/// Note: The line unit must be a weight unit if PacksWeight is selected
		/// <see cref="ActualRateLinesValidation.CheckTL_UnitFactor"/>
		/// </summary>
		public Quantity GetPacksWeightForBreakSearch(IRateLine line, string pkgUnit)
		{
			decimal amount = 0;
			string packType = null;

			var isWarehouseAndHasPackageUnit = !string.IsNullOrEmpty(pkgUnit) && line.RateType() == RateType.Warehouse;
			if (isWarehouseAndHasPackageUnit)
			{
				var parts = AmountByLine.GetPartsForLineMeasure(MeasureType.Weight, line);
				if (parts != null)
				{
					// Different WhsOrderLines with same product and packType should not accumulate value.
					// So each combination of product and pack type is calculated once.
					foreach (var partsByProduct in parts.Where(x => x.Part.ProductPk.HasValue)
						.GroupBy(x => x.Part.ProductPk.Value))
					{
						var product = Factory.Load<OrgSupplierPart>(partsByProduct.Key);
						foreach (var partsByPackType in partsByProduct.GroupBy(x => x.Part.PackageType ?? string.Empty))
						{
							packType = partsByPackType.Key;
							if (product != null && (product.OP_Weight.IsEmpty || product.UnitConverter.Convertible(packType, pkgUnit)))
							{
								var packsWeight = product.UnitConverter.Convert(qty: 1, fromUQ: packType, toUQ: pkgUnit);
								amount += Utilities.Round(packsWeight, 4);
							}
							else
							{
								throw new Calculator.CalculationException(ErrorMessages.PacksWeightNonConvertiblePackType(packType));
							}
						}
					}
				}
			}

			var description = !string.IsNullOrEmpty(packType)
				? Res.GetString("a415eb59-d60d-45cd-bd9c-43c21f7e1a17", "{0}/{1} Packs Weight", pkgUnit, packType)
				: null;

			return new Quantity(amount, pkgUnit, description: description);
		}

		public decimal GetPivotBreakForSlidingCalculator(IRateLine line)
		{
			var partList = AmountByLine.GetPartsForLineMeasure(MeasureType.Weight, line)
				//In case of Cargoguide, system set measures using MeasureType.ContainerCount
				//and in case of CW1, we get it using MeasureType.Weight
				//and Usually, GetMeasureTypes(line).FirstOrDefault() returns correct Measure
				//However, it's not always true.
				//When there are setup where both CW1(commodity same as on container) and Cargoguide setup
				//then system finds parts using ContainerCount for CW1 rates and CG we find against MeasureType.Weight (reason: not known yet)
				//So, we are prioritizing MeasureType.Weight first and if we don't find then only we will look for MeasureType.ContainerCount
				//Test case - WiseRatesIntegrationBusinessTests.TestRatesServiceCombinedCalculator_ContainerPivotBreak_OverridesBreakValue_AIR_WithCW1Costs
				?? AmountByLine.GetPartsForLineMeasure(MeasureType.ContainerCount, line);

			return partList != null && partList.Any() ? partList.Max(x => x.Part.PivotBreak) : 0;
		}

		public IEnumerable<Quantity> GetChargeableByContainer(IRateLine line)
		{
			var parts = GetPartsForMeasure(MeasureType.Weight, line).Parts;

			if (parts != null)
			{
				return parts.GroupBy(x => x.ContainerPK).Select(g => GetChargeableAmountInternal(line, g));
			}

			return Enumerable.Empty<Quantity>();
		}

		/// <summary>
		///		Calculates chargeable for WarehousePackage, WarehousePackageWeight and WarehousePackageVolume measure types.
		///
		///		Required virtual for AssertGetChargeableAmount_WarehousePackageLine
		/// </summary>
		public virtual Quantity CalculateChargeableForWarehouse(IRateLine rateLine, string unit, MeasureType measureType)
		{
			var isWeight = QuantityExtensions.IsWeight(unit);
			var isVolume = QuantityExtensions.IsVolume(unit);

			if (isWeight || isVolume)
			{
				// Weight and Volume should share the same parts. So, we need to find them either by WarehousePackageWeight
				// or WarehousePackageVolume (depending on the unit) and then take weight and volume values from it.
				var parts = isWeight
					? GetPartsForMeasure(MeasureType.WarehousePackageWeight, rateLine)
					: GetPartsForMeasure(MeasureType.WarehousePackageVolume, rateLine);

				var weightAmount = parts.IsLineMatch
					? SumAmounts(MeasureType.WarehousePackageWeight, parts.Parts, MeasureAmountType.Actual)
					: 0;

				var volumeAmount = parts.IsLineMatch
					? SumAmounts(MeasureType.WarehousePackageVolume, parts.Parts, MeasureAmountType.Actual)
					: 0;

				// Cater for TL_ActualPercentage i.e. part weight and part volume
				var weightQuantity = new Quantity(weightAmount, RatingConstants.Units.KG);
				var volumeQuantity = new Quantity(volumeAmount, RatingConstants.Units.M3);
				return CalculateChargeable(rateLine, weightQuantity, volumeQuantity, targetUnit: unit);
			}
			else
			{
				var amount = AmountByLine.GetTotalMeasureValueForLineWithUnit(rateLine, measureType, unit);
				return new Quantity(amount, unit);
			}
		}

		public decimal GetTotalMeasureValueForLine(IRateLine rateLine, MeasureType measureType)
			=> AmountByLine.GetTotalMeasureValueForLine(rateLine, measureType);

		/// <summary>
		/// Get the amount for unit type (Box, Pallet, etc.) that matched the line (the combined measure value from each part)
		/// and return it as a quantity.
		/// </summary>
		public Quantity GetQuantity(MeasureType type, IRateLine line, string unit, bool unitIsPackage, bool useContainerReferences = false)
		{
			var packageUnitInfo = new PackageUnitInfo(line, unitIsPackage ? unit : null);
			(var parts, var isLineMatch) = GetPackageFilteredPartsForMeasure(type, line, packageUnitInfo.PackageUnitFilter);
			var amount = SumAmountFromParts(type, line, packageUnitInfo.PackageUnitConvert, parts, isLineMatch, MeasureAmountType.Actual);
			var reference = GetReferenceFromParts(parts, type, useContainerReferences);
			return new Quantity(amount, unit, reference: reference);
		}

		/// <summary>
		///	Get the amount that matched the line (the combined measure value from each part)
		///	and return it as a quantity.
		/// </summary>
		public Quantity GetQuantity(MeasureType type, IRateLine line)
		{
			var unit = Criteria.JobMeasures.GetUnit(type);

			if (unit.IsEmpty)
			{
				return Quantity.Empty($"No measure unit available for {type}");
			}

			var partList = AmountByLine.GetPartsForLineMeasure(type, line) ?? new List<PartWithDimensions>();
			var parts = partList.Select(p => p.Part).ToList();
			var amount = SumAmounts(type, parts, MeasureAmountType.Actual);
			var reference = GetReferenceFromParts(parts, type, false);

			return new Quantity(amount, unit, reference: reference);
		}

		decimal SumAmountFromParts(MeasureType type, IRateLine line, string packageUnitConvert, IEnumerable<IRateablePart> parts, bool isLineMatch, MeasureAmountType amountType)
		{
			decimal amount = 0;
			if (parts != null && parts.Any())
			{
				if (isLineMatch)
				{
					amount = SumConvertedAmountFromParts(type, line, packageUnitConvert, parts, amountType);
				}
				else
				{
					amount = SumAmounts(type, parts, amountType);
				}
			}

			return amount;
		}

		/// <summary>
		/// Get the parts for the given measure.
		/// If there are specific matches to the given line then return those,
		/// otherwise fallback to all the parts for the measure.
		/// </summary>
		(IEnumerable<IRateablePart> Parts, bool IsLineMatch) GetPartsForMeasure(MeasureType type, IRateLine line)
		{
			var partList = AmountByLine.GetPartsForLineMeasure(type, line);
			if (partList != null)
			{
				var parts = partList.Select(x => x.Part);
				return (parts, true);
			}
			else
			{
				var allParts = Criteria?.RateableMeasures.GetPartList(type);
				return (allParts, false);
			}
		}

		/// <summary>
		/// Get the parts for the given measure and PackageUnitInfo.
		/// If there are specific matches to the given line then return those,
		/// otherwise fallback to all the parts for the measure.
		/// </summary>
		/// <remarks>
		///		This logic looks hacky. If a line has a package unit (Box, Pallet, etc.), then  AmountByLine.GetPartsForLineMeasure
		///		should already filter parts based in this line unit, i.e. there must be PackageComparer implemented similar to CommodityComparer,
		///		ContainerTypeComparer. But for some reason, it wasn't implemented and we do extra filtering by package type here which looks hacky.
		///		This logic has to be revised.
		/// </remarks>
		(IEnumerable<IRateablePart> Parts, bool IsLineMatch) GetPackageFilteredPartsForMeasure(MeasureType type, IRateLine line, string packageUnitFilter)
		{
			var partList = AmountByLine.GetPartsForLineMeasure(type, line);
			if (partList != null)
			{
				var parts = partList.Select(x => x.Part);

				// PK package unit means Package, i.e. all packages regardless of a specific package type.
				// So, in case of PK we match all packages and don't apply the filter.
				if (!string.IsNullOrEmpty(packageUnitFilter) && packageUnitFilter != QuantityUnit.PK)
				{
					parts = parts.Where(x => x.PackageType == packageUnitFilter);
				}

				return (parts, true);
			}
			else
			{
				var allParts = Criteria?.RateableMeasures.GetPartList(type);
				return (allParts, false);
			}
		}

		string GetReferenceFromParts(IEnumerable<IRateablePart> parts, MeasureType type, bool useContainerReferences)
		{
			if (parts != null && parts.Any())
			{
				if (useContainerReferences)
				{
					return string.Join(", ", parts.OfType<IRateableContainer>()
						.Select(x => x.Reference)
						.Where(x => !string.IsNullOrEmpty(x))
						.Distinct());
				}
				else if (type == MeasureType.Package)
				{
					// Only packages have reference text
					return string.Join(",", parts.Select(x => x.PackageCountReference)
						.Where(x => !string.IsNullOrEmpty(x)));
				}
			}

			return string.Empty;
		}

		enum MeasureAmountType
		{
			Actual,
			ForClient,
			ForProvider
		}

		decimal SumConvertedAmountFromParts(MeasureType measureType, IRateLine line, string packageUnitConvert, IEnumerable<IRateablePart> parts, MeasureAmountType amountType)
		{
			if (!parts.Any())
			{
				return 0m;
			}

			var result = 0m;

			if (packageUnitConvert != null)
			{
				foreach (var partsByProduct in parts.Where(x => x.ProductPk.HasValue)
					.GroupBy(x => x.ProductPk.Value))
				{
					var product = Factory.Load<OrgSupplierPart>(partsByProduct.Key);
					if (product != null)
					{
						foreach (var part in partsByProduct)
						{
							result += GetProductAmount(product, part, packageUnitConvert);
						}
					}
				}
			}
			else
			{
				// Historical hack, yet to be explained...
				var shouldDoHackyRounding = measureType == MeasureType.LocationPallet;
				ZString baseUnitForHack = ZString.Empty;
				if (shouldDoHackyRounding)
				{
					baseUnitForHack = line.Calculator.GetUnits(this).FirstOrDefault();
				}

				var measureInfo = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				var amountFunc = GetAmountFunc(measureInfo, amountType);

				foreach (var part in parts)
				{
					var amount = amountFunc(part);
					if (shouldDoHackyRounding)
					{
						amount = line.Calculator.RoundAmount(new Quantity(amount, baseUnitForHack)).Amount;
					}

					result += amount;
				}
			}
			return result;
		}

		static Func<IRateablePart, decimal> GetAmountFunc(IPartMeasureInfo info, MeasureAmountType amountType)
		{
			if (amountType == MeasureAmountType.Actual)
			{
				return info.GetActualValue;
			}
			else if (amountType == MeasureAmountType.ForClient)
			{
				return info.GetClientValue;
			}
			else
			{
				return info.GetProviderValue;
			}
		}

		decimal GetProductAmount(OrgSupplierPart product, IRateablePart part, ZString pkgUnit)
		{
			var amount = part.UnitCount ?? 0;
			if (pkgUnit != product.OP_StockKeepingUnit && amount != 0)
			{
				if (product.UnitConverter.Convertible(product.OP_StockKeepingUnit, pkgUnit))
				{
					amount = product.UnitConverter.Convert(amount, product.OP_StockKeepingUnit, pkgUnit);
				}
				else
				{
					throw new Calculator.CalculationException(Res.GetString("0aa300e1-a5ac-4e7a-8635-e21c29d047d3", "no {0} to {1} unit conversion present on {2} ({3})", product.OP_StockKeepingUnit, pkgUnit, product.OP_PartNum, product.OP_Desc));
				}
			}

			return amount;
		}

		public virtual Quantity GetChargeableAmount(IRateLine line) => GetChargeableAmountInternal(line, default);

		bool RequiresChargeableFromJob(IRateLine line)
		{
			var requiresChargeableFromJob = RateLineHelper.RequiresChargeableFromJob(line);
			if (requiresChargeableFromJob)
			{
				// Jobs can override the "Chargeable" setting on a rate line with weight unit if they have any weights without volume.
				// ‘Gross Weight’ is used for calculation regardless Actual Weight/Volume is checked or NOT and which Rounding is chosen.
				var targetUnit = line.Calculator?.GetUnit(this) ?? ZString.Empty;
				if (!targetUnit.IsEmpty && QuantityUnit.IsWeight(targetUnit))
				{
					(var amount, var parts) = GetWeight(line);
					if (parts != null && parts.Any(x => x.IsWeightWithoutVolume))
					{
						requiresChargeableFromJob = false;
					}
				}
			}
			return requiresChargeableFromJob;
		}

		Quantity GetChargeableAmountInternal(IRateLine line, IEnumerable<IRateablePart> parts)
		{
			if (line.IsContainerSpotRate())
			{
				return GetChargeableContainerSpotRateAmount(line);
			}

			if (RequiresChargeableFromJob(line) && Criteria.JobMeasures.ContainsKey(MeasureType.Chargeable))
			{
				parts = GetPartsForMeasure(MeasureType.Chargeable, line).Parts;
				var amountType = _Rating.Cost ? MeasureAmountType.ForProvider : MeasureAmountType.ForClient;
				var amount = SumAmounts(MeasureType.Chargeable, parts, amountType);
				var unit = Criteria.JobMeasures.GetUnit(MeasureType.Chargeable);

				var matchesLineUnit =
					QuantityExtensions.IsWeight(unit) == QuantityExtensions.IsWeight(line.TL_WeightVolume) ||
					QuantityExtensions.IsVolume(unit) == QuantityExtensions.IsVolume(line.TL_WeightVolume);

				if (!matchesLineUnit)
				{
					throw new AutoRater.AutoRaterCalculationException(Res.GetString(
						"dbea3f78-b6d5-47c5-aeb2-d8dbf2e3062e",
						@"Charge code {0} uses the Chargeable from the job, but the units ({1}) specified on the job are different to the units specified in the rates ({2}).
The units should be the same for the Rounding option of 'Use Chargeable from Job' to work.",
						line.ChargeCode.AC_Code,
						unit,
						line.TL_WeightVolume));
				}

				return new Quantity(amount, unit);
			}

			return CalculateChargeable(line, parts: parts);
		}

		/// <summary>
		/// Similar to GetContainers() except this handles lines that are spot rates,
		/// by only returning containers with spot rates with the same PK as the spot rate entry containerPK <see cref="RateEntry.ContainerPKForSpotEntry>"
		/// </summary>
		public virtual IEnumerable<IRateableContainer> GetChargeableContainers(IRateLine line)
		{
			var containers = GetContainers(GetMeasureTypes(line).FirstOrDefault(), line);
			if (!line.IsContainerSpotRate())
			{
				return containers;
			}
			else
			{
				var pkToFind = line.ParentRateEntry.ContainerPKForSpotEntry;
				return containers.Where(x => x.ContainerSpotRates != null
						&& x.ContainerSpotRates.ContainerPK == pkToFind)
					.ToList();
			}
		}

		public ZString GetContainerNumberOrContainerTypeCodeWithCount(IRateLine rateLine)
		{
			if (ContainerNumberFilter.HasValue && !ContainerNumberFilter.Value.IsEmpty)
			{
				return ContainerNumberFilter.Value;
			}

			var container = rateLine?.ParentRateEntry?.Container;

			if (container == null)
			{
				return ZString.Empty;
			}

			if (Criteria.JobMeasures.HasContainerMeasure)
			{
				var containers = GetContainers(MeasureType.ContainerCount, rateLine);
				return ZString.Format("{0} ({1})", container.RC_Code, containers.Sum(x => x.ContainerCount));
			}

			return ZString.Empty;
		}

		public (IEnumerable<IRateableContainer> containers, ZString reference, bool shouldContainersBeEmpty) GetContainersForContainerServices(IRateLine line)
		{
			var (containers, reference) = ServiceRater.GetContainersForContainerServices(line);

			if (!ContainerNumberFilter.HasValue || !containers.Any())
			{
				return (containers, reference, false);
			}

			// If we have a ContainerNumberFilter to apply, we should only include the relevant container in calculations.
			// This is used in CTN unit factor where charges are invoiced per container, therefore ContainerNumberFilter will be the relevant container number.
			var filteredContainer = containers.FirstOrDefault(container => container.ContainerNumber.Equals(ContainerNumberFilter));

			if (filteredContainer == null)
			{
				return (Enumerable.Empty<IRateableContainer>(), reference, true);
			}

			return (new List<IRateableContainer> { filteredContainer }, filteredContainer.ContainerNumber, false);
		}

		public IEnumerable<JobServiceInfo> GetServicesBeingCalculated(IRateLine line)
		{
			var services = ServiceRater.GetServicesBeingCalculated(line);

			if (!ContainerNumberFilter.HasValue)
			{
				return services;
			}

			// If we have a ContainerNumberFilter to apply, we should only include the relevant services in calculations.
			// This is used in CTN unit factor where charges are invoiced per container, therefore ContainerNumberFilter will be the relevant container number.
			return services.Where(service => service.ContainerNumber.Equals(ContainerNumberFilter));
		}

		public int GetOccupiedContainerCount(IRateLine line, IRateableContainer container)
		{
			var entry = line.ParentRateEntry;

			RefContainer refContainer = null;

			if (entry.ContainerPayloadWeight == 0 || entry.ContainerPayloadVolume == 0)
			{
				refContainer = container.ContainerTypePk.HasValue
					? Factory.Load<RefContainer>(container.ContainerTypePk.Value)
					: entry.Container;
			}

			// We use payload weight from Rate Entry container then fallback to container on measures
			var payloadWeight = entry.ContainerPayloadWeight > 0
				? entry.ContainerPayloadWeight
				: refContainer?.RC_NetWeight ?? 0;

			// We use payload volume from Rate Entry container then fallback to container on measures
			var payloadVolume = entry.ContainerPayloadVolume > 0
				? entry.ContainerPayloadVolume
				: refContainer?.RC_CubicCapacity ?? 0;

			if (payloadWeight <= 0 && payloadVolume <= 0)
			{
				return 0;
			}

			var weight = line.UseOnlyActualWeightMeasure()
				? container.ContainerWeightInKG
				: (decimal)CalculateChargeable(
						line,
						weight: new Quantity(container.ContainerWeightInKG, Weight.Kilograms),
						volume: new Quantity(container.ContainerVolumeInM3, Volume.CubicMetres),
						targetUnit: Weight.Kilograms)
					.Amount;

			var occupiedCountBasedOnWeight = payloadWeight > 0
				? Math.Ceiling(weight / payloadWeight)
				: 0;

			var occupiedCountBasedOnVolume = payloadVolume > 0
				? Math.Ceiling(container.ContainerVolumeInM3 / payloadVolume)
				: 0;

			return (int)Math.Max(occupiedCountBasedOnWeight, occupiedCountBasedOnVolume);
		}

		/// <summary>
		/// Get the chargeable amount for a container spot rate line.
		/// This is the number of containers for the line that
		/// have a ContainerPK that matches the spot rate PK <see cref="RateEntry.ContainerPKForSpotEntry"/>.
		/// </summary>
		Quantity GetChargeableContainerSpotRateAmount(IRateLine line)
		{
			var parts = GetPartsForMeasure(GetMeasureTypes(line).FirstOrDefault(), line).Parts;
			var pkToFind = line.ParentRateEntry.ContainerPKForSpotEntry;
			var count = parts?.OfType<IRateableContainer>()
				.Count(x => x.ContainerSpotRates?.ContainerPK == pkToFind) ?? 0;
			return new Quantity(count, QuantityUnit.CN);
		}

		#region Chargeable Calculation

		internal abstract IRateEntry FreightLeg1Entry { get; }
		internal abstract IRateEntry FreightLeg2Entry { get; }

		public Quantity CalculateChargeable(IRateLine line, Quantity weight = default, Quantity volume = default, string targetUnit = default, IEnumerable<IRateablePart> parts = default)
		{
			if (line.ParentRateEntry.IsFreightEntry() || line.Uses(CalculatorType.Cartage) || line.Uses(CalculatorType.CartageZoneDistance))
			{
				return CalculateChargeableInternal(line, frtLine: null, weight, volume, targetUnit, parts);
			}
			else if (line.ParentRateEntry.IsDestinationEntry())
			{
				var frtLine = (FreightLeg2Entry ?? FreightLeg1Entry)?.ChildRateLines.FirstOrDefault();

				return CalculateChargeableInternal(line, frtLine, weight, volume, targetUnit, parts);
			}
			else
			{
				var frtLine = FreightLeg1Entry?.ChildRateLines.FirstOrDefault();

				return CalculateChargeableInternal(line, frtLine, weight, volume, targetUnit, parts);
			}
		}

		Quantity CalculateChargeableInternal(IRateLine line, IRateLine frtLine, Quantity specifiedWeight, Quantity specifiedVolume, string targetUnit, IEnumerable<IRateablePart> parts)
		{
			var factors = GetConversionFactors(line, frtLine, targetUnit);

			if (targetUnit == default)
			{
				targetUnit = GetTargetUnit(line);
			}

			var targetIsWeight = QuantityUnit.IsWeight(targetUnit);

			Quantity weight;
			if (specifiedWeight == default)
			{
				(var totalWeight, var weightParts) = GetWeight(line, parts);
				weight = totalWeight;
				if (targetIsWeight && weightParts != null && weightParts.Any(x => x.IsWeightWithoutVolume))
				{
					return CalculateChargeableWeightsWithoutVolume(line, factors, targetUnit, weightParts);
				}
			}
			else
			{
				weight = specifiedWeight;
			}
			var volume = specifiedVolume == default ? GetVolume(line, parts) : specifiedVolume;
			var loadingMeters = GetLoadingMeters(line, parts);

			var result = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Weight = weight,
				Volume = volume,
				LoadingLength = loadingMeters,
				TargetUnit = targetUnit,
				ConversionFactors = factors
			});

			var actualAmount = 0m;

			if (targetIsWeight)
			{
				actualAmount = result.Weight.Amount;
			}
			else if (QuantityUnit.IsVolume(targetUnit))
			{
				actualAmount = result.Volume.Amount;
			}
			else if (QuantityUnit.IsLoadingMeter(targetUnit))
			{
				actualAmount = result.LoadingLength.Amount;
			}

			var reference = weight.Reference.IsEmpty ? volume.Reference : weight.Reference;
			var chargeablePercent = 100 - line.TL_ActualPercentage;
			var actualPercent = line.TL_ActualPercentage;

			var amount = ((result.Chargeable.Amount * chargeablePercent) + (actualAmount * actualPercent)) / 100;
			return new Quantity(amount, targetUnit, reference: reference);
		}

		Quantity CalculateChargeableWeightsWithoutVolume(IRateLine line, IEnumerable<ConversionFactor> factors, string targetUnit, IEnumerable<IRateablePart> partList)
		{
			var amountType = _Rating.Cost ? MeasureAmountType.ForProvider : MeasureAmountType.ForClient;

			var weightMeasureType = GetWeightMeasureType(line);
			var weightUnit = Criteria.JobMeasures.GetUnit(weightMeasureType);
			var weightFunc = GetAmountFunc(PartMeasureInfoProvider.Instance.GetPartMeasureInfo(weightMeasureType), amountType);

			var volumeMeasureType = GetVolumeMeasureType(line);
			var volumeUnit = Criteria.JobMeasures.GetUnit(volumeMeasureType);
			var volumeFunc = GetAmountFunc(PartMeasureInfoProvider.Instance.GetPartMeasureInfo(volumeMeasureType), amountType);

			decimal totalWeightWithoutVolume = 0;
			decimal totalWeightWithVolume = 0;
			decimal totalVolume = 0;
			foreach (var part in partList)
			{
				if (part.IsWeightWithoutVolume)
				{
					totalWeightWithoutVolume += weightFunc(part);
				}
				else
				{
					totalWeightWithVolume += weightFunc(part);
					totalVolume += volumeFunc(part);
				}
			}

			var chargeableWithoutVolume = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Weight = new Quantity(totalWeightWithoutVolume, weightUnit),
				TargetUnit = targetUnit,
				ConversionFactors = factors
			});
			var finalWeightWithoutVolume = chargeableWithoutVolume.Weight.Amount;

			var chargeableWithVolume = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Weight = new Quantity(totalWeightWithVolume, weightUnit),
				Volume = new Quantity(totalVolume, volumeUnit),
				TargetUnit = targetUnit,
				ConversionFactors = factors
			});

			var chargeablePercent = 100 - line.TL_ActualPercentage;
			var actualPercent = line.TL_ActualPercentage;
			var finalWeightWithVolume = ((chargeableWithVolume.Chargeable.Amount * chargeablePercent) + (chargeableWithVolume.Weight.Amount * actualPercent)) / 100;

			return new Quantity(finalWeightWithoutVolume + finalWeightWithVolume, targetUnit);
		}

		IEnumerable<ConversionFactor> GetConversionFactors(IRateLine line, IRateLine frtLine, string targetUnit)
		{
			var factorsByPriority = new List<ConversionFactor>();
			var invalidConversionFactorTemplate = (NoResString)"Invalid Conversion Factor '{0}' on RateLine '{1}' has been replaced with default Conversion Factor '{2}'";  // log message, subject to change, more for support people as of now

			ConversionFactor targetUnitConversionFactor = default;

			if (!string.IsNullOrEmpty(targetUnit) && targetUnit != line.TL_WeightVolume)
			{
				targetUnitConversionFactor = line.GetDefaultConversionFactor(targetUnit);
			}

			var lineConversionFactor = line.ConversionFactor;
			if (!line.ConversionFactor.IsEmpty && !line.ConversionFactor.IsValid)
			{
				Logger.Warning(ZString.Format(invalidConversionFactorTemplate, lineConversionFactor, line.DisplayInfo(), line.GetDefaultConversionFactor(line.TL_WeightVolume)));
				lineConversionFactor = line.GetDefaultConversionFactor(line.TL_WeightVolume);
			}

			var frtLineConversionFactor = frtLine?.ConversionFactor ?? ConversionFactor.Empty;
			if (frtLine != null && !frtLineConversionFactor.IsEmpty && !frtLineConversionFactor.IsValid)
			{
				Logger.Warning(ZString.Format(invalidConversionFactorTemplate, frtLineConversionFactor, frtLine.DisplayInfo(), frtLine.GetDefaultConversionFactor(frtLine.TL_WeightVolume)));
				frtLineConversionFactor = frtLine.GetDefaultConversionFactor(frtLine.TL_WeightVolume);
			}

			factorsByPriority.Add(lineConversionFactor);
			factorsByPriority.Add(targetUnitConversionFactor);
			factorsByPriority.Add(frtLineConversionFactor);
			factorsByPriority.Add(GetChargeableConversionFactor(line));

			if (Criteria.IsRoadFreight && FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value)
			{
				factorsByPriority.Add(new ConversionFactor(FreightDataRegistry.Instance.RoadLoadingMetersWeightPerLDM.Value, Weight.Kilograms, LoadingLength.LoadingMeters));
			}

			var factors = new List<ConversionFactor>();

			foreach (var factor in factorsByPriority.Where(f => !f.IsEmpty))
			{
				if (!factors.Any(f => EqualByMesurmentTypeRelations(f, factor)))
				{
					factors.Add(factor);
				}
			}

			return factors;
		}

		bool EqualByMesurmentTypeRelations(ConversionFactor factor1, ConversionFactor factor2)
		{
			return factor1.NumeratorMeasureType == factor2.NumeratorMeasureType && factor1.DenominatorMeasureType == factor2.DenominatorMeasureType
				|| factor1.NumeratorMeasureType == factor2.DenominatorMeasureType && factor1.DenominatorMeasureType == factor2.NumeratorMeasureType;
		}

		ZString GetTargetUnit(IRateLine line)
		{
			var targetUnit = line.Calculator?.GetUnit(this) ?? ZString.Empty;

			if (targetUnit.IsEmpty)
			{
				var weight = GetWeight(line).Amount;
				var volume = GetVolume(line);

				return ChargeableAmountCalculator.GetChargeableUnit(GetGenericTransportMode(), weight.Unit, volume.Unit);
			}

			if (targetUnit == Constants.Volume.TeaChest)
			{
				return Constants.Volume.CubicMetres;
			}

			return targetUnit;
		}

		ConversionFactor GetChargeableConversionFactor(IRateLine line)
		{
			if (line == null)
			{
				return ConversionFactor.Empty;
			}

			string targetUnit = GetTargetUnit(line);

			var factor = ChargeableFactor.GetDefault(GetChargeableFactorSource(Criteria), GetGenericTransportMode());
			if (factor != null)
			{
				var targetUnitIsImperial = Constants.Weight.IsImperial(targetUnit) || Constants.Volume.IsImperial(targetUnit);
				return targetUnitIsImperial ? factor.ImperialFactor : factor.MetricFactor;
			}

			return ConversionFactor.Empty;

			ChargeableFactorSource GetChargeableFactorSource(RatingCriteria criteria)
			{
				if (Criteria.IsDomestic())
				{
					return ChargeableFactorSource.Domestic;
				}
				else if (criteria.IsWarehouseHandling || criteria.IsWarehouseStorage)
				{
					return ChargeableFactorSource.Warehouse;
				}
				else if (criteria.IsTransitTransportationUnit)
				{
					return ChargeableFactorSource.TransitWarehouse;
				}
				else if (criteria.IsTransportBooking)
				{
					return ChargeableFactorSource.TransportBooking;
				}
				else
				{
					return ChargeableFactorSource.International;
				}
			}
		}

		internal (Quantity Amount, IEnumerable<IRateablePart> Parts) GetWeight(IRateLine line, IEnumerable<IRateablePart> parts = default)
		{
			var measureType = GetWeightMeasureType(line);

			var unit = Criteria.JobMeasures.GetUnit(measureType);

			if (!string.IsNullOrWhiteSpace(unit))
			{
				return GetWeightOrVolume(_Rating.Cost, measureType, line, unit, parts);
			}
			else
			{
				return (default, parts);
			}
		}

		(Quantity Amount, IEnumerable<IRateablePart> Parts) GetWeightOrVolume(bool forProvider, MeasureType measureType, IRateLine line, string unit, IEnumerable<IRateablePart> parts)
		{
			Quantity value;

			if (TryGetNoApportionmentWeightOrVolume(forProvider, Criteria, line, measureType, unit, out value))
			{
				return (value, null);
			}

			if (parts == default)
			{
				parts = GetPartsForMeasure(measureType, line).Parts;
			}

			var amountType = forProvider ? MeasureAmountType.ForProvider : MeasureAmountType.ForClient;
			var amount = SumAmounts(measureType, parts, amountType);
			// Not sure if references are needed here, but just in case...
			var reference = GetReferenceFromParts(parts, measureType, useContainerReferences: false);
			return (new Quantity(amount, unit, reference: reference), parts);
		}

		static bool IsNoApportionmentRateLine(IRateLine rateLine) =>
			(rateLine.Uses(CalculatorType.Unit) && rateLine.TL_WeightVolume.ToString().In(QuantityUnit.KG, QuantityUnit.M3))
			|| rateLine.Uses(CalculatorType.Combined);

		static bool TryGetNoApportionmentWeightOrVolume(bool forProvider, RatingCriteria criteria, IRateLine line, MeasureType measureType, string unit, out Quantity quantity)
		{
			if (criteria.AutoRating is AutoRatingProxy autoRatingProxy
				&& autoRatingProxy.AutoRating is ConsolLeadShipmentRatingAdapter consolLeadAdapter
				&&
				(
					line.TL_UnitFactor == UnitFactorList.Codes.BCN
					||
					line.TL_UnitFactor == UnitFactorList.Codes.SCN
				)
				&& IsNoApportionmentRateLine(line))
			{
				var thisShipment = consolLeadAdapter.ThisShipment;
				if (thisShipment != null)
				{
					// This may need optimizing - getting all the measures every time just to get the weight/volume is inefficient.
					// Depends how many charges are using this BCN logic on a consol. Possible there is usually only one so not worth it.
					var measures = (RateableMeasureSet)thisShipment.RatingAdapter.RateableMeasures;
					var amount = forProvider
						? measures.GetForProvider(measureType)
						: measures.GetForClient(measureType);
					// It seems we are assuming the job weight is in KG and the volume is in M3, so no unit conversion is needed.
					quantity = new Quantity(amount, unit);
					return true;
				}
			}

			quantity = default;
			return false;
		}

		internal Quantity GetVolume(IRateLine line, IEnumerable<IRateablePart> parts = default)
		{
			var measureType = GetVolumeMeasureType(line);

			var unit = Criteria.JobMeasures.GetUnit(measureType);

			Quantity volume;
			if (!string.IsNullOrWhiteSpace(unit))
			{
				if (line.TL_WeightVolume == Constants.Volume.TeaChest && Criteria.PackageInformation != null)
				{
					var vol = 0m;
					foreach (var info in Criteria.PackageInformation)
					{
						vol += info.Volume;
					}
					volume = new Quantity(vol, unit);
				}
				else
				{
					volume = GetWeightOrVolume(_Rating.Cost, measureType, line, unit, parts).Amount;
				}
			}
			else
			{
				volume = default;
			}

			return volume;
		}

		Quantity GetLoadingMeters(IRateLine line, IEnumerable<IRateablePart> parts = default)
		{
			if (GetGenericTransportMode() != TransportModes.Road)
			{
				return default(Quantity);
			}

			if (parts == default)
			{
				parts = GetPartsForMeasure(MeasureType.LoadingMeters, line).Parts;
			}

			var amountType = _Rating.Cost ? MeasureAmountType.ForProvider : MeasureAmountType.ForClient;
			var amount = SumAmounts(MeasureType.LoadingMeters, parts, amountType);
			return new Quantity(amount, LoadingLength.LoadingMeters);
		}

		string GetGenericTransportMode()
		{
			var transportMode = "";
			if (Criteria.IsAirFreight)
			{
				transportMode = Constants.TransportModes.Air;
			}
			else if (Criteria.IsRailFreight)
			{
				transportMode = Constants.TransportModes.Rail;
			}
			else if (Criteria.IsSeaFreight)
			{
				transportMode = Constants.TransportModes.Sea;
			}
			else if (Criteria.IsRoadFreight)
			{
				transportMode = Constants.TransportModes.Road;
			}
			else if (Criteria.IsWarehouseStorage)
			{
				transportMode = Constants.TransportModes.Storage;
			}
			else if (Criteria.IsWarehouseHandling)
			{
				transportMode = Constants.TransportModes.WarehouseHandling;
			}
			else
			{
				transportMode = Constants.TransportModes.Air;
			}

			return transportMode;
		}

		#endregion

		#region GetMeasureType

		public IEnumerable<MeasureType> GetMeasureTypes(FastLine line) => GetMeasureTypes(line.Line);

		public IEnumerable<MeasureType> GetMeasureTypes(IRateLine line)
		{
			var measureTypes = new List<MeasureType>();

			foreach (var unit in line.Calculator.GetUnits(this))
			{
				var measureType = RatingCache.GetMeasureTypeFromUnit(unit);

				// Weight (like volume, packages, units) can come from different set of parts. So, we need to identify
				// the exact measure type (Weight, StorageWeight, InnerPacksWeight, etc.) based on the context and line attributes
				// to know which weight to get.

				switch (measureType)
				{
					case MeasureType.Weight:
						measureType = GetWeightMeasureType(line);
						break;

					case MeasureType.Volume:
						measureType = GetVolumeMeasureType(line);
						break;

					case MeasureType.Package:
						measureType = GetPackageMeasureType(line);
						break;

					case MeasureType.Unit:
						measureType = GetUnitMeasureType(line, unit);
						break;

					case MeasureType.Unidentified:
						if (line.TL_UnitFactor == UnitFactorList.Codes.PackageLine)
						{
							// It is a hack for Flat calculator with PackageLine unit factor.
							//
							// Normally, a flat value is applied once to all measures. This is what actually Unidentified measure type does.
							// But in case of PackageLine unit factor, we want it to be applied to all packages.
							//
							// For example, if a job has the following packages:
							//	50 BOX
							//	25 PLT
							//  Flat Calculator is $100
							//  We want $100 to be applied twice - once for 50 BOX and once for 25 PLT.
							//
							// Specifying non Unidentified measure type will help to achieve it.
							measureType = MeasureType.WarehousePackage;
						}
						break;
				}

				measureTypes.Add(measureType);
			}

			if (measureTypes.Count == 0)
			{
				measureTypes.Add(MeasureType.Unidentified);
			}

			return measureTypes;
		}

		static bool IsWhsStorageChargeCode(AccChargeCode chargeCode)
		{
			return chargeCode != null
				&& chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.Storage
				&& (chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSOutwards || chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.WHSInwards);
		}

		static MeasureType GetWeightMeasureType(IRateLine line)
		{
			if (line.TL_IsWhsJobLevelCharge)
			{
				return MeasureType.JobWeight;
			}

			if (IsWhsStorageChargeCode(line.ChargeCode))
			{
				return MeasureType.StorageWeight;
			}

			if (line.TL_UnitFactor == UnitFactorList.Codes.PackageLine)
			{
				return MeasureType.WarehousePackageWeight;
			}

			if (line.TL_UnitFactor == UnitFactorList.Codes.InnerPack)
			{
				return MeasureType.InnerPacksWeight;
			}

			return MeasureType.Weight;
		}

		static MeasureType GetVolumeMeasureType(IRateLine line)
		{
			if (line.TL_IsWhsJobLevelCharge)
			{
				return MeasureType.JobVolume;
			}

			if (IsWhsStorageChargeCode(line.ChargeCode))
			{
				return MeasureType.StorageVolume;
			}

			if (line.TL_UnitFactor == UnitFactorList.Codes.PackageLine)
			{
				return MeasureType.WarehousePackageVolume;
			}

			if (line.TL_UnitFactor == UnitFactorList.Codes.InnerPack)
			{
				return MeasureType.InnerPacksVolume;
			}

			return MeasureType.Volume;
		}

		static MeasureType GetPackageMeasureType(IRateLine line)
		{
			if (line.TL_UnitFactor == UnitFactorList.Codes.PackageLine)
			{
				return MeasureType.WarehousePackage;
			}

			if (line.TL_UnitFactor == UnitFactorList.Codes.InnerPack)
			{
				return MeasureType.InnerPacksPackage;
			}

			return MeasureType.Package;
		}

		static MeasureType GetUnitMeasureType(IRateLine line, string unit)
		{
			if (line.TL_IsWhsJobLevelCharge)
			{
				if (unit == PkgUnit.Pallet)
				{
					return MeasureType.ChargeablePallet;
				}

				if (unit == PkgUnit.Unit)
				{
					return MeasureType.JobUnit;
				}

				// It is a hack. For some reson, warehouse stuff uses units as Package measrue type.
				// It must be Unit.
				return MeasureType.Package;
			}

			// For some reason we use units as warehouse package in case of PackageLine unit factor.
			// It must be something like WarehouseUnit. Anyway, keeping it as it is.
			if (line.TL_UnitFactor == UnitFactorList.Codes.PackageLine)
			{
				return MeasureType.WarehousePackage;
			}

			if (IsWhsStorageChargeCode(line.ChargeCode))
			{
				return MeasureType.StorageUnit;
			}

			if (line.TL_UnitFactor == UnitFactorList.Codes.InnerPack)
			{
				return MeasureType.InnerPacksUnit;
			}

			return MeasureType.Unit;
		}

		#endregion

		#region Filters

		public abstract OrgSupplierPart ProductFilter { get; }
		public abstract ProductAttributesMeasure ProductAttributesFilter { get; }
		public abstract LocationMeasure LocationFilter { get; }
		public abstract ZString DocketReferenceFilter { get; }
		public abstract ZGuid CartageLegPKFilter { get; }
		/// <summary>
		/// Null if it should not be applied. If it contains a value, the filter should be applied on the container number.
		/// The other filters must be checked if they need to follow the same logic.
		/// </summary>
		public abstract ZString? ContainerNumberFilter { get; }
		public abstract RefContainer ContainerTypeFilter { get; }

		#endregion

		/// <summary>
		/// RateEntryAdapter by definition has similar lines, so no need to try and remove them.
		/// Its measures are constructed from a rate entry, so they must match.
		/// </summary>
		protected bool NeedRemoveSimilarCharges
			=> !(Criteria.AutoRating is RateEntryAdapter);

		internal void RemoveSimilarCharges(RateLinesRepository rateLinesRepository)
		{
			if (!NeedRemoveSimilarCharges)
			{
				return;
			}

			RemoveSimilarChargesCore(rateLinesRepository);
		}

		protected abstract void RemoveSimilarChargesCore(RateLinesRepository rateLinesRepository);

		/// <summary>
		/// Add a match for the given MeasureType between the given line and the part with given index.
		/// </summary>
		/// <param name="partIndex">the index of the part in the list of parts for the measureType. The first part added will have index zero.</param>
		internal void AddLineMeasureMatch(MeasureType measureType, IRateLine line, int partIndex)
		{
			var part = Criteria.JobMeasures.Measures.GetPartList(measureType)[partIndex];
			AmountByLine.AddAmount(measureType, line, part);
		}
	}

	/// <summary>
	/// AutoRatingCalculatorParameters for an entire job, with no filtering.
	/// Used for those jobs with measures that are not filtered into groups of parts.
	/// This is the type that is first contructed during a rating run.
	/// For some jobs it may then be used to split into multiple AutoRatingCalculatorParametersWithFilter instances.
	/// </summary>
	public class AutoRatingCalculatorParametersWithoutFilter : AutoRatingCalculatorParameters
	{
		public AutoRatingCalculatorParametersWithoutFilter(RatingCriteria criteria, FreightAutoRater autoRater, bool? isRatingCost)
			: base(criteria, autoRater)
		{
			ServiceRater = new ServiceAutoRater(criteria, autoRater.Factory, isRatingCost);
		}

		public AutoRatingCalculatorParametersWithoutFilter(RatingCriteria criteria, FreightAutoRater autoRater)
			: this(criteria, autoRater, null)
		{
		}

		internal override ServiceAutoRater ServiceRater { get; }

		protected override IAmountByLine AmountByLine
			=> amountByLine ?? (amountByLine = new AmountByLineTable(this));
		AmountByLineTable amountByLine;

		#region Results

		public override AutoRateInfoCollection Results
			=> resultsLazy ?? (resultsLazy = new AutoRateInfoCollection(Factory));
		AutoRateInfoCollection resultsLazy;

#if DEBUG
		public override void SetResults_ForTest(AutoRateInfoCollection results)
		{
			resultsLazy = results;
		}
#endif

		#endregion

		#region Lines to Calculate

		public override List<FastLine> LinesToCalculate
			=> linesToCalculateLazy ?? (linesToCalculateLazy = new List<FastLine>());
		List<FastLine> linesToCalculateLazy;

		internal void SetLinesToCalculate(RateLinesRepository rateLinesRepository)
		{
			foreach (var fastLine in rateLinesRepository.GetLines())
			{
				LinesToCalculate.Add(fastLine);
			}

			var entryPKs = new HashSet<ZGuid>();
			var headerPKs = new HashSet<ZGuid>();

			var nonZeroLines = LinesToCalculate.Select(x => x.Line).OfType<RateLine>().Where(x => x != null && x.Header != null).ToList();

			entryPKs.UnionWith(nonZeroLines.Select(x => x.TL_TI));
			headerPKs.UnionWith(nonZeroLines.Select(x => x.Header.PK));

			var cancellation = CheckEntriesSecurity(entryPKs.ToArray(), headerPKs.ToArray(), Factory);

			if (cancellation != null)
			{
				LinesToCalculate.Clear();
				Results.CancelAutoRating(cancellation);
				return;
			}

			calculationOrderResolverLazy = new CalculationOrderResolver(this, sort: true);
		}

		static AutoRatingCancellation CheckEntriesSecurity(ZGuid[] entryPKs, ZGuid[] headerPKs, BusinessObjectFactory factory)
		{
			var result = RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(entryPKs, headerPKs, factory);
			if (result != null)
			{
				var message = Res.GetString("262effc8-64af-4eb1-b3c4-c928219ee85f", "AutoRating has encountered {0} rates for {1}.{2}{3}",
					result.OrgCompanyData.OB_RateSecurityGroup,
					result.OrgCompanyData.Organisation.OH_Code,
					System.Environment.NewLine,
					result.SecurityCheckPoint.ErrorMessageForNotAllowed);

				return new AutoRatingCancellation(AutoRatingCancellation.Reasons.RatesSecurity, message);
			}

			return null;
		}

		#endregion

		internal override CalculationOrderResolver CalculationOrderResolver
			=> calculationOrderResolverLazy ?? (calculationOrderResolverLazy = new CalculationOrderResolver(this, sort: false));
		CalculationOrderResolver calculationOrderResolverLazy;

		#region Multiple Legs

		public void SetFreightEntries(IRateEntry freightLeg1EntryToSet, IRateEntry freightLeg2EntryToSet)
		{
			freightLeg1Entry = freightLeg1EntryToSet;
			freightLeg2Entry = freightLeg2EntryToSet;
		}

		internal override IRateEntry FreightLeg1Entry => freightLeg1Entry;
		internal override IRateEntry FreightLeg2Entry => freightLeg2Entry;
		IRateEntry freightLeg1Entry;
		IRateEntry freightLeg2Entry;

		#endregion

		#region Matching Line to Measures

		protected override void RemoveSimilarChargesCore(RateLinesRepository rateLinesRepository)
			=> RemoveSimilarChargesInternal(rateLinesRepository, updateAmountByLine: false);

		internal void RemoveSimilarChargesAndBuildLineMeasureMatches(RateLinesRepository rateLinesRepository)
		{
			if (NeedRemoveSimilarCharges)
			{
				RemoveSimilarChargesInternal(rateLinesRepository, updateAmountByLine: true);
			}
		}

		void RemoveSimilarChargesInternal(RateLinesRepository rateLinesRepository, bool updateAmountByLine)
		{
			new LineMeasureMatcher(Criteria)
				.RemoveSimilarCharges(this, rateLinesRepository, updateAmountByLine ? AmountByLine : null);
		}

		#endregion

		#region Creating Filter

		/// <summary>
		/// If needed, splits this AutoRatingCalculatorParameters into one for each group of parts to be calculated.
		/// Some jobs are calculated in separate groups of parts.
		/// The split depends on the job type and rate line attributes.
		/// For example, warehouse jobs calculate each docket separately.
		/// The rate line TL_UnitFactor can also affect the the split, making it specific to the given line.
		/// Each new params is then passed to the line calculator in turn, and this original is not used.
		/// If no split is needed, this original instance is used.
		/// </summary>
		/// <returns>IsSplit true if a split was need and the SplitParams should be used instead of this</returns>
		internal (bool IsSplit, IEnumerable<AutoRatingCalculatorParametersWithFilter> SplitParams) CreatedFilteredParametersIfNeeded(FastLine fastLine)
		{
			var line = fastLine.Line;
			var partFilter = GetPartFilterForLine(this, line, Criteria);

			if (partFilter != null)
			{
				var parts = AmountByLine.GetPartsForLine(line);
				var result = new List<AutoRatingCalculatorParametersWithFilter>();
				foreach (var partGroup in parts
					.Where(x => partFilter.HasAnyDimensions(x.HasDimensions))
					.GroupBy(x => x, partFilter))
				{
					var filter = new AmountByLineTableFilter(partFilter, partGroup.Key);
					var newParams = CreatedFilteredParameters(filter);
					result.Add(newParams);
				}

				if (result.Count > 0)
				{
					return (true, result);
				}
			}

			return (false, null);
		}

		/// <summary>
		/// Create a single filtered parameters for a single filter.
		/// </summary>
		AutoRatingCalculatorParametersWithFilter CreatedFilteredParameters(AmountByLineTableFilter filter)
			=> new AutoRatingCalculatorParametersWithFilter(this, AmountByLine, filter);

#if DEBUG
		public AutoRatingCalculatorParametersWithFilter CreatedFilteredParametersByContainer_ForTest(ZString containerNumber)
		{
			foreach (MeasureType measureType in Enum.GetValues(typeof(MeasureType)))
			{
				var partList = Criteria.RateableMeasures.GetPartList(measureType);
				if (partList != null && partList.HasContainerNumber)
				{
					for (int i = 0; i < partList.Count; ++i)
					{
						var part = partList[i];
						if (part.ContainerNumber == containerNumber)
						{
							var filter = new AmountByLineTableFilter(PartFilterProvider.CreateForLocalTransport(),
								new PartWithDimensions(partList, part));
							return CreatedFilteredParameters(filter);
						}
					}
				}
			}
			throw new InvalidOperationException("test needs to create a measure part with a matching value");
		}

		public AutoRatingCalculatorParametersWithFilter CreatedFilteredParametersByCartageLeg_ForTest(ZGuid cartageLegPK)
		{
			foreach (MeasureType measureType in Enum.GetValues(typeof(MeasureType)))
			{
				var partList = Criteria.RateableMeasures.GetPartList(measureType);
				if (partList != null && partList.HasCartageLegPK)
				{
					for (int i = 0; i < partList.Count; ++i)
					{
						var part = partList[i];
						if (part.CartageLegPK == cartageLegPK)
						{
							var filter = new AmountByLineTableFilter(PartFilterProvider.CreateForLocalTransport(),
								new PartWithDimensions(partList, part));
							return CreatedFilteredParameters(filter);
						}
					}
				}
			}
			throw new InvalidOperationException("test needs to create a measure part with a matching value");
		}

		public AutoRatingCalculatorParametersWithFilter CreatedFilteredParametersByDocket_ForTest(ZString docketRef)
		{
			foreach (MeasureType measureType in Enum.GetValues(typeof(MeasureType)))
			{
				var partList = Criteria.RateableMeasures.GetPartList(measureType);
				if (partList != null && partList.HasDocketReference)
				{
					for (int i = 0; i < partList.Count; ++i)
					{
						var part = partList[i];
						if (part.DocketReference == docketRef)
						{
							var filter = new AmountByLineTableFilter(PartFilterProvider.CreateForDocket(),
								new PartWithDimensions(partList, part));
							return CreatedFilteredParameters(filter);
						}
					}
				}
			}
			throw new InvalidOperationException("test needs to create a measure part with a matching value");
		}

		public AutoRatingCalculatorParametersWithFilter CreatedFilteredParameters_ForTest(LocationMeasure locationInfo)
		{
			foreach (MeasureType measureType in Enum.GetValues(typeof(MeasureType)))
			{
				var partList = Criteria.RateableMeasures.GetPartList(measureType);
				if (partList != null && partList.HasLocation)
				{
					for (int i = 0; i < partList.Count; ++i)
					{
						var part = partList[i];
						if (Equals(part.Location, locationInfo))
						{
							var filter = new AmountByLineTableFilter(PartFilterProvider.CreateForLocation(),
								new PartWithDimensions(partList, part));
							return CreatedFilteredParameters(filter);
						}
					}
				}
			}

			throw new InvalidOperationException("test needs to create a measure part with a matching value");
		}

		public AutoRatingCalculatorParametersWithFilter CreatedFilteredParameters_ForTest(OrgSupplierPart product)
		{
			var partList = Criteria.RateableMeasures.GetPartList(MeasureType.Unit);
			for (int i = 0; i < partList.Count; ++i)
			{
				var part = partList[i];
				if (part.ProductPk == product.PK.ToGuid())
				{
					var filter = new AmountByLineTableFilter(PartFilterProvider.CreateForDocketProduct(),
						new PartWithDimensions(partList, part));
					return CreatedFilteredParameters(filter);
				}
			}

			return null;
		}

#endif

		static IPartFilter GetPartFilterForLine(AutoRatingCalculatorParameters parameters, IRateLine line, RatingCriteria criteria)
		{
			IPartFilter result = null;

			if (IsWarehouseRating(criteria))
			{
				result = GetWarehousePartFilterForLine(parameters, line);
			}
			else if (IsLocalTransportRating(criteria))
			{
				result = PartFilterProvider.CreateForLocalTransport();
			}
			else if (IsUnitFactorPerContainer(line))
			{
				result = PartFilterProvider.CreateForContainer();
			}

			return result;
		}

		public static bool IsUsingFilter(AutoRatingProxy autoRatingInfo)
			=> IsWarehouseRating(autoRatingInfo) || IsLocalTransportRating(autoRatingInfo);

		static bool IsWarehouseRating(AutoRatingProxy autoRatingInfo)
		{
			return autoRatingInfo.ConsumerType == JobInvoicingConsumerTypes.WarehouseStorage ||
					 autoRatingInfo.ConsumerType == JobInvoicingConsumerTypes.WarehouseInwards ||
					 autoRatingInfo.ConsumerType == JobInvoicingConsumerTypes.WarehouseStocktake ||
					 autoRatingInfo.ConsumerType == JobInvoicingConsumerTypes.WarehouseOutwards ||
					 autoRatingInfo.ConsumerType == JobInvoicingConsumerTypes.WarehouseAdHocServiceJob;
		}

		static bool IsLocalTransportRating(AutoRatingProxy autoRatingInfo)
		{
			return autoRatingInfo.ConsumerType == JobInvoicingConsumerTypes.LocalCartage;
		}

		static bool IsUnitFactorPerContainer(IRateLine rateLine)
			=> rateLine.TL_UnitFactor == UnitFactorList.Codes.CTN;

		static IPartFilter GetWarehousePartFilterForLine(AutoRatingCalculatorParameters parameters, IRateLine line)
		{
			var baseLine = line.Calculator.GetBaseCalculator(parameters).Line;

			if (baseLine.Uses(CalculatorType.WarehouseLocationType))
			{
				return PartFilterProvider.CreateForLocation();
			}
			else if (
				(
					baseLine.RequiresWeightVolume()
					|| !string.IsNullOrEmpty(baseLine.Calculator.DefaultWeightVolume)
					|| (baseLine.TL_UnitFactor == UnitFactorList.Codes.PackageLine && line.Calculator.SupportsPackageLineUnitFactor)
					|| (baseLine.TL_UnitFactor == UnitFactorList.Codes.ProductLine && line.Calculator.SupportsProductLineUnitFactor)
				)
				&& baseLine.TL_WeightVolume != QuantityUnit.SV) // Not Per Service rate line. Tested in Warehouse by TestAutoRateJobHeader_RateServices().
			{
				switch (baseLine.TL_UnitFactor.ToString())
				{
					case UnitFactorList.Codes.PacksWeight:
						return PartFilterProvider.CreateForDocketProductPackageType();
					case UnitFactorList.Codes.PackageLine:
						return PartFilterProvider.CreateForDocketPackage();
					case UnitFactorList.Codes.ProductLine:
						return PartFilterProvider.CreateForDocketProductLine();
					default:
						return PartFilterProvider.CreateForDocketProduct();
				}
			}
			else
			{
				return PartFilterProvider.CreateForDocket(); // Tested in Warehouse by TestAutoRateJobHeader_FlatCalculatorReturnsDocketReference().
			}
		}

		#endregion

		#region Filter Values

		public override OrgSupplierPart ProductFilter => null;
		public override ProductAttributesMeasure ProductAttributesFilter => ProductAttributesMeasure.Empty;
		public override LocationMeasure LocationFilter => LocationMeasure.Empty;
		public override ZString DocketReferenceFilter => ZString.Empty;
		public override ZGuid CartageLegPKFilter => ZGuid.Empty;
		public override ZString? ContainerNumberFilter => null;
		public override RefContainer ContainerTypeFilter => null;

		#endregion
	}

	public class AutoRatingCalculatorParametersWithFilter : AutoRatingCalculatorParameters
	{
		readonly AutoRatingCalculatorParametersWithoutFilter ParentParams;

		/// <summary>
		/// Constructor for creating AutoRatingCalculatorParameters with a filter from a parent that has no filter.
		/// Allows this AutoRatingCalculatorParameters to be immutable, rather than modifying the parent.
		/// </summary>
		public AutoRatingCalculatorParametersWithFilter(AutoRatingCalculatorParametersWithoutFilter parentParams, IAmountByLine parentAmountByLine, AmountByLineTableFilter filter)
			: base(parentParams.Criteria, parentParams.AutoRater)
		{
			ParentParams = parentParams;
			AmountByLine = ((AmountByLineTable)parentAmountByLine).CreateWithFilter(this, filter);
		}

		public override AutoRateInfoCollection Results => ParentParams.Results;

#if DEBUG
		public override void SetResults_ForTest(AutoRateInfoCollection results)
		{
			ParentParams.SetResults_ForTest(results);
		}
#endif

		internal override ServiceAutoRater ServiceRater
			=> ParentParams.ServiceRater;

		internal override CalculationOrderResolver CalculationOrderResolver
			=> ParentParams.CalculationOrderResolver;

		public override List<FastLine> LinesToCalculate
			=> ParentParams.LinesToCalculate;

		protected override void RemoveSimilarChargesCore(RateLinesRepository rateLinesRepository)
		{
			new LineMeasureMatcher(Criteria)
				.RemoveSimilarCharges(this, rateLinesRepository, null);
		}

		protected override IAmountByLine AmountByLine { get; }

		internal override IRateEntry FreightLeg1Entry => ParentParams.FreightLeg1Entry;
		internal override IRateEntry FreightLeg2Entry => ParentParams.FreightLeg2Entry;

		#region Filters

		public override OrgSupplierPart ProductFilter => AmountByLine.ProductFilter;
		public override ProductAttributesMeasure ProductAttributesFilter => AmountByLine.ProductAttributesFilter;
		public override LocationMeasure LocationFilter => AmountByLine.LocationFilter;
		public override ZString DocketReferenceFilter => AmountByLine.DocketReferenceFilter;
		public override ZGuid CartageLegPKFilter => AmountByLine.CartageLegPKFilter;
		public override ZString? ContainerNumberFilter => AmountByLine.ContainerNumberFilter;
		public override RefContainer ContainerTypeFilter => AmountByLine.ContainerTypeFilter;

		#endregion
	}
}

