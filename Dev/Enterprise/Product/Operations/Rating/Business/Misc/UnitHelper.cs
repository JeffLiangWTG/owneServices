using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Rating.Business.QuantityUnit;

namespace Enterprise.Rating.Business
{
	public class UnitHelper
	{
		public static CodeDescriptionPairList GetUnits(IRateEntry rateEntry, string countryCode, BusinessObjectFactory factory, Constants.PluralState pluralState = Constants.PluralState.Plural, bool isEqualizer = false)
		{
			var rateType = rateEntry != null ? rateEntry.RateType() : RateType.Forwarding;
			var withoutTeu = rateEntry == null || rateEntry.IsAir() || rateEntry.IsSea() && rateEntry.TI_Mode == Core.Constants.RateMode.LCL;
			var teuSpecificCachePart = withoutTeu ? "WithoutTeu" : string.Empty;
			var isEqualizerSea = isEqualizer && rateEntry != null && rateEntry.IsFCL();
			var equalizerSeaCachePart = isEqualizerSea ? "EqualizerSea" : string.Empty;
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", rateType, "WeightVolumes" + pluralState + teuSpecificCachePart + equalizerSeaCachePart);

			return factory.GetCachedValue(cacheKey,
				() =>
				{
					var codeDescriptionPairList = GetUnitList(factory, rateType, countryCode, pluralState);

					if (withoutTeu)
					{
						codeDescriptionPairList.RemoveCode(QuantityUnit.TU);
					}

					if (isEqualizerSea)
					{
						codeDescriptionPairList = new CodeDescriptionPairList();
						codeDescriptionPairList.AddPair(QuantityUnit.M3, QuantityUnit.GetDescription(QuantityUnit.M3, rateType, pluralState));
						codeDescriptionPairList.AddPair(QuantityUnit.CN, QuantityUnit.GetDescription(QuantityUnit.CN, rateType, pluralState));
						codeDescriptionPairList.AddPair(QuantityUnit.TU, QuantityUnit.GetDescription(QuantityUnit.TU, rateType, pluralState));
					}

					return codeDescriptionPairList;
				});
		}

		public static CodeDescriptionPairList GetForwardingAndCustomsUnits(BusinessObjectFactory factory, string countryCode, Constants.PluralState pluralState = Constants.PluralState.Plural)
		{
			var rateType = RateType.Forwarding | RateType.Customs;
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", rateType, "QuantityUnits" + pluralState + countryCode);
			return factory.GetCachedValue(cacheKey,
				() =>
				{
					var result = new CodeDescriptionPairList();
					var weightUnit = factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
					var volumnUnit = factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

					result.AddRange(weightUnit);
					result.AddRange(volumnUnit);

					var infos = typeof(QuantityUnit).GetFields(BindingFlags.Public | BindingFlags.Static);
					foreach (var info in infos)
					{
						var rateTypeFwdOrCustoms = info
								.GetCustomAttributes(typeof(RateTypeAttribute), false)
								.Cast<RateTypeAttribute>()
								.Any(x => (x.RateType & RateType.Forwarding) != 0 || (x.RateType & RateType.Customs) != 0);
						var countries = info
								.GetCustomAttributes(typeof(MeasureTypeCountrySpecificAttribute), false)
								.Cast<MeasureTypeCountrySpecificAttribute>();
						var hasCountryOrNone =
							countries.Any(x => x.CountryCodes.Contains(countryCode))
							||
							!countries.Any();

						if (rateTypeFwdOrCustoms && hasCountryOrNone)
						{
							var code = (string)info.GetValue(null);
							var description = GetDescription(code, rateType, pluralState);
							result.AddPair(code, description);
						}
					}

					result.AddRange(GetCachedPackageTypes(factory));
					return result;
				});
		}

		public CodeDescriptionPairList GetUnitMultiple(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UnitMultiples", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("3ec28d99-e85e-495a-a279-24d66896ee61", "No unit multiple"));
				result.AddPair("10", Res.GetString("dff354a7-aa3e-4f00-a063-dc8320da7e43", "Per 10 units"));
				result.AddPair("100", Res.GetString("dab53b8c-3a0d-4ce1-bd52-1c253ffe4705", "Per 100 units"));
				result.AddPair("1000", Res.GetString("7025641f-f5af-4986-a6c0-af895a7f526a", "Per 1000 units"));
				result.AddPair("10000", Res.GetString("32369d87-99b7-4220-a52c-bb8ce99237fe", "Per 10000 units"));
				return result;
			});
		}

		public static CodeDescriptionPairList GetUnitList(BusinessObjectFactory factory, RateType rateType, string countryCode, Constants.PluralState pluralState = Constants.PluralState.Plural)
		{
			var unitList = new UnitList(factory, rateType, countryCode, pluralState);
			unitList.SortByDescription();
			return unitList;
		}

		class UnitList : CodeDescriptionPairList
		{
			[CodeStringFinderHint(typeof(Constants.Weight), "GetDescription")]
			[CodeStringFinderHint(typeof(Constants.Volume), "GetDescription")]
			public UnitList(BusinessObjectFactory factory, RateType rateType, string countryCode, Constants.PluralState pluralState)
			{
				if (rateType != RateType.ShippingImportDetention && rateType != RateType.ShippingExportDetention)
				{
					AddWeightPairs(pluralState);
					AddVolumePairs(pluralState);
				}

				var infos = typeof(QuantityUnit).GetFields(BindingFlags.Public | BindingFlags.Static);
				foreach (var info in infos)
				{
					var code = (string)info.GetValue(null);
					var description = QuantityUnit.GetDescription(code, rateType, pluralState);
					var rateTypes = info.GetCustomAttributes(typeof(RateTypeAttribute), false);
					var countries = info.GetCustomAttributes(typeof(MeasureTypeCountrySpecificAttribute), false);

					if (rateTypes.Length == 1 && (rateType == 0 || (((RateTypeAttribute)rateTypes[0]).RateType & rateType) != 0))
					{
						if (countries.Length == 0 || countries.Length == 1 && ((MeasureTypeCountrySpecificAttribute)countries[0]).CountryCodes.Contains(countryCode))
						{
							AddPair(code, description);
						}
					}
				}

				AddRange(GetCachedPackageTypes(factory));
			}
		}

		public static RefPackTypeCollection GetCachedPackageTypes(BusinessObjectFactory factory)
			=> factory.GetCachedValue(string.Empty, () => new RefPackTypeCollection(factory));

		public static List<string> GetCachedSortedPackageTypeCodes(BusinessObjectFactory factory)
			=> factory.GetCachedValue(nameof(GetCachedSortedPackageTypeCodes), ()
				=> new List<string>(GetCachedPackageTypes(factory).Select(x => (string)x.F3_Code).OrderBy(x => x)));

		public static bool IsTopPack(string unit, BusinessObjectFactory factory)
			=> unit == QuantityUnit.CN || unit == QuantityUnit.PK || GetCachedSortedPackageTypeCodes(factory).BinarySearch(unit) >= 0;

		public static bool IsPkgUnit(string tl_weightVolume)
			=> RatingCache.GetMeasureTypeFromUnit(tl_weightVolume) == MeasureType.Unit;

		public static bool IsPkgUnitOrPallet(string tl_weightVolume)
			=> tl_weightVolume == Constants.PkgUnit.Pallet || IsPkgUnit(tl_weightVolume);
	}
}


