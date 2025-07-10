using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface IUnitConverter
	{
		ZString ParentUnit { get; }
		ZString ChildUnit { get; }
		ZDecimal ConversionFactor { get; }
	}

	public interface IUnitConverterDataProvider
	{
		/// <summary>
		/// This could be null
		/// </summary>
		OrgSupplierPart Product { get; }
		ZString CountryCode { get; }
		ZGuid SupplierFK { get; }
		BusinessObjectFactory Factory { get; }
		bool ProductHasSpecificUnitConversions { get; }
		ZString Type { get; }

		IEnumerable<IUnitConverter> GetUnitConversionFactorsFromProductUnits();
	}

	/// <summary>
	/// This utility class offers unit conversions between package type units. 
	/// It refers to RefPack table and PartUnit Collection, if the owner class is OrgSupplierPart.
	/// If a unit is non-pack type, like weight, it uses a standard conversion in Core.Constants
	/// </summary>
	public sealed class UnitConverter
	{
		public UnitConverter(IUnitConverterDataProvider dataProvider)
		{
			this.Factory = dataProvider.Factory;
			this.dataProvider = dataProvider;
		}

		public bool UseCachedConvertion
		{
			get
			{
				return GetCachedConvertion(Factory).UseCachedConvertion;
			}
		}

		public static IDisposable TemporarySetupCachedConvertion(BusinessObjectFactory factory)
		{
			return GetCachedConvertion(factory).TemporarySetupCachedConvertion();
		}

		static CachedConvertion GetCachedConvertion(BusinessObjectFactory factory)
		{
			var workingFactory = factory;
			return workingFactory.GetCachedValue("UnitConverterCachedConvertion", () => new CachedConvertion(workingFactory));
		}

		class CachedConvertion
		{
			internal CachedConvertion(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}
			readonly BusinessObjectFactory factory;

			public IDisposable TemporarySetupCachedConvertion()
			{
				return new CachedConvertionSetup(this);
			}

			public bool UseCachedConvertion
			{
				get { return cachedConvertionIndex > 0; }
			}

			sealed class CachedConvertionSetup : IDisposable
			{
				public CachedConvertionSetup(CachedConvertion cached)
				{
					this.cached = cached;
					cached.IncreaseCachedConvertionIndex();
				}

				readonly CachedConvertion cached;

				public void Dispose()
				{
					cached.DecreaseCachedConvertionIndex();
				}
			}

			void IncreaseCachedConvertionIndex()
			{
				cachedConvertionIndex++;
			}

			void DecreaseCachedConvertionIndex()
			{
				cachedConvertionIndex--;
				if (cachedConvertionIndex == 0)
				{
					ClearCachedValue<CodeDescriptionPairList>("GetConversionFactorConvertibleUQsDictionary");
					ClearCachedValue<IEnumerable<IUnitConverter>>("UnitConverterAllRelevantConvertersDictionary");
				}
			}

			void ClearCachedValue<T>(string dictionaryKey)
			{
				var dictionary = factory.GetCachedValue(dictionaryKey, () => new Dictionary<ZString, T>());
				dictionary.Clear();
			}

			int cachedConvertionIndex;
		}

		public bool Convertible(ZString fromUQ, ZString toUQ)
		{
			return ConversionFactor(fromUQ, toUQ) != 0m;
		}

		public ZDecimal Convert(ZDecimal qty, ZString fromUQ, ZString toUQ)
		{
			// if ConversionFactor between FromUQ and ToUQ is recurring decimal ie. 1/3 == 0.(3) then calculation result will be incorrect, rounding solve this problem.
			return Utilities.Round(qty * ConversionFactor(fromUQ, toUQ), 10); // 10 is enough to correctly convert miligrams into tonnes
		}

		public bool HasMoreThanOneConversionFactor(ZString fromUQ, ZString toUQ)
		{
			bool result = false;
			ZDecimal[] factors = GetAllConversionFactors(fromUQ, toUQ, scanForAllConversions: true);

			Array.Sort(factors);

			ZDecimal previousFactor = -1m;
			foreach (ZDecimal factor in factors)
			{
				if (previousFactor != -1m && decimal.Round(previousFactor, 6) != decimal.Round(factor, 6))//6 is very arbitrary.
				{
					result = true;
					break;
				}
				previousFactor = factor;
			}
			return result;
		}

		public ZDecimal ConversionFactor(ZString fromUQ, ZString toUQ)
		{
			fromUQ = fromUQ.ToUpper().Trim();
			toUQ = toUQ.ToUpper().Trim();

			ZDecimal result = 0m;

			if (!fromUQ.IsEmpty && !toUQ.IsEmpty)
			{
				if (fromUQ == toUQ || fromUQ.IsEmpty || toUQ.IsEmpty)
				{
					result = 1m;
				}
				else
				{
					return GetConversionFactor(fromUQ, toUQ);
				}
			}
			return result;
		}

#if DEBUG
		public int ConversionFactorInvokedCountForTest;
#endif

		public CodeDescriptionPairList ConvertibleUQs
		{
			get
			{
				return UseCachedConvertion ? GetCachedValue("GetConversionFactorConvertibleUQsDictionary", GetKey, GetConvertibleUQs) : GetConvertibleUQs();
			}
		}

		#region Implementation
		T GetCachedValue<T>(string dictionaryKey, Func<ZString> getKey, Func<T> getValue)
		{
			var dictionary = Factory.GetCachedValue(dictionaryKey, () => new Dictionary<ZString, T>());
			var key = getKey();
			T result;
			if (!dictionary.TryGetValue(key, out result))
			{
				result = getValue();
				dictionary.Add(key, result);
			}
			return result;
		}

		CodeDescriptionPairList GetConvertibleUQs()
		{
			var result = new CodeDescriptionPairList();
			foreach (var refPack in AllCusRefPacks)
			{
				result.AddPair(refPack.RP_CustomsPack, refPack.RP_CustomsPack_List.GetDescriptionFromCode(refPack.RP_CustomsPack));
			}
			return result;
		}

		readonly BusinessObjectFactory Factory;
		readonly IUnitConverterDataProvider dataProvider;

		internal Dictionary<string, List<IUnitConverter>> AllRelevantConvertersGroupByUnit()
		{
			return UseCachedConvertion ? GetCachedValue("UnitConverterAllRelevantConvertersGroupByUnitDictionary", GetKey, GetAllRelevantConvertersGroupByUnit) : GetAllRelevantConvertersGroupByUnit();
		}

		Dictionary<string, List<IUnitConverter>> GetAllRelevantConvertersGroupByUnit()
		{
			var result = new Dictionary<string, List<IUnitConverter>>(StringComparer.OrdinalIgnoreCase);
			foreach (var converter in AllRelevantConverters().Values.SelectMany(x => x))
			{
				AddToDictionary(result, converter.ParentUnit, converter);
				AddToDictionary(result, converter.ChildUnit, converter);
			}
			return result;
		}

		void AddToDictionary(Dictionary<string, List<IUnitConverter>> dictionary, ZString key, IUnitConverter converter)
		{
			if (!dictionary.TryGetValue(key, out var list))
			{
				list = new List<IUnitConverter>();
				dictionary.Add(key, list);
			}
			list.Add(converter);
		}

		internal Dictionary<string, List<IUnitConverter>> AllRelevantConverters()
		{
			return UseCachedConvertion ? GetCachedValue("UnitConverterAllRelevantConvertersDictionary", GetKey, GetAllRelevantConverters) : GetAllRelevantConverters();
		}

		Dictionary<string, List<IUnitConverter>> GetAllRelevantConverters()
		{
			var result = new Dictionary<string, List<IUnitConverter>>(StringComparer.OrdinalIgnoreCase);
			var product = dataProvider.Product;
			if (product != null)
			{
				AddConverters(result, dataProvider.GetUnitConversionFactorsFromProductUnits());
			}

			GetCusRefPacksExcluding(result.Values.SelectMany(x => x)).ForEach(c =>
			{
				var key = GetKey(c);
				if (!result.TryGetValue(key, out var list))
				{
					list = new List<IUnitConverter>();
					result.Add(key, list);
				}
				list.Add(c);
			});
			AddConverters(result, CoreUnits.Cast<IUnitConverter>());
			return result;
		}

		void AddConverters(Dictionary<string, List<IUnitConverter>> dictionary, IEnumerable<IUnitConverter> unitConverters)
		{
			unitConverters.ForEach(x =>
			{
				var key = GetKey(x);
				if (!dictionary.TryGetValue(key, out var list))
				{
					list = new List<IUnitConverter>();
					dictionary.Add(key, list);
				}
				list.Add(x);
			});
		}

		string GetKey(IUnitConverter refPacks) => GetKey(refPacks.ParentUnit, refPacks.ChildUnit);

		string GetKey(string parentUnit, string childUnit) => parentUnit + Separator + childUnit;
		const string Separator = "|";

		ZString GetKey()
		{
			var result = new ZStringBuilder(dataProvider.SupplierFK.ToStringKey());
			result.Append(dataProvider.CountryCode);
			var product = dataProvider.Product;
			result.Append(product != null && dataProvider.ProductHasSpecificUnitConversions ? product.PK.ToStringKey() : string.Empty);
			return result.ToStringWithDelimiterBetweenAppends("|");
		}

		ZDecimal[] GetAllConversionFactors(ZString fromUQ, ZString toUQ, bool scanForAllConversions = false)
		{
			return UseCachedConvertion ? GetCachedValue("UnitConverterGetAllConversionFactorsDictionary", () => GetKey() + "|" + fromUQ + "|" + toUQ, () => GetAllConversionFactorsCore(fromUQ, toUQ, scanForAllConversions)) : GetAllConversionFactorsCore(fromUQ, toUQ, scanForAllConversions);
		}

		ZDecimal[] GetAllConversionFactorsCore(ZString fromUQ, ZString toUQ, bool scanForAllConversions = false)
		{
#if DEBUG
			ConversionFactorInvokedCountForTest++;
#endif
			var conversionPaths = new List<List<UnitAccumulatedFactorPair>>();
			var currentPath = new List<UnitAccumulatedFactorPair>();

			currentPath.Add(new UnitAccumulatedFactorPair(fromUQ, 1m, 0));

			ZDecimal[] result = null;
			if (!scanForAllConversions)
			{
				var directConversion = DirectConversion(fromUQ, toUQ);
				if (directConversion.HasValue)
				{
					result = new[] { directConversion.Value };
				}
			}

			if (result == null)
			{
				FindAllConversionPaths(toUQ, currentPath, conversionPaths, AllRelevantConvertersGroupByUnit(), 0);
				result = conversionPaths.Select(x => x.Last()).OrderBy(p => p.PathConnectorCount).Select(f => f.AccumulatedFactor).ToArray();
			}

			return result.ToArray();
		}

		ZDecimal GetConversionFactor(ZString fromUQ, ZString toUQ, bool scanForAllConversions = false)
		{
			return UseCachedConvertion ? GetCachedValue("UnitConverterGetConversionFactorDictionary", () => GetKey() + "|" + fromUQ + "|" + toUQ, () => GetConversionFactorCore(fromUQ, toUQ, scanForAllConversions)) : GetConversionFactorCore(fromUQ, toUQ, scanForAllConversions);
		}

		ZDecimal GetConversionFactorCore(ZString fromUQ, ZString toUQ, bool scanForAllConversions = false)
		{
#if DEBUG
			ConversionFactorInvokedCountForTest++;
#endif

			if (!scanForAllConversions)
			{
				var directConversion = DirectConversion(fromUQ, toUQ);
				if (directConversion.HasValue)
				{
					return directConversion.Value;
				}
			}

			return FindShortestConversionPath(fromUQ, toUQ, AllRelevantConvertersGroupByUnit())?.Last().AccumulatedFactor ?? 0;
		}

		ZDecimal? DirectConversion(ZString fromUQ, ZString toUQ)
		{
			ZDecimal? result = null;
			IUnitConverter converter = null;
			// If a conversion between two units exists in a product, the conversions from CusRefPacks should not be used and will be removed from allUnits.
			// So, We don't need to filter the elements of allUnits.
			var key = GetKey(fromUQ, toUQ);
			var allUnits = AllRelevantConverters();
			if (allUnits.TryGetValue(key, out var list))
			{
				converter = list.FirstOrDefault(x => !(x is CoreConversion));
			}
			if (converter != null)
			{
				result = converter.ConversionFactor;
			}
			else
			{
				key = GetKey(toUQ, fromUQ);
				if (allUnits.TryGetValue(key, out list))
				{
					converter = list.FirstOrDefault(x => !(x is CoreConversion));
				}
				if (converter != null && converter.ConversionFactor != 0)
				{
					result = 1.0m / converter.ConversionFactor;
				}
			}

			return result;
		}

		internal CusRefPacks[] AllCusRefPacks
		{
			get
			{
				if (allPacks == null || ShouldRefreshCusRefPacks)
				{
					var packs = Factory.Load<CusRefPacks>(AllPacksFilter);
					var filterRefPacks = CusRefPacksHelper.FilterRefPacks(packs, dataProvider.Type);
					allPacks = OrderByTypeAndUQs(filterRefPacks);
					previousSupplierPK = dataProvider.SupplierFK;
					previousCountryCode = dataProvider.CountryCode;
					previousPart = dataProvider.Product;
				}
				return allPacks;
			}
		}

		CusRefPacks[] OrderByTypeAndUQs(List<CusRefPacks> filterRefPacks)
		{
			var result = OrderByUQs(filterRefPacks.Where(x => x.RP_Type == dataProvider.Type));
			if (dataProvider.Type != RPTypeList.Codes.AllAreas)
			{
				result.AddRange(OrderByUQs(filterRefPacks.Where(x => x.RP_Type == RPTypeList.Codes.AllAreas)));
			}
			return result.ToArray();
		}

		List<CusRefPacks> OrderByUQs(IEnumerable<CusRefPacks> packs)
		{
			return packs.OrderBy(x => x.RP_CommercialPack.PadRight(BaseRefPacks.Schema.RP_CommercialPackMaxLength) + x.RP_CustomsPack).ToList();
		}

		CusRefPacks[] allPacks;

		ZGuid previousSupplierPK;
		ZString previousCountryCode;
		OrgSupplierPart previousPart;

		bool ShouldRefreshCusRefPacks
		{
			get
			{
				return previousSupplierPK != dataProvider.SupplierFK
					|| previousCountryCode != dataProvider.CountryCode
					|| previousPart != dataProvider.Product
					|| (allPacks != null && allPacks.Any(p => p.IsDeleted));
			}
		}

		ZQuery AllPacksFilter
		{
			get
			{
				ZQuery result;
				if (dataProvider.SupplierFK.IsValid)
				{
					result = SupplierSpecificRefPackFilter;
					result.AddToFilter(NonSupplierRefPacksFilter, JoinCondition.Or);
				}
				else
				{
					result = new ZQuery(RefPacksSchema.RP_OH_Supplier, DBNull.Value);
				}
				result.AddToFilter(CusRefPacksHelper.GetRefPacksQuery(dataProvider.CountryCode, dataProvider.Type, ZString.Empty), JoinCondition.And);
				return result;
			}
		}

		ZQuery SupplierSpecificRefPackFilter
		{
			get
			{
				ZQuery filter = new ZQuery(RefPacksSchema.RP_OH_Supplier, SQLComparisonOperator.Equal, dataProvider.SupplierFK);
				return filter;
			}
		}

		/// <summary>
		/// Among generic pack conversions, get rid of records that have the same values both for commercial and customs pack
		/// </summary>
		ZQuery NonSupplierRefPacksFilter
		{
			get
			{
				var result = new ZDBOnlyQuery(typeof(CusRefPacks));

				var parameterCollection = new ZSqlParameterCollection();
				parameterCollection.Add("@Country", dataProvider.CountryCode, RefPacksSchema.RP_CustomsCountry);
				parameterCollection.Add("@Supplier", dataProvider.SupplierFK, RefPacksSchema.RP_OH_Supplier);

				var subQuery = new ZDBOnlySubQuery(typeof(CusRefPacks), RefPacksSchema.PK);
				subQuery.AddFilterAndZSQLParameterCollection(GenericRefPacksFilterScript, parameterCollection);
				result.AddSubQuery(subQuery, JoinCondition.And);

				return result;
			}
		}

		const string GenericRefPacksFilterScript = @"
	RP_CustomsCountry = @Country
	AND
	(
		RP_OH_Supplier is null 
		AND NOT EXISTS 
			(
				SELECT NULL FROM dbo.RefPacks Generic
				WHERE 
					Generic.RP_OH_Supplier = @Supplier
					AND Generic.RP_CustomsCountry = @Country
					AND Generic.RP_CustomsPack = RefPacks.RP_CustomsPack
					AND Generic.RP_CommercialPack = RefPacks.RP_CommercialPack
			)
	)
";

		List<UnitAccumulatedFactorPair> FindShortestConversionPath(ZString fromUQ, ZString toUQ, Dictionary<string, List<IUnitConverter>> converters)
		{
			var visitedUnits = new HashSet<ZString>();
			var currentConversionPaths = new List<List<UnitAccumulatedFactorPair>>();
			var start = new List<UnitAccumulatedFactorPair>();
			start.Add(new UnitAccumulatedFactorPair(fromUQ, 1m, 0));
			visitedUnits.Add(fromUQ);
			currentConversionPaths.Add(start);
			var nextConversionPaths = new List<List<UnitAccumulatedFactorPair>>();

			for (var depth = 0; depth < 999; ++depth)
			{
				if (currentConversionPaths.Count == 0)
				{
					return null;
				}

				foreach (var currentPath in currentConversionPaths)
				{
					var current = currentPath[currentPath.Count - 1];
					fromUQ = current.NextUQ;
					var unitConversions = GetConvertibleUnits(fromUQ, converters, currentPath);
					foreach (IUnitConverter one in unitConversions)
					{
						var childUnit = one.ChildUnit;
						if (!visitedUnits.Contains(childUnit))
						{
							//I think you can only do this optimization if you're not checking for paradoxes - if you are, then it's 'visitedEdges' that you don't want to double up on instead
							visitedUnits.Add(childUnit);
							//TODO: could be more performant by using a tree structure
							var nextPath = new List<UnitAccumulatedFactorPair>(currentPath);
							nextPath.Add(new UnitAccumulatedFactorPair(childUnit, current.AccumulatedFactor * one.ConversionFactor, depth));
							if (childUnit.Equals(toUQ))
							{
								return nextPath;
							}
							else
							{
								nextConversionPaths.Add(nextPath);
							}
						}
					}
				}
				currentConversionPaths = nextConversionPaths;
				nextConversionPaths = new List<List<UnitAccumulatedFactorPair>>();
			}

			return null;
		}

		void FindAllConversionPaths(ZString toUQ, List<UnitAccumulatedFactorPair> currentPath, List<List<UnitAccumulatedFactorPair>> paths, Dictionary<string, List<IUnitConverter>> converters, ZInt depth)
		{
			//Only use this function if you really want ALL of the conversion paths, and not just the first one we find - can get very slow
			var current = currentPath[currentPath.Count - 1];
			var fromUQ = current.NextUQ;

			if (!current.NextUQ.EqualsIgnoringCase(toUQ))
			{
				var unitConversions = GetConvertibleUnits(fromUQ, converters, currentPath);
				foreach (IUnitConverter one in unitConversions)
				{
					var childUnit = one.ChildUnit;
					currentPath.Add(new UnitAccumulatedFactorPair(childUnit, current.AccumulatedFactor * one.ConversionFactor, depth));
					if (childUnit.EqualsIgnoringCase(toUQ))
					{
						paths.Add(new List<UnitAccumulatedFactorPair>(currentPath));
					}
					else
					{
						FindAllConversionPaths(toUQ, currentPath, paths, converters, depth + 1); // to get all possible paths new instance of Current Path should be passed into recursion. However the calculation is going to become a lot slower - so not done yet.
					}
					currentPath.RemoveAt(currentPath.Count - 1); //pop the stack once we return to this level 
				}
			}
		}

		bool HasUsedThisConversionFactor(IUnitConverter unitConverter, List<UnitAccumulatedFactorPair> currentPath, bool shouldMatchFactor)
		{
			bool result = false;

			for (int index = 1; index < currentPath.Count; index++)
			{
				var previousPair = currentPath[index - 1];
				var currentPair = currentPath[index];

				if (currentPair.NextUQ.EqualsIgnoringCase(unitConverter.ChildUnit) && previousPair.NextUQ.EqualsIgnoringCase(unitConverter.ParentUnit)
					&& (!shouldMatchFactor || currentPair.AccumulatedFactor == previousPair.AccumulatedFactor * unitConverter.ConversionFactor)

				|| currentPair.NextUQ.EqualsIgnoringCase(unitConverter.ParentUnit) && previousPair.NextUQ.EqualsIgnoringCase(unitConverter.ChildUnit)
				&& (!shouldMatchFactor || currentPair.AccumulatedFactor == (unitConverter.ConversionFactor == 0 ? 0m : previousPair.AccumulatedFactor * (1 / unitConverter.ConversionFactor)))) // due to possible rounding issues this conversion should match conversion in GetConvertibleUnits(..) to avoid Stack Overflow exception.
				{
					result = true;
					break;
				}
			}

			return result;
		}

		IEnumerable<IUnitConverter> GetConvertibleUnits(ZString fromUQ, Dictionary<string, List<IUnitConverter>> converters, List<UnitAccumulatedFactorPair> currentPath)
		{
			var result = new List<IUnitConverter>();
			if (converters.TryGetValue(fromUQ, out var list))
			{
				foreach (IUnitConverter conversion in list)
				{
					var factor = conversion.ConversionFactor;
					if (factor != 0)
					{
						var childUnit = conversion.ChildUnit;
						var parentUnit = conversion.ParentUnit;
						bool shouldReverse = childUnit.EqualsIgnoringCase(fromUQ);
						if (shouldReverse && list.Any(x => x.ChildUnit.EqualsIgnoringCase(parentUnit)))
						{
							continue;
						}

						if (!HasUsedThisConversionFactor(conversion, currentPath, true))
						{
							var nextUQ = childUnit;

							if (shouldReverse)
							{
								factor = 1 / factor;
								nextUQ = parentUnit;
							}

							result.Add(new UnitConversion(factor, nextUQ, fromUQ));
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// If a conversion between two units exists in a product, the conversions from CusRefPacks should not be used.
		/// Not just a direct conversion between two units.
		/// 4 PCE - UNT exists in Product. 1 PCE - 1 NO - 1 UNT (therefore 1 PCE - 1 UNT) exist in CusRefPacks. This CusRefPacks conversion should be avoided.
		/// </summary>
		IEnumerable<IUnitConverter> GetCusRefPacksExcluding(IEnumerable<IUnitConverter> productUnits)
		{
			var result = new Dictionary<string, List<IUnitConverter>>();
			AllCusRefPacks.ForEach(x =>
			{
				AddToDictionary(result, x.ParentUnit, x);
				AddToDictionary(result, x.ChildUnit, x);
			});

			foreach (IUnitConverter unitConversionFactor in productUnits)
			{
				var conversionPaths = new List<List<UnitAccumulatedFactorPair>>();
				var currentPath = new List<UnitAccumulatedFactorPair>();

				currentPath.Add(new UnitAccumulatedFactorPair(unitConversionFactor.ChildUnit, 1m, 0));
				FindAllConversionPaths(unitConversionFactor.ParentUnit, currentPath, conversionPaths, result, 0);

				foreach (var aPath in conversionPaths)
				{
					var final = aPath[aPath.Count - 1];
					if (final.AccumulatedFactor > 0m)
					{
						foreach (var list in result.Values)
						{
							list.RemoveAll(x => HasUsedThisConversionFactor(x, aPath, false));
						}
					}
				}
			}

			return result.Values.SelectMany(x => x).WhereNotNull().Distinct().OrderBy(x => x.ParentUnit).ThenBy(x => x.ChildUnit);
		}

		struct UnitAccumulatedFactorPair
		{
			public ZString NextUQ;
			public ZDecimal AccumulatedFactor;
			public ZInt PathConnectorCount;
			public UnitAccumulatedFactorPair(ZString uQ, ZDecimal factor, ZInt pathConnectorCount)
			{
				NextUQ = uQ;
				AccumulatedFactor = factor;
				PathConnectorCount = pathConnectorCount;
			}
		}

		class CoreConversion : IUnitConverter
		{
			public CoreConversion(ZString type, ZString toUQ)
			{
				switch (type)
				{
					case UQType.Weight:
						this.FromUQ = Core.Constants.Weight.Kilograms;
						break;
					case UQType.Volume:
						this.FromUQ = Core.Constants.Volume.CubicMetres;
						break;
					case UQType.Length:
						this.FromUQ = Core.Constants.Length.Metres;
						break;
					case UQType.Area:
						this.FromUQ = Core.Constants.Area.SquareMetre;
						break;
				}
				this.ToUQ = toUQ;
				this.Type = type;
			}

			readonly ZString Type;
			readonly ZString FromUQ;
			readonly ZString ToUQ;

			#region IUnitConverter Members

			public ZString ParentUnit
			{
				get { return ToUQ; }
			}

			public ZString ChildUnit
			{
				get { return FromUQ; }
			}

			public ZDecimal ConversionFactor
			{
				get
				{
					switch (Type)
					{
						case UQType.Weight:
							return Core.Constants.Weight.Convert(1m, ParentUnit, ChildUnit);
						case UQType.Volume:
							return Core.Constants.Volume.Convert(1m, ParentUnit, ChildUnit);
						case UQType.Length:
							return Core.Constants.Length.Convert(1m, ParentUnit, ChildUnit);
						case UQType.Area:
							return Core.Constants.Area.Convert(1m, ParentUnit, ChildUnit);
						default:
							return 0m;
					}
				}
			}

			#endregion

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		static class UQType
		{
			public const string Weight = "Weight";
			public const string Volume = "Volume";
			public const string Length = "Length";
			public const string Area = "Area";
		}

		IEnumerable<CoreConversion> CoreUnits
		{
			get
			{
				return Factory.GetCachedValue("UnitConverterCoreUnits", () =>
				{
					var fCoreUnits = new List<CoreConversion>(27);
					fCoreUnits.Add(new CoreConversion(UQType.Weight, Core.Constants.Weight.Grams));
					fCoreUnits.Add(new CoreConversion(UQType.Weight, Core.Constants.Weight.Ounces));
					fCoreUnits.Add(new CoreConversion(UQType.Weight, Core.Constants.Weight.OuncesTroy));
					fCoreUnits.Add(new CoreConversion(UQType.Weight, Core.Constants.Weight.Pounds));
					fCoreUnits.Add(new CoreConversion(UQType.Weight, Core.Constants.Weight.PoundsTroy));
					fCoreUnits.Add(new CoreConversion(UQType.Weight, Core.Constants.Weight.Tonnes));
					fCoreUnits.Add(new CoreConversion(UQType.Weight, Core.Constants.Weight.ShortTons));
					fCoreUnits.Add(new CoreConversion(UQType.Weight, Core.Constants.Weight.LongTons));

					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.CubicCentimeters));
					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.CubicDecimetres));
					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.CubicFeet));
					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.CubicInches));
					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.CubicYards));
					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.Litre));
					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.MegaLitre));
					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.USGallons));
					fCoreUnits.Add(new CoreConversion(UQType.Volume, Core.Constants.Volume.ImperialGallons));

					fCoreUnits.Add(new CoreConversion(UQType.Length, Core.Constants.Length.Feet));
					fCoreUnits.Add(new CoreConversion(UQType.Length, Core.Constants.Length.Inches));
					fCoreUnits.Add(new CoreConversion(UQType.Length, Core.Constants.Length.Millimetres));
					fCoreUnits.Add(new CoreConversion(UQType.Length, Core.Constants.Length.Yards));
					fCoreUnits.Add(new CoreConversion(UQType.Length, Core.Constants.Length.Centimetres));

					fCoreUnits.Add(new CoreConversion(UQType.Area, Core.Constants.Area.SquareCentimetre));
					fCoreUnits.Add(new CoreConversion(UQType.Area, Core.Constants.Area.SquareFoot));
					fCoreUnits.Add(new CoreConversion(UQType.Area, Core.Constants.Area.SquareInch));
					fCoreUnits.Add(new CoreConversion(UQType.Area, Core.Constants.Area.SquareMillimetre));
					fCoreUnits.Add(new CoreConversion(UQType.Area, Core.Constants.Area.SquareYard));
					return fCoreUnits;
				});
			}
		}

		#endregion

	}
}
