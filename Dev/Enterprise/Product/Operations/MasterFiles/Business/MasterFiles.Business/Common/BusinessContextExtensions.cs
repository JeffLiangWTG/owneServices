using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public static class BusinessContextExtensions
	{
		#region BusinessObjectFactory Extensions

		public static bool HasContext<T>(this BusinessObjectFactory factory, params T[] contexts) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.HasContext(factory, contexts);
		}

		public static bool HasAnyOfContexts<T>(this BusinessObjectFactory factory, params T[] contexts) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.HasAnyOfContexts(factory, contexts);
		}

		public static void SetContext<T>(this BusinessObjectFactory factory, params T[] contexts) where T : struct, IComparable, IConvertible
		{
			GenericBusinessContext<T>.SetContext(factory, contexts);
		}

		public static void RemoveContext<T>(this BusinessObjectFactory factory, params T[] contexts) where T : struct, IComparable, IConvertible
		{
			GenericBusinessContext<T>.RemoveContext(factory, contexts);
		}

		public static List<T> GetContexts<T>(this BusinessObjectFactory factory) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.GetOrCreateBusinessContext(factory).GetFactoryContexts();
		}

		public static IDisposable SetTempContext<T>(this BusinessObjectFactory factory, params T[] contexts) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.GetOrCreateBusinessContext(factory).SetTempContext(factory, contexts);
		}

		#endregion

		#region BusinessObject Extensions

		public static IDisposable SetTempContext<T>(this BusinessObject bizObj, T context) where T : struct, IComparable, IConvertible => new DisposableAction(() => GenericBusinessContext<T>.SetContext(bizObj, context), () => GenericBusinessContext<T>.RemoveContext(bizObj, context));

		public static void SetContext<T>(this BusinessObject bizObj, T context) where T : struct, IComparable, IConvertible
		{
			GenericBusinessContext<T>.SetContext(bizObj, context);
		}

		public static bool HasContext<T>(this BusinessObject bizObj, T context) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.HasContext(bizObj, context);
		}

		public static bool HasContextWithoutFactoryFallback<T>(this BusinessObject bizObj, T context) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.HasContextWithoutFactoryFallback(bizObj, context);
		}

		public static bool HasAnyOfContexts<T>(this BusinessObject bizObj, params T[] contexts) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.GetOrCreateBusinessContext(bizObj).HasAnyOfContexts(bizObj, contexts);
		}

		public static void RemoveContext<T>(this BusinessObject bizObj, T context) where T : struct, IComparable, IConvertible
		{
			GenericBusinessContext<T>.RemoveContext(bizObj, context);
		}

		public static List<T> GetContexts<T>(this BusinessObject bizObj) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.GetOrCreateBusinessContext(bizObj).GetContexts(bizObj);
		}

		#endregion
	}
}
