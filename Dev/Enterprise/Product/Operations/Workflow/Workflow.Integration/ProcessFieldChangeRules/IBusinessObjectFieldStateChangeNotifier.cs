using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public interface IBusinessObjectFieldStateChangeNotifier
	{
		void FieldChangeTrackerStateChanged(BusinessObject child, BusinessObjectFieldStateChangeEvent stateChange);
	}

	public static class BusinessObjectFieldStateChangeExtenstion
	{
		public static void FieldChangeTrackerStateChanged(this BusinessObject child, BusinessObjectFieldStateChangeEvent stateChange)
		{
			ObjectFactory.Get<IBusinessObjectFieldStateChangeNotifier>().FieldChangeTrackerStateChanged(child, stateChange);
		}
	}

	#region Test
#if DEBUG

	public delegate void ModifyParentObjectLocators(ref Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators);
	public delegate void ModifyBusinessObjectState(ref Dictionary<string, IBusinessObjectFieldChangeState> businessObjectFieldChangeStates);

	public static class ParentLocatorFactoryOverridesForTest
	{
		static readonly Overridable<ModifyParentObjectLocators> onGetParentLocators = new Overridable<ModifyParentObjectLocators>(null);
		static readonly Overridable<ModifyBusinessObjectState> onGetBusinessObjectStates = new Overridable<ModifyBusinessObjectState>(null);

		public static ModifyParentObjectLocators ModifiedParentObjectLocators
		{
			get
			{
				return onGetParentLocators.Value;
			}
			set
			{
				onGetParentLocators.Value = value;
			}
		}

		public static ModifyBusinessObjectState ModifiedBusinessObjectStates
		{
			get
			{
				return onGetBusinessObjectStates.Value;
			}
			set
			{
				onGetBusinessObjectStates.Value = value;
			}
		}

		public static void SetOnGetParentLocatorsHookForTest(ModifyParentObjectLocators onGetParentLocators)
		{
			ParentLocatorFactoryOverridesForTest.onGetParentLocators.Value = onGetParentLocators;
		}

		public static void SetOnGetBusinessObjectStatesHookForTest(ModifyBusinessObjectState onGetBusinessObjectStates)
		{
			ParentLocatorFactoryOverridesForTest.onGetBusinessObjectStates.Value = onGetBusinessObjectStates;
		}
	}

#endif
	#endregion
}
