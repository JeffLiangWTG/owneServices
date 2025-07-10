using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Workflow.Business
{
	public partial class ProcessFieldOnChangeHookFactory : IBusinessObjectOnIntializedFactory
	{
		[ThreadSafe]
		static readonly Overridable<IBusinessObjectOnInitialized> processFieldOnChangeHook = new Overridable<IBusinessObjectOnInitialized>(new ProcessFieldOnChangeHook(() => CachedParentLocators.BusinessObjectParentLocators, () => CachedParentLocators.BusinessObjectFieldChangeStates));

		public IBusinessObjectOnInitialized BusinessObjectInitializedHandler
		{
			get
			{
				var onChangeHook = processFieldOnChangeHook.Value;
				HookBusinessObjectInitializedHandlerForTest(ref onChangeHook);
				return onChangeHook;
			}
		}

		static partial void HookBusinessObjectInitializedHandlerForTest(ref IBusinessObjectOnInitialized processFieldOnChangeHook);
	}

	static partial class CachedParentLocators
	{
		[ThreadSafe]
		static readonly Lazy<List<IBusinessObjectParentLocatorFactory>> parentLocatorFactories = new Lazy<List<IBusinessObjectParentLocatorFactory>>(() =>
		{
			var result = new List<IBusinessObjectParentLocatorFactory>();
			if (ObjectFactory.Contains("BusinessObjectParentLocatorFactories"))
			{
				var parentLocatorFactories = ObjectFactory.Get<ICollection>("BusinessObjectParentLocatorFactories");
				foreach (var factory in parentLocatorFactories.Cast<IBusinessObjectParentLocatorFactory>())
				{
					result.Add(factory);
				}
			}

			return result;
		}, LazyThreadSafetyMode.PublicationOnly);

		[ThreadSafe]
		static readonly Lazy<List<IBusinessObjectFieldChangeStateFactory>> childFieldStateFactories = new Lazy<List<IBusinessObjectFieldChangeStateFactory>>(() =>
		{
			var result = new List<IBusinessObjectFieldChangeStateFactory>();
			if (ObjectFactory.Contains("BusinessObjectFieldChangeStateFactories"))
			{
				var childFieldStateFactories = ObjectFactory.Get<ICollection>("BusinessObjectFieldChangeStateFactories");
				foreach (var factory in childFieldStateFactories.Cast<IBusinessObjectFieldChangeStateFactory>())
				{
					result.Add(factory);
				}
			}

			return result;
		}, LazyThreadSafetyMode.PublicationOnly);

		public static Dictionary<string, List<IBusinessObjectParentLocator>> BusinessObjectParentLocators
		{
			get
			{
				var result = new Dictionary<string, List<IBusinessObjectParentLocator>>(); // remember case insensitive!
				foreach (var factory in parentLocatorFactories.Value)
				{
					if (!result.TryGetValue(factory.ChildTableCodePrefix, out var parentLocators))
					{
						parentLocators = new List<IBusinessObjectParentLocator>();
						result.Add(factory.ChildTableCodePrefix, parentLocators);
					}

					parentLocators.Add(factory.BusinessObjectParentLocator);
				}

				HookBusinessObjectParentLocatorGetterForUnitTests(ref result);
				return result;
			}
		}

		public static Dictionary<string, IBusinessObjectFieldChangeState> BusinessObjectFieldChangeStates
		{
			get
			{
				var result = new Dictionary<string, IBusinessObjectFieldChangeState>();
				foreach (var factory in childFieldStateFactories.Value)
				{
					result.Add(factory.ChildTableCodePrefix, factory.BusinessObjectFieldChangeState);
				}

				HookBusinessObjectFieldChangeStatesGetterForUnitTests(ref result);
				return result;
			}
		}

		static partial void HookBusinessObjectParentLocatorGetterForUnitTests(ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators);
		static partial void HookBusinessObjectFieldChangeStatesGetterForUnitTests(ref Dictionary<string, IBusinessObjectFieldChangeState> businessObjectFieldChangeStates);
	}

	#region Test
#if DEBUG

	public partial class ProcessFieldOnChangeHookFactory
	{
		static partial void HookBusinessObjectInitializedHandlerForTest(ref IBusinessObjectOnInitialized processFieldOnChangeHook)
		{
			if (ParentLocatorFactoryOverridesForTest.ModifiedParentObjectLocators != null || ParentLocatorFactoryOverridesForTest.ModifiedBusinessObjectStates != null)
			{
				processFieldOnChangeHook = new ProcessFieldOnChangeHook(() => CachedParentLocators.BusinessObjectParentLocators, () => CachedParentLocators.BusinessObjectFieldChangeStates);
			}
		}
	}

	public static partial class CachedParentLocators
	{
		static partial void HookBusinessObjectParentLocatorGetterForUnitTests(ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators)
		{
			if (ParentLocatorFactoryOverridesForTest.ModifiedParentObjectLocators != null)
			{
				ParentLocatorFactoryOverridesForTest.ModifiedParentObjectLocators.Invoke(ref businessObjectParentLocators);
			}
		}

		static partial void HookBusinessObjectFieldChangeStatesGetterForUnitTests(ref Dictionary<string, IBusinessObjectFieldChangeState> businessObjectFieldChangeStates)
		{
			if (ParentLocatorFactoryOverridesForTest.ModifiedBusinessObjectStates != null)
			{
				ParentLocatorFactoryOverridesForTest.ModifiedBusinessObjectStates.Invoke(ref businessObjectFieldChangeStates);
			}
		}
	}
#endif
	#endregion
}
