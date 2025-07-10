using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Workflow.Business
{
	sealed partial class ProcessFieldOnChangeHook : IBusinessObjectOnInitialized
	{
		const string FieldChangeParameterKey = "CHG";

		// Special case for QuotedBooking as .WorkflowType has side affects here
		[ThreadSafe]
		static readonly IReadOnlyDictionary<string, string> TableToWorkflowDescriptorCode = new Dictionary<string, string>()
		{
			{ ViewQuotedBookingSchema.Constants.TableName, MasterFiles.Business.WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode }
		};

		public const string EmptyValue = "[]";

		Dictionary<string, List<IBusinessObjectParentLocator>> businessObjectParentLocators;
		Dictionary<string, IBusinessObjectFieldChangeState> businessObjectFieldChangeStates;
		readonly Func<Dictionary<string, List<IBusinessObjectParentLocator>>> lazyBusinessObjectParentLocators;
		readonly Func<Dictionary<string, IBusinessObjectFieldChangeState>> lazyBusinessObjectFieldChangeStates;

		public ProcessFieldOnChangeHook(
			Func<Dictionary<string, List<IBusinessObjectParentLocator>>> lazyBusinessObjectParentLocators,
			Func<Dictionary<string, IBusinessObjectFieldChangeState>> lazyBusinessObjectFieldChangeStates)
		{
			this.lazyBusinessObjectParentLocators = lazyBusinessObjectParentLocators;
			this.lazyBusinessObjectFieldChangeStates = lazyBusinessObjectFieldChangeStates;
		}

		/// <summary>
		/// Wire up the tracking for field changes on a business object after it has been constructed 
		/// </summary>
		/// <param name="bizo">The business object to hook</param>
		public void OnBusinessObjectInitialized(BusinessObject bizo)
		{
			try
			{
				if (!ObjectFactory.Get<ISuppressHookHelper>().IsSuppressFieldOnChangeHook() && bizo.Factory.IsTableReferencedByFieldChangeRules(bizo.TablePrefix))
				{
					if (!ChildTracker.TryGetChildSharedState(bizo.Factory, bizo, out var trackingSharedState))
					{
						trackingSharedState = new TrackingState.SharedTrackingState(TrackingState.RootState.NotRoot);
						ChildTracker.AddChildSharedState(bizo.Factory, bizo, trackingSharedState);
					}

					var trackingState = new TrackingState(bizo, TrackingState.InitializationState.Initializing, trackingSharedState);
					if (BusinessObjectParentLocators.TryGetValue(bizo.TablePrefix, out var parentLocators))
					{
						if (BusinessObjectFieldChangeStates.TryGetValue(bizo.TablePrefix, out var fieldStateChange))
						{
							var stateChange = new BusinessObjectFieldChangeStateExceptionWrapper(fieldStateChange);
							HookRelationshipChanges(bizo, trackingState);

							if (stateChange.IsObjectInitialized(bizo))
							{
								trackingState.InitState = TrackingState.InitializationState.Initialized;
								HookParentFieldChanges(bizo, trackingState, parentLocators);

								if (stateChange.IsRootObject(bizo))
								{
									trackingState.SharedState.RootState = TrackingState.RootState.Root;
									HookFieldChangesToSelf(trackingState);
								}
							}
							else
							{
								trackingState.InitState = TrackingState.InitializationState.Initializing;
							}

							HookChildFieldStateChanges(bizo, trackingState, stateChange, parentLocators, Enumerable.Empty<ZPropertyValueChangedEventArgs>());
						}
						else
						{
							ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeState", $"For every table referenced with an IBusinessObjectParentLocator, there must be a corresponding IBusinessObjectFieldChangeState. For table prefix [{bizo.TablePrefix}]");
						}
					}
					else
					{
						trackingState.InitState = TrackingState.InitializationState.Initialized;
						trackingState.SharedState.RootState = TrackingState.RootState.Root;
						HookFieldChangesToSelf(trackingState);
					}
				}
			}
			catch (Exception ex) when (ex.InnerException is InvalidOperationException innerEx && innerEx.Message.Contains(openDataReaderError))
			{
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		internal const string openDataReaderError = "There is already an open DataReader associated with this Command which must be closed first.";

		Dictionary<string, List<IBusinessObjectParentLocator>> BusinessObjectParentLocators { get => businessObjectParentLocators = businessObjectParentLocators ?? lazyBusinessObjectParentLocators.Invoke(); }
		Dictionary<string, IBusinessObjectFieldChangeState> BusinessObjectFieldChangeStates { get => businessObjectFieldChangeStates = businessObjectFieldChangeStates ?? lazyBusinessObjectFieldChangeStates.Invoke(); }

		static void HookChildFieldStateChanges(
			BusinessObject bizo,
			TrackingState trackingState,
			BusinessObjectFieldChangeStateExceptionWrapper stateChange,
			List<IBusinessObjectParentLocator> parentLocators,
			IEnumerable<ZPropertyValueChangedEventArgs> missedValueChanges)
		{
			BusinessObjectFieldStateChange fieldStateChangeEventHandler = null;

			fieldStateChangeEventHandler = (newState) =>
			{
				switch (newState)
				{
					case BusinessObjectFieldStateChangeEvent.ObjectIntialized:
						if (trackingState.InitState == TrackingState.InitializationState.Initialized)
						{
							ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeState", $"Objects should only ever receive an initialization event once. Table Name: {bizo.TableName}, PK: {bizo.PK}");
							break;
						}

						trackingState.InitState = TrackingState.InitializationState.Initialized;
						HookParentFieldChanges(bizo, trackingState, parentLocators, missedValueChanges);

						if (stateChange.IsRootObject(bizo))
						{
							trackingState.SharedState.RootState = TrackingState.RootState.Root;
							HookFieldChangesToSelf(trackingState);
						}
						break;
					case BusinessObjectFieldStateChangeEvent.ObjectNowRoot:
						if (trackingState.SharedState.RootState == TrackingState.RootState.Root)
						{
#if DEBUG
							ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeState", $"Objects should only ever receive an ObjectNowRoot event once. Table Name: {bizo.TableName}, PK: {bizo.PK}");
#endif

							break;
						}

						trackingState.SharedState.RootState = TrackingState.RootState.Root;
						HookFieldChangesToSelf(trackingState);
						break;
					default:
						break;
				}
			};

			BusinessObjectFieldStateChangeNotifier.RegisterBusinessObjectFieldStateChangedNotifier(bizo, fieldStateChangeEventHandler);
			stateChange.NotifyObjectHooked(bizo);
		}

		static void HookParentFieldChanges(
			BusinessObject bizo,
			TrackingState trackingState,
			List<IBusinessObjectParentLocator> parentLocators,
			IEnumerable<ZPropertyValueChangedEventArgs> missedValueChanges = null)
		{
			foreach (var parentLocator in parentLocators)
			{
				try
				{
					if (parentLocator.TryLocateParentBusinessObjects(bizo, out var parents))
					{
						foreach (var parent in parents)
						{
							HookFieldChangesIfRulesDefined(trackingState, (IWorkflowProviderCore)parent, missedValueChanges?.ToList());
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.IBusinessObjectParentLocatorException", $"Exception while attemping to locate parent object for child. Child Table Name: {bizo.TableName}, Child PK: {bizo.PK}.", ex);
				}
			}
		}

		static void HookRelationshipChanges(
			BusinessObject bizo,
			TrackingState trackingState)
		{
			BusinessObjectRelationChange businessObjectRelationChangedEventHandler = null;

			businessObjectRelationChangedEventHandler = (stateChange, relation) =>
			{
				var parent = relation as IWorkflowProviderCore;
				if (parent == null)
				{
					ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.BusinessObjectRelationChangePassedNulL", $"Parent {stateChange} event received, but parent was null or not an IStmALogParent. Table Name: {bizo.TableName}, PK: {bizo.PK}");
					return;
				}

				switch (stateChange)
				{
					case BusinessObjectParentLocatorEvent.ParentRemoved:
						if (bizo != relation)
						{
							foreach (var aliasedChildState in trackingState.SharedState.WrappedObjects)
							{
								if (aliasedChildState.ConnectedParentHooks.TryGetValue(relation, out var handler))
								{
									aliasedChildState.Object.PropertyValueChanged -= handler;
									aliasedChildState.ConnectedParentHooks.Remove(relation);
									ChildTracker.GetChildrenOfParent(bizo.Factory, parent).Remove(aliasedChildState);
								}
								else
								{
									ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.ParentRemovedIncorrectly", $"Parent remove event received, but parent was never registered. Table Name: {bizo.TableName}, PK: {bizo.PK}");
								}
							}
						}
						else
						{
							ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.ParentRemovedIncorrectly", $"Parent remove event received, but parent was child. Table Name: {bizo.TableName}, PK: {bizo.PK}");
						}
						break;
					case BusinessObjectParentLocatorEvent.ParentAdded:
						if (!trackingState.ConnectedParentHooks.ContainsKey(relation))
						{
							foreach (var aliasedChildState in trackingState.SharedState.WrappedObjects)
							{
								HookFieldChangesIfRulesDefined(aliasedChildState, (IWorkflowProviderCore)relation);
							}
						}
						else
						{
							ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.ParentDoubleHooked", $"Objects should only ever receive an ParentAdded event once. Child Table Name: {bizo.TableName}, Child PK: {bizo.PK}. Parent Table Name: {relation.TableName}, Parent PK: {relation.PK}");
						}
						break;
					default:
						break;
				}
			};

			BusinessObjectRelationNotifier.RegisterBusinessObjectRelationChangedNotifier(bizo, businessObjectRelationChangedEventHandler);
		}

		static void HookFieldChangesToSelf(
			TrackingState trackingState,
			List<ZPropertyValueChangedEventArgs> missedChanges = null)
		{
			var existingParent = trackingState.SharedState.WrappedObjects.FirstOrDefault(state => state.SelfHook != null);

			if (existingParent != null)
			{
				HookFieldChangesIfRulesDefined(trackingState, existingParent.SelfHook, missedChanges);
			}
			else if (trackingState.Object is IWorkflowProviderCore bizo)
			{
				trackingState.SelfHook = bizo;
				foreach (var aliasedChildState in trackingState.SharedState.WrappedObjects.Where(state => state.InitState == TrackingState.InitializationState.Initialized))
				{
					HookFieldChangesIfRulesDefined(aliasedChildState, bizo, missedChanges);
				}
			}
			else
			{
				// no self hooking of non workflowproviders allowed.
			}
		}

		static void HookFieldChangesIfRulesDefined(
			TrackingState trackingState,
			IWorkflowProviderCore parent,
			List<ZPropertyValueChangedEventArgs> missedChanges = null)
		{
			var bizo = trackingState.Object;
			if (!TableToWorkflowDescriptorCode.TryGetValue(bizo.TableName, out var workflowType))
			{
				workflowType = parent.WorkflowType;
			}

			var fieldChangeRules = bizo.Factory.GetProcessFieldChangeRules(workflowType);
			var childStatesOfParent = ChildTracker.GetChildrenOfParent(bizo.Factory, parent);
			childStatesOfParent.Add(trackingState);

			if (fieldChangeRules.Count > 0)
			{
				if (missedChanges != null)
				{
					foreach (var change in missedChanges)
					{
						OnProcessFieldValueChanged(childStatesOfParent, (IStmALogParent)parent, change, fieldChangeRules);
					}
				}

				ZPropertyValueChangedEventHandler propertyValueChangedHandler = (_, args) => OnProcessFieldValueChanged(childStatesOfParent, (IStmALogParent)parent, args, fieldChangeRules);
				bizo.PropertyValueChanged += propertyValueChangedHandler;
				trackingState.ConnectedParentHooks[(BusinessObject)parent] = propertyValueChangedHandler;
			}
			else
			{
				trackingState.ConnectedParentHooks[(BusinessObject)parent] = null;
			}
		}

		static void OnProcessFieldValueChanged(
			HashSet<TrackingState> childStates,
			IStmALogParent parent,
			ZPropertyValueChangedEventArgs args,
			Dictionary<string, List<IReadOnlyProcessFieldChangeRule>> fieldChangeRules)
		{
			if (parent is BusinessObject bo && bo.IsNull)
			{
				return;
			}
			if (fieldChangeRules.TryGetValue(args.Property.Name, out var rules))
			{
				if (args.OldValue.Equals(args.Property.Value))
				{
					return;
				}

				PurgeSavedProperties(childStates);
				UpdatePropertyState(childStates, args);

				foreach (var rule in rules)
				{
					var eventLogReferenceBuilder = EventLogReferenceBuilder.New();
					var freeTextBits = new List<string>();
					var paramBits = new List<(string key, string value)>();
					eventLogReferenceBuilder.ParseReference(rule.Reference, (text) => freeTextBits.Add(text), (key, value) => paramBits.Add((key, value)));

					foreach (var freeTextBit in freeTextBits)
					{
						eventLogReferenceBuilder.AddMandatory(freeTextBit);
					}

					if (!TryExtractChangedParameters(childStates, rule.Fields, out var changedProperties))
					{
						// All parameter values are reverted, so we delete any in memory logs we have added
						parent.ClearInMemoryUpdatesForSource(rule.PK, AutoEvents.All[rule.SE_NKEvent]);
						continue;
					}

					var uniqueParamKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					foreach (var paramBit in paramBits)
					{
						if (!uniqueParamKeys.Contains(paramBit.key))
						{
							eventLogReferenceBuilder.AddMandatory(paramBit.key, paramBit.value);
							uniqueParamKeys.Add(paramBit.key);
						}
					}

					var remainingSpace = StmALogSchema.SL_Reference.MaxLength - eventLogReferenceBuilder.Build().Length;
					if (!uniqueParamKeys.Contains(FieldChangeParameterKey))
					{
						AddMultipleFieldChanges(eventLogReferenceBuilder, changedProperties, remainingSpace);
					}

					parent.CreateOrReplaceEventFromSource(rule.PK, AutoEvents.All[rule.SE_NKEvent], eventLogReferenceBuilder.Build());
				}
			}
		}

		static bool TryExtractChangedParameters(
			IEnumerable<TrackingState> children,
			IEnumerable<IReadOnlyProcessFieldChangeRuleField> fields,
			out List<TrackingState.PropertyChange> changedProperties)
		{
			changedProperties = new List<TrackingState.PropertyChange>();

			var fieldRulesIndexedByTableCode = new Dictionary<string, List<IReadOnlyProcessFieldChangeRuleField>>();
			foreach (var fieldRule in fields)
			{
				if (!fieldRulesIndexedByTableCode.TryGetValue(fieldRule.TableCode, out var fieldRuleCollection))
				{
					fieldRuleCollection = new List<IReadOnlyProcessFieldChangeRuleField>();
					fieldRulesIndexedByTableCode.Add(fieldRule.TableCode, fieldRuleCollection);
				}

				fieldRuleCollection.Add(fieldRule);
			}

			foreach (var childGroup in children.GroupBy(child => child.Object.TablePrefix))
			{
				if (!fieldRulesIndexedByTableCode.TryGetValue(childGroup.Key, out var fieldRules))
				{
					continue;
				}

				foreach (var child in childGroup)
				{
					foreach (var fieldRule in fieldRules)
					{
						// We do not want to raise events unless they have been modified directly or indirectly by the user
						if (child.SharedState.ChangedProperties.TryGetValue(fieldRule.FieldName, out var changedProperty) &&
							!changedProperty.OriginalValue.Equals(changedProperty.CurrentValue))
						{
							changedProperties.Add(changedProperty);
						}
					}
				}
			}

			return changedProperties.Count > 0;
		}

		static void PurgeSavedProperties(IEnumerable<TrackingState> trackingStates)
		{
			foreach (var trackingState in trackingStates.Where(childState => childState.Object.IsInDatabase))
			{
				var itemsToRemove = new HashSet<string>();
				var itemsToUpdate = new HashSet<TrackingState.PropertyChange>();
				foreach (var property in trackingState.SharedState.ChangedProperties)
				{
					var propertyInfo = trackingState.Object.FindPropertyInfo(property.Key);
					if (propertyInfo == null)
					{
						// Property belongs to a sibling object, so we should ignore it
						continue;
					}

					if (!propertyInfo.HasChanges && property.Value.CurrentValue.Equals(propertyInfo.OriginalValue))
					{
						itemsToRemove.Add(property.Key);
					}
					else
					{
						itemsToUpdate.Add(new TrackingState.PropertyChange(
									propertyInfo.Name,
									propertyInfo.PropertyType,
									propertyInfo.DefaultValue,
									propertyInfo.OriginalValue,
									property.Value.CurrentValue));
					}
				}

				foreach (var item in itemsToRemove)
				{
					trackingState.SharedState.ChangedProperties.Remove(item);
				}

				foreach (var property in itemsToUpdate)
				{
					trackingState.SharedState.ChangedProperties[property.Name] = property;
				}
			}
		}

		static void UpdatePropertyState(
			IEnumerable<TrackingState> childStates,
			ZPropertyValueChangedEventArgs args)
		{
			foreach (var childState in childStates)
			{
				if (childState.SharedState.ChangedProperties.TryGetValue(args.Property.Name, out var propertyChange))
				{
					childState.SharedState.ChangedProperties[args.Property.Name] =
						new TrackingState.PropertyChange(
							propertyChange.Name,
							propertyChange.PropertyType,
							propertyChange.DefaultValue,
							propertyChange.OriginalValue,
							args.Property.Value);
				}
				else
				{
					childState.SharedState.ChangedProperties[args.Property.Name] =
						new TrackingState.PropertyChange(
							args.Property.Name,
							args.Property.PropertyType,
							args.Property.DefaultValue,
							args.OldValue,
							args.Property.Value);
				}
			}
		}

		sealed class FieldChange
		{
			public FieldChange(Type propertyType, string propertyName, string oldValue, string newValue)
			{
				PropertyType = propertyType;
				PropertyName = propertyName;
				OldValue = oldValue;
				NewValue = newValue;
			}

			public Type PropertyType { get; }
			public string PropertyName { get; }
			public string OldValue { get; set; }
			public string NewValue { get; set; }
			public int Length
			{
				get
				{
					return PropertyName.Length + OldValue.Length + NewValue.Length + 7;
				}
			}
		}

		sealed class TrackingState
		{
			public enum InitializationState
			{
				Initializing,
				Initialized
			}

			public enum RootState
			{
				NotRoot,
				Root
			}

			public TrackingState(
				BusinessObject trackedObject,
				InitializationState initState,
				SharedTrackingState sharedState)
			{
				Object = trackedObject;
				InitState = initState;
				SharedState = sharedState;
				ConnectedParentHooks = new Dictionary<BusinessObject, ZPropertyValueChangedEventHandler>();
				SharedState.WrappedObjects.Add(this);
			}

			public InitializationState InitState { get; set; }
			public BusinessObject Object { get; }
			public SharedTrackingState SharedState { get; }
			public Dictionary<BusinessObject, ZPropertyValueChangedEventHandler> ConnectedParentHooks { get; }
			public IWorkflowProviderCore SelfHook { get; set; }

			public sealed class SharedTrackingState
			{
				public SharedTrackingState(RootState rootState)
				{
					RootState = rootState;
					WrappedObjects = new HashSet<TrackingState>();
					ChangedProperties = new Dictionary<string, PropertyChange>();
				}

				public RootState RootState { get; set; }
				public HashSet<TrackingState> WrappedObjects { get; }
				public Dictionary<string, PropertyChange> ChangedProperties { get; }
			}

			public class PropertyChange
			{
				public PropertyChange(
					string name,
					Type propertyType,
					IZType defaultValue,
					IZType originalValue,
					IZType currentValue)
				{
					Name = name;
					PropertyType = propertyType;
					DefaultValue = defaultValue;
					OriginalValue = originalValue;
					CurrentValue = currentValue;
				}

				public string Name { get; }
				public Type PropertyType { get; }
				public IZType DefaultValue { get; }
				public IZType OriginalValue { get; }
				public IZType CurrentValue { get; }
			}
		}

		sealed class ChildTracker : IService
		{
			Dictionary<IWorkflowProviderCore, HashSet<TrackingState>> ChildrenOfParents { get; } = new Dictionary<IWorkflowProviderCore, HashSet<TrackingState>>();
			Dictionary<ZGuid, TrackingState.SharedTrackingState> SharedChildStates { get; } = new Dictionary<ZGuid, TrackingState.SharedTrackingState>();

			public static HashSet<TrackingState> GetChildrenOfParent(BusinessObjectFactory factory, IWorkflowProviderCore parent)
			{
				var childTracker = GetService(factory);
				if (!childTracker.ChildrenOfParents.TryGetValue(parent, out var result))
				{
					result = new HashSet<TrackingState>();
					childTracker.ChildrenOfParents.Add(parent, result);
				}

				return result;
			}

			public static bool TryGetChildSharedState(BusinessObjectFactory factory, BusinessObject child, out TrackingState.SharedTrackingState sharedChildState)
			{
				sharedChildState = null;
				var childTracker = GetService(factory);
				return childTracker.SharedChildStates.TryGetValue(child.PK, out sharedChildState);
			}

			public static void AddChildSharedState(BusinessObjectFactory factory, BusinessObject child, TrackingState.SharedTrackingState sharedChildState)
			{
				GetService(factory).SharedChildStates.Add(child.PK, sharedChildState);
			}

			static ChildTracker GetService(BusinessObjectFactory factory)
			{
				var childTracker = factory.ServiceContainer.GetService<ChildTracker>();
				childTracker ??= factory.ServiceContainer.AddService(new ChildTracker());

				return childTracker;
			}
		}

		sealed class BusinessObjectFieldChangeStateExceptionWrapper : IBusinessObjectFieldChangeState
		{
			readonly IBusinessObjectFieldChangeState businessObjectFieldChangeState;

			public BusinessObjectFieldChangeStateExceptionWrapper(IBusinessObjectFieldChangeState businessObjectFieldChangeState)
			{
				this.businessObjectFieldChangeState = businessObjectFieldChangeState;
			}

			public bool IsObjectInitialized(BusinessObject bizo)
			{
				try
				{
					return businessObjectFieldChangeState.IsObjectInitialized(bizo);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeStateException", $"Exception while attemping to call IsObjectInitialized. Child Table Name: {bizo.TableName}, Child PK: {bizo.PK}.", ex);
					return true;
				}
			}

			public bool IsRootObject(BusinessObject bizo)
			{
				try
				{
					return businessObjectFieldChangeState.IsRootObject(bizo);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeStateException", $"Exception while attemping to call IsRootObject. Child Table Name: {bizo.TableName}, Child PK: {bizo.PK}.", ex);
					return true;
				}
			}

			public void NotifyObjectHooked(BusinessObject bizo)
			{
				try
				{
					businessObjectFieldChangeState.NotifyObjectHooked(bizo);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("ProcessFieldOnChangeHook.IBusinessObjectFieldChangeStateException", $"Exception while attemping to call NotifyObjectHooked. Child Table Name: {bizo.TableName}, Child PK: {bizo.PK}.", ex);
				}
			}
		}
	}
}
