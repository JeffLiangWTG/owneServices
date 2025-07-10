using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor;

public static class TransformHelper
{
	readonly static Type[] topLevelTables = {
			typeof(RefCarrierCode),
			typeof(RefCusCodeList),
			typeof(RefCusCodeType),
			typeof(RefCusConditionType),
			typeof(RefCusConditionValueType),
			typeof(RefCusMap),
			typeof(RefCusMapType),
			typeof(RefCusNomenclatureGroup),
			typeof(RefCusNomenclatureGroupType),
			typeof(RefCusPreference),
			typeof(RefCusProcedure),
			typeof(RefCusRateType),
			typeof(RefCusTariff),
			typeof(RefCusTariffType),
			typeof(RefCusTaxOrFeeType),
			typeof(RefCusTradeGroup),
			typeof(RefDataGrouping),
			typeof(RefExchangeRateZZ),
			typeof(RefVesselZZ),
			typeof(UNDGCommonData),
			typeof(UNDGSubstance),
			typeof(RefCountry),
			typeof(RefTimeZoneSet),
			typeof(RefCusRuling),
			typeof(RefUNLOCO),
			typeof(RefAccTaxRate),
			typeof(RefCurrency),
			typeof(RefSysConfigType),
			typeof(RefShippingLine)
		};

	public static void Transform<T>(T result, IEnumerable<ITransformStrategy> strategies)
	{
		Argument.NotNull(strategies, nameof(strategies));

		foreach (var stratergy in strategies)
		{
			Transform(result, stratergy);
		}
	}

	public static bool NeedTransform(Type type, ITransformStrategy transformStrategy)
	{
		Argument.NotNull(type, nameof(type));
		Argument.NotNull(transformStrategy, nameof(transformStrategy));

		if (type.IsArray)
		{
			var elementType = type.GetElementType();
			return NeedTransform(elementType, transformStrategy);
		}

		if (NotTransformableType(type))
		{
			return false;
		}

		var planner = PathPlannerFactory.Create(transformStrategy.ToString(), type);
		if (!planner.IsFirstTimeCreated)
		{
			return planner.PathNodeTouched;
		}

		if (transformStrategy.ToBeTransformedTypes.Any(x => x == type))
		{
			return planner.PathNodeTouched = true;
		}

		PropertyInfo[] properties;
		if (IsTopLevelTable(type))
		{
			properties = GetPropertiesCache(type);
			foreach (var propertyInfo in properties)
			{
				if (transformStrategy.ToBeTransformedTypes.Any(x => x == propertyInfo.PropertyType))
				{
					return planner.PathNodeTouched = true;
				}
			}
			return false;
		}

		properties = GetPropertiesCache(type);
		foreach (var propertyInfo in properties)
		{
			if (NeedTransform(propertyInfo.PropertyType, transformStrategy))
			{
				return planner.PathNodeTouched = true;
			}
		}
		return false;
	}

	public static bool Transform<T>(T result, ITransformStrategy transformStrategy)
	{
		Argument.NotNull(result, nameof(result));
		Argument.NotNull(transformStrategy, nameof(transformStrategy));

		transformStrategy.Transform(result);

		var type = result.GetType();
		var propertyInfos = GetPropertiesCache(type);
		foreach (var propertyInfo in propertyInfos)
		{
			var propertyValue = type.GetProperty(propertyInfo.Name)?.GetValue(result, null);
			TransformCoreType(propertyValue, propertyInfo.PropertyType, transformStrategy);
		}
		return true;
	}

	static void TransformCoreType<T>(T result, Type type, ITransformStrategy transformStrategy)
	{
		Argument.NotNull(type, nameof(type));
		Argument.NotNull(transformStrategy, nameof(transformStrategy));

		if (result == null)
		{
			return;
		}

		if (!NeedTransform(type, transformStrategy))
		{
			return;
		}

		if (type.IsArray)
		{
			foreach (var item in (IEnumerable)result)
			{
				TransformCoreType(item, item.GetType(), transformStrategy);
			}
			return;
		}

		if (NotTransformableType(type))
		{
			return;
		}

		transformStrategy.Transform(result);

		PropertyInfo[] properties;
		if (IsTopLevelTable(type))
		{
			properties = GetPropertiesCache(type);
			foreach (var propertyInfo in properties)
			{
				var propertyValue = type.GetProperty(propertyInfo.Name)?.GetValue(result, null);
				if (propertyValue != null)
				{
					transformStrategy.Transform(propertyValue);
				}
			}
			return;
		}

		properties = GetPropertiesCache(type);
		foreach (var propertyInfo in properties)
		{
			var propertyValue = type.GetProperty(propertyInfo.Name)?.GetValue(result, null);
			if (propertyValue != null)
			{
				TransformCoreType(propertyValue, propertyValue.GetType(), transformStrategy);
			}
		}
	}

