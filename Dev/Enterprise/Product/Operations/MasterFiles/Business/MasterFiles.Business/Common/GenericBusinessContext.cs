using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class GenericBusinessContext<T> : IService, IGenericBusinessContext<T> where T : struct, IComparable, IConvertible
	{
		GenericBusinessContext(BusinessObjectFactory factory)
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			factory.ServiceContainer.AddService(this);
			ResetFactoryContexts();
			ResetBusinessObjectContexts();
		}

		GenericBusinessContext(BusinessObject businessObject)
			: this(businessObject.Factory)
		{
		}

		public static bool HasContext(BusinessObjectFactory factory, params T[] contexts)
		{
			var contextObj = GetOrCreateBusinessContext(factory);
			bool result = contexts.Any();
			foreach (var context in contexts)
			{
				result &= HasFactoryContext(contextObj, context);
			}
			return result;
		}

		public static bool HasAnyOfContexts(BusinessObjectFactory factory, params T[] contexts)
		{
			var contextObj = GetOrCreateBusinessContext(factory);
			bool result = false;
			foreach (var context in contexts)
			{
				result |= HasFactoryContext(contextObj, context);
			}
			return result;
		}

		public static bool HasContext(BusinessObject bizObj, T context)
		{
			var contextObj = GetOrCreateBusinessContext(bizObj);
			return HasContext(contextObj, bizObj.PK, context, true);
		}

		public bool HasAnyOfContexts(BusinessObject bizObj, params T[] contexts)
		{
			bool result = false;
			foreach (var context in contexts)
			{
				result |= HasContext(bizObj.PK, context, true);
			}

			return result;
		}

		public static bool HasContextWithoutFactoryFallback(BusinessObject bizObj, T context)
		{
			var contextObj = GetOrCreateBusinessContext(bizObj);
			return HasContext(contextObj, bizObj.PK, context, false);
		}

		static bool HasContext(GenericBusinessContext<T> contextObj, ZGuid bizObjPK, T context, bool doFactoryFallback) => contextObj.HasContext(bizObjPK, context, doFactoryFallback);

		bool HasContext(ZGuid bizObjPK, T context, bool doFactoryFallback)
		{
			var result = false;
			if (currentBusinessObjectContexts.ContainsKey(bizObjPK))
			{
				result = currentBusinessObjectContexts[bizObjPK].Any(x => x.Context.Equals(context));
			}

			if (doFactoryFallback)
			{
				result |= HasFactoryContext(context);
			}

			return result;
		}

		static bool HasFactoryContext(GenericBusinessContext<T> contextObj, T context) => contextObj.HasFactoryContext(context);

		bool HasFactoryContext(T context) => currentFactoryContexts.Any(x => x.Context.Equals(context));

		public IDisposable SetTempContext(BusinessObjectFactory factory, params T[] contexts)
		{
			return new DisposableAction(() => SetContext(factory, contexts), () => RemoveContext(factory, contexts));
		}

		public static void SetContext(BusinessObjectFactory factory, params T[] contexts)
		{
			var contextObj = GetOrCreateBusinessContext(factory);
			foreach (var context in contexts)
			{
				var existingContext = contextObj.currentFactoryContexts.FirstOrDefault(x => x.Context.Equals(context));
				if (existingContext != null)
				{
					existingContext.UseCount++;
				}
				else
				{
					contextObj.currentFactoryContexts.Add(new ContextContainer(context));
				}
			}
		}

		public static void SetContext(BusinessObject bizObj, T context)
		{
			var contextObj = GetOrCreateBusinessContext(bizObj);
			if (contextObj.currentBusinessObjectContexts.ContainsKey(bizObj.PK))
			{
				var existingContext = contextObj.currentBusinessObjectContexts[bizObj.PK].FirstOrDefault(x => x.Context.Equals(context));

				if (existingContext != null)
				{
					existingContext.UseCount++;
				}
				else
				{
					contextObj.currentBusinessObjectContexts[bizObj.PK].Add(new ContextContainer(context));
				}
			}
			else
			{
				contextObj.currentBusinessObjectContexts.Add(bizObj.PK, new List<ContextContainer>() { new ContextContainer(context) });
			}
		}

		public static void RemoveContext(BusinessObjectFactory factory, params T[] contexts)
		{
			var contextObj = GetOrCreateBusinessContext(factory);
			foreach (var context in contexts)
			{
				var existingContext = contextObj.currentFactoryContexts.FirstOrDefault(x => x.Context.Equals(context));
				if (existingContext != null)
				{
					existingContext.UseCount--;
					if (existingContext.UseCount == 0)
					{
						contextObj.currentFactoryContexts.Remove(existingContext);
					}
				}
			}
		}

		public static void RemoveContext(BusinessObject bizObj, T context)
		{
			var contextObj = GetOrCreateBusinessContext(bizObj);

			if (contextObj.currentBusinessObjectContexts != null)
			{
				if (contextObj.currentBusinessObjectContexts.ContainsKey(bizObj.PK))
				{
					var bizObjContext = contextObj.currentBusinessObjectContexts[bizObj.PK];
					var existingContext = bizObjContext.FirstOrDefault(x => x.Context.Equals(context));
					if (existingContext != null)
					{
						existingContext.UseCount--;
						if (existingContext.UseCount == 0)
						{
							bizObjContext.Remove(existingContext);
							if (bizObjContext.Count == 0)
							{
								contextObj.currentBusinessObjectContexts.Remove(bizObj.PK);
							}
						}
					}
				}
			}
		}

		public static void CopyContext(BusinessObjectFactory source, BusinessObjectFactory target)
		{
			var sourceContext = GetBusinessContext(source);
			if (sourceContext != null)
			{
				var targetContext = GetOrCreateBusinessContext(target);

				if (targetContext.currentFactoryContexts.Any())
				{
					throw new ArgumentException("Cannot copy Business Contexts over existing ones.");
				}
				targetContext.ResetFactoryContexts();

				foreach (var contextContainer in sourceContext.currentFactoryContexts)
				{
					targetContext.currentFactoryContexts.Add(new ContextContainer(contextContainer));
				}
			}
		}

		public static void CopyContext(BusinessObject source, BusinessObject target)
		{
			var sourceContext = GetBusinessContext(source.Factory);
			if (sourceContext != null && sourceContext.currentBusinessObjectContexts.ContainsKey(source.PK))
			{
				var targetContext = GetOrCreateBusinessContext(target);

				if (targetContext.currentBusinessObjectContexts.ContainsKey(target.PK))
				{
					throw new ArgumentException("Cannot copy Business Contexts over existing ones.");
				}

				targetContext.currentBusinessObjectContexts.Add(target.PK, new List<ContextContainer>());
				var targetBizObjContext = targetContext.currentBusinessObjectContexts[target.PK];

				foreach (var sourceContextContainer in sourceContext.currentBusinessObjectContexts[source.PK])
				{
					targetBizObjContext.Add(new ContextContainer(sourceContextContainer));
				}
			}
		}

		public List<T> GetContexts(BusinessObject bizObj)
		{
			var result = new List<T>();
			if (currentBusinessObjectContexts.ContainsKey(bizObj.PK))
			{
				result.AddRange(currentBusinessObjectContexts[bizObj.PK].Select(x => x.Context));
			}
			return result;
		}

		public List<T> GetFactoryContexts()
		{
			return currentFactoryContexts.Select(x => x.Context).ToList();
		}

		static GenericBusinessContext<T> GetBusinessContext(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<GenericBusinessContext<T>>();
		}

		internal static GenericBusinessContext<T> GetOrCreateBusinessContext(BusinessObjectFactory factory)
		{
			var result = GetBusinessContext(factory);
			return result ?? new GenericBusinessContext<T>(factory);
		}

		internal static GenericBusinessContext<T> GetOrCreateBusinessContext(BusinessObject businessObject)
		{
			var result = GetBusinessContext(businessObject.Factory);
			return result ?? new GenericBusinessContext<T>(businessObject);
		}

		void ResetFactoryContexts()
		{
			currentFactoryContexts = new List<ContextContainer>();
		}

		void ResetBusinessObjectContexts()
		{
			currentBusinessObjectContexts = new Dictionary<ZGuid, List<ContextContainer>>();
		}

		List<ContextContainer> currentFactoryContexts;
		Dictionary<ZGuid, List<ContextContainer>> currentBusinessObjectContexts;

		class ContextContainer
		{
			public ContextContainer(T context)
				: this(context, 1)
			{
			}

			public ContextContainer(ContextContainer contextHolder)
				: this(contextHolder.Context, contextHolder.UseCount)
			{
			}

			ContextContainer(T context, int useCount = 1)
			{
				this.Context = context;
				this.UseCount = useCount;
			}

			public T Context { get; set; }
			public int UseCount { get; set; }
		}
	}
}
