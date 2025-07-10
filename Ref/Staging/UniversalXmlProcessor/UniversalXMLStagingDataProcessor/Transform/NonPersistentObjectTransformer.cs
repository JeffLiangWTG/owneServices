using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor;

public class NonPersistentObjectTransformer(ISafeDataProvider safeDataProvider) : INonPersistentObjectTransformer
{
	public SafeObjectUpdaterResult[] TransformNonPersistentObjects(SafeObjectUpdaterResult[] updaterResults)
	{
		var result = new List<SafeObjectUpdaterResult>();

		safeDataProvider.SavePersistentObjects();
		nonPersistentToPersistentDictionary = BuildNonPersistentToPersistentObjectDictionary();
		foreach (var updaterResult in updaterResults)
		{
			var isNonPersistentUpdaterResult = nonPersistentToPersistentDictionary.ContainsKey(updaterResult.ParentPK);
			if (isNonPersistentUpdaterResult)
			{
				result.AddRange(TransformToPersistentUpdaterResults(updaterResult));
			}
			else
			{
				result.Add(updaterResult);
			}
		}

		return result.DistinctBy(x => x.ParentPK).ToArray();
	}

	List<SafeObjectUpdaterResult> TransformToPersistentUpdaterResults(SafeObjectUpdaterResult updaterResult)
	{
		List<SafeObjectUpdaterResult> result = [];

		var persistentObjects = nonPersistentToPersistentDictionary[updaterResult.ParentPK];
		foreach (var (tableType, tableCode, pk) in persistentObjects)
		{
			var newUpdaterResult = new SafeObjectUpdaterResult
			{
				ParentCode = tableCode,
				ParentPK = pk,
				Action = updaterResult.Action,
				DatasetPK = updaterResult.DatasetPK,
				ExpirableAncestorPK = GetExpirableAncestorPK(updaterResult, tableType, persistentObjects),
				NewRecordForCloneActionPK = GetNewRecordForCloneActionPK(updaterResult)
			};

			result.Add(newUpdaterResult);
		}

		return result;
	}

	Guid? GetExpirableAncestorPK(SafeObjectUpdaterResult updaterResult, Type tableType,
		(Type TableType, string TableCode, Guid PK)[] persistentObjects)
	{
		return tableType switch
		{
			{ Name: nameof(RefCusRate) or nameof(RefCusCondition) } => updaterResult.ExpirableAncestorPK,
			{ Name: nameof(RefCusApplicability) } => persistentObjects.FirstOrDefault(x => x.TableCode is "ZZ2" or "ZX1").PK,
			{ Name: nameof(RefCusRateUOM) } => nonPersistentToPersistentDictionary[updaterResult.ExpirableAncestorPK!.Value].FirstOrDefault(x => x.TableCode is "ZZ2").PK,
			{ Name: nameof(RefCusExcludedTradeGroup) } => nonPersistentToPersistentDictionary[updaterResult.ExpirableAncestorPK!.Value].FirstOrDefault(x => x.TableCode is "ZZT").PK,
			{ Name: nameof(RefCusConditionValue) } => nonPersistentToPersistentDictionary[updaterResult.ExpirableAncestorPK!.Value].FirstOrDefault(x => x.TableCode is "ZX1").PK,
			{ Name: nameof(RefCusConditionLanguage) } => nonPersistentToPersistentDictionary[updaterResult.ExpirableAncestorPK!.Value].FirstOrDefault(x => x.TableCode is "ZX1").PK,
			_ => throw new NotImplementedException($"Expirable Ancestor PK for {tableType} is not implemented.")
		};
	}

	Guid? GetNewRecordForCloneActionPK(SafeObjectUpdaterResult updaterResult)
	{
		var newRecordForCloneActionPK = updaterResult.NewRecordForCloneActionPK;
		if (updaterResult.NewRecordForCloneActionPK.HasValue
			&& nonPersistentToPersistentDictionary.ContainsKey(updaterResult.NewRecordForCloneActionPK.Value))
		{
			var objs = nonPersistentToPersistentDictionary[updaterResult.ParentPK];
			newRecordForCloneActionPK = objs.Length > 1
				? nonPersistentToPersistentDictionary[updaterResult.ParentPK]
					.FirstOrDefault(x => x.TableCode is "ZZ2" or "ZX1").PK
				: nonPersistentToPersistentDictionary[updaterResult.ParentPK].First().PK;
		}

		return newRecordForCloneActionPK;
	}

	/// <summary>
	///   Key: Non-Persistent Object PK
	///   Value: Related Persistent Object Type, Table Code, and PK, e.g. RateApp -> Rate + App
	/// </summary>
	Dictionary<Guid, (Type TableType, string TableCode, Guid PK)[]> BuildNonPersistentToPersistentObjectDictionary()
	{
		var persistentObjects = safeDataProvider.GetAllPersistentObjects();
		return persistentObjects
			.SelectMany(x =>
				GetNonPersistentObjects(x).Where(y => y != null).Select(y => Tuple.Create(y.GetPKValue(), x))
					.Distinct())
			.GroupBy(x => x.Item1)
			.ToDictionary(k => k.Key,
				v => v.Select(x => (x.Item2.GetEntityType(), x.Item2.GetEntityType().GetTablePrefix(),
					x.Item2.GetPKValue())).ToArray());
	}

	static IEnumerable<object> GetNonPersistentObjects<T>(T obj)
	{
		return obj switch
		{
			RefCusApplicability app => new List<object>().Union(app.RefCusRateApplicabilities)
				.Union(app.RefCusConditionApplicabilities),
			RefCusRate rate => rate.RefCusApplicabilities.SelectMany(y => y.RefCusRateApplicabilities),
			RefCusRateUOM uom => uom.RefCusRateApplicabilityUOMs,
			RefCusExcludedTradeGroup ex => ex.RefCusExcludedTradeGroupNews,
			RefCusCondition cond => cond.RefCusApplicabilities.SelectMany(y => y.RefCusConditionApplicabilities),
			RefCusConditionValue val => val.RefCusConditionApplicabilityValues,
			RefCusConditionLanguage lang => lang.RefCusConditionApplicabilityLanguages,
			_ => []
		};
	}

	Dictionary<Guid, (Type TableType, string TableCode, Guid PK)[]> nonPersistentToPersistentDictionary;
}
