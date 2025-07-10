using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ITriggerActionTracker
	{
		void OnAllActionsRun();
	}

	public static class WorkflowTriggerActionTracker
	{
		const string CacheKey = nameof(WorkflowTriggerActionTracker);

		public static IDisposable TrackTriggerActions(BusinessObjectFactory factory)
		{
			Stack<TriggerActionTrackerManager> managerStack;
			var manager = new TriggerActionTrackerManager();
			if (TryGetManagers(factory, out managerStack))
			{
				managerStack.Push(manager);
				return new DisposableAction(() => managerStack.Pop());
			}
			else
			{
				managerStack = new Stack<TriggerActionTrackerManager>();
				managerStack.Push(manager);
				factory.GetCachedValue(CacheKey, () => managerStack, CacheStalenessPolicy.NeverStale);
				return new DisposableAction(() => factory.ClearCachedValue<Stack<TriggerActionTrackerManager>>(CacheKey));
			}
		}

		/// <summary>
		/// Get or create an instance of T.
		/// Throws InvalidOperationException when called outside the scope of TrackTriggerActions.
		/// </summary>
		public static T GetOrCreateTracker<T>(BusinessObjectFactory factory, Func<T> initialiser)
			where T : class, ITriggerActionTracker
		{
			return GetManager(factory).GetOrCreateTracker(initialiser);
		}

		public static bool TryGetTracker<T>(BusinessObjectFactory factory, out T tracker)
			where T : class, ITriggerActionTracker
		{
			if (TryGetManagers(factory, out var managers))
			{
				return managers.Peek().TryGetTracker(out tracker);
			}
			tracker = null;
			return false;
		}

		/// <summary>
		/// Call OnAllActionsRun on all subscribed ITriggerActionTracker objects
		/// Throws InvalidOperationException when called outside the scope of TrackTriggerActions.
		/// </summary>
		public static void OnAllActionsRun(BusinessObjectFactory factory)
		{
			GetManager(factory).OnAllActionsRun();
		}

		static TriggerActionTrackerManager GetManager(BusinessObjectFactory factory)
		{
			if (TryGetManagers(factory, out var managers))
			{
				return managers.Peek();
			}

			throw new InvalidOperationException($"{CacheKey} has not been initialised");
		}

		static bool TryGetManagers(BusinessObjectFactory factory, out Stack<TriggerActionTrackerManager> managers)
		{
			return factory.TryGetValueFromCacheOnly(CacheKey, out managers);
		}
	}

	class TriggerActionTrackerManager
	{
		readonly Dictionary<string, ITriggerActionTracker> trackers = new Dictionary<string, ITriggerActionTracker>();

		internal void OnAllActionsRun()
		{
			trackers.Values.ForEach(x => x.OnAllActionsRun());
		}

		internal T GetOrCreateTracker<T>(Func<T> initialiser)
			where T : class, ITriggerActionTracker
		{
			T tracker;
			if (!TryGetTracker(out tracker))
			{
				tracker = initialiser();
				trackers.Add(nameof(T), tracker);
			}
			return tracker;
		}

		internal bool TryGetTracker<T>(out T tracker)
			where T : class, ITriggerActionTracker
		{
			if (trackers.TryGetValue(nameof(T), out var result))
			{
				tracker = result as T;
				return true;
			}
			else
			{
				tracker = null;
				return false;
			}
		}
	}
}
