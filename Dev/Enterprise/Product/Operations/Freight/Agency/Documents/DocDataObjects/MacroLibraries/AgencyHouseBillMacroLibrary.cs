using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public sealed class AgencyHouseBillMacroLibrary : MacroLibraryBase
	{
		public override IEnumerator<IMacroMetaData> GetEnumerator() => lazyMacrosRegister.Value.GetEnumerator();

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Lazy<ICollection<IMacroMetaData>> lazyMacrosRegister = new Lazy<ICollection<IMacroMetaData>>(() => Load(MacroHandlers));

		#region SuppressResourceStringsCheckRegion

		static IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<IPackingLine, string>>(
					"DangerousGoodsDescription",
					"Generates dangerous goods description according to House Bill registry setting.",
					packingLine => CreateDangerousGoodsDescription(packingLine));

				yield return new Handler<Func<IDangerousGood, string>>(
					"DangerousGoodDescription",
					"Generates dangerous good description according to House Bill registry setting.",
					dangerousGood => CreateDescription(dangerousGood));

				yield return new Handler<Func<bool>>(
					"PrintSignature",
					"Returns value of the 'Liner & Agency -> Bills of Lading -> Print Signature' registry",
					() => PrintSignature());

				yield return new Handler<Func<bool>>(
					"ShowPacklineDetailsOnBillsOfLading",
					"Returns value of the 'Liner & Agency -> Bills of Lading -> Show Packline Details on Bills of Lading' registry",
					() => ShowPacklineDetailsOnBillsOfLading());

				yield return new Handler<Func<IMeasurement, string, IMeasurement>>(
					"ConvertTo",
					"Converts weight/volume to another unit.",
					(measurement, unit) => ConvertMeasurement(measurement, unit));
			}
		}

		#endregion

		#region CreateDangerousGoodsDescription

		static string CreateDangerousGoodsDescription(IPackingLine packingLine)
		{
			return packingLine.DangerousGoods?.Count > 0
				? string.Join(System.Environment.NewLine, packingLine.DangerousGoods.Select(CreateDescription))
				: string.Empty;
		}

		static string CreateDescription(IDangerousGood dangerousGood)
		{
			var flashPointValue = Utilities.Round(dangerousGood.FlashPoint?.Value ?? 0, 1);
			var flashPointWithUnit = flashPointValue == 0 ? ZString.Empty : new ZString(FormattableString.Invariant($"({flashPointValue}C c.c.)"));

			var packageType = dangerousGood.Quantity > 0 && dangerousGood.PackageType != null ? dangerousGood.PackageType.Description : ZString.Empty;

			return string.Join(", ", new[] { dangerousGood.Code, dangerousGood.TechnicalName, dangerousGood.IMOClass, packageType, flashPointWithUnit }.Where(x => !x.IsEmpty));
		}

		#endregion

		#region PrintSignature

		static bool PrintSignature() => AgencyRegistry.Instance.PrintSignature.Value;

		#endregion

		#region ShowPacklineDetailsOnBillsOfLading

		static bool ShowPacklineDetailsOnBillsOfLading() => AgencyRegistry.Instance.ShowPacklineDetailsOnBillsOfLading.Value;

		#endregion

		#region ConvertMeasurement

		static IMeasurement ConvertMeasurement(IMeasurement measurement, string unit)
		{
			if (measurement?.Unit == null
				|| string.Compare(measurement.Unit.Code, unit, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return measurement;
			}

			if (Core.Constants.Weight.ContainsCode(measurement.Unit.Code)
				&& Core.Constants.Weight.ContainsCode(unit))
			{
				return ConvertWeight(measurement, unit);
			}

			if (Core.Constants.Volume.ContainsCode(measurement.Unit.Code)
				&& Core.Constants.Volume.ContainsCode(unit))
			{
				return ConvertVolume(measurement, unit);
			}

			return measurement;
		}

		static IMeasurement ConvertWeight(IMeasurement weight, string unit)
		{
			return new Measurement
			{
				Value = Core.Constants.Weight.Convert(weight.Value, weight.Unit.Code, unit),
				Unit = new CodeDescription(weight.Unit.Codes as ICodeDescriptionPairList)
				{
					Code = unit
				}
			};
		}

		static IMeasurement ConvertVolume(IMeasurement volume, string unit)
		{
			return new Measurement
			{
				Value = Core.Constants.Volume.Convert(volume.Value, volume.Unit.Code, unit),
				Unit = new CodeDescription(volume.Unit.Codes as ICodeDescriptionPairList)
				{
					Code = unit
				}
			};
		}

		#endregion
	}
}
