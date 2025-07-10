using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionPathway : AutoRefCusProfileQuestionPathway
	{
		public RefCusProfileQuestionPathway(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public RefCusProfileQuestion QuestionParent => Factory.Load<RefCusProfileQuestion>(XQP_XQ2_QuestionParent);

		public RefCusProfileQuestion QuestionChild => Factory.Load<RefCusProfileQuestion>(XQP_XQ2_QuestionChild);

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefCusProfileQuestionPathway[] Load(ZGuid[] parentQuestionPKs, ZDateTime date, bool recursive = false)
			{
				Argument.NotNull(parentQuestionPKs, nameof(parentQuestionPKs));

				var effectiveDate = date.IsValid ? date.Date : ZDateTime.Today;

				var result = LoadFromCacheOnly(parentQuestionPKs, effectiveDate, recursive, out var nonCachedParentPKs);

				if (nonCachedParentPKs.Count > 0)
				{
					result.AddRange(LoadFromDatabase(parentQuestionPKs, effectiveDate, recursive));
				}

				return result.Distinct().ToArray();
			}

			string GetCacheKey(ZGuid questionPK, ZDateTime effectiveDate, bool recursive) => $"LoadRefCusProfileQuestionPathway_{questionPK}_{effectiveDate}_{recursive}";

			List<RefCusProfileQuestionPathway> LoadFromCacheOnly(ZGuid[] parentQuestionPKs, ZDateTime effectiveDate, bool recursive, out List<ZGuid> nonCachedParentPKs)
			{
				var result = new List<RefCusProfileQuestionPathway>();
				nonCachedParentPKs = new List<ZGuid>();
				
				foreach (var questionPK in parentQuestionPKs)
				{
					if (Factory.TryGetValueFromCacheOnly(GetCacheKey(questionPK, effectiveDate, recursive), out RefCusProfileQuestionPathway[] cachedPathways))
					{
						result.AddRange(cachedPathways);
					}
					else
					{
						nonCachedParentPKs.Add(questionPK);
					}
				}

				return result;
			}

			RefCusProfileQuestionPathway[] LoadFromDatabase(ZGuid[] parentQuestionPKs, ZDateTime effectiveDate, bool recursive)
			{
				if (parentQuestionPKs.Length == 0)
				{
					return Array.Empty<RefCusProfileQuestionPathway>();
				}

				var filter = recursive ? GetRecursiveFilter(parentQuestionPKs, effectiveDate) : GetFilter(parentQuestionPKs, effectiveDate);
				var result = Factory.Load<RefCusProfileQuestionPathway>(filter);
				return parentQuestionPKs.SelectMany(questionPK => Factory.GetCachedValue(GetCacheKey(questionPK, effectiveDate, recursive),
					() => LoadFromCollection(result, questionPK, recursive).ToArray())).Distinct().ToArray();
			}

			ZQuery GetRecursiveFilter(ZGuid[] parentQuestionPKs, ZDateTime effectiveDate)
			{
				var recursiveQuery = (NoResString)$@"WITH recursive_cte AS (
	SELECT XQP_PK, XQP_XQ2_QuestionParent, XQP_XQ2_QuestionChild
	FROM {RefCusProfileQuestionPathwaySchema.Constants.TableName}
	WHERE XQP_XQ2_QuestionParent IN (SELECT Value FROM @parentID) AND XQP_StartDate <= @effectiveDate AND XQP_EndDate >= @effectiveDate
	UNION ALL
	SELECT main.XQP_PK, main.XQP_XQ2_QuestionParent, main.XQP_XQ2_QuestionChild
	FROM {RefCusProfileQuestionPathwaySchema.Constants.TableName} AS main
	JOIN recursive_cte AS rc
	ON main.XQP_XQ2_QuestionParent = rc.XQP_XQ2_QuestionChild AND main.XQP_StartDate <= @effectiveDate AND main.XQP_EndDate >= @effectiveDate
)
SELECT XQP_PK
FROM recursive_cte";

				var parameters = new ZSqlParameterCollection()
				{
					ZSqlParameter.New("@parentID", parentQuestionPKs.ToList(), RefCusProfileQuestionPathwaySchema.XQP_XQ2_QuestionParent, isTableValued: true),
					ZSqlParameter.New("@effectiveDate", effectiveDate, RefCusProfileQuestionPathwaySchema.XQP_StartDate)
				};

				var queryResult = new DynamicBusinessObjectCollection(Factory);
				queryResult.Load(recursiveQuery, parameters);

				return new ZQuery(RefCusProfileQuestionPathwaySchema.PK, queryResult.Select(x => (ZGuid)x["XQP_PK"]));
			}

			ZQuery GetFilter(ZGuid[] parentQuestionPKs, ZDateTime effectiveDate)
			{
				return new ZQuery(RefCusProfileQuestionPathwaySchema.XQP_XQ2_QuestionParent, parentQuestionPKs)
					.AddToFilter(RefCusProfileQuestionPathwaySchema.XQP_StartDate, SQLComparisonOperator.LessThanOrEqualTo, effectiveDate)
					.AddToFilter(RefCusProfileQuestionPathwaySchema.XQP_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, effectiveDate);
			}

			IEnumerable<RefCusProfileQuestionPathway> LoadFromCollection(RefCusProfileQuestionPathway[] pathways, ZGuid questionPK, bool recursive)
			{
				foreach (var pathway in pathways.Where(x => x.XQP_XQ2_QuestionParent == questionPK))
				{
					yield return pathway;

					if (recursive)
					{
						foreach (var child in LoadFromCollection(pathways, pathway.XQP_XQ2_QuestionChild, recursive))
						{
							yield return child;
						}
					}
				}
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusProfileQuestionPathway);
		}
	}
}
