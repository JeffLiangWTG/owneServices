using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public struct UNDGNatureAndQuantityOfGoodsElement : IEquatable<UNDGNatureAndQuantityOfGoodsElement>
	{
		public UNDGNatureAndQuantityOfGoodsElement(
			ZString unnoPrefix,
			ZString unno,
			ZDecimal measurementQuantity,
			ZString measurementUnit,
			int packageCount,
			ZString properShippingName,
			bool isSubstancePermittedInLimitedQuantities)
		{
			UNNOPrefix = unnoPrefix;
			UNNO = unno;
			MeasurementQuantity = measurementQuantity;
			MeasurementUnit = measurementUnit;
			PackageCount = packageCount;
			ProperShippingName = properShippingName;
			IsSubstancePermittedInLimitedQuantities = isSubstancePermittedInLimitedQuantities;
		}

		public ZString UNNOPrefix { get; }
		public ZString UNNO { get; }
		public ZDecimal MeasurementQuantity { get; }
		public ZString MeasurementUnit { get; }
		public int PackageCount { get; }
		public ZString ProperShippingName { get; }
		public bool IsSubstancePermittedInLimitedQuantities { get; }

		public override bool Equals(object obj)
		{
			return obj is UNDGNatureAndQuantityOfGoodsElement element && this == element;
		}

		public static bool operator ==(UNDGNatureAndQuantityOfGoodsElement a, UNDGNatureAndQuantityOfGoodsElement b)
		{
			return a.UNNOPrefix == b.UNNOPrefix &&
				a.UNNO == b.UNNO &&
				a.MeasurementQuantity == b.MeasurementQuantity &&
				a.MeasurementUnit == b.MeasurementUnit &&
				a.PackageCount == b.PackageCount &&
				a.ProperShippingName == b.ProperShippingName &&
				a.IsSubstancePermittedInLimitedQuantities == b.IsSubstancePermittedInLimitedQuantities;
		}
		public static bool operator !=(UNDGNatureAndQuantityOfGoodsElement a, UNDGNatureAndQuantityOfGoodsElement b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 152319884;
				hashCode = hashCode * -1521134295 + UNNOPrefix.GetHashCode();
				hashCode = hashCode * -1521134295 + UNNO.GetHashCode();
				hashCode = hashCode * -1521134295 + MeasurementQuantity.GetHashCode();
				hashCode = hashCode * -1521134295 + MeasurementUnit.GetHashCode();
				hashCode = hashCode * -1521134295 + PackageCount.GetHashCode();
				hashCode = hashCode * -1521134295 + ProperShippingName.GetHashCode();
				hashCode = hashCode * -1521134295 + IsSubstancePermittedInLimitedQuantities.GetHashCode();
				return hashCode;
			}
		}

		bool IEquatable<UNDGNatureAndQuantityOfGoodsElement>.Equals(UNDGNatureAndQuantityOfGoodsElement other)
		{
			return other is UNDGNatureAndQuantityOfGoodsElement element && this == element;
		}
	}

	public static class ExcludedDangerousGoodsDetails
	{
		static bool RequiresMeasurementDetailsUNDGs(ZString unno)
		{
			return unno == ShippersDeclarationUNDGExclusions.UNNOCodes.UN1845;
		}

		public static IEnumerable<IEnumerable<UNDGNatureAndQuantityOfGoodsElement>> GetExcludedDangerousGoodDetailsElements(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return new List<List<UNDGNatureAndQuantityOfGoodsElement>>();
			}

			var shipments = consol.Shipments.OfType<ForwardingShipment>();

			var undgNatureAndQuantityOfGoodsElementsList = shipments.Select(shipment =>
			{
				var dangerousPackLines = shipment
					.OuterPackLines
					.OfType<PackLine>()
					.Distinct()
					.Where(p => p.UNDGs.Any());

				var packLinesExcludedFromShippersDeclaration = dangerousPackLines
					.Where(p => !ShippersDeclarationUNDGExclusions.DoesPackLineRequireDeclaration(p) && p.UNDGs.Any(undg => !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(undg)))
					.ToList();

				var undgNatureAndQuantityOfGoodsElements = packLinesExcludedFromShippersDeclaration
					.SelectMany(packLine => GetUNDGNatureAndQuantityOfGoodsDetailsFromPackLine(packLine))
					.GroupBy(
						element => new
						{
							element.UNNOPrefix,
							element.UNNO,
							element.MeasurementQuantity,
							element.MeasurementUnit,
						},
						(key, values) => new UNDGNatureAndQuantityOfGoodsElement
						(
							key.UNNOPrefix,
							key.UNNO,
							values.First().MeasurementQuantity,
							values.First().MeasurementUnit,
							values.Sum(packLineElement => packLineElement.PackageCount),
							values.First().ProperShippingName,
							values.First().IsSubstancePermittedInLimitedQuantities
						)
					);

				return undgNatureAndQuantityOfGoodsElements;
			});

			return undgNatureAndQuantityOfGoodsElementsList;
		}

		public static IEnumerable<DetailLine> GetDetailLinesFromPackLineUNDGNatureAndQuantityOfGoodsElement(UNDGNatureAndQuantityOfGoodsElement element)
		{
			var dgDetailLines = new List<DetailLine>();
			if (RequiresMeasurementDetailsUNDGs(element.UNNO))
			{
				dgDetailLines.Add(new(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} {1} ({2}x{3}{4})", // multiplier symbol
					element.UNNOPrefix,
					element.UNNO,
					element.PackageCount,
					element.MeasurementQuantity,
					element.MeasurementUnit
				)));
			}
			else
			{
				dgDetailLines.Add(new(Res.GetString("7e9f18d4-7e5d-6697-4cd6-3e986adfe1b7", "{0} {1} ({2} PKG)",
						element.UNNOPrefix,
						element.UNNO,
						element.PackageCount)));
			}

			dgDetailLines.Add(new(element.ProperShippingName, isText: true));

			if (element.IsSubstancePermittedInLimitedQuantities)
			{
				dgDetailLines.Add(new(Res.GetString("5d3a55d3-9d41-454a-af38-5877c0199ee5", "Dangerous Goods in")));
				dgDetailLines.Add(new(Res.GetString("a5921dd2-0552-47e9-85c2-f5ec44ad6838", "Excepted Quantities")));
			}
			return dgDetailLines;
		}

		static IEnumerable<UNDGNatureAndQuantityOfGoodsElement> GetUNDGNatureAndQuantityOfGoodsDetailsFromPackLine(PackLine packLine)
		{
			var packLineHasExtraUNDGDetails = ShippersDeclarationUNDGExclusions.DoesPackLineHaveExtraUNDGDetails(packLine);
			var undgsExcludingLithium = packLine.UNDGs.Where(undg => undg.Substance != null && !ShippersDeclarationUNDGExclusions.IsLithiumUNDGs(undg));

			(ZDecimal, ZString) GetUNDGNatureAndQuantityOfGoodsMeasurementDetails(UNDGDataItem undg)
			{
				if (RequiresMeasurementDetailsUNDGs(undg.Substance.DG_UNNO))
				{
					if (packLineHasExtraUNDGDetails)
					{
						var useWeightUnit = undg.DI_DGWeight > 0;
						var useVolumeUnit = undg.DI_DGVolume > 0;

						if (useWeightUnit)
						{
							return (undg.DI_DGWeight, undg.DI_UnitOfWeight);
						}
						else if (useVolumeUnit)
						{
							return (undg.DI_DGVolume, undg.DI_UnitOfVolume);
						}
					}
					else
					{
						var useWeightUnit = packLine.JL_ActualWeight > 0;
						var useVolumeUnit = packLine.JL_ActualVolume > 0;

						if (useWeightUnit)
						{
							return (packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ);
						}
						else if (useVolumeUnit)
						{
							return (packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ);
						}
					}
				}

				return (0, Weight.Kilograms);
			}

			var undgNatureAndQuantityOfGoodsElements = undgsExcludingLithium.Select(undg =>
			{
				(var measurementQuantity, var measurementUnit) = GetUNDGNatureAndQuantityOfGoodsMeasurementDetails(undg);

				return new UNDGNatureAndQuantityOfGoodsElement
				(
					undg.GetUnnoPrefix(),
					undg.Substance.DG_UNNO,
					measurementQuantity,
					measurementUnit,
					packLineHasExtraUNDGDetails ? undg.DI_PackageCount : packLine.JL_PackageCount,
					undg.Substance.DG_PSN,
					ExceptedQuantityUtilities.IsSubstancePermittedInLimitedQuantities(undg.Substance)
				);
			});

			return undgNatureAndQuantityOfGoodsElements;
		}
	}
}
