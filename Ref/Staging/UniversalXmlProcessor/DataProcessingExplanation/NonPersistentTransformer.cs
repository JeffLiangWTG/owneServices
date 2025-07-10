using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.DataProcessingExplanation;

static class NonPersistentTransformer
{
	public static string TransformToPersistentPropertyName(string message, IEnumerable<string> propertyNames, bool replaceFullGroup = false)
	{
		return TransformToPersistent(message, propertyNames, GetPersistnetPropertyName, replaceFullGroup);
	}

	public static string TransformToPersistentTablePrefix(string message, IEnumerable<string> tablePrefixes, bool replaceFullGroup = false)
	{
		return TransformToPersistent(message, tablePrefixes, GetPersistnetTablePrefixes, replaceFullGroup);
	}

	public static string TransformToPersistentType(string message, IEnumerable<Type> types, bool replaceFullGroup = false)
	{
		return TransformToPersistent(message, types.Select(t => t.Name), GetPersistnetTypeName, replaceFullGroup);
	}

	static string TransformToPersistent(string message, IEnumerable<string> items, Func<string, IEnumerable<string>> transformFunc, bool replaceFullGroup = false)
	{
		foreach (var item in items)
		{
			var transformed = transformFunc(item);
			var result = transformed.LastOrDefault();
			if (replaceFullGroup && transformed.Count() > 1)
			{
				result = string.Join(",", transformed);
			}

			message = message.Replace(item, result);
		}

		return message;
	}

	public static IEnumerable<string> GetPersistnetTypeName(string typeName)
	{
		return TypeNameDictionary.Value.TryGetValue(typeName, out var persistentTypeNames)
			? persistentTypeNames
			: [typeName];
	}

	public static IEnumerable<string> GetPersistnetTablePrefixes(string tablePrefix)
	{
		return TablePrefixDictionary.Value.TryGetValue(tablePrefix, out var persistentTablePrefixs)
			? persistentTablePrefixs
			: [tablePrefix];
	}

	public static IEnumerable<string> GetPersistnetPropertyName(string propertyName)
	{
		return PropertyNameDictionary.Value.TryGetValue(propertyName, out var persistentPropertyNames)
			? persistentPropertyNames
			: [propertyName];
	}

	static readonly Lazy<Dictionary<string, IEnumerable<string>>> PropertyNameDictionary = new(() =>
		{
			return SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup().Union(SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup())
				.DistinctBy(x => x.GetType())
				.SelectMany(x => x.PropertyMappers)
				.GroupBy(x => x.transformedProperty)
				.ToDictionary(x => x.Key, x => x.Select(y => y.originalProperty));
		});

	static readonly Lazy<Dictionary<string, IEnumerable<string>>> TypeNameDictionary = new(() =>
	{
		var result = SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup().Union(SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup())
			.DistinctBy(x => x.GetType())
			.Select(x => x.NameMapper)
			.GroupBy(x => x.transformedName)
			.ToDictionary(x => x.Key, x => x.Select(y => y.originalName));

		result.Add(SchemaMapperHelper.ConditionWithoutApplicabilityNameMapper.transformedName, [SchemaMapperHelper.ConditionWithoutApplicabilityNameMapper.originalName]);
		result.Add(SchemaMapperHelper.RateWithoutApplicabilityNameMapper.transformedName, [SchemaMapperHelper.RateWithoutApplicabilityNameMapper.originalName]);
		return result;
	});

	static readonly Lazy<Dictionary<string, IEnumerable<string>>> TablePrefixDictionary = new(() =>
	{
		return SchemaMapperHelper.GetSchemaMappers_RateAndApplicabilityGroup().Union(SchemaMapperHelper.GetSchemaMappers_ConditionAndApplicabilityGroup())
			.DistinctBy(x => x.GetType())
			.Select(x => x.TablePrefixMapper)
			.GroupBy(x => x.transformedTablePrefix)
			.ToDictionary(x => x.Key, y => y.Select(z => z.originalTablePrefix));
	});
}
