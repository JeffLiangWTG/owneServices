using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class ExpandHelper
	{
		public static void Expand<T>(this IReferenceDataRepository repo, IEnumerable<T> objs, IExpandClauseWrapper expandClause)
		{
			Argument.NotNull(repo, nameof(repo));
			Argument.NotNull(objs, nameof(objs));
			Argument.NotNull(expandClause, nameof(expandClause));

			foreach (var expandedItem in expandClause.GetExpandClauseWrapper())
			{
				Expand(objs, repo, expandedItem);
			}
		}

		static void Expand<T>(IEnumerable<T> objs, IReferenceDataRepository repo, IExpandedItemWrapper expandedItem)
		{
			Argument.NotNull(repo, nameof(repo));
			Argument.NotNull(objs, nameof(objs));
			Argument.NotNull(expandedItem, nameof(expandedItem));

			typeof(ExpandHelper).InvokeStaticGenericMethod(nameof(ExpandHelper.ExpandCore), new[] { typeof(T), expandedItem.GetExpandType() }, objs, repo, expandedItem);
		}

		static void ExpandCore<T, TRelated>(IEnumerable<T> objs, IReferenceDataRepository repo, IExpandedItemWrapper expandedItem) where TRelated : class
		{
			Argument.NotNull(repo, nameof(repo));
			Argument.NotNull(objs, nameof(objs));
			Argument.NotNull(expandedItem, nameof(expandedItem));

			var relatedEntities = Expand<T, TRelated>(objs, repo);
			var expandClause = expandedItem.GetExpandClause();
			if (expandClause != null)
			{
				Expand(repo, relatedEntities, expandClause);
			}
		}

		static IEnumerable<TRelated> Expand<T, TRelated>(IEnumerable<T> objs, IReferenceDataRepository repo) where TRelated : class
		{
			Argument.NotNull(repo, nameof(repo));
			Argument.NotNull(objs, nameof(objs));

			var fkColumn = typeof(TRelated).GetFKPropertyInfo(typeof(T));
			if (fkColumn != null)
			{
				// T => TRelated: 1 to many
				var pkExp = ExpressionHelper.GetPKExpression<T>();
				var pks = objs.ToDictionary(pkExp.Compile(), x => x);
				var fkExp = ExpressionHelper.GetStructPropertyExpression<TRelated, Guid>(fkColumn);
				var containExp = ExpressionHelper.ContainsStructPropertyExpression<TRelated, Guid>(pks.Keys.ToList(), fkColumn);
				var relatedEntitiesDict = repo.Get<TRelated>()?.Where(containExp)?.GroupBy(fkExp)?.ToDictionary(x => x.Key, x => x) ?? new Dictionary<Guid, IGrouping<Guid, TRelated>>();
				var navigationProperty = typeof(T).GetCollectionNavigationPropertyInfo(typeof(TRelated));
				foreach (var pk in pks)
				{
					if (relatedEntitiesDict.ContainsKey(pk.Key))
					{
						var collection = (ICollection<TRelated>)navigationProperty.GetValue(pk.Value);
						var relatedEntities = relatedEntitiesDict[pk.Key];
						foreach (var relatedEntity in relatedEntities)
						{
							collection.Add(relatedEntity);
						}
					}
				}
				return relatedEntitiesDict.Values.SelectMany(x => x);
			}
			else
			{
				// T => TRelated: many to 1
				fkColumn = typeof(T).GetFKPropertyInfo(typeof(TRelated));
				var fkGroups = new Dictionary<Guid, List<T>>();
				bool nullable = fkColumn.PropertyType.Name.StartsWith("Nullable", StringComparison.OrdinalIgnoreCase);
				if (!nullable)
				{
					fkGroups = objs.GroupBy(ExpressionHelper.GetPropertyExpression<T, Guid>(fkColumn).Compile()).ToDictionary(g => g.Key, g => g.ToList());
				}
				else
				{
					var expression = ExpressionHelper.GetPropertyExpression<T, Guid?>(fkColumn);
					fkGroups = objs.GroupBy(expression.Compile()).Where(x => x.Key.HasValue).ToDictionary(g => g.Key.Value, g => g.ToList());
				}
				var pkInfo = typeof(TRelated).GetPKPropertyInfo();
				var containExp = ExpressionHelper.ContainsStructPropertyExpression<TRelated, Guid>(fkGroups.Keys.ToList(), pkInfo);
				var relatedEntities = repo.Get<TRelated>()?.Where(containExp)?.ToList() ?? new List<TRelated>();
				var navigationProperty = typeof(T).GetNavigationPropertyInfo(typeof(TRelated));
				foreach (var relatedEntity in relatedEntities)
				{
					var pk = (Guid) pkInfo.GetValue(relatedEntity);
					foreach (var obj in fkGroups[pk])
					{
						navigationProperty.SetValue(obj, relatedEntity);
					}
				}
				return relatedEntities;
			}
		}
	}
}
