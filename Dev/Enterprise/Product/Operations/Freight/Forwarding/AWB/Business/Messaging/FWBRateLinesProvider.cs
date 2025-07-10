using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	sealed class FWBRateLinesProvider
	{
		public FWBRateLinesProvider(IEnumerable<ExportAWBRateLine> rateLines, FWB.Version version)
		{
			this.rateLines = Argument.NotNull(rateLines, "rateLines");
			this.version = version;
		}

		readonly IEnumerable<ExportAWBRateLine> rateLines;
		readonly FWB.Version version;

		#region Fields

		ZString rateDescriptionWeightUnit;
		ZString rateDescriptionUldType;

		#endregion

		#region Create FWB RateLines

		public IEnumerable<FWBRateLine> CreateFWBRateLines()
		{
			rateDescriptionWeightUnit = ZString.Empty;
			rateDescriptionUldType = ZString.Empty;

			var processedLines = ProcessLinesWrappingOrMergingDescription(rateLines.ToArray())
				.ToArray();

			var leaveLastLineBlankForNDA = version == FWB.Version.No16
				&& FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.Value
				&& !processedLines.Any(line =>
				{
					return line.RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation
						|| line.NatureAndQtyOfGoods != null && IsNatureAndQtyGoodsDimension(line.NatureAndQtyOfGoods.Type);
				});

			var overflow = processedLines.Length - FWB.NatureAndQtyMaxLines + (leaveLastLineBlankForNDA ? 1 : 0);

			return overflow > 0
				? RemoveExcessLines(processedLines, overflow)
				: processedLines;
		}

		#region RemoveExcessLines

		IEnumerable<FWBRateLine> RemoveExcessLines(IEnumerable<FWBRateLine> rateLinesParam, int overflow)
		{
			var result = new List<FWBRateLine>();

			foreach (var rateLine in rateLinesParam.Reverse())
			{
				if (overflow <= 0
					|| !rateLine.IsRateDescriptionEmpty
					|| rateLine.NatureAndQtyOfGoods == null
					|| !IsNatureAndQtyOfGoodsWrappable(rateLine.NatureAndQtyOfGoods.Type))
				{
					result.Add(rateLine);
				}
				else
				{
					overflow--;
				}
			}

			return result.AsEnumerable().Reverse();
		}

		#endregion

		#endregion

		#region C and G Description Merging Or Wrapping

		IEnumerable<FWBRateLine> ProcessLinesWrappingOrMergingDescription(ExportAWBRateLine[] rateLinesParam)
		{
			var accumulator = new List<ExportAWBRateLine>();
			var currentDescriptionIdentifier = string.Empty;

			var result = new List<FWBRateLine>();

			var needsChunking = rateLinesParam.Any(rateLine =>
			{
				return IsNatureAndQtyOfGoodsWrappable(rateLine.ER_NatureAndQtyOfGoodsType)
					&& rateLine.NatureAndQtyOfGoods != null
					&& rateLine.NatureAndQtyOfGoods.Text.Length > FWB.NatureAndQtyMaxLength;
			});

			foreach (var rateLine in rateLinesParam)
			{
				if (rateLine.IsEmpty)
				{
					continue;
				}

				if (rateLine.IsRateDescriptionEmpty
					&& (rateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription
						|| rateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation)
					&& rateLine.NatureAndQtyOfGoods.Text.IsCargoIMPEmpty())
				{
					continue;
				}

				if (!needsChunking || IsBeginingOfAnotherChunk(rateLine, currentDescriptionIdentifier))
				{
					result.AddRange(ProcessChunk(currentDescriptionIdentifier, accumulator));

					accumulator.Clear();
					currentDescriptionIdentifier = rateLine.ER_NatureAndQtyOfGoodsType;
				}

				accumulator.Add(rateLine);
			}

			result.AddRange(ProcessChunk(currentDescriptionIdentifier, accumulator));

			return result;
		}

		IEnumerable<FWBRateLine> ProcessChunk(string descriptionIdentifier, ICollection<ExportAWBRateLine> rateLinesParam)
		{
			var result = new List<FWBRateLine>();

			if (!rateLinesParam.Any())
			{
				return result;
			}

			var natureAndQtyOfGoods = CreateNatureAndQtyOfGoods(descriptionIdentifier, rateLinesParam)
				.ToArray();

			var fwbRateLine = CreateFWBRateLine(rateLinesParam.First());

			fwbRateLine.NatureAndQtyOfGoods = natureAndQtyOfGoods.FirstOrDefault();

			result.Add(fwbRateLine);

			foreach (var fwbNatureAndQtyOfGoods in natureAndQtyOfGoods.Skip(1))
			{
				result.Add(new FWBRateLine
				{
					NatureAndQtyOfGoods = fwbNatureAndQtyOfGoods
				});
			}

			return result;
		}

		bool IsBeginingOfAnotherChunk(ExportAWBRateLine rateLine, string currentChunkIdentifier)
		{
			if (!rateLine.IsRateDescriptionEmpty)
			{
				return true;
			}

			return rateLine.NatureAndQtyOfGoods != null
				&& (!IsNatureAndQtyOfGoodsWrappable(rateLine.ER_NatureAndQtyOfGoodsType)
					|| currentChunkIdentifier != rateLine.ER_NatureAndQtyOfGoodsType);
		}

		bool IsNatureAndQtyOfGoodsWrappable(string type)
		{
			return type == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription
				|| type == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
		}

		bool IsNatureAndQtyGoodsDimension(string type)
		{
			return type == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions
				|| type == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
		}

		#endregion

		#region FWB RateLine

		FWBRateLine CreateFWBRateLine(ExportAWBRateLine rateLine)
		{
			if (!rateLine.IsRateDescriptionEmpty)
			{
				rateDescriptionWeightUnit = rateLine.ER_GrossWeight > 0 && rateLine.ER_WeightInLBsOrKGs.Length == 1
					? rateLine.ER_WeightInLBsOrKGs
					: rateDescriptionWeightUnit;

				var commodityItemNumber = rateLine.ER_CommodityItemNumber;

				if (rateLine.ER_RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation)
				{
					rateDescriptionUldType = rateLine.ER_CommodityItemNumber.Length > 0
						? rateLine.ER_CommodityItemNumber
						: rateDescriptionUldType;

					commodityItemNumber = rateDescriptionUldType;
				}

				return new FWBRateLine
				{
					CommodityItemNumber = commodityItemNumber,

					NoOfPiecesOrRCP = rateLine.ER_NoOfPiecesOrRCP,
					WeightInLBsOrKGs = rateLine.ER_GrossWeight > 0 ? rateDescriptionWeightUnit : ZString.Empty,

					GrossWeight = rateLine.ER_GrossWeight,
					ChargeableWeight = rateLine.ER_ChargeableWeight,

					RateClass = rateLine.ER_RateClass,
					RateChargeOrDiscount = rateLine.ER_RateChargeOrDiscount,
					Total = rateLine.ER_Total
				};
			}

			return new FWBRateLine();
		}

		#endregion

		#region Nature And Qty of Goods

		IEnumerable<FWBNatureAndQtyOfGoods> CreateNatureAndQtyOfGoods(ZString identifier, IEnumerable<ExportAWBRateLine> rateLinesParam)
		{
			var descriptionParts = new List<string>();
			var otherRateLines = new List<ExportAWBRateLine>();

			foreach (var rateLine in rateLinesParam)
			{
				if (rateLine.NatureAndQtyOfGoods == null)
				{
					continue;
				}

				if (!IsNatureAndQtyOfGoodsWrappable(rateLine.ER_NatureAndQtyOfGoodsType)
					|| rateLine.ER_NatureAndQtyOfGoodsType != identifier)
				{
					otherRateLines.Add(rateLine);
				}
				else
				{
					var description = rateLine
						.NatureAndQtyOfGoods
						.Text
						.ToUpperInvariant()
						.KeepChars(FWB.NatureAndQtyCharactersToKeep, " ")
						.Trim();

					if (!description.IsEmpty)
					{
						descriptionParts.Add(description);
					}
				}
			}

			if (descriptionParts.Any())
			{
				foreach (var line in SplitText(string.Join(" ", descriptionParts), FWB.NatureAndQtyMaxLength))
				{
					yield return new FWBNatureAndQtyOfGoodsDescription(identifier, line);
				}
			}

			foreach (var line in otherRateLines)
			{
				yield return CreateFWBNatureAndQtyOfGoods(line);
			}
		}

		FWBNatureAndQtyOfGoods CreateFWBNatureAndQtyOfGoods(ExportAWBRateLine rateLine)
		{
			switch (rateLine.ER_NatureAndQtyOfGoodsType)
			{
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions:
					var dims = rateLine.NatureAndQtyOfGoodsDimensions;

					return new FWBNatureAndQtyOfGoodsDimensions
					{
						Length = dims.Length,
						Width = dims.Width,
						Height = dims.Height,
						Unit = dims.Unit,
						Count = dims.Count
					};

				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume:
					var vol = rateLine.NatureAndQtyOfGoodsVolume;

					return new FWBNatureAndQtyOfGoodsVolume
					{
						Value = vol.Volume,
						Unit = vol.Unit
					};

				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount:
					return new FWBNatureAndQtyOfGoodsSLAC
					{
						Count = rateLine.NatureAndQtyOfGoodsSLAC.Count
					};

				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin:
					return new FWBNatureAndQtyOfGoodsOrigin
					{
						Country = rateLine.NatureAndQtyOfGoodsOrigin.Country
					};

				default:
					return new FWBNatureAndQtyOfGoodsDescription(
						rateLine.ER_NatureAndQtyOfGoodsType,
						rateLine.NatureAndQtyOfGoods.Text);
			}
		}

		IEnumerable<string> SplitText(string text, int width)
		{
			text = text.Trim();

			if (width > 0 && text.Length > width)
			{
				var heading = text.Substring(0, width);
				var reminder = text.Substring(width, text.Length - width).TrimStart();

				yield return heading.TrimEnd();

				if (!string.IsNullOrEmpty(reminder))
				{
					foreach (var line in SplitText(reminder, width))
					{
						yield return line;
					}
				}
			}
			else
			{
				yield return text;
			}
		}

		#endregion
	}
}