	static bool NotTransformableType(Type type)
	{
		Argument.NotNull(type, nameof(type));

		return (type == typeof(string) || type.IsPrimitive || type == typeof(DateTime));
	}

	static bool IsTopLevelTable(Type type)
	{
		return topLevelTables.Contains(type);
	}

	static ConcurrentDictionary<Type, PropertyInfo[]> dict = new ConcurrentDictionary<Type, PropertyInfo[]>();

	static PropertyInfo[] GetPropertiesCache(Type type)
	{
		Argument.NotNull(type, nameof(type));

		if (dict.ContainsKey(type))
		{ return dict[type]; }
		return dict[type] = type.GetProperties();
	}
}

public class TransformStrategyPicker
{
	public virtual IEnumerable<ITransformStrategy> GetStrategies(int version, Type dataSetType)
	{
		return AllStrategies.Where(x => x.RequireTransform(dataSetType, version));
	}

	static IEnumerable<ITransformStrategy> AllStrategies
	{
		get
		{
			yield return V10Transform.Instance();
			yield return V13Transform.Instance();
			yield return V14Transform.Instance();
			yield return V15Transform.Instance();
			yield return V17Transform.Instance();
			yield return V19Transform.Instance();
			yield return V21Transform.Instance();
			yield return V24Transform.Instance();
			yield return V26Transform.Instance();
			yield return V27Transform.Instance();
			yield return V28Transform.Instance();
			yield return V29Transform.Instance();
			yield return V30Transform.Instance();
			yield return V31Transform.Instance();
			yield return V32Transform.Instance();
			yield return V33Transform.Instance();
			yield return V37Transform.Instance();
			yield return V38Transform.Instance();
			yield return V40Transform.Instance();
			yield return V44Transform.Instance();
			yield return V46Transform.Instance();
			yield return V54Transform.Instance();
			yield return V59Transform.Instance();
			yield return V82Transform.Instance();
			yield return V87Transform.Instance();
			yield return V89Transform.Instance();
			yield return V97Transform.Instance();
			yield return V107Transform.Instance();
			yield return V108Transform.Instance();
			yield return V112Transform.Instance();
			yield return V113Transform.Instance();
			yield return V118Transform.Instance();
			yield return V125Transform.Instance();
			yield return V139Transform.Instance();
			yield return V141Transform.Instance();
			yield return V147Transform.Instance();
			yield return V164Transform.Instance();
			yield return PriorV20Transform.Instance();
			yield return OffsetFromUtcTransform.Instance();
		}
	}

	public virtual IEnumerable<ITransformStrategy> GetStrategiesBySRDbVersion(int sRDbVersion, Type dataSetType)
	{
		return NewStrategies.Where(x => x.RequireTransform(dataSetType, sRDbVersion));
	}

	static IEnumerable<ITransformStrategy> NewStrategies =>
	[
#if DEBUG
		SV564Transform.Instance()   //A sample to tranform by SRDbVersion.
#endif
	];
}

public static class PathPlannerFactory
{
	static readonly ConcurrentDictionary<string, PathPlanner> dict = new ConcurrentDictionary<string, PathPlanner>();

	public static PathPlanner Create(string key, Type type)
	{
		Argument.NotNull(key, nameof(key));
		Argument.NotNull(type, nameof(type));

		var combinedKey = key + type.ToString();
		if (dict.ContainsKey(combinedKey))
		{
			dict[combinedKey].IsFirstTimeCreated = false;
			return dict[combinedKey];
		}

		var planner = new PathPlanner();
		planner.IsFirstTimeCreated = true;
		dict[combinedKey] = planner;
		return planner;
	}
}

public class PathPlanner
{
	public bool IsFirstTimeCreated { get; set; }

	public bool PathNodeTouched { get; set; }
}
