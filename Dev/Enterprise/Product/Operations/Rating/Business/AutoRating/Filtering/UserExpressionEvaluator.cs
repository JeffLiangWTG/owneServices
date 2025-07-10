using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	sealed class UserExpressionEvaluator : IUserExpressionEvaluator, IDisposable
	{
		public UserExpressionEvaluator(BusinessObject bizObj)
		{
			this.bizObj = bizObj;
		}

		readonly BusinessObject bizObj;

		/// <summary>
		/// This is to contain data source for DocEngine macros, it needs to be reset when the data source or any of it's children changes, so in practice each time we autorate
		/// </summary>
		readonly IDictionary<ZGuid, IBODocDataProvider[]> docEngineDataProviderCache = new Dictionary<ZGuid, IBODocDataProvider[]>();

		/// <summary>
		/// This is to store the results of macro evaluation for the same business object for same macro.
		/// It needs to be reset when the data source or any of it's children changes, so in practice each time we autorate
		/// </summary>
		readonly IDictionary<string, bool?> resultCache = new Dictionary<string, bool?>();

		/// <summary>
		/// Clears session caches (the ones that are depened on business object state)
		/// </summary>
		public void Dispose()
		{
			docEngineDataProviderCache.Clear();
			resultCache.Clear();

			//Clearing Factory Cache that is created in DocJobHeader.New
			if (bizObj is IJobInvoicingPlugIn jobInvoicingPlugIn && jobInvoicingPlugIn?.InvoicingSupporter?.Job != null)
			{
				bizObj.Factory.ClearCachedValue<DocumentWrapper>(jobInvoicingPlugIn.InvoicingSupporter.Job.PK.ToStringKey());
			}
		}

#if DEBUG
		/// <summary>
		/// Forces to clears static caches (the ones that are not depended on business object state)
		/// </summary>
		public static void ClearStaticCaches()
		{
			foreach (var item in MemoryCache.Default.Cast<KeyValuePair<string, object>>().ToArray())
			{
				if (item.Key?.StartsWith(cacheKeyPrefix, StringComparison.OrdinalIgnoreCase) ?? false)
				{
					MemoryCache.Default.Remove(item.Key);
				}
			}
		}
#endif

		public bool? IsUserDefinedConditionMet(ZString conditionalExpression)
		{
			if (string.IsNullOrWhiteSpace(conditionalExpression))
			{
				return null;
			}

			if (resultCache.TryGetValue(conditionalExpression, out var res))
			{
				return res;
			}

			var expressionType = GetExpressionTypeFromCache(conditionalExpression);

			switch (expressionType)
			{
				case ExpressionType.FormBuilder:
					res = TryToEvaluateUsingFormBuilder(conditionalExpression, bizObj);
					break;

				case ExpressionType.DocEngine:
					res = TryToEvaluateUsingDocumentEngine(conditionalExpression, bizObj);
					break;

				default:
					res = TryToEvaluateUsingFormBuilder(conditionalExpression, bizObj, true)
						?? TryToEvaluateUsingDocumentEngine(conditionalExpression, bizObj, true);
					break;
			}

			resultCache[conditionalExpression] = res;

			return res;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "variable name")]
		bool? TryToEvaluateUsingFormBuilder(ZString conditionalExpression, BusinessObject bizObj, bool addToCache = false)
		{
			bool? res = null;

			var expr = GetOrCreateMacroExpression(conditionalExpression);

			using (var scope = new MacroScope(bizObj))
			{
				const string enviromentVariableName = "env";
				scope.SetVariable(enviromentVariableName, new MasterFiles.Business.Macros.Environment());

				var evaluationResult = expr.Evaluate(scope);

				switch (evaluationResult)
				{
					case bool boolRes:
						res = boolRes;
						break;

					case ZBool zBoolRes:
						res = zBoolRes;
						break;
				}
			}

			if (addToCache
				&& res.HasValue)
			{
				AddExpressionTypeToCache(conditionalExpression, ExpressionType.FormBuilder);
			}

			return res;
		}

		IMacroExpression GetOrCreateMacroExpression(string conditionalExpression)
		{
			var macroExpression = GetMacroExpressionFromCache(conditionalExpression);

			if (macroExpression == null)
			{
				macroExpression = CreateNewMacroExpression(conditionalExpression);
				AddMacroExpressionToCache(conditionalExpression, macroExpression);
			}

			return macroExpression;
		}

		IMacroExpression CreateNewMacroExpression(string conditionalExpression)
		{
			return conditionalExpression
				.With<StandardLibrary>()
				.And<RateLineConditionMacroLibrary>()
				.CreateExpression();
		}

		bool? TryToEvaluateUsingDocumentEngine(ZString conditionalExpression, BusinessObject bizObj, bool addToCache = false)
		{
			try
			{
				var useJSEngine = RawDataRegistry.Instance.UseJSEngineForAutoRatingConditionsEvaluation.Value;
				var providers = GetDocEngineDataProviders(bizObj);

				var res = ZExpressionEvaluator.Evaluate(conditionalExpression, useJSEngine, bizObj as IDocumentSupportable, providers);

				if (addToCache)
				{
					AddExpressionTypeToCache(conditionalExpression, ExpressionType.DocEngine);
				}

				return res;
			}
			catch (ExpressionEvaluationException)
			{
				return null;
			}
		}

		IBODocDataProvider[] GetDocEngineDataProviders(BusinessObject bizObj)
		{
			var cacheKey = bizObj?.PK ?? ZGuid.Empty;

			if (docEngineDataProviderCache.TryGetValue(cacheKey, out var res))
			{
				return res;
			}

			var providers = CreateDocEngineDataProviders(bizObj);
			docEngineDataProviderCache[cacheKey] = providers;
			return providers;
		}

		IBODocDataProvider[] CreateDocEngineDataProviders(BusinessObject bizObj)
		{
			var docWrappers = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, bizObj);
			var providers = new List<IBODocDataProvider>(docWrappers.Length + 1);
			providers.AddRange(docWrappers.Cast<IBODocDataProvider>().ToArray());
			providers.Add(BODocDataProvider.Get(bizObj));

			return providers.ToArray();
		}

		const string cacheKeyPrefix = "AutoRating:UserExpressionEvaluator:";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int cacheDurationInHours = 24;

		IMacroExpression GetMacroExpressionFromCache(string conditionalExpression)
		{
			var cacheKey = CreateMacroExpressionCacheKey(conditionalExpression);

			return MemoryCache.Default.Get(cacheKey) as IMacroExpression;
		}

		void AddMacroExpressionToCache(string conditionalExpression, IMacroExpression macroExpression)
		{
			var cacheKey = CreateMacroExpressionCacheKey(conditionalExpression);

			MemoryCache.Default.Set(cacheKey, macroExpression, CreateCachePolicy());
		}

		string CreateMacroExpressionCacheKey(string conditionalExpression) => string.Concat(cacheKeyPrefix, nameof(IMacroExpression), conditionalExpression);

		ExpressionType GetExpressionTypeFromCache(string conditionalExpression)
		{
			var cacheKey = CreateExpressionTypeCacheKey(conditionalExpression);

			var value = MemoryCache.Default.Get(cacheKey);

			return value is ExpressionType expressionType
				? expressionType
				: ExpressionType.Undefined;
		}

		void AddExpressionTypeToCache(string conditionalExpression, ExpressionType expressionType)
		{
			var cacheKey = CreateExpressionTypeCacheKey(conditionalExpression);

			MemoryCache.Default.Set(cacheKey, expressionType, CreateCachePolicy());
		}

		string CreateExpressionTypeCacheKey(string conditionalExpression) => string.Concat(cacheKeyPrefix, bizObj?.GetType().Name, conditionalExpression);

		CacheItemPolicy CreateCachePolicy() => new CacheItemPolicy
		{
			AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(cacheDurationInHours)
		};

		enum ExpressionType
		{
			Undefined,
			FormBuilder,
			DocEngine
		}
	}
}
